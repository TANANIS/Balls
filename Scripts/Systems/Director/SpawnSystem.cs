using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public partial class SpawnSystem : Node
{
	[Export] public PackedScene EnemyScene;
	[Export] public NodePath EnemiesPath = "../Enemies";
	[Export] public NodePath PlayerPath = "../Player";

	[Export] public float SpawnInterval = 1.0f;
	[Export] public int MaxAliveEnemies = 50;
	[Export] public float SpawnRadiusMin = 420f;
	[Export] public float SpawnRadiusMax = 560f;

	[Export] public bool UseTierRulesCsv = true;
	[Export] public string PressureTierRulesCsvPath = "res://Data/Director/PressureTierRules.csv";
	[Export] public string EnemyDefinitionsCsvPath = "res://Data/Director/EnemyDefinitions.csv";
	[Export] public string TierEnemyWeightsCsvPath = "res://Data/Director/TierEnemyWeights.csv";
	[Export] public bool VerboseLog = true;
	[Export] public bool UseUpgradeCountUnlocks = true;
	[Export] public int EliteUnlockUpgradeCount = 4;
	[Export] public float EliteInjectChanceMin = 0.10f;
	[Export] public float EliteInjectChanceMax = 0.15f;
	[Export] public string EliteEnemyId = "elite_swarm_circle";
	[Export] public int MiniBossUnlockUpgradeCount = 6;
	[Export] public string MiniBossEnemyId = "miniboss_hex";
	[Export] public float MiniBossFreezeSeconds = 2.0f;

	[Export] public float LateGameStartSeconds = 60f;
	[Export] public float LateGameSpawnIntervalMinClamp = 0.1f;
	[Export] public int LateGameMaxAliveCap = 900;
	[Export] public float LateGameSecondRampStartSeconds = 240f;
	[Export] public float LateGameSecondRampStepSeconds = 20f;
	[Export] public float LateGameEliteChanceMultiplier = 2.0f;
	[Export] public int LateGameEliteUnlockReduction = 4;
	[Export] public int LateGameMiniBossUnlockReduction = 4;
	[Export] public float LateGameMiniBossInterval = 60f;
	[Export] public float LateGameWeightSwarm = 40f;
	[Export] public float LateGameWeightCharger = 30f;
	[Export] public float LateGameWeightTank = 20f;
	[Export] public float LateGameWeightElite = 10f;

	private Node2D _enemiesRoot;
	private Node2D _player;
	private PressureSystem _pressureSystem;
	private UpgradeSystem _upgradeSystem;
	private float _timer;
	private int _activeTier = -1;
	private int _activeTierRuleIndex = -1;
	private float _activeSpawnIntervalMin;
	private float _activeSpawnIntervalMax;
	private int _activeMaxAlive;
	private float _baseSpawnIntervalMin;
	private float _baseSpawnIntervalMax;
	private int _baseMaxAlive;
	private float _activeSpawnRadiusMin;
	private float _activeSpawnRadiusMax;
	private readonly List<TierRule> _tierRules = new();
	private readonly Dictionary<string, EnemyDefinition> _enemyDefinitions = new();
	private readonly Dictionary<int, List<WeightedEnemy>> _tierWeights = new();
	private readonly RandomNumberGenerator _rng = new();
	private bool _miniBossScheduled = false;
	private bool _miniBossSpawned = false;
	private float _spawnFreezeTimer = 0f;
	private float _survivalSeconds = 0f;
	private float _nextLateMiniBossAt = -1f;

	public override void _Ready()
	{
		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		_rng.Randomize();

		if (EnemyScene == null)
			DebugSystem.Warn("[SpawnSystem] EnemyScene is null. CSV and weighted spawn must be valid.");

		if (_enemiesRoot == null)
			DebugSystem.Error("[SpawnSystem] Enemies root not found. Check EnemiesPath.");

		if (_player == null)
			DebugSystem.Error("[SpawnSystem] Player not found. Check PlayerPath.");

		EnsurePressureSystem();
		EnsureUpgradeSystem();
		ApplyFallbackRuntimeSettings();

		if (UseTierRulesCsv)
		{
			LoadTierRulesFromCsv();
			LoadEnemyDefinitionsFromCsv();
			LoadTierWeightsFromCsv();
		}

		ResetSpawnTimer();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_enemiesRoot == null || _player == null)
			return;

		_survivalSeconds += (float)delta;

		EnsurePressureSystem();
		EnsureUpgradeSystem();
		UpdateTierRuntimeSettings();
		UpdateUpgradeDrivenEvents((float)delta);
		TryLateGameMiniBoss();

		if (_spawnFreezeTimer > 0f)
			return;

		int maxAlive = GetLateGameMaxAlive();
		if (_enemiesRoot.GetChildCount() >= maxAlive)
			return;

		_timer -= (float)delta;
		if (_timer > 0f)
			return;

		ResetSpawnTimer();
		SpawnOne();
	}

	private void SpawnOne()
	{
		PackedScene scene = PickEnemySceneForCurrentTier();
		if (scene == null)
			return;

		if (scene.Instantiate() is not Node2D enemy)
		{
			DebugSystem.Error("[SpawnSystem] Spawn scene root must inherit Node2D.");
			return;
		}

		enemy.GlobalPosition = GetSpawnPositionAroundPlayer();
		_enemiesRoot.AddChild(enemy);
	}

	private Vector2 GetSpawnPositionAroundPlayer()
	{
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		float radius = _rng.RandfRange(_activeSpawnRadiusMin, _activeSpawnRadiusMax);

		Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		return _player.GlobalPosition + offset;
	}

	private void EnsurePressureSystem()
	{
		if (IsInstanceValid(_pressureSystem))
			return;

		var list = GetTree().GetNodesInGroup("PressureSystem");
		if (list.Count > 0)
			_pressureSystem = list[0] as PressureSystem;
	}

	private void EnsureUpgradeSystem()
	{
		if (IsInstanceValid(_upgradeSystem))
			return;

		var list = GetTree().GetNodesInGroup("UpgradeSystem");
		if (list.Count > 0)
			_upgradeSystem = list[0] as UpgradeSystem;
	}

	private void ApplyFallbackRuntimeSettings()
	{
		_activeSpawnIntervalMin = Mathf.Max(0.05f, SpawnInterval);
		_activeSpawnIntervalMax = _activeSpawnIntervalMin;
		_activeMaxAlive = Mathf.Max(1, MaxAliveEnemies);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
		_baseMaxAlive = _activeMaxAlive;
		_activeSpawnRadiusMin = Mathf.Max(1f, SpawnRadiusMin);
		_activeSpawnRadiusMax = Mathf.Max(_activeSpawnRadiusMin, SpawnRadiusMax);
	}

	private void ResetSpawnTimer()
	{
		float min = Mathf.Max(0.05f, GetLateGameSpawnInterval(_baseSpawnIntervalMin));
		float max = Mathf.Max(min, GetLateGameSpawnInterval(_baseSpawnIntervalMax));
		_timer = _rng.RandfRange(min, max);
	}

	private PackedScene PickEnemySceneForCurrentTier()
	{
		if (!UseTierRulesCsv || _enemyDefinitions.Count == 0 || _tierWeights.Count == 0)
			return EnemyScene;

		List<WeightedEnemy> weights = GetWeightsForTier(_activeTier);
		if (weights == null || weights.Count == 0)
			return EnemyScene;

		float total = 0f;
		int upgradeCount = GetUpgradeCount();
		bool eliteUnlocked = !UseUpgradeCountUnlocks || upgradeCount >= EliteUnlockUpgradeCount;
		foreach (var item in weights)
		{
			if (item.Weight <= 0f)
				continue;

			if (!_enemyDefinitions.TryGetValue(item.EnemyId, out EnemyDefinition def))
				continue;

			if (_activeTier < def.MinTier || def.Scene == null)
				continue;
			if (!eliteUnlocked && string.Equals(item.EnemyId, EliteEnemyId, StringComparison.OrdinalIgnoreCase))
				continue;

			total += item.Weight;
		}

		if (total <= 0f)
			return EnemyScene;

		float roll = _rng.RandfRange(0f, total);
		float accumulated = 0f;

		foreach (var item in weights)
		{
			if (item.Weight <= 0f)
				continue;

			if (!_enemyDefinitions.TryGetValue(item.EnemyId, out EnemyDefinition def))
				continue;

			if (_activeTier < def.MinTier || def.Scene == null)
				continue;
			if (!eliteUnlocked && string.Equals(item.EnemyId, EliteEnemyId, StringComparison.OrdinalIgnoreCase))
				continue;

			accumulated += item.Weight;
			if (roll <= accumulated)
			{
				PackedScene picked = def.Scene;
				return TryInjectElite(picked, upgradeCount);
			}
		}

		return TryInjectElite(EnemyScene, upgradeCount);
	}

	private List<WeightedEnemy> GetWeightsForTier(int tier)
	{
		if (IsLateGame())
			return BuildLateGameWeights(tier);

		if (_tierWeights.TryGetValue(tier, out List<WeightedEnemy> weights))
			return weights;

		for (int t = tier - 1; t >= 0; t--)
		{
			if (_tierWeights.TryGetValue(t, out weights))
				return weights;
		}

		return null;
	}

	private List<WeightedEnemy> BuildLateGameWeights(int tier)
	{
		var list = new List<WeightedEnemy>();

		TryAddWeight(list, "swarm_circle", LateGameWeightSwarm, tier);
		TryAddWeight(list, "charger_triangle", LateGameWeightCharger, tier);
		TryAddWeight(list, "tank_square", LateGameWeightTank, tier);
		TryAddWeight(list, "elite_swarm_circle", LateGameWeightElite, tier);

		return list;
	}

	private void TryAddWeight(List<WeightedEnemy> list, string enemyId, float weight, int tier)
	{
		if (weight <= 0f)
			return;
		if (!_enemyDefinitions.TryGetValue(enemyId, out EnemyDefinition def))
			return;
		if (def.Scene == null || tier < def.MinTier)
			return;

		list.Add(new WeightedEnemy { EnemyId = enemyId, Weight = weight });
	}

	private void UpdateTierRuntimeSettings()
	{
		if (!UseTierRulesCsv || _tierRules.Count == 0)
			return;

		float pressure = _pressureSystem?.CurrentPressure ?? 0f;
		int idx = FindTierRuleIndex(pressure);
		if (idx < 0)
			return;
		if (idx == _activeTierRuleIndex)
			return;

		_activeTierRuleIndex = idx;
		TierRule active = _tierRules[idx];
		_activeTier = Mathf.Max(active.Tier, GetTimeForcedTier());
		_activeSpawnIntervalMin = Mathf.Max(0.05f, active.SpawnIntervalMin);
		_activeSpawnIntervalMax = Mathf.Max(_activeSpawnIntervalMin, active.SpawnIntervalMax);
		_activeMaxAlive = Mathf.Max(1, active.MaxAlive);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
		_baseMaxAlive = _activeMaxAlive;
		_activeSpawnRadiusMin = Mathf.Max(1f, active.SpawnRadiusMin);
		_activeSpawnRadiusMax = Mathf.Max(_activeSpawnRadiusMin, active.SpawnRadiusMax);

		if (VerboseLog)
		{
			DebugSystem.Log(
				$"[SpawnSystem] Tier={active.Tier} pressure={pressure:F1} " +
				$"spawn={_activeSpawnIntervalMin:F2}-{_activeSpawnIntervalMax:F2}s " +
				$"maxAlive={_activeMaxAlive} radius={_activeSpawnRadiusMin:F0}-{_activeSpawnRadiusMax:F0}");
		}

		ResetSpawnTimer();
	}

	private int GetTimeForcedTier()
	{
		// Force higher tiers by time so tanks/elites appear quickly.
		if (_survivalSeconds >= 240f) return 4;
		if (_survivalSeconds >= 180f) return 4;
		if (_survivalSeconds >= 120f) return 3;
		if (_survivalSeconds >= 60f) return 2;
		return -1;
	}

	private bool IsLateGame()
	{
		return LateGameStartSeconds > 0f && _survivalSeconds >= LateGameStartSeconds;
	}

	private float GetLateGameMultiplier()
	{
		if (!IsLateGame())
			return 1f;

		float minutesSince = Mathf.Max(0f, (_survivalSeconds - LateGameStartSeconds) / 60f);
		int steps = Mathf.FloorToInt(minutesSince) + 1; // 1 min => x2, 2 min => x4, ...
		float mult = Mathf.Pow(2f, steps);

		// Second ramp: after a later time, keep doubling faster.
		if (LateGameSecondRampStartSeconds > 0f && _survivalSeconds >= LateGameSecondRampStartSeconds)
		{
			float extra = Mathf.Max(0f, _survivalSeconds - LateGameSecondRampStartSeconds);
			int extraSteps = Mathf.FloorToInt(extra / Mathf.Max(1f, LateGameSecondRampStepSeconds));
			if (extraSteps > 0)
				mult *= Mathf.Pow(2f, extraSteps);
		}

		return mult;
	}

	private float GetLateGameSpawnInterval(float baseInterval)
	{
		float mult = GetLateGameMultiplier();
		float interval = baseInterval / mult;
		return Mathf.Max(LateGameSpawnIntervalMinClamp, interval);
	}

	private int GetLateGameMaxAlive()
	{
		float mult = GetLateGameMultiplier();
		int max = Mathf.RoundToInt(_baseMaxAlive * mult);
		if (LateGameMaxAliveCap > 0)
			max = Mathf.Min(max, LateGameMaxAliveCap);
		return Mathf.Max(1, max);
	}

	private void UpdateUpgradeDrivenEvents(float dt)
	{
		if (!UseUpgradeCountUnlocks)
			return;

		if (_spawnFreezeTimer > 0f)
		{
			_spawnFreezeTimer -= dt;
			if (_spawnFreezeTimer <= 0f && _miniBossScheduled && !_miniBossSpawned)
				SpawnScheduledMiniBoss();
			return;
		}

		if (_miniBossScheduled || _miniBossSpawned)
			return;

		int upgradeCount = GetUpgradeCount();
		int required = MiniBossUnlockUpgradeCount;
		if (IsLateGame())
			required = Mathf.Max(0, MiniBossUnlockUpgradeCount - LateGameMiniBossUnlockReduction);
		if (upgradeCount != required)
			return;

		_miniBossScheduled = true;
		_spawnFreezeTimer = Mathf.Max(0f, MiniBossFreezeSeconds);
		DebugSystem.Log($"[SpawnSystem] MiniBoss scheduled at upgrade_count={upgradeCount}, freeze={_spawnFreezeTimer:F2}s");
	}

	private void SpawnScheduledMiniBoss()
	{
		_miniBossScheduled = false;
		_miniBossSpawned = true;

		if (!_enemyDefinitions.TryGetValue(MiniBossEnemyId, out EnemyDefinition def) || def.Scene == null)
		{
			DebugSystem.Warn($"[SpawnSystem] MiniBoss definition missing: {MiniBossEnemyId}");
			return;
		}

		if (def.Scene.Instantiate() is not Node2D miniBoss)
		{
			DebugSystem.Warn("[SpawnSystem] MiniBoss scene root is not Node2D.");
			return;
		}

		miniBoss.GlobalPosition = GetSpawnPositionAroundPlayer();
		_enemiesRoot.AddChild(miniBoss);
		DebugSystem.Log($"[SpawnSystem] MiniBoss spawned: {MiniBossEnemyId}");
	}

	private PackedScene TryInjectElite(PackedScene picked, int upgradeCount)
	{
		if (!UseUpgradeCountUnlocks)
			return picked;
		int required = EliteUnlockUpgradeCount;
		if (IsLateGame())
			required = Mathf.Max(0, EliteUnlockUpgradeCount - LateGameEliteUnlockReduction);
		if (upgradeCount < required)
			return picked;
		if (!_enemyDefinitions.TryGetValue(EliteEnemyId, out EnemyDefinition eliteDef) || eliteDef.Scene == null)
			return picked;

		float min = Mathf.Clamp(EliteInjectChanceMin, 0f, 1f);
		float max = Mathf.Clamp(EliteInjectChanceMax, min, 1f);
		if (IsLateGame())
		{
			min = Mathf.Clamp(min * LateGameEliteChanceMultiplier, 0f, 1f);
			max = Mathf.Clamp(max * LateGameEliteChanceMultiplier, min, 1f);
		}
		float chance = _rng.RandfRange(min, max);
		if (_rng.Randf() <= chance)
			return eliteDef.Scene;

		return picked;
	}

	private int GetUpgradeCount()
	{
		if (_upgradeSystem == null)
			return 0;

		return Mathf.Max(0, _upgradeSystem.AppliedUpgradeCount);
	}

	private void TryLateGameMiniBoss()
	{
		if (!IsLateGame())
			return;
		if (LateGameMiniBossInterval <= 0f)
			return;
		if (_spawnFreezeTimer > 0f)
			return;
		if (_enemiesRoot == null)
			return;

		if (_nextLateMiniBossAt < 0f)
			_nextLateMiniBossAt = LateGameStartSeconds + LateGameMiniBossInterval;

		if (_survivalSeconds >= _nextLateMiniBossAt)
		{
			SpawnScheduledMiniBoss();
			_nextLateMiniBossAt = _survivalSeconds + LateGameMiniBossInterval;
		}
	}

	private int FindTierRuleIndex(float pressure)
	{
		for (int i = 0; i < _tierRules.Count; i++)
		{
			TierRule rule = _tierRules[i];
			if (pressure >= rule.PressureMin && pressure < rule.PressureMax)
				return i;
		}

		return _tierRules.Count > 0 ? _tierRules.Count - 1 : -1;
	}

	private void LoadTierRulesFromCsv()
	{
		_tierRules.Clear();

		if (!TryReadCsvLines(PressureTierRulesCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 10)
				continue;

			_tierRules.Add(new TierRule
			{
				Tier = ParseInt(cols[0], 0),
				PressureMin = ParseFloat(cols[1], 0f),
				PressureMax = ParseFloat(cols[2], 100f),
				SpawnIntervalMin = ParseFloat(cols[3], SpawnInterval),
				SpawnIntervalMax = ParseFloat(cols[4], SpawnInterval),
				MaxAlive = ParseInt(cols[7], MaxAliveEnemies),
				SpawnRadiusMin = ParseFloat(cols[8], SpawnRadiusMin),
				SpawnRadiusMax = ParseFloat(cols[9], SpawnRadiusMax)
			});
		}

		_tierRules.Sort((a, b) => a.PressureMin.CompareTo(b.PressureMin));
		DebugSystem.Log($"[SpawnSystem] Loaded {_tierRules.Count} rows from {PressureTierRulesCsvPath}.");
	}

	private void LoadEnemyDefinitionsFromCsv()
	{
		_enemyDefinitions.Clear();

		if (!TryReadCsvLines(EnemyDefinitionsCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 5)
				continue;

			string id = cols[0];
			string scenePath = cols[1];
			int minTier = ParseInt(cols[4], 0);

			if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(scenePath))
				continue;

			PackedScene scene = GD.Load<PackedScene>(scenePath);
			if (scene == null)
			{
				DebugSystem.Warn($"[SpawnSystem] Missing enemy scene for '{id}': {scenePath}");
				continue;
			}

			_enemyDefinitions[id] = new EnemyDefinition
			{
				Id = id,
				ScenePath = scenePath,
				MinTier = minTier,
				Scene = scene
			};
		}

		DebugSystem.Log($"[SpawnSystem] Loaded {_enemyDefinitions.Count} enemy definitions from {EnemyDefinitionsCsvPath}.");
	}

	private void LoadTierWeightsFromCsv()
	{
		_tierWeights.Clear();

		if (!TryReadCsvLines(TierEnemyWeightsCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 3)
				continue;

			int tier = ParseInt(cols[0], -1);
			string enemyId = cols[1];
			float weight = ParseFloat(cols[2], 0f);

			if (tier < 0 || string.IsNullOrWhiteSpace(enemyId) || weight <= 0f)
				continue;

			if (!_tierWeights.TryGetValue(tier, out List<WeightedEnemy> list))
			{
				list = new List<WeightedEnemy>();
				_tierWeights[tier] = list;
			}

			list.Add(new WeightedEnemy { EnemyId = enemyId, Weight = weight });
		}

		DebugSystem.Log($"[SpawnSystem] Loaded weights for {_tierWeights.Count} tiers from {TierEnemyWeightsCsvPath}.");
	}

	private static bool TryReadCsvLines(string path, out List<string> lines)
	{
		lines = new List<string>();

		if (string.IsNullOrWhiteSpace(path))
		{
			DebugSystem.Warn("[SpawnSystem] CSV path is empty.");
			return false;
		}

		if (!FileAccess.FileExists(path))
		{
			DebugSystem.Warn($"[SpawnSystem] CSV not found: {path}");
			return false;
		}

		using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			DebugSystem.Warn($"[SpawnSystem] Failed to open CSV: {path}");
			return false;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine().Trim();
			if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
				continue;

			lines.Add(line);
		}

		return true;
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
		public float SpawnIntervalMin;
		public float SpawnIntervalMax;
		public int MaxAlive;
		public float SpawnRadiusMin;
		public float SpawnRadiusMax;
	}

	private struct EnemyDefinition
	{
		public string Id;
		public string ScenePath;
		public int MinTier;
		public PackedScene Scene;
	}

	private struct WeightedEnemy
	{
		public string EnemyId;
		public float Weight;
	}
}
