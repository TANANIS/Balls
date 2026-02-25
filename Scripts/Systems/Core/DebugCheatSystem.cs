using Godot;
using System;
using System.Collections.Generic;

public partial class DebugCheatSystem : Node
{
	private const int DebugUiFontSize = 18;
	private const float DebugUiRowMinHeight = 40f;
	private const string CardLocalizationCsvPath = "res://Data/Localization/Cards.csv";

	[Export] public bool EnableDebugMenu = false;
	[Export] public bool StartVisible = false;
	[Export(PropertyHint.Range, "0.1,4.0,0.05")] public float DefaultTimeScale = 1.0f;
	[Export] public NodePath PlayerPath = "../../Player";

	private CanvasLayer _layer;
	private PanelContainer _panel;
	private SpinBox _timeSecondsInput;
	private SpinBox _walletInput;
	private SpinBox _hpInput;
	private CheckBox _noDamageToggle;
	private OptionButton _enemyIdOption;
	private SpinBox _spawnCountInput;
	private OptionButton _upgradeIdOption;
	private SpinBox _upgradeApplyCountInput;
	private SpinBox _upgradeLevelGrantInput;
	private HSlider _timeScaleSlider;
	private Label _timeScaleValueLabel;
	private Label _upgradeActionLabel;
	private readonly Dictionary<string, string> _cardZhLookup = new();
	private bool _cardZhLookupLoaded = false;

	private PlayerHealth _playerHealth;
	private StabilitySystem _stabilitySystem;
	private SpawnSystem _spawnSystem;
	private ProgressionSystem _progressionSystem;
	private UpgradeSystem _upgradeSystem;
	private bool _pausedByDebugMenu = false;
	private bool _pauseStateBeforeDebugMenu = false;

	public override void _Ready()
	{
		if (!EnableDebugMenu)
			return;

		ProcessMode = ProcessModeEnum.Always;
		// Prevent stale timescale from previous test sessions.
		Engine.TimeScale = Mathf.Clamp(DefaultTimeScale, 0.1f, 4.0f);
		ResolveRefs();
		BuildUi();
		RefreshEnemyList();
		RefreshUpgradeList();
		PullCurrentValues();
		SetMenuVisible(StartVisible);
	}

	public override void _ExitTree()
	{
		if (_pausedByDebugMenu && GetTree() != null)
			GetTree().Paused = _pauseStateBeforeDebugMenu;
		_pausedByDebugMenu = false;

		if (EnableDebugMenu)
			Engine.TimeScale = Mathf.Clamp(DefaultTimeScale, 0.1f, 4.0f);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!EnableDebugMenu || @event is not InputEventKey keyEvent)
			return;
		if (!keyEvent.Pressed || keyEvent.Echo)
			return;
		if (keyEvent.PhysicalKeycode != Key.F3)
			return;

