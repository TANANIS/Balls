using Godot;

public partial class ObstacleFieldGenerator
{
	private bool PassesTerrainAffinity(ObstacleVariant variant, Vector2 worldPos)
	{
		if (variant.Affinity == TerrainAffinity.Any || !GodotObject.IsInstanceValid(_terrainBackground))
			return true;

		int probeRadius = Mathf.Clamp(TerrainAffinityProbeRadiusTiles, 0, 16);
		float dirtRatio = SampleDirtRatio(worldPos, probeRadius);
		float strength = Mathf.Clamp(TerrainAffinityStrength, 0f, 1f);
		float target = variant.Affinity == TerrainAffinity.PreferDirt ? dirtRatio : 1f - dirtRatio;
		// Keep soft randomness, but strongly favor matching terrain to avoid large empty zones.
		float accept = Mathf.Lerp(0.85f, Mathf.Clamp(target, 0f, 1f), strength);
		accept = Mathf.Clamp(accept, 0.08f, 0.98f);
		return _rng.Randf() <= accept;
	}

	private bool PassesEnvironmentRule(ObstacleVariant variant, Vector2 worldPos, float radiusWorld)
	{
		if (GodotObject.IsInstanceValid(_terrainBackground) && AvoidTerrainEdges)
		{
			int guardTiles = Mathf.Clamp(TerrainEdgeGuardTiles, 1, 4);
			float edgeRadius = radiusWorld;
			// Trees have large colliders; using full radius over-rejects them near valid interiors.
			// Keep anti-edge rule, but sample with a reduced footprint for large foliage.
			if (variant.Kind == ObstacleKind.Tree)
				edgeRadius *= 0.35f;
			else if (variant.Kind == ObstacleKind.Bush)
				edgeRadius *= 0.55f;
			if (IsNearTerrainBoundary(worldPos, guardTiles, edgeRadius))
				return false;
		}

		if (variant.Kind == ObstacleKind.GrassDeco && GodotObject.IsInstanceValid(_terrainBackground))
		{
			// Deco grass should stay on grass tiles.
			if (_terrainBackground.IsDirtAtWorldPosition(worldPos))
				return false;
		}

		if (variant.Kind == ObstacleKind.Tree || variant.Kind == ObstacleKind.Bush)
		{
			float biome = SampleVegetationBiome(worldPos);
			float threshold = Mathf.Clamp(VegetationBiomeThreshold, 0f, 1f);
			float strength = Mathf.Clamp(VegetationBiomeStrength, 0f, 1f);
			float localChance = Mathf.Clamp((biome - threshold) / Mathf.Max(0.001f, 1f - threshold), 0f, 1f);
			float accept = Mathf.Lerp(1f, localChance, strength);
			// Prevent very large grass zones from becoming visually empty.
			if (variant.Kind == ObstacleKind.Tree)
				accept = Mathf.Max(accept, 0.68f);
			else
				accept = Mathf.Max(accept, 0.60f);
			if (_rng.Randf() > accept)
				return false;

			// Tree species should prefer local species biome, creating same-species groves.
			if (variant.Kind == ObstacleKind.Tree && variant.SpeciesId >= 0)
			{
				int preferredSpecies = SampleTreeSpeciesGroup(worldPos);
				float clusterStrength = Mathf.Clamp(TreeSpeciesClusterStrength, 0f, 1f);
				float speciesAccept = preferredSpecies == variant.SpeciesId
					? Mathf.Lerp(1f, 0.96f, clusterStrength)
					: Mathf.Lerp(1f, 0.72f, clusterStrength);
				if (_rng.Randf() > speciesAccept)
					return false;
			}
		}

		return true;
	}

	private float SampleDirtRatio(Vector2 worldPos, int probeRadiusTiles)
	{
		if (!GodotObject.IsInstanceValid(_terrainBackground))
			return 0.5f;

		float tileSpan = _terrainBackground.GetTileWorldSpan();
		int total = 0;
		int dirt = 0;
		for (int oy = -probeRadiusTiles; oy <= probeRadiusTiles; oy++)
		{
			for (int ox = -probeRadiusTiles; ox <= probeRadiusTiles; ox++)
			{
				total++;
				Vector2 p = worldPos + new Vector2(ox * tileSpan, oy * tileSpan);
				if (_terrainBackground.IsDirtAtWorldPosition(p))
					dirt++;
			}
		}

		if (total <= 0)
			return 0.5f;
		return (float)dirt / total;
	}

	private bool IsNearTerrainBoundary(Vector2 worldPos, int guardTiles, float radiusWorld)
	{
		if (!GodotObject.IsInstanceValid(_terrainBackground))
			return false;

		float step = _terrainBackground.GetTileWorldSpan();
		int footprintTiles = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(0f, radiusWorld) / Mathf.Max(1f, step * 3f)), 0, 2);
		int scanTiles = Mathf.Clamp(guardTiles + footprintTiles, 1, 6);
		bool centerIsDirt = _terrainBackground.IsDirtAtWorldPosition(worldPos);

		// Strict boundary guard:
		// if any sampled tile in guard area belongs to the opposite terrain type,
		// this position is treated as an edge and rejected.
		for (int oy = -scanTiles; oy <= scanTiles; oy++)
		{
			for (int ox = -scanTiles; ox <= scanTiles; ox++)
			{
				if (ox == 0 && oy == 0)
					continue;
				Vector2 p = worldPos + new Vector2(ox * step, oy * step);
				if (_terrainBackground.IsDirtAtWorldPosition(p) != centerIsDirt)
					return true;
			}
		}

		return false;
	}

	private float SampleVegetationBiome(Vector2 worldPos)
	{
		float scale = Mathf.Max(6f, VegetationBiomeScaleTiles);
		Vector2 p = worldPos / scale;
		int x0 = Mathf.FloorToInt(p.X);
		int y0 = Mathf.FloorToInt(p.Y);
		int x1 = x0 + 1;
		int y1 = y0 + 1;
		float tx = p.X - x0;
		float ty = p.Y - y0;
		float sx = tx * tx * (3f - (2f * tx));
		float sy = ty * ty * (3f - (2f * ty));

		float v00 = Hash01(x0, y0, _biomeSalt);
		float v10 = Hash01(x1, y0, _biomeSalt);
		float v01 = Hash01(x0, y1, _biomeSalt);
		float v11 = Hash01(x1, y1, _biomeSalt);
		float ix0 = Mathf.Lerp(v00, v10, sx);
		float ix1 = Mathf.Lerp(v01, v11, sx);
		return Mathf.Lerp(ix0, ix1, sy);
	}

	private int SampleTreeSpeciesGroup(Vector2 worldPos)
	{
		float scale = Mathf.Max(8f, TreeSpeciesBiomeScaleTiles);
		Vector2 p = worldPos / scale;
		int gx = Mathf.FloorToInt(p.X);
		int gy = Mathf.FloorToInt(p.Y);
		float n = Hash01(gx, gy, _speciesSalt);
		return Mathf.Clamp(Mathf.FloorToInt(n * 3f), 0, 2);
	}

	private static float Hash01(int x, int y, int salt)
	{
		unchecked
		{
			uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ (uint)(salt * 83492791);
			h ^= h >> 13;
			h *= 1274126177u;
			h ^= h >> 16;
			return (h & 65535u) / 65535f;
		}
	}
}
