using Godot;

public partial class GameFlowUI
{
	[ExportGroup("Node Paths/Localization")]
	[Export] private NodePath StartTitleLabelPath = "Panels/StartPanel/Panel/MainScroll/VBox/Header/Title";
	[Export] private NodePath StartSubtitleLabelPath = "Panels/StartPanel/Panel/MainScroll/VBox/Header/SubTitle";
	[Export] private NodePath StartDescriptionLabelPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/Desc";
	[Export] private NodePath StartPerfectBoardTitleLabelPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/PerfectBoardTitle";
	[Export] private NodePath StartCharacterSelectTitleLabelPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/HeaderRow/Title";
	[Export] private NodePath StartCharacterFluxLabelPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/HeaderRow/FluxHeader/FluxLabel";
	[Export] private NodePath StartAbilityTreeGraphLabelPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/AbilityTreePanel/Margin/AbilityTreeVBox/AbilityTreeGraph";
	[Export] private NodePath StartSettingsTitleLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/Title";
	[Export] private NodePath StartCardsTitleLabelPath = "Panels/StartPanel/Panel/CardsPanel/VBox/Title";
	[Export] private NodePath PauseTitleLabelPath = "Panels/PausePanel/Panel/VBox/Title";
	[Export] private NodePath PauseSettingsTitleLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/Title";
	[Export] private NodePath StartBgmLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmLabel";
	[Export] private NodePath StartSfxLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxLabel";
	[Export] private NodePath StartWindowModeLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeLabel";
	[Export] private NodePath StartWindowSizeLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeLabel";
	[Export] private NodePath StartLanguageLabelPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageLabel";
	[Export] private NodePath PauseBgmLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmLabel";
	[Export] private NodePath PauseSfxLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxLabel";
	[Export] private NodePath PauseWindowModeLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeLabel";
	[Export] private NodePath PauseWindowSizeLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeLabel";
	[Export] private NodePath PauseLanguageLabelPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageLabel";
}
