using Godot;
using System;
using System.Collections.Generic;

public partial class PressureSystem : Node
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public NodePath UpgradeMenuPath = "../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu";
	[Export] public bool UseTierRulesCsv = true;
	[Export] public string PressureTierRulesCsvPath = "res://Data/Director/PressureTierRules.csv";

	[Export] public float MaxPressure = 100f;
	[Export] public float TriggerThreshold = 60f;
	[Export] public float FirstTriggerThreshold = 35f;
	[Export] public float TriggerCooldown = 8f;
	[Export] public float PressureDropOnTrigger = 25f;
	[Export] public float MaxUpgradeProgress = 100f;
	[Export] public float ProgressDropOnTrigger = 65f;
	[Export] public float KillProgressBase = 18f;
	[Export] public float KillPressureBonusFactor = 0.8f;
	[Export] public float TimeProgressPerSecond = 0.7f;
	[Export] public bool UseExperiencePickupUpgradeFlow = true;
	[Export] public int ExperiencePerPickup = 1;
	[Export] public float SurvivorXpBaseRequirement = 8f;
	[Export] public float SurvivorXpLinearGrowth = 2f;
	[Export] public float SurvivorXpGrowthFactor = 1.08f;

	[Export] public int EnemyCountForMaxPressure = 24;
	[Export] public float SecondsForMaxTimePressure = 130f;

	[Export] public float EnemyWeight = 0.55f;
	[Export] public float LowHpWeight = 0.25f;
	[Export] public float StabilityWeight = 0.20f;
	[Export] public bool UseLegacyTimePressure = false;
	[Export] public float TimeWeight = 0.0f;

	[Export] public float RisePerSecond = 45f;
	[Export] public float FallPerSecond = 20f;
	[Export] public float StablePressureFallMultiplier = 1.00f;
	[Export] public float EnergyAnomalyPressureFallMultiplier = 0.75f;
	[Export] public float StructuralFracturePressureFallMultiplier = 0.40f;
	[Export] public float CollapseCriticalPressureFallMultiplier = 0.08f;
	[Export] public float PressureWaveFrequency = 0.7f;

	[Export] public bool VerboseLog = true;
	[Export] public float LogInterval = 0.5f;

	private PlayerHealth _playerHealth;
	private Node _player;
	private Node2D _enemiesRoot;
	private UpgradeMenu _upgradeMenu;
	private CombatSystem _combatSystem;
	private StabilitySystem _stabilitySystem;

	private float _pressure = 0f;
	private float _upgradeProgress = 0f;
	private float _currentUpgradeRequirement = 0f;
	private float _triggerCooldownTimer = 0f;
	private float _survivalSeconds = 0f;
	private float _logTimer = 0f;
	private bool _firstUpgradeTriggered = false;
	private bool _upgradeArmed = false;
	private int _upgradeLevel = 0;
	private int _pendingUpgradeOpens = 0;
	private readonly List<TierRule> _tierRules = new();
	private int _activeTierIndex = -1;
	private float _killProgressMultiplier = 1f;
	private float _timeProgressMultiplier = 1f;
	private float _triggerThresholdOffset = 0f;
	private float _pressureDropOnTriggerBonus = 0f;

	public float CurrentPressure => _pressure;
	public float CurrentUpgradeProgress => _upgradeProgress;
	public bool IsUpgradeReady => _pendingUpgradeOpens > 0 || (_upgradeArmed && _triggerCooldownTimer <= 0f);
	public int CurrentUpgradeLevel => _upgradeLevel;
	public int PendingUpgradeCount => _pendingUpgradeOpens;

	public override void _EnterTree()
	{
		AddToGroup("PressureSystem");
	}

	public override void _Ready()
	{
		_player = GetNodeOrNull<Node>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeMenu");
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu");
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combatSystem = list[0] as CombatSystem;
		if (_combatSystem != null)
			_combatSystem.EnemyKilled += OnEnemyKilled;
		var stabilityList = GetTree().GetNodesInGroup("StabilitySystem");
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		if (_playerHealth == null)
			DebugSystem.Error("[PressureSystem] PlayerHealth not found.");
		if (_enemiesRoot == null)
			DebugSystem.Error("[PressureSystem] Enemies root not found.");
		if (_upgradeMenu == null)
			DebugSystem.Error("[PressureSystem] UpgradeMenu not found.");
		if (_combatSystem == null)
			DebugSystem.Error("[PressureSystem] CombatSystem not found.");
		if (_stabilitySystem == null)
			DebugSystem.Warn("[PressureSystem] StabilitySystem not found. Stability pressure contribution disabled.");

		if (UseTierRulesCsv)
			LoadTierRulesFromCsv();

		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());
	}

	public override void _ExitTree()
	{
		if (_combatSystem != null)
			_combatSystem.EnemyKilled -= OnEnemyKilled;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		_survivalSeconds += dt;

		UpdateTierRuntimeSettings();

		if (_triggerCooldownTimer > 0f)
			_triggerCooldownTimer -= dt;

		float target = CalculateTargetPressure();
		float speed = target >= _pressure ? RisePerSecond : GetPhaseFallSpeed();
		_pressure = Mathf.MoveToward(_pressure, target, speed * dt);
		if (!UseExperiencePickupUpgradeFlow && TimeProgressPerSecond > 0f)
			_upgradeProgress = Mathf.Clamp(_upgradeProgress + (TimeProgressPerSecond * _timeProgressMultiplier * dt), 0f, MaxUpgradeProgress);

		if (VerboseLog)
		{
			_logTimer -= dt;
			if (_logTimer <= 0f)
			{
				float req = Mathf.Max(1f, GetCurrentUpgradeRequirement());
				DebugSystem.Log($"[PressureSystem] pressure={_pressure:F1}/{MaxPressure:F1} target={target:F1} xp={_upgradeProgress:F1}/{req:F1} pending={_pendingUpgradeOpens} armed={_upgradeArmed}");
				_logTimer = Mathf.Max(0.1f, LogInterval);
			}
		}

		if (_upgradeMenu == null)
			return;

		if (UseExperiencePickupUpgradeFlow)
		{
			TryConsumePendingUpgrade("pending experience");
			return;
		}

		if (_upgradeMenu.IsOpen)
			return;

		if (_triggerCooldownTimer > 0f)
			return;

		float required = GetCurrentUpgradeRequirement();
		if (_upgradeProgress >= required)
			_upgradeArmed = true;
	}

	private float CalculateTargetPressure()
	{
		float enemyFactor = 0f;
		if (_enemiesRoot != null && EnemyCountForMaxPressure > 0)
			enemyFactor = Mathf.Clamp((float)_enemiesRoot.GetChildCount() / EnemyCountForMaxPressure, 0f, 1f);

		float hpFactor = 0f;
		if (_playerHealth != null && _playerHealth.MaxHp > 0)
			hpFactor = Mathf.Clamp(1f - ((float)_playerHealth.Hp / _playerHealth.MaxHp), 0f, 1f);

		float stabilityFactor = 0f;
		if (_stabilitySystem != null)
			stabilityFactor = Mathf.Clamp(1f - (_stabilitySystem.CurrentStability / 100f), 0f, 1f);

		float timeFactor = 0f;
		if (UseLegacyTimePressure && SecondsForMaxTimePressure > 0f)
			timeFactor = Mathf.Clamp(_survivalSeconds / SecondsForMaxTimePressure, 0f, 1f);

		float weighted = (enemyFactor * EnemyWeight) + (hpFactor * LowHpWeight) + (stabilityFactor * StabilityWeight);
		float totalWeight = EnemyWeight + LowHpWeight + Mathf.Max(0f, StabilityWeight);
		if (UseLegacyTimePressure && TimeWeight > 0f)
		{
			weighted += timeFactor * TimeWeight;
			totalWeight += TimeWeight;
		}

		if (totalWeight <= 0f)
			return 0f;

		float normalized = Mathf.Clamp(weighted / totalWeight, 0f, 1f);
		if (_stabilitySystem != null)
		{
			float amp = Mathf.Clamp(_stabilitySystem.GetPressureFluctuationAmplitude(), 0f, 0.45f);
			if (amp > 0f)
			{
				float wave = Mathf.Sin(_survivalSeconds * Mathf.Tau * Mathf.Max(0.05f, PressureWaveFrequency));
				normalized = Mathf.Clamp(normalized + (wave * amp), 0f, 1f);
			}
		}

		return normalized * MaxPressure;
	}

	private float GetPhaseFallSpeed()
	{
		if (_stabilitySystem == null)
			return FallPerSecond;

		float mult = _stabilitySystem.CurrentPhase switch
		{
			StabilitySystem.StabilityPhase.Stable => StablePressureFallMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyPressureFallMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFracturePressureFallMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalPressureFallMultiplier,
			_ => 1f
		};

		return Mathf.Max(0.01f, FallPerSecond * Mathf.Max(0f, mult));
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (UseExperiencePickupUpgradeFlow)
			return;

		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_triggerCooldownTimer > 0f)
			return;
		if (source == null || _player == null || source != _player)
			return;

		bool wasArmed = _upgradeArmed;

		float pressureNorm = MaxPressure > 0f ? Mathf.Clamp(_pressure / MaxPressure, 0f, 1f) : 0f;
		float gain = KillProgressBase * _killProgressMultiplier * (1f + (pressureNorm * KillPressureBonusFactor));
		_upgradeProgress = Mathf.Clamp(_upgradeProgress + gain, 0f, MaxUpgradeProgress);
		if (VerboseLog)
			DebugSystem.Log($"[PressureSystem] kill gain={gain:F1} progress={_upgradeProgress:F1}/{MaxUpgradeProgress:F1}");

		if (!wasArmed)
			return;

		TriggerUpgradeMenu("kill after threshold");
	}

	public void TriggerUpgradeFromExperiencePickup()
	{
		if (!UseExperiencePickupUpgradeFlow)
			return;

		AddExperienceFromPickup(ExperiencePerPickup);
	}

	public void AddExperienceFromPickup(int amount)
	{
		if (!UseExperiencePickupUpgradeFlow)
			return;
		if (amount <= 0)
			return;

		float expToAdd = Mathf.Max(1f, amount);
		_upgradeProgress += expToAdd;
		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());

		while (_upgradeProgress >= _currentUpgradeRequirement)
		{
			_upgradeProgress -= _currentUpgradeRequirement;
			_upgradeLevel++;
			_pendingUpgradeOpens++;
			_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());
		}

		_upgradeProgress = Mathf.Clamp(_upgradeProgress, 0f, _currentUpgradeRequirement);
		TryConsumePendingUpgrade("experience pickup");
	}

	public float GetCurrentUpgradeRequirement()
	{
		if (UseExperiencePickupUpgradeFlow)
		{
			float level = _upgradeLevel;
			float curve = SurvivorXpBaseRequirement + (SurvivorXpLinearGrowth * level);
			curve *= Mathf.Pow(Mathf.Max(1f, SurvivorXpGrowthFactor), level);
			return Mathf.Max(1f, curve);
		}

		float requiredBase = _firstUpgradeTriggered ? TriggerThreshold : FirstTriggerThreshold;
		return Mathf.Max(5f, requiredBase + _triggerThresholdOffset);
	}

	public void ForceOpenForBoss()
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		TriggerUpgradeMenu("boss/event exception");
	}

	private void TriggerUpgradeMenu(string reason)
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		_upgradeMenu.OpenMenu();
		_firstUpgradeTriggered = true;
		_upgradeArmed = false;

		if (!UseExperiencePickupUpgradeFlow)
		{
			_triggerCooldownTimer = TriggerCooldown;
			_pressure = Mathf.Max(0f, _pressure - (PressureDropOnTrigger + _pressureDropOnTriggerBonus));
			_upgradeProgress = Mathf.Max(0f, _upgradeProgress - ProgressDropOnTrigger);
		}

		DebugSystem.Log($"[PressureSystem] Triggered upgrade menu: {reason}.");
	}

	private void TryConsumePendingUpgrade(string reason)
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_pendingUpgradeOpens <= 0)
			return;

		_pendingUpgradeOpens--;
		TriggerUpgradeMenu(reason);
	}

	public void MultiplyKillProgressGain(float factor)
	{
		_killProgressMultiplier = Mathf.Clamp(_killProgressMultiplier * factor, 0.2f, 4.5f);
	}

	public void MultiplyTimeProgressGain(float factor)
	{
		_timeProgressMultiplier = Mathf.Clamp(_timeProgressMultiplier * factor, 0.2f, 4.5f);
	}

	public void AddTriggerThresholdOffset(float amount)
	{
		_triggerThresholdOffset = Mathf.Clamp(_triggerThresholdOffset + amount, -40f, 40f);
	}

	public void AddPressureDropOnTrigger(float amount)
	{
		_pressureDropOnTriggerBonus = Mathf.Clamp(_pressureDropOnTriggerBonus + amount, -20f, 50f);
	}
}
