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

}
