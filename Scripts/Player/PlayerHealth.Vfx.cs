using Godot;

public partial class PlayerHealth
{
	private void EnsureShieldVisual()
	{
		if (_shieldSprite != null || _shieldFallbackRing != null)
			return;

		Node2D anchor = ResolveSkillVfxRoot();
		if (anchor == null)
			return;

		_shieldSprite = new Sprite2D
		{
			Name = "ShieldVfx",
			Centered = true,
			TopLevel = false,
			ZAsRelative = false,
			ZIndex = ShieldZIndex,
			Visible = false,
			Texture = ShieldTexture ?? GD.Load<Texture2D>("res://Assets/Sprites/Skills/Shield/shield.png")
		};
		anchor.AddChild(_shieldSprite);
		_shieldSprite.Position = Vector2.Zero;

		if (EnableShieldFallbackRing)
			EnsureShieldFallbackRing(anchor);

		ApplyShieldVisualScale();
	}

	private Node2D ResolveSkillVfxRoot()
	{
		if (_skillVfxRoot != null && IsInstanceValid(_skillVfxRoot))
			return _skillVfxRoot;

		Player player = GetParentOrNull<Player>();
		if (player != null)
		{
			_skillVfxRoot = player.GetSkillVfxRoot();
			if (_skillVfxRoot != null)
				return _skillVfxRoot;
		}

		if (SkillVfxRootPath != null && !SkillVfxRootPath.IsEmpty)
			_skillVfxRoot = GetNodeOrNull<Node2D>(SkillVfxRootPath);

		if (_skillVfxRoot != null)
			return _skillVfxRoot;

		// Backward compatibility fallback.
		_skillVfxRoot = GetParentOrNull<Node2D>();
		if (_skillVfxRoot != null)
			return _skillVfxRoot;
		return null;
	}

	private void ApplyShieldVisualScale()
	{
		if (_shieldSprite?.Texture == null)
			return;

		Vector2 texSize = _shieldSprite.Texture.GetSize();
		float texBase = Mathf.Max(1f, Mathf.Min(texSize.X, texSize.Y));
		float targetDiameter = Mathf.Clamp(ShieldVisualRadius, 16f, 180f) * 2f;
		float scale = (targetDiameter / texBase) * Mathf.Clamp(ShieldTextureScaleMultiplier, 0.1f, 4f);
		_shieldSprite.Scale = new Vector2(scale, scale);
	}

	private void EnsureShieldFallbackRing(Node2D anchor)
	{
		if (_shieldFallbackRing != null || anchor == null)
			return;

		_shieldFallbackRing = new Line2D
		{
			Name = "ShieldFallbackRing",
			TopLevel = false,
			ZAsRelative = false,
			ZIndex = ShieldZIndex,
			Width = Mathf.Clamp(ShieldFallbackRingWidth, 1f, 8f),
			DefaultColor = ShieldFallbackRingColor,
			Closed = true,
			Visible = false
		};

		UpdateFallbackRingPoints(Mathf.Clamp(ShieldVisualRadius, 16f, 180f) + 8f);

		anchor.AddChild(_shieldFallbackRing);
		_shieldFallbackRing.Position = Vector2.Zero;
	}

