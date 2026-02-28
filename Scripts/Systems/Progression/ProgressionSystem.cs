using Godot;

public partial class ProgressionSystem : Node
{
	[Export] public NodePath UpgradeMenuPath = "../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu";
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public int ExperiencePerPickup = 1;
	[Export] public bool UseEarlyUpgradeCurve = true;
	[Export] public float EarlyUpgradeRequirement1 = 3f;
	[Export] public float EarlyUpgradeRequirement2 = 5f;
	[Export] public float EarlyUpgradeRequirement3 = 7f;
	[Export] public float SurvivorXpBaseRequirement = 8f;
	[Export] public float SurvivorXpLinearGrowth = 2f;
	[Export] public float SurvivorXpGrowthFactor = 1.08f;
	[Export] public int LateXpSlowdownStartLevel = 5;
	[Export(PropertyHint.Range, "1.0,3.0,0.05")] public float LateXpSlowdownMultiplier = 2.0f;
	[Export(PropertyHint.Range, "0,10,1")] public int LateXpSlowdownRampLevels = 2;
	[Export(PropertyHint.Range, "1.0,4.0,0.05")] public float RangedPickupRadiusMultiplier = 2.0f;
	[Export(PropertyHint.Range, "0.5,4.0,0.05")] public float NonRangedPickupRadiusMultiplier = 1.0f;

	private UpgradeMenu _upgradeMenu;
	private float _upgradeProgress = 0f;
	private float _currentUpgradeRequirement = 0f;
	private int _upgradeLevel = 0;
	private int _pendingUpgradeOpens = 0;
	private float _experienceGainMultiplier = 1f;
	private float _xpRequirementOffset = 0f;
	private float _triggerReliefBonus = 0f;
	private float _pickupRadiusMultiplier = 1f;
	private float _characterPickupRadiusMultiplier = 1f;
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
	public float PickupRadiusMultiplier
	{
		get
		{
			RefreshCharacterPickupRadiusProfile();
			return Mathf.Clamp(_pickupRadiusMultiplier * _characterPickupRadiusMultiplier, 0.5f, 8f);
		}
	}

	public override void _EnterTree()
	{
		AddToGroup(RuntimeGroups.ProgressionSystem);
	}

	public override void _Ready()
	{
		EnsureUpgradeMenu();

		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");
		RefreshCharacterPickupRadiusProfile();

		var combatList = GetTree().GetNodesInGroup(RuntimeGroups.CombatSystem);
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
		if (UseEarlyUpgradeCurve)
		{
			if (_upgradeLevel <= 0)
				return Mathf.Max(1f, EarlyUpgradeRequirement1 + _xpRequirementOffset);
			if (_upgradeLevel == 1)
				return Mathf.Max(1f, EarlyUpgradeRequirement2 + _xpRequirementOffset);
			if (_upgradeLevel == 2)
				return Mathf.Max(1f, EarlyUpgradeRequirement3 + _xpRequirementOffset);

			float postEarlyLevel = _upgradeLevel - 3f;
			float baseRequirement = SurvivorXpBaseRequirement + (SurvivorXpLinearGrowth * postEarlyLevel);
			float postEarlyCurve = baseRequirement * Mathf.Pow(Mathf.Max(1f, SurvivorXpGrowthFactor), postEarlyLevel);
			return Mathf.Max(1f, ApplyLateXpSlowdown(postEarlyCurve + _xpRequirementOffset));
		}

		float level = _upgradeLevel;
		float curve = SurvivorXpBaseRequirement + (SurvivorXpLinearGrowth * level);
		curve *= Mathf.Pow(Mathf.Max(1f, SurvivorXpGrowthFactor), level);
		return Mathf.Max(1f, ApplyLateXpSlowdown(curve + _xpRequirementOffset));
	}

	private float ApplyLateXpSlowdown(float requirement)
	{
		float baseRequirement = Mathf.Max(1f, requirement);
		int startLevel = Mathf.Max(0, LateXpSlowdownStartLevel);
		if (_upgradeLevel < startLevel)
			return baseRequirement;

		float targetMultiplier = Mathf.Max(1f, LateXpSlowdownMultiplier);
		int rampLevels = Mathf.Max(0, LateXpSlowdownRampLevels);
		if (rampLevels <= 0)
			return baseRequirement * targetMultiplier;

		float t = Mathf.Clamp((_upgradeLevel - startLevel + 1f) / rampLevels, 0f, 1f);
		float multiplier = Mathf.Lerp(1f, targetMultiplier, t);
		return baseRequirement * multiplier;
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
		EnsureUpgradeMenu();
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;

		TriggerUpgradeMenu("boss/event exception");
	}

	public bool DebugForceOpenUpgradeMenu()
	{
		return TriggerUpgradeMenu("debug cheat");
	}

	public void DebugGrantUpgradeLevels(int levels, bool openMenu = true)
	{
		int amount = Mathf.Max(0, levels);
		if (amount <= 0)
			return;

		_upgradeLevel += amount;
		_pendingUpgradeOpens += amount;
		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());

		if (openMenu)
			TryConsumePendingUpgrade("debug cheat grant level");
	}

	private bool TriggerUpgradeMenu(string reason)
	{
		EnsureUpgradeMenu();
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return false;

		_upgradeMenu.OpenMenu();
		return _upgradeMenu.IsOpen;
	}

	private void TryConsumePendingUpgrade(string reason)
	{
		EnsureUpgradeMenu();
		if (_upgradeMenu == null || _upgradeMenu.IsOpen)
			return;
		if (_pendingUpgradeOpens <= 0)
			return;

		if (TriggerUpgradeMenu(reason))
			_pendingUpgradeOpens--;
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

	private void EnsureUpgradeMenu()
	{
		if (_upgradeMenu != null)
			return;

		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeMenu");
		if (_upgradeMenu == null)
			_upgradeMenu = GetNodeOrNull<UpgradeMenu>("../../CanvasLayer/UI/UpgradeLayer/UpgradeMenu");
	}

	private void RefreshCharacterPickupRadiusProfile()
	{
		if (!IsInstanceValid(_player))
			_player = GetNodeOrNull<Player>(PlayerPath);

		bool isRangedCharacter = _player?.PrimarySupportsRanged() ?? false;
		_characterPickupRadiusMultiplier = isRangedCharacter
			? Mathf.Max(1f, RangedPickupRadiusMultiplier)
			: Mathf.Max(0.5f, NonRangedPickupRadiusMultiplier);
	}

	public void ResetForNewRun()
	{
		_upgradeProgress = 0f;
		_upgradeLevel = 0;
		_pendingUpgradeOpens = 0;
		_experienceGainMultiplier = 1f;
		_xpRequirementOffset = 0f;
		_triggerReliefBonus = 0f;
		_pickupRadiusMultiplier = 1f;
		_characterPickupRadiusMultiplier = 1f;
		_killChanceLifestealEnabled = false;
		_killChanceLifestealHeal = 1;
		_killChanceLifestealChance = 0.12f;
		_currentUpgradeRequirement = Mathf.Max(1f, GetCurrentUpgradeRequirement());
		RefreshCharacterPickupRadiusProfile();
	}
}
