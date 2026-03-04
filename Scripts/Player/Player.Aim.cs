using Godot;

public partial class Player
{
	private enum AutoAimMode
	{
		None = 0,
		Gamepad = 1,
		Mouse = 2
	}

	[ExportGroup("Auto Aim")]
	[Export] public bool AutoAimEnabled = true;
	[Export] public bool EnableGamepadAutoAim = true;
	[Export] public bool EnableMouseAutoAim = true;
	[Export(PropertyHint.Range, "0.05,0.95,0.01")] public float RightStickDeadzone = 0.30f;
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float RightStickDirectionDotThreshold = 0.20f;
	[Export(PropertyHint.Range, "80,2400,10")] public float AutoAimMaxDistance = 1200f;
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float LockStickiness = 0.20f;
	[Export] public NodePath CursorRingPath = "../CanvasLayer/CursorRing";

	private EnemyHurtbox _lockedAimTarget;
	private CursorRing _cursorRing;

	public override void _Process(double delta)
	{
		UpdateAimWorldPosition();
	}

	public Vector2 GetAimWorldPosition()
	{
		UpdateAimWorldPosition();
		return _aimWorldPosition;
	}

	public Vector2 GetAimDirection(Vector2 fallback)
	{
		Vector2 dir = GetAimWorldPosition() - GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			return fallback.LengthSquared() < 0.0001f ? Vector2.Right : fallback.Normalized();
		return dir.Normalized();
	}

	public bool GetAutoAimEnabled()
	{
		return AutoAimEnabled;
	}

	public void SetAutoAimEnabled(bool enabled)
	{
		AutoAimEnabled = enabled;
		if (!AutoAimEnabled)
		{
			_lockedAimTarget = null;
			ApplyAutoAimMarker(active: false, _aimWorldPosition, suppressMouseCursor: false);
		}
	}

	private void ResolveAutoAimReferences()
	{
		_cursorRing = GetNodeOrNull<CursorRing>(CursorRingPath);
	}

	private void UpdateAimWorldPosition()
	{
		AutoAimMode mode = ResolveAutoAimMode();
		if (mode == AutoAimMode.None)
		{
			_lockedAimTarget = null;
			_aimWorldPosition = GetGlobalMousePosition();
			ApplyAutoAimMarker(active: false, _aimWorldPosition, suppressMouseCursor: false);
			return;
		}

		RefreshLockedTarget(mode);
		if (IsValidLockedTarget(_lockedAimTarget))
		{
			_aimWorldPosition = _lockedAimTarget.GlobalPosition;
			ApplyAutoAimMarker(active: true, _aimWorldPosition, suppressMouseCursor: true);
			return;
		}

		_aimWorldPosition = GetGlobalMousePosition();
		ApplyAutoAimMarker(active: false, _aimWorldPosition, suppressMouseCursor: true);
	}

	private AutoAimMode ResolveAutoAimMode()
	{
		bool hasGamepad = Input.GetConnectedJoypads().Count > 0;
		if (EnableGamepadAutoAim && hasGamepad && InputDeviceService.IsGamepadActive)
			return AutoAimMode.Gamepad;

		if (!AutoAimEnabled)
			return AutoAimMode.None;

		if (EnableMouseAutoAim)
			return AutoAimMode.Mouse;
		return AutoAimMode.None;
	}

	private void RefreshLockedTarget(AutoAimMode mode)
	{
		switch (mode)
		{
			case AutoAimMode.Gamepad:
				RefreshGamepadLockedTarget();
				break;
			case AutoAimMode.Mouse:
				_lockedAimTarget = FindNearestTargetToPoint(GetGlobalMousePosition());
				break;
			default:
				_lockedAimTarget = null;
				break;
		}
	}

	private void RefreshGamepadLockedTarget()
	{
		Vector2 stickVector = InputDeviceService.GetActiveRightStickVector(RightStickDeadzone);
		if (stickVector.LengthSquared() < 0.0001f)
		{
			if (!IsValidLockedTarget(_lockedAimTarget))
				_lockedAimTarget = FindNearestTargetToPoint(GlobalPosition);
			return;
		}

		Vector2 stickDirection = stickVector.Normalized();
		EnemyHurtbox bestTarget = FindDirectionalTarget(stickDirection);
		if (bestTarget != null)
			_lockedAimTarget = bestTarget;
	}

	private EnemyHurtbox FindDirectionalTarget(Vector2 stickDirection)
	{
		if (GetTree() == null)
			return null;

		float bestScore = float.NegativeInfinity;
		EnemyHurtbox bestTarget = null;
		float maxDistance = Mathf.Max(80f, AutoAimMaxDistance);
		var candidates = GetTree().GetNodesInGroup(RuntimeGroups.EnemyHurtbox);
		foreach (Node node in candidates)
		{
			if (node is not EnemyHurtbox hurtbox || !IsValidLockedTarget(hurtbox))
				continue;

			Vector2 toTarget = hurtbox.GlobalPosition - GlobalPosition;
			float distance = toTarget.Length();
			if (distance <= 1f || distance > maxDistance)
				continue;

			Vector2 direction = toTarget / distance;
			float dot = direction.Dot(stickDirection);
			if (dot < Mathf.Clamp(RightStickDirectionDotThreshold, 0f, 1f))
				continue;

			float distanceScore = 1f - Mathf.Clamp(distance / maxDistance, 0f, 1f);
			float score = (dot * 0.78f) + (distanceScore * 0.22f);
			if (hurtbox == _lockedAimTarget)
				score += Mathf.Clamp(LockStickiness, 0f, 1f);

			if (score <= bestScore)
				continue;

			bestScore = score;
			bestTarget = hurtbox;
		}

		if (bestTarget != null)
			return bestTarget;
		return IsValidLockedTarget(_lockedAimTarget) ? _lockedAimTarget : null;
	}

	private EnemyHurtbox FindNearestTargetToPoint(Vector2 point)
	{
		if (GetTree() == null)
			return null;

		float bestDistanceSq = float.PositiveInfinity;
		EnemyHurtbox bestTarget = null;
		float maxDistanceSq = Mathf.Max(80f, AutoAimMaxDistance);
		maxDistanceSq *= maxDistanceSq;
		var candidates = GetTree().GetNodesInGroup(RuntimeGroups.EnemyHurtbox);
		foreach (Node node in candidates)
		{
			if (node is not EnemyHurtbox hurtbox || !IsValidLockedTarget(hurtbox))
				continue;

			float distanceSq = point.DistanceSquaredTo(hurtbox.GlobalPosition);
			if (distanceSq > maxDistanceSq)
				continue;

			if (distanceSq >= bestDistanceSq)
				continue;
			bestDistanceSq = distanceSq;
			bestTarget = hurtbox;
		}

		return bestTarget;
	}

	private static bool IsValidLockedTarget(EnemyHurtbox target)
	{
		if (target == null || !GodotObject.IsInstanceValid(target))
			return false;
		return !target.IsDead;
	}

	private void ApplyAutoAimMarker(bool active, Vector2 worldPosition, bool suppressMouseCursor)
	{
		if (!IsInstanceValid(_cursorRing))
			ResolveAutoAimReferences();
		if (!IsInstanceValid(_cursorRing))
			return;
		_cursorRing.SetAutoAimMarkerWorldPosition(worldPosition, active, suppressMouseCursor);
	}
}
