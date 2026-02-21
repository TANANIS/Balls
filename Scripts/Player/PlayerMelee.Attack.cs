using Godot;

public partial class PlayerMelee
{
	private void ExecuteAttack()
	{
		if (_combat == null || _player == null)
			return;

		AudioManager.Instance?.PlaySfxPlayerMelee();

		Vector2 attackDir = _player.GetGlobalMousePosition() - _player.GlobalPosition;
		if (attackDir.LengthSquared() < 0.0001f)
			attackDir = _player.LastMoveDir;
		else
			attackDir = attackDir.Normalized();

		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		float runtimeRange = Range * (1f + ((powerMult - 1f) * 0.25f));
		float dmgMult = Mathf.Max(0.1f, DamageMultiplier);
		int runtimeDamage = Mathf.Max(1, Mathf.RoundToInt(Damage * dmgMult * powerMult));

		SpawnVfx(attackDir, runtimeRange);
		QueryAndApplyMeleeDamage(attackDir, runtimeRange, runtimeDamage);
	}

	private void QueryAndApplyMeleeDamage(Vector2 attackDir, float runtimeRange, int runtimeDamage)
	{
		var circle = new CircleShape2D { Radius = runtimeRange };
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = circle,
			Transform = new Transform2D(0f, _player.GlobalPosition),
			CollisionMask = TargetMask,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var space = _player.GetWorld2D().DirectSpaceState;
		var results = space.IntersectShape(query, 32);
		float halfArcRad = Mathf.DegToRad(ArcDegrees) * 0.5f;

		foreach (var hit in results)
		{
			if (!hit.ContainsKey("collider"))
				continue;

			var colliderObj = hit["collider"].AsGodotObject();
			if (colliderObj is not Area2D area)
				continue;

			Vector2 toTarget = area.GlobalPosition - _player.GlobalPosition;
			if (toTarget.LengthSquared() < 0.0001f)
				continue;

			Vector2 targetDir = toTarget.Normalized();
			float dot = Mathf.Clamp(attackDir.Dot(targetDir), -1f, 1f);
			float angle = Mathf.Acos(dot);
			if (angle > halfArcRad)
				continue;

			if (area is not IDamageable)
				continue;

			var req = new DamageRequest(
				source: _player,
				target: area,
				baseDamage: runtimeDamage,
				worldPos: area.GlobalPosition,
				tag: "melee"
			);

			_combat.RequestDamage(req);
		}
	}

	private void SpawnVfx(Vector2 direction, float runtimeRange)
	{
		if (MeleeVfxScene == null || _player == null)
			return;

		Node vfx = MeleeVfxScene.Instantiate();
		if (vfx is Node2D vfx2d)
		{
			Vector2 right = direction;
			Vector2 up = right.Orthogonal();
			vfx2d.GlobalPosition = _player.GlobalPosition + right * VfxForwardOffset + up * VfxSideOffset;
			vfx2d.Rotation = direction.Angle();
		}

		Node parent = _player.GetParent();
		if (VfxPath != null && !VfxPath.IsEmpty)
			parent = GetNode(VfxPath);
		parent.AddChild(vfx);

		if (vfx is MeleeVFX meleeVfx)
			meleeVfx.Init(direction, runtimeRange, ArcDegrees, VfxDuration, VfxColor);
	}
}
