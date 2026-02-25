using Godot;

public partial class Bullet
{
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

		bool builtElementalBurstFrames = _isElementalBurstShot && TryBuildElementalBurstFrames(frames);
		if (!builtElementalBurstFrames && EffectFrames != null && EffectFrames.Count > 0)
		{
			for (int i = 0; i < EffectFrames.Count; i++)
			{
				Texture2D texture = EffectFrames[i];
				if (texture == null)
					continue;
				frames.AddFrame("default", texture);
			}
		}
		else if (!builtElementalBurstFrames && EffectTexture != null)
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

	private static bool TryBuildElementalBurstFrames(SpriteFrames frames)
	{
		if (frames == null)
			return false;

		bool addedAny = false;
		for (int i = 0; i < ElementalBurstFramePaths.Length; i++)
		{
			string path = ElementalBurstFramePaths[i];
			if (!ResourceLoader.Exists(path))
				continue;

			Texture2D texture = GD.Load<Texture2D>(path);
			if (texture == null)
				continue;

			frames.AddFrame("default", texture);
			addedAny = true;
		}

		return addedAny;
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

		if (_isElementalBurstShot && _runtimeFrameCount >= 2)
		{
			prepareStart = 0;
			prepareEnd = 0;
			flightStart = 0;
			flightEnd = Mathf.Max(0, maxFrame - 1);
			impactStart = maxFrame;
			impactEnd = maxFrame;
		}

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
}
