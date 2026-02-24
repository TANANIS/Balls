using Godot;

public partial class Player
{
	public override void _Process(double delta)
	{
		_aimWorldPosition = GetGlobalMousePosition();
	}

	public Vector2 GetAimWorldPosition()
	{
		_aimWorldPosition = GetGlobalMousePosition();
		return _aimWorldPosition;
	}

	public Vector2 GetAimDirection(Vector2 fallback)
	{
		Vector2 dir = GetAimWorldPosition() - GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			return fallback.LengthSquared() < 0.0001f ? Vector2.Right : fallback.Normalized();
		return dir.Normalized();
	}
}
