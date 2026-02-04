using Godot;

public partial class EnemySeparationModule : Node
{
	private Vector2 _separationVelocity = Vector2.Zero;
	private float _remainingTime = 0f;

	public void ApplyImpulse(Vector2 pushDir, float strength, float duration)
	{
		if (pushDir.LengthSquared() < 0.0001f)
			pushDir = Vector2.Right;

		_separationVelocity = pushDir.Normalized() * strength;
		_remainingTime = Mathf.Max(duration, 0.01f);
	}

	public void ApplyToVelocity(ref Vector2 velocity, float dt)
	{
		if (_remainingTime <= 0f)
		{
			_separationVelocity = Vector2.Zero;
			return;
		}

		_remainingTime -= dt;
		velocity += _separationVelocity;
	}
}
