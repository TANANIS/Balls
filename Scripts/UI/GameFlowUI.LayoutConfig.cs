using Godot;

public partial class GameFlowUI
{
	[ExportGroup("UI Layout/Dialog")]
	[Export] private Vector2I _startDeleteSaveDialogPopupSize = new(520, 220);

	[ExportGroup("UI Layout/Menu Background")]
	[Export(PropertyHint.Range, "0,64,1")] private float _menuBackgroundBleed = 8f;
	[Export] private bool _menuBackgroundSnapCenterToPixel = true;

	[ExportGroup("UI Layout/Event Loadout/Responsive")]
	[Export(PropertyHint.Range, "200,2000,1")] private float _eventLoadoutWidthFor4Columns = 700f;
	[Export(PropertyHint.Range, "120,2000,1")] private float _eventLoadoutWidthFor2Columns = 430f;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutResultFont4Columns = 18;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutResultFont2Columns = 24;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutResultFont1Column = 26;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutStatFont4Columns = 14;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutStatFontCompact = 18;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutTagFont4Columns = 12;
	[Export(PropertyHint.Range, "8,72,1")] private int _eventLoadoutTagFontCompact = 14;
	[Export(PropertyHint.Range, "0,240,1")] private float _eventLoadoutEventOptionMinWidthCompact = 120f;
	[Export(PropertyHint.Range, "24,96,1")] private float _eventLoadoutEventOptionHeight = 34f;
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutControlsSeparation = 4;

	[ExportGroup("UI Layout/Event Loadout/Stat Boxes")]
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutStatsSeparation = 6;
	[Export(PropertyHint.Range, "20,96,1")] private float _eventLoadoutStatPanelMinHeight = 36f;
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutStatMarginLeft = 6;
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutStatMarginTop = 2;
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutStatMarginRight = 6;
	[Export(PropertyHint.Range, "0,24,1")] private int _eventLoadoutStatMarginBottom = 2;

	[ExportGroup("UI Layout/Event Loadout/Roll Flash")]
	[Export(PropertyHint.Range, "8,96,1")] private int _eventLoadoutRollFlashFontSize = 30;
	[Export(PropertyHint.Range, "0,24,1")] private float _eventLoadoutRollFlashX = 6f;
	[Export(PropertyHint.Range, "0,64,1")] private float _eventLoadoutRollFlashWidthPadding = 12f;
	[Export(PropertyHint.Range, "16,256,1")] private float _eventLoadoutRollFlashHeight = 56f;
	[Export(PropertyHint.Range, "0.0,1.0,0.01")] private float _eventLoadoutRollFlashYFactor = 0.44f;
	[Export(PropertyHint.Range, "-128,128,1")] private float _eventLoadoutRollFlashYOffset = -28f;
	[Export(PropertyHint.Range, "0,24,1")] private float _eventLoadoutRollShakeDistance = 8f;
	[Export(PropertyHint.Range, "0.01,0.20,0.005")] private double _eventLoadoutRollStepSeconds = 0.038;
	[Export(PropertyHint.Range, "0.02,0.30,0.01")] private double _eventLoadoutRollRevealHoldSeconds = 0.09;

	[ExportGroup("UI Layout/Event Unlock/Table")]
	[Export(PropertyHint.Range, "0,40,1")] private int _eventUnlockSectionsSeparation = 14;
	[Export(PropertyHint.Range, "0,40,1")] private int _eventUnlockListSeparation = 10;
	[Export(PropertyHint.Range, "0,40,1")] private int _eventUnlockHeaderColumnSeparation = 14;
	[Export(PropertyHint.Range, "20,96,1")] private float _eventUnlockHeaderMinHeight = 30f;
	[Export(PropertyHint.Range, "40,220,1")] private float _eventUnlockCostColumnWidth = 108f;
	[Export(PropertyHint.Range, "40,220,1")] private float _eventUnlockStatusColumnWidth = 110f;
	[Export(PropertyHint.Range, "40,220,1")] private float _eventUnlockDetailColumnWidth = 84f;
	[Export(PropertyHint.Range, "40,220,1")] private float _eventUnlockActionColumnWidth = 94f;
	[Export(PropertyHint.Range, "20,120,1")] private float _eventUnlockDetailButtonHeight = 34f;
	[Export(PropertyHint.Range, "20,120,1")] private float _eventUnlockActionButtonHeightEvent = 42f;
	[Export(PropertyHint.Range, "20,120,1")] private float _eventUnlockActionButtonHeightHybrid = 34f;
	[Export(PropertyHint.Range, "0,40,1")] private int _eventUnlockEntryVSeparation = 4;
	[Export(PropertyHint.Range, "0,40,1")] private int _eventUnlockEntryHSeparation = 14;
	[Export(PropertyHint.Range, "20,120,1")] private float _eventUnlockEntryMinHeightEvent = 52f;
	[Export(PropertyHint.Range, "20,120,1")] private float _eventUnlockEntryMinHeightHybrid = 42f;
	[Export(PropertyHint.Range, "20,180,1")] private float _eventUnlockRowHeightCollapsed = 46f;
	[Export(PropertyHint.Range, "40,240,1")] private float _eventUnlockRowHeightExpanded = 122f;
	[Export(PropertyHint.Range, "0,40,1")] private float _eventUnlockRowGap = 10f;
	[Export(PropertyHint.Range, "0,240,1")] private float _eventUnlockSectionBaseHeight = 86f;
}
