using Godot;

public partial class ExperiencePickup : Area2D
{
	[Export] public float LifetimeSeconds = 0f;
	[Export] public float PickupRadius = 16f;
	[Export] public int ExperienceValue = 1;
	[Export] public NodePath PlayerPath = "../Player";
	[Export] public bool EnableAutoAttract = true;
	[Export] public float AutoAttractRange = 180f;
	[Export] public float AutoAttractSpeed = 780f;

	private ProgressionSystem _progressionSystem;
	private Player _player;
	private CircleShape2D _pickupShape;
	private float _lifeTimer;

	public override void _Ready()
	{
		_lifeTimer = Mathf.Max(0f, LifetimeSeconds);
		BodyEntered += OnBodyEntered;

		var shape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (shape?.Shape is CircleShape2D circle)
		{
			_pickupShape = circle;
			_pickupShape.Radius = PickupRadius;
		}

		var list = GetTree().GetNodesInGroup("ProgressionSystem");
		if (list.Count > 0)
			_progressionSystem = list[0] as ProgressionSystem;

		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player == null)
			_player = GetTree().CurrentScene?.GetNodeOrNull<Player>("Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (LifetimeSeconds > 0f)
		{
			_lifeTimer -= (float)delta;
			if (_lifeTimer <= 0f)
			{
				QueueFree();
				return;
			}
		}

		TickAutoAttract((float)delta);
		TickPickupRadius();
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
		float radiusMult = _progressionSystem?.PickupRadiusMultiplier ?? 1f;
		float effectiveRange = AutoAttractRange * Mathf.Clamp(radiusMult, 0.5f, 4f);
		if (dist <= 0.001f || dist > effectiveRange)
			return;

		Vector2 dir = toPlayer / dist;
		GlobalPosition += dir * AutoAttractSpeed * dt;
	}

	private void TickPickupRadius()
	{
		if (_pickupShape == null)
			return;

		float radiusMult = _progressionSystem?.PickupRadiusMultiplier ?? 1f;
		float effectiveRadius = PickupRadius * Mathf.Clamp(radiusMult, 0.5f, 4f);
		_pickupShape.Radius = effectiveRadius;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player)
			return;

		_progressionSystem?.AddExperienceFromPickup(Mathf.Max(1, ExperienceValue));
		AudioManager.Instance?.PlaySfxPlayerUpgrade();
		QueueFree();
	}
}
