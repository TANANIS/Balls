using Godot;
using System.Collections.Generic;

public partial class CursorRing : Node2D
{
	public enum CursorPresentationMode
	{
		GameplayAim = 0,
		UiPointer = 1
	}

	private enum UiPointerVisualState
	{
		Idle = 0,
		Hover = 1,
		Pressed = 2,
		Disabled = 3
	}

	private const string DefaultUiPointerPath = "res://Assets/UI/Cursors/mouse_pointer.png";
	private const string DefaultUiCursorIdlePath = "res://Assets/mouse_ui_set/mouse_idle_64.png";
	private const string DefaultUiCursorHoverPath = "res://Assets/mouse_ui_set/mouse_hover_64.png";
	private const string DefaultUiCursorPressedPath = "res://Assets/mouse_ui_set/mouse_pressed_64.png";
	private const string DefaultUiCursorDisabledPath = "res://Assets/mouse_ui_set/mouse_disabled_64.png";

	[Export] public float Radius = 12f;
	[Export] public float Thickness = 2.0f;
	[Export] public Color RingColor = new Color(0.67f, 0.50f, 0.28f, 0.9f);
	[Export] public Color GlowColor = new Color(0.42f, 0.28f, 0.12f, 0.20f);
	[Export] public float PulseSpeed = 2.4f;
	[Export] public float PulseAmount = 0.15f;
	[Export] public bool HideWhenMouseOutside = true;
	[Export] public Texture2D UiPointerTexture;
	[Export] public Vector2 UiPointerHotspot = Vector2.Zero;
	[Export(PropertyHint.Range, "0.50,4.00,0.05")] public float UiPointerScale = 1.25f;
	[Export] public bool UseUiCursorStateSet = true;
	[Export] public Texture2D UiPointerIdleTexture;
	[Export] public Texture2D UiPointerHoverTexture;
	[Export] public Texture2D UiPointerPressedTexture;
	[Export] public Texture2D UiPointerDisabledTexture;

	private float _time;
	private Vector2 _lastMouse = Vector2.Zero;
	private bool _useAutoAimMarker;
	private Vector2 _autoAimMarkerWorldPosition = Vector2.Zero;
	private bool _suppressMouseCursor;
	private CursorPresentationMode _presentationMode = CursorPresentationMode.GameplayAim;
	private UiPointerVisualState _uiPointerVisualState = UiPointerVisualState.Idle;
	private float _lastUiPointerScale = -1f;
	private readonly Dictionary<string, Texture2D> _scaledCursorCache = new();

	public Vector2 LastMouseScreenPosition => _lastMouse;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		UiPointerTexture ??= GD.Load<Texture2D>(DefaultUiPointerPath);
		UiPointerIdleTexture ??= GD.Load<Texture2D>(DefaultUiCursorIdlePath);
		UiPointerHoverTexture ??= GD.Load<Texture2D>(DefaultUiCursorHoverPath);
		UiPointerPressedTexture ??= GD.Load<Texture2D>(DefaultUiCursorPressedPath);
		UiPointerDisabledTexture ??= GD.Load<Texture2D>(DefaultUiCursorDisabledPath);
		ApplyPresentationMode();
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;

		Viewport viewport = GetViewport();
		if (viewport == null)
			return;
		_lastMouse = viewport.GetMousePosition();

		if (_presentationMode == CursorPresentationMode.UiPointer)
		{
			Visible = false;
			RefreshUiPointerVisual(viewport);
			return;
		}

		if (_useAutoAimMarker)
		{
			Transform2D canvasToScreen = viewport.GetCanvasTransform();
			GlobalPosition = canvasToScreen * _autoAimMarkerWorldPosition;
			Visible = true;
			return;
		}

		if (_suppressMouseCursor)
		{
			Visible = false;
			return;
		}

		GlobalPosition = _lastMouse;

