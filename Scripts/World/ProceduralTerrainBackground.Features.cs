using Godot;
using System.Collections.Generic;

public partial class ProceduralTerrainBackground
{
	private const int FeatureChunkSalt = 6001;
	private const int PathChunkSalt = 7103;
	private const int PathChunkSize = 24;

	private void ApplyFeaturePass(Dictionary<Vector2I, bool> mask, Rect2I simRange)
	{
		ApplyIslandFeatures(mask, simRange);
		if (EnableMudPaths)
			ApplyMudPathFeatures(mask, simRange);
	}

	private void ApplyIslandFeatures(Dictionary<Vector2I, bool> mask, Rect2I simRange)
	{
		int chunkSize = Mathf.Max(4, IslandChunkSize);
		int rMin = Mathf.Max(1, IslandRadiusMin);
		int rMax = Mathf.Max(rMin, IslandRadiusMax);
		float chance = Mathf.Clamp(IslandChancePerChunk, 0f, 1f);
		int minChunkX = FloorDiv(simRange.Position.X, chunkSize);
		int maxChunkX = FloorDiv(simRange.End.X - 1, chunkSize);
		int minChunkY = FloorDiv(simRange.Position.Y, chunkSize);
		int maxChunkY = FloorDiv(simRange.End.Y - 1, chunkSize);

		for (int cy = minChunkY; cy <= maxChunkY; cy++)
		{
			for (int cx = minChunkX; cx <= maxChunkX; cx++)
			{
				Vector2I chunk = new(cx, cy);
				if (FeatureHash01(chunk, FeatureChunkSalt) > chance)
					continue;

				int x = cx * chunkSize;
				int y = cy * chunkSize;
				int localX = Mathf.FloorToInt(FeatureHash01(chunk, 6003) * (chunkSize - 1));
				int localY = Mathf.FloorToInt(FeatureHash01(chunk, 6007) * (chunkSize - 1));
				Vector2I center = new(x + localX, y + localY);
				if (!simRange.HasPoint(center))
					continue;

				if (!HasMostlyGrassNeighborhood(mask, center, rMax + 1))
					continue;

				int radius = Mathf.RoundToInt(Mathf.Lerp(rMin, rMax, FeatureHash01(chunk, 6011)));
				StampDirtBlob(mask, center, radius, 6029);
			}
		}
	}

	private void ApplyMudPathFeatures(Dictionary<Vector2I, bool> mask, Rect2I simRange)
	{
		int countPerChunk = Mathf.Max(0, MudPathCount);
		int lenMin = Mathf.Max(1, MudPathLengthMin);
		int lenMax = Mathf.Max(lenMin, MudPathLengthMax);
		int halfWidth = Mathf.Clamp(MudPathHalfWidth, 0, 2);
		int minChunkX = FloorDiv(simRange.Position.X, PathChunkSize);
		int maxChunkX = FloorDiv(simRange.End.X - 1, PathChunkSize);
		int minChunkY = FloorDiv(simRange.Position.Y, PathChunkSize);
		int maxChunkY = FloorDiv(simRange.End.Y - 1, PathChunkSize);

		for (int cy = minChunkY; cy <= maxChunkY; cy++)
		{
			for (int cx = minChunkX; cx <= maxChunkX; cx++)
			{
				int chunkOriginX = cx * PathChunkSize;
				int chunkOriginY = cy * PathChunkSize;
				for (int i = 0; i < countPerChunk; i++)
				{
					Vector2I seedKey = new((cx * 31) + i, (cy * 73) - i);
					if (FeatureHash01(seedKey, PathChunkSalt) < 0.4f)
						continue;

					Vector2I p = new(
						chunkOriginX + Mathf.FloorToInt(FeatureHash01(seedKey, 7109) * (PathChunkSize - 1)),
						chunkOriginY + Mathf.FloorToInt(FeatureHash01(seedKey, 7117) * (PathChunkSize - 1)));
					int length = Mathf.RoundToInt(Mathf.Lerp(lenMin, lenMax, FeatureHash01(seedKey, 7121)));
					Vector2I dir = PickCardinalDirection(FeatureHash01(seedKey, 7129));

					for (int step = 0; step < length; step++)
					{
						if (simRange.HasPoint(p))
							StampPathCell(mask, p, halfWidth);

						if (FeatureHash01(new Vector2I(seedKey.X, step), 7151) < 0.34f)
							dir = TurnDirection(dir, FeatureHash01(new Vector2I(seedKey.Y, step), 7159) < 0.5f);
						p += dir;
					}
				}
			}
		}
	}

	private static Vector2I PickCardinalDirection(float t)
	{
		if (t < 0.25f) return Vector2I.Right;
		if (t < 0.5f) return Vector2I.Left;
		if (t < 0.75f) return Vector2I.Up;
		return Vector2I.Down;
	}

	private static Vector2I TurnDirection(Vector2I dir, bool clockwise)
	{
		if (dir == Vector2I.Up) return clockwise ? Vector2I.Right : Vector2I.Left;
		if (dir == Vector2I.Right) return clockwise ? Vector2I.Down : Vector2I.Up;
		if (dir == Vector2I.Down) return clockwise ? Vector2I.Left : Vector2I.Right;
		return clockwise ? Vector2I.Up : Vector2I.Down;
	}

	private static void StampPathCell(Dictionary<Vector2I, bool> mask, Vector2I center, int halfWidth)
	{
		for (int oy = -halfWidth; oy <= halfWidth; oy++)
		{
			for (int ox = -halfWidth; ox <= halfWidth; ox++)
				mask[new Vector2I(center.X + ox, center.Y + oy)] = true;
		}
	}

	private void StampDirtBlob(Dictionary<Vector2I, bool> mask, Vector2I center, int radius, int saltBase)
	{
		int r = Mathf.Max(1, radius);
		float rSq = r * r;
		for (int oy = -r; oy <= r; oy++)
		{
			for (int ox = -r; ox <= r; ox++)
			{
				Vector2I p = new(center.X + ox, center.Y + oy);
				float dSq = (ox * ox) + (oy * oy);
				if (dSq > rSq)
					continue;

				float rimNoise = FeatureHash01(p, saltBase);
				float rimCut = 0.75f + (0.25f * rimNoise);
				if (dSq > (rSq * rimCut))
					continue;

				mask[p] = true;
			}
		}
	}

	private static bool HasMostlyGrassNeighborhood(Dictionary<Vector2I, bool> mask, Vector2I center, int radius)
	{
		int r = Mathf.Max(1, radius);
		int total = 0;
		int dirt = 0;
		for (int oy = -r; oy <= r; oy++)
		{
			for (int ox = -r; ox <= r; ox++)
			{
				total++;
				if (IsDirt(mask, new Vector2I(center.X + ox, center.Y + oy)))
					dirt++;
			}
		}

		if (total <= 0)
			return false;
		float dirtRatio = (float)dirt / total;
		return dirtRatio <= 0.25f;
	}

	private float FeatureHash01(Vector2I key, int salt)
	{
		uint h = HashSeeded(key, salt);
		return (h & 65535u) / 65535f;
	}

	private static int FloorDiv(int value, int divisor)
	{
		if (divisor <= 0)
			return 0;
		if (value >= 0)
			return value / divisor;
		return -(((-value) + divisor - 1) / divisor);
	}
}
