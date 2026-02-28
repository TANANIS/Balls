using Godot;
using System.Collections.Generic;
/*
 * Bullet sensor:
 * - Moves forward for a limited lifetime.
 * - On first valid hit, submits DamageRequest to CombatSystem.
 * - Never applies damage directly.
 */
public partial class Bullet : Area2D
{
	private const string DefaultProjectileTexturePath = "res://Assets/Sprites/Projectiles/Common/projectile_orb_default.png";
	private const string ElementalBurstFramesBasePath = "res://Assets/Sprites/Projectiles/ElementalBurst/Projectile";
	private const string ElementalBurstExplosionRunePath = "res://Assets/Sprites/Projectiles/ElementalBurst/Explosion/elemental_burst_explosion_01.png";
	private const string ElementalBurstExplosionFramesBasePath = "res://Assets/Sprites/Projectiles/ElementalBurst/Explosion";

	[Export] public float LifeTime = 1.2f;
	[Export] public bool DespawnOutsideViewport = true;
	[Export(PropertyHint.Range, "0,1024,1")] public float DespawnOutsideViewportMargin = 64f;
	[Export] public string DamageTag = "bullet";
	[Export(PropertyHint.Range, "0.10,3.00,0.05")] public float RuntimeSpeedScale = 1.00f;
	[Export(PropertyHint.Range, "0.00,0.20,0.005")] public float SplitChildHitArmDelaySeconds = 0.05f;
	[Export] public bool RotateToDirection = true;
	[Export(PropertyHint.Range, "-180,180,1")] public float RotationOffsetDegrees = 0f;
	[ExportGroup("Homing")]
	[Export] public bool HomingEnabled = false;
	[Export(PropertyHint.Range, "60,1440,1")] public float HomingTurnRateDegrees = 680f;
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float HomingForwardDotThreshold = 0.15f;
	[ExportGroup("Penetration / Ricochet")]
	[Export(PropertyHint.Range, "0,6,1")] public int DefaultPierceCount = 0;
	[Export(PropertyHint.Range, "0,6,1")] public int DefaultRicochetCount = 0;
	[Export(PropertyHint.Range, "64,2400,1")] public float RicochetSearchRadius = 640f;
	[Export(PropertyHint.Range, "0.05,1.00,0.01")] public float PierceDamageMultiplierPerHit = 0.60f;
	[Export(PropertyHint.Range, "0.05,1.00,0.01")] public float RicochetDamageMultiplierPerBounce = 0.50f;
	[Export(PropertyHint.Range, "0.01,0.30,0.005")] public float PostHitRetargetDelaySeconds = 0.05f;
	[Export(PropertyHint.Range, "0,120,1")] public float PostHitForwardOffset = 10f;
	[ExportGroup("Effect")]
	[Export] public Texture2D EffectTexture;
	[Export] public Godot.Collections.Array<Texture2D> EffectFrames = new();
	[Export(PropertyHint.Range, "1,32,1")] public int TotalFrames = 10;
	[Export] public bool EnablePreparePhase = true;
	[Export(PropertyHint.Range, "0,31,1")] public int PrepareStartFrame = 0;
	[Export(PropertyHint.Range, "0,31,1")] public int PrepareEndFrame = 1;
	[Export(PropertyHint.Range, "0,31,1")] public int FlightStartFrame = 2;
	[Export(PropertyHint.Range, "0,31,1")] public int FlightEndFrame = 4;
	[Export(PropertyHint.Range, "0,31,1")] public int ImpactStartFrame = 5;
	[Export(PropertyHint.Range, "0,31,1")] public int ImpactEndFrame = 9;
	[Export(PropertyHint.Range, "1,60,1")] public float EffectFps = 18f;
	[ExportGroup("Split Shot")]
	[Export(PropertyHint.Range, "0.01,1.0,0.01")] public float SplitChildDamageMultiplier = 0.50f;
	[Export(PropertyHint.Range, "0.25,2.0,0.05")] public float SplitChildSpeedMultiplier = 1.00f;
	[Export(PropertyHint.Range, "1,80,1")] public float SplitBaseAngleDegrees = 18f;
	[Export(PropertyHint.Range, "0,40,1")] public float SplitAngleStepDegrees = 10f;
	[Export(PropertyHint.Range, "0,200,1")] public float SplitSpawnForwardOffset = 30f;
	[Export] public PackedScene SplitChildProjectileScene;
	[ExportGroup("Elemental Burst")]
	[Export(PropertyHint.Range, "16,400,1")] public float ElementalBurstExplosionRadius = 130f;
	[Export(PropertyHint.Range, "0.10,3.00,0.05")] public float ElementalBurstDamageMultiplier = 1.20f;
	[Export(PropertyHint.Range, "32,2000,1")] public float ElementalBurstMaxDistance = 280f;
	[Export(PropertyHint.Range, "1,32,1")] public int ElementalBurstMaxTargets = 5;
	[Export(PropertyHint.Range, "0.05,2.0,0.01")] public float ElementalBurstRuneDurationSeconds = 0.65f;
	[Export(PropertyHint.Range, "1,30,1")] public float ElementalBurstExplosionVfxFps = 9f;
	[Export(PropertyHint.Range, "0.1,1.0,0.05")] public float ElementalBurstRuneStartAlpha = 0.92f;
	[Export(PropertyHint.Range, "1.0,2.0,0.01")] public float ElementalBurstRuneScaleExpand = 1.12f;

