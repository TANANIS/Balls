using Godot;
using System;
using System.Collections.Generic;

public partial class GameFlowUI
{
	[ExportGroup("Node Paths/Event Unlock")]
	[Export] private NodePath StartEventUnlockPanelPath = "Panels/StartPanel/EventUnlockPanel";
	[Export] private NodePath StartEventUnlockTitlePath = "Panels/StartPanel/EventUnlockPanel/VBox/HeaderRow/Title";
	[Export] private NodePath StartEventUnlockWalletPath = "Panels/StartPanel/EventUnlockPanel/VBox/HeaderRow/ShardWallet";
	[Export] private NodePath StartEventUnlockIntroPath = "Panels/StartPanel/EventUnlockPanel/VBox/Intro";
	[Export] private NodePath StartEventUnlockEventSectionTitlePath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/EventSection/Margin/VBox/SectionTitle";
	[Export] private NodePath StartEventUnlockEventListPath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/EventSection/Margin/VBox/EventList";
	[Export] private NodePath StartEventUnlockEventSectionPath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/EventSection";
	[Export] private NodePath StartEventUnlockHybridSectionTitlePath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/HybridSection/Margin/VBox/SectionTitle";
	[Export] private NodePath StartEventUnlockHybridListPath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/HybridSection/Margin/VBox/HybridList";
	[Export] private NodePath StartEventUnlockHybridSectionPath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox/HybridSection";
	[Export] private NodePath StartEventUnlockSectionsVBoxPath = "Panels/StartPanel/EventUnlockPanel/VBox/SectionsScroll/SectionsVBox";
	[Export] private NodePath StartEventUnlockBackButtonPath = "Panels/StartPanel/EventUnlockPanel/VBox/ActionButtons/BackButton";
	[Export] private NodePath StartEventUnlockContinueButtonPath = "Panels/StartPanel/EventUnlockPanel/VBox/ActionButtons/ContinueButton";

	private static readonly string[] PurchasableDomainIds =
	{
		"Ice",
		"Spacetime",
		"War"
	};

	private static readonly string[] UnlockHybridVariantIds =
	{
		"HYB_ICE_SPACE_GLACIAL_HORIZON",
		"HYB_SPACE_WAR_WARP_ASSAULT"
	};

	private sealed class UnlockEntryRow
	{
		public string EntryId;
		public bool IsHybrid;
		public VBoxContainer Root;
		public HBoxContainer HeaderRow;
		public Label DetailLabel;
		public Button DetailToggleButton;
		public Label NameLabel;
		public Label CostLabel;
		public Label StatusLabel;
		public Button ActionButton;
	}

	private Control _startEventUnlockPanel;
	private Label _startEventUnlockTitleLabel;
	private Label _startEventUnlockWalletLabel;
	private Label _startEventUnlockIntroLabel;
	private Label _startEventUnlockEventSectionTitleLabel;
	private Label _startEventUnlockHybridSectionTitleLabel;
	private VBoxContainer _startEventUnlockSectionsVBox;
	private Control _startEventUnlockEventSectionPanel;
	private Control _startEventUnlockHybridSectionPanel;
	private VBoxContainer _startEventUnlockEventList;
	private VBoxContainer _startEventUnlockHybridList;
	private Button _startEventUnlockBackButton;
	private Button _startEventUnlockContinueButton;
	private bool _startEventUnlockOpen;
	private readonly List<UnlockEntryRow> _eventUnlockRows = new();
	private readonly List<UnlockEntryRow> _hybridUnlockRows = new();
	private readonly Dictionary<string, bool> _eventUnlockExpandedByEntryId = new(StringComparer.Ordinal);

