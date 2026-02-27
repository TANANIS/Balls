using Godot;
using System.Collections.Generic;

public partial class ProceduralTerrainBackground
{
	private const int BaseMaskChunkSize = 32;
	private readonly Dictionary<Vector2I, bool[]> _baseMaskChunkCache = new();
	private int _baseMaskCacheSignature = int.MinValue;

	private void InitializeNoiseSeed()
	{
		if (TerrainSeed < 0)
		{
			var rng = new RandomNumberGenerator();
			rng.Randomize();
			_runtimeTerrainSeed = (int)rng.Randi();
		}
		else
		{
			_runtimeTerrainSeed = TerrainSeed;
		}

		_seedOffsetBaseA = BuildSeedOffset(101);
		_seedOffsetBaseB = BuildSeedOffset(211);
		_seedOffsetWarpA = BuildSeedOffset(307);
		_seedOffsetWarpB = BuildSeedOffset(401);
		_seedOffsetRidge = BuildSeedOffset(503);
	}

	private Vector2 BuildSeedOffset(int salt)
	{
		float x = (SeedHash01((salt * 17) + 3) * 8192f) - 4096f;
		float y = (SeedHash01((salt * 17) + 7) * 8192f) - 4096f;
		return new Vector2(x, y);
	}

	private float SeedHash01(int salt)
	{
		uint h = Hash(new Vector2I(_runtimeTerrainSeed, salt), 92821);
		return (h & 65535u) / 65535f;
	}

	private Dictionary<Vector2I, bool> BuildDirtMask(Rect2I range)
	{
		// Keep a larger simulation border so feature/cap logic can sample stable neighbors.
		int padding = Mathf.Max(6, SmoothPasses + 4);
		int minX = range.Position.X - padding;
		int maxX = range.End.X + padding - 1;
		int minY = range.Position.Y - padding;
		int maxY = range.End.Y + padding - 1;
		Rect2I simRange = new(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);

		EnsureBaseMaskCacheValid();
		Dictionary<Vector2I, bool> mask = BuildBaseMaskFromChunkCache(simRange);

		if (EnableFeaturePass)
			ApplyFeaturePass(mask, simRange);

		ResolveAlternatingEdgeConflicts(mask);
		return CleanupMask(mask);
	}

	private void EnsureBaseMaskCacheValid()
	{
		int signature = ComputeBaseMaskSignature();
		if (signature == _baseMaskCacheSignature)
			return;

		_baseMaskCacheSignature = signature;
		_baseMaskChunkCache.Clear();
	}

	private int ComputeBaseMaskSignature()
	{
		unchecked
		{
			int h = 17;
			h = (h * 31) + _runtimeTerrainSeed;
			h = (h * 31) + ContinentScaleTiles;
			h = (h * 31) + SmoothPasses;
			h = (h * 31) + Mathf.RoundToInt(DirtThreshold * 1000f);
			h = (h * 31) + Mathf.RoundToInt(DetailWeight * 1000f);
			h = (h * 31) + Mathf.RoundToInt(DomainWarpStrength * 1000f);
			return h;
		}
	}

	private Dictionary<Vector2I, bool> BuildBaseMaskFromChunkCache(Rect2I simRange)
	{
		var mask = new Dictionary<Vector2I, bool>(simRange.Size.X * simRange.Size.Y);
		for (int y = simRange.Position.Y; y < simRange.End.Y; y++)
		{
			for (int x = simRange.Position.X; x < simRange.End.X; x++)
			{
				Vector2I tile = new(x, y);
				mask[tile] = GetBaseMaskValue(tile);
			}
		}

		return mask;
	}

	private bool GetBaseMaskValue(Vector2I tile)
	{
		Vector2I chunk = new(FloorDiv(tile.X, BaseMaskChunkSize), FloorDiv(tile.Y, BaseMaskChunkSize));
		bool[] chunkData = GetOrBuildBaseMaskChunk(chunk);
		int baseX = chunk.X * BaseMaskChunkSize;
		int baseY = chunk.Y * BaseMaskChunkSize;
		int lx = tile.X - baseX;
		int ly = tile.Y - baseY;
		int index = (ly * BaseMaskChunkSize) + lx;
		return chunkData[index];
	}

	private bool IsDirtAtTile(Vector2I tile)
	{
		EnsureBaseMaskCacheValid();
		// Obstacle placement uses a stable macro-terrain query.
		// This intentionally ignores temporary/local feature overlays.
		return GetBaseMaskValue(tile);
	}

