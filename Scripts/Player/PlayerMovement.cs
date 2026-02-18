using Godot;

public partial class PlayerMovement : Node
{
	[Export] public float MaxSpeed = 320f;
	[Export] public float Accel = 2200f;
	[Export] public float Friction = 2600f;
	[Export] public float StopThreshold = 5f;

	private Player _player;
	private StabilitySystem _stabilitySystem;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();
	}

	public void Tick(float dt, Vector2 inputDir)
	{
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();

		float inertiaMult = _stabilitySystem?.GetPlayerInertiaMultiplier() ?? 1f;
		float inputSign = _stabilitySystem?.InputDirectionSign ?? 1f;
		Vector2 runtimeInput = inputDir * inputSign;

		Vector2 targetVel = runtimeInput * MaxSpeed;
		float rate = (runtimeInput.LengthSquared() > 0f) ? Accel : Friction;
		rate *= Mathf.Max(0.1f, inertiaMult);

		_player.Velocity = _player.Velocity.MoveToward(targetVel, rate * dt);

		if (runtimeInput == Vector2.Zero && _player.Velocity.Length() < StopThreshold)
			_player.Velocity = Vector2.Zero;

		_player.MoveAndSlide();
	}

	private void ResolveStabilitySystem()
	{
		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}
}
