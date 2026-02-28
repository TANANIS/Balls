using Godot;

public partial class PlayerHealth
{
	private const string PriestCharacterId = "tank_burst";
	private const string DefaultPriestHealTexturePath = "res://Assets/Sprites/Player/Priest/Priest-Heal.png";
	private SpriteFrames _priestHealFrames;

	private void TryPlayPriestHealVfx()
	{
		if (!IsPriestCharacter())
			return;

		Player player = GetParentOrNull<Player>();
		if (player == null)
			return;

		Node2D anchor = ResolveSkillVfxRoot();
		if (anchor == null)
			return;

		SpriteFrames frames = BuildPriestHealFramesIfNeeded();
		if (frames == null || frames.GetFrameCount("default") <= 0)
			return;

		float duration = frames.GetFrameCount("default") / Mathf.Max(1f, PriestHealFps);
		player.LockAttacks(duration, interruptCurrentAttack: true);

		var fx = new AnimatedSprite2D
		{
			Name = "PriestHealVfx",
			SpriteFrames = frames,
			Animation = "default",
			Centered = true,
			TopLevel = false,
			ZAsRelative = false,
			ZIndex = PriestHealZIndex,
			Position = PriestHealOffset,
			Scale = Vector2.One * Mathf.Max(0.1f, PriestHealScale)
		};

		anchor.AddChild(fx);
		fx.Play("default");
		fx.AnimationFinished += () =>
		{
			if (IsInstanceValid(fx))
				fx.QueueFree();
		};
	}

	private SpriteFrames BuildPriestHealFramesIfNeeded()
	{
		if (_priestHealFrames != null)
			return _priestHealFrames;

		Texture2D texture = PriestHealTexture;
		if (texture == null)
			texture = GD.Load<Texture2D>(DefaultPriestHealTexturePath);
		if (texture == null)
			return null;

		int frameCount = Mathf.Max(1, PriestHealFrameCount);
		if (frameCount <= 1 && texture.GetHeight() > 0)
		{
			float ratio = texture.GetWidth() / (float)texture.GetHeight();
			frameCount = Mathf.Max(1, Mathf.RoundToInt(ratio));
		}

		var frames = new SpriteFrames();
		frames.AddAnimation("default");
		frames.SetAnimationLoop("default", false);
		frames.SetAnimationSpeed("default", Mathf.Max(1f, PriestHealFps));

		if (frameCount <= 1)
		{
			frames.AddFrame("default", texture);
		}
		else
		{
			float frameWidth = texture.GetWidth() / (float)frameCount;
			float frameHeight = texture.GetHeight();
			for (int i = 0; i < frameCount; i++)
			{
				var atlas = new AtlasTexture
				{
					Atlas = texture,
					Region = new Rect2(i * frameWidth, 0f, frameWidth, frameHeight),
					FilterClip = true
				};
				frames.AddFrame("default", atlas);
			}
		}

		_priestHealFrames = frames;
		return _priestHealFrames;
	}

	private bool IsPriestCharacter()
	{
		Player player = GetParentOrNull<Player>();
		if (player?.ActiveCharacter == null)
			return false;
		return string.Equals(
			player.ActiveCharacter.CharacterId,
			PriestCharacterId,
			System.StringComparison.OrdinalIgnoreCase);
	}
}
