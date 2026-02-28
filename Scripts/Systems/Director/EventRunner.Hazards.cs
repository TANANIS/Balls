using Godot;
using System.Collections.Generic;

public sealed partial class EventRunner
{
	private void UpdateIceZones(float dt)
	{
		for (int i = _iceZones.Count - 1; i >= 0; i--)
		{
			RuntimeZone zone = _iceZones[i];
			zone.Remaining -= dt;
			if (zone.Remaining <= 0f)
				_iceZones.RemoveAt(i);
		}
	}

	private void SpawnIceZone()
	{
		int maxZones = IsD1() ? 6 : 4;
		if (IsHybridIceSpace())
			maxZones += 1;
		while (_iceZones.Count >= maxZones)
			_iceZones.RemoveAt(0);

		float radiusMult = IsHybridIceSpace() ? 1.10f : 1f;
		_iceZones.Add(new RuntimeZone
		{
			Center = PickPointNearPlayer(80f, 360f),
			Radius = _rng.RandfRange(IsD1() ? 94f : 74f, IsD1() ? 132f : 106f) * radiusMult,
			Remaining = 8f
		});
	}

	private void EmitPulseRing()
	{
		Rect2 rect = GetCurrentWorldRect();
		Vector2 center = rect.GetCenter();
		float maxRadius = Mathf.Max(rect.Size.X, rect.Size.Y) * 0.75f;
		float thicknessMult = IsHybridIceSpace() ? 1.12f : 1f;
		_pulseRings.Add(new PulseRingState
		{
			Center = center,
			Radius = 0f,
			Speed = IsD1() ? 430f : 360f,
			Thickness = (IsD1() ? 72f : 58f) * thicknessMult,
			MaxRadius = Mathf.Max(120f, maxRadius)
		});
	}

	private void UpdatePulseRings(float dt)
	{
		for (int i = _pulseRings.Count - 1; i >= 0; i--)
		{
			PulseRingState ring = _pulseRings[i];
			ring.Radius += ring.Speed * dt;
			ApplyPulseRingHit(ring);
			if (ring.Radius >= ring.MaxRadius)
				_pulseRings.RemoveAt(i);
		}
	}

	private void ApplyPulseRingHit(PulseRingState ring)
	{
		float halfThickness = ring.Thickness * 0.5f;
		float slowSeconds = IsD1() ? 1.6f : 1.2f;
		if (IsHybridIceSpace())
			slowSeconds += 0.35f;

		if (GodotObject.IsInstanceValid(_player))
		{
			ulong playerId = _player.GetInstanceId();
			if (!ring.HitActorIds.Contains(playerId))
			{
				float dist = _player.GlobalPosition.DistanceTo(ring.Center);
				if (Mathf.Abs(dist - ring.Radius) <= halfThickness)
				{
					_playerSlowTimer = Mathf.Max(_playerSlowTimer, slowSeconds);
					ring.HitActorIds.Add(playerId);
				}
			}
		}

		if (!GodotObject.IsInstanceValid(_enemiesRoot))
			return;
		foreach (Node child in _enemiesRoot.GetChildren())
		{
			if (child is not Enemy enemy)
				continue;
			if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
				continue;

			ulong id = enemy.GetInstanceId();
			if (ring.HitActorIds.Contains(id))
				continue;
			float dist = enemy.GlobalPosition.DistanceTo(ring.Center);
			if (Mathf.Abs(dist - ring.Radius) > halfThickness)
				continue;

			_enemySlowTimers[id] = Mathf.Max(_enemySlowTimers.TryGetValue(id, out float t) ? t : 0f, slowSeconds);
			ring.HitActorIds.Add(id);
		}
	}

	private void MoveEventHorizon(float dt)
	{
		_eventHorizonCenter += _eventHorizonVelocity * dt;
		Rect2 rect = GetCurrentWorldRect().Grow(-Mathf.Max(24f, _eventHorizonRadius * 0.35f));
		if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
			return;

		float minX = rect.Position.X;
		float maxX = rect.Position.X + rect.Size.X;
		float minY = rect.Position.Y;
		float maxY = rect.Position.Y + rect.Size.Y;
		if (_eventHorizonCenter.X < minX || _eventHorizonCenter.X > maxX)
			_eventHorizonVelocity.X *= -1f;
		if (_eventHorizonCenter.Y < minY || _eventHorizonCenter.Y > maxY)
			_eventHorizonVelocity.Y *= -1f;
		_eventHorizonCenter.X = Mathf.Clamp(_eventHorizonCenter.X, minX, maxX);
		_eventHorizonCenter.Y = Mathf.Clamp(_eventHorizonCenter.Y, minY, maxY);
	}

	private void SpawnGravityWells()
	{
		int tier = Mathf.Clamp(_activeSlot?.ResolvedTierIndex ?? 0, 0, 3);
		int count = IsD1() ? 2 : 1;
		if (IsHybridSpaceWar())
			count += 1;
		float baseRadius = Mathf.Lerp(140f, 200f, tier / 3f) * (IsD1() ? 1.15f : 1f);
		if (IsHybridIceSpace())
			baseRadius *= 1.08f;
		float strength = IsD1() ? 238f : 190f;
		if (IsHybridSpaceWar())
			strength *= 1.15f;
		for (int i = 0; i < count; i++)
		{
			_gravityWells.Add(new GravityWellState
			{
				Center = PickPointNearPlayer(80f, 340f),
				Radius = baseRadius,
				Strength = strength,
				Remaining = IsD1() ? 4.6f : 4.0f
			});
		}
	}

	private void UpdateGravityWells(float dt)
	{
		for (int i = _gravityWells.Count - 1; i >= 0; i--)
		{
			GravityWellState well = _gravityWells[i];
			well.Remaining -= dt;
			if (well.Remaining <= 0f)
				_gravityWells.RemoveAt(i);
		}
	}

	private bool IsInsideZones(Vector2 worldPos, List<RuntimeZone> zones)
	{
		foreach (RuntimeZone zone in zones)
		{
			if (worldPos.DistanceTo(zone.Center) <= zone.Radius)
				return true;
		}

		return false;
	}

	private bool IsInsideEventHorizon(Vector2 worldPos)
	{
		return _eventHorizonRadius > 0f && worldPos.DistanceTo(_eventHorizonCenter) <= _eventHorizonRadius;
	}
}
