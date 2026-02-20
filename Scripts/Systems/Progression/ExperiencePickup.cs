using Godot;

public partial class ExperiencePickup : Area2D
{
	[Export] public float LifetimeSeconds = 20f;
	[Export] public float PickupRadius = 16f;
	[Export] public int ExperienceValue = 1;

	private PressureSystem _pressureSystem;
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
	}

	public override void _PhysicsProcess(double delta)
	{
		_lifeTimer -= (float)delta;
		if (_lifeTimer <= 0f)
			QueueFree();
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