	private Vector2 _dir = Vector2.Right;
	private float _speed = 900f;
	private int _damage = 1;
	private float _damageScale = 1f;
	private float _hitArmDelayTimer = 0f;
	private float _ignoreTargetTimer = 0f;
	private ulong _ignoreTargetInstanceId = 0;
	private Node _source;
	private PackedScene _projectileScene;
	private float _lifeTimer = 0f;
	private bool _hasHit = false;
	private bool _impactStarted = false;
	private bool _prepareFinished = true;
	private bool _canSplitOnHit = true;
	private int _splitShotLevel = 0;
	private bool _homingEnabledRuntime = false;
	private float _homingTurnRateRuntime = 0f;
	private Node2D _homingTarget;
	private int _pierceRemaining = 0;
	private int _ricochetRemaining = 0;
	private float _ricochetSearchRadiusRuntime = 0f;
	private readonly HashSet<ulong> _hitTargetIds = new();
	private bool _isElementalBurstShot = false;
	private bool _elementalBurstDetonated = false;
	private float _elementalBurstRadiusRuntime = 0f;
	private float _elementalBurstDamageMultiplierRuntime = 1f;
	private float _elementalBurstMaxDistanceRuntime = 0f;
	private int _elementalBurstMaxTargetsRuntime = 1;
	private Node _elementalBurstOwner;
	private float _travelDistance = 0f;
	private static readonly string[] ElementalBurstFramePaths =
	{
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_01.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_02.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_03.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_04.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_05.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_06.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_07.png",
		$"{ElementalBurstFramesBasePath}/elemental_burst_charge_08.png"
	};
	private static readonly string[] ElementalBurstExplosionFramePaths =
	{
		$"{ElementalBurstExplosionFramesBasePath}/elemental_burst_explosion_01.png",
		$"{ElementalBurstExplosionFramesBasePath}/elemental_burst_explosion_02.png",
		$"{ElementalBurstExplosionFramesBasePath}/elemental_burst_explosion_03.png",
		$"{ElementalBurstExplosionFramesBasePath}/elemental_burst_explosion_04.png",
		$"{ElementalBurstExplosionFramesBasePath}/elemental_burst_explosion_05.png"
	};
	private CombatSystem _combat;
	private AnimatedSprite2D _fx;
	private float _frameTimer = 0f;
	private int _currentFrame = 0;
	private int _runtimeFrameCount = 0;

	public void InitFromPlayer(Node source, Vector2 dir, float speed, int damage)
	{
		InitFromPlayer(
			source,
			dir,
			speed,
			damage,
			splitShotLevel: 0,
			canSplitOnHit: true,
			projectileScene: null,
			damageScale: 1f,
			hitArmDelaySeconds: 0f,
			ignoreTargetInstanceId: 0,
			ignoreTargetSeconds: 0f,
			isElementalBurstShot: false,
			elementalBurstRadius: 0f,
			elementalBurstDamageMultiplier: 1f,
			elementalBurstMaxDistance: 0f,
			elementalBurstMaxTargets: 0,
			elementalBurstOwner: null,
			homingTarget: null,
			homingTurnRateDegrees: 0f,
			pierceCount: 0,
			ricochetCount: 0,
			ricochetSearchRadius: 0f);
	}

