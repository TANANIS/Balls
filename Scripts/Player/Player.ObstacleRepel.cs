using Godot;

public partial class Player
{
	[Export(PropertyHint.Range, "1,8,1")] public int ObstacleStickFramesBeforeRepel = 1;
	[Export(PropertyHint.Range, "0,40,1")] public float ObstacleRepelDistance = 8f;
	[Export(PropertyHint.Range, "0,800,1")] public float ObstacleRepelSpeed = 220f;

	private ulong _lastObstacleColliderId = 0ul;
	private int _obstacleStickFrames = 0;

	public void ResolveObstacleStick()
	{
		if (!ObstacleCollisionHelper.TryGetObstacleCollision(this, out KinematicCollision2D collision))
		{
			ResetObstacleStickState();
			return;
		}

		ulong colliderId = ObstacleCollisionHelper.GetColliderInstanceId(collision);
		if (colliderId != 0ul && colliderId == _lastObstacleColliderId)
			_obstacleStickFrames++;
		else
		{
			_lastObstacleColliderId = colliderId;
			_obstacleStickFrames = 1;
		}

		int frameThreshold = Mathf.Max(1, ObstacleStickFramesBeforeRepel);
		if (_obstacleStickFrames <= frameThreshold)
			return;

		Vector2 repelDirection = ObstacleCollisionHelper.GetRepelDirection(this, collision);
		if (repelDirection.LengthSquared() <= 0.0001f)
			return;

		float pushDistance = Mathf.Max(0f, ObstacleRepelDistance);
		if (pushDistance > 0f)
			GlobalPosition += repelDirection * pushDistance;

		float pushSpeed = Mathf.Max(0f, ObstacleRepelSpeed);
		if (pushSpeed > 0f)
		{
			Vector2 velocity = Velocity.Slide(repelDirection);
			velocity += repelDirection * pushSpeed;
			Velocity = velocity;
		}

		_obstacleStickFrames = 0;
	}

	private void ResetObstacleStickState()
	{
		_lastObstacleColliderId = 0ul;
		_obstacleStickFrames = 0;
	}
}
