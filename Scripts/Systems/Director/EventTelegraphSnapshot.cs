using Godot;
using System.Collections.Generic;

public readonly struct EventTelegraphCircle
{
	public readonly Vector2 Center;
	public readonly float Radius;
	public readonly Color Color;
	public readonly float StrokeWidth;
	public readonly bool Filled;

	public EventTelegraphCircle(Vector2 center, float radius, Color color, float strokeWidth, bool filled)
	{
		Center = center;
		Radius = radius;
		Color = color;
		StrokeWidth = strokeWidth;
		Filled = filled;
	}
}

public readonly struct EventTelegraphMarker
{
	public readonly Vector2 Position;
	public readonly float Radius;
	public readonly Color Color;

	public EventTelegraphMarker(Vector2 position, float radius, Color color)
	{
		Position = position;
		Radius = radius;
		Color = color;
	}
}

public sealed class EventTelegraphSnapshot
{
	public string EventId { get; set; } = string.Empty;
	public string EventName { get; set; } = string.Empty;
	public string DomainId { get; set; } = string.Empty;
	public string EventHintText { get; set; } = string.Empty;
	public string HybridHintText { get; set; } = string.Empty;
	public bool HasDirection { get; set; }
	public Vector2 DirectionOrigin { get; set; } = Vector2.Zero;
	public Vector2 DirectionVector { get; set; } = Vector2.Zero;
	public Color DirectionColor { get; set; } = Colors.White;
	public List<EventTelegraphCircle> Circles { get; } = new();
	public List<EventTelegraphMarker> Markers { get; } = new();

	public void Clear()
	{
		EventId = string.Empty;
		EventName = string.Empty;
		DomainId = string.Empty;
		EventHintText = string.Empty;
		HybridHintText = string.Empty;
		HasDirection = false;
		DirectionOrigin = Vector2.Zero;
		DirectionVector = Vector2.Zero;
		DirectionColor = Colors.White;
		Circles.Clear();
		Markers.Clear();
	}
}
