using Godot;

public partial class ObstacleFieldGenerator
{
	private void SpawnBatch(int count)
	{
		if (_variants.Count == 0)
			return;
		if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float distanceScale = Mathf.Clamp(SpawnDistanceScale, 0.5f, 1.2f);
		float minDist = (Mathf.Max(halfVisible.X, halfVisible.Y) * distanceScale) + Mathf.Max(0f, SpawnOutsideMargin);
		float maxDist = minDist + Mathf.Max(60f, SpawnRingThickness);
		_runtimeMinObstacleSpacingWorld = GetMinimumObstacleSpacingWorld();
		PrepareCluster(playerPos, minDist, maxDist);

		for (int i = 0; i < count; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;
			bool spawned = false;
			int retries = Mathf.Clamp(SpawnVariantRetryCount, 1, 12);
			for (int attempt = 0; attempt < retries; attempt++)
			{
				int variantIndex = PickVariantIndex();
				ObstacleVariant variant = _variants[variantIndex];
				if (!TryFindPlacement(playerPos, minDist, maxDist, variant.Radius, variant.SpacingBias, out Vector2 pos))
					continue;
				if (!PassesTerrainAffinity(variant, pos))
					continue;
				if (!PassesEnvironmentRule(variant, pos, variant.Radius))
					continue;

				if (CreateObstacle(variant.Scene, pos))
				{
					_placed.Add(new PlacedObstacle(pos, variant.Radius, variantIndex, variant.SpacingBias));
					_lastVariantIndex = variantIndex;
					if (_clusterRemaining > 0)
						_clusterRemaining--;
					spawned = true;
					break;
				}
			}
			if (!spawned)
				continue;
		}
	}

	private int PickVariantIndex()
	{
		if (_variants.Count <= 1)
			return 0;

		float total = 0f;
		var weights = new float[_variants.Count];
		for (int i = 0; i < _variants.Count; i++)
		{
			float w = Mathf.Max(0.01f, _variants[i].Weight);
			// Soften streaks, but keep natural randomness.
			if (EnforceAlternatingTypes && _lastVariantIndex == i)
				w *= 0.45f;
			weights[i] = w;
			total += w;
		}

		float roll = _rng.RandfRange(0f, total);
		for (int i = 0; i < weights.Length; i++)
		{
			roll -= weights[i];
			if (roll <= 0f)
				return i;
		}

		return weights.Length - 1;
	}

	private bool TryFindPlacement(Vector2 playerPos, float minDist, float maxDist, float radius, float spacingBias, out Vector2 pos)
	{
		float effectiveRadius = Mathf.Max(24f, radius);
		for (int attempt = 0; attempt < PlacementAttemptsPerSpawn; attempt++)
		{
			if (TrySamplePosition(playerPos, minDist, maxDist, out pos) &&
				IsPlacementValid(pos, effectiveRadius, spacingBias))
				return true;
		}

		pos = Vector2.Zero;
		return false;
	}

	private void PrepareCluster(Vector2 playerPos, float minDist, float maxDist)
	{
		if (!UseNaturalClusters)
		{
			_hasClusterCenter = false;
			_clusterRemaining = 0;
			return;
		}

		if (_hasClusterCenter && _clusterRemaining > 0)
			return;

		float angle = _rng.RandfRange(0f, Mathf.Tau);
		float dist = _rng.RandfRange(minDist, maxDist);
		Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		_clusterCenter = playerPos + (dir * dist);
		_clusterRemaining = Mathf.Max(1, ClusterLifetimeSpawns);
		_hasClusterCenter = true;
	}

	private bool TrySamplePosition(Vector2 playerPos, float minDist, float maxDist, out Vector2 pos)
	{
		if (UseNaturalClusters && _hasClusterCenter && _clusterRemaining > 0 && _rng.Randf() <= Mathf.Clamp(ClusterStickiness, 0f, 1f))
		{
			float radius = _rng.RandfRange(Mathf.Max(8f, ClusterRadiusMin), Mathf.Max(ClusterRadiusMin, ClusterRadiusMax));
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			float r = Mathf.Sqrt(_rng.Randf()) * radius;
			pos = _clusterCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
			float toPlayer = pos.DistanceTo(playerPos);
			if (toPlayer >= minDist && toPlayer <= maxDist)
				return true;
		}

		float ringAngle = _rng.RandfRange(0f, Mathf.Tau);
		float ringDist = _rng.RandfRange(minDist, maxDist);
		Vector2 ringDir = new Vector2(Mathf.Cos(ringAngle), Mathf.Sin(ringAngle));
		pos = playerPos + (ringDir * ringDist);
		return true;
	}

