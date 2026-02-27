using Godot;
using System.Collections.Generic;

/*
 * ProceduralTerrainBackground (Refactor):
 * - Phase 1: build a deterministic dirt mask from noise + smoothing.
 * - Phase 2: map neighborhood bitmask -> canonical tile texture.
 * - No cap/deco overlay guesses in this pass; prioritize stable coastline output.
 */
public partial class ProceduralTerrainBackground : Node2D
{
	[Export] public Vector2 TileScale = new(3f, 3f);
	[Export] public int ExtraTileMargin = 2;
	[Export(PropertyHint.Range, "4,96,1")] public int ContinentScaleTiles = 22;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DirtThreshold = 0.53f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DetailWeight = 0.28f;
	[Export] public int TerrainSeed = -1; // < 0 means randomize per run.
	[Export(PropertyHint.Range, "0,1,0.01")] public float DomainWarpStrength = 0.22f;
	[Export(PropertyHint.Range, "0,4,1")] public int SmoothPasses = 2;
	[Export] public bool RebuildOnlyOnTileStep = true;
	[Export(PropertyHint.Range, "0.02,0.5,0.01")] public float RebuildCheckInterval = 0.08f;
	[Export] public bool EnableFeaturePass = true;
	[Export(PropertyHint.Range, "0,1,0.01")] public float IslandChancePerChunk = 0.35f;
	[Export(PropertyHint.Range, "1,32,1")] public int IslandChunkSize = 12;
	[Export(PropertyHint.Range, "1,6,1")] public int IslandRadiusMin = 1;
	[Export(PropertyHint.Range, "1,8,1")] public int IslandRadiusMax = 3;
	[Export] public bool EnableMudPaths = true;
	[Export(PropertyHint.Range, "0,24,1")] public int MudPathCount = 3;
	[Export(PropertyHint.Range, "4,128,1")] public int MudPathLengthMin = 18;
	[Export(PropertyHint.Range, "8,196,1")] public int MudPathLengthMax = 42;
	[Export(PropertyHint.Range, "0,2,1")] public int MudPathHalfWidth = 0;

	private const string CanonicalRoot = "res://Assets/Sprites/World/Terrain/Canonical/";

	private Texture2D _grassFill;
	private Texture2D _dirtFill;
	private Texture2D _edgeN;
	private Texture2D _edgeE;
	private Texture2D _edgeS;
	private Texture2D _edgeW;
	private Texture2D _diagMudNeGrassSw;
	private Texture2D _diagMudNwGrassSe;
	private Texture2D _diagMudSeGrassNw;
	private Texture2D _diagMudSwGrassNe;
	private Texture2D _stripMudVMid;
	private Texture2D _stripMudHMid;
	private Texture2D _stripMudVCapN;
	private Texture2D _stripMudVCapS;
	private Texture2D _stripMudHCapE;
	private Texture2D _stripMudHCapW;
	private Texture2D _capGrassNw;
	private Texture2D _capGrassNe;
	private Texture2D _capGrassSw;
	private Texture2D _capGrassSe;
	private Texture2D _capGrassNwSe;
	private Texture2D _capGrassNeSw;

	private readonly Dictionary<Vector2I, Sprite2D> _grassTiles = new();
	private readonly Dictionary<Vector2I, Sprite2D> _dirtTiles = new();
	private readonly Dictionary<Vector2I, Sprite2D> _capTiles = new();
	private Node2D _generatedRoot;
	private Node2D _generatedGrassLayer;
	private Node2D _generatedDirtLayer;
	private Node2D _generatedCapLayer;
	private int _runtimeTerrainSeed;
	private Vector2 _seedOffsetBaseA;
	private Vector2 _seedOffsetBaseB;
	private Vector2 _seedOffsetWarpA;
	private Vector2 _seedOffsetWarpB;
	private Vector2 _seedOffsetRidge;
	private Rect2I _lastRange;
	private Vector2 _lastTileSize = Vector2.Zero;
	private bool _rangeInitialized = false;
	private float _rebuildCheckTimer = 0f;
	private bool _hasCameraSample = false;
	private Vector2I _lastCameraTile = Vector2I.Zero;
	private Vector2 _lastCameraZoom = Vector2.One;

	public override void _Ready()
	{
		ZIndex = -20;
		InitializeNoiseSeed();
		LoadCanonicalTextures();
		EnsureGeneratedLayers();
		RebuildTiles();
		GetViewport().SizeChanged += OnViewportSizeChanged;
	}

