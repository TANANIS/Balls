using Godot;
using System.Collections.Generic;

/*
 * Temporary space backdrop:
 * - Fullscreen black mask.
 * - White glowing dots with lightweight twinkle animation.
 * - Built as runtime-generated visuals so art can be swapped later.
 */
public partial class SpaceBackdrop : Node2D
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public int StarCount = 120;
	[Export] public float MaskAlpha = 0.92f;
	[Export] public float StarMinScale = 0.05f;
	[Export] public float StarMaxScale = 0.22f;
	[Export] public float StarMinAlpha = 0.30f;
	[Export] public float StarMaxAlpha = 0.95f;
	[Export] public float StarParallaxMin = 0.03f;
	[Export] public float StarParallaxMax = 0.13f;
	[Export] public float StarFieldFillMultiplier = 1.6f;
	[Export] public float TwinkleSpeedMin = 0.30f;
	[Export] public float TwinkleSpeedMax = 1.25f;
	[Export] public float TwinkleAmplitude = 0.35f;

	private readonly RandomNumberGenerator _rng = new();
	private readonly List<StarData> _stars = new();
	private ColorRect _mask;
	private Node2D _starRoot;
	private Texture2D _starTexture;
	private Vector2 _viewportSize;
	private Node2D _player;

	private sealed class StarData
	{
		public Sprite2D Sprite;
		public Vector2 ViewportOffset;
		public float ParallaxFactor;
		public float BaseAlpha;
		public float TwinkleSpeed;
		public float TwinklePhase;
	}

	public override void _Ready()
	{
		ZIndex = -30;
		_rng.Randomize();

		_mask = new ColorRect
		{
			Name = "Mask",
			TopLevel = true,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ZIndex = -30,
			Color = new Color(0f, 0f, 0f, Mathf.Clamp(MaskAlpha, 0f, 1f))
		};
		AddChild(_mask);

		_starRoot = new Node2D
		{
			Name = "Stars",
			TopLevel = true,
			ZIndex = -29
		};
		AddChild(_starRoot);

		_starTexture = BuildGlowTexture(36);
		GetViewport().SizeChanged += RegenerateStars;
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		RegenerateStars();
	}

	public override void _Process(double delta)
	{
		UpdateBackdropTransform();
		UpdateTwinkle((float)delta);
	}

	private void RegenerateStars()
	{
		_viewportSize = GetViewport().GetVisibleRect().Size;
		foreach (Node child in _starRoot.GetChildren())
			child.QueueFree();
		_stars.Clear();

		int count = Mathf.Max(16, StarCount);
		Vector2 field = _viewportSize * Mathf.Max(1.1f, StarFieldFillMultiplier);
		for (int i = 0; i < count; i++)
		{
			var sprite = new Sprite2D
			{
				Texture = _starTexture,
				Centered = true,
				Scale = Vector2.One * _rng.RandfRange(StarMinScale, StarMaxScale)
			};
			_starRoot.AddChild(sprite);

			float baseAlpha = _rng.RandfRange(StarMinAlpha, StarMaxAlpha);
			var data = new StarData
			{
				Sprite = sprite,
				ViewportOffset = new Vector2(
					_rng.RandfRange(-field.X * 0.5f, field.X * 0.5f),
					_rng.RandfRange(-field.Y * 0.5f, field.Y * 0.5f)),
				ParallaxFactor = _rng.RandfRange(StarParallaxMin, StarParallaxMax),
				BaseAlpha = baseAlpha,
				TwinkleSpeed = _rng.RandfRange(TwinkleSpeedMin, TwinkleSpeedMax),
				TwinklePhase = _rng.RandfRange(0f, Mathf.Tau)
			};
			sprite.Modulate = new Color(1f, 1f, 1f, baseAlpha);
			_stars.Add(data);
		}

		UpdateBackdropTransform();
	}

	private void UpdateBackdropTransform()
	{
		if (!IsInstanceValid(_player))
			_player = GetNodeOrNull<Node2D>(PlayerPath);

		Vector2 center = GetCameraCenter();
		_mask.Size = _viewportSize;
		_mask.GlobalPosition = center - (_viewportSize * 0.5f);
		_starRoot.GlobalPosition = center;

		Vector2 driver = GetParallaxDriverPosition(center);
		Vector2 field = _viewportSize * Mathf.Max(1.1f, StarFieldFillMultiplier);
		float halfW = field.X * 0.5f;
		float halfH = field.Y * 0.5f;

		foreach (StarData star in _stars)
		{
			Vector2 raw = star.ViewportOffset - (driver * star.ParallaxFactor);
			star.Sprite.Position = new Vector2(
				Wrap(raw.X, halfW),
				Wrap(raw.Y, halfH));
		}
	}

	private void UpdateTwinkle(float dt)
	{
		if (_stars.Count == 0)
			return;

		foreach (StarData star in _stars)
		{
			star.TwinklePhase += dt * star.TwinkleSpeed;
			float pulse = Mathf.Sin(star.TwinklePhase);
			float alpha = star.BaseAlpha + (pulse * TwinkleAmplitude * star.BaseAlpha);
			alpha = Mathf.Clamp(alpha, 0.05f, 1f);
			star.Sprite.Modulate = new Color(1f, 1f, 1f, alpha);
		}
	}

	private Vector2 GetCameraCenter()
	{
		Camera2D camera = GetViewport().GetCamera2D();
		if (camera != null)
			return camera.GetScreenCenterPosition();
		Rect2 rect = GetViewport().GetVisibleRect();
		return rect.Position + (rect.Size * 0.5f);
	}

	private Vector2 GetParallaxDriverPosition(Vector2 fallback)
	{
		if (IsInstanceValid(_player))
			return _player.GlobalPosition;
		return fallback;
	}

	private static float Wrap(float value, float halfSpan)
	{
		float span = halfSpan * 2f;
		if (span <= 0.001f)
			return value;
		return Mathf.PosMod(value + halfSpan, span) - halfSpan;
	}

	private static Texture2D BuildGlowTexture(int size)
	{
		var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
		Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
		float radius = size * 0.5f;

		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float dist = new Vector2(x, y).DistanceTo(center);
				float t = Mathf.Clamp(1f - (dist / radius), 0f, 1f);
				float a = Mathf.Pow(t, 2.4f);
				image.SetPixel(x, y, new Color(1f, 1f, 1f, a));
			}
		}

		return ImageTexture.CreateFromImage(image);
	}
}
