using Godot;

public partial class Player
{
	private void ResolveModules()
	{
		// Hard dependencies expected in player scene tree.
		_health = GetNode<PlayerHealth>("Health");
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_primaryAttack = GetNode<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = GetNode<PlayerMelee>("SecondaryAttack");
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (_sprite != null)
			_baseSpriteScale = _sprite.Scale;
		_camera = GetNodeOrNull<Camera2D>("Camera2D");
		if (_camera != null)
			_cameraBaseZoom = _camera.Zoom;
		ResolveStabilitySystem();
	}

	private void BindSignals()
	{
		if (_health != null)
			_health.Died += OnDied;
	}

	private void SetupModules()
	{
		// Pass player context to child behavior modules.
		_movement.Setup(this);
		_dash.Setup(this);
		_primaryAttack.Setup(this);
		_secondaryAttack.Setup(this);
	}

	private void OnDied()
	{
		if (_deathLogged)
			return;
		_deathLogged = true;
		DebugSystem.Log("[Player] Died.");
	}

	private void ResolveStabilitySystem()
	{
		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}

	private void UpdatePhaseCamera(float dt)
	{
		if (_camera == null)
			return;
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();
		if (_stabilitySystem == null)
			return;

		float zoomMult = _stabilitySystem.GetCameraZoomMultiplier();
		Vector2 target = _cameraBaseZoom * zoomMult;
		_camera.Zoom = _camera.Zoom.Lerp(target, Mathf.Clamp(dt * 2.2f, 0f, 1f));
	}
}