	private void ResolveEventUnlockNodes()
	{
		_startEventUnlockPanel = GetNodeOrNull<Control>(StartEventUnlockPanelPath);
		_startEventUnlockTitleLabel = GetNodeOrNull<Label>(StartEventUnlockTitlePath);
		_startEventUnlockWalletLabel = GetNodeOrNull<Label>(StartEventUnlockWalletPath);
		_startEventUnlockIntroLabel = GetNodeOrNull<Label>(StartEventUnlockIntroPath);
		_startEventUnlockEventSectionTitleLabel = GetNodeOrNull<Label>(StartEventUnlockEventSectionTitlePath);
		_startEventUnlockHybridSectionTitleLabel = GetNodeOrNull<Label>(StartEventUnlockHybridSectionTitlePath);
		_startEventUnlockSectionsVBox = GetNodeOrNull<VBoxContainer>(StartEventUnlockSectionsVBoxPath);
		_startEventUnlockEventSectionPanel = GetNodeOrNull<Control>(StartEventUnlockEventSectionPath);
		_startEventUnlockHybridSectionPanel = GetNodeOrNull<Control>(StartEventUnlockHybridSectionPath);
		_startEventUnlockEventList = GetNodeOrNull<VBoxContainer>(StartEventUnlockEventListPath);
		_startEventUnlockHybridList = GetNodeOrNull<VBoxContainer>(StartEventUnlockHybridListPath);
		_startEventUnlockBackButton = GetNodeOrNull<Button>(StartEventUnlockBackButtonPath);
		_startEventUnlockContinueButton = GetNodeOrNull<Button>(StartEventUnlockContinueButtonPath);

		_startEventUnlockSectionsVBox?.AddThemeConstantOverride("separation", _eventUnlockSectionsSeparation);
		_startEventUnlockEventList?.AddThemeConstantOverride("separation", _eventUnlockListSeparation);
		_startEventUnlockHybridList?.AddThemeConstantOverride("separation", _eventUnlockListSeparation);

		EnsureEventUnlockRowsBuilt();
		UpdateEventUnlockSectionMinHeights();
		RefreshEventUnlockUi();
	}

	private void BindEventUnlockSignals()
	{
		if (_startEventUnlockBackButton != null)
			_startEventUnlockBackButton.Pressed += OnEventUnlockBackPressed;
		if (_startEventUnlockContinueButton != null)
			_startEventUnlockContinueButton.Pressed += OnEventUnlockContinuePressed;
	}

