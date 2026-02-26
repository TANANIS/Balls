using Godot;

public partial class EnemyRangedAttackBehavior : EnemyBehaviorModule
{
	[Export] public PackedScene ProjectileScene;
	[Export] public NodePath ProjectileContainerPath = new("../../../Projectiles");
	[Export] public float AttackRange = 520f;
	[Export] public float MinRange = 180f;
	[Export] public float AttackCooldown = 1.8f;
	[Export] public float ProjectileSpeed = 540f;
	[Export] public int ProjectileDamage = 1;
	[Export] public float ProjectileLifeTime = 2.2f;
	[Export] public float AimLeadSeconds = 0.18f;
	[Export] public float ProjectileSpawnForwardOffset = 22f;
	[Export] public float ChaseSpeedMultiplier = 0.72f;
	[Export] public float RetreatSpeedMultiplier = 0.64f;
	[Export] public bool UseAttackAnimation = false;
	[Export] public StringName AttackAnimation = "attack";
	[Export] public StringName MoveAnimation = "walk";
	[Export] public float AttackAnimLockSeconds = 0.22f;

	private float _cooldownTimer = 0f;
	private float _attackAnimLockTimer = 0f;
	private AnimatedSprite2D _animatedSprite;
	private Node2D _projectileRoot;

	public override void OnInitialized(Enemy enemy)
	{
		_cooldownTimer = (float)GD.RandRange(0f, Mathf.Max(0.01f, AttackCooldown * 0.55f));
		_attackAnimLockTimer = 0f;
		_animatedSprite = enemy?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? enemy?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		ResolveProjectileRoot(enemy);
	}

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		float dt = (float)delta;
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;
		if (_attackAnimLockTimer > 0f)
			_attackAnimLockTimer -= dt;

		if (!IsInstanceValid(_projectileRoot))
			ResolveProjectileRoot(enemy);

		Vector2 targetPos = player.GlobalPosition;
		if (player is CharacterBody2D movingPlayer)
			targetPos += movingPlayer.Velocity * Mathf.Max(0f, AimLeadSeconds);

		Vector2 toTarget = targetPos - enemy.GlobalPosition;
		float distance = toTarget.Length();
		if (distance < 0.0001f)
			return Vector2.Zero;

		Vector2 forward = toTarget / distance;

		if (_cooldownTimer <= 0f && distance <= AttackRange && distance >= MinRange)
		{
			TryFire(enemy, forward);
			_cooldownTimer = Mathf.Max(0.05f, AttackCooldown);
			_attackAnimLockTimer = Mathf.Max(0f, AttackAnimLockSeconds);
			PlayAttack();
		}

		if (_attackAnimLockTimer > 0f)
			return Vector2.Zero;

		PlayMoveIfNeeded();

		if (distance > AttackRange)
			return forward * enemy.MaxSpeed * Mathf.Max(0f, ChaseSpeedMultiplier);

		if (distance < MinRange)
			return -forward * enemy.MaxSpeed * Mathf.Max(0f, RetreatSpeedMultiplier);

		return Vector2.Zero;
	}

	private void TryFire(Enemy enemy, Vector2 direction)
	{
		if (ProjectileScene == null)
			return;

		if (ProjectileScene.Instantiate() is not EnemyProjectile projectile)
			return;

		projectile.GlobalPosition = enemy.GlobalPosition + (direction * Mathf.Max(0f, ProjectileSpawnForwardOffset));
		projectile.Init(
			source: enemy,
			direction: direction,
			speed: ProjectileSpeed,
			damage: Mathf.Max(1, ProjectileDamage),
			lifeTimeSeconds: ProjectileLifeTime);

		Node parent = _projectileRoot ?? enemy.GetParent();
		parent?.AddChild(projectile);
	}

	private void ResolveProjectileRoot(Enemy enemy)
	{
		_projectileRoot = null;
		if (enemy == null)
			return;

		_projectileRoot = enemy.GetNodeOrNull<Node2D>(ProjectileContainerPath);
		if (_projectileRoot != null)
			return;

		Node root = enemy.GetTree()?.Root;
		_projectileRoot = root?.FindChild("Projectiles", recursive: true, owned: false) as Node2D;
	}

	private void PlayAttack()
	{
		if (!UseAttackAnimation || _animatedSprite?.SpriteFrames == null)
			return;
		if (AttackAnimation.IsEmpty || !_animatedSprite.SpriteFrames.HasAnimation(AttackAnimation))
			return;
		_animatedSprite.Play(AttackAnimation);
	}

	private void PlayMoveIfNeeded()
	{
		if (_animatedSprite?.SpriteFrames == null)
			return;
		if (MoveAnimation.IsEmpty || !_animatedSprite.SpriteFrames.HasAnimation(MoveAnimation))
			return;
		if (_animatedSprite.Animation == MoveAnimation && _animatedSprite.IsPlaying())
			return;
		_animatedSprite.Play(MoveAnimation);
	}
}
