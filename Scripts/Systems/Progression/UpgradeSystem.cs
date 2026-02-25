using Godot;
using System.Collections.Generic;

public partial class UpgradeSystem : Node
{
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public UpgradeCatalog Catalog;

	[ExportGroup("Pool Router")]
	[Export] public bool EnablePhasePoolRouter = true;
	[Export(PropertyHint.Range, "0,100,1")] public int MidPoolStartUpgradeCount = 6;
	[Export(PropertyHint.Range, "0,100,1")] public int LatePoolStartUpgradeCount = 12;
	[Export] public bool PhasePoolStrictFilter = false;

	[ExportGroup("Weighting")]
	[Export(PropertyHint.Range, "0,2,0.01")] public float CategoryBiasPerPick = 0.18f;
	[Export] public bool UseCategoryWeightDecay = true;
	[Export(PropertyHint.Range, "0,1,0.01")] public float CategoryWeightDecayPerPick = 0.12f;
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float CategoryWeightDecayFloor = 0.30f;
	[Export(PropertyHint.Range, "1,20,1")] public int RarePityThreshold = 4;
	[Export(PropertyHint.Range, "1,30,1")] public int EpicPityThreshold = 8;

	// Cached runtime dependencies.
	private Player _player;
	private PlayerHealth _playerHealth;
	private ProgressionSystem _progressionSystem;
	private int _appliedUpgradeCount = 0;
	private readonly Dictionary<UpgradeId, UpgradeDefinition> _definitions = new();
	private readonly Dictionary<UpgradeId, int> _stacks = new();
	private readonly Dictionary<UpgradeCategory, int> _categoryPickCounts = new();
	private int _offersWithoutRare = 0;
	private int _offersWithoutEpic = 0;

	public int AppliedUpgradeCount => _appliedUpgradeCount;

	public override void _EnterTree()
	{
		AddToGroup("UpgradeSystem");
	}

	public override void _Ready()
	{
		// Resolve player and cache all upgrade targets once.
		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player == null)
			return;

		_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		var progressionList = GetTree().GetNodesInGroup("ProgressionSystem");
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		RebuildDefinitionIndex();
		ValidateCatalogIntegrity();
	}

	private void RebuildDefinitionIndex()
	{
		_definitions.Clear();

		if (Catalog == null || Catalog.Entries == null)
			return;

		foreach (var entry in Catalog.Entries)
		{
			if (entry == null)
				continue;
			_definitions[entry.Id] = entry;
		}
	}

	private bool TryGetDefinition(UpgradeId id, out UpgradeDefinition definition)
	{
		if (_definitions.Count == 0)
			RebuildDefinitionIndex();

		return _definitions.TryGetValue(id, out definition);
	}

	private int GetStack(UpgradeId id)
	{
		return _stacks.TryGetValue(id, out int stack) ? stack : 0;
	}

	private void AddStack(UpgradeId id, int amount)
	{
		if (amount <= 0)
			return;

		int stack = GetStack(id);
		_stacks[id] = stack + amount;
	}

	private void AddCategoryPick(UpgradeCategory category)
	{
		int count = 0;
		_categoryPickCounts.TryGetValue(category, out count);
		_categoryPickCounts[category] = count + 1;
	}

	public void ResetForNewRun()
	{
		_appliedUpgradeCount = 0;
		_stacks.Clear();
		_categoryPickCounts.Clear();
		_offersWithoutRare = 0;
		_offersWithoutEpic = 0;
	}
}
