using Godot;

public partial class Bullet
{
	private void OnAreaEntered(Area2D other)
	{
		TryHit(other);
	}

	private void OnBodyEntered(Node2D other)
	{
		TryHit(other);
	}

	private void TryHit(Node other)
	{
		if (_hasHit)
			return;

		if (other.IsInGroup("World"))
		{
			_hasHit = true;
			QueueFree();
			return;
		}

		if (_combat == null || _source == null || other == null)
			return;
		if (other == _source)
			return;
		if (other is not IDamageable)
			return;

		var req = new DamageRequest(
			source: _source,
			target: other,
			baseDamage: _damage,
			worldPos: GlobalPosition,
			tag: DamageTag
		);

		_combat.RequestDamage(req);
		_hasHit = true;
		QueueFree();
	}
}
