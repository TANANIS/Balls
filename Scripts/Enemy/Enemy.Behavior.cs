using Godot;

public partial class Enemy
{
	private Vector2 GetDesiredVelocity(double delta)
	{
		if (_behavior != null && _behavior.Active)
			return _behavior.GetDesiredVelocity(this, _player, delta);

		// Fallback behavior: simple direct chase.
		if (_player == null)
			return Vector2.Zero;

		Vector2 toPlayer = _player.GlobalPosition - GlobalPosition;
		if (toPlayer.LengthSquared() < 0.0001f)
			return Vector2.Zero;

		return toPlayer.Normalized() * MaxSpeed;
	}

	private void EmitSpawned()
	{
		ForEachActiveEventModule(evt => evt.OnSpawned(this));
	}

	private void ForEachActiveEventModule(System.Action<EnemyEventModule> fn)
	{
		foreach (EnemyEventModule evt in _events)
		{
			if (!evt.Active)
				continue;
			fn(evt);
		}
	}
}
