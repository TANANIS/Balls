using Godot;
using System;
using System.Collections.Generic;

public sealed partial class EventRunner
{
	private const string HybridIceSpaceGlacialHorizon = "HYB_ICE_SPACE_GLACIAL_HORIZON";
	private const string HybridSpaceWarWarpAssault = "HYB_SPACE_WAR_WARP_ASSAULT";

	private readonly Node _owner;
	private readonly RandomNumberGenerator _rng = new();

	private SpawnSystem _spawnSystem;
	private CombatSystem _combatSystem;
	private Node2D _player;
	private Node _enemiesRoot;

	private EventLoadoutSlot _activeSlot;
	private readonly List<RuntimeZone> _iceZones = new();
	private readonly List<PulseRingState> _pulseRings = new();
	private readonly List<GravityWellState> _gravityWells = new();
	private readonly HashSet<ulong> _berserkEnemyIds = new();
	private readonly Dictionary<ulong, float> _enemySlowTimers = new();
	private readonly List<ulong> _enemySlowKeyBuffer = new();
	private readonly List<ulong> _enemySlowGcBuffer = new();

	private float _iceZoneSpawnTimer;
	private float _pulseEmitTimer;
	private float _gravitySpawnTimer;
	private float _berserkRetagTimer;
	private float _playerSlowTimer;
	private bool _berserkExplosionOnDeath;
	private bool _subscribedCombatKilled;
	private Vector2 _eventHorizonCenter;
	private Vector2 _eventHorizonVelocity;
	private float _eventHorizonRadius;
	private Vector2 _bloodTideDirection = Vector2.Right;

	private sealed class RuntimeZone
	{
		public Vector2 Center;
		public float Radius;
		public float Remaining;
	}

	private sealed class PulseRingState
	{
		public Vector2 Center;
		public float Radius;
		public float Speed;
		public float Thickness;
		public float MaxRadius;
		public readonly HashSet<ulong> HitActorIds = new();
	}

	private sealed class GravityWellState
	{
		public Vector2 Center;
		public float Radius;
		public float Strength;
		public float Remaining;
	}

	public EventRunner(Node owner)
	{
		_owner = owner;
	}

	public bool IsRunning => _activeSlot != null;
	public string ActiveEventId => _activeSlot?.EventId ?? string.Empty;

	public void BeginEvent(EventLoadoutSlot slot)
	{
		EndEvent();
		if (slot == null || string.IsNullOrWhiteSpace(slot.EventId))
			return;

		_activeSlot = slot.Clone();
		_rng.Seed = HashSeed(slot.EventId, slot.SlotIndex);
		ResolveRuntimeRefs();

		switch (_activeSlot.EventId)
		{
			case "EVT_ICE_ICESTORM":
				_iceZoneSpawnTimer = 0f;
				break;
			case "EVT_ICE_FROZEN_PULSE":
				_pulseEmitTimer = 0.30f;
				break;
			case "EVT_WAR_BLOOD_TIDE":
				_bloodTideDirection = PickCardinalDirection();
				SpawnBloodTideRush();
				_pulseEmitTimer = 3.8f;
				break;
			case "EVT_WAR_BERSERK_MARK":
				TagBerserkTargets();
				_berserkRetagTimer = 4.2f;
				_berserkExplosionOnDeath = IsD1();
				BindCombatKillEventIfNeeded();
				break;
			case "EVT_SPACE_EVENT_HORIZON":
				_eventHorizonCenter = PickPointNearPlayer(60f, 220f);
				_eventHorizonRadius = Mathf.Lerp(180f, 260f, Mathf.Clamp(_activeSlot.ResolvedTierIndex / 3f, 0f, 1f));
				if (IsHybridIceSpace())
					_eventHorizonRadius *= 1.12f;
				if (IsD1() || IsHybridSpaceWar())
				{
					float horizonSpeed = IsHybridSpaceWar() ? 54f : 44f;
					_eventHorizonVelocity = PickCardinalDirection().Rotated(_rng.RandfRange(-0.35f, 0.35f)) * horizonSpeed;
				}
				break;
			case "EVT_SPACE_GRAVITY_WELL":
				_gravitySpawnTimer = 0.15f;
				break;
		}
	}

