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
}
