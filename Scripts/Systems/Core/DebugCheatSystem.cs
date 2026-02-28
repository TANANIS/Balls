using Godot;
using System;
using System.Collections.Generic;
using System.Text;

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

		var stabilityList = GetTree().GetNodesInGroup(RuntimeGroups.StabilitySystem);
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		var spawnList = GetTree().GetNodesInGroup(RuntimeGroups.SpawnSystem);
		if (spawnList.Count > 0)
			_spawnSystem = spawnList[0] as SpawnSystem;

		var progressionList = GetTree().GetNodesInGroup(RuntimeGroups.ProgressionSystem);
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		var upgradeList = GetTree().GetNodesInGroup(RuntimeGroups.UpgradeSystem);
		if (upgradeList.Count > 0)
			_upgradeSystem = upgradeList[0] as UpgradeSystem;
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

		List<string> cols = SplitCsvLine(line);
		if (cols.Count < 3)
			return false;

		col0 = cols[0].Trim();
		col1 = cols[1].Trim();
		col2 = cols[2].Trim();
		return true;
	}

	private static List<string> SplitCsvLine(string line)
	{
		var cols = new List<string>();
		if (line == null)
			return cols;

		var sb = new StringBuilder(line.Length);
		bool inQuotes = false;
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				// Escaped quote inside quoted field: ""
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					sb.Append('"');
					i++;
					continue;
				}

				inQuotes = !inQuotes;
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				cols.Add(sb.ToString());
				sb.Clear();
				continue;
			}

			sb.Append(c);
		}

		cols.Add(sb.ToString());
		return cols;
	}
}
