using Godot;

public partial class PlayerWeapon : Node
{
	[Export] public string AttackAction = InputActions.AttackPrimary;
	[Export] public bool EnabledInCurrentCharacter = true;

	[Export] public PackedScene ProjectileScene;
	[Export] public NodePath ProjectileContainerPath;

	[Export] public float Cooldown = 0.12f;
	[Export] public float ProjectileSpeed = 900f;
	[Export] public int Damage = 1;
	[Export] public PrimaryFirePattern FirePattern = PrimaryFirePattern.Single;
	[Export] public float BurstShotInterval = 0.08f;

	private Player _player;
	private Node _projectileContainer;
	private StabilitySystem _stabilitySystem;
	private float _cooldownTimer = 0f;
	private float _burstTimer = 0f;
	private int _burstShotsRemaining = 0;
	private Vector2 _burstDir = Vector2.Right;
	private float _burstSpeed = 0f;
	private int _burstDamage = 1;
	private string _resolvedAction = InputActions.AttackPrimary;
	private bool _isEnabled = true;

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentProjectileSpeed => ProjectileSpeed;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();

		if (ProjectileContainerPath != null && !ProjectileContainerPath.IsEmpty)
			_projectileContainer = GetNode(ProjectileContainerPath);

		_isEnabled = EnabledInCurrentCharacter;
		ResolveInputAction();
	}

	public void Tick(float dt)
	{
		if (!_isEnabled)
			return;

		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();

		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		ProcessBurst(dt);
		if (_cooldownTimer > 0f || _burstShotsRemaining > 0)
			return;

		if (!Input.IsActionPressed(_resolvedAction))
			return;

		ExecuteAttack();
	}

	private void ExecuteAttack()
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		AudioManager.Instance?.PlaySfxPlayerFire();

		Vector2 mouseWorld = _player.GetGlobalMousePosition();
		Vector2 dir = mouseWorld - _player.GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			dir = Vector2.Right;
		else
			dir = dir.Normalized();

		Node bullet = ProjectileScene.Instantiate();
		if (bullet is Node2D bullet2D)
			bullet2D.GlobalPosition = _player.GlobalPosition;

		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		float speed = ProjectileSpeed * (1f + ((powerMult - 1f) * 0.35f));
		int damage = Mathf.Max(1, Mathf.RoundToInt(Damage * powerMult));
		if (FirePattern == PrimaryFirePattern.Burst3)
		{
			_burstDir = dir;
			_burstSpeed = speed;
			_burstDamage = damage;
			_burstShotsRemaining = 2;
			_burstTimer = Mathf.Max(0.01f, BurstShotInterval);
			SpawnProjectile(dir, speed, damage, bullet);
			_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
			return;
		}

		bullet.Call("InitFromPlayer", _player, dir, speed, damage);
		_projectileContainer.AddChild(bullet);
		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void SpawnProjectile(Vector2 dir, float speed, int damage, Node preInstanced = null)
	{
		Node bullet = preInstanced ?? ProjectileScene.Instantiate();
		if (bullet is Node2D bullet2D)
			bullet2D.GlobalPosition = _player.GlobalPosition;
		bullet.Call("InitFromPlayer", _player, dir.Normalized(), speed, damage);
		_projectileContainer.AddChild(bullet);
	}

	private void ResolveStabilitySystem()
	{
		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}

	private void ResolveInputAction()
	{
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

	public void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		EnabledInCurrentCharacter = enabled;
		if (!enabled)
		{
			_burstShotsRemaining = 0;
			_burstTimer = 0f;
		}
	}

	public void SetAttackAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
			return;
		AttackAction = action;
		ResolveInputAction();
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

	public void SetBaseStats(int damage, float cooldown, float projectileSpeed)
	{
		Damage = Mathf.Max(1, damage);
		Cooldown = Mathf.Clamp(cooldown, 0.02f, 10f);
		ProjectileSpeed = Mathf.Max(50f, projectileSpeed);
	}

	public void SetFirePattern(PrimaryFirePattern pattern, float burstShotInterval)
	{
		FirePattern = pattern;
		BurstShotInterval = Mathf.Clamp(burstShotInterval, 0.01f, 0.5f);
	}

	private void ProcessBurst(float dt)
	{
		if (_burstShotsRemaining <= 0)
			return;
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
		{
			_burstShotsRemaining = 0;
			return;
		}

		_burstTimer -= dt;
		if (_burstTimer > 0f)
			return;

		SpawnProjectile(_burstDir, _burstSpeed, _burstDamage);
		_burstShotsRemaining--;
		if (_burstShotsRemaining > 0)
			_burstTimer = Mathf.Max(0.01f, BurstShotInterval);
	}
}
