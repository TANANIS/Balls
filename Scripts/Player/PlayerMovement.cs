using Godot;

public partial class PlayerMovement : Node
{
	[Export] public float MaxSpeed = 320f;
	[Export] public float Accel = 2200f;
	[Export] public float Friction = 2600f;
	[Export] public float StopThreshold = 5f;
	[Export(PropertyHint.Range, "0,0.50,0.01")] public float FreezeReapplyCooldown = 0.18f;

	private Player _player;
	private StabilitySystem _stabilitySystem;
	private float _movementFreezeTimer = 0f;
	private float _freezeReapplyTimer = 0f;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();
	}

	public void Tick(float dt, Vector2 inputDir)
	{
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();
		if (_freezeReapplyTimer > 0f)
			_freezeReapplyTimer = Mathf.Max(0f, _freezeReapplyTimer - dt);

		if (_movementFreezeTimer > 0f)
		{
			_movementFreezeTimer = Mathf.Max(0f, _movementFreezeTimer - dt);
			_player.Velocity = Vector2.Zero;
			_player.MoveAndSlide();
			_player.ResolveObstacleStick();
			return;
		}

		float inertiaMult = _stabilitySystem?.GetPlayerInertiaMultiplier() ?? 1f;
		float inputSign = _stabilitySystem?.InputDirectionSign ?? 1f;
		Vector2 runtimeInput = inputDir * inputSign;
		bool hasInput = runtimeInput.LengthSquared() > 0.0001f;

		Vector2 targetVel = runtimeInput * MaxSpeed;
		float rate = hasInput ? Accel : Friction;
		rate *= Mathf.Max(0.1f, inertiaMult);

		_player.Velocity = _player.Velocity.MoveToward(targetVel, rate * dt);

		if (!hasInput && _player.Velocity.Length() < StopThreshold)
			_player.Velocity = Vector2.Zero;

		_player.MoveAndSlide();
		_player.ResolveObstacleStick();
	}

	public void ApplyMovementFreeze(float duration)
	{
		if (duration <= 0f)
			return;

		if (_movementFreezeTimer <= 0f && _freezeReapplyTimer > 0f)
			return;

		_movementFreezeTimer = Mathf.Max(_movementFreezeTimer, duration);
		_freezeReapplyTimer = Mathf.Max(_freezeReapplyTimer, Mathf.Max(0f, FreezeReapplyCooldown));
	}

	private void ResolveStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, RuntimeGroups.StabilitySystem, _stabilitySystem);
	}

	public void SetBaseStats(float maxSpeed, float accel, float friction, float stopThreshold)
	{
		MaxSpeed = Mathf.Max(10f, maxSpeed);
		Accel = Mathf.Max(1f, accel);
		Friction = Mathf.Max(1f, friction);
		StopThreshold = Mathf.Max(0f, stopThreshold);
	}

	public void ResetRuntimeState()
	{
		_movementFreezeTimer = 0f;
		_freezeReapplyTimer = 0f;
	}
}