	public void EndEvent()
	{
		UnbindCombatKillEvent();
		_activeSlot = null;
		_iceZones.Clear();
		_pulseRings.Clear();
		_gravityWells.Clear();
		_berserkEnemyIds.Clear();
		_enemySlowTimers.Clear();
		_playerSlowTimer = 0f;
		_berserkExplosionOnDeath = false;
		_iceZoneSpawnTimer = 0f;
		_pulseEmitTimer = 0f;
		_gravitySpawnTimer = 0f;
		_berserkRetagTimer = 0f;
		_eventHorizonCenter = Vector2.Zero;
		_eventHorizonVelocity = Vector2.Zero;
		_eventHorizonRadius = 0f;
	}

	public void Tick(float dt)
	{
		if (!IsRunning || dt <= 0f)
			return;

		ResolveRuntimeRefs();
		UpdateSlowTimers(dt);
		UpdateIceZones(dt);
		UpdatePulseRings(dt);
		UpdateGravityWells(dt);

		switch (_activeSlot.EventId)
		{
			case "EVT_ICE_ICESTORM":
				_iceZoneSpawnTimer -= dt;
				if (_iceZoneSpawnTimer <= 0f)
				{
					SpawnIceZone();
					_iceZoneSpawnTimer = IsD1() ? 2.2f : 3.0f;
				}
				break;
			case "EVT_ICE_FROZEN_PULSE":
				_pulseEmitTimer -= dt;
				if (_pulseEmitTimer <= 0f)
				{
					EmitPulseRing();
					_pulseEmitTimer = IsD1() ? 4.2f : 6.0f;
				}
				break;
			case "EVT_WAR_BLOOD_TIDE":
				_pulseEmitTimer -= dt;
				if (_pulseEmitTimer <= 0f)
				{
					SpawnBloodTideRush();
					_pulseEmitTimer = 4.5f;
				}
				break;
			case "EVT_WAR_BERSERK_MARK":
				_berserkRetagTimer -= dt;
				if (_berserkRetagTimer <= 0f)
				{
					TagBerserkTargets();
					_berserkRetagTimer = 4.2f;
				}
				break;
			case "EVT_SPACE_EVENT_HORIZON":
				if ((IsD1() || IsHybridSpaceWar()) && _eventHorizonVelocity.LengthSquared() > 0f)
					MoveEventHorizon(dt);
				break;
			case "EVT_SPACE_GRAVITY_WELL":
				_gravitySpawnTimer -= dt;
				if (_gravitySpawnTimer <= 0f)
				{
					SpawnGravityWells();
					_gravitySpawnTimer = 4.8f;
				}
				break;
		}
	}

	public float GetMoveMultiplierAt(Vector2 worldPos, ulong actorInstanceId, bool isPlayer)
	{
		if (!IsRunning)
			return 1f;

		float mult = 1f;
		if (_activeSlot.EventId == "EVT_ICE_ICESTORM" && IsInsideZones(worldPos, _iceZones))
		{
			float iceMult = IsD1() ? 0.68f : 0.75f;
			if (IsHybridIceSpace())
				iceMult *= 0.92f;
			mult *= iceMult;
		}
		if (_activeSlot.EventId == "EVT_SPACE_EVENT_HORIZON" && IsInsideEventHorizon(worldPos))
		{
			mult *= 0.72f;
			if (IsHybridIceSpace())
				mult *= 0.90f;
		}
		if (_activeSlot.EventId == "EVT_WAR_BLOOD_TIDE" && isPlayer)
			mult *= IsHybridSpaceWar() ? 1.14f : 1.10f;
		if (_activeSlot.EventId == "EVT_WAR_BERSERK_MARK")
		{
			if (isPlayer)
				mult *= IsHybridSpaceWar() ? 1.18f : 1.14f;
			else if (_berserkEnemyIds.Contains(actorInstanceId))
				mult *= IsHybridSpaceWar() ? 1.34f : 1.28f;
		}

		if (isPlayer && _playerSlowTimer > 0f)
			mult *= 0.76f;
		if (!isPlayer && _enemySlowTimers.TryGetValue(actorInstanceId, out float slow) && slow > 0f)
			mult *= 0.76f;

		return Mathf.Clamp(mult, 0.35f, 1.80f);
	}

