using Godot;
using System;
using System.Collections.Generic;

public partial class PressureSystem : Node
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public NodePath UpgradeMenuPath = "../../CanvasLayer/UI/UpgradeMenu";
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

	[Export] public int EnemyCountForMaxPressure = 24;
	[Export] public float SecondsForMaxTimePressure = 130f;

	[Export] public float EnemyWeight = 0.55f;
	[Export] public float LowHpWeight = 0.25f;
	[Export] public float StabilityWeight = 0.20f;
	[Export] public bool UseLegacyTimePressure = false;
	[Export] public float TimeWeight = 0.0f;

	[Export] public float RisePerSecond = 45f;
	[Export] public float FallPerSecond = 20f;
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
	private float _triggerCooldownTimer = 0f;
	private float _survivalSeconds = 0f;
	private float _logTimer = 0f;
	private bool _firstUpgradeTriggered = false;
	private bool _upgradeArmed = false;
	private readonly List<TierRule> _tierRules = new();
	private int _activeTierIndex = -1;

	public float CurrentPressure => _pressure;
	public float CurrentUpgradeProgress => _upgradeProgress;

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
		float speed = target >= _pressure ? RisePerSecond : FallPerSecond;
		_pressure = Mathf.MoveToward(_pressure, target, speed * dt);
		if (TimeProgressPerSecond > 0f)
			_upgradeProgress = Mathf.Clamp(_upgradeProgress + (TimeProgressPerSecond * dt), 0f, MaxUpgradeProgress);

		if (VerboseLog)
		{
			_logTimer -= dt;
			if (_logTimer <= 0f)
			{
				DebugSystem.Log($"[PressureSystem] pressure={_pressure:F1}/{MaxPressure:F1} target={target:F1} progress={_upgradeProgress:F1}/{MaxUpgradeProgress:F1} armed={_upgradeArmed}");
				_logTimer = Mathf.Max(0.1f, LogInterval);
			}
		}

		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		if (_triggerCooldownTimer > 0f)
			return;

		float required = _firstUpgradeTriggered ? TriggerThreshold : FirstTriggerThreshold;
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

	private void OnEnemyKilled(Node source, Node target)
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_triggerCooldownTimer > 0f)
			return;
		if (source == null || _player == null || source != _player)
			return;

		bool wasArmed = _upgradeArmed;

		float pressureNorm = MaxPressure > 0f ? Mathf.Clamp(_pressure / MaxPressure, 0f, 1f) : 0f;
		float gain = KillProgressBase * (1f + (pressureNorm * KillPressureBonusFactor));
		_upgradeProgress = Mathf.Clamp(_upgradeProgress + gain, 0f, MaxUpgradeProgress);
		if (VerboseLog)
			DebugSystem.Log($"[PressureSystem] kill gain={gain:F1} progress={_upgradeProgress:F1}/{MaxUpgradeProgress:F1}");

		if (!wasArmed)
			return;

		TriggerUpgradeMenu("kill after threshold");
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
		_triggerCooldownTimer = TriggerCooldown;
		_pressure = Mathf.Max(0f, _pressure - PressureDropOnTrigger);
		_upgradeProgress = Mathf.Max(0f, _upgradeProgress - ProgressDropOnTrigger);
		DebugSystem.Log($"[PressureSystem] Triggered upgrade menu: {reason}.");
	}
}
