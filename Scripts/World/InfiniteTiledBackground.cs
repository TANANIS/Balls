using Godot;
using System.Collections.Generic;

/*
 * InfiniteTiledBackground:
 * - Repeats one tile texture infinitely around the camera.
 * - Uses deterministic per-tile horizontal flip for variation.
 * - Keeps a small active tile cache instead of rebuilding each frame.
 */
public partial class InfiniteTiledBackground : Node2D
{
	[Export] public Texture2D TileTexture;
	[Export] public Vector2 TileScale = Vector2.One;
	[Export] public int ExtraTileMargin = 1;
	[Export(PropertyHint.Range, "0,1,0.01")] public float FlipXChance = 0.5f;

	private readonly Dictionary<Vector2I, Sprite2D> _activeTiles = new();
	private Rect2I _lastRange;
	private Vector2 _lastTileSize = Vector2.Zero;
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
		if (TileTexture == null)
			return;

		Vector2 rawSize = TileTexture.GetSize();
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
		if (_rangeInitialized && range == _lastRange && tileSize.IsEqualApprox(_lastTileSize))
			return;
		_lastRange = range;
		_lastTileSize = tileSize;
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
			if (_activeTiles.TryGetValue(key, out Sprite2D sprite))
				sprite.QueueFree();
			_activeTiles.Remove(key);
		}
	}

	private void EnsureTile(Vector2I key, float tileW, float tileH)
	{
		if (!_activeTiles.TryGetValue(key, out Sprite2D sprite))
		{
			sprite = new Sprite2D
			{
				Texture = TileTexture,
				Centered = false
			};
			AddChild(sprite);
			_activeTiles[key] = sprite;
		}

		bool flip = ShouldFlipX(key);
		float sx = Mathf.Abs(TileScale.X) * (flip ? -1f : 1f);
		float sy = TileScale.Y;
		if (Mathf.IsZeroApprox(sx))
			sx = flip ? -1f : 1f;
		if (Mathf.IsZeroApprox(sy))
			sy = 1f;
		sprite.Scale = new Vector2(sx, sy);
		float px = key.X * tileW;
		float py = key.Y * tileH;
		if (sx < 0f)
			px += tileW;
		sprite.Position = new Vector2(px, py);
	}

	private bool ShouldFlipX(Vector2I key)
	{
		uint h = (uint)(key.X * 73856093) ^ (uint)(key.Y * 19349663);
		float v = (h & 1023u) / 1023f;
		return v < Mathf.Clamp(FlipXChance, 0f, 1f);
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