	public override void _Process(double delta)
	{
		float interval = Mathf.Max(0.02f, RebuildCheckInterval);
		_rebuildCheckTimer += (float)delta;
		if (_rebuildCheckTimer < interval)
			return;
		_rebuildCheckTimer = 0f;

		if (!RebuildOnlyOnTileStep)
		{
			RebuildTiles();
			return;
		}

		TryRebuildOnCameraTileStep();
	}

	public void RefreshForNewRun()
	{
		InitializeNoiseSeed();
		_rangeInitialized = false;
		_hasCameraSample = false;
		_rebuildCheckTimer = 0f;
		ClearAll();
		RebuildTiles();
	}

	private void LoadCanonicalTextures()
	{
		_grassFill = LoadTexture("terrain_grass_fill.png");
		_dirtFill = LoadTexture("terrain_dirt_fill.png");
		_edgeN = LoadTexture("terrain_edge_open_n.png");
		_edgeE = LoadTexture("terrain_edge_open_e.png");
		_edgeS = LoadTexture("terrain_edge_open_s.png");
		_edgeW = LoadTexture("terrain_edge_open_w.png");
		_diagMudNeGrassSw = LoadTexture("terrain_diag_mud_ne_grass_sw.png");
		_diagMudNwGrassSe = LoadTexture("terrain_diag_mud_nw_grass_se.png");
		_diagMudSeGrassNw = LoadTexture("terrain_diag_mud_se_grass_nw.png");
		_diagMudSwGrassNe = LoadTexture("terrain_diag_mud_sw_grass_ne.png");
		_stripMudVMid = LoadTexture("terrain_strip_mud_v_mid.png");
		_stripMudHMid = LoadTexture("terrain_strip_mud_h_mid.png");
		_stripMudVCapN = LoadTexture("terrain_strip_mud_v_cap_n.png");
		_stripMudVCapS = LoadTexture("terrain_strip_mud_v_cap_s.png");
		_stripMudHCapE = LoadTexture("terrain_strip_mud_h_cap_e.png");
		_stripMudHCapW = LoadTexture("terrain_strip_mud_h_cap_w.png");
		_capGrassNw = LoadTexture("terrain_cap_grass_nw.png");
		_capGrassNe = LoadTexture("terrain_cap_grass_ne.png");
		_capGrassSw = LoadTexture("terrain_cap_grass_sw.png");
		_capGrassSe = LoadTexture("terrain_cap_grass_se.png");
		_capGrassNwSe = LoadTexture("terrain_cap_grass_nw_se.png");
		_capGrassNeSw = LoadTexture("terrain_cap_grass_ne_sw.png");
	}

	private Texture2D LoadTexture(string fileName)
	{
		return GD.Load<Texture2D>(CanonicalRoot + fileName);
	}

	private void RebuildTiles()
	{
		if (_grassFill == null || _dirtFill == null)
		{
			ClearAll();
			_rangeInitialized = false;
			return;
		}

		EnsureGeneratedLayers();

		Vector2 tileSize = ResolveTileSize(_grassFill);
		Rect2I range = ResolveVisibleRange(tileSize);
		if (_rangeInitialized && range == _lastRange && tileSize.IsEqualApprox(_lastTileSize))
			return;

		_rangeInitialized = true;
		_lastRange = range;
		_lastTileSize = tileSize;

		Dictionary<Vector2I, bool> dirtMask = BuildDirtMask(range);
		MaskView maskView = BuildMaskView(range, dirtMask);
		RebuildGrassLayer(range, tileSize);
		RebuildDirtLayer(range, tileSize, maskView);
		RebuildCapLayer(range, tileSize, maskView);
	}

