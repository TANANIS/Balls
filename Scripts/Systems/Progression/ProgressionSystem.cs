using Godot;

public partial class ProgressionSystem : Node
{
	[Export] public NodePath UpgradeMenuPath = "../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu";
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public int ExperiencePerPickup = 1;
	[Export] public float SurvivorXpBaseRequirement = 8f;
	[Export] public float SurvivorXpLinearGrowth = 2f;
	[Export] public float SurvivorXpGrowthFactor = 1.08f;

	private UpgradeMenu _upgradeMenu;
	private float _upgradeProgress = 0f;
	private float _currentUpgradeRequirement = 0f;
	private int _upgradeLevel = 0;
	private int _pendingUpgradeOpens = 0;
	private float _experienceGainMultiplier = 1f;
	private float _xpRequirementOffset = 0f;
	private float _triggerReliefBonus = 0f;
	private float _pickupRadiusMultiplier = 1f;
	private bool _killChanceLifestealEnabled = false;
	private int _killChanceLifestealHeal = 1;
	private float _killChanceLifestealChance = 0.12f;
	private readonly RandomNumberGenerator _rng = new();
	private Player _player;
	private PlayerHealth _playerHealth;
	private CombatSystem _combatSystem;

	public float CurrentUpgradeProgress => _upgradeProgress;
	public bool IsUpgradeReady => _pendingUpgradeOpens > 0;
	public int CurrentUpgradeLevel => _upgradeLevel;
	public int PendingUpgradeCount => _pendingUpgradeOpens;
	public float PickupRadiusMultiplier => _pickupRadiusMultiplier;

	public override void _EnterTree()
	{
		AddToGroup("ProgressionSystem");
	}

	public override void _Ready()
	{
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeMenu");
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu");

		if (_upgradeMenu == null)
			DebugSystem.Warn("[ProgressionSystem] UpgradeMenu not found. Placeholder node is active only.");

		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		var combatList = GetTree().GetNodesInGroup("CombatSystem");
		if (combatList.Count > 0)
			_combatSystem = combatList[0] as CombatSystem;
		if (_combatSystem != null)
			_combatSystem.EnemyKilled += OnEnemyKilled;

		_rng.Randomize();
		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());
	}

	public override void _ExitTree()
	{
		if (_combatSystem != null)
			_combatSystem.EnemyKilled -= OnEnemyKilled;
	}

	public void TickPendingUpgradeOpen()
	{
		TryConsumePendingUpgrade("pending experience");
	}

	public void TriggerUpgradeFromExperiencePickup()
	{
		AddExperienceFromPickup(ExperiencePerPickup);
	}

	public void AddExperienceFromPickup(int amount)
	{
		if (amount <= 0)
			return;

		float expToAdd = Mathf.Max(1f, amount) * _experienceGainMultiplier;
		_upgradeProgress += expToAdd;
		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());

		while (_upgradeProgress >= _currentUpgradeRequirement)
		{
			_upgradeProgress -= _currentUpgradeRequirement;
			_upgradeLevel++;
			_pendingUpgradeOpens++;
			_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());
		}

		_upgradeProgress = Mathf.Clamp(_upgradeProgress, 0f, _currentUpgradeRequirement);
		TryConsumePendingUpgrade("experience pickup");
	}

	public float GetCurrentUpgradeRequirement()
	{
		float level = _upgradeLevel;
		float curve = SurvivorXpBaseRequirement + (SurvivorXpLinearGrowth * level);
		curve *= Mathf.Pow(Mathf.Max(1f, SurvivorXpGrowthFactor), level);
		return Mathf.Max(1f, curve + _xpRequirementOffset);
	}

	public void MultiplyKillProgressGain(float factor)
	{
		_experienceGainMultiplier = Mathf.Clamp(_experienceGainMultiplier * factor, 0.2f, 4.5f);
	}

	public void MultiplyTimeProgressGain(float factor)
	{
		// EXP pickup mode has no passive drip; keep multiplier behavior aligned for compatibility.
		_experienceGainMultiplier = Mathf.Clamp(_experienceGainMultiplier * factor, 0.2f, 4.5f);
	}

	public void AddTriggerThresholdOffset(float amount)
	{
		// In EXP mode this maps to level-up requirement offset.
		_xpRequirementOffset = Mathf.Clamp(_xpRequirementOffset + amount, -40f, 80f);
	}

	public void AddPressureDropOnTrigger(float amount)
	{
		// Compatibility placeholder for legacy pressure-relief style upgrades.
		_triggerReliefBonus = Mathf.Clamp(_triggerReliefBonus + amount, -20f, 50f);
	}

	public void MultiplyPickupRadius(float factor)
	{
		_pickupRadiusMultiplier = Mathf.Clamp(_pickupRadiusMultiplier * Mathf.Max(0.1f, factor), 0.5f, 4f);
	}

	public void EnableKillChanceLifesteal(int healAmount, float chance)
	{
		_killChanceLifestealEnabled = true;
		_killChanceLifestealHeal = Mathf.Clamp(healAmount, 1, 5);
		_killChanceLifestealChance = Mathf.Clamp(chance, 0.01f, 1f);
	}

	public void ForceOpenForBoss()
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		TriggerUpgradeMenu("boss/event exception");
	}

	private void TriggerUpgradeMenu(string reason)
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		_upgradeMenu.OpenMenu();
		DebugSystem.Log($"[ProgressionSystem] Triggered upgrade menu: {reason}.");
	}

	private void TryConsumePendingUpgrade(string reason)
	{
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_pendingUpgradeOpens <= 0)
			return;

		_pendingUpgradeOpens--;
		TriggerUpgradeMenu(reason);
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (!_killChanceLifestealEnabled)
			return;
		if (_player == null || _playerHealth == null)
			return;
		if (source != _player)
			return;
		if (target == null)
			return;
		if (_rng.Randf() > _killChanceLifestealChance)
			return;

		_playerHealth.Heal(_killChanceLifestealHeal);
	}
}