	public float GetProjectileSpeedMultiplierAt(Vector2 worldPos)
	{
		if (!IsRunning)
			return 1f;
		if (_activeSlot.EventId != "EVT_SPACE_EVENT_HORIZON" || !IsInsideEventHorizon(worldPos))
			return 1f;
		float mult = 0.72f;
		if (IsHybridIceSpace())
			mult *= 0.90f;
		return mult;
	}

	public float GetPlayerRangeMultiplierAt(Vector2 worldPos)
	{
		if (!IsRunning)
			return 1f;
		if (_activeSlot.EventId != "EVT_SPACE_EVENT_HORIZON" || !IsInsideEventHorizon(worldPos))
			return 1f;
		float mult = 0.72f;
		if (IsHybridIceSpace())
			mult *= 0.90f;
		return mult;
	}

	public Vector2 GetExternalVelocityAt(Vector2 worldPos)
	{
		if (!IsRunning || _activeSlot.EventId != "EVT_SPACE_GRAVITY_WELL" || _gravityWells.Count == 0)
			return Vector2.Zero;

		Vector2 pull = Vector2.Zero;
		foreach (GravityWellState well in _gravityWells)
		{
			Vector2 toWell = well.Center - worldPos;
			float dist = toWell.Length();
			if (dist <= 0.001f || dist > well.Radius)
				continue;
			float t = 1f - (dist / well.Radius);
			pull += (toWell / dist) * (well.Strength * t);
		}

		float cap = IsHybridSpaceWar() ? 300f : 260f;
		return pull.LimitLength(cap);
	}

	private void ResolveRuntimeRefs()
	{
		_spawnSystem = GroupServiceResolver.ResolveFirstInGroup(_owner, RuntimeGroups.SpawnSystem, _spawnSystem);
		_combatSystem = GroupServiceResolver.ResolveFirstInGroup(_owner, RuntimeGroups.CombatSystem, _combatSystem);
		if (!GodotObject.IsInstanceValid(_player))
			_player = _owner.GetNodeOrNull<Node2D>("../../Player");
		if (!GodotObject.IsInstanceValid(_enemiesRoot))
			_enemiesRoot = _owner.GetNodeOrNull<Node>("../../Enemies");
	}

	private void UpdateSlowTimers(float dt)
	{
		_playerSlowTimer = Mathf.Max(0f, _playerSlowTimer - dt);

		_enemySlowKeyBuffer.Clear();
		foreach (ulong key in _enemySlowTimers.Keys)
			_enemySlowKeyBuffer.Add(key);
		_enemySlowGcBuffer.Clear();
		foreach (ulong key in _enemySlowKeyBuffer)
		{
			float next = Mathf.Max(0f, _enemySlowTimers[key] - dt);
			if (next <= 0f)
				_enemySlowGcBuffer.Add(key);
			else
				_enemySlowTimers[key] = next;
		}
		foreach (ulong key in _enemySlowGcBuffer)
			_enemySlowTimers.Remove(key);
	}

	private bool IsD1()
	{
		return string.Equals(_activeSlot?.DistortionLevel, "D1", StringComparison.OrdinalIgnoreCase);
	}

	private bool IsHybridActive()
	{
		return _activeSlot?.HybridVariantTriggered == true && !string.IsNullOrWhiteSpace(_activeSlot.HybridVariantId);
	}

	private bool IsHybridIceSpace()
	{
		return IsHybridActive() && string.Equals(_activeSlot.HybridVariantId, HybridIceSpaceGlacialHorizon, StringComparison.Ordinal);
	}

	private bool IsHybridSpaceWar()
	{
		return IsHybridActive() && string.Equals(_activeSlot.HybridVariantId, HybridSpaceWarWarpAssault, StringComparison.Ordinal);
	}
}
