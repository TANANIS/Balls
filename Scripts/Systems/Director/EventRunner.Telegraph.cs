using Godot;
using System;

public sealed partial class EventRunner
{
	public string BuildEventHintTextForUi()
	{
		return IsRunning ? BuildEventHintText() : string.Empty;
	}

	public void FillTelegraphSnapshot(EventTelegraphSnapshot snapshot)
	{
		if (snapshot == null)
			return;

		snapshot.Clear();
		if (!IsRunning || _activeSlot == null)
			return;

		snapshot.EventId = _activeSlot.EventId ?? string.Empty;
		snapshot.EventName = _activeSlot.EventName ?? string.Empty;
		snapshot.DomainId = _activeSlot.DomainId ?? string.Empty;
		snapshot.EventHintText = BuildEventHintText();
		snapshot.HybridHintText = BuildHybridHintText();

		switch (_activeSlot.EventId)
		{
			case "EVT_ICE_ICESTORM":
				foreach (RuntimeZone zone in _iceZones)
				{
					snapshot.Circles.Add(new EventTelegraphCircle(
						zone.Center,
						Mathf.Max(8f, zone.Radius),
						new Color(0.47f, 0.82f, 1f, 0.18f),
						2.4f,
						filled: true));
				}
				break;
			case "EVT_ICE_FROZEN_PULSE":
				foreach (PulseRingState ring in _pulseRings)
				{
					float radius = Mathf.Max(8f, ring.Radius);
					float width = Mathf.Max(2f, ring.Thickness * 0.5f);
					snapshot.Circles.Add(new EventTelegraphCircle(
						ring.Center,
						radius,
						new Color(0.68f, 0.90f, 1f, 0.85f),
						width,
						filled: false));
				}
				break;
			case "EVT_SPACE_EVENT_HORIZON":
				if (_eventHorizonRadius > 0f)
				{
					snapshot.Circles.Add(new EventTelegraphCircle(
						_eventHorizonCenter,
						_eventHorizonRadius,
						new Color(0.60f, 0.38f, 0.98f, 0.15f),
						3.2f,
						filled: true));
				}
				break;
			case "EVT_SPACE_GRAVITY_WELL":
				foreach (GravityWellState well in _gravityWells)
				{
					snapshot.Circles.Add(new EventTelegraphCircle(
						well.Center,
						Mathf.Max(8f, well.Radius),
						new Color(0.66f, 0.48f, 1f, 0.16f),
						2.8f,
						filled: true));
					snapshot.Circles.Add(new EventTelegraphCircle(
						well.Center,
						Mathf.Max(6f, well.Radius * 0.34f),
						new Color(0.86f, 0.72f, 1f, 0.62f),
						2.2f,
						filled: false));
				}
				break;
			case "EVT_WAR_BLOOD_TIDE":
				snapshot.HasDirection = true;
				snapshot.DirectionOrigin = GodotObject.IsInstanceValid(_player)
					? _player.GlobalPosition
					: GetCurrentWorldRect().GetCenter();
				snapshot.DirectionVector = _bloodTideDirection;
				snapshot.DirectionColor = new Color(0.98f, 0.36f, 0.30f, 0.95f);
				break;
			case "EVT_WAR_BERSERK_MARK":
				AppendBerserkMarkers(snapshot);
				break;
		}
	}

	private void AppendBerserkMarkers(EventTelegraphSnapshot snapshot)
	{
		if (snapshot == null)
			return;

		if (GodotObject.IsInstanceValid(_player))
		{
			snapshot.Markers.Add(new EventTelegraphMarker(
				_player.GlobalPosition,
				20f,
				new Color(1f, 0.48f, 0.36f, 0.92f)));
		}

		if (!GodotObject.IsInstanceValid(_enemiesRoot) || _berserkEnemyIds.Count <= 0)
			return;

		foreach (Node child in _enemiesRoot.GetChildren())
		{
			if (child is not Enemy enemy)
				continue;
			if (!_berserkEnemyIds.Contains(enemy.GetInstanceId()))
				continue;
			if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
				continue;

			snapshot.Markers.Add(new EventTelegraphMarker(
				enemy.GlobalPosition,
				14f,
				new Color(0.98f, 0.40f, 0.34f, 0.86f)));
		}
	}

	private string BuildEventHintText()
	{
		return _activeSlot?.EventId switch
		{
			"EVT_ICE_ICESTORM" => "IceStorm: ice patches refresh every 3s and slow movement.",
			"EVT_ICE_FROZEN_PULSE" => "Frozen Pulse: center ring expands and applies short slow.",
			"EVT_WAR_BLOOD_TIDE" => $"Blood Tide: rush waves from {GetDirectionLabel(_bloodTideDirection)} edge.",
			"EVT_WAR_BERSERK_MARK" => "Berserk Mark: player + marked enemies gain rage speed.",
			"EVT_SPACE_EVENT_HORIZON" => "Event Horizon: compression field slows and reduces range.",
			"EVT_SPACE_GRAVITY_WELL" => "Gravity Well: active pull zones drag players and enemies.",
			_ => string.Empty
		};
	}

	private string BuildHybridHintText()
	{
		if (!IsHybridActive())
			return string.Empty;
		return _activeSlot.HybridVariantId switch
		{
			HybridIceSpaceGlacialHorizon => "Hybrid Resonance: Glacial Horizon active.",
			HybridSpaceWarWarpAssault => "Hybrid Resonance: Warp Assault active.",
			_ => "Hybrid Resonance active."
		};
	}

	private static string GetDirectionLabel(Vector2 dir)
	{
		if (dir == Vector2.Left)
			return "LEFT";
		if (dir == Vector2.Right)
			return "RIGHT";
		if (dir == Vector2.Up)
			return "TOP";
		if (dir == Vector2.Down)
			return "BOTTOM";
		return "UNKNOWN";
	}
}
