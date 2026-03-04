using Godot;
using System;

public partial class StartCardsPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath BackdropDimPath = "BackdropDim";
	[Export] private NodePath TitlePath = "VBox/Title";
	[Export] private NodePath ContentPath = "VBox/CardsScroll/CardsContent";
	[Export] private NodePath BackButtonPath = "VBox/BackButton";

	[ExportGroup("Backdrop Dim FX")]
	[Export] private bool EnableBackdropDimFx = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BackdropDimAlpha = 0.46f;
	[Export(PropertyHint.Range, "0.05,1.0,0.01")] private float BackdropDimFadeInSeconds = 0.20f;

	public event Action BackPressed;

	public Button BackButton => _backButton;
	public Label ContentLabel => _cardsContentLabel;

	private Label _titleLabel;
	private Label _cardsContentLabel;
	private Button _backButton;
	private ColorRect _backdropDim;
	private Tween _backdropDimTween;
	private bool _backdropDimBaseColorCached;
	private Color _backdropDimBaseColor = new Color(0f, 0f, 0f, 1f);

	public override void _Ready()
	{
		ResolveNodeReferences();
		InitializeBackdropDimFx();
		BindSignals();
	}

	public override void _Notification(int what)
	{
		if (what != NotificationVisibilityChanged)
			return;

		if (IsVisibleInTree())
			PlayBackdropDimFx(expand: true, immediate: false);
		else
			PlayBackdropDimFx(expand: false, immediate: true);
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
		_backdropDim = GetNodeOrNull<ColorRect>(BackdropDimPath);
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_cardsContentLabel = GetNodeOrNull<Label>(ContentPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
	}

	private void InitializeBackdropDimFx()
	{
		if (_backdropDim == null)
			return;

		_backdropDim.MouseFilter = MouseFilterEnum.Ignore;
		_backdropDimBaseColor = _backdropDim.Color;
		_backdropDimBaseColor.A = 1f;
		_backdropDimBaseColorCached = true;
		ApplyBackdropDimAlpha(0f, visible: false);
	}

	private void PlayBackdropDimFx(bool expand, bool immediate)
	{
		if (_backdropDim == null)
			return;

		if (_backdropDimTween != null && _backdropDimTween.IsValid())
			_backdropDimTween.Kill();
		_backdropDimTween = null;

		float targetAlpha = expand && EnableBackdropDimFx
			? Mathf.Clamp(BackdropDimAlpha, 0f, 1f)
			: 0f;

		if (immediate || !IsInsideTree() || !EnableBackdropDimFx)
		{
			ApplyBackdropDimAlpha(targetAlpha, visible: targetAlpha > 0.001f);
			return;
		}

		Color color = _backdropDim.Color;
		float currentAlpha = Mathf.Clamp(color.A, 0f, 1f);
		if (Mathf.IsEqualApprox(currentAlpha, targetAlpha))
		{
			_backdropDim.Visible = targetAlpha > 0.001f;
			return;
		}

		_backdropDim.Visible = targetAlpha > 0.001f || currentAlpha > 0.001f;
		_backdropDimTween = CreateTween();
		_backdropDimTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_backdropDimTween.TweenProperty(
			_backdropDim,
			"color:a",
			targetAlpha,
			Mathf.Max(0.05f, BackdropDimFadeInSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(targetAlpha >= currentAlpha ? Tween.EaseType.Out : Tween.EaseType.InOut);
		_backdropDimTween.TweenCallback(Callable.From(() =>
		{
			_backdropDimTween = null;
			if (_backdropDim != null && targetAlpha <= 0.001f)
				_backdropDim.Visible = false;
		}));
	}

	private void ApplyBackdropDimAlpha(float alpha, bool visible)
	{
		if (_backdropDim == null)
			return;

		Color color = _backdropDimBaseColorCached ? _backdropDimBaseColor : _backdropDim.Color;
		color.A = Mathf.Clamp(alpha, 0f, 1f);
		_backdropDim.Color = color;
		_backdropDim.Visible = visible;
	}

	private void BindSignals()
	{
		if (_backButton != null)
			_backButton.Pressed += () => BackPressed?.Invoke();
	}
}
