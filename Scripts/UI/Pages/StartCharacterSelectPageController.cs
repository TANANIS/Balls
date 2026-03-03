using Godot;
using System;

public enum StartCharacterSlotKind
{
	Ranged = 0,
	Swordsman = 1,
	Tank = 2,
	Archer = 3
}

public partial class StartCharacterSelectPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath TitlePath = "VBox/HeaderRow/Title";
	[Export] private NodePath FluxLabelPath = "VBox/HeaderRow/FluxHeader/FluxLabel";
	[Export] private NodePath FluxValuePath = "VBox/HeaderRow/FluxHeader/FluxValue";
	[Export] private NodePath AbilityTreeGraphPath = "VBox/ContentRow/AbilityTreePanel/Margin/AbilityTreeVBox/AbilityTreeGraph";
	[Export] private NodePath DescriptionPath = "VBox/BottomRow/DetailPanel/Margin/DescScroll/SelectedCharacterDesc";
	[Export] private NodePath RangedButtonPath = "VBox/ContentRow/LeftColumn/CharacterButtons/RangedButton";
	[Export] private NodePath SwordsmanButtonPath = "VBox/ContentRow/LeftColumn/CharacterButtons/SwordsmanButton";
	[Export] private NodePath TankButtonPath = "VBox/ContentRow/LeftColumn/CharacterButtons/TankButton";
	[Export] private NodePath ArcherButtonPath = "VBox/ContentRow/LeftColumn/CharacterButtons/ArcherButton";
	[Export] private NodePath BackButtonPath = "VBox/ActionButtons/BackButton";
	[Export] private NodePath ConfirmButtonPath = "VBox/ActionButtons/ConfirmButton";

	public event Action RangedPressed;
	public event Action SwordsmanPressed;
	public event Action TankPressed;
	public event Action ArcherPressed;
	public event Action BackPressed;
	public event Action ConfirmPressed;

	public Button RangedButton => _rangedButton;
	public Button SwordsmanButton => _swordsmanButton;
	public Button TankButton => _tankButton;
	public Button ArcherButton => _archerButton;
	public Button BackButton => _backButton;
	public Button ConfirmButton => _confirmButton;
	public Label FluxValueLabel => _fluxValueLabel;
	public Label DescriptionLabel => _descriptionLabel;

	private Label _titleLabel;
	private Label _fluxLabel;
	private Label _fluxValueLabel;
	private Label _abilityTreeGraphLabel;
	private Label _descriptionLabel;
	private Button _rangedButton;
	private Button _swordsmanButton;
	private Button _tankButton;
	private Button _archerButton;
	private Button _backButton;
	private Button _confirmButton;

	public override void _Ready()
	{
		ResolveNodeReferences();
		BindSignals();
	}

	public void ApplyLocalizedStaticTexts()
	{
		if (_titleLabel != null)
			_titleLabel.Text = Tr("UI.META.TITLE");
		if (_fluxLabel != null)
			_fluxLabel.Text = $"{Tr("UI.META.FLUX")}:";
		if (_abilityTreeGraphLabel != null)
			_abilityTreeGraphLabel.Text = Tr("UI.META.NOT_AVAILABLE");
		if (_backButton != null)
			_backButton.Text = Tr("UI.COMMON.BACK");
	}

	public void SetFluxValue(int value)
	{
		if (_fluxValueLabel != null)
			_fluxValueLabel.Text = value.ToString();
	}

	public void SetDescription(string text)
	{
		if (_descriptionLabel != null)
			_descriptionLabel.Text = text ?? string.Empty;
	}

	public void SetCharacterButton(StartCharacterSlotKind kind, string text, bool disabled)
	{
		Button button = kind switch
		{
			StartCharacterSlotKind.Ranged => _rangedButton,
			StartCharacterSlotKind.Swordsman => _swordsmanButton,
			StartCharacterSlotKind.Tank => _tankButton,
			StartCharacterSlotKind.Archer => _archerButton,
			_ => null
		};
		if (button == null)
			return;

		button.Text = text ?? string.Empty;
		button.Disabled = disabled;
	}

	public void SetConfirmButton(string text, bool disabled)
	{
		if (_confirmButton == null)
			return;

		_confirmButton.Text = text ?? string.Empty;
		_confirmButton.Disabled = disabled;
	}

	public void FocusConfirmButton()
	{
		_confirmButton?.GrabFocus();
	}

	public void FocusBackButton()
	{
		_backButton?.GrabFocus();
	}

	private void ResolveNodeReferences()
	{
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_fluxLabel = GetNodeOrNull<Label>(FluxLabelPath);
		_fluxValueLabel = GetNodeOrNull<Label>(FluxValuePath);
		_abilityTreeGraphLabel = GetNodeOrNull<Label>(AbilityTreeGraphPath);
		_descriptionLabel = GetNodeOrNull<Label>(DescriptionPath);
		_rangedButton = GetNodeOrNull<Button>(RangedButtonPath);
		_swordsmanButton = GetNodeOrNull<Button>(SwordsmanButtonPath);
		_tankButton = GetNodeOrNull<Button>(TankButtonPath);
		_archerButton = GetNodeOrNull<Button>(ArcherButtonPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
		_confirmButton = GetNodeOrNull<Button>(ConfirmButtonPath);
	}

	private void BindSignals()
	{
		if (_rangedButton != null)
			_rangedButton.Pressed += () => RangedPressed?.Invoke();
		if (_swordsmanButton != null)
			_swordsmanButton.Pressed += () => SwordsmanPressed?.Invoke();
		if (_tankButton != null)
			_tankButton.Pressed += () => TankPressed?.Invoke();
		if (_archerButton != null)
			_archerButton.Pressed += () => ArcherPressed?.Invoke();
		if (_backButton != null)
			_backButton.Pressed += () => BackPressed?.Invoke();
		if (_confirmButton != null)
			_confirmButton.Pressed += () => ConfirmPressed?.Invoke();
	}
}
