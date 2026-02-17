using Godot;
using System;
using System.Collections.Generic;

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
}
