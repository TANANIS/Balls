using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
	[Export] public float TimeWeight = 0.20f;

	[Export] public float RisePerSecond = 45f;
	[Export] public float FallPerSecond = 20f;

	[Export] public bool VerboseLog = true;
	[Export] public float LogInterval = 0.5f;

	private PlayerHealth _playerHealth;
	private Node _player;
	private Node2D _enemiesRoot;
	private UpgradeMenu _upgradeMenu;
	private CombatSystem _combatSystem;

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

		if (_playerHealth == null)
			DebugSystem.Error("[PressureSystem] PlayerHealth not found.");
		if (_enemiesRoot == null)
			DebugSystem.Error("[PressureSystem] Enemies root not found.");
		if (_upgradeMenu == null)
			DebugSystem.Error("[PressureSystem] UpgradeMenu not found.");
		if (_combatSystem == null)
			DebugSystem.Error("[PressureSystem] CombatSystem not found.");

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

		float timeFactor = 0f;
		if (SecondsForMaxTimePressure > 0f)
			timeFactor = Mathf.Clamp(_survivalSeconds / SecondsForMaxTimePressure, 0f, 1f);

		float weighted = (enemyFactor * EnemyWeight) + (hpFactor * LowHpWeight) + (timeFactor * TimeWeight);
		return Mathf.Clamp(weighted, 0f, 1f) * MaxPressure;
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

	private void LoadTierRulesFromCsv()
	{
		_tierRules.Clear();

		if (string.IsNullOrWhiteSpace(PressureTierRulesCsvPath))
		{
			DebugSystem.Warn("[PressureSystem] PressureTierRulesCsvPath is empty. Using inspector values.");
			return;
		}

		if (!FileAccess.FileExists(PressureTierRulesCsvPath))
		{
			DebugSystem.Warn($"[PressureSystem] CSV not found: {PressureTierRulesCsvPath}. Using inspector values.");
			return;
		}

		using var file = FileAccess.Open(PressureTierRulesCsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			DebugSystem.Warn($"[PressureSystem] Failed to open CSV: {PressureTierRulesCsvPath}. Using inspector values.");
			return;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine().Trim();
			if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
				continue;

			var cols = ParseCsvLine(line);
			if (cols.Count < 18)
				continue;

			var rule = new TierRule
			{
				Tier = ParseInt(cols[0], 0),
				PressureMin = ParseFloat(cols[1], 0f),
				PressureMax = ParseFloat(cols[2], 100f),
				MaxAlive = ParseInt(cols[7], EnemyCountForMaxPressure),
				KillProgressBase = ParseFloat(cols[13], KillProgressBase),
				KillPressureBonusFactor = ParseFloat(cols[14], KillPressureBonusFactor),
				TimeProgressPerSecond = ParseFloat(cols[15], TimeProgressPerSecond),
				UpgradeThreshold = ParseFloat(cols[16], TriggerThreshold),
				FirstUpgradeThreshold = ParseFloat(cols[17], FirstTriggerThreshold)
			};

			_tierRules.Add(rule);
		}

		_tierRules.Sort((a, b) => a.PressureMin.CompareTo(b.PressureMin));
		DebugSystem.Log($"[PressureSystem] Loaded {_tierRules.Count} tier rule rows from CSV.");
	}

	private void UpdateTierRuntimeSettings()
	{
		if (!UseTierRulesCsv || _tierRules.Count == 0)
			return;

		int idx = -1;
		for (int i = 0; i < _tierRules.Count; i++)
		{
			var rule = _tierRules[i];
			if (_pressure >= rule.PressureMin && _pressure < rule.PressureMax)
			{
				idx = i;
				break;
			}
		}
		if (idx < 0)
			idx = _tierRules.Count - 1;

		if (idx == _activeTierIndex)
			return;

		_activeTierIndex = idx;
		var active = _tierRules[idx];

		EnemyCountForMaxPressure = Mathf.Max(1, active.MaxAlive);
		KillProgressBase = Mathf.Max(0f, active.KillProgressBase);
		KillPressureBonusFactor = Mathf.Max(0f, active.KillPressureBonusFactor);
		TimeProgressPerSecond = Mathf.Max(0f, active.TimeProgressPerSecond);
		TriggerThreshold = Mathf.Clamp(active.UpgradeThreshold, 1f, MaxUpgradeProgress);
		FirstTriggerThreshold = Mathf.Clamp(active.FirstUpgradeThreshold, 1f, MaxUpgradeProgress);

		DebugSystem.Log($"[PressureSystem] Tier {active.Tier} active: killBase={KillProgressBase:F1}, bonus={KillPressureBonusFactor:F2}, drip={TimeProgressPerSecond:F2}, threshold={TriggerThreshold:F1}");
	}

	private static List<string> ParseCsvLine(string line)
	{
		var result = new List<string>();
		var sb = new StringBuilder();
		bool inQuotes = false;

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				result.Add(sb.ToString().Trim());
				sb.Clear();
				continue;
			}

			sb.Append(c);
		}

		result.Add(sb.ToString().Trim());
		return result;
	}

	private static float ParseFloat(string s, float fallback)
	{
		if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
			return v;
		return fallback;
	}

	private static int ParseInt(string s, int fallback)
	{
		if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
			return v;
		return fallback;
	}

	private struct TierRule
	{
		public int Tier;
		public float PressureMin;
		public float PressureMax;
		public int MaxAlive;
		public float KillProgressBase;
		public float KillPressureBonusFactor;
		public float TimeProgressPerSecond;
		public float UpgradeThreshold;
		public float FirstUpgradeThreshold;
	}
}