	private bool IsPlacementValid(Vector2 pos, float radius, float spacingBias)
	{
		float spacingFloor = _runtimeMinObstacleSpacingWorld > 0f
			? _runtimeMinObstacleSpacingWorld
			: GetMinimumObstacleSpacingWorld();

		foreach (PlacedObstacle existing in _placed)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.8f, ObstacleSpacingMultiplier * pairBias);
			spacing = Mathf.Max(spacing, spacingFloor);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		return true;
	}

	private bool IsDecorationPlacementValid(Vector2 pos, float radius, float spacingBias)
	{
		foreach (PlacedObstacle existing in _placed)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.45f, DecorationSpacingMultiplier * pairBias);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		foreach (PlacedObstacle existing in _placedDecor)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.35f, DecorationSpacingMultiplier * pairBias);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		return true;
	}

	private void SpawnInitialInsideView()
	{
		int target = Mathf.Max(0, InitialInsideViewSpawnCount);
		if (target == 0 || _variants.Count == 0)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 center = camera != null ? camera.GetScreenCenterPosition() : playerPos;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float pad = Mathf.Max(0f, InitialInsidePadding);
		Rect2 rect = new(
			center - halfVisible + new Vector2(pad, pad),
			(halfVisible * 2f) - new Vector2(pad * 2f, pad * 2f));
		if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
			return;
		_runtimeMinObstacleSpacingWorld = GetMinimumObstacleSpacingWorld();

		float safeRadius = Mathf.Max(0f, InitialInsideSafeRadius);
		int attempts = Mathf.Max(PlacementAttemptsPerSpawn * 2, 32);
		int spawned = 0;
		for (int i = 0; i < target; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;
			int retries = Mathf.Clamp(SpawnVariantRetryCount, 1, 12);
			for (int attempt = 0; attempt < retries; attempt++)
			{
				int variantIndex = PickVariantIndex();
				ObstacleVariant variant = _variants[variantIndex];
				if (!TryFindPlacementInRect(rect, playerPos, safeRadius, variant.Radius, variant.SpacingBias, attempts, out Vector2 pos))
					continue;
				if (!PassesTerrainAffinity(variant, pos))
					continue;
				if (!PassesEnvironmentRule(variant, pos, variant.Radius))
					continue;

				if (!CreateObstacle(variant.Scene, pos))
					continue;
				_placed.Add(new PlacedObstacle(pos, variant.Radius, variantIndex, variant.SpacingBias));
				_lastVariantIndex = variantIndex;
				spawned++;
				break;
			}
		}
	}

	private bool TryFindPlacementInRect(
		Rect2 rect,
		Vector2 playerPos,
		float safeRadius,
		float radius,
		float spacingBias,
		int attempts,
		out Vector2 pos)
	{
		float effectiveRadius = Mathf.Max(24f, radius);
		for (int i = 0; i < attempts; i++)
		{
			pos = new Vector2(
				_rng.RandfRange(rect.Position.X, rect.End.X),
				_rng.RandfRange(rect.Position.Y, rect.End.Y));
			if (pos.DistanceTo(playerPos) < safeRadius)
				continue;
			if (IsPlacementValid(pos, effectiveRadius, spacingBias))
				return true;
		}

		pos = Vector2.Zero;
		return false;
	}

	private void SpawnInitialDecorationsInsideView()
	{
		SpawnDecorationInView(Mathf.Max(0, InitialDecorationInsideViewCount));
	}

	private void SpawnDecorationBatch(int count)
	{
		if (count <= 0)
			return;
		SpawnDecorationInView(count);
	}

	private void SpawnDecorationInView(int count)
	{
		if (_decoVariants.Count == 0)
			return;
		if (MaxDecorationCount > 0 && _placedDecor.Count >= MaxDecorationCount)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 center = camera != null ? camera.GetScreenCenterPosition() : playerPos;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		Rect2 rect = new(center - halfVisible, halfVisible * 2f);
		float safeRadius = Mathf.Max(0f, InitialInsideSafeRadius * 0.5f);

		int attempts = Mathf.Max(PlacementAttemptsPerSpawn, 18);
		for (int i = 0; i < count; i++)
		{
			if (MaxDecorationCount > 0 && _placedDecor.Count >= MaxDecorationCount)
				break;
			ObstacleVariant variant = PickDecorationVariant();
			if (!TryFindDecorationPlacementInRect(rect, playerPos, safeRadius, variant, attempts, out Vector2 pos))
				continue;
			if (!PassesEnvironmentRule(variant, pos, variant.Radius))
				continue;
			if (!CreateDecoration(variant.Scene, pos))
				continue;
			_placedDecor.Add(new PlacedObstacle(pos, variant.Radius, -1, variant.SpacingBias));
		}
	}

	private ObstacleVariant PickDecorationVariant()
	{
		float total = 0f;
		foreach (ObstacleVariant v in _decoVariants)
			total += Mathf.Max(0.01f, v.Weight);
		float roll = _rng.RandfRange(0f, total);
		foreach (ObstacleVariant v in _decoVariants)
		{
			roll -= Mathf.Max(0.01f, v.Weight);
			if (roll <= 0f)
				return v;
		}
		return _decoVariants[_decoVariants.Count - 1];
	}

	private bool TryFindDecorationPlacementInRect(
		Rect2 rect,
		Vector2 playerPos,
		float safeRadius,
		ObstacleVariant variant,
		int attempts,
		out Vector2 pos)
	{
		for (int i = 0; i < attempts; i++)
		{
			pos = new Vector2(
				_rng.RandfRange(rect.Position.X, rect.End.X),
				_rng.RandfRange(rect.Position.Y, rect.End.Y));
			if (pos.DistanceTo(playerPos) < safeRadius)
				continue;
			if (!IsDecorationPlacementValid(pos, variant.Radius, variant.SpacingBias))
				continue;
			return true;
		}

		pos = Vector2.Zero;
		return false;
	}
}
