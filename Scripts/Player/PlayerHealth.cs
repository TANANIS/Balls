using Godot;
using System;

/*
 * PlayerHealth.cs
 *
 * Responsibilities:
 * - Owns player health state: HP / IsDead / IsInvincible
 * - Applies damage state only; rules are decided in CombatSystem
 */

public partial class PlayerHealth : Node
{
	public event Action Died;

	[Export] public int MaxHp = 3;
	[Export] public float HurtIFrame = 0.5f;
	[Export] public int RegenAmount = 0;
	[Export] public float RegenIntervalSeconds = 60f;
	[Export] public bool EnableDamageFeedback = true;
	[Export(PropertyHint.Range, "0.02,0.30,0.005")] public float DamageFeedbackDurationSeconds = 0.10f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DamageFlashStrength = 0.95f;
	[Export] public bool EnableDebugInvincibleToggle = true;
	[Export] public Key DebugInvincibleToggleKey = Key.I;
	[Export] public NodePath SkillVfxRootPath = new NodePath("../SkillVfxRoot");
	[Export] public Texture2D ShieldTexture;
	[Export] public Color ShieldReadyColor = new Color(1.0f, 0.95f, 0.70f, 0.62f);
	[Export] public Color ShieldCooldownColor = new Color(1.0f, 0.95f, 0.70f, 0.28f);
	[Export] public Color ShieldHitFlashColor = new Color(1f, 1f, 1f, 1f);
	[Export(PropertyHint.Range, "16,180,0.5")] public float ShieldVisualRadius = 44f;
	[Export(PropertyHint.Range, "0.1,4,0.05")] public float ShieldTextureScaleMultiplier = 0.95f;
	[Export(PropertyHint.Range, "0.2,15,0.1")] public float ShieldRespawnBlinkWindowSeconds = 8f;
	[Export(PropertyHint.Range, "1,30,0.5")] public float ShieldRespawnBlinkRate = 10f;
	[Export(PropertyHint.Range, "0.02,0.30,0.01")] public float ShieldHitFlashDurationSeconds = 0.08f;
	[Export(PropertyHint.Range, "0,5000,1")] public int ShieldZIndex = 1200;
	[Export] public bool EnableShieldFallbackRing = true;
	[Export] public bool ShieldAlwaysShowRing = true;
	[Export] public Color ShieldFallbackRingColor = new Color(1.0f, 0.95f, 0.70f, 0.55f);
	[Export(PropertyHint.Range, "1,8,0.5")] public float ShieldFallbackRingWidth = 2.0f;

	private int _hp;
	private bool _isDead = false;
	private float _invincibleTimer = 0f;
	private float _regenTimer = 0f;
	private bool _shieldEnabled = false;
	private float _shieldCooldownSeconds = 0f;
	private float _shieldCooldownTimer = 0f;
	private Sprite2D _shieldSprite;
	private Line2D _shieldFallbackRing;
	private Node2D _skillVfxRoot;
	private bool _shieldVisualReadyLastFrame = false;
	private bool _shieldFlashActive = false;
	private int _shieldFlashToken = 0;
	private bool _debugInvincible = false;
	private bool _togglePressedLastFrame = false;
	private int _damageFeedbackToken = 0;
	private ShaderMaterial _damageFlashMaterial;
	private Material _spriteMaterialBeforeFlash;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _debugInvincible || _invincibleTimer > 0f;
	public bool IsDebugInvincible => _debugInvincible;
	public bool IsShieldEnabled => _shieldEnabled;
	public bool IsShieldReady => _shieldEnabled && _shieldCooldownTimer <= 0f;
	public float ShieldCooldownRemaining => Mathf.Max(0f, _shieldCooldownTimer);
	public float ShieldCooldownSeconds => _shieldCooldownSeconds;

	public override void _Ready()
	{
		_hp = MaxHp;
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
		EnsureShieldVisual();
		RefreshShieldVisual(force: true);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;
		if (_shieldCooldownTimer > 0f)
			_shieldCooldownTimer -= dt;
		RefreshShieldVisual();
		TickDebugInvincibleToggle();
		TickRegen(dt);
	}

