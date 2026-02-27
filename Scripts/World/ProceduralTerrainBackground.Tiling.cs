using Godot;
using System;
using System.Collections.Generic;

public partial class ProceduralTerrainBackground
{
	private readonly struct MaskView
	{
		public readonly Rect2I Bounds;
		private readonly bool[] _data;
		private readonly int _stride;

		public MaskView(Rect2I bounds, bool[] data)
		{
			Bounds = bounds;
			_data = data;
			_stride = bounds.Size.X;
		}

		public bool IsDirt(Vector2I key)
		{
			if (!Bounds.HasPoint(key))
				return false;
			int x = key.X - Bounds.Position.X;
			int y = key.Y - Bounds.Position.Y;
			return _data[(y * _stride) + x];
		}
	}

	[Flags]
	private enum CapBits
	{
		None = 0,
		TopLeft = 1 << 0,
		TopRight = 1 << 1,
		BottomLeft = 1 << 2,
		BottomRight = 1 << 3
	}

	private readonly struct TileTopology
	{
		public readonly bool N;
		public readonly bool E;
		public readonly bool S;
		public readonly bool W;
		public readonly bool NE;
		public readonly bool NW;
		public readonly bool SE;
		public readonly bool SW;
		public readonly bool OpenN;
		public readonly bool OpenE;
		public readonly bool OpenS;
		public readonly bool OpenW;
		public readonly int OpenCount;

		public TileTopology(
			bool n, bool e, bool s, bool w,
			bool ne, bool nw, bool se, bool sw)
		{
			N = n;
			E = e;
			S = s;
			W = w;
			NE = ne;
			NW = nw;
			SE = se;
			SW = sw;
			OpenN = !n;
			OpenE = !e;
			OpenS = !s;
			OpenW = !w;
			int c = 0;
			if (OpenN) c++;
			if (OpenE) c++;
			if (OpenS) c++;
			if (OpenW) c++;
			OpenCount = c;
		}
	}

	private void RebuildGrassLayer(Rect2I range, Vector2 tileSize)
	{
		var needed = new HashSet<Vector2I>();
		for (int y = range.Position.Y; y < range.End.Y; y++)
		{
			for (int x = range.Position.X; x < range.End.X; x++)
			{
				Vector2I key = new(x, y);
				needed.Add(key);
				if (_grassTiles.TryGetValue(key, out Sprite2D existing) && IsInstanceValid(existing))
				{
					existing.Position = TileToWorld(key, tileSize);
					continue;
				}

				var sprite = new Sprite2D
				{
					Texture = _grassFill,
					Centered = false,
					Scale = TileScale,
					Position = TileToWorld(key, tileSize),
					ZIndex = 0,
					ZAsRelative = true
				};
				_generatedGrassLayer.AddChild(sprite);
				_grassTiles[key] = sprite;
			}
		}

		RemoveUnused(_grassTiles, needed);
	}

	private void RebuildDirtLayer(Rect2I range, Vector2 tileSize, MaskView mask)
	{
		var needed = new HashSet<Vector2I>();
		for (int y = range.Position.Y; y < range.End.Y; y++)
		{
			for (int x = range.Position.X; x < range.End.X; x++)
			{
				Vector2I key = new(x, y);
				if (!mask.IsDirt(key))
					continue;

				TileTopology t = BuildTopology(mask, key);
				Texture2D texture = ResolveTileTexture(t);
				needed.Add(key);
				if (_dirtTiles.TryGetValue(key, out Sprite2D existing) && IsInstanceValid(existing))
				{
					existing.Texture = texture;
					existing.Position = TileToWorld(key, tileSize);
					continue;
				}

				var sprite = new Sprite2D
				{
					Texture = texture,
					Centered = false,
					Scale = TileScale,
					Position = TileToWorld(key, tileSize),
					ZIndex = 1,
					ZAsRelative = true
				};
				_generatedDirtLayer.AddChild(sprite);
				_dirtTiles[key] = sprite;
			}
		}

		RemoveUnused(_dirtTiles, needed);
	}

	private void RebuildCapLayer(Rect2I range, Vector2 tileSize, MaskView mask)
	{
		bool hasCapTextures =
			_capGrassNw != null ||
			_capGrassNe != null ||
			_capGrassSw != null ||
			_capGrassSe != null ||
			_capGrassNwSe != null ||
			_capGrassNeSw != null;
		if (!hasCapTextures)
		{
			RemoveUnused(_capTiles, new HashSet<Vector2I>());
			return;
		}

		var requestedCaps = new Dictionary<Vector2I, CapBits>();
		for (int y = range.Position.Y; y < range.End.Y; y++)
		{
			for (int x = range.Position.X; x < range.End.X; x++)
			{
				Vector2I source = new(x, y);
				if (!mask.IsDirt(source))
					continue;

				TileTopology t = BuildTopology(mask, source);
				ApplyDiagDrivenCap(mask, source, t, requestedCaps);
				ApplyFillCornerCap(source, t, requestedCaps);
			}
		}

		var needed = new HashSet<Vector2I>();
		foreach (var kv in requestedCaps)
		{
			Texture2D texture = ResolveCapTexture(kv.Value);
			if (texture == null)
				continue;

			Vector2I anchor = kv.Key;
			needed.Add(anchor);
			if (_capTiles.TryGetValue(anchor, out Sprite2D existing) && IsInstanceValid(existing))
			{
				existing.Texture = texture;
				existing.Position = TileToWorld(anchor, tileSize);
				continue;
			}

			var sprite = new Sprite2D
			{
				Texture = texture,
				Centered = false,
				Scale = TileScale,
				Position = TileToWorld(anchor, tileSize),
				ZIndex = 2,
				ZAsRelative = true
			};
			_generatedCapLayer.AddChild(sprite);
			_capTiles[anchor] = sprite;
		}

		RemoveUnused(_capTiles, needed);
	}

