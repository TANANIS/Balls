using Godot;

public partial class GameFlowUI
{
	private void ApplyPaperUiSkin()
	{
		// Material/skin styling is scene-authored in .tscn.
		// Runtime only adjusts hierarchy/layout behavior.
		ApplyCharacterSelectBookLayout();
		ApplyOrderArchiveBookLayout();
	}

	private void ApplyCharacterSelectBookLayout()
	{
		Control characterSelectPanel = GetNodeOrNull<Control>("Panels/StartPanel/Panel/CharacterSelectPanel");
		VBoxContainer rootVBox = GetNodeOrNull<VBoxContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox");
		HBoxContainer contentRow = GetNodeOrNull<HBoxContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow");
		VBoxContainer heroColumn = GetNodeOrNull<VBoxContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn");
		Panel abilityPanel = GetNodeOrNull<Panel>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/AbilityTreePanel");
		GridContainer heroButtons = GetNodeOrNull<GridContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn/CharacterButtons");
		HBoxContainer bottomRow = GetNodeOrNull<HBoxContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/BottomRow");
		Panel detailPanel = GetNodeOrNull<Panel>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/BottomRow/DetailPanel");
		HBoxContainer actionButtons = GetNodeOrNull<HBoxContainer>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ActionButtons");

		if (characterSelectPanel == null || rootVBox == null || contentRow == null || heroColumn == null || abilityPanel == null || heroButtons == null || bottomRow == null || detailPanel == null || actionButtons == null)
			return;

		EnsureCharacterSelectSplitSheets(characterSelectPanel, rootVBox);

		// Left page (ability/details) + right page (hero grid)
		if (contentRow.GetChildCount() >= 2)
		{
			contentRow.MoveChild(abilityPanel, 0);
			contentRow.MoveChild(heroColumn, 1);
		}

		contentRow.CustomMinimumSize = new Vector2(0f, 300f);
		contentRow.AddThemeConstantOverride("separation", 8);

		abilityPanel.CustomMinimumSize = new Vector2(360f, 0f);
		abilityPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		heroColumn.CustomMinimumSize = new Vector2(300f, 0f);
		heroColumn.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		heroColumn.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		heroColumn.AddThemeConstantOverride("separation", 10);

		heroButtons.Columns = 2;
		heroButtons.AddThemeConstantOverride("separation", 10);
		foreach (Node node in heroButtons.GetChildren())
		{
			if (node is not Button button)
				continue;
			button.CustomMinimumSize = new Vector2(0f, 92f);
			button.AddThemeFontSizeOverride("font_size", 14);
		}

		bottomRow.CustomMinimumSize = new Vector2(0f, 170f);
		bottomRow.AddThemeConstantOverride("separation", 18);

		Control rightSpacer = bottomRow.GetNodeOrNull<Control>("RightSpacer");
		if (rightSpacer == null)
		{
			rightSpacer = new Control { Name = "RightSpacer", MouseFilter = Control.MouseFilterEnum.Ignore };
			bottomRow.AddChild(rightSpacer);
		}

		detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		rightSpacer.CustomMinimumSize = new Vector2(300f, 0f);
		rightSpacer.SizeFlagsHorizontal = Control.SizeFlags.Fill;

		Label selectedDesc = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/BottomRow/DetailPanel/Margin/DescScroll/SelectedCharacterDesc");
		if (selectedDesc != null)
		{
			selectedDesc.CustomMinimumSize = new Vector2(0f, 140f);
			selectedDesc.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			selectedDesc.AutowrapMode = TextServer.AutowrapMode.Word;
		}

		actionButtons.AddThemeConstantOverride("separation", 12);
		actionButtons.Alignment = BoxContainer.AlignmentMode.End;
		if (_startCharacterBackButton != null)
			_startCharacterBackButton.CustomMinimumSize = new Vector2(170f, 50f);
		if (_startCharacterConfirmButton != null)
			_startCharacterConfirmButton.CustomMinimumSize = new Vector2(220f, 50f);
	}

	private void EnsureCharacterSelectSplitSheets(Control characterSelectPanel, VBoxContainer rootVBox)
	{
		Panel leftSheet = characterSelectPanel.GetNodeOrNull<Panel>("LeftSheetBackdrop");
		Panel rightSheet = characterSelectPanel.GetNodeOrNull<Panel>("RightSheetBackdrop");
		if (leftSheet == null || rightSheet == null)
			return;

		characterSelectPanel.MoveChild(leftSheet, 0);
		characterSelectPanel.MoveChild(rightSheet, 1);
		characterSelectPanel.MoveChild(rootVBox, characterSelectPanel.GetChildCount() - 1);
		rootVBox.ZIndex = 20;
	}

	private void ApplyOrderArchiveBookLayout()
	{
		Control unlockPanel = _startEventUnlockPanel;
		Control unlockContent = GetNodeOrNull<Control>("Panels/StartPanel/Panel/EventUnlockPanel/VBox");
		EnsureSplitPaperBackdrop(unlockPanel, unlockContent, "ArchiveLeftSheetBackdrop", "ArchiveRightSheetBackdrop");

		Control loadoutPanel = _startEventLoadoutPanel;
		Control loadoutContent = GetNodeOrNull<Control>("Panels/StartPanel/Panel/EventLoadoutPanel/VBox");
		EnsureSplitPaperBackdrop(loadoutPanel, loadoutContent, "ArchiveLeftSheetBackdrop", "ArchiveRightSheetBackdrop");
	}

	private void EnsureSplitPaperBackdrop(Control panelRoot, Control contentRoot, string leftName, string rightName)
	{
		if (panelRoot == null || contentRoot == null)
			return;

		Panel leftSheet = panelRoot.GetNodeOrNull<Panel>(leftName);
		Panel rightSheet = panelRoot.GetNodeOrNull<Panel>(rightName);
		if (leftSheet == null || rightSheet == null)
			return;

		panelRoot.MoveChild(leftSheet, 0);
		panelRoot.MoveChild(rightSheet, 1);
		panelRoot.MoveChild(contentRoot, panelRoot.GetChildCount() - 1);
		contentRoot.ZIndex = 20;
	}

}