	private void EnterEventUnlockPanel()
	{
		if (_startEventUnlockPanel == null)
		{
			EnterEventLoadout();
			return;
		}

		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startControlsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventLoadoutOpen = false;
		_startEventUnlockOpen = true;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: false, showEventLoadout: false, showEventUnlock: true);
		RefreshEventUnlockUi();
		_startEventUnlockContinueButton?.GrabFocus();
	}

	private void OnEventUnlockBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startControlsOpen = false;
		_startEventUnlockOpen = false;
		_startCharacterSelectOpen = true;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: true, showEventLoadout: false, showEventUnlock: false);
		_startCharacterConfirmButton?.GrabFocus();
	}

	private void OnEventUnlockContinuePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		EnterEventLoadout();
	}

	private void EnsureEventUnlockRowsBuilt()
	{
		if (_startEventUnlockEventList != null && _eventUnlockRows.Count == 0)
		{
			CreateUnlockListHeader(_startEventUnlockEventList);
			foreach (string domainId in PurchasableDomainIds)
				_eventUnlockRows.Add(CreateUnlockEntryRow(_startEventUnlockEventList, domainId, isHybrid: false));
		}

		if (_startEventUnlockHybridList != null && _hybridUnlockRows.Count == 0)
		{
			CreateUnlockListHeader(_startEventUnlockHybridList);
			foreach (string variantId in UnlockHybridVariantIds)
				_hybridUnlockRows.Add(CreateUnlockEntryRow(_startEventUnlockHybridList, variantId, isHybrid: true));
		}

		UpdateEventUnlockSectionMinHeights();
	}

	private void CreateUnlockListHeader(VBoxContainer parent)
	{
		if (parent == null)
			return;

		var header = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, _eventUnlockHeaderMinHeight),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		header.AddThemeConstantOverride("separation", _eventUnlockHeaderColumnSeparation);

		var nameHeader = new Label
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			Text = TrOrDefault("UI.META.EVENT_UNLOCK.COL_NAME", "Event", "事件")
		};
		nameHeader.AddThemeColorOverride("font_color", new Color(0.90f, 0.86f, 0.76f, 0.92f));
		nameHeader.AddThemeFontSizeOverride("font_size", 13);
		header.AddChild(nameHeader);

		var costHeader = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockCostColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Right,
			Text = TrOrDefault("UI.META.EVENT_UNLOCK.COL_COST", "Cost", "花費")
		};
		costHeader.AddThemeColorOverride("font_color", new Color(0.70f, 0.88f, 1f, 0.95f));
		costHeader.AddThemeFontSizeOverride("font_size", 13);
		header.AddChild(costHeader);

		var statusHeader = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockStatusColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Right,
			Text = TrOrDefault("UI.META.EVENT_UNLOCK.COL_STATUS", "Status", "狀態")
		};
		statusHeader.AddThemeColorOverride("font_color", new Color(0.90f, 0.84f, 0.72f, 0.92f));
		statusHeader.AddThemeFontSizeOverride("font_size", 13);
		header.AddChild(statusHeader);

		var detailHeader = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockDetailColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Center,
			Text = TrOrDefault("UI.META.EVENT_UNLOCK.COL_DETAIL", "Detail", "詳情")
		};
		detailHeader.AddThemeColorOverride("font_color", new Color(0.90f, 0.84f, 0.72f, 0.92f));
		detailHeader.AddThemeFontSizeOverride("font_size", 13);
		header.AddChild(detailHeader);

		var actionHeader = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockActionColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Center,
			Text = TrOrDefault("UI.META.EVENT_UNLOCK.COL_ACTION", "Action", "操作")
		};
		actionHeader.AddThemeColorOverride("font_color", new Color(0.90f, 0.84f, 0.72f, 0.92f));
		actionHeader.AddThemeFontSizeOverride("font_size", 13);
		header.AddChild(actionHeader);

		parent.AddChild(header);
	}

	private UnlockEntryRow CreateUnlockEntryRow(VBoxContainer parent, string entryId, bool isHybrid)
	{
		var root = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, isHybrid ? _eventUnlockEntryMinHeightHybrid : _eventUnlockEntryMinHeightEvent),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		root.AddThemeConstantOverride("separation", _eventUnlockEntryVSeparation);

		var row = new HBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		row.AddThemeConstantOverride("separation", _eventUnlockEntryHSeparation);
		root.AddChild(row);

		var nameLabel = new Label
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			ClipText = isHybrid
		};
		nameLabel.AutowrapMode = TextServer.AutowrapMode.Off;
		nameLabel.VerticalAlignment = VerticalAlignment.Center;
		nameLabel.AddThemeColorOverride("font_color", new Color(0.94f, 0.89f, 0.80f, 1f));
		nameLabel.AddThemeFontSizeOverride("font_size", 15);
		row.AddChild(nameLabel);

		var costLabel = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockCostColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Right
		};
		costLabel.AddThemeColorOverride("font_color", new Color(0.56f, 0.83f, 1f, 1f));
		costLabel.AddThemeFontSizeOverride("font_size", 14);
		row.AddChild(costLabel);

		var statusLabel = new Label
		{
			CustomMinimumSize = new Vector2(_eventUnlockStatusColumnWidth, 0f),
			HorizontalAlignment = HorizontalAlignment.Right
		};
		statusLabel.AddThemeColorOverride("font_color", new Color(0.87f, 0.80f, 0.67f, 1f));
		statusLabel.AddThemeFontSizeOverride("font_size", 13);
		row.AddChild(statusLabel);

		Button detailToggle = null;
		if (!isHybrid)
		{
			detailToggle = new Button
			{
				CustomMinimumSize = new Vector2(_eventUnlockDetailColumnWidth, _eventUnlockDetailButtonHeight)
			};
			detailToggle.Pressed += () => OnEventUnlockDetailTogglePressed(entryId);
			row.AddChild(detailToggle);
		}
		else
		{
			var spacer = new Control
			{
				CustomMinimumSize = new Vector2(_eventUnlockDetailColumnWidth, _eventUnlockDetailButtonHeight)
			};
			row.AddChild(spacer);
		}

		var actionButton = new Button
		{
			CustomMinimumSize = new Vector2(
				_eventUnlockActionColumnWidth,
				isHybrid ? _eventUnlockActionButtonHeightHybrid : _eventUnlockActionButtonHeightEvent)
		};
		actionButton.Pressed += () => OnUnlockEntryPressed(entryId, isHybrid);
		row.AddChild(actionButton);

		Label detailLabel = null;
		if (!isHybrid)
		{
			detailLabel = new Label
			{
				Visible = false,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			detailLabel.AddThemeColorOverride("font_color", new Color(0.86f, 0.80f, 0.71f, 0.92f));
			detailLabel.AddThemeFontSizeOverride("font_size", 14);
			root.AddChild(detailLabel);
		}

		parent.AddChild(root);
		return new UnlockEntryRow
		{
			EntryId = entryId,
			IsHybrid = isHybrid,
			Root = root,
			HeaderRow = row,
			DetailLabel = detailLabel,
			DetailToggleButton = detailToggle,
			NameLabel = nameLabel,
			CostLabel = costLabel,
			StatusLabel = statusLabel,
			ActionButton = actionButton
		};
	}

	private void UpdateEventUnlockSectionMinHeights()
	{
		UpdateSingleUnlockSectionMinHeight(_startEventUnlockEventSectionPanel, _eventUnlockRows);
		UpdateSingleUnlockSectionMinHeight(_startEventUnlockHybridSectionPanel, _hybridUnlockRows);
	}

	private void UpdateSingleUnlockSectionMinHeight(Control sectionPanel, List<UnlockEntryRow> rows)
	{
		if (sectionPanel == null)
			return;

		int safeRows = Math.Max(0, rows?.Count ?? 0);
		float dynamicRows = 0f;
		if (rows != null)
		{
			foreach (UnlockEntryRow row in rows)
			{
				if (row == null || row.IsHybrid || row.DetailLabel == null || !row.DetailLabel.Visible)
				{
					dynamicRows += _eventUnlockRowHeightCollapsed;
					continue;
				}

				dynamicRows += _eventUnlockRowHeightExpanded;
			}
		}
		else
		{
			dynamicRows = safeRows * _eventUnlockRowHeightCollapsed;
		}

		float minHeight = _eventUnlockSectionBaseHeight
			+ _eventUnlockHeaderMinHeight
			+ dynamicRows
			+ (Math.Max(0, safeRows - 1) * _eventUnlockRowGap);
		sectionPanel.CustomMinimumSize = new Vector2(sectionPanel.CustomMinimumSize.X, minHeight);
	}

	private void OnEventUnlockDetailTogglePressed(string entryId)
	{
		if (string.IsNullOrWhiteSpace(entryId))
			return;

		bool expanded = false;
		if (_eventUnlockExpandedByEntryId.TryGetValue(entryId, out bool existing))
			expanded = existing;
		_eventUnlockExpandedByEntryId[entryId] = !expanded;
		RefreshEventUnlockUi();
	}

	private void OnUnlockEntryPressed(string entryId, bool isHybrid)
	{
		bool success = isHybrid
			? MetaProgressionService.Instance.TryUnlockHybridVariant(entryId)
			: MetaProgressionService.Instance.TryPurchaseDomainPower(entryId, 1);
		if (!success)
		{
			AudioManager.Instance?.PlaySfxUiExit();
			RefreshEventUnlockUi();
			return;
		}

		AudioManager.Instance?.PlaySfxUiButton();
		RefreshEventUnlockUi();
		RefreshEventLoadoutUi();
	}

}