		SetMenuVisible(!(_panel?.Visible ?? false));
		GetViewport().SetInputAsHandled();
	}

	private void ResolveRefs()
	{
		Node player = GetNodeOrNull(PlayerPath);
		_playerHealth = player?.GetNodeOrNull<PlayerHealth>("Health");

		var stabilityList = GetTree().GetNodesInGroup("StabilitySystem");
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		var spawnList = GetTree().GetNodesInGroup("SpawnSystem");
		if (spawnList.Count > 0)
			_spawnSystem = spawnList[0] as SpawnSystem;

		var progressionList = GetTree().GetNodesInGroup("ProgressionSystem");
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		var upgradeList = GetTree().GetNodesInGroup("UpgradeSystem");
		if (upgradeList.Count > 0)
			_upgradeSystem = upgradeList[0] as UpgradeSystem;
	}

	private void BuildUi()
	{
		_layer = new CanvasLayer
		{
			Name = "DebugCheatLayer",
			Layer = 90
		};
		AddChild(_layer);

		_panel = new PanelContainer
		{
			Name = "DebugCheatPanel",
			OffsetLeft = 14f,
			OffsetTop = 14f,
			OffsetRight = 560f,
			OffsetBottom = 760f
		};
		_layer.AddChild(_panel);
		_panel.Visible = false;

		var margin = new MarginContainer
		{
		};
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		_panel.AddChild(margin);

		var root = new VBoxContainer
		{
		};
		root.AddThemeConstantOverride("separation", 8);
		margin.AddChild(root);

		var title = new Label { Text = Bi("DEBUG CHEAT PANEL (F3)", "除錯作弊面板 (F3)") };
		root.AddChild(title);

		root.AddChild(BuildTimeRow());
		root.AddChild(BuildWalletRow());
		root.AddChild(BuildHpRow());
		root.AddChild(BuildNoDamageRow());
		root.AddChild(BuildUpgradeDraftRow());
		root.AddChild(BuildUpgradeLevelRow());
		root.AddChild(BuildDirectUpgradeRow());
		root.AddChild(BuildSpawnRow());
		root.AddChild(BuildSpeedRow());
		root.AddChild(BuildBottomButtons());
		ApplyLargeTextUi(_panel);
	}

	private Control BuildTimeRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Time(sec)", "時間(秒)"), CustomMinimumSize = new Vector2(170, 0) });
		_timeSecondsInput = new SpinBox { MinValue = 0, MaxValue = 36000, Step = 1, Rounded = true };
		_timeSecondsInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_timeSecondsInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += () =>
		{
			_stabilitySystem?.DebugSetElapsedSeconds((float)_timeSecondsInput.Value);
		};
		row.AddChild(apply);
		return row;
	}

	private Control BuildWalletRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Flux", "代幣"), CustomMinimumSize = new Vector2(170, 0) });
		_walletInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true };
		_walletInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_walletInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += () => MetaProgressionService.Instance.DebugSetCurrencyWallet((int)_walletInput.Value, saveNow: true);
		row.AddChild(apply);
		return row;
	}

	private Control BuildHpRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("HP", "生命"), CustomMinimumSize = new Vector2(170, 0) });
		_hpInput = new SpinBox { MinValue = 0, MaxValue = 999, Step = 1, Rounded = true };
		_hpInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_hpInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += () => _playerHealth?.DebugSetCurrentHp((int)_hpInput.Value);
		row.AddChild(apply);
		return row;
	}

	private Control BuildNoDamageRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Damage", "受傷"), CustomMinimumSize = new Vector2(170, 0) });
		_noDamageToggle = new CheckBox { Text = Bi("No HP Loss", "受傷不扣血") };
		_noDamageToggle.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_noDamageToggle.Toggled += OnNoDamageToggled;
		row.AddChild(_noDamageToggle);
		return row;
	}

	private Control BuildUpgradeDraftRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Draft", "詞條"), CustomMinimumSize = new Vector2(170, 0) });

		var open = new Button { Text = Bi("Open Draft", "開啟詞條") };
		open.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		open.Pressed += () =>
		{
			ResolveRefs();
			bool opened = _progressionSystem?.DebugForceOpenUpgradeMenu() ?? false;
			SetUpgradeActionMessage(opened ? Bi("Draft opened.", "已開啟詞條。") : Bi("Draft open failed.", "詞條開啟失敗。"));
		};
		row.AddChild(open);
		return row;
	}

	private Control BuildDirectUpgradeRow()
	{
		var row = new VBoxContainer();
		row.AddThemeConstantOverride("separation", 4);

		var top = new HBoxContainer();
		top.AddChild(new Label { Text = Bi("Upgrade", "升級"), CustomMinimumSize = new Vector2(170, 0) });
		_upgradeIdOption = new OptionButton();
		_upgradeIdOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		top.AddChild(_upgradeIdOption);

		var reload = new Button { Text = Bi("Reload", "重載") };
		reload.Pressed += RefreshUpgradeList;
		top.AddChild(reload);
		row.AddChild(top);

		var bottom = new HBoxContainer();
		bottom.AddChild(new Label { Text = Bi("Count", "次數"), CustomMinimumSize = new Vector2(170, 0) });
		_upgradeApplyCountInput = new SpinBox { MinValue = 1, MaxValue = 20, Step = 1, Rounded = true, Value = 1 };
		_upgradeApplyCountInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		bottom.AddChild(_upgradeApplyCountInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += ApplyRequestedUpgrade;
		bottom.AddChild(apply);
		row.AddChild(bottom);

		_upgradeActionLabel = new Label { Text = " " };
		row.AddChild(_upgradeActionLabel);

		return row;
	}

	private Control BuildUpgradeLevelRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("LevelUp", "升級次數"), CustomMinimumSize = new Vector2(170, 0) });
		_upgradeLevelGrantInput = new SpinBox { MinValue = 1, MaxValue = 20, Step = 1, Rounded = true, Value = 1 };
		_upgradeLevelGrantInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_upgradeLevelGrantInput);

		var grant = new Button { Text = Bi("Grant+Open", "發放並開啟") };
		grant.Pressed += () =>
		{
			ResolveRefs();
			if (_progressionSystem == null)
			{
				SetUpgradeActionMessage(Bi("Progression system unavailable.", "進度系統不可用。"));
				return;
			}

			int count = (int)Mathf.Clamp((float)_upgradeLevelGrantInput.Value, 1f, 20f);
			_progressionSystem.DebugGrantUpgradeLevels(count, openMenu: true);
			SetUpgradeActionMessage(Bi($"Granted upgrade level x{count}.", $"已發放升級次數 x{count}。"));
		};
		row.AddChild(grant);
		return row;
	}

	private Control BuildSpawnRow()
	{
		var row = new VBoxContainer();

		var top = new HBoxContainer();
		top.AddChild(new Label { Text = Bi("Spawn", "生成"), CustomMinimumSize = new Vector2(170, 0) });
		_enemyIdOption = new OptionButton();
		_enemyIdOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		top.AddChild(_enemyIdOption);
		row.AddChild(top);

		var bottom = new HBoxContainer();
		bottom.AddChild(new Label { Text = Bi("Count", "數量"), CustomMinimumSize = new Vector2(170, 0) });
		_spawnCountInput = new SpinBox { MinValue = 1, MaxValue = 64, Step = 1, Rounded = true, Value = 1 };
		_spawnCountInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		bottom.AddChild(_spawnCountInput);

		var spawn = new Button { Text = Bi("Spawn", "生成") };
		spawn.Pressed += SpawnRequestedEnemy;
		bottom.AddChild(spawn);

		var refresh = new Button { Text = Bi("Reload IDs", "重載 ID") };
		refresh.Pressed += RefreshEnemyList;
		bottom.AddChild(refresh);
		row.AddChild(bottom);

		return row;
	}

	private Control BuildSpeedRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Speed", "速度"), CustomMinimumSize = new Vector2(170, 0) });
		_timeScaleSlider = new HSlider
		{
			MinValue = 0.1,
			MaxValue = 4.0,
			Step = 0.05,
			Value = 1.0
		};
		_timeScaleSlider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_timeScaleSlider.ValueChanged += OnTimeScaleChanged;
		row.AddChild(_timeScaleSlider);

		_timeScaleValueLabel = new Label { Text = "1.00x", CustomMinimumSize = new Vector2(54, 0) };
		row.AddChild(_timeScaleValueLabel);
		return row;
	}

	private Control BuildBottomButtons()
	{
		var row = new HBoxContainer();

		var sync = new Button { Text = Bi("Sync Values", "同步數值") };
		sync.Pressed += PullCurrentValues;
		row.AddChild(sync);

		var resetSpeed = new Button { Text = Bi("Reset Speed", "重設速度") };
		resetSpeed.Pressed += () =>
		{
			Engine.TimeScale = Mathf.Clamp(DefaultTimeScale, 0.1f, 4.0f);
			_timeScaleSlider.Value = Engine.TimeScale;
			UpdateTimeScaleLabel();
		};
		row.AddChild(resetSpeed);

		return row;
	}

	private void PullCurrentValues()
	{
		ResolveRefs();

		if (_stabilitySystem != null && _timeSecondsInput != null)
			_timeSecondsInput.Value = Mathf.RoundToInt(_stabilitySystem.ElapsedSeconds);

		if (_walletInput != null)
			_walletInput.Value = MetaProgressionService.Instance.CurrencyWallet;

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
			SetUpgradeActionMessage(Bi("Invalid upgrade id.", "無效的升級 ID。"));
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

	private void SetMenuVisible(bool visible)
	{
		if (_panel == null)
			return;

		bool currentlyVisible = _panel.Visible;
		if (visible && !currentlyVisible)
		{
			_pauseStateBeforeDebugMenu = GetTree()?.Paused ?? false;
			if (!_pauseStateBeforeDebugMenu && GetTree() != null)
			{
				GetTree().Paused = true;
				_pausedByDebugMenu = true;
			}
			else
			{
				_pausedByDebugMenu = false;
			}
		}
		else if (!visible && currentlyVisible)
		{
			if (_pausedByDebugMenu && GetTree() != null)
				GetTree().Paused = _pauseStateBeforeDebugMenu;
			_pausedByDebugMenu = false;
		}

		_panel.Visible = visible;
	}

	private static string Bi(string en, string zh)
	{
		return $"{en} / {zh}";
	}

	private string ResolveZhByKey(string key, string fallback)
	{
		if (string.IsNullOrWhiteSpace(key))
			return fallback;

		EnsureCardZhLookupLoaded();
		if (_cardZhLookup.TryGetValue(key, out string zh) && !string.IsNullOrWhiteSpace(zh))
			return zh;

		return fallback;
	}

	private void EnsureCardZhLookupLoaded()
	{
		if (_cardZhLookupLoaded)
			return;

		_cardZhLookupLoaded = true;
		_cardZhLookup.Clear();

		if (!FileAccess.FileExists(CardLocalizationCsvPath))
			return;

		using FileAccess file = FileAccess.Open(CardLocalizationCsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
			return;

		while (!file.EofReached())
		{
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line))
				continue;
			if (line.StartsWith("keys,"))
				continue;
			if (!TrySplitCsv3(line, out string key, out _, out string zh))
				continue;
			if (string.IsNullOrWhiteSpace(key))
				continue;

			_cardZhLookup[key] = zh.Trim();
		}
	}

	private static bool TrySplitCsv3(string line, out string col0, out string col1, out string col2)
	{
		col0 = string.Empty;
		col1 = string.Empty;
		col2 = string.Empty;
		if (string.IsNullOrEmpty(line))
			return false;

		int first = line.IndexOf(',');
		int last = line.LastIndexOf(',');
		if (first <= 0 || last <= first)
			return false;

		col0 = line.Substring(0, first).Trim();
		col1 = line.Substring(first + 1, last - first - 1).Trim();
		col2 = line.Substring(last + 1).Trim();
		return true;
	}

	private static void ApplyLargeTextUi(Control root)
	{
		if (root == null)
			return;

		root.AddThemeFontSizeOverride("font_size", DebugUiFontSize);
		if (root is Label || root is Button || root is CheckBox || root is OptionButton || root is SpinBox)
		{
			Vector2 min = root.CustomMinimumSize;
			root.CustomMinimumSize = new Vector2(min.X, Mathf.Max(min.Y, DebugUiRowMinHeight));
		}

		foreach (Node child in root.GetChildren())
		{
			if (child is Control childControl)
				ApplyLargeTextUi(childControl);
		}
	}
}