	private static TileTopology BuildTopology(MaskView mask, Vector2I key)
	{
		bool n = mask.IsDirt(key + Vector2I.Up);
		bool e = mask.IsDirt(key + Vector2I.Right);
		bool s = mask.IsDirt(key + Vector2I.Down);
		bool w = mask.IsDirt(key + Vector2I.Left);
		bool ne = mask.IsDirt(key + Vector2I.Up + Vector2I.Right);
		bool nw = mask.IsDirt(key + Vector2I.Up + Vector2I.Left);
		bool se = mask.IsDirt(key + Vector2I.Down + Vector2I.Right);
		bool sw = mask.IsDirt(key + Vector2I.Down + Vector2I.Left);
		return new TileTopology(n, e, s, w, ne, nw, se, sw);
	}

	private void ApplyDiagDrivenCap(
		MaskView mask,
		Vector2I source,
		TileTopology t,
		Dictionary<Vector2I, CapBits> requestedCaps)
	{
		// CAP is directly coupled to diagonal coastline topology.
		if (t.OpenCount != 2)
			return;

		if (t.OpenN && t.OpenE)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Left, Vector2I.Up + Vector2I.Left, CapBits.TopRight);
			return;
		}

		if (t.OpenN && t.OpenW)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Right, Vector2I.Up + Vector2I.Right, CapBits.TopLeft);
			return;
		}

		if (t.OpenS && t.OpenE)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Left, Vector2I.Down + Vector2I.Left, CapBits.BottomRight);
			return;
		}

		if (t.OpenS && t.OpenW)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Right, Vector2I.Down + Vector2I.Right, CapBits.BottomLeft);
		}
	}

	private static void ApplyFillCornerCap(Vector2I source, TileTopology t, Dictionary<Vector2I, CapBits> requestedCaps)
	{
		// Fill tile inner-corner cap: if one diagonal is grass while both adjacent cardinals are dirt,
		// add a small grass nib on the fill tile. This fixes missing caps between edge/diag joins.
		if (t.OpenCount != 0)
			return;

		if (!t.NE && t.N && t.E)
			AddCapRequest(requestedCaps, source, CapBits.TopRight);
		if (!t.NW && t.N && t.W)
			AddCapRequest(requestedCaps, source, CapBits.TopLeft);
		if (!t.SW && t.S && t.W)
			AddCapRequest(requestedCaps, source, CapBits.BottomLeft);
		if (!t.SE && t.S && t.E)
			AddCapRequest(requestedCaps, source, CapBits.BottomRight);
	}

	private static void RequestDiagCap(
		MaskView mask,
		Vector2I source,
		Dictionary<Vector2I, CapBits> requestedCaps,
		Vector2I preferredSideOffset,
		Vector2I endpointDiagonalOffset,
		CapBits capBit)
	{
		Vector2I sideAnchor = source + preferredSideOffset;
		bool sideIsDirt = mask.IsDirt(sideAnchor);
		bool sideCanReceive = false;
		if (sideIsDirt)
		{
			TileTopology sideTopo = BuildTopology(mask, sideAnchor);
			// Prevent caps from spilling onto edge/diag tiles (e.g. repeated cap on E:s chains).
			sideCanReceive = sideTopo.OpenCount == 0;
		}

		if (sideCanReceive)
			AddCapRequest(requestedCaps, sideAnchor, capBit);

		bool endpointDiagonalIsDirt = mask.IsDirt(source + endpointDiagonalOffset);
		// Only fallback to source when there is no side dirt tile.
		// If side is dirt but not cap-eligible (edge/diag), skipping fallback avoids noisy caps beside E:n/E:s chains.
		if (!sideIsDirt && !endpointDiagonalIsDirt)
			AddCapRequest(requestedCaps, source, capBit);
	}

	private static void AddCapRequest(Dictionary<Vector2I, CapBits> requestedCaps, Vector2I anchor, CapBits capBit)
	{
		if (requestedCaps.TryGetValue(anchor, out CapBits existing))
			requestedCaps[anchor] = existing | capBit;
		else
			requestedCaps[anchor] = capBit;
	}

	private Texture2D ResolveCapTexture(CapBits bits)
	{
		if ((bits & (CapBits.TopLeft | CapBits.BottomRight)) == (CapBits.TopLeft | CapBits.BottomRight))
			return _capGrassNwSe ?? _capGrassNe ?? _capGrassSe;

		if ((bits & (CapBits.TopRight | CapBits.BottomLeft)) == (CapBits.TopRight | CapBits.BottomLeft))
			return _capGrassNeSw ?? _capGrassNw ?? _capGrassSw;

		if ((bits & CapBits.TopRight) != 0)
			return _capGrassNw;
		if ((bits & CapBits.TopLeft) != 0)
			return _capGrassNe;
		if ((bits & CapBits.BottomLeft) != 0)
			return _capGrassSw;
		if ((bits & CapBits.BottomRight) != 0)
			return _capGrassSe;

		return null;
	}

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