	private Rect2I ResolveVisibleRange(Vector2 tileSize)
	{
		Vector2 center = GetCameraCenter();
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		float halfW = viewport.X * 0.5f * zoom.X;
		float halfH = viewport.Y * 0.5f * zoom.Y;
		int margin = Mathf.Max(0, ExtraTileMargin);

		int minX = Mathf.FloorToInt((center.X - halfW) / tileSize.X) - margin;
		int maxX = Mathf.FloorToInt((center.X + halfW) / tileSize.X) + margin;
		int minY = Mathf.FloorToInt((center.Y - halfH) / tileSize.Y) - margin;
		int maxY = Mathf.FloorToInt((center.Y + halfH) / tileSize.Y) + margin;
		return new Rect2I(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
	}

	private Vector2 ResolveTileSize(Texture2D texture)
	{
		Vector2 raw = texture.GetSize();
		return new Vector2(
			Mathf.Max(1f, raw.X * Mathf.Abs(TileScale.X)),
			Mathf.Max(1f, raw.Y * Mathf.Abs(TileScale.Y)));
	}

	private Vector2 TileToWorld(Vector2I tile, Vector2 tileSize)
	{
		return new Vector2(tile.X * tileSize.X, tile.Y * tileSize.Y);
	}

	private void ClearAll()
	{
		foreach (var map in new[] { _grassTiles, _dirtTiles, _capTiles })
		{
			foreach (Sprite2D sprite in map.Values)
			{
				if (IsInstanceValid(sprite))
					sprite.QueueFree();
			}
			map.Clear();
		}

		ClearLayerChildren(_generatedGrassLayer);
		ClearLayerChildren(_generatedDirtLayer);
		ClearLayerChildren(_generatedCapLayer);
	}

	private Vector2 GetCameraCenter()
	{
		Camera2D camera = GetViewport().GetCamera2D();
		if (camera != null)
			return camera.GetScreenCenterPosition();
		Rect2 rect = GetViewport().GetVisibleRect();
		return rect.Position + (rect.Size * 0.5f);
	}

	private void OnViewportSizeChanged()
	{
		_hasCameraSample = false;
		RebuildTiles();
	}

	private void TryRebuildOnCameraTileStep()
	{
		if (_grassFill == null)
			return;

		Vector2 tileSize = ResolveTileSize(_grassFill);
		Vector2 center = GetCameraCenter();
		Vector2I cameraTile = new(
			Mathf.FloorToInt(center.X / tileSize.X),
			Mathf.FloorToInt(center.Y / tileSize.Y));

		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;

		bool zoomChanged = !_lastCameraZoom.IsEqualApprox(zoom);
		bool tileChanged = cameraTile != _lastCameraTile;
		if (!_hasCameraSample || zoomChanged || tileChanged)
		{
			_hasCameraSample = true;
			_lastCameraTile = cameraTile;
			_lastCameraZoom = zoom;
			RebuildTiles();
		}
	}

	public bool IsDirtAtWorldPosition(Vector2 worldPosition)
	{
		if (_grassFill == null)
			return false;

		Vector2 tileSize = ResolveTileSize(_grassFill);
		Vector2I tile = new(
			Mathf.FloorToInt(worldPosition.X / tileSize.X),
			Mathf.FloorToInt(worldPosition.Y / tileSize.Y));
		return IsDirtAtTile(tile);
	}

	public float GetTileWorldSpan()
	{
		if (_grassFill == null)
			return 48f;
		Vector2 tileSize = ResolveTileSize(_grassFill);
		return Mathf.Max(1f, Mathf.Max(tileSize.X, tileSize.Y));
	}

	private void EnsureGeneratedLayers()
	{
		_generatedRoot ??= GetNodeOrNull<Node2D>("Generated");
		if (!IsInstanceValid(_generatedRoot))
		{
			_generatedRoot = new Node2D { Name = "Generated", ZAsRelative = true };
			AddChild(_generatedRoot);
		}
		else
		{
			_generatedRoot.ZAsRelative = true;
		}

		_generatedGrassLayer = EnsureLayerNode(_generatedRoot, _generatedGrassLayer, "GrassLayer", 0);
		_generatedDirtLayer = EnsureLayerNode(_generatedRoot, _generatedDirtLayer, "DirtLayer", 1);
		_generatedCapLayer = EnsureLayerNode(_generatedRoot, _generatedCapLayer, "CapLayer", 2);
	}

	private static Node2D EnsureLayerNode(Node2D parent, Node2D layer, string name, int zIndex)
	{
		layer ??= parent.GetNodeOrNull<Node2D>(name);
		if (!GodotObject.IsInstanceValid(layer))
		{
			layer = new Node2D { Name = name };
			parent.AddChild(layer);
		}

		layer.ZIndex = zIndex;
		layer.ZAsRelative = true;
		return layer;
	}

	private static void ClearLayerChildren(Node2D layer)
	{
		if (!GodotObject.IsInstanceValid(layer))
			return;

		foreach (Node child in layer.GetChildren())
			child.QueueFree();
	}
}
