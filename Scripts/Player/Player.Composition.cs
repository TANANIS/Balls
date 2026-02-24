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
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_animatedSprite?.SpriteFrames != null)
			_baseSpriteFrames = (SpriteFrames)_animatedSprite.SpriteFrames.Duplicate(true);
		_visualRoot = (Node2D)_animatedSprite ?? _sprite;
		_skillVfxRoot = GetNodeOrNull<Node2D>("SkillVfxRoot");
		if (_skillVfxRoot == null)
		{
			_skillVfxRoot = this;
		}
		if (_visualRoot != null)
			_baseSpriteScale = _visualRoot.Scale;
		_camera = GetNodeOrNull<Camera2D>("Camera2D");
		if (_camera != null)
			_cameraBaseZoom = _camera.Zoom;
		ResolveStabilitySystem();
	}

	public Node2D GetSkillVfxRoot()
	{
		if (_skillVfxRoot == null || !IsInstanceValid(_skillVfxRoot))
		{
			_skillVfxRoot = GetNodeOrNull<Node2D>("SkillVfxRoot") ?? this;
		}

		return _skillVfxRoot;
	}

	private void BindSignals()
	{
		if (_health != null)
		{
			_health.Died += OnDied;
			_health.Damaged += OnDamaged;
		}
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
		TriggerDeathAnimation();
	}

	private void OnDamaged(int amount, object source)
	{
		if (_health == null || _health.IsDead)
			return;

		QueueHurtCommand(_health.DamageMoveFreezeSeconds, 0.30f);
	}

	private void ResolveStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
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
