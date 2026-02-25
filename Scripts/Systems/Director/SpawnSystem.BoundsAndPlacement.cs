using Godot;
using System.Collections.Generic;

public partial class SpawnSystem
{
	private List<Vector2> BuildPackCenters(int packs)
	{
		var centers = new List<Vector2>(Mathf.Max(1, packs));
		if (packs <= 0)
			return centers;

		if (!UseEncirclementPackLayout || packs == 1)
		{
			for (int i = 0; i < packs; i++)
				centers.Add(GetSpawnPositionAroundPlayer());
			return centers;
		}

		GetSpawnRadiusRange(out float radiusMin, out float radiusMax);
		float centerMin = Mathf.Max(radiusMin * Mathf.Clamp(PackCenterRadiusBias, 0.35f, 1.8f), 1f);
		float centerMax = Mathf.Max(centerMin + 1f, radiusMax);
		float baseAngle = _rng.RandfRange(0f, Mathf.Tau);
		float jitterRadians = Mathf.DegToRad(Mathf.Max(0f, PackAngleJitterDegrees));
		float spreadRadians = Mathf.DegToRad(Mathf.Clamp(PackInterceptSpreadDegrees, 10f, 170f));
		Vector2 anchor = _player.GlobalPosition;
		bool hasMotionIntercept = false;
		Vector2 moveDir = Vector2.Right;

		if (UsePlayerPathInterceptCenters && _player is CharacterBody2D movingPlayer)
		{
			Vector2 v = movingPlayer.Velocity;
			float speed = v.Length();
			if (speed >= Mathf.Max(1f, InterceptVelocityThreshold))
			{
				hasMotionIntercept = true;
				moveDir = v / speed;
				baseAngle = moveDir.Angle();
				float leadDistance = Mathf.Clamp(speed * Mathf.Max(0f, InterceptLeadSeconds), 35f, 260f);
				anchor += moveDir * leadDistance * Mathf.Clamp(InterceptForwardBias, 0f, 1f);
			}
		}

		float[] interceptPattern =
		{
			0f,
			spreadRadians,
			-spreadRadians,
			spreadRadians * 1.95f,
			-spreadRadians * 1.95f,
			Mathf.Pi
		};

		for (int i = 0; i < packs; i++)
		{
			float slotAngle = hasMotionIntercept
				? baseAngle + interceptPattern[i % interceptPattern.Length]
				: baseAngle + (Mathf.Tau * i / packs);
			float jitter = _rng.RandfRange(-jitterRadians, jitterRadians);
			float angle = slotAngle + jitter;
			float radius = _rng.RandfRange(centerMin, centerMax);
			if (hasMotionIntercept)
			{
				float dot = Mathf.Cos(angle - baseAngle);
				if (dot > 0.35f)
					radius *= 0.84f; // Forward packs cut off player path tighter.
				else if (dot < -0.45f)
					radius *= 1.08f; // Rear packs keep escape-closing pressure.
			}

			Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			Vector2 candidate = anchor + dir * radius;
			Vector2 fromPlayer = candidate - _player.GlobalPosition;
			float distToPlayer = fromPlayer.Length();
			if (distToPlayer < radiusMin)
			{
				Vector2 safeDir = distToPlayer > 0.001f ? fromPlayer / distToPlayer : dir;
				candidate = _player.GlobalPosition + safeDir * radiusMin;
			}

			centers.Add(candidate);
		}

		return centers;
	}

	private Vector2 GetSpawnPositionAroundPlayer()
	{
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		GetSpawnRadiusRange(out float radiusMin, out float radiusMax);
		float radius = _rng.RandfRange(radiusMin, radiusMax);

		Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		return _player.GlobalPosition + offset;
	}

	private void GetSpawnRadiusRange(out float radiusMin, out float radiusMax)
	{
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		var camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float minVisibleRadius = Mathf.Max(halfVisible.X, halfVisible.Y) + Mathf.Max(0f, SpawnOutsideViewportMargin);
		radiusMin = Mathf.Max(_activeSpawnRadiusMin, minVisibleRadius);
		radiusMax = Mathf.Max(radiusMin + 1f, Mathf.Max(_activeSpawnRadiusMax, radiusMin + Mathf.Max(1f, SpawnRingThickness)));
	}

	private bool TryFindPackOffset(List<Vector2> usedOffsets, out Vector2 offset)
	{
		float radiusMax = Mathf.Max(1f, PackScatterRadius);
		float minSpacing = Mathf.Max(1f, PackMinSpacing);
		int attempts = Mathf.Max(1, PackPlacementAttempts);

		for (int attempt = 0; attempt < attempts; attempt++)
		{
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			float radius = _rng.RandfRange(0f, radiusMax);
			Vector2 candidate = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

			bool overlap = false;
			for (int i = 0; i < usedOffsets.Count; i++)
			{
				if (candidate.DistanceTo(usedOffsets[i]) < minSpacing)
				{
					overlap = true;
					break;
				}
			}

			if (!overlap)
			{
				offset = candidate;
				return true;
			}
		}

		offset = Vector2.Zero;
		return false;
	}
}