	private void TickDebugInvincibleToggle()
	{
		if (!EnableDebugInvincibleToggle)
		{
			_togglePressedLastFrame = false;
			return;
		}

		bool pressed = Input.IsPhysicalKeyPressed(DebugInvincibleToggleKey);
		if (pressed && !_togglePressedLastFrame)
		{
			_debugInvincible = !_debugInvincible;
			DebugSystem.Log(_debugInvincible
				? "[PlayerHealth] Debug invincible ON."
				: "[PlayerHealth] Debug invincible OFF.");
		}

		_togglePressedLastFrame = pressed;
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f) return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void TakeDamage(int amount, object source)
	{
		// Minimal safeguard in case someone bypasses CombatSystem
		if (_isDead) return;
		if (IsInvincible) return;
		if (_shieldEnabled && _shieldCooldownTimer <= 0f)
		{
			_shieldCooldownTimer = _shieldCooldownSeconds;
			TriggerShieldHitFlash();
			RefreshShieldVisual(force: true);
			DebugSystem.Log("[PlayerHealth] Shield absorbed damage.");
			return;
		}

		_hp -= amount;
		TriggerDamageFeedback();
		AudioManager.Instance?.PlaySfxPlayerGetHit();
		DebugSystem.Log($"[PlayerHealth] Took {amount} damage. HP: {_hp}/{MaxHp}");
		if (RegenAmount > 0 && RegenIntervalSeconds > 0f)
			_regenTimer = RegenIntervalSeconds;

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0 && !_isDead)
		{
			_isDead = true;
			AudioManager.Instance?.StopLowHpLoop();
			AudioManager.Instance?.PlaySfxPlayerDie();
			Died?.Invoke();
		}
		else
		{
			UpdateLowHpAudio();
		}
	}

	public void ResetToFull()
	{
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
		_shieldEnabled = false;
		_shieldCooldownSeconds = 0f;
		_shieldCooldownTimer = 0f;
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
		RefreshShieldVisual(force: true);
		UpdateLowHpAudio();
	}

	public void AddMaxHp(int amount, bool healByAmount = true)
	{
		if (amount <= 0) return;

		MaxHp += amount;
		if (healByAmount)
			_hp += amount;

		if (_hp > MaxHp)
			_hp = MaxHp;

		UpdateLowHpAudio();
	}

	public void Heal(int amount)
	{
		if (amount <= 0 || _isDead)
			return;
		if (_hp >= MaxHp)
			return;

		_hp = Mathf.Min(MaxHp, _hp + amount);
		UpdateLowHpAudio();
	}

	public void EnableShield(float cooldownSeconds)
	{
		float cd = Mathf.Clamp(cooldownSeconds, 1f, 120f);
		if (!_shieldEnabled)
		{
			_shieldEnabled = true;
			_shieldCooldownSeconds = cd;
			_shieldCooldownTimer = 0f;
			RefreshShieldVisual(force: true);
			DebugSystem.Log($"[PlayerHealth] Shield enabled. Cooldown={_shieldCooldownSeconds:0.##}s");
			return;
		}

		_shieldCooldownSeconds = Mathf.Min(_shieldCooldownSeconds, cd);
		RefreshShieldVisual(force: true);
		DebugSystem.Log($"[PlayerHealth] Shield cooldown updated. Cooldown={_shieldCooldownSeconds:0.##}s");
	}

	private void UpdateLowHpAudio()
	{
		if (_isDead)
			return;

		if (_hp <= 1)
			AudioManager.Instance?.StartLowHpLoop();
		else
			AudioManager.Instance?.StopLowHpLoop();
	}

	public void SetBaseStats(int maxHp, float hurtIFrame, bool refill = true)
	{
		MaxHp = Mathf.Max(1, maxHp);
		HurtIFrame = Mathf.Max(0f, hurtIFrame);
		_shieldEnabled = false;
		_shieldCooldownSeconds = 0f;
		_shieldCooldownTimer = 0f;
		RefreshShieldVisual(force: true);
		if (refill)
		{
			_hp = MaxHp;
			_isDead = false;
			_invincibleTimer = 0f;
			UpdateLowHpAudio();
		}
		else if (_hp > MaxHp)
		{
			_hp = MaxHp;
		}
	}

	public void SetRegen(int amount, float intervalSeconds)
	{
		RegenAmount = Mathf.Max(0, amount);
		RegenIntervalSeconds = Mathf.Max(0f, intervalSeconds);
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
	}

	private void TickRegen(float dt)
	{
		if (_isDead || RegenAmount <= 0 || RegenIntervalSeconds <= 0f)
			return;
		if (_hp >= MaxHp)
			return;

		_regenTimer -= dt;
		if (_regenTimer > 0f)
			return;

		_hp = Mathf.Min(MaxHp, _hp + RegenAmount);
		_regenTimer = RegenIntervalSeconds;
		UpdateLowHpAudio();
	}

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
