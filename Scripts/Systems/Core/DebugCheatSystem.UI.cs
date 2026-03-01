using Godot;

public partial class DebugCheatSystem
{
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

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		_panel.AddChild(margin);

		var root = new VBoxContainer();
		root.AddThemeConstantOverride("separation", 8);
		margin.AddChild(root);

		var title = new Label { Text = Bi("DEBUG CHEAT PANEL (F3)", "除錯作弊面板 (F3)") };
		root.AddChild(title);

		root.AddChild(BuildTimeRow());
		root.AddChild(BuildWalletRow());
		root.AddChild(BuildDomainShardRow());
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
		apply.Pressed += () => _stabilitySystem?.DebugSetElapsedSeconds((float)_timeSecondsInput.Value);
		row.AddChild(apply);
		return row;
	}

	private Control BuildWalletRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Flux", "資源"), CustomMinimumSize = new Vector2(170, 0) });
		_walletInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true };
		_walletInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_walletInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += () => MetaProgressionService.Instance.DebugSetCurrencyWallet((int)_walletInput.Value, saveNow: true);
		row.AddChild(apply);
		return row;
	}

	private Control BuildDomainShardRow()
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 6);
		row.AddChild(new Label { Text = Bi("Shards", "碎片"), CustomMinimumSize = new Vector2(170, 0) });

		row.AddChild(new Label { Text = Bi("Ice", "冰"), CustomMinimumSize = new Vector2(48, 0) });
		_iceShardInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true, CustomMinimumSize = new Vector2(90, 0) };
		row.AddChild(_iceShardInput);

		row.AddChild(new Label { Text = Bi("Spacetime", "時空"), CustomMinimumSize = new Vector2(80, 0) });
		_spacetimeShardInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true, CustomMinimumSize = new Vector2(90, 0) };
		row.AddChild(_spacetimeShardInput);

		row.AddChild(new Label { Text = Bi("War", "戰爭"), CustomMinimumSize = new Vector2(48, 0) });
		_warShardInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true, CustomMinimumSize = new Vector2(90, 0) };
		row.AddChild(_warShardInput);

		var apply = new Button { Text = Bi("Apply", "套用") };
		apply.Pressed += () =>
		{
			MetaProgressionService.Instance.DebugSetDomainShardBalance("Ice", (int)_iceShardInput.Value, saveNow: false);
			MetaProgressionService.Instance.DebugSetDomainShardBalance("Spacetime", (int)_spacetimeShardInput.Value, saveNow: false);
			MetaProgressionService.Instance.DebugSetDomainShardBalance("War", (int)_warShardInput.Value, saveNow: true);
		};
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
		_noDamageToggle = new CheckBox { Text = Bi("No HP Loss", "不扣血") };
		_noDamageToggle.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_noDamageToggle.Toggled += OnNoDamageToggled;
		row.AddChild(_noDamageToggle);
		return row;
	}

	private Control BuildUpgradeDraftRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = Bi("Draft", "抽卡"), CustomMinimumSize = new Vector2(170, 0) });

		var open = new Button { Text = Bi("Open Draft", "開啟抽卡") };
		open.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		open.Pressed += () =>
		{
			ResolveRefs();
			bool opened = _progressionSystem?.DebugForceOpenUpgradeMenu() ?? false;
			SetUpgradeActionMessage(opened ? Bi("Draft opened.", "已開啟抽卡。") : Bi("Draft open failed.", "開啟抽卡失敗。"));
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

		var reload = new Button { Text = Bi("Reload", "重新整理") };
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

		var grant = new Button { Text = Bi("Grant+Open", "給予並開啟") };
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
			SetUpgradeActionMessage(Bi($"Granted upgrade level x{count}.", $"已給予升級 x{count}。"));
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

		var refresh = new Button { Text = Bi("Reload IDs", "重整ID") };
		refresh.Pressed += RefreshEnemyList;
		bottom.AddChild(refresh);
		row.AddChild(bottom);

		_spawnSourceLabel = new Label
		{
			Text = "Spawn source: -",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		row.AddChild(_spawnSourceLabel);

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
