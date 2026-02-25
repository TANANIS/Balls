using Godot;

public partial class PlayerMelee : PlayerAbilityModule
{
	[Export] public string AttackAction = InputActions.AttackSecondary;
	[Export] public bool EnabledInCurrentCharacter = true;
	[Export] public float Cooldown = 0.35f;
	[Export] public float Range = 140f;
	[Export] public float ArcDegrees = 220f;
	[Export] public int Damage = 3;
	[Export] public float DamageMultiplier = 1f;
	[Export] public uint TargetMask = 1u << 5; // Layer 6: EnemyHurtbox

	[Export] public PackedScene MeleeVfxScene;
	[Export] public NodePath VfxPath;
	[Export] public float VfxDuration = 0.12f;
	[Export] public Color VfxColor = new Color(1f, 0.9f, 0.2f, 0.7f);
	[Export] public float VfxForwardOffset = 36f;
	[Export] public float VfxSideOffset = 0f;
	[Export(PropertyHint.Range, "0.01,1.50,0.01")] public float WindupSeconds = 0.10f;
	[Export(PropertyHint.Range, "0.05,0.95,0.01")] public float HitAtNormalizedTime = 0.55f;
	[Export(PropertyHint.Range, "0,0.50,0.01")] public float RecoverSeconds = 0.02f;

	private CombatSystem _combat;
	private float _attackAnimationSpeedMultiplier = 1f;
	private float _cooldownTimer = 0f;
	private string _resolvedAction = InputActions.AttackSecondary;
	private readonly PlayerMeleeTimeline _timeline = new();

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentRange => Range;
	public float CurrentArcDegrees => ArcDegrees;

	public void Setup(Player player)
	{
		SetupAbility(player, EnabledInCurrentCharacter);

		// Resolve combat service from group to keep scene wiring flexible.
		TryResolveCombatSystem();

		ResolveInputAction();
	}

	public void Tick(float dt)
	{
		Tick(dt, Input.IsActionPressed(_resolvedAction));
	}

	public void Tick(float dt, bool wantAttack)
	{
		if (!_isEnabled)
			return;

		EnsureStabilitySystem();
		TickCooldown(ref _cooldownTimer, dt);
		_timeline.Tick(dt, OnTimelineHit);
		if (_cooldownTimer > 0f || _timeline.IsBusy)
			return;

		if (!wantAttack)
			return;

		if (_combat == null)
			TryResolveCombatSystem();
		if (_combat == null)
			return;

		StartAttackTimeline();
		float powerMult = GetPowerMultiplier();
		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void ResolveInputAction()
	{
		_resolvedAction = ResolveInputActionOrFallback(AttackAction);
	}

	public void SetEnabled(bool enabled)
	{
		SetEnabledState(enabled);
		EnabledInCurrentCharacter = enabled;
	}

	public void SetAttackAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
			return;
		AttackAction = action;
		ResolveInputAction();
	}

	public void ResetRuntimeState()
	{
		_attackAnimationSpeedMultiplier = 1f;
		_cooldownTimer = 0f;
		_timeline.Reset();
	}

	private void StartAttackTimeline()
	{
		if (_player == null)
			return;

		float windup = Mathf.Clamp(WindupSeconds, 0.01f, 1.5f);
		float recover = Mathf.Clamp(RecoverSeconds, 0f, 0.5f);
		float baseTotalDuration = Mathf.Clamp(windup + recover, 0.06f, 1.5f);
		float runtimeTotalDuration = _player.TriggerPrimaryAttackAnimationAndGetDuration(
			baseTotalDuration,
			_attackAnimationSpeedMultiplier);
		float durationScale = runtimeTotalDuration / Mathf.Max(0.01f, baseTotalDuration);
		float runtimeWindup = Mathf.Max(0.01f, windup * durationScale);
		float runtimeRecover = Mathf.Max(0f, recover * durationScale);
		Vector2 attackDir = _player.GetAimDirection(_player.LastMoveDir);

		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		float runtimeRange = Range * (1f + ((powerMult - 1f) * 0.25f));
		float dmgMult = Mathf.Max(0.1f, DamageMultiplier);
		int runtimeDamage = Mathf.Max(1, Mathf.RoundToInt(Damage * dmgMult * powerMult));

		_timeline.BeginAttack(
			windupDurationSeconds: runtimeWindup,
			hitAtNormalized: HitAtNormalizedTime,
			recoverDurationSeconds: runtimeRecover,
			attackDir: attackDir,
			attackRange: runtimeRange,
			attackDamage: runtimeDamage);
	}

	private void OnTimelineHit(Vector2 attackDir, float runtimeRange, int runtimeDamage)
	{
		if (_combat == null || _player == null)
			return;

		AudioManager.Instance?.PlaySfxPlayerMelee();
		SpawnVfx(attackDir, runtimeRange);
		QueryAndApplyMeleeDamage(attackDir, runtimeRange, runtimeDamage);
	}

	private void TryResolveCombatSystem()
	{
		if (_combat != null)
			return;

		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;
	}
}
