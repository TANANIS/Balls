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
		if (_sprite != null)
		{
			_baseSpriteScale = _sprite.Scale;
			_baseSpriteModulate = _sprite.Modulate;
		}
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

		if (_sprite == null)
			return;

		SpawnWhiteHitFlash();

		_feedbackTween?.Kill();
		Color baseColor = _baseSpriteModulate;
		Vector2 baseScale = _baseSpriteScale;
		_sprite.Modulate = new Color(1f, 1f, 1f, 1f);
		_sprite.Scale = baseScale * Mathf.Max(1f, HitPunchScale);

		_feedbackTween = CreateTween();
		_feedbackTween.TweenProperty(_sprite, "modulate", baseColor, Mathf.Max(0.03f, HitFlashDuration));
		_feedbackTween.Parallel().TweenProperty(_sprite, "scale", baseScale, Mathf.Max(0.04f, HitFlashDuration + 0.03f));
	}

	private void SpawnWhiteHitFlash()
	{
		if (_ownerEnemy == null || _sprite == null || _sprite.Texture == null)
			return;

		var flashSprite = new Sprite2D
		{
			Texture = _sprite.Texture,
			Centered = _sprite.Centered,
			Offset = _sprite.Offset,
			FlipH = _sprite.FlipH,
			FlipV = _sprite.FlipV,
			Hframes = _sprite.Hframes,
			Vframes = _sprite.Vframes,
			Frame = _sprite.Frame,
			FrameCoords = _sprite.FrameCoords,
			RegionEnabled = _sprite.RegionEnabled,
			RegionRect = _sprite.RegionRect,
			RegionFilterClipEnabled = _sprite.RegionFilterClipEnabled,
			Position = _sprite.Position,
			Rotation = _sprite.Rotation,
			Scale = _sprite.Scale * Mathf.Max(1f, HitFlashOverlayScale),
			ZIndex = _sprite.ZIndex + 1,
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
			_sprite.Scale * Mathf.Max(1.05f, HitFlashOverlayScale + 0.12f),
			duration);
		tween.Finished += flashSprite.QueueFree;
	}

	public void SetMaxHpAndRefill(int maxHp)
	{
		MaxHp = Mathf.Max(1, maxHp);
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
	}
}
