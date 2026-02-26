using Godot;

public partial class Player
{
	private readonly struct CharacterAnimationProfile
	{
		public CharacterAnimationProfile(
			string basePath,
			string attackSuffix,
			string deathSuffix,
			string dashSuffix = "",
			int dashFrameCount = 0,
			int dashSourceFrameCount = 0)
		{
			BasePath = basePath;
			AttackSuffix = attackSuffix;
			DeathSuffix = deathSuffix;
			DashSuffix = dashSuffix;
			DashFrameCount = dashFrameCount;
			DashSourceFrameCount = dashSourceFrameCount;
		}

		public string BasePath { get; }
		public string AttackSuffix { get; }
		public string DeathSuffix { get; }
		public string DashSuffix { get; }
		public int DashFrameCount { get; }
		public int DashSourceFrameCount { get; }
	}

	private void ApplyCharacterAnimationProfile(string characterId)
	{
		if (_animatedSprite == null || _baseSpriteFrames == null)
			return;

		CharacterAnimationProfile profile = ResolveAnimationProfile(characterId);
		SpriteFrames runtimeFrames = BuildSpriteFramesFromProfile(_baseSpriteFrames, profile);
		_animatedSprite.SpriteFrames = runtimeFrames;
	}

	private CharacterAnimationProfile ResolveAnimationProfile(string characterId)
	{
		if (string.Equals(characterId, "swordsman") || string.Equals(characterId, "sowrdman") || string.Equals(characterId, "melee"))
			return new CharacterAnimationProfile(
				"res://Assets/Sprites/Player/Swordsman/Swordsman",
				"-Attack01",
				"-Death",
				"-Dash",
				3,
				3);
		if (string.Equals(characterId, "tank_burst"))
			return new CharacterAnimationProfile("res://Assets/Sprites/Player/Priest/Priest", "-Attack", "-Death");
		if (string.Equals(characterId, "archer"))
			return new CharacterAnimationProfile("res://Assets/Sprites/Player/Archer/Archer", "-Attack02", "-Death");
		return new CharacterAnimationProfile("res://Assets/Sprites/Player/Wizard/Wizard", "-Attack02", "-DEATH");
	}

	private SpriteFrames BuildSpriteFramesFromProfile(SpriteFrames template, CharacterAnimationProfile profile)
	{
		var runtime = new SpriteFrames();

		Texture2D idle = GD.Load<Texture2D>($"{profile.BasePath}-Idle.png");
		Texture2D walk = GD.Load<Texture2D>($"{profile.BasePath}-Walk.png");
		Texture2D hurt = GD.Load<Texture2D>($"{profile.BasePath}-Hurt.png");
		Texture2D attack = GD.Load<Texture2D>($"{profile.BasePath}{profile.AttackSuffix}.png");
		Texture2D death = GD.Load<Texture2D>($"{profile.BasePath}{profile.DeathSuffix}.png");

		foreach (StringName animation in template.GetAnimationNames())
		{
			runtime.AddAnimation(animation);
			runtime.SetAnimationLoop(animation, template.GetAnimationLoop(animation));
			runtime.SetAnimationSpeed(animation, template.GetAnimationSpeed(animation));

			Texture2D animationAtlas = ResolveAnimationAtlas(animation, idle, walk, hurt, attack, death);
			int frameCount = template.GetFrameCount(animation);
			for (int i = 0; i < frameCount; i++)
			{
				Texture2D frameTexture = BuildFrameTexture(template, animation, i, animationAtlas, frameCount);
				runtime.AddFrame(animation, frameTexture, template.GetFrameDuration(animation, i));
			}
		}

		AppendDashAnimationIfConfigured(runtime, profile);
		return runtime;
	}

	private static void AppendDashAnimationIfConfigured(SpriteFrames runtime, CharacterAnimationProfile profile)
	{
		if (string.IsNullOrWhiteSpace(profile.DashSuffix) || profile.DashFrameCount <= 0)
			return;

		Texture2D dashAtlas = GD.Load<Texture2D>($"{profile.BasePath}{profile.DashSuffix}.png");
		if (dashAtlas == null)
			return;

		StringName dash = "dash";
		if (runtime.HasAnimation(dash))
			runtime.RemoveAnimation(dash);
		runtime.AddAnimation(dash);
		runtime.SetAnimationLoop(dash, true);
		runtime.SetAnimationSpeed(dash, 16f);

		int sourceFrameCount = Mathf.Max(1, profile.DashSourceFrameCount);
		int useFrameCount = Mathf.Clamp(profile.DashFrameCount, 1, sourceFrameCount);
		float frameWidth = dashAtlas.GetWidth() / (float)sourceFrameCount;
		for (int i = 0; i < useFrameCount; i++)
		{
			var atlas = new AtlasTexture
			{
				Atlas = dashAtlas,
				Region = new Rect2(i * frameWidth, 0f, frameWidth, dashAtlas.GetHeight()),
				FilterClip = true
			};
			runtime.AddFrame(dash, atlas, 1f);
		}
	}

	private static Texture2D ResolveAnimationAtlas(
		StringName animation,
		Texture2D idle,
		Texture2D walk,
		Texture2D hurt,
		Texture2D attack,
		Texture2D death)
	{
		if (animation == "idle")
			return idle;
		if (animation == "walk")
			return walk;
		if (animation == "hurt")
			return hurt;
		if (animation == "attack")
			return attack;
		if (animation == "death")
			return death;
		return null;
	}

	private static Texture2D BuildFrameTexture(
		SpriteFrames template,
		StringName animation,
		int frameIndex,
		Texture2D newAtlas,
		int fallbackFrameCount)
	{
		Texture2D templateTexture = template.GetFrameTexture(animation, frameIndex);
		if (newAtlas == null)
			return templateTexture;

		if (templateTexture is AtlasTexture templateAtlas)
		{
			Rect2 templateRegion = templateAtlas.Region;
			if (templateRegion.End.X <= newAtlas.GetWidth() && templateRegion.End.Y <= newAtlas.GetHeight())
			{
				return new AtlasTexture
				{
					Atlas = newAtlas,
					Region = templateRegion,
					FilterClip = true
				};
			}
		}

		int frameCount = Mathf.Max(1, fallbackFrameCount);
		float frameWidth = newAtlas.GetWidth() / (float)frameCount;
		return new AtlasTexture
		{
			Atlas = newAtlas,
			Region = new Rect2(frameIndex * frameWidth, 0f, frameWidth, newAtlas.GetHeight()),
			FilterClip = true
		};
	}
}
