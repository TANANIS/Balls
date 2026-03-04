using Godot;

public partial class CursorRing : Node2D
{
	public enum CursorPresentationMode
	{
		GameplayAim = 0,
		UiPointer = 1
	}

	private const string DefaultUiPointerPath = "res://Assets/mouse pointer.png";

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

	private float _time;
	private Vector2 _lastMouse = Vector2.Zero;
	private bool _useAutoAimMarker;
	private Vector2 _autoAimMarkerWorldPosition = Vector2.Zero;
	private bool _suppressMouseCursor;
	private CursorPresentationMode _presentationMode = CursorPresentationMode.GameplayAim;

	public Vector2 LastMouseScreenPosition => _lastMouse;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		UiPointerTexture ??= GD.Load<Texture2D>(DefaultUiPointerPath);
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
			Texture2D cursorTexture = BuildUiPointerCursorTexture();
			Vector2 hotspot = UiPointerHotspot * Mathf.Max(0.05f, UiPointerScale);
			Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, hotspot);
			Visible = false;
			return;
		}

		Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow, Vector2.Zero);
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	private Texture2D BuildUiPointerCursorTexture()
	{
		Texture2D source = UiPointerTexture;
		if (source == null)
			return null;

		float scale = Mathf.Max(0.05f, UiPointerScale);
		if (Mathf.IsEqualApprox(scale, 1f))
			return source;

		Image image = source.GetImage();
		if (image == null || image.IsEmpty())
			return source;

		int targetWidth = Mathf.Max(1, Mathf.RoundToInt(image.GetWidth() * scale));
		int targetHeight = Mathf.Max(1, Mathf.RoundToInt(image.GetHeight() * scale));
		image.Resize(targetWidth, targetHeight, Image.Interpolation.Nearest);
		return ImageTexture.CreateFromImage(image);
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

