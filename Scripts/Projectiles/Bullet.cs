using Godot;

/*
 * Bullet sensor:
 * - Moves forward for a limited lifetime.
 * - On first valid hit, submits DamageRequest to CombatSystem.
 * - Never applies damage directly.
 */
public partial class Bullet : Area2D
{
	private const string DefaultSplitProjectileScenePath = "res://Prefabs/SplitProjectile.tscn";
	private const string SplitProjectileTexturePath = "res://Assets/Sprites/Projectiles/Split/split_bullet.png";
	private const string DefaultProjectileTexturePath = "res://Assets/Sprites/player_orb_transparent.png";

	[Export] public float LifeTime = 1.5f;
	[Export] public string DamageTag = "bullet";
	[Export(PropertyHint.Range, "0.10,3.00,0.05")] public float RuntimeSpeedScale = 1.00f;
	[Export(PropertyHint.Range, "0.00,0.20,0.005")] public float SplitChildHitArmDelaySeconds = 0.05f;
	[Export] public bool RotateToDirection = true;
	[Export(PropertyHint.Range, "-180,180,1")] public float RotationOffsetDegrees = 0f;
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
			ignoreTargetSeconds: 0f);
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
		float ignoreTargetSeconds = 0f)
	{
		_source = source;
		_dir = dir == Vector2.Zero ? Vector2.Right : dir.Normalized();
		_speed = speed * Mathf.Max(0.01f, RuntimeSpeedScale);
		_damage = damage;
		_damageScale = Mathf.Clamp(damageScale, 0f, 1f);
		_hitArmDelayTimer = Mathf.Max(0f, hitArmDelaySeconds);
		_ignoreTargetInstanceId = ignoreTargetInstanceId;
		_ignoreTargetTimer = Mathf.Max(0f, ignoreTargetSeconds);
		_splitShotLevel = Mathf.Clamp(splitShotLevel, 0, 4);
		_canSplitOnHit = canSplitOnHit;
		_projectileScene = projectileScene;
		ApplyFacingByDirection();
	}

	public override void _Ready()
	{
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
			GlobalPosition += _dir * _speed * dt;
			ApplyFacingByDirection();
		}
	}

	private void TryResolveCombatSystem()
	{
		if (_combat != null)
			return;

		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;
	}

	private void ResolveEffect()
	{
		_fx = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
	}

	private void BuildEffectFramesIfNeeded()
	{
		if (_fx == null)
			return;

		var frames = new SpriteFrames();
		frames.AddAnimation("default");
		frames.SetAnimationLoop("default", false);
		frames.SetAnimationSpeed("default", Mathf.Max(1f, EffectFps));

		if (EffectFrames != null && EffectFrames.Count > 0)
		{
			for (int i = 0; i < EffectFrames.Count; i++)
			{
				Texture2D texture = EffectFrames[i];
				if (texture == null)
					continue;
				frames.AddFrame("default", texture);
			}
		}
		else if (EffectTexture != null)
		{
			int frameCount = Mathf.Max(1, TotalFrames);
			float frameWidth = EffectTexture.GetWidth() / (float)frameCount;
			float frameHeight = EffectTexture.GetHeight();

			for (int i = 0; i < frameCount; i++)
			{
				var atlas = new AtlasTexture
				{
					Atlas = EffectTexture,
					Region = new Rect2(i * frameWidth, 0f, frameWidth, frameHeight)
				};
				frames.AddFrame("default", atlas);
			}
		}

		// Safety fallback: if imported textures are missing/null, ensure projectile still renders.
		if (frames.GetFrameCount("default") <= 0)
		{
			if (!TryAddSingleFrameFromPath(frames, SplitProjectileTexturePath))
				TryAddSingleFrameFromPath(frames, DefaultProjectileTexturePath);
		}

		_runtimeFrameCount = frames.GetFrameCount("default");
		if (_runtimeFrameCount <= 0)
			return;
		_fx.SpriteFrames = frames;
		_fx.Animation = "default";
		_fx.Play();
		_fx.Stop();
	}

	private static bool TryAddSingleFrameFromPath(SpriteFrames frames, string texturePath)
	{
		if (frames == null)
			return false;
		if (string.IsNullOrWhiteSpace(texturePath))
			return false;
		if (!ResourceLoader.Exists(texturePath))
			return false;

		Texture2D texture = GD.Load<Texture2D>(texturePath);
		if (texture == null)
			return false;

		frames.AddFrame("default", texture);
		return true;
	}

	private void UpdateEffect(float dt)
	{
		if (_fx == null)
			return;

		int maxFrame = Mathf.Max(0, (_runtimeFrameCount > 0 ? _runtimeFrameCount : TotalFrames) - 1);
		int prepareStart = Mathf.Clamp(PrepareStartFrame, 0, maxFrame);
		int prepareEnd = Mathf.Clamp(PrepareEndFrame, prepareStart, maxFrame);
		int minFlightStart = EnablePreparePhase ? Mathf.Min(maxFrame, prepareEnd + 1) : 0;
		int flightStart = Mathf.Clamp(FlightStartFrame, minFlightStart, maxFrame);
		int flightEnd = Mathf.Clamp(FlightEndFrame, flightStart, maxFrame);
		int impactStart = Mathf.Clamp(ImpactStartFrame, flightEnd + 1, maxFrame);
		int impactEnd = Mathf.Clamp(ImpactEndFrame, impactStart, maxFrame);

		_frameTimer += dt;
		float frameDuration = 1f / Mathf.Max(1f, EffectFps);
		while (_frameTimer >= frameDuration)
		{
			_frameTimer -= frameDuration;

			if (_impactStarted)
			{
				if (_currentFrame < impactStart)
					_currentFrame = impactStart;
				else
					_currentFrame++;

				if (_currentFrame > impactEnd)
				{
					QueueFree();
					return;
				}

				ApplyFrame(_currentFrame);
				continue;
			}

			if (!_prepareFinished)
			{
				if (_currentFrame < prepareStart)
					_currentFrame = prepareStart;
				else
					_currentFrame++;

				if (_currentFrame > prepareEnd)
				{
					_prepareFinished = true;
					_currentFrame = flightStart;
				}

				ApplyFrame(_currentFrame);
				continue;
			}

			_currentFrame++;
			if (_currentFrame > flightEnd)
				_currentFrame = flightStart;
			ApplyFrame(_currentFrame);
		}
	}

	private void ApplyFrame(int frame)
	{
		if (_fx?.SpriteFrames == null)
			return;
		int frameCount = _fx.SpriteFrames.GetFrameCount("default");
		if (frameCount <= 0)
			return;
		_fx.Frame = Mathf.Clamp(frame, 0, frameCount - 1);
	}

	private void BeginImpact()
	{
		if (_impactStarted)
			return;
		_impactStarted = true;
		SetDeferred("monitoring", false);
	}

	private void TrySpawnSplitShotsOnHit(Node hitTarget = null)
	{
		if (!_canSplitOnHit || _splitShotLevel <= 0)
			return;

		Node parent = GetParent();
		if (parent == null)
			return;

		// Prefer dedicated split projectile prefab so split visuals/motion stay deterministic.
		PackedScene scene = SplitChildProjectileScene;
		if (scene == null && ResourceLoader.Exists(DefaultSplitProjectileScenePath))
			scene = GD.Load<PackedScene>(DefaultSplitProjectileScenePath);
		if (scene == null)
			scene = _projectileScene;
		if (scene == null && !string.IsNullOrWhiteSpace(SceneFilePath))
			scene = GD.Load<PackedScene>(SceneFilePath);
		if (scene == null)
			return;

		float baseAngle = Mathf.Clamp(SplitBaseAngleDegrees, 1f, 85f);
		float stepAngle = Mathf.Clamp(SplitAngleStepDegrees, 0f, 45f);
		int level = Mathf.Max(1, _splitShotLevel);
		int splitCount = GetSplitProjectileCount(level);
		bool radial360 = splitCount >= 6;
		float childDamageFactor = Mathf.Clamp(SplitChildDamageMultiplier, 0.01f, 1f);
		float childSpeedFactor = Mathf.Max(0.1f, SplitChildSpeedMultiplier);
		int childDamage = Mathf.Max(1, _damage);
		float childDamageScale = Mathf.Clamp(_damageScale * childDamageFactor, 0.001f, 1f);
		float childSpeed = Mathf.Max(1f, _speed * childSpeedFactor);
		ulong ignoreTargetId = (ulong)(hitTarget?.GetInstanceId() ?? 0);
		Vector2 splitOrigin = (hitTarget as Node2D)?.GlobalPosition ?? GlobalPosition;
		// Push split spawn a bit past the current target to avoid instant re-hit on the same enemy.
		splitOrigin += _dir * Mathf.Max(0f, SplitSpawnForwardOffset);

		if (radial360)
		{
			float step360 = 360f / splitCount;
			for (int i = 0; i < splitCount; i++)
			{
				float angle = step360 * i;
				SpawnSplitChild(scene, parent, splitOrigin, angle, childSpeed, childDamage, childDamageScale, ignoreTargetId);
			}
		}
		else
		{
			float halfSpan = baseAngle + ((splitCount - 3) * stepAngle);
			halfSpan = Mathf.Clamp(halfSpan, 1f, 179f);
			for (int i = 0; i < splitCount; i++)
			{
				float t = splitCount <= 1 ? 0.5f : (float)i / (splitCount - 1);
				float angle = Mathf.Lerp(-halfSpan, halfSpan, t);
				SpawnSplitChild(scene, parent, splitOrigin, angle, childSpeed, childDamage, childDamageScale, ignoreTargetId);
			}
		}

		_canSplitOnHit = false;
	}

	private void SpawnSplitChild(
		PackedScene scene,
		Node parent,
		Vector2 spawnPos,
		float angleDegrees,
		float speed,
		int damage,
		float damageScale,
		ulong ignoreTargetInstanceId)
	{
		Node spawned = scene.Instantiate();
		if (spawned is Node2D child2D)
			child2D.GlobalPosition = spawnPos;

		Vector2 splitDir = _dir.Rotated(Mathf.DegToRad(angleDegrees)).Normalized();
		if (spawned is Bullet splitBullet)
		{
			// Explicitly disable chaining to avoid runaway recursive split behavior.
			splitBullet.InitFromPlayer(
				_source,
				splitDir,
				speed,
				damage,
				splitShotLevel: 0,
				canSplitOnHit: false,
				projectileScene: scene,
				damageScale: damageScale,
				hitArmDelaySeconds: 0f,
				ignoreTargetInstanceId: ignoreTargetInstanceId,
				ignoreTargetSeconds: SplitChildHitArmDelaySeconds);
		}
		else
		{
			spawned.Call("InitFromPlayer", _source, splitDir, speed, damage);
		}

		parent.AddChild(spawned);
	}

	private static int GetSplitProjectileCount(int level)
	{
		// SplitShot stack progression: Lv1=3, Lv2=4, Lv3=5, Lv4=6.
		return Mathf.Clamp(2 + level, 3, 6);
	}

	private void ApplyFacingByDirection()
	{
		if (!RotateToDirection)
			return;
		float baseAngle = _dir.Angle();
		Rotation = baseAngle + Mathf.DegToRad(RotationOffsetDegrees);
	}
}
