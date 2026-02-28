using Godot;
using System.Collections.Generic;

public partial class SpawnSystem : Node
{
	private struct TierRule
	{
		public int Tier;
		public float PressureMin;
		public float PressureMax;
		public float SpawnIntervalMin;
		public float SpawnIntervalMax;
		public int BudgetMin;
		public int BudgetMax;
		public int MaxAlive;
		public float SpawnRadiusMin;
		public float SpawnRadiusMax;
	}

	private struct EnemyDefinition
	{
		public string Id;
		public string ScenePath;
		public int Cost;
		public int MinTier;
		public int HpOverride;
		public float SpeedOverride;
		public int ContactDamageOverride;
		public PackedScene Scene;
	}

	private struct PendingSpawnRequest
	{
		public EnemyDefinition Definition;
		public Vector2 Position;
	}

	private struct WeightedEnemy
	{
		public string EnemyId;
		public float Weight;
	}

	[Export] public PackedScene EnemyScene;
	[Export] public NodePath EnemiesPath = "../Enemies";
	[Export] public NodePath PlayerPath = "../Player";

	[Export] public float SpawnInterval = 1.0f;
	[Export] public int MaxAliveEnemies = 50;
	[Export] public float SpawnRadiusMin = 420f;
	[Export] public float SpawnRadiusMax = 560f;
	[Export] public int SpawnBudgetMin = 2;
	[Export] public int SpawnBudgetMax = 4;
	[Export] public float StableBudgetMultiplier = 1.0f;
	[Export] public float EnergyAnomalyBudgetMultiplier = 1.03f;
	[Export] public float StructuralFractureBudgetMultiplier = 1.08f;
	[Export] public float CollapseCriticalBudgetMultiplier = 1.15f;
	[Export] public float StableTierTailRampMultiplier = 1.35f;
	[Export] public float EnergyAnomalyTierTailRampMultiplier = 1.30f;
	[Export] public float StructuralFractureTierTailRampMultiplier = 1.24f;
	[Export] public float CollapseCriticalTierTailRampMultiplier = 1.16f;
	[Export] public float HordeTargetAliveRatio = 0.82f;
	[Export] public float HordeCatchUpBudgetFactor = 0.22f;
	[Export] public int StablePacksPerWave = 3;
	[Export] public int EnergyAnomalyPacksPerWave = 3;
	[Export] public int StructuralFracturePacksPerWave = 3;
	[Export] public int CollapseCriticalPacksPerWave = 4;
	[Export] public bool UseEncirclementPackLayout = true;
	[Export] public bool UsePlayerPathInterceptCenters = true;
	[Export] public float InterceptLeadSeconds = 0.65f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float InterceptForwardBias = 0.62f;
	[Export] public float InterceptVelocityThreshold = 85f;
	[Export] public float PackAngleJitterDegrees = 20f;
	[Export] public float PackInterceptSpreadDegrees = 62f;
	[Export] public float PackCenterRadiusBias = 1.08f;
	[Export] public float PackScatterRadius = 210f;
	[Export] public float PackMinSpacing = 78f;
	[Export] public int PackPlacementAttempts = 26;
	[Export] public float SpawnStepIntervalMinSeconds = 0.042f;
	[Export] public float SpawnStepIntervalMaxSeconds = 0.095f;
	[Export] public float StableSpawnStepMultiplier = 1.0f;
	[Export] public float EnergyAnomalySpawnStepMultiplier = 0.82f;
	[Export] public float StructuralFractureSpawnStepMultiplier = 0.70f;
	[Export] public float CollapseCriticalSpawnStepMultiplier = 0.56f;
	[Export] public bool UseOpeningRamp = true;
	[Export] public float OpeningRampSeconds = 10f;
	[Export] public float OpeningSpawnIntervalStartMultiplier = 2.2f;
	[Export] public float OpeningBudgetStartMultiplier = 0.34f;
	[Export] public float OpeningMaxAliveStartMultiplier = 0.42f;
	[Export] public float SpawnOutsideViewportMargin = 320f;
	[Export] public float SpawnRingThickness = 460f;
	[Export] public int MaxPendingSpawns = 320;

