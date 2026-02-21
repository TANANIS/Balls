using Godot;

public partial class ExperiencePickup : Area2D
{
	[Export] public float LifetimeSeconds = 20f;
	[Export] public float PickupRadius = 16f;
	[Export] public int ExperienceValue = 1;
	[Export] public NodePath PlayerPath = "../Player";
	[Export] public bool EnableAutoAttract = true;
	[Export] public float AutoAttractRange = 180f;
	[Export] public float AutoAttractSpeed = 780f;

	private PressureSystem _pressureSystem;
	private Player _player;
	private float _lifeTimer;

	public override void _Ready()
	{
		_lifeTimer = LifetimeSeconds;
		BodyEntered += OnBodyEntered;

		var shape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (shape?.Shape is CircleShape2D circle)
			circle.Radius = PickupRadius;

		var list = GetTree().GetNodesInGroup("PressureSystem");
		if (list.Count > 0)
			_pressureSystem = list[0] as PressureSystem;

		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player == null)
			_player = GetTree().CurrentScene?.GetNodeOrNull<Player>("Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		_lifeTimer -= (float)delta;
		if (_lifeTimer <= 0f)
		{
			QueueFree();
			return;
		}

		TickAutoAttract((float)delta);
	}

	private void TickAutoAttract(float dt)
	{
		if (!EnableAutoAttract || AutoAttractRange <= 0f || AutoAttractSpeed <= 0f)
			return;
		if (!IsInstanceValid(_player))
		{
			_player = GetNodeOrNull<Player>(PlayerPath);
			if (!IsInstanceValid(_player))
				_player = GetTree().CurrentScene?.GetNodeOrNull<Player>("Player");
		}
		if (!IsInstanceValid(_player))
			return;

		Vector2 toPlayer = _player.GlobalPosition - GlobalPosition;
		float dist = toPlayer.Length();
		if (dist <= 0.001f || dist > AutoAttractRange)
			return;

		Vector2 dir = toPlayer / dist;
		GlobalPosition += dir * AutoAttractSpeed * dt;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player)
			return;

		_pressureSystem?.AddExperienceFromPickup(Mathf.Max(1, ExperienceValue));
		AudioManager.Instance?.PlaySfxPlayerUpgrade();
		QueueFree();
	}
}