	private void RefreshShieldVisual(bool force = false)
	{
		if (_shieldSprite == null)
			EnsureShieldVisual();
		if (_shieldSprite == null && _shieldFallbackRing == null)
			return;
		if (_shieldSprite != null)
		{
			_shieldSprite.Position = Vector2.Zero;
			ApplyShieldVisualScale();
		}
		if (_shieldFallbackRing != null)
		{
			_shieldFallbackRing.Position = Vector2.Zero;
			UpdateFallbackRingPoints(Mathf.Clamp(ShieldVisualRadius, 16f, 180f) + 8f);
		}

		if (_shieldFlashActive)
			return;

		bool ready = _shieldEnabled && _shieldCooldownTimer <= 0f;
		bool visualVisible = _shieldSprite != null ? _shieldSprite.Visible : (_shieldFallbackRing != null && _shieldFallbackRing.Visible);
		if (!force && ready == _shieldVisualReadyLastFrame && visualVisible == _shieldEnabled)
			return;

		_shieldVisualReadyLastFrame = ready;
		if (!_shieldEnabled)
		{
			if (_shieldSprite != null)
				_shieldSprite.Visible = false;
			if (_shieldFallbackRing != null)
				_shieldFallbackRing.Visible = false;
			return;
		}

		if (ready)
		{
			if (_shieldSprite != null)
			{
				_shieldSprite.Visible = true;
				_shieldSprite.Modulate = ShieldReadyColor;
			}
			if (_shieldFallbackRing != null)
			{
				bool ringVisible = ShieldAlwaysShowRing || _shieldSprite == null || _shieldSprite.Texture == null;
				_shieldFallbackRing.Visible = ringVisible;
				_shieldFallbackRing.DefaultColor = ShieldReadyColor;
			}
			return;
		}

		float blinkWindow = Mathf.Clamp(ShieldRespawnBlinkWindowSeconds, 0.2f, 15f);
		if (_shieldCooldownTimer <= blinkWindow)
		{
			float blinkRate = Mathf.Clamp(ShieldRespawnBlinkRate, 1f, 30f);
			float phase = Time.GetTicksMsec() / 1000.0f;
			float pulse = (Mathf.Sin(phase * Mathf.Tau * blinkRate) + 1f) * 0.5f;
			bool on = pulse > 0.45f;
			if (_shieldSprite != null)
			{
				_shieldSprite.Visible = on;
				_shieldSprite.Modulate = ShieldCooldownColor;
			}
			if (_shieldFallbackRing != null)
			{
				bool ringVisible = ShieldAlwaysShowRing || _shieldSprite == null || _shieldSprite.Texture == null;
				_shieldFallbackRing.Visible = on && ringVisible;
				_shieldFallbackRing.DefaultColor = ShieldCooldownColor;
			}
			return;
		}

		if (_shieldSprite != null)
		{
			_shieldSprite.Visible = false;
			_shieldSprite.Modulate = ShieldCooldownColor;
		}
		if (_shieldFallbackRing != null)
		{
			_shieldFallbackRing.Visible = false;
			_shieldFallbackRing.DefaultColor = ShieldCooldownColor;
		}
	}

	private async void TriggerShieldHitFlash()
	{
		if (_shieldSprite == null && _shieldFallbackRing == null)
			EnsureShieldVisual();
		if ((_shieldSprite == null && _shieldFallbackRing == null) || GetTree() == null)
			return;

		_shieldFlashToken++;
		int token = _shieldFlashToken;
		_shieldFlashActive = true;
		if (_shieldSprite != null)
		{
			_shieldSprite.Visible = true;
			_shieldSprite.Modulate = ShieldHitFlashColor;
		}
		if (_shieldFallbackRing != null)
		{
			_shieldFallbackRing.Visible = true;
			_shieldFallbackRing.DefaultColor = ShieldHitFlashColor;
		}

		float duration = Mathf.Clamp(ShieldHitFlashDurationSeconds, 0.02f, 0.30f);
		var timer = GetTree().CreateTimer(duration, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (token != _shieldFlashToken)
			return;

		_shieldFlashActive = false;
		if (_shieldSprite != null)
			_shieldSprite.Visible = false;
		if (_shieldFallbackRing != null)
			_shieldFallbackRing.Visible = false;
		RefreshShieldVisual(force: true);
	}

	private void UpdateFallbackRingPoints(float radius)
	{
		if (_shieldFallbackRing == null)
			return;

		const int segments = 56;
		if (_shieldFallbackRing.GetPointCount() != segments)
		{
			_shieldFallbackRing.ClearPoints();
			for (int i = 0; i < segments; i++)
			{
				float t = i / (float)segments;
				float angle = t * Mathf.Tau;
				_shieldFallbackRing.AddPoint(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
			}
			return;
		}

		for (int i = 0; i < segments; i++)
		{
			float t = i / (float)segments;
			float angle = t * Mathf.Tau;
			_shieldFallbackRing.SetPointPosition(i, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
		}
	}
}
