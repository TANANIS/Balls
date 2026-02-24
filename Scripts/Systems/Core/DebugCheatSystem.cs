using Godot;
using System;

public partial class DebugCheatSystem : Node
{
	[Export] public bool EnableDebugMenu = false;
	[Export] public bool StartVisible = false;
	[Export(PropertyHint.Range, "0.1,4.0,0.05")] public float DefaultTimeScale = 1.0f;
	[Export] public NodePath PlayerPath = "../../Player";

	private CanvasLayer _layer;
	private PanelContainer _panel;
	private SpinBox _timeSecondsInput;
	private SpinBox _walletInput;
	private SpinBox _hpInput;
	private OptionButton _enemyIdOption;
	private SpinBox _spawnCountInput;
	private HSlider _timeScaleSlider;
	private Label _timeScaleValueLabel;

	private PlayerHealth _playerHealth;
	private StabilitySystem _stabilitySystem;
	private SpawnSystem _spawnSystem;

	public override void _Ready()
	{
		if (!EnableDebugMenu)
			return;

		// Prevent stale timescale from previous test sessions.
		Engine.TimeScale = Mathf.Clamp(DefaultTimeScale, 0.1f, 4.0f);
		ResolveRefs();
		BuildUi();
		RefreshEnemyList();
		PullCurrentValues();
		SetMenuVisible(StartVisible);
	}

	public override void _ExitTree()
	{
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
			OffsetRight = 420f,
			OffsetBottom = 470f
		};
		_layer.AddChild(_panel);

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

		var title = new Label { Text = "DEBUG MODIFIER (F3)" };
		root.AddChild(title);

		root.AddChild(BuildTimeRow());
		root.AddChild(BuildWalletRow());
		root.AddChild(BuildHpRow());
		root.AddChild(BuildSpawnRow());
		root.AddChild(BuildSpeedRow());
		root.AddChild(BuildBottomButtons());
	}

	private Control BuildTimeRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = "Time(sec)", CustomMinimumSize = new Vector2(90, 0) });
		_timeSecondsInput = new SpinBox { MinValue = 0, MaxValue = 36000, Step = 1, Rounded = true };
		_timeSecondsInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_timeSecondsInput);

		var apply = new Button { Text = "Apply" };
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
		row.AddChild(new Label { Text = "Flux", CustomMinimumSize = new Vector2(90, 0) });
		_walletInput = new SpinBox { MinValue = 0, MaxValue = 9999999, Step = 10, Rounded = true };
		_walletInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_walletInput);

		var apply = new Button { Text = "Apply" };
		apply.Pressed += () => MetaProgressionService.Instance.DebugSetCurrencyWallet((int)_walletInput.Value, saveNow: true);
		row.AddChild(apply);
		return row;
	}

	private Control BuildHpRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = "HP", CustomMinimumSize = new Vector2(90, 0) });
		_hpInput = new SpinBox { MinValue = 0, MaxValue = 999, Step = 1, Rounded = true };
		_hpInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(_hpInput);

		var apply = new Button { Text = "Apply" };
		apply.Pressed += () => _playerHealth?.DebugSetCurrentHp((int)_hpInput.Value);
		row.AddChild(apply);
		return row;
	}

	private Control BuildSpawnRow()
	{
		var row = new VBoxContainer();

		var top = new HBoxContainer();
		top.AddChild(new Label { Text = "Spawn", CustomMinimumSize = new Vector2(90, 0) });
		_enemyIdOption = new OptionButton();
		_enemyIdOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		top.AddChild(_enemyIdOption);
		row.AddChild(top);

		var bottom = new HBoxContainer();
		bottom.AddChild(new Label { Text = "Count", CustomMinimumSize = new Vector2(90, 0) });
		_spawnCountInput = new SpinBox { MinValue = 1, MaxValue = 64, Step = 1, Rounded = true, Value = 1 };
		_spawnCountInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		bottom.AddChild(_spawnCountInput);

		var spawn = new Button { Text = "Spawn" };
		spawn.Pressed += SpawnRequestedEnemy;
		bottom.AddChild(spawn);

		var refresh = new Button { Text = "Reload IDs" };
		refresh.Pressed += RefreshEnemyList;
		bottom.AddChild(refresh);
		row.AddChild(bottom);

		return row;
	}

	private Control BuildSpeedRow()
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = "Speed", CustomMinimumSize = new Vector2(90, 0) });
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

		var sync = new Button { Text = "Sync Values" };
		sync.Pressed += PullCurrentValues;
		row.AddChild(sync);

		var resetSpeed = new Button { Text = "Reset Speed" };
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
		_panel.Visible = visible;
	}
}
