using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MetaProgressionService
{
	private const string FirstClearGlobalFlag = "meta.first_clear.global";
	private const string FirstClearCharacterFlagPrefix = "meta.first_clear.character.";
	private const string DefaultProfileId = "default";

	private static readonly Lazy<MetaProgressionService> LazyInstance = new(() => new MetaProgressionService());

	private readonly JsonSaveStore _saveStore = new(DefaultProfileId);
	private readonly EconomyTuning _tuning = new();
	private MetaProgressionState _state;

	public static MetaProgressionService Instance => LazyInstance.Value;

	public int CurrencyWallet => _state.CurrencyWallet;
	public MetaProgressionState State => _state;
	public string CurrentProfileId => _saveStore.ProfileId;
	public string CurrentSavePath => _saveStore.SavePath;

	private MetaProgressionService()
	{
		_state = _saveStore.LoadState() ?? new MetaProgressionState();
		bool changed = EnsureBaselineUnlocks();
		if (changed)
			_saveStore.SaveState(_state);
	}

	public RewardBreakdown SettleRun(RunResult result)
	{
		result ??= new RunResult();
		if (string.IsNullOrWhiteSpace(result.RunId))
			result.RunId = Guid.NewGuid().ToString("N");

		bool firstGlobal = !_state.Flags.Has(FirstClearGlobalFlag);
		bool firstCharacter = IsFirstCharacterClear(result.CharacterId);

		var effective = new RunResult
		{
			RunId = result.RunId,
			Score = result.Score,
			CharacterId = result.CharacterId,
			IsPerfectClear = result.IsPerfectClear,
			IsFirstClearGlobal = firstGlobal,
			IsFirstClearForCharacter = firstCharacter
		};

		RewardBreakdown breakdown = RewardCalculator.Calculate(effective, _tuning, _state);
		if (breakdown.IsDuplicateRun)
			return breakdown;

		_state.MarkRunSettled(effective.RunId);
		_state.AddCurrency(breakdown.TotalCurrency);

		if (firstGlobal)
			_state.Flags.Add(FirstClearGlobalFlag);
		if (firstCharacter && !string.IsNullOrWhiteSpace(effective.CharacterId))
			_state.Flags.Add($"{FirstClearCharacterFlagPrefix}{effective.CharacterId}");

		_saveStore.SaveState(_state);
		return breakdown;
	}

	public bool IsCharacterUnlocked(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return false;
		return _state.UnlockedCharacterIds.Contains(characterId);
	}

	public bool TryUnlockCharacter(string characterId)
	{
		if (!CanUnlockCharacter(characterId, out int cost))
			return false;

		_state.TrySpendCurrency(cost);
		_state.UnlockCharacter(characterId);
		_saveStore.SaveState(_state);
		return true;
	}

	public bool CanUnlockCharacter(string characterId, out int cost)
	{
		cost = 0;
		if (!ProgressionDefs.TryGetCharacter(characterId, out CharacterDef def))
			return false;
		if (IsCharacterUnlocked(characterId))
			return false;

		cost = Math.Max(0, def.UnlockCost);
		return _state.CurrencyWallet >= cost;
	}

	public int GetCharacterLevel(string characterId)
	{
		if (!_state.TryGetCharacterProgress(characterId, out CharacterProgress progress))
			return 1;
		return progress.Level;
	}

	public bool TryUpgradeCharacterLevel(string characterId)
	{
		if (!CanUpgradeCharacterLevel(characterId, out int cost))
			return false;

		_state.TrySpendCurrency(cost);
		CharacterProgress progress = _state.EnsureCharacterProgress(characterId);
		if (progress == null)
			return false;
		progress.SetLevel(progress.Level + 1);
		_saveStore.SaveState(_state);
		return true;
	}

	public bool CanUpgradeCharacterLevel(string characterId, out int cost)
	{
		cost = 0;
		if (!ProgressionDefs.TryGetCharacter(characterId, out CharacterDef def))
			return false;
		if (!IsCharacterUnlocked(characterId))
			return false;

		int currentLevel = GetCharacterLevel(characterId);
		if (currentLevel >= def.MaxLevel)
			return false;

		cost = def.GetLevelUpCost(currentLevel);
		return _state.CurrencyWallet >= cost;
	}

	public bool TryUnlockAbilityNode(string characterId, string nodeId)
	{
		if (!CanUnlockAbilityNode(characterId, nodeId, out int cost))
			return false;

		_state.TrySpendCurrency(cost);
		CharacterProgress progress = _state.EnsureCharacterProgress(characterId);
		if (progress == null || !progress.UnlockAbilityNode(nodeId))
			return false;
		_saveStore.SaveState(_state);
		return true;
	}

	public bool CanUnlockAbilityNode(string characterId, string nodeId, out int cost)
	{
		cost = 0;
		if (!ProgressionDefs.TryGetCharacter(characterId, out CharacterDef characterDef))
			return false;
		if (!IsCharacterUnlocked(characterId))
			return false;
		if (!ProgressionDefs.TryGetAbilityNode(characterId, nodeId, out AbilityNodeDef nodeDef))
			return false;

		CharacterProgress progress = _state.TryGetCharacterProgress(characterId, out CharacterProgress existing)
			? existing
			: new CharacterProgress();
		if (progress.UnlockedAbilityNodes.Contains(nodeId))
			return false;
		if (progress.Level < Math.Max(1, nodeDef.MinCharacterLevel))
			return false;
		if (!HasPrerequisiteNodes(progress, nodeDef))
			return false;

		cost = Math.Max(0, nodeDef.UnlockCost);
		return _state.CurrencyWallet >= cost;
	}

	public IReadOnlyCollection<string> GetUnlockedAbilityNodes(string characterId)
	{
		return _state.TryGetCharacterProgress(characterId, out CharacterProgress progress)
			? progress.UnlockedAbilityNodes
			: Array.Empty<string>();
	}

	public void SaveNow()
	{
		_saveStore.SaveState(_state);
	}

	public void SetProfile(string profileId)
	{
		_saveStore.SetProfile(profileId);
		_state = _saveStore.LoadState() ?? new MetaProgressionState();
		bool changed = EnsureBaselineUnlocks();
		if (changed)
			_saveStore.SaveState(_state);
	}

	public bool DeleteCurrentProfileSave()
	{
		bool deleted = _saveStore.DeleteSaveFile();
		_state = new MetaProgressionState();
		EnsureBaselineUnlocks();
		return deleted;
	}

	public void DebugSetCurrencyWallet(int wallet, bool saveNow = true)
	{
		int nextWallet = Math.Max(0, wallet);
		int currentWallet = _state.CurrencyWallet;
		int earned = _state.CurrencyEarnedTotal;
		int spent = _state.CurrencySpentTotal;

		if (nextWallet > currentWallet)
			earned += nextWallet - currentWallet;
		else if (nextWallet < currentWallet)
			spent += currentWallet - nextWallet;

		_state.ReplaceCurrencySnapshot(nextWallet, Math.Max(nextWallet, earned), Math.Max(0, spent));
		if (saveNow)
			_saveStore.SaveState(_state);
	}

	public void RecordPerfectClear(int score, string characterName)
	{
		long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
		_state.AddPerfectClearRecord(score, characterName, unixTime);
		_saveStore.SaveState(_state);
	}

	public IReadOnlyList<PerfectClearRecord> GetPerfectLeaderboard(int maxCount)
	{
		if (maxCount <= 0)
			return Array.Empty<PerfectClearRecord>();
		return _state.GetPerfectClearRecords(maxCount).ToList();
	}

	private bool EnsureBaselineUnlocks()
	{
		bool changed = false;
		foreach (CharacterDef def in ProgressionDefs.GetAllCharacters())
		{
			if (def != null && def.IsDefaultUnlocked)
				changed |= _state.UnlockCharacter(def.CharacterId);
		}
		return changed;
	}

	private bool IsFirstCharacterClear(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return false;
		return !_state.Flags.Has($"{FirstClearCharacterFlagPrefix}{characterId}");
	}

	private static bool HasPrerequisiteNodes(CharacterProgress progress, AbilityNodeDef nodeDef)
	{
		if (progress == null || nodeDef?.PrerequisiteNodeIds == null || nodeDef.PrerequisiteNodeIds.Count == 0)
			return true;

		foreach (string prereq in nodeDef.PrerequisiteNodeIds)
		{
			if (string.IsNullOrWhiteSpace(prereq))
				continue;
			if (!progress.UnlockedAbilityNodes.Contains(prereq))
				return false;
		}

		return true;
	}
}
