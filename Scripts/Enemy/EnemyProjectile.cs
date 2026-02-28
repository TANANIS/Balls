using Godot;

public partial class EnemyProjectile : Area2D
{
	[Export] public float LifeTime = 1.6f;
	[Export] public bool DespawnOutsideViewport = true;
	[Export(PropertyHint.Range, "0,1024,1")] public float DespawnOutsideViewportMargin = 32f;
	[Export] public string DamageTag = "enemy_projectile";
	[Export] public bool RotateToDirection = true;
	[Export(PropertyHint.Range, "-180,180,1")] public float RotationOffsetDegrees = 0f;
	[Export(PropertyHint.Range, "0.00,0.50,0.01")] public float HitArmDelaySeconds = 0.06f;

	private Vector2 _dir = Vector2.Right;
	private float _speed = 520f;
	private int _damage = 1;
	private Node _source;
	private CombatSystem _combat;
	private float _lifeTimer = 0f;
	private float _hitArmTimer = 0f;
	private bool _hasHit = false;

	public void Init(Node source, Vector2 direction, float speed, int damage, float lifeTimeSeconds)
	{
		_source = source;
		_dir = direction == Vector2.Zero ? Vector2.Right : direction.Normalized();
		_speed = Mathf.Max(10f, speed);
		_damage = Mathf.Max(1, damage);
		LifeTime = Mathf.Max(0.05f, lifeTimeSeconds);
		_hitArmTimer = Mathf.Max(0f, HitArmDelaySeconds);
		ApplyFacingByDirection();
	}

	public override void _Ready()
	{
		TryResolveCombatSystem();
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_hasHit)
			return;

		float dt = (float)delta;
		_lifeTimer += dt;
		if (_hitArmTimer > 0f)
			_hitArmTimer -= dt;
		if (_lifeTimer >= LifeTime)
		{
			QueueFree();
			return;
		}

		GlobalPosition += _dir * _speed * dt;
		if (DespawnOutsideViewport && IsOutsideActiveCameraViewport())
		{
			QueueFree();
			return;
		}
	}

	private void OnAreaEntered(Area2D other)
	{
		TryHit(other);
	}

	private void OnBodyEntered(Node2D other)
	{
		TryHit(other);
	}

	private void TryHit(Node other)
	{
		if (_hasHit || other == null)
			return;
		if (_hitArmTimer > 0f)
			return;
		if (_source != null && (other == _source || _source.IsAncestorOf(other) || other.IsAncestorOf(_source)))
			return;
		if (other.IsInGroup(RuntimeGroups.EnemyHitbox) || other.IsInGroup(RuntimeGroups.EnemyHurtbox))
			return;

		if (other.IsInGroup(RuntimeGroups.World))
		{
			Consume();
			return;
		}

		if (!other.IsInGroup(RuntimeGroups.PlayerHurtbox))
			return;

		if (_combat == null)
			TryResolveCombatSystem();
		if (_combat == null || _source == null)
			return;

		var req = new DamageRequest(
			source: _source,
			target: other,
			baseDamage: _damage,
			worldPos: GlobalPosition,
			tag: DamageTag);

		_combat.RequestDamage(req);
		Consume();
	}

	private void Consume()
	{
		if (_hasHit)
			return;
		_hasHit = true;
		SetDeferred("monitoring", false);
		QueueFree();
	}

	private void TryResolveCombatSystem()
	{
		if (_combat != null)
			return;

		var list = GetTree().GetNodesInGroup(RuntimeGroups.CombatSystem);
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;
	}

	private void ApplyFacingByDirection()
	{
		if (!RotateToDirection)
			return;

		float baseAngle = _dir.Angle();
		Rotation = baseAngle + Mathf.DegToRad(RotationOffsetDegrees);
	}

	private bool IsOutsideActiveCameraViewport()
	{
		Viewport viewport = GetViewport();
		if (viewport == null)
			return false;

		Camera2D camera = viewport.GetCamera2D();
		if (camera == null)
			return false;

		Vector2 screenSize = viewport.GetVisibleRect().Size;
		Vector2 worldSize = new Vector2(
			screenSize.X * Mathf.Abs(camera.Zoom.X),
			screenSize.Y * Mathf.Abs(camera.Zoom.Y));
		Vector2 half = worldSize * 0.5f;
		Rect2 worldRect = new Rect2(camera.GlobalPosition - half, worldSize)
			.Grow(Mathf.Max(0f, DespawnOutsideViewportMargin));
		return !worldRect.HasPoint(GlobalPosition);
	}
}
