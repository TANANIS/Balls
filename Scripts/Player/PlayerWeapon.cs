using Godot;
using System.Collections.Generic;

public partial class PlayerWeapon : Node
{
	[Export] public string AttackAction = InputActions.AttackPrimary;
	[Export] public bool EnabledInCurrentCharacter = true;

	[Export] public PackedScene ProjectileScene;
	[Export] public NodePath ProjectileContainerPath;

	[Export] public float Cooldown = 0.12f;
	[Export] public float ProjectileSpeed = 900f;
	[Export] public int Damage = 1;
	[Export] public float CritChance = 0f;
	[Export] public float CritDamageMultiplier = 1.5f;
	[Export] public int ExtraProjectiles = 0;
	[Export] public int SplitShotLevel = 0;
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
	private int _burstBaseDamage = 1;
	private string _resolvedAction = InputActions.AttackPrimary;
	private bool _isEnabled = true;
	private readonly RandomNumberGenerator _rng = new();

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentProjectileSpeed => ProjectileSpeed;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();
		_rng.Randomize();

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

		Vector2 mouseWorld = _player.GetGlobalMousePosition();
		Vector2 dir = mouseWorld - _player.GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			dir = Vector2.Right;
		else
			dir = dir.Normalized();

		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		float speed = ProjectileSpeed * (1f + ((powerMult - 1f) * 0.35f));
		int baseDamage = Mathf.Max(1, Mathf.RoundToInt(Damage * powerMult));
		if (FirePattern == PrimaryFirePattern.Burst3)
		{
			_burstDir = dir;
			_burstSpeed = speed;
			_burstBaseDamage = baseDamage;
			_burstShotsRemaining = 2;
			_burstTimer = Mathf.Max(0.01f, BurstShotInterval);
			FireVolley(dir, speed, baseDamage);
			_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
			return;
		}

		FireVolley(dir, speed, baseDamage);
		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void FireVolley(Vector2 baseDir, float speed, int baseDamage)
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		AudioManager.Instance?.PlaySfxPlayerFire();

		foreach (float angleDeg in BuildVolleyAngles())
		{
			Vector2 dir = baseDir.Rotated(Mathf.DegToRad(angleDeg)).Normalized();
			int damage = RollDamage(baseDamage);
			SpawnProjectile(dir, speed, damage);
		}
	}

	private List<float> BuildVolleyAngles()
	{
		var angles = new List<float> { 0f };
		int count = Mathf.Max(1, 1 + ExtraProjectiles);
		angles.Clear();
		if (count == 1)
		{
			angles.Add(0f);
		}
		else
		{
			float spacing = 7f;
			float start = -spacing * (count - 1) * 0.5f;
			for (int i = 0; i < count; i++)
				angles.Add(start + (spacing * i));
		}

		for (int level = 1; level <= Mathf.Clamp(SplitShotLevel, 0, 2); level++)
		{
			float offset = 12f + ((level - 1) * 10f);
			angles.Add(offset);
			angles.Add(-offset);
		}

		return angles;
	}

	private int RollDamage(int baseDamage)
	{
		float chance = Mathf.Clamp(CritChance, 0f, 0.95f);
		bool crit = _rng.Randf() < chance;
		if (!crit)
			return baseDamage;

		float mult = Mathf.Max(1f, CritDamageMultiplier);
		return Mathf.Max(baseDamage, Mathf.RoundToInt(baseDamage * mult));
	}

	private void SpawnProjectile(Vector2 dir, float speed, int damage)
	{
		Node bullet = ProjectileScene.Instantiate();
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

	public void MultiplyDamage(float factor)
	{
		Damage = Mathf.Max(1, Mathf.RoundToInt(Damage * Mathf.Max(0.1f, factor)));
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
		CritChance = 0f;
		CritDamageMultiplier = 1.5f;
		ExtraProjectiles = 0;
		SplitShotLevel = 0;
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

		FireVolley(_burstDir, _burstSpeed, _burstBaseDamage);
		_burstShotsRemaining--;
		if (_burstShotsRemaining > 0)
			_burstTimer = Mathf.Max(0.01f, BurstShotInterval);
	}

	public void AddProjectileCount(int amount)
	{
		ExtraProjectiles = Mathf.Clamp(ExtraProjectiles + amount, 0, 10);
	}

	public void AddSplitShotLevel(int amount)
	{
		SplitShotLevel = Mathf.Clamp(SplitShotLevel + amount, 0, 2);
	}

	public void AddCritChance(float amount)
	{
		CritChance = Mathf.Clamp(CritChance + amount, 0f, 0.95f);
	}
}