	private bool[] GetOrBuildBaseMaskChunk(Vector2I chunk)
	{
		if (_baseMaskChunkCache.TryGetValue(chunk, out bool[] cached))
			return cached;

		int passes = Mathf.Clamp(SmoothPasses, 0, 4);
		int padding = Mathf.Max(4, passes + 2);
		int coreMinX = chunk.X * BaseMaskChunkSize;
		int coreMinY = chunk.Y * BaseMaskChunkSize;
		int coreMaxX = coreMinX + BaseMaskChunkSize - 1;
		int coreMaxY = coreMinY + BaseMaskChunkSize - 1;

		int simMinX = coreMinX - padding;
		int simMaxX = coreMaxX + padding;
		int simMinY = coreMinY - padding;
		int simMaxY = coreMaxY + padding;

		var simMask = new Dictionary<Vector2I, bool>();
		for (int y = simMinY; y <= simMaxY; y++)
		{
			for (int x = simMinX; x <= simMaxX; x++)
			{
				Vector2I key = new(x, y);
				float n = SampleContinentNoise(key);
				simMask[key] = n >= DirtThreshold;
			}
		}

		for (int i = 0; i < passes; i++)
			simMask = SmoothMask(simMask);
		simMask = CleanupMask(simMask);

		bool[] data = new bool[BaseMaskChunkSize * BaseMaskChunkSize];
		for (int y = coreMinY; y <= coreMaxY; y++)
		{
			for (int x = coreMinX; x <= coreMaxX; x++)
			{
				int lx = x - coreMinX;
				int ly = y - coreMinY;
				data[(ly * BaseMaskChunkSize) + lx] = IsDirt(simMask, new Vector2I(x, y));
			}
		}

		_baseMaskChunkCache[chunk] = data;
		return data;
	}

	private static Dictionary<Vector2I, bool> SmoothMask(Dictionary<Vector2I, bool> source)
	{
		var next = new Dictionary<Vector2I, bool>(source.Count);
		foreach (var kv in source)
		{
			Vector2I key = kv.Key;
			bool isDirt = kv.Value;
			int cardinal = CountCardinalDirt(source, key);
			int diagonal = CountDiagonalDirt(source, key);

			if (isDirt)
			{
				// Keep connected masses, remove isolated noise.
				bool keep = cardinal >= 2 || (cardinal == 1 && diagonal >= 3);
				next[key] = keep;
			}
			else
			{
				// Fill tiny holes inside larger masses.
				bool fill = cardinal >= 3 || (cardinal == 2 && diagonal >= 4);
				next[key] = fill;
			}
		}

		return next;
	}

	private static Dictionary<Vector2I, bool> CleanupMask(Dictionary<Vector2I, bool> source)
	{
		var next = new Dictionary<Vector2I, bool>(source.Count);
		foreach (var kv in source)
		{
			Vector2I key = kv.Key;
			bool isDirt = kv.Value;
			int cardinal = CountCardinalDirt(source, key);

			// Remove 1-cell spikes and fill 1-cell holes.
			if (isDirt && cardinal <= 1)
				next[key] = false;
			else if (!isDirt && cardinal >= 4)
				next[key] = true;
			else
				next[key] = isDirt;
		}

		return next;
	}

	private void ResolveAlternatingEdgeConflicts(Dictionary<Vector2I, bool> mask)
	{
		// Fix "E:e next to E:w" style alternating necks before tile selection.
		// This removes ambiguous one-tile zigzags that produce edge-choice conflicts.
		var patchToDirt = new HashSet<Vector2I>();

		foreach (var kv in mask)
		{
			if (!kv.Value)
				continue;

			Vector2I a = kv.Key;
			Vector2I b = a + Vector2I.Down;
			if (IsAlternatingVerticalPair(mask, a, b))
			{
				if (PickConflictSide(a, 9203))
					patchToDirt.Add(a + Vector2I.Right);
				else
					patchToDirt.Add(b + Vector2I.Left);
			}

			Vector2I c = a + Vector2I.Right;
			if (IsAlternatingHorizontalPair(mask, a, c))
			{
				if (PickConflictSide(a, 9221))
					patchToDirt.Add(a + Vector2I.Down);
				else
					patchToDirt.Add(c + Vector2I.Up);
			}
		}

		foreach (Vector2I key in patchToDirt)
			mask[key] = true;
	}

	private static bool IsAlternatingVerticalPair(Dictionary<Vector2I, bool> mask, Vector2I a, Vector2I b)
	{
		if (!IsDirt(mask, a) || !IsDirt(mask, b))
			return false;

		bool aLooksEdgeE = !IsDirt(mask, a + Vector2I.Right) && IsDirt(mask, a + Vector2I.Left);
		bool bLooksEdgeW = !IsDirt(mask, b + Vector2I.Left) && IsDirt(mask, b + Vector2I.Right);
		bool aSupported = IsDirt(mask, a + Vector2I.Up);
		bool bSupported = IsDirt(mask, b + Vector2I.Down);
		if (aLooksEdgeE && bLooksEdgeW && aSupported && bSupported)
			return true;

		bool aLooksEdgeW = !IsDirt(mask, a + Vector2I.Left) && IsDirt(mask, a + Vector2I.Right);
		bool bLooksEdgeE = !IsDirt(mask, b + Vector2I.Right) && IsDirt(mask, b + Vector2I.Left);
		return aLooksEdgeW && bLooksEdgeE && aSupported && bSupported;
	}