		if (HideWhenMouseOutside)
		{
			Rect2 rect = GetViewport().GetVisibleRect();
			Visible = rect.HasPoint(_lastMouse);
		}
	}

	public void SetPresentationMode(CursorPresentationMode mode)
	{
		if (_presentationMode == mode)
			return;
		_presentationMode = mode;
		ApplyPresentationMode();
	}

	public void SetAutoAimMarkerWorldPosition(Vector2 worldPosition, bool active, bool suppressMouseCursor)
	{
		_useAutoAimMarker = active;
		_suppressMouseCursor = suppressMouseCursor;
		if (active)
			_autoAimMarkerWorldPosition = worldPosition;
	}

	private void ApplyPresentationMode()
	{
		if (_presentationMode == CursorPresentationMode.UiPointer)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			RefreshUiPointerVisual(GetViewport(), force: true);
			Visible = false;
			return;
		}

		Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow, Vector2.Zero);
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	private void RefreshUiPointerVisual(Viewport viewport, bool force = false)
	{
		if (_presentationMode != CursorPresentationMode.UiPointer)
			return;

		UiPointerVisualState nextState = ResolveUiPointerVisualState(viewport);
		float scale = Mathf.Max(0.05f, UiPointerScale);
		if (!force && nextState == _uiPointerVisualState && Mathf.IsEqualApprox(scale, _lastUiPointerScale))
			return;

		Texture2D source = SelectUiPointerSourceTexture(nextState) ?? UiPointerTexture;
		Texture2D cursorTexture = BuildScaledCursorTexture(source);
		Vector2 hotspot = UiPointerHotspot * scale;
		Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, hotspot);

		_uiPointerVisualState = nextState;
		_lastUiPointerScale = scale;
	}

	private UiPointerVisualState ResolveUiPointerVisualState(Viewport viewport)
	{
		if (!UseUiCursorStateSet || viewport == null)
			return UiPointerVisualState.Idle;

		Control hovered = viewport.GuiGetHoveredControl();
		if (!IsUiInteractableControl(hovered))
			return UiPointerVisualState.Idle;

		if (IsDisabledControl(hovered))
			return UiPointerVisualState.Disabled;

		if (Input.IsMouseButtonPressed(MouseButton.Left))
			return UiPointerVisualState.Pressed;

		return UiPointerVisualState.Hover;
	}

	private static bool IsUiInteractableControl(Control control)
	{
		if (control == null)
			return false;
		if (!control.Visible)
			return false;
		if (control.MouseFilter == Control.MouseFilterEnum.Ignore)
			return false;

		return control is BaseButton
			or Slider
			or OptionButton
			or SpinBox
			or LineEdit
			or TextEdit
			or ItemList
			or Tree
			or TabBar
			or TabContainer
			or MenuButton
			or ColorPickerButton
			or TextureButton
			or CheckBox
			or CheckButton
			or LinkButton
			or RichTextLabel
			|| (control.FocusMode != Control.FocusModeEnum.None && control.MouseFilter != Control.MouseFilterEnum.Ignore);
	}

	private static bool IsDisabledControl(Control control)
	{
		if (control == null)
			return false;
		if (control is BaseButton button && button.Disabled)
			return true;
		if (control is LineEdit lineEdit && !lineEdit.Editable)
			return true;
		if (control is TextEdit textEdit && !textEdit.Editable)
			return true;
		return false;
	}

	private Texture2D SelectUiPointerSourceTexture(UiPointerVisualState state)
	{
		if (!UseUiCursorStateSet)
			return UiPointerTexture;

		Texture2D texture = state switch
		{
			UiPointerVisualState.Hover => UiPointerHoverTexture,
			UiPointerVisualState.Pressed => UiPointerPressedTexture,
			UiPointerVisualState.Disabled => UiPointerDisabledTexture,
			_ => UiPointerIdleTexture
		};
		return texture ?? UiPointerIdleTexture ?? UiPointerTexture;
	}

	private Texture2D BuildScaledCursorTexture(Texture2D source)
	{
		if (source == null)
			return null;

		float scale = Mathf.Max(0.05f, UiPointerScale);
		if (Mathf.IsEqualApprox(scale, 1f))
			return source;

		string sourceKey = string.IsNullOrWhiteSpace(source.ResourcePath)
			? source.GetInstanceId().ToString()
			: source.ResourcePath;
		string cacheKey = $"{sourceKey}|{scale:0.00}";
		if (_scaledCursorCache.TryGetValue(cacheKey, out Texture2D cached) && IsInstanceValid(cached))
			return cached;

		Image image = source.GetImage();
		if (image == null || image.IsEmpty())
			return source;

		int targetWidth = Mathf.Max(1, Mathf.RoundToInt(image.GetWidth() * scale));
		int targetHeight = Mathf.Max(1, Mathf.RoundToInt(image.GetHeight() * scale));
		image.Resize(targetWidth, targetHeight, Image.Interpolation.Nearest);
		Texture2D scaledTexture = ImageTexture.CreateFromImage(image);
		_scaledCursorCache[cacheKey] = scaledTexture;
		return scaledTexture;
	}

	public override void _Draw()
	{
		float pulse = 1f + Mathf.Sin(_time * PulseSpeed) * PulseAmount;
		float r = Radius * pulse;

		DrawCircle(Vector2.Zero, r + 6f, GlowColor);
		DrawArc(Vector2.Zero, r, 0f, Mathf.Tau, 64, RingColor, Thickness, true);
		DrawCircle(Vector2.Zero, 2.0f, RingColor);
	}

	public Vector2 GetMouseWorldPosition()
	{
		Viewport viewport = GetViewport();
		if (viewport == null)
			return Vector2.Zero;

		Transform2D canvasToScreen = viewport.GetCanvasTransform();
		Transform2D screenToCanvas = canvasToScreen.AffineInverse();
		return screenToCanvas * _lastMouse;
	}
}

