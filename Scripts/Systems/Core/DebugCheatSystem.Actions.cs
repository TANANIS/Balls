using Godot;
using System;

public partial class DebugCheatSystem
{
	private void PullCurrentValues()
	{
		ResolveRefs();

		if (_stabilitySystem != null && _timeSecondsInput != null)
			_timeSecondsInput.Value = Mathf.RoundToInt(_stabilitySystem.ElapsedSeconds);

		if (_walletInput != null)
			_walletInput.Value = MetaProgressionService.Instance.CurrencyWallet;
		if (_iceShardInput != null)
			_iceShardInput.Value = MetaProgressionService.Instance.GetDomainShardBalance("Ice");
		if (_spacetimeShardInput != null)
			_spacetimeShardInput.Value = MetaProgressionService.Instance.GetDomainShardBalance("Spacetime");
		if (_warShardInput != null)
			_warShardInput.Value = MetaProgressionService.Instance.GetDomainShardBalance("War");

		if (_playerHealth != null && _hpInput != null)
		{
			_hpInput.MaxValue = Mathf.Max(1, _playerHealth.MaxHp);
			_hpInput.Value = _playerHealth.Hp;
		}

		if (_noDamageToggle != null)
			_noDamageToggle.ButtonPressed = _playerHealth?.IsDebugNoDamage ?? false;

		if (_timeScaleSlider != null)
			_timeScaleSlider.Value = Engine.TimeScale;
		UpdateTimeScaleLabel();
	}

	private void RefreshEnemyList()
	{
		ResolveRefs();
		if (_enemyIdOption == null)
			return;

		_enemyIdOption.Clear();
		if (_spawnSystem == null)
			return;

		string[] ids = _spawnSystem.DebugGetEnemyIds();
		foreach (string id in ids)
			_enemyIdOption.AddItem(id);
	}

	private void SpawnRequestedEnemy()
	{
		if (_spawnSystem == null || _enemyIdOption == null || _enemyIdOption.ItemCount <= 0)
			return;

		string id = _enemyIdOption.GetItemText(_enemyIdOption.Selected);
		int count = (int)Mathf.Clamp((float)_spawnCountInput.Value, 1f, 64f);
		_spawnSystem.DebugSpawnEnemyById(id, count);
	}

	private void RefreshUpgradeList()
	{
		ResolveRefs();
		if (_upgradeIdOption == null)
			return;

		_upgradeIdOption.Clear();
		if (_upgradeSystem?.Catalog?.Entries == null)
			return;

		foreach (var entry in _upgradeSystem.Catalog.Entries)
		{
			if (entry == null)
				continue;

			int id = (int)entry.Id;
			string titleEn = string.IsNullOrWhiteSpace(entry.Title) ? $"{entry.Id}" : entry.Title;
			string titleZh = ResolveZhByKey(entry.TitleKey, titleEn);
			string text = $"{entry.Id} | {Bi(titleEn, titleZh)}";
			_upgradeIdOption.AddItem(text, id);
		}
	}

	private void ApplyRequestedUpgrade()
	{
		ResolveRefs();
		if (_upgradeSystem == null || _upgradeIdOption == null || _upgradeIdOption.ItemCount <= 0)
		{
			SetUpgradeActionMessage(Bi("Upgrade system unavailable.", "升級系統不可用。"));
			return;
		}

		int selectedId = _upgradeIdOption.GetSelectedId();
		if (!Enum.IsDefined(typeof(UpgradeId), selectedId))
		{
			SetUpgradeActionMessage(Bi("Invalid upgrade id.", "無效的升級ID。"));
			return;
		}

		UpgradeId id = (UpgradeId)selectedId;
		int count = (int)Mathf.Clamp((float)(_upgradeApplyCountInput?.Value ?? 1), 1f, 20f);
		int applied = 0;
		for (int i = 0; i < count; i++)
		{
			if (!_upgradeSystem.DebugApplyUpgrade(id))
				break;
			applied++;
		}

		SetUpgradeActionMessage(Bi($"Applied {id}: {applied}/{count}", $"已套用 {id}: {applied}/{count}"));
	}

	private void OnNoDamageToggled(bool enabled)
	{
		ResolveRefs();
		_playerHealth?.SetDebugNoDamage(enabled);
	}

	private void SetUpgradeActionMessage(string message)
	{
		if (_upgradeActionLabel != null)
			_upgradeActionLabel.Text = message;
	}

	private void OnTimeScaleChanged(double value)
	{
		Engine.TimeScale = Mathf.Clamp((float)value, 0.1f, 4.0f);
		UpdateTimeScaleLabel();
	}

	private void UpdateTimeScaleLabel()
	{
		if (_timeScaleValueLabel != null)
			_timeScaleValueLabel.Text = $"{Engine.TimeScale:0.00}x";
	}
}
