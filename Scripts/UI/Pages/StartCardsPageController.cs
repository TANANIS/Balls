using Godot;
using System;

public partial class StartCardsPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath TitlePath = "VBox/Title";
	[Export] private NodePath ContentPath = "VBox/CardsScroll/CardsContent";
	[Export] private NodePath BackButtonPath = "VBox/BackButton";

	public event Action BackPressed;

	public Button BackButton => _backButton;
	public Label ContentLabel => _cardsContentLabel;

	private Label _titleLabel;
	private Label _cardsContentLabel;
	private Button _backButton;

	public override void _Ready()
	{
		ResolveNodeReferences();
		BindSignals();
	}

	public void FocusBackButton()
	{
		_backButton?.GrabFocus();
	}

	public void SetCardsContent(string text)
	{
		if (_cardsContentLabel != null)
			_cardsContentLabel.Text = text ?? string.Empty;
	}

	public void ApplyLocalizedTexts()
	{
		if (_titleLabel != null)
			_titleLabel.Text = Tr("UI.START.CARDS_TITLE");
		if (_backButton != null)
			_backButton.Text = Tr("UI.COMMON.BACK");
	}

	private void ResolveNodeReferences()
	{
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_cardsContentLabel = GetNodeOrNull<Label>(ContentPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
	}

	private void BindSignals()
	{
		if (_backButton != null)
			_backButton.Pressed += () => BackPressed?.Invoke();
	}
}
