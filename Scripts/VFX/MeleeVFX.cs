using Godot;
using System;

public partial class MeleeVFX : Node2D
{
	[Export] public bool UseArc = false;
	[Export] public int ArcSegments = 12;
	[Export] public NodePath SpritePath = "Sprite2D";
	[Export] public float SpriteForwardOffset = 0f;
	[Export] public float SpriteSideOffset = 0f;
	[Export] public float SpriteAngleOffset = 0f;
	[Export] public bool FadeSprite = true;
	[Export] public bool MatchSpriteSizeToRange = true;
	[Export] public float SpriteSizeFromRange = 1.2f;
	[Export] public float MinSpriteScale = 0.05f;
	[Export] public float MaxSpriteScale = 1.0f;

	private Polygon2D _arc;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_arc = GetNodeOrNull<Polygon2D>("Arc");
		if (SpritePath != null && !SpritePath.IsEmpty)
			_sprite = GetNodeOrNull<Sprite2D>(SpritePath);
	}

	public void Init(Vector2 direction, float range, float arcDegrees, float duration, Color color)
	{
		if (_arc == null)
			_arc = GetNodeOrNull<Polygon2D>("Arc");
		if (_sprite == null && SpritePath != null && !SpritePath.IsEmpty)
			_sprite = GetNodeOrNull<Sprite2D>(SpritePath);

		if (direction.LengthSquared() < 0.0001f)
			direction = Vector2.Right;

		if (_arc != null)
		{
			_arc.Visible = UseArc;
			if (UseArc)
			{
				float halfArc = Mathf.DegToRad(arcDegrees) * 0.5f;
				int steps = Mathf.Max(3, ArcSegments);
				var points = new Vector2[steps + 2];
				points[0] = Vector2.Zero;
				for (int i = 0; i <= steps; i++)
				{
					float t = (float)i / steps;
					float ang = -halfArc + (2f * halfArc * t);
					points[i + 1] = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * range;
				}

				_arc.Polygon = points;
				_arc.Color = color;
			}
		}
		if (_sprite != null)
		{
			if (MatchSpriteSizeToRange && _sprite.Texture != null)
			{
				Vector2 texSize = _sprite.Texture.GetSize();
				float textureMaxSize = Mathf.Max(texSize.X, texSize.Y);
				if (textureMaxSize > 0f)
				{
					float targetWorldSize = Mathf.Max(1f, range * SpriteSizeFromRange);
					float uniformScale = Mathf.Clamp(targetWorldSize / textureMaxSize, MinSpriteScale, MaxSpriteScale);
					_sprite.Scale = new Vector2(uniformScale, uniformScale);
				}
			}

			_sprite.Position = new Vector2(SpriteForwardOffset, SpriteSideOffset);
			_sprite.Rotation = Mathf.DegToRad(SpriteAngleOffset);
			_sprite.Modulate = color;
		}

		var tween = CreateTween();
		if (_arc != null && UseArc)
			tween.TweenProperty(_arc, "modulate:a", 0f, duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		if (_sprite != null && FadeSprite)
		{
			tween.Parallel().TweenProperty(_sprite, "modulate:a", 0f, duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.Parallel().TweenProperty(_sprite, "scale", _sprite.Scale * 1.06f, duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		}
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
