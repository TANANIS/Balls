using Godot;

public partial class CursorRing : Node2D
{
	[Export] public float Radius = 12f;
	[Export] public float Thickness = 2.0f;
	[Export] public Color RingColor = new Color(0.1f, 0.55f, 0.75f, 0.9f);
	[Export] public Color GlowColor = new Color(0.0f, 0.35f, 0.55f, 0.18f);
	[Export] public float PulseSpeed = 2.4f;
	[Export] public float PulseAmount = 0.15f;
	[Export] public bool HideWhenMouseOutside = true;

	private float _time;
	private Vector2 _lastMouse = Vector2.Zero;

	public Vector2 LastMouseScreenPosition => _lastMouse;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;

		Viewport viewport = GetViewport();
		if (viewport == null)
			return;
		_lastMouse = viewport.GetMousePosition();

		GlobalPosition = _lastMouse;

		if (HideWhenMouseOutside)
		{
			Rect2 rect = GetViewport().GetVisibleRect();
			Visible = rect.HasPoint(_lastMouse);
		}
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
