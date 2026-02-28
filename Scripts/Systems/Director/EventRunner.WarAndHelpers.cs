using Godot;
using System.Collections.Generic;

public sealed partial class EventRunner
{
	private void SpawnBloodTideRush()
	{
		if (_spawnSystem == null)
			return;

		int tier = Mathf.Clamp(_activeSlot?.ResolvedTierIndex ?? 0, 0, 3);
		int count = Mathf.Clamp(4 + tier + (IsD1() ? 1 : 0), 4, 10);
		if (IsHybridSpaceWar())
			count = Mathf.Min(12, count + 2);
		_spawnSystem.EventSpawnDirectionalRush(_bloodTideDirection, count, includeEliteBias: IsD1());
	}

	private void TagBerserkTargets()
	{
		_berserkEnemyIds.Clear();
		if (!GodotObject.IsInstanceValid(_enemiesRoot))
			return;

		var candidates = new List<Enemy>();
		foreach (Node child in _enemiesRoot.GetChildren())
		{
			if (child is not Enemy enemy)
				continue;
			if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
				continue;
			candidates.Add(enemy);
		}

		int tier = Mathf.Clamp(_activeSlot?.ResolvedTierIndex ?? 0, 0, 3);
		int targetCount = Mathf.Clamp(3 + tier + (IsD1() ? 2 : 0), 2, 12);
		if (IsHybridSpaceWar())
			targetCount = Mathf.Min(14, targetCount + 2);
		targetCount = Mathf.Min(targetCount, candidates.Count);
		for (int i = 0; i < targetCount; i++)
		{
			int pick = _rng.RandiRange(0, candidates.Count - 1);
			Enemy enemy = candidates[pick];
			_berserkEnemyIds.Add(enemy.GetInstanceId());
			candidates.RemoveAt(pick);
		}
	}

	private void OnCombatEnemyKilled(Node source, Node target)
	{
		if (!_berserkExplosionOnDeath || target is not EnemyHurtbox hurtbox)
			return;
		if (hurtbox.GetParent() is not Enemy deadEnemy)
			return;
		if (!_berserkEnemyIds.Contains(deadEnemy.GetInstanceId()))
			return;
		if (_combatSystem == null)
			return;

		Vector2 center = deadEnemy.GlobalPosition;
		float radius = IsHybridSpaceWar() ? 118f : 96f;
		const int damage = 1;
		ulong deadId = deadEnemy.GetInstanceId();

		DamageRequest? reqToPlayer = TryBuildRadiusDamageToFirst(RuntimeGroups.PlayerHurtbox, center, radius, damage);
		if (reqToPlayer != null)
			_combatSystem.RequestDamage(reqToPlayer.Value);

		var enemyHurtboxes = _owner.GetTree().GetNodesInGroup(RuntimeGroups.EnemyHurtbox);
		foreach (Node node in enemyHurtboxes)
		{
			if (node is not Node2D hurt || hurt == hurtbox)
				continue;
			if (hurt.GetParent() is Enemy parentEnemy && parentEnemy.GetInstanceId() == deadId)
				continue;
			if (hurt.GlobalPosition.DistanceTo(center) > radius)
				continue;

			var req = new DamageRequest(_owner, hurt, damage, center, "event_berserk_explosion");
			_combatSystem.RequestDamage(req);
		}
	}

	private DamageRequest? TryBuildRadiusDamageToFirst(string group, Vector2 center, float radius, int damage)
	{
		var list = _owner.GetTree().GetNodesInGroup(group);
		foreach (Node node in list)
		{
			if (node is not Node2D target)
				continue;
			if (target.GlobalPosition.DistanceTo(center) > radius)
				continue;
			return new DamageRequest(_owner, target, damage, center, "event_berserk_explosion");
		}

		return null;
	}

	private void BindCombatKillEventIfNeeded()
	{
		if (_subscribedCombatKilled || _combatSystem == null)
			return;
		_combatSystem.EnemyKilled += OnCombatEnemyKilled;
		_subscribedCombatKilled = true;
	}

	private void UnbindCombatKillEvent()
	{
		if (!_subscribedCombatKilled || _combatSystem == null)
			return;
		_combatSystem.EnemyKilled -= OnCombatEnemyKilled;
		_subscribedCombatKilled = false;
	}

	private Vector2 PickPointNearPlayer(float minDistance, float maxDistance)
	{
		Rect2 rect = GetCurrentWorldRect().Grow(-18f);
		Vector2 anchor = GodotObject.IsInstanceValid(_player) ? _player.GlobalPosition : rect.GetCenter();
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		float radius = _rng.RandfRange(Mathf.Max(0f, minDistance), Mathf.Max(minDistance + 1f, maxDistance));
		Vector2 point = anchor + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
		float minX = rect.Position.X;
		float maxX = rect.Position.X + rect.Size.X;
		float minY = rect.Position.Y;
		float maxY = rect.Position.Y + rect.Size.Y;
		point.X = Mathf.Clamp(point.X, minX, maxX);
		point.Y = Mathf.Clamp(point.Y, minY, maxY);
		return point;
	}

	private Vector2 PickCardinalDirection()
	{
		Vector2[] dirs = { Vector2.Right, Vector2.Left, Vector2.Up, Vector2.Down };
		return dirs[_rng.RandiRange(0, dirs.Length - 1)];
	}

	private Rect2 GetCurrentWorldRect()
	{
		Viewport viewport = _owner.GetViewport();
		if (viewport == null)
			return new Rect2(Vector2.Zero, new Vector2(1280f, 720f));

		Camera2D camera = viewport.GetCamera2D();
		if (camera == null)
		{
			Vector2 center = GodotObject.IsInstanceValid(_player) ? _player.GlobalPosition : Vector2.Zero;
			return new Rect2(center - new Vector2(640f, 360f), new Vector2(1280f, 720f));
		}

		Vector2 screenSize = viewport.GetVisibleRect().Size;
		Vector2 zoom = new Vector2(Mathf.Abs(camera.Zoom.X), Mathf.Abs(camera.Zoom.Y));
		Vector2 worldSize = new Vector2(screenSize.X * zoom.X, screenSize.Y * zoom.Y);
		return new Rect2(camera.GlobalPosition - (worldSize * 0.5f), worldSize);
	}

	private static ulong HashSeed(string eventId, int slotIndex)
	{
		ulong hash = 1469598103934665603ul;
		string value = $"{eventId}|{slotIndex}";
		for (int i = 0; i < value.Length; i++)
		{
			hash ^= value[i];
			hash *= 1099511628211ul;
		}

		return hash;
	}
}