	[Export] public bool UseTierRulesCsv = true;
	[Export] public string PressureTierRulesCsvPath = "res://Data/Director/PressureTierRules.csv";
	[Export] public string EnemyDefinitionsCsvPath = "res://Data/Director/EnemyDefinitions.csv";
	[Export] public string TierEnemyWeightsCsvPath = "res://Data/Director/TierEnemyWeights.csv";
	[Export] public bool UseUpgradeCountUnlocks = false;
	[Export] public int EliteUnlockUpgradeCount = 4;
	[Export] public float EliteInjectChanceMin = 0.02f;
	[Export] public float EliteInjectChanceMax = 0.05f;
	[Export] public string EliteEnemyId = "werewolf";
	[Export] public int MiniBossUnlockUpgradeCount = 6;
	[Export] public string MiniBossEnemyId = "boss_lancer";
	[Export] public string Phase3MiniBossEnemyId = "boss_greatsword_skeleton";
	[Export] public float MiniBossFreezeSeconds = 2.0f;
	[Export] public bool UsePhaseTailMiniBossSchedule = true;
	[Export] public float Phase1MiniBossAtSeconds = 225f;
	[Export] public float Phase2MiniBossAtSeconds = 450f;
	[Export] public float Phase3MiniBossAtSeconds = 675f;
	[Export] public float Phase4MiniBossAtSeconds = 870f;
	[Export] public float PhaseMiniBossFreezeSeconds = 1.2f;
	[Export] public float PhaseMiniBossScaleBase = 1.15f;
	[Export] public float PhaseMiniBossScaleStep = 0.18f;
	[Export] public int PhaseMiniBossHpBase = 95;
	[Export] public int PhaseMiniBossHpStep = 35;
	[Export] public int PhaseMiniBossContactDamageBase = 1;
	[Export] public int PhaseMiniBossContactDamageStep = 0;
	[Export] public float PhaseTailPrepSeconds = 26f;
	[Export] public float PhaseTailBudgetMultiplier = 0.82f;
	[Export] public float PhaseTailMaxAliveMultiplier = 0.86f;
	[Export] public float PhaseTailSwarmWeightMultiplier = 0.55f;
	[Export] public float PhaseTailChargerWeightMultiplier = 1.35f;
	[Export] public float PhaseTailTankWeightMultiplier = 1.35f;
	[Export] public float PhaseTailEliteWeightMultiplier = 1.20f;

	[Export] public float ChaosWeightSwarm = 40f;
	[Export] public float ChaosWeightCharger = 30f;
	[Export] public float ChaosWeightTank = 20f;
	[Export] public float ChaosWeightElite = 3f;
	[Export(PropertyHint.Range, "0,6,1")] public int LateTierSlimeSuppressionStartTier = 2;
	[Export(PropertyHint.Range, "0.01,1.00,0.01")] public float LateTierSlimeWeightMultiplier = 0.20f;

	private Node2D _enemiesRoot;
	private Node2D _player;
	private UpgradeSystem _upgradeSystem;
	private StabilitySystem _stabilitySystem;
	private float _timer;
	private int _activeTier = -1;
	private int _activeTierRuleIndex = -1;
	private float _activeSpawnIntervalMin;
	private float _activeSpawnIntervalMax;
	private int _activeBudgetMin;
	private int _activeBudgetMax;
	private int _activeMaxAlive;
	private float _baseSpawnIntervalMin;
	private float _baseSpawnIntervalMax;
	private int _baseBudgetMin;
	private int _baseBudgetMax;
	private int _baseMaxAlive;
	private float _activeSpawnRadiusMin;
	private float _activeSpawnRadiusMax;
	private readonly List<TierRule> _tierRules = new();
	private readonly Dictionary<string, EnemyDefinition> _enemyDefinitions = new();
	private readonly Dictionary<int, List<WeightedEnemy>> _tierWeights = new();
	private readonly RandomNumberGenerator _rng = new();
	private readonly Queue<PendingSpawnRequest> _pendingSpawns = new();
	private float _spawnStepTimer = 0f;
	private bool _miniBossScheduled = false;
	private bool _miniBossSpawned = false;
	private int _pendingPhaseMiniBossIndex = -1;
	private readonly bool[] _phaseMiniBossSpawned = new bool[4];
	private float _spawnFreezeTimer = 0f;
	private float _survivalSeconds = 0f;
	private float _nextLateMiniBossAt = -1f;

