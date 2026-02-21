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
}
