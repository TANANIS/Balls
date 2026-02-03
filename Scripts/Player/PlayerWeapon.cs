using Godot;

public partial class PlayerWeapon : Node
{
	[Export] public string AttackAction = InputActions.AttackPrimary;

	[Export] public PackedScene ProjectileScene;
	[Export] public NodePath ProjectileContainerPath;

	[Export] public float Cooldown = 0.12f;
	[Export] public float ProjectileSpeed = 900f;
	[Export] public int Damage = 1;

	private Player _player;
	private Node _projectileContainer;
	private float _cooldownTimer = 0f;
	private string _resolvedAction = InputActions.AttackPrimary;

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentProjectileSpeed => ProjectileSpeed;

	public void Setup(Player player)
	{
		_player = player;

		if (ProjectileContainerPath != null && !ProjectileContainerPath.IsEmpty)
			_projectileContainer = GetNode(ProjectileContainerPath);

		if (InputMap.HasAction(AttackAction))
		{
			_resolvedAction = AttackAction;
		}
		else if (InputMap.HasAction(InputActions.LegacyAttackPrimary))
		{
			_resolvedAction = InputActions.LegacyAttackPrimary;
			DebugSystem.Warn("[PlayerWeapon] attack_primary not found. Fallback to legacy action 'fire'.");
		}
		else
		{
			DebugSystem.Error("[PlayerWeapon] No valid primary attack action found.");
		}
	}

	public void Tick(float dt)
	{
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		if (_cooldownTimer > 0f)
			return;

		if (!Input.IsActionPressed(_resolvedAction))
			return;

		ExecuteAttack();
		_cooldownTimer = Cooldown;
	}

	private void ExecuteAttack()
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		Vector2 mouseWorld = _player.GetGlobalMousePosition();
		Vector2 dir = mouseWorld - _player.GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			dir = Vector2.Right;
		else
			dir = dir.Normalized();

		Node bullet = ProjectileScene.Instantiate();
		if (bullet is Node2D bullet2D)
			bullet2D.GlobalPosition = _player.GlobalPosition;

		bullet.Call("InitFromPlayer", _player, dir, ProjectileSpeed, Damage);
		_projectileContainer.AddChild(bullet);
	}

	public void AddDamage(int amount)
	{
		Damage = Mathf.Max(1, Damage + amount);
	}

	public void AddProjectileSpeed(float amount)
	{
		ProjectileSpeed = Mathf.Max(50f, ProjectileSpeed + amount);
	}

	public void MultiplyCooldown(float factor)
	{
		Cooldown = Mathf.Clamp(Cooldown * factor, 0.02f, 10f);
	}
}