	[Export] public float StableSpawnRateMultiplier = 1.0f;
	[Export] public float EnergyAnomalySpawnRateMultiplier = 1.02f;
	[Export] public float StructuralFractureSpawnRateMultiplier = 1.08f;
	[Export] public float CollapseCriticalSpawnRateMultiplier = 1.16f;
	[Export] public float SpawnIntervalMinClamp = 0.1f;

	[Export] public float StableMaxAliveMultiplier = 1.0f;
	[Export] public float EnergyAnomalyMaxAliveMultiplier = 1.02f;
	[Export] public float StructuralFractureMaxAliveMultiplier = 1.06f;
	[Export] public float CollapseCriticalMaxAliveMultiplier = 1.12f;
	[Export] public int MaxAliveCap = 900;

	[Export] public float CriticalMiniBossInterval = 60f;
	[Export] public int EnergyAnomalyEliteUnlockReduction = 1;
	[Export] public int StructuralFractureEliteUnlockReduction = 2;
	[Export] public int CollapseCriticalEliteUnlockReduction = 4;
	[Export] public int StructuralFractureMiniBossUnlockReduction = 2;
	[Export] public int CollapseCriticalMiniBossUnlockReduction = 4;
	[Export] public float StructuralFractureEliteChanceMultiplier = 1.25f;
	[Export] public float CollapseCriticalEliteChanceMultiplier = 2.0f;
	[Export] public float CollapseCriticalSpawnChaosJitter = 0.35f;

	public override void _EnterTree()
	{
		AddToGroup(RuntimeGroups.SpawnSystem);
	}

	public override void _Ready()
	{
		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		_rng.Randomize();

		EnsureUpgradeSystem();
		ApplyFallbackRuntimeSettings();

		if (UseTierRulesCsv)
		{
			LoadTierRulesFromCsv();
			LoadEnemyDefinitionsFromCsv();
			LoadTierWeightsFromCsv();
			ReportCsvLoadHealth();
		}

		ResetSpawnTimer();
	}

	private void ReportCsvLoadHealth()
	{
		if (_tierRules.Count > 0 && _enemyDefinitions.Count > 0 && _tierWeights.Count > 0)
			return;

		GD.PushError(
			$"[SpawnSystem] CSV runtime data incomplete. " +
			$"tier_rules={_tierRules.Count}, enemy_defs={_enemyDefinitions.Count}, tier_weights={_tierWeights.Count}. " +
			$"Check export include_filter for Data/Director/*.csv.");
	}

	public override void _PhysicsProcess(double delta)
	{
		EnsureSpawnAnchors();
		if (_enemiesRoot == null || _player == null)
			return;

		_survivalSeconds += (float)delta;

		EnsureUpgradeSystem();
		EnsureStabilitySystem();
		UpdateTierRuntimeSettings();
		UpdatePhaseTailMiniBossSchedule((float)delta);
		TickFarEnemyRecycle((float)delta);

		if (_spawnFreezeTimer > 0f)
			return;

		int maxAlive = GetPhaseMaxAlive();
		TrySpawnPending((float)delta, maxAlive);
		int alive = _enemiesRoot.GetChildCount();
		int effectiveAlive = alive + _pendingSpawns.Count;
		if (effectiveAlive >= maxAlive)
			return;

		_timer -= (float)delta;
		if (_timer > 0f)
			return;

		ResetSpawnTimer();
		if (MaxPendingSpawns > 0 && _pendingSpawns.Count >= MaxPendingSpawns)
			return;

		ScheduleWave(effectiveAlive, maxAlive);
	}
}
