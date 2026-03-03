using Godot;

public partial class GameFlowUI
{
	[ExportGroup("Title Branding/Node Paths")]
	[Export] private NodePath BootTitleLogoPath = "Panels/TitleScreen/Center/VBox/Title";
	[Export] private NodePath BootTitleSubtitlePath = "Panels/TitleScreen/Center/VBox/SubTitle";
	[Export] private NodePath BootTitlePromptPath = "Panels/TitleScreen/Center/VBox/PressAnyButton";
	[Export] private NodePath StartMenuLogoPath = "Panels/StartPanel/MainScroll/VBox/Header/Title";
	[Export] private NodePath BootBackgroundPath = "Panels/TitleScreen/Background";
	[Export] private NodePath BootTopLetterboxPath = "Panels/TitleScreen/TopLetterbox";
	[Export] private NodePath BootBottomLetterboxPath = "Panels/TitleScreen/BottomLetterbox";
	[Export] private NodePath BootOpeningMaskPath = "Panels/TitleScreen/OpeningMask";

	[ExportGroup("Title Branding/Logo")]
	[Export] private Texture2D SharedTitleLogoTexture;
	[Export] private Vector2 BootTitleLogoMinSize = new Vector2(0f, 270f);
	[Export] private Vector2 StartMenuLogoMinSize = new Vector2(0f, 120f);

	[ExportGroup("Title Branding/Letterbox")]
	[Export] private bool EnableBootLetterbox = true;
	[Export] private float BootLetterboxHeight = 90f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootLetterboxAlpha = 1f;

	[ExportGroup("Title Branding/Text Overrides")]
	[Export] private bool UseInspectorBootSubtitleText;
	[Export(PropertyHint.MultilineText)] private string BootSubtitleText = "Hold the line for 15 minutes in a collapsing universe.";
	[Export] private bool UseInspectorBootPromptText;
	[Export] private string BootPromptText = "Press Any Button";
	[Export] private bool UseInspectorStartSubtitleText;
	[Export(PropertyHint.MultilineText)] private string StartSubtitleText = "Survive the collapsing universe and build your strategy.";

	private void ApplyTitleBrandingOverrides()
	{
		ApplyTitleLogoOverride(GetNodeOrNull<TextureRect>(BootTitleLogoPath), BootTitleLogoMinSize);
		ApplyTitleLogoOverride(GetNodeOrNull<TextureRect>(StartMenuLogoPath), StartMenuLogoMinSize);

		ApplyTitleTextOverride(GetNodeOrNull<Label>(BootTitleSubtitlePath), UseInspectorBootSubtitleText, BootSubtitleText);
		ApplyTitleTextOverride(GetNodeOrNull<Label>(BootTitlePromptPath), UseInspectorBootPromptText, BootPromptText);
		ApplyTitleTextOverride(GetNodeOrNull<Label>(StartSubtitleLabelPath), UseInspectorStartSubtitleText, StartSubtitleText);
		ApplyBootLetterboxOverride();
	}

	private void ApplyTitleLogoOverride(TextureRect logo, Vector2 minSize)
	{
		if (logo == null)
			return;

		if (SharedTitleLogoTexture != null)
			logo.Texture = SharedTitleLogoTexture;

		logo.CustomMinimumSize = minSize;
	}

	private static void ApplyTitleTextOverride(Label label, bool enabled, string text)
	{
		if (!enabled || label == null)
			return;

		label.Text = text ?? string.Empty;
	}

	private void ApplyBootLetterboxOverride()
	{
		ApplyLetterboxBar(GetNodeOrNull<ColorRect>(BootTopLetterboxPath), isTop: true);
		ApplyLetterboxBar(GetNodeOrNull<ColorRect>(BootBottomLetterboxPath), isTop: false);
	}

	private void ApplyLetterboxBar(ColorRect bar, bool isTop)
	{
		if (bar == null)
			return;

		bar.Visible = EnableBootLetterbox;
		if (!EnableBootLetterbox)
			return;

		float clampedHeight = Mathf.Max(0f, BootLetterboxHeight);
		float clampedAlpha = Mathf.Clamp(BootLetterboxAlpha, 0f, 1f);
		bar.Color = new Color(0f, 0f, 0f, clampedAlpha);
		bar.MouseFilter = Control.MouseFilterEnum.Ignore;

		if (isTop)
		{
			bar.OffsetTop = 0f;
			bar.OffsetBottom = clampedHeight;
			return;
		}

		bar.OffsetTop = -clampedHeight;
		bar.OffsetBottom = 0f;
	}
}
