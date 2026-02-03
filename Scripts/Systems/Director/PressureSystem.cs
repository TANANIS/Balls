using Godot;

public partial class PressureSystem : Node
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public NodePath UpgradeMenuPath = "../../CanvasLayer/UI/UpgradeMenu";

	[Export] public float MaxPressure = 100f;
	[Export] public float TriggerThreshold = 55f;
	[Export] public float FirstTriggerThreshold = 35f;
	[Export] public float TriggerCooldown = 8f;
	[Export] public float PressureDropOnTrigger = 25f;

	[Export] public int EnemyCountForMaxPressure = 24;
	[Export] public float SecondsForMaxTimePressure = 130f;

	[Export] public float EnemyWeight = 0.55f;
	[Export] public float LowHpWeight = 0.25f;
	[Export] public float TimeWeight = 0.20f;

	[Export] public float RisePerSecond = 45f;
	[Export] public float FallPerSecond = 20f;

	[Export] public bool VerboseLog = false;

	private PlayerHealth _playerHealth;
	private Node _player;
	private Node2D _enemiesRoot;
	private UpgradeMenu _upgradeMenu;
	private CombatSystem _combatSystem;

	private float _pressure = 0f;
	private float _triggerCooldownTimer = 0f;
	private float _survivalSeconds = 0f;
	private bool _firstUpgradeTriggered = false;
	private bool _upgradeArmed = false;

	public float CurrentPressure => _pressure;

	public override void _EnterTree()
	{
		AddToGroup("PressureSystem");
	}

	public override void _Ready()
	{
		_player = GetNodeOrNull<Node>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combatSystem = list[0] as CombatSystem;
		if (_combatSystem != null)
			_combatSystem.EnemyKilled += OnEnemyKilled;

		if (_playerHealth == null)
			DebugSystem.Error("[PressureSystem] PlayerHealth not found.");
		if (_enemiesRoot == null)
			DebugSystem.Error("[PressureSystem] Enemies root not found.");
		if (_upgradeMenu == null)
			DebugSystem.Error("[PressureSystem] UpgradeMenu not found.");
		if (_combatSystem == null)
			DebugSystem.Error("[PressureSystem] CombatSystem not found.");
	}

	public override void _ExitTree()
	{
		if (_combatSystem != null)
			_combatSystem.EnemyKilled -= OnEnemyKilled;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		_survivalSeconds += dt;

		if (_triggerCooldownTimer > 0f)
			_triggerCooldownTimer -= dt;

		float target = CalculateTargetPressure();
		float speed = target >= _pressure ? RisePerSecond : FallPerSecond;
		_pressure = Mathf.MoveToward(_pressure, target, speed * dt);

		if (VerboseLog && Engine.GetPhysicsFrames() % 30 == 0)
			DebugSystem.Log($"[PressureSystem] pressure={_pressure:F1}/{MaxPressure:F1} target={target:F1}");

		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		if (_triggerCooldownTimer > 0f)
			return;

		float required = _firstUpgradeTriggered ? TriggerThreshold : FirstTriggerThreshold;
		if (_pressure >= required)
			_upgradeArmed = true;
	}

	private float CalculateTargetPressure()
	{
		float enemyFactor = 0f;
		if (_enemiesRoot != null && EnemyCountForMaxPressure > 0)
			enemyFactor = Mathf.Clamp((float)_enemiesRoot.GetChildCount() / EnemyCountForMaxPressure, 0f, 1f);

		float hpFactor = 0f;
		if (_playerHealth != null && _playerHealth.MaxHp > 0)
			hpFactor = Mathf.Clamp(1f - ((float)_playerHealth.Hp / _playerHealth.MaxHp), 0f, 1f);

		float timeFactor = 0f;
		if (SecondsForMaxTimePressure > 0f)
			timeFactor = Mathf.Clamp(_survivalSeconds / SecondsForMaxTimePressure, 0f, 1f);

		float weighted = (enemyFactor * EnemyWeight) + (hpFactor * LowHpWeight) + (timeFactor * TimeWeight);
		return Mathf.Clamp(weighted, 0f, 1f) * MaxPressure;
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (!_upgradeArmed || _upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_triggerCooldownTimer > 0f)
			return;
		if (source == null || _player == null || source != _player)
			return;

		_upgradeMenu.OpenMenu();
		_firstUpgradeTriggered = true;
		_upgradeArmed = false;
		_triggerCooldownTimer = TriggerCooldown;
		_pressure = Mathf.Max(0f, _pressure - PressureDropOnTrigger);
		DebugSystem.Log("[PressureSystem] Triggered upgrade menu by kill after pressure threshold.");
	}

	public void ForceOpenForBoss()
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		_upgradeMenu.OpenMenu();
		_firstUpgradeTriggered = true;
		_upgradeArmed = false;
		_triggerCooldownTimer = TriggerCooldown;
		_pressure = Mathf.Max(0f, _pressure - PressureDropOnTrigger);
		DebugSystem.Log("[PressureSystem] Force-opened upgrade menu (boss/event exception).");
	}
}