	private static bool IsAlternatingHorizontalPair(Dictionary<Vector2I, bool> mask, Vector2I a, Vector2I c)
	{
		if (!IsDirt(mask, a) || !IsDirt(mask, c))
			return false;

		bool aLooksEdgeS = !IsDirt(mask, a + Vector2I.Down) && IsDirt(mask, a + Vector2I.Up);
		bool cLooksEdgeN = !IsDirt(mask, c + Vector2I.Up) && IsDirt(mask, c + Vector2I.Down);
		bool aSupported = IsDirt(mask, a + Vector2I.Left);
		bool cSupported = IsDirt(mask, c + Vector2I.Right);
		if (aLooksEdgeS && cLooksEdgeN && aSupported && cSupported)
			return true;

		bool aLooksEdgeN = !IsDirt(mask, a + Vector2I.Up) && IsDirt(mask, a + Vector2I.Down);
		bool cLooksEdgeS = !IsDirt(mask, c + Vector2I.Down) && IsDirt(mask, c + Vector2I.Up);
		return aLooksEdgeN && cLooksEdgeS && aSupported && cSupported;
	}

	private bool PickConflictSide(Vector2I key, int salt)
	{
		uint h = HashSeeded(key, salt);
		return (h & 1u) == 0u;
	}

	private static bool IsDirt(Dictionary<Vector2I, bool> mask, Vector2I key)
	{
		return mask.TryGetValue(key, out bool v) && v;
	}

	private static int CountCardinalDirt(Dictionary<Vector2I, bool> mask, Vector2I key)
	{
		int count = 0;
		if (IsDirt(mask, key + Vector2I.Up)) count++;
		if (IsDirt(mask, key + Vector2I.Right)) count++;
		if (IsDirt(mask, key + Vector2I.Down)) count++;
		if (IsDirt(mask, key + Vector2I.Left)) count++;
		return count;
	}

	private static int CountDiagonalDirt(Dictionary<Vector2I, bool> mask, Vector2I key)
	{
		int count = 0;
		if (IsDirt(mask, key + Vector2I.Up + Vector2I.Right)) count++;
		if (IsDirt(mask, key + Vector2I.Right + Vector2I.Down)) count++;
		if (IsDirt(mask, key + Vector2I.Down + Vector2I.Left)) count++;
		if (IsDirt(mask, key + Vector2I.Left + Vector2I.Up)) count++;
		return count;
	}

	private float SampleContinentNoise(Vector2I tile)
	{
		float scale = Mathf.Max(4f, ContinentScaleTiles);
		Vector2 p = new(tile.X, tile.Y);

		float warpScale = scale * 0.85f;
		float warpX = (ValueNoise((p / warpScale) + _seedOffsetWarpA, 1403) * 2f) - 1f;
		float warpY = (ValueNoise((p / warpScale) + _seedOffsetWarpB, 1427) * 2f) - 1f;
		float warpAmount = scale * Mathf.Clamp(DomainWarpStrength, 0f, 1f);
		Vector2 warped = p + (new Vector2(warpX, warpY) * warpAmount);

		float n0 = ValueNoise((warped / scale) + _seedOffsetBaseA, 811);
		float n1 = ValueNoise((warped / (scale * 0.54f)) + _seedOffsetBaseB, 1201);
		float ridge = ValueNoise((warped / (scale * 0.32f)) + _seedOffsetRidge, 1807);

		float baseBlend = Mathf.Lerp(n0, n1, Mathf.Clamp(DetailWeight, 0f, 1f));
		float ridgeWeight = Mathf.Clamp((DetailWeight * 0.5f) + 0.1f, 0f, 0.35f);
		return Mathf.Lerp(baseBlend, ridge, ridgeWeight);
	}

	private float ValueNoise(Vector2 p, int salt)
	{
		int x0 = Mathf.FloorToInt(p.X);
		int y0 = Mathf.FloorToInt(p.Y);
		int x1 = x0 + 1;
		int y1 = y0 + 1;
		float tx = p.X - x0;
		float ty = p.Y - y0;
		float sx = tx * tx * (3f - (2f * tx));
		float sy = ty * ty * (3f - (2f * ty));

		float v00 = Hash01(x0, y0, salt);
		float v10 = Hash01(x1, y0, salt);
		float v01 = Hash01(x0, y1, salt);
		float v11 = Hash01(x1, y1, salt);

		float ix0 = Mathf.Lerp(v00, v10, sx);
		float ix1 = Mathf.Lerp(v01, v11, sx);
		return Mathf.Lerp(ix0, ix1, sy);
	}

	private float Hash01(int x, int y, int salt)
	{
		uint h = HashSeeded(new Vector2I(x, y), salt);
		return (h & 65535u) / 65535f;
	}

	private uint HashSeeded(Vector2I key, int salt)
	{
		unchecked
		{
			int sm = _runtimeTerrainSeed;
			Vector2I seeded = new(key.X + (sm * 3), key.Y - (sm * 5));
			return Hash(seeded, salt ^ (sm * 83491));
		}
	}

	private static uint Hash(Vector2I key, int salt)
	{
		unchecked
		{
			uint h = (uint)(key.X * 73856093) ^ (uint)(key.Y * 19349663) ^ (uint)(salt * 83492791);
			h ^= h >> 13;
			h *= 1274126177u;
			h ^= h >> 16;
			return h;
		}
	}
}