	public void InitFromPlayer(
		Node source,
		Vector2 dir,
		float speed,
		int damage,
		int splitShotLevel,
		bool canSplitOnHit,
		PackedScene projectileScene,
		float damageScale = 1f,
		float hitArmDelaySeconds = 0f,
		ulong ignoreTargetInstanceId = 0,
		float ignoreTargetSeconds = 0f,
		bool isElementalBurstShot = false,
		float elementalBurstRadius = 0f,
		float elementalBurstDamageMultiplier = 1f,
		float elementalBurstMaxDistance = 0f,
		int elementalBurstMaxTargets = 0,
		Node elementalBurstOwner = null,
		Node2D homingTarget = null,
		float homingTurnRateDegrees = 0f,
		int pierceCount = 0,
		int ricochetCount = 0,
		float ricochetSearchRadius = 0f)
	{
		_source = source;
		_dir = dir == Vector2.Zero ? Vector2.Right : dir.Normalized();
		float runtimeScale = Mathf.Max(0.01f, RuntimeSpeedScale);
		_speed = Mathf.Max(50f, speed * runtimeScale);
		_damage = damage;
		_damageScale = Mathf.Clamp(damageScale, 0f, 1f);
		_hitArmDelayTimer = Mathf.Max(0f, hitArmDelaySeconds);
		_ignoreTargetInstanceId = ignoreTargetInstanceId;
		_ignoreTargetTimer = Mathf.Max(0f, ignoreTargetSeconds);
		_splitShotLevel = Mathf.Clamp(splitShotLevel, 0, 4);
		_canSplitOnHit = canSplitOnHit;
		_projectileScene = projectileScene;
		_isElementalBurstShot = isElementalBurstShot;
		_elementalBurstDetonated = false;
		_elementalBurstRadiusRuntime = isElementalBurstShot
			? Mathf.Max(1f, elementalBurstRadius > 0f ? elementalBurstRadius : ElementalBurstExplosionRadius)
			: 0f;
		_elementalBurstDamageMultiplierRuntime = isElementalBurstShot
			? Mathf.Clamp(elementalBurstDamageMultiplier > 0f ? elementalBurstDamageMultiplier : ElementalBurstDamageMultiplier, 0.01f, 10f)
			: 1f;
		_elementalBurstMaxDistanceRuntime = isElementalBurstShot
			? Mathf.Max(1f, elementalBurstMaxDistance > 0f ? elementalBurstMaxDistance : ElementalBurstMaxDistance)
			: 0f;
		_elementalBurstMaxTargetsRuntime = isElementalBurstShot
			? Mathf.Max(1, elementalBurstMaxTargets > 0 ? elementalBurstMaxTargets : ElementalBurstMaxTargets)
			: 1;
		_elementalBurstOwner = isElementalBurstShot ? elementalBurstOwner : null;
		_homingTarget = homingTarget;
		_homingTurnRateRuntime = homingTurnRateDegrees > 0f ? homingTurnRateDegrees : HomingTurnRateDegrees;
		_homingEnabledRuntime = HomingEnabled || homingTurnRateDegrees > 0f;
		_pierceRemaining = Mathf.Max(0, Mathf.Max(DefaultPierceCount, pierceCount));
		_ricochetRemaining = Mathf.Max(0, Mathf.Max(DefaultRicochetCount, ricochetCount));
		_ricochetSearchRadiusRuntime = ricochetSearchRadius > 0f
			? ricochetSearchRadius
			: Mathf.Max(64f, RicochetSearchRadius);
		_hitTargetIds.Clear();
		_travelDistance = 0f;
		ApplyFacingByDirection();
	}

	public override void _Ready()
	{
		_travelDistance = 0f;
		TryResolveCombatSystem();
		ResolveEffect();
		BuildEffectFramesIfNeeded();
		if (_fx?.SpriteFrames != null && _fx.SpriteFrames.HasAnimation("default") && _fx.SpriteFrames.GetFrameCount("default") > 0)
		{
			_prepareFinished = !EnablePreparePhase;
			_currentFrame = _prepareFinished ? FlightStartFrame : PrepareStartFrame;
			ApplyFrame(_currentFrame);
		}

		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		_lifeTimer += dt;
		if (_lifeTimer >= LifeTime)
		{
			if (_isElementalBurstShot && !_elementalBurstDetonated)
			{
				TryTriggerElementalBurstExplosion(null);
				_hasHit = true;
				BeginImpact();
				return;
			}

			QueueFree();
			return;
		}
		if (_hitArmDelayTimer > 0f)
			_hitArmDelayTimer -= dt;
		if (_ignoreTargetTimer > 0f)
			_ignoreTargetTimer -= dt;

		UpdateEffect(dt);

		if (!_impactStarted && _prepareFinished)
		{
			UpdateHomingDirection(dt);
			Vector2 step = _dir * _speed * dt;
			GlobalPosition += step;
			_travelDistance += step.Length();
			ApplyFacingByDirection();
			if (DespawnOutsideViewport && IsOutsideActiveCameraViewport())
			{
				QueueFree();
				return;
			}

			if (_isElementalBurstShot && !_elementalBurstDetonated && _travelDistance >= _elementalBurstMaxDistanceRuntime)
			{
				TryTriggerElementalBurstExplosion(null);
				_hasHit = true;
				BeginImpact();
			}
		}
	}

	private void TryResolveCombatSystem()
	{
		if (_combat != null)
			return;

		var list = GetTree().GetNodesInGroup(RuntimeGroups.CombatSystem);
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;
	}

	private void BeginImpact()
	{
		if (_impactStarted)
			return;
		_impactStarted = true;
		SetDeferred("monitoring", false);
	}

	private void ApplyFacingByDirection()
	{
		if (!RotateToDirection)
			return;
		float baseAngle = _dir.Angle();
		Rotation = baseAngle + Mathf.DegToRad(RotationOffsetDegrees);
	}
}
