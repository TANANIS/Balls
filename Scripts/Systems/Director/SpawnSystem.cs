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
	[Export] public float HordeTargetAliveRatio = 0.90f;
	[Export] public float HordeCatchUpBudgetFactor = 0.40f;
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
	[Export] public bool VerboseLog = true;
	[Export] public bool UseUpgradeCountUnlocks = false;
	[Export] public int EliteUnlockUpgradeCount = 4;
	[Export] public float EliteInjectChanceMin = 0.02f;
	[Export] public float EliteInjectChanceMax = 0.05f;
	[Export] public string EliteEnemyId = "elite_swarm_circle";
	[Export] public int MiniBossUnlockUpgradeCount = 6;
	[Export] public string MiniBossEnemyId = "miniboss_hex";
	[Export] public float MiniBossFreezeSeconds = 2.0f;
	[Export] public bool UsePhaseTailMiniBossSchedule = true;
	[Export] public float Phase1MiniBossAtSeconds = 225f;
	[Export] public float Phase2MiniBossAtSeconds = 450f;
	[Export] public float Phase3MiniBossAtSeconds = 675f;
	[Export] public float Phase4MiniBossAtSeconds = 870f;
	[Export] public float PhaseMiniBossFreezeSeconds = 1.2f;
	[Export] public float PhaseMiniBossScaleBase = 1.15f;
	[Export] public float PhaseMiniBossScaleStep = 0.18f;
	[Export] public int PhaseMiniBossHpBase = 120;
	[Export] public int PhaseMiniBossHpStep = 50;
	[Export] public int PhaseMiniBossContactDamageBase = 3;
	[Export] public int PhaseMiniBossContactDamageStep = 1;
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

	private Node2D _enemiesRoot;
	private Node2D _player;
	private PressureSystem _pressureSystem;
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
	private readonly Queue<(PackedScene scene, Vector2 pos)> _pendingSpawns = new();
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
		AddToGroup("SpawnSystem");
	}

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
		EnsureStabilitySystem();
		UpdateTierRuntimeSettings();
		UpdatePhaseTailMiniBossSchedule((float)delta);

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

	private void ScheduleWave(int aliveCount, int maxAlive)
	{
		int budget = RollWaveBudget(aliveCount, maxAlive);
		int spawnSlots = Mathf.Max(0, maxAlive - aliveCount);
		int packs = Mathf.Clamp(GetPhasePackCount(), 1, Mathf.Max(1, spawnSlots));
		if (budget <= 0 || spawnSlots <= 0)
			return;

		int upgradeCount = GetUpgradeCount();
		int spawned = 0;
		int remainingBudget = budget;
		int remainingSlots = spawnSlots;
		List<Vector2> packCenters = BuildPackCenters(packs);
		for (int i = 0; i < packs; i++)
		{
			if (remainingBudget <= 0 || remainingSlots <= 0)
				break;

			int remainingPacks = packs - i;
			int packBudget = Mathf.Max(1, remainingBudget / remainingPacks);
			int packSlots = Mathf.Max(1, remainingSlots / remainingPacks);
			Vector2 center = i < packCenters.Count ? packCenters[i] : GetSpawnPositionAroundPlayer();
			spawned += SchedulePackedGroup(center, packBudget, packSlots, upgradeCount);
			remainingBudget = Mathf.Max(0, remainingBudget - packBudget);
			remainingSlots = Mathf.Max(0, remainingSlots - packSlots);
		}

		if (VerboseLog && spawned > 0)
		{
			DebugSystem.Log($"[SpawnSystem] Wave queued={spawned} budget={budget} alive={aliveCount}->{aliveCount + spawned}/{maxAlive} phase={GetCurrentPhase()} tier={_activeTier}");
		}
	}

	private int SchedulePackedGroup(Vector2 center, int budget, int spawnSlots, int upgradeCount)
	{
		if (budget <= 0 || spawnSlots <= 0)
			return 0;

		int spawned = 0;
		int remainingBudget = budget;
		var usedOffsets = new List<Vector2>(Mathf.Max(1, spawnSlots));

		for (int i = 0; i < spawnSlots && remainingBudget > 0; i++)
		{
			if (!TryPickEnemyDefinitionForCurrentTier(remainingBudget, upgradeCount, out EnemyDefinition def))
				break;

			if (!TryFindPackOffset(usedOffsets, out Vector2 offset))
				break;

			EnqueueSpawn(def.Scene, center + offset);
			spawned++;
			remainingBudget -= Mathf.Max(1, def.Cost);
			usedOffsets.Add(offset);
		}

		return spawned;
	}

	private void EnqueueSpawn(PackedScene scene, Vector2 position)
	{
		if (scene == null)
			return;

		_pendingSpawns.Enqueue((scene, position));
	}

	private void TrySpawnPending(float dt, int maxAlive)
	{
		if (_pendingSpawns.Count == 0)
			return;

		_spawnStepTimer -= dt;
		if (_spawnStepTimer > 0f)
			return;
		if (_enemiesRoot.GetChildCount() >= maxAlive)
			return;

		var item = _pendingSpawns.Dequeue();
		SpawnEnemyAt(item.scene, item.pos);
		_spawnStepTimer = GetSpawnStepInterval();
	}

	private float GetSpawnStepInterval()
	{
		float min = Mathf.Max(0.01f, SpawnStepIntervalMinSeconds);
		float max = Mathf.Max(min, SpawnStepIntervalMaxSeconds);
		float baseInterval = _rng.RandfRange(min, max);

		float phaseMult = GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableSpawnStepMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalySpawnStepMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureSpawnStepMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalSpawnStepMultiplier,
			_ => 1f
		};

		float interval = baseInterval * Mathf.Max(0.1f, phaseMult);
		interval *= GetOpeningSpawnIntervalMultiplier();
		return Mathf.Max(0.01f, interval);
	}

	private bool SpawnEnemyAt(PackedScene scene, Vector2 position)
	{
		if (scene == null)
			return false;

		if (scene.Instantiate() is not Node2D enemy)
		{
			DebugSystem.Error("[SpawnSystem] Spawn scene root must inherit Node2D.");
			return false;
		}

		enemy.GlobalPosition = position;
		_enemiesRoot.AddChild(enemy);
		return true;
	}

	private int RollWaveBudget(int aliveCount, int maxAlive)
	{
		int minBudget = Mathf.Max(1, Mathf.RoundToInt(GetPhaseBudget(_baseBudgetMin)));
		int maxBudget = Mathf.Max(minBudget, Mathf.RoundToInt(GetPhaseBudget(_baseBudgetMax)));
		int waveBudget = _rng.RandiRange(minBudget, maxBudget);

		int targetAlive = Mathf.Clamp(Mathf.RoundToInt(maxAlive * Mathf.Clamp(HordeTargetAliveRatio, 0.2f, 1f)), 1, maxAlive);
		int deficit = Mathf.Max(0, targetAlive - aliveCount);
		if (deficit > 0)
		{
			int catchUp = Mathf.RoundToInt(deficit * Mathf.Max(0f, HordeCatchUpBudgetFactor));
			waveBudget += catchUp;
		}

		return Mathf.Max(1, waveBudget);
	}

	private List<Vector2> BuildPackCenters(int packs)
	{
		var centers = new List<Vector2>(Mathf.Max(1, packs));
		if (packs <= 0)
			return centers;

		if (!UseEncirclementPackLayout || packs == 1)
		{
			for (int i = 0; i < packs; i++)
				centers.Add(GetSpawnPositionAroundPlayer());
			return centers;
		}

		GetSpawnRadiusRange(out float radiusMin, out float radiusMax);
		float centerMin = Mathf.Max(radiusMin * Mathf.Clamp(PackCenterRadiusBias, 0.35f, 1.8f), 1f);
		float centerMax = Mathf.Max(centerMin + 1f, radiusMax);
		float baseAngle = _rng.RandfRange(0f, Mathf.Tau);
		float jitterRadians = Mathf.DegToRad(Mathf.Max(0f, PackAngleJitterDegrees));
		float spreadRadians = Mathf.DegToRad(Mathf.Clamp(PackInterceptSpreadDegrees, 10f, 170f));
		Vector2 anchor = _player.GlobalPosition;
		bool hasMotionIntercept = false;
		Vector2 moveDir = Vector2.Right;

		if (UsePlayerPathInterceptCenters && _player is CharacterBody2D movingPlayer)
		{
			Vector2 v = movingPlayer.Velocity;
			float speed = v.Length();
			if (speed >= Mathf.Max(1f, InterceptVelocityThreshold))
			{
				hasMotionIntercept = true;
				moveDir = v / speed;
				baseAngle = moveDir.Angle();
				float leadDistance = Mathf.Clamp(speed * Mathf.Max(0f, InterceptLeadSeconds), 35f, 260f);
				anchor += moveDir * leadDistance * Mathf.Clamp(InterceptForwardBias, 0f, 1f);
			}
		}

		float[] interceptPattern =
		{
			0f,
			spreadRadians,
			-spreadRadians,
			spreadRadians * 1.95f,
			-spreadRadians * 1.95f,
			Mathf.Pi
		};

		for (int i = 0; i < packs; i++)
		{
			float slotAngle = hasMotionIntercept
				? baseAngle + interceptPattern[i % interceptPattern.Length]
				: baseAngle + (Mathf.Tau * i / packs);
			float jitter = _rng.RandfRange(-jitterRadians, jitterRadians);
			float angle = slotAngle + jitter;
			float radius = _rng.RandfRange(centerMin, centerMax);
			if (hasMotionIntercept)
			{
				float dot = Mathf.Cos(angle - baseAngle);
				if (dot > 0.35f)
					radius *= 0.84f; // Forward packs cut off player path tighter.
				else if (dot < -0.45f)
					radius *= 1.08f; // Rear packs keep escape-closing pressure.
			}

			Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			Vector2 candidate = anchor + dir * radius;
			Vector2 fromPlayer = candidate - _player.GlobalPosition;
			float distToPlayer = fromPlayer.Length();
			if (distToPlayer < radiusMin)
			{
				Vector2 safeDir = distToPlayer > 0.001f ? fromPlayer / distToPlayer : dir;
				candidate = _player.GlobalPosition + safeDir * radiusMin;
			}

			centers.Add(candidate);
		}

		return centers;
	}

	private Vector2 GetSpawnPositionAroundPlayer()
	{
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		GetSpawnRadiusRange(out float radiusMin, out float radiusMax);
		float radius = _rng.RandfRange(radiusMin, radiusMax);

		Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		return _player.GlobalPosition + offset;
	}

	private void GetSpawnRadiusRange(out float radiusMin, out float radiusMax)
	{
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		var camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float minVisibleRadius = Mathf.Max(halfVisible.X, halfVisible.Y) + Mathf.Max(0f, SpawnOutsideViewportMargin);
		radiusMin = Mathf.Max(_activeSpawnRadiusMin, minVisibleRadius);
		radiusMax = Mathf.Max(radiusMin + 1f, Mathf.Max(_activeSpawnRadiusMax, radiusMin + Mathf.Max(1f, SpawnRingThickness)));
	}

	private bool TryFindPackOffset(List<Vector2> usedOffsets, out Vector2 offset)
	{
		float radiusMax = Mathf.Max(1f, PackScatterRadius);
		float minSpacing = Mathf.Max(1f, PackMinSpacing);
		int attempts = Mathf.Max(1, PackPlacementAttempts);

		for (int attempt = 0; attempt < attempts; attempt++)
		{
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			float radius = _rng.RandfRange(0f, radiusMax);
			Vector2 candidate = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

			bool overlap = false;
			for (int i = 0; i < usedOffsets.Count; i++)
			{
				if (candidate.DistanceTo(usedOffsets[i]) < minSpacing)
				{
					overlap = true;
					break;
				}
			}

			if (!overlap)
			{
				offset = candidate;
				return true;
			}
		}

		offset = Vector2.Zero;
		return false;
	}
}
