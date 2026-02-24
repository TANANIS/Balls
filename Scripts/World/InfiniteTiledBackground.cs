using Godot;
using System.Collections.Generic;

/*
 * InfiniteTiledBackground:
 * - Repeats one base tile texture infinitely around the camera.
 * - Optionally adds deterministic random overlay tiles on top.
 * - Keeps a small active tile cache instead of rebuilding each frame.
 */
public partial class InfiniteTiledBackground : Node2D
{
	[Export] public Texture2D BaseTexture;
	[Export] public Godot.Collections.Array<Texture2D> OverlayTextures = new();
	[Export] public Vector2 TileScale = Vector2.One;
	[Export] public int ExtraTileMargin = 1;
	[Export(PropertyHint.Range, "0,1,0.01")] public float BaseFlipXChance = 0.0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float OverlaySpawnChance = 0.85f;
	[Export(PropertyHint.Range, "1,4,1")] public int OverlayPerTileMin = 1;
	[Export(PropertyHint.Range, "1,4,1")] public int OverlayPerTileMax = 2;
	[Export(PropertyHint.Range, "0,1,0.01")] public float OverlayFlipXChance = 0.35f;
	[Export] public bool AllowOverlayDuplicates = false;

	private sealed class TileEntry
	{
		public Node2D Root;
		public uint Signature;
	}

	private readonly Dictionary<Vector2I, TileEntry> _activeTiles = new();
	private Rect2I _lastRange;
	private Vector2 _lastTileSize = Vector2.Zero;
	private int _lastStaticConfigHash = 0;
	private bool _rangeInitialized = false;

	public override void _Ready()
	{
		ZIndex = -20;
		RebuildTiles();
		GetViewport().SizeChanged += RebuildTiles;
	}

	public override void _Process(double delta)
	{
		RebuildTiles();
	}

