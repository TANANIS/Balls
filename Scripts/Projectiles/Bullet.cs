using Godot;

/*
 * Bullet sensor:
 * - Moves forward for a limited lifetime.
 * - On first valid hit, submits DamageRequest to CombatSystem.
 * - Never applies damage directly.
 */
public partial class Bullet : Area2D
{
	[Export] public float LifeTime = 1.5f;
	[Export] public string DamageTag = "bullet";
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

	private Vector2 _dir = Vector2.Right;
	private float _speed = 900f;
	private int _damage = 1;
	private Node _source;
	private float _lifeTimer = 0f;
	private bool _hasHit = false;
	private bool _impactStarted = false;
	private bool _prepareFinished = true;
	private CombatSystem _combat;
	private AnimatedSprite2D _fx;
	private float _frameTimer = 0f;
	private int _currentFrame = 0;
	private int _runtimeFrameCount = 0;

	public void InitFromPlayer(Node source, Vector2 dir, float speed, int damage)
	{
		_source = source;
		_dir = dir == Vector2.Zero ? Vector2.Right : dir.Normalized();
		_speed = speed;
		_damage = damage;
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

		_runtimeFrameCount = frames.GetFrameCount("default");
		if (_runtimeFrameCount <= 0)
			return;
		_fx.SpriteFrames = frames;
		_fx.Animation = "default";
		_fx.Play();
		_fx.Stop();
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

	private void ApplyFacingByDirection()
	{
		if (!RotateToDirection)
			return;
		float baseAngle = _dir.Angle();
		Rotation = baseAngle + Mathf.DegToRad(RotationOffsetDegrees);
	}
}
