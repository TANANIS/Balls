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

}