	private void RebuildTiles()
	{
		if (BaseTexture == null)
		{
			ClearAllTiles();
			_rangeInitialized = false;
			return;
		}

		Vector2 rawSize = BaseTexture.GetSize();
		float tileW = Mathf.Max(1f, rawSize.X * Mathf.Abs(TileScale.X));
		float tileH = Mathf.Max(1f, rawSize.Y * Mathf.Abs(TileScale.Y));

		Vector2 center = GetCameraCenter();
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		float halfW = viewport.X * 0.5f * zoom.X;
		float halfH = viewport.Y * 0.5f * zoom.Y;

		int margin = Mathf.Max(0, ExtraTileMargin);
		int minX = Mathf.FloorToInt((center.X - halfW) / tileW) - margin;
		int maxX = Mathf.FloorToInt((center.X + halfW) / tileW) + margin;
		int minY = Mathf.FloorToInt((center.Y - halfH) / tileH) - margin;
		int maxY = Mathf.FloorToInt((center.Y + halfH) / tileH) + margin;
		Rect2I range = new Rect2I(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
		Vector2 tileSize = new Vector2(tileW, tileH);
		int staticConfig = GetStaticConfigHash();
		if (_rangeInitialized &&
			range == _lastRange &&
			tileSize.IsEqualApprox(_lastTileSize) &&
			staticConfig == _lastStaticConfigHash)
			return;

		_lastRange = range;
		_lastTileSize = tileSize;
		_lastStaticConfigHash = staticConfig;
		_rangeInitialized = true;

		var needed = new HashSet<Vector2I>();
		for (int y = range.Position.Y; y < range.End.Y; y++)
		{
			for (int x = range.Position.X; x < range.End.X; x++)
			{
				Vector2I key = new Vector2I(x, y);
				needed.Add(key);
				EnsureTile(key, tileW, tileH);
			}
		}

		var removeKeys = new List<Vector2I>();
		foreach (Vector2I key in _activeTiles.Keys)
		{
			if (!needed.Contains(key))
				removeKeys.Add(key);
		}

		foreach (Vector2I key in removeKeys)
		{
			if (_activeTiles.TryGetValue(key, out TileEntry entry) && IsInstanceValid(entry.Root))
				entry.Root.QueueFree();
			_activeTiles.Remove(key);
		}
	}

	private void EnsureTile(Vector2I key, float tileW, float tileH)
	{
		uint signature = BuildTileSignature(key);
		if (_activeTiles.TryGetValue(key, out TileEntry current))
		{
			if (current.Signature == signature && IsInstanceValid(current.Root))
			{
				PlaceTileRoot(current.Root, key, tileW, tileH);
				return;
			}

			if (IsInstanceValid(current.Root))
				current.Root.QueueFree();
			_activeTiles.Remove(key);
		}

		var root = new Node2D { Name = $"Tile_{key.X}_{key.Y}" };
		AddChild(root);
		PlaceTileRoot(root, key, tileW, tileH);

		bool baseFlip = RollChance(Hash(key, 11), BaseFlipXChance);
		Sprite2D baseSprite = CreateLayerSprite(BaseTexture, baseFlip, tileW);
		baseSprite.ZIndex = 0;
		root.AddChild(baseSprite);

		if (ShouldSpawnOverlays(key))
		{
			int overlayCount = ResolveOverlayCount(key);
			var used = new HashSet<int>();
			for (int i = 0; i < overlayCount; i++)
			{
				int overlayIndex = ResolveOverlayIndex(key, i, used);
				if (overlayIndex < 0 || overlayIndex >= OverlayTextures.Count)
					continue;

				Texture2D overlay = OverlayTextures[overlayIndex];
				if (overlay == null)
					continue;

				bool overlayFlip = RollChance(Hash(key, 101 + (i * 19)), OverlayFlipXChance);
				Sprite2D overlaySprite = CreateLayerSprite(overlay, overlayFlip, tileW);
				overlaySprite.ZIndex = 1 + i;
				root.AddChild(overlaySprite);
			}
		}

		_activeTiles[key] = new TileEntry
		{
			Root = root,
			Signature = signature
		};
	}

	private Sprite2D CreateLayerSprite(Texture2D texture, bool flipX, float tileW)
	{
		float sxAbs = Mathf.Abs(TileScale.X);
		float sy = TileScale.Y;
		float sx = sxAbs * (flipX ? -1f : 1f);
		if (Mathf.IsZeroApprox(sx))
			sx = flipX ? -1f : 1f;
		if (Mathf.IsZeroApprox(sy))
			sy = 1f;

		var sprite = new Sprite2D
		{
			Texture = texture,
			Centered = false,
			Scale = new Vector2(sx, sy),
			Position = sx < 0f ? new Vector2(tileW, 0f) : Vector2.Zero
		};
		return sprite;
	}

	private void PlaceTileRoot(Node2D root, Vector2I key, float tileW, float tileH)
	{
		root.Position = new Vector2(key.X * tileW, key.Y * tileH);
	}

	private bool ShouldSpawnOverlays(Vector2I key)
	{
		if (OverlayTextures == null || OverlayTextures.Count == 0)
			return false;
		return RollChance(Hash(key, 23), OverlaySpawnChance);
	}

	private int ResolveOverlayCount(Vector2I key)
	{
		int min = Mathf.Max(1, OverlayPerTileMin);
		int max = Mathf.Max(min, OverlayPerTileMax);
		if (!AllowOverlayDuplicates)
			max = Mathf.Min(max, OverlayTextures.Count);
		min = Mathf.Min(min, max);

		if (max <= min)
			return min;

		int span = max - min + 1;
		return min + (int)(Hash(key, 29) % (uint)span);
	}

	private int ResolveOverlayIndex(Vector2I key, int slot, HashSet<int> used)
	{
		if (OverlayTextures == null || OverlayTextures.Count == 0)
			return -1;

		int seedIndex = (int)(Hash(key, 41 + (slot * 17)) % (uint)OverlayTextures.Count);
		if (AllowOverlayDuplicates || used.Add(seedIndex))
			return seedIndex;

		for (int i = 1; i < OverlayTextures.Count; i++)
		{
			int candidate = (seedIndex + i) % OverlayTextures.Count;
			if (used.Add(candidate))
				return candidate;
		}

		return -1;
	}

	private uint BuildTileSignature(Vector2I key)
	{
		uint signature = 2166136261u;
		signature = Mix(signature, (uint)GetStaticConfigHash());
		signature = Mix(signature, (uint)key.X);
		signature = Mix(signature, (uint)key.Y);
		signature = Mix(signature, RollChance(Hash(key, 11), BaseFlipXChance) ? 1u : 0u);

		if (!ShouldSpawnOverlays(key))
			return Mix(signature, 0u);

		int count = ResolveOverlayCount(key);
		signature = Mix(signature, (uint)count);
		var used = new HashSet<int>();
		for (int i = 0; i < count; i++)
		{
			int idx = ResolveOverlayIndex(key, i, used);
			bool flip = RollChance(Hash(key, 101 + (i * 19)), OverlayFlipXChance);
			signature = Mix(signature, (uint)(idx + 1));
			signature = Mix(signature, flip ? 1u : 0u);
		}

		return signature;
	}

	private int GetStaticConfigHash()
	{
		int h = 17;
		h = (h * 31) + (BaseTexture?.ResourcePath?.GetHashCode() ?? 0);
		int overlayCount = OverlayTextures?.Count ?? 0;
		h = (h * 31) + overlayCount;
		for (int i = 0; i < overlayCount; i++)
			h = (h * 31) + (OverlayTextures[i]?.ResourcePath?.GetHashCode() ?? 0);
		h = (h * 31) + TileScale.GetHashCode();
		h = (h * 31) + ExtraTileMargin;
		h = (h * 31) + BaseFlipXChance.GetHashCode();
		h = (h * 31) + OverlaySpawnChance.GetHashCode();
		h = (h * 31) + OverlayPerTileMin;
		h = (h * 31) + OverlayPerTileMax;
		h = (h * 31) + OverlayFlipXChance.GetHashCode();
		h = (h * 31) + (AllowOverlayDuplicates ? 1 : 0);
		return h;
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

	private static bool RollChance(uint hash, float chance)
	{
		float p = Mathf.Clamp(chance, 0f, 1f);
		float v = (hash & 65535u) / 65535f;
		return v < p;
	}

	private static uint Mix(uint current, uint value)
	{
		current ^= value + 0x9e3779b9u + (current << 6) + (current >> 2);
		return current;
	}

	private void ClearAllTiles()
	{
		foreach (TileEntry entry in _activeTiles.Values)
		{
			if (IsInstanceValid(entry.Root))
				entry.Root.QueueFree();
		}
		_activeTiles.Clear();
	}

	private Vector2 GetCameraCenter()
	{
		Camera2D camera = GetViewport().GetCamera2D();
		if (camera != null)
			return camera.GetScreenCenterPosition();
		Rect2 rect = GetViewport().GetVisibleRect();
		return rect.Position + (rect.Size * 0.5f);
	}
}
