using Godot;

public partial class PlayerHealth
{
	private void EnsureShieldVisual()
	{
		if (_shieldSprite != null || _shieldFallbackRing != null)
			return;

		Node2D anchor = ResolveSkillVfxRoot();
		if (anchor == null)
		{
			DebugSystem.Warn("[PlayerHealth] Shield visual anchor is null. Expected SkillVfxRoot or Player node parent.");
			return;
		}

		_shieldSprite = new Sprite2D();
		_shieldSprite.Name = "ShieldVfx";
		_shieldSprite.Centered = true;
		_shieldSprite.TopLevel = false;
		_shieldSprite.ZAsRelative = false;
		_shieldSprite.ZIndex = ShieldZIndex;
		_shieldSprite.Visible = false;
		_shieldSprite.Texture = ShieldTexture ?? GD.Load<Texture2D>("res://Assets/Sprites/Skills/Shield/shield.png");
		anchor.AddChild(_shieldSprite);
		_shieldSprite.Position = Vector2.Zero;
		if (_shieldSprite.Texture != null)
		{
			Vector2 size = _shieldSprite.Texture.GetSize();
			DebugSystem.Log($"[PlayerHealth] Shield texture loaded: '{_shieldSprite.Texture.ResourcePath}' size={size}. anchor={anchor.Name}");
		}
		else
		{
			DebugSystem.Warn("[PlayerHealth] Shield texture is null. Trying fallback ring visual.");
		}

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
			DebugSystem.Warn("[PlayerHealth] SkillVfxRoot not found; falling back to Player root.");
		return _skillVfxRoot;
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

		_shieldFallbackRing = new Line2D();
		_shieldFallbackRing.Name = "ShieldFallbackRing";
		_shieldFallbackRing.TopLevel = false;
		_shieldFallbackRing.ZAsRelative = false;
		_shieldFallbackRing.ZIndex = ShieldZIndex;
		_shieldFallbackRing.Width = Mathf.Clamp(ShieldFallbackRingWidth, 1f, 8f);
		_shieldFallbackRing.DefaultColor = ShieldFallbackRingColor;
		_shieldFallbackRing.Closed = true;
		_shieldFallbackRing.Visible = false;

		UpdateFallbackRingPoints(Mathf.Clamp(ShieldVisualRadius, 16f, 180f) + 8f);

		anchor.AddChild(_shieldFallbackRing);
		_shieldFallbackRing.Position = Vector2.Zero;
		DebugSystem.Log("[PlayerHealth] Shield fallback ring created.");
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
			DebugSystem.Log("[PlayerHealth] Shield visual state -> OFF");
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
			DebugSystem.Log("[PlayerHealth] Shield visual state -> READY");
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

	private async void TriggerDamageFeedback()
	{
		if (!EnableDamageFeedback || _isDead || GetTree() == null)
			return;

		Node2D anchor = GetParentOrNull<Node2D>();
		if (anchor == null)
			return;

		Sprite2D sprite = anchor.GetNodeOrNull<Sprite2D>("Sprite2D");
		float duration = Mathf.Clamp(DamageFeedbackDurationSeconds, 0.02f, 0.30f);
		if (duration <= 0f)
			return;

		_damageFeedbackToken++;
		int token = _damageFeedbackToken;

		if (sprite != null)
		{
			EnsureDamageFlashMaterial();
			if (_damageFlashMaterial != null)
			{
				if (sprite.Material != _damageFlashMaterial)
					_spriteMaterialBeforeFlash = sprite.Material;
				sprite.Material = _damageFlashMaterial;
				_damageFlashMaterial.SetShaderParameter("flash_amount", Mathf.Clamp(DamageFlashStrength, 0f, 1f));
			}
		}

		var timer = GetTree().CreateTimer(duration * 0.45f, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (token != _damageFeedbackToken)
			return;

		if (sprite != null && _damageFlashMaterial != null)
			_damageFlashMaterial.SetShaderParameter("flash_amount", Mathf.Clamp(DamageFlashStrength * 0.45f, 0f, 1f));

		timer = GetTree().CreateTimer(duration * 0.55f, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (token != _damageFeedbackToken)
			return;

		if (sprite != null && _damageFlashMaterial != null)
		{
			_damageFlashMaterial.SetShaderParameter("flash_amount", 0f);
			sprite.Material = _spriteMaterialBeforeFlash;
		}
	}

	private void EnsureDamageFlashMaterial()
	{
		if (_damageFlashMaterial != null)
			return;

		var shader = new Shader();
		shader.Code = @"
shader_type canvas_item;
uniform float flash_amount : hint_range(0.0, 1.0) = 0.0;

void fragment()
{
	vec4 tex = texture(TEXTURE, UV) * COLOR;
	tex.rgb = mix(tex.rgb, vec3(1.0), flash_amount);
	COLOR = tex;
}";

		_damageFlashMaterial = new ShaderMaterial();
		_damageFlashMaterial.Shader = shader;
		_damageFlashMaterial.SetShaderParameter("flash_amount", 0f);
	}
}
