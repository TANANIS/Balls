using Godot;

public partial class Player
{
	private void ClampInsideBounds()
	{
		if (!UseMovementBounds)
			return;

		Vector2 p = GlobalPosition;
		p.X = Mathf.Clamp(p.X, MovementBounds.Position.X, MovementBounds.Position.X + MovementBounds.Size.X);
		p.Y = Mathf.Clamp(p.Y, MovementBounds.Position.Y, MovementBounds.Position.Y + MovementBounds.Size.Y);
		GlobalPosition = p;
	}
}
