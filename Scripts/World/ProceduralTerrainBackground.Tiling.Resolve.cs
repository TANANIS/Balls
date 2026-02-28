using Godot;
using System.Collections.Generic;

public partial class ProceduralTerrainBackground
{
	private Texture2D ResolveTileTexture(TileTopology t)
	{
		if (t.OpenCount == 0)
			return _dirtFill;

		if (t.OpenCount == 1)
		{
			if (t.OpenN) return _edgeN ?? _dirtFill;
			if (t.OpenE) return _edgeE ?? _dirtFill;
			if (t.OpenS) return _edgeS ?? _dirtFill;
			return _edgeW ?? _dirtFill;
		}

		if (t.OpenCount == 2)
		{
			// Adjacent openings -> diagonal coast tile.
			if (t.OpenN && t.OpenE) return _diagMudSwGrassNe ?? _dirtFill;
			if (t.OpenE && t.OpenS) return _diagMudNwGrassSe ?? _dirtFill;
			if (t.OpenS && t.OpenW) return _diagMudNeGrassSw ?? _dirtFill;
			if (t.OpenW && t.OpenN) return _diagMudSeGrassNw ?? _dirtFill;

			// Opposite openings -> strip tile (avoid wrong full-dirt fallback between edge chains).
			// Open N+S means dirt continuity is horizontal, so use horizontal strip.
			if (t.OpenN && t.OpenS) return _stripMudHMid ?? _dirtFill;
			// Open E+W means dirt continuity is vertical, so use vertical strip.
			if (t.OpenE && t.OpenW) return _stripMudVMid ?? _dirtFill;
			return _dirtFill;
		}

		// 3 openings: keep coastline continuity by using the single-closed-side edge.
		if (t.OpenCount == 3)
		{
			// Strip termini: prevent edge/strip conflicts like E:w beside S:hmid.
			if (t.OpenN && t.OpenS && t.OpenW) return _stripMudHCapW ?? _edgeW ?? _dirtFill;
			if (t.OpenN && t.OpenS && t.OpenE) return _stripMudHCapE ?? _edgeE ?? _dirtFill;
			if (t.OpenE && t.OpenW && t.OpenN) return _stripMudVCapN ?? _edgeN ?? _dirtFill;
			if (t.OpenE && t.OpenW && t.OpenS) return _stripMudVCapS ?? _edgeS ?? _dirtFill;

			if (!t.OpenN) return _edgeS ?? _dirtFill;
			if (!t.OpenE) return _edgeW ?? _dirtFill;
			if (!t.OpenS) return _edgeN ?? _dirtFill;
			return _edgeE ?? _dirtFill;
		}

		// 4 openings: isolated dirt pixel fallback.
		return _dirtFill;
	}

	private static void RemoveUnused(Dictionary<Vector2I, Sprite2D> map, HashSet<Vector2I> needed)
	{
		var remove = new List<Vector2I>();
		foreach (Vector2I key in map.Keys)
		{
			if (!needed.Contains(key))
				remove.Add(key);
		}

		foreach (Vector2I key in remove)
		{
			if (map.TryGetValue(key, out Sprite2D sprite) && GodotObject.IsInstanceValid(sprite))
				sprite.QueueFree();
			map.Remove(key);
		}
	}

	private static MaskView BuildMaskView(Rect2I visibleRange, Dictionary<Vector2I, bool> mask)
	{
		// Need 2-tile border for cap-side topology checks around visible cells.
		Rect2I bounds = new(
			visibleRange.Position.X - 2,
			visibleRange.Position.Y - 2,
			visibleRange.Size.X + 4,
			visibleRange.Size.Y + 4);
		int stride = bounds.Size.X;
		bool[] data = new bool[stride * bounds.Size.Y];
		for (int y = bounds.Position.Y; y < bounds.End.Y; y++)
		{
			for (int x = bounds.Position.X; x < bounds.End.X; x++)
			{
				Vector2I key = new(x, y);
				if (!IsDirt(mask, key))
					continue;
				int lx = x - bounds.Position.X;
				int ly = y - bounds.Position.Y;
				data[(ly * stride) + lx] = true;
			}
		}

		return new MaskView(bounds, data);
	}
}
