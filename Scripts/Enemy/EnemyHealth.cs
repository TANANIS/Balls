using Godot;

public partial class EnemyHealth : Node
{
	[Export] public int MaxHp = 3;
	[Export] public float HurtIFrame = 0.05f;
	[Export] public float HurtKnockbackStrength = 74f;
	[Export] public float HurtKnockbackDuration = 0.08f;
	[Export] public float HitFlashDuration = 0.08f;
	[Export] public float HitPunchScale = 1.08f;
	[Export] public float HitFlashOverlayScale = 1.22f;
	[Export] public float HitFlashOverlayAlpha = 0.88f;

	private int _hp;
	private bool _isDead;
	private float _invincibleTimer;
	private Enemy _ownerEnemy;
	private Sprite2D _sprite;
	private AnimatedSprite2D _animatedSprite;
	private Node2D _visualNode;
	private CanvasItem _visualCanvas;
	private Tween _feedbackTween;
	private Vector2 _baseSpriteScale = Vector2.One;
	private Color _baseSpriteModulate = Colors.White;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	public override void _Ready()
	{
		_hp = MaxHp;
		_ownerEnemy = GetParent() as Enemy;
		_sprite = _ownerEnemy?.GetNodeOrNull<Sprite2D>("Sprite2D");
		_animatedSprite = _ownerEnemy?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? _ownerEnemy?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		_visualNode = (Node2D)_sprite ?? _animatedSprite;
		_visualCanvas = (CanvasItem)_sprite ?? _animatedSprite;
		if (_visualNode != null)
			_baseSpriteScale = _visualNode.Scale;
		if (_visualCanvas != null)
			_baseSpriteModulate = _visualCanvas.Modulate;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_invincibleTimer > 0f)
			_invincibleTimer -= (float)delta;
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f)
			return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void TakeDamage(int amount, object source)
	{
		if (_isDead || IsInvincible || amount <= 0)
			return;

		_hp -= amount;
		_ownerEnemy?.NotifyDamaged(amount, source);
		ApplyHitFeedback(source);

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp > 0)
			return;

		_isDead = true;
		_ownerEnemy?.NotifyDeath(source);
	}

	private void ApplyHitFeedback(object source)
	{
		if (_ownerEnemy != null && HurtKnockbackStrength > 0f)
		{
			Vector2 dir = Vector2.Right;
			if (source is Node2D srcNode)
			{
				dir = _ownerEnemy.GlobalPosition - srcNode.GlobalPosition;
				if (dir.LengthSquared() < 0.0001f)
					dir = Vector2.Right;
				else
					dir = dir.Normalized();
			}

			_ownerEnemy.ApplySeparation(dir, HurtKnockbackStrength, HurtKnockbackDuration);
		}

		if (_visualNode == null || _visualCanvas == null)
			return;

		SpawnWhiteHitFlash();

		_feedbackTween?.Kill();
		Color baseColor = _baseSpriteModulate;
		Vector2 baseScale = _baseSpriteScale;
		_visualCanvas.Modulate = new Color(1f, 1f, 1f, 1f);
		_visualNode.Scale = baseScale * Mathf.Max(1f, HitPunchScale);

		_feedbackTween = CreateTween();
		_feedbackTween.TweenProperty(_visualCanvas, "modulate", baseColor, Mathf.Max(0.03f, HitFlashDuration));
		_feedbackTween.Parallel().TweenProperty(_visualNode, "scale", baseScale, Mathf.Max(0.04f, HitFlashDuration + 0.03f));
	}

	private void SpawnWhiteHitFlash()
	{
		if (_ownerEnemy == null || _visualNode == null)
			return;

		Texture2D texture = ResolveCurrentVisualTexture();
		if (texture == null)
			return;

		var flashSprite = new Sprite2D
		{
			Texture = texture,
			Centered = true,
			Position = _visualNode.Position,
			Rotation = _visualNode.Rotation,
			Scale = _visualNode.Scale * Mathf.Max(1f, HitFlashOverlayScale),
			ZIndex = _visualNode.ZIndex + 1,
			Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(HitFlashOverlayAlpha, 0f, 1f))
		};

		flashSprite.Material = new CanvasItemMaterial
		{
			BlendMode = CanvasItemMaterial.BlendModeEnum.Add
		};

		_ownerEnemy.AddChild(flashSprite);

		float duration = Mathf.Max(0.03f, HitFlashDuration);
		Tween tween = CreateTween();
		tween.TweenProperty(flashSprite, "modulate:a", 0f, duration);
		tween.Parallel().TweenProperty(
			flashSprite,
			"scale",
			_visualNode.Scale * Mathf.Max(1.05f, HitFlashOverlayScale + 0.12f),
			duration);
		tween.Finished += flashSprite.QueueFree;
	}

	private Texture2D ResolveCurrentVisualTexture()
	{
		if (_sprite != null)
			return _sprite.Texture;

		if (_animatedSprite == null || _animatedSprite.SpriteFrames == null)
			return null;

		StringName animation = _animatedSprite.Animation;
		if (animation.IsEmpty)
			return null;

		int frameCount = _animatedSprite.SpriteFrames.GetFrameCount(animation);
		if (frameCount <= 0)
			return null;

		int frame = Mathf.Clamp(_animatedSprite.Frame, 0, frameCount - 1);
		return _animatedSprite.SpriteFrames.GetFrameTexture(animation, frame);
	}

	public void SetMaxHpAndRefill(int maxHp)
	{
		MaxHp = Mathf.Max(1, maxHp);
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
	}
}
