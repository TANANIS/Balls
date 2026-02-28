using Godot;

public partial class EventTelegraphOverlay : Node2D
{
	[Export] public bool Enabled = true;
	[Export(PropertyHint.Range, "24,160,1")] public int ArcPointCount = 72;
	[Export(PropertyHint.Range, "120,520,1")] public float DirectionArrowLength = 260f;

	private EventDirector _eventDirector;
	private readonly EventTelegraphSnapshot _snapshot = new();

	public override void _Ready()
	{
		ZIndex = 240;
		Visible = false;
	}

	public override void _Process(double delta)
	{
		if (!Enabled || GetTree().Paused)
		{
			SetVisibleIfChanged(false);
			return;
		}

		_eventDirector = GroupServiceResolver.ResolveFirstInGroup(this, RuntimeGroups.EventDirector, _eventDirector);
		if (!GodotObject.IsInstanceValid(_eventDirector) || !_eventDirector.IsEventActive)
		{
			SetVisibleIfChanged(false);
			return;
		}

		_eventDirector.FillTelegraphSnapshot(_snapshot);
		bool hasTelegraph =
			_snapshot.Circles.Count > 0
			|| _snapshot.Markers.Count > 0
			|| _snapshot.HasDirection;

		SetVisibleIfChanged(hasTelegraph);
		if (hasTelegraph)
			QueueRedraw();
	}

	public override void _Draw()
	{
		if (!Visible)
			return;

		for (int i = 0; i < _snapshot.Circles.Count; i++)
		{
			EventTelegraphCircle circle = _snapshot.Circles[i];
			if (circle.Radius <= 0f)
				continue;

			if (circle.Filled)
				DrawCircle(circle.Center, circle.Radius, circle.Color);
			if (circle.StrokeWidth > 0f)
			{
				Color edge = circle.Color;
				edge.A = Mathf.Clamp(edge.A + 0.28f, 0f, 1f);
				DrawArc(
					circle.Center,
					circle.Radius,
					0f,
					Mathf.Tau,
					Mathf.Clamp(ArcPointCount, 16, 256),
					edge,
					circle.StrokeWidth);
			}
		}

		for (int i = 0; i < _snapshot.Markers.Count; i++)
		{
			EventTelegraphMarker marker = _snapshot.Markers[i];
			if (marker.Radius <= 0f)
				continue;

			Color fill = marker.Color;
			fill.A = Mathf.Clamp(fill.A * 0.24f, 0f, 1f);
			DrawCircle(marker.Position, marker.Radius, fill);
			DrawArc(marker.Position, marker.Radius, 0f, Mathf.Tau, 36, marker.Color, 2.2f);
		}

		if (_snapshot.HasDirection && _snapshot.DirectionVector.LengthSquared() > 0.0001f)
			DrawDirectionArrow();
	}

	private void DrawDirectionArrow()
	{
		Vector2 origin = _snapshot.DirectionOrigin;
		Vector2 dir = _snapshot.DirectionVector.Normalized();
		Vector2 tip = origin + (dir * Mathf.Max(40f, DirectionArrowLength));

		DrawLine(origin, tip, _snapshot.DirectionColor, 5f, antialiased: true);

		Vector2 back = -dir;
		Vector2 wingLeft = tip + back.Rotated(0.45f) * 26f;
		Vector2 wingRight = tip + back.Rotated(-0.45f) * 26f;
		DrawLine(tip, wingLeft, _snapshot.DirectionColor, 4f, antialiased: true);
		DrawLine(tip, wingRight, _snapshot.DirectionColor, 4f, antialiased: true);
	}

	private void SetVisibleIfChanged(bool visible)
	{
		if (Visible == visible)
			return;

		Visible = visible;
		if (!visible)
			QueueRedraw();
	}
}
