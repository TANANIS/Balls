using Godot;

public partial class Bullet
{
	private void UpdateHomingDirection(float dt)
	{
		if (!_homingEnabledRuntime)
			return;
		if (!IsRuntimeRetargetCandidate(_homingTarget))
			_homingTarget = AcquireHomingTargetByCurrentDirection();
		if (!IsRuntimeRetargetCandidate(_homingTarget))
			return;

		Vector2 toTarget = _homingTarget.GlobalPosition - GlobalPosition;
		if (toTarget.LengthSquared() < 0.0001f)
			return;

		Vector2 desiredDir = toTarget.Normalized();
		float maxTurnRadians = Mathf.DegToRad(Mathf.Max(0f, _homingTurnRateRuntime)) * Mathf.Max(0f, dt);
		if (maxTurnRadians <= 0f)
		{
			_dir = desiredDir;
			return;
		}

		float signedAngle = _dir.AngleTo(desiredDir);
		if (Mathf.Abs(signedAngle) <= maxTurnRadians)
			_dir = desiredDir;
		else
			_dir = _dir.Rotated(Mathf.Sign(signedAngle) * maxTurnRadians).Normalized();
	}

	private EnemyHurtbox AcquireHomingTargetByCurrentDirection()
	{
		SceneTree tree = GetTree();
		if (tree == null)
			return null;

		EnemyHurtbox best = null;
		float bestDistSq = float.MaxValue;
		Vector2 forward = _dir.LengthSquared() < 0.0001f ? Vector2.Right : _dir.Normalized();
		float threshold = Mathf.Clamp(HomingForwardDotThreshold, -1f, 1f);

		foreach (Node node in tree.GetNodesInGroup(RuntimeGroups.EnemyHurtbox))
		{
			if (node is not EnemyHurtbox hurtbox || !IsRuntimeRetargetCandidate(hurtbox))
				continue;

			Vector2 toTarget = hurtbox.GlobalPosition - GlobalPosition;
			float distSq = toTarget.LengthSquared();
			if (distSq < 0.0001f)
				continue;

			float dot = forward.Dot(toTarget.Normalized());
			if (dot < threshold)
				continue;

			if (distSq < bestDistSq)
			{
				bestDistSq = distSq;
				best = hurtbox;
			}
		}

		return best;
	}

	private bool IsRuntimeRetargetCandidate(Node2D target)
	{
		if (!IsHomingTargetValid(target))
			return false;

		ulong id = (ulong)target.GetInstanceId();
		if (_hitTargetIds.Contains(id))
			return false;
		if (_ignoreTargetTimer > 0f && _ignoreTargetInstanceId != 0 && id == _ignoreTargetInstanceId)
			return false;
		return true;
	}

	private static bool IsHomingTargetValid(Node2D target)
	{
		if (target == null || !IsInstanceValid(target))
			return false;
		if (target is EnemyHurtbox hurtbox && hurtbox.IsDead)
			return false;
		return true;
	}

	private bool IsOutsideActiveCameraViewport()
	{
		Viewport viewport = GetViewport();
		if (viewport == null)
			return false;

		Camera2D camera = viewport.GetCamera2D();
		if (camera == null)
			return false;

		Vector2 screenSize = viewport.GetVisibleRect().Size;
		Vector2 worldSize = new Vector2(
			screenSize.X * Mathf.Abs(camera.Zoom.X),
			screenSize.Y * Mathf.Abs(camera.Zoom.Y));
		Vector2 half = worldSize * 0.5f;
		Rect2 worldRect = new Rect2(camera.GlobalPosition - half, worldSize)
			.Grow(Mathf.Max(0f, DespawnOutsideViewportMargin));
		return !worldRect.HasPoint(GlobalPosition);
	}
}
