using Godot;
using System;

public partial class BackgroundAnimatedSprite : AnimatedSprite2D
{
	[Export] public bool FitToViewport = true;
	[Export] public bool Cover = true;
	[Export] public string FramesDir = "res://Assets/Sprites/Background/PNG";
	[Export] public float FramesPerSecond = 60.0f;

	public override void _Ready()
	{
		EnsureFrames();
		Play("default");

		if (FitToViewport)
			Fit();

		GetViewport().SizeChanged += OnViewportSizeChanged;
	}

	private void OnViewportSizeChanged()
	{
		if (FitToViewport)
			Fit();
	}

	private void Fit()
	{
		var frames = SpriteFrames;
		if (frames == null)
			return;

		var tex = frames.GetFrameTexture("default", 0);
		if (tex == null)
			return;

		Vector2 texSize = tex.GetSize();
		if (texSize.X <= 0 || texSize.Y <= 0)
			return;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scale = Cover
			? Mathf.Max(viewportSize.X / texSize.X, viewportSize.Y / texSize.Y)
			: Mathf.Min(viewportSize.X / texSize.X, viewportSize.Y / texSize.Y);

		Scale = new Vector2(scale, scale);
		Position = viewportSize * 0.5f;
		Centered = true;
	}

	private void EnsureFrames()
	{
		if (SpriteFrames != null && SpriteFrames.HasAnimation("default"))
		{
			// Keep speed/loop in sync even if frames already exist.
			SpriteFrames.SetAnimationLoop("default", true);
			SpriteFrames.SetAnimationSpeed("default", FramesPerSecond);

			if (SpriteFrames.GetFrameCount("default") > 1)
				return;
		}

		var frames = new SpriteFrames();
		frames.AddAnimation("default");
		frames.SetAnimationLoop("default", true);
		frames.SetAnimationSpeed("default", FramesPerSecond);

		var files = new System.Collections.Generic.List<string>();
		var dir = DirAccess.Open(FramesDir);
		if (dir != null)
		{
			dir.ListDirBegin();
			while (true)
			{
				var file = dir.GetNext();
				if (file == "")
					break;
				if (dir.CurrentIsDir())
					continue;
				if (!file.EndsWith(".png"))
					continue;
				files.Add(file);
			}
			dir.ListDirEnd();
		}

		files.Sort(StringComparer.Ordinal);
		foreach (var file in files)
		{
			var path = FramesDir.TrimEnd('/') + "/" + file;
			var tex = GD.Load<Texture2D>(path);
			if (tex != null)
				frames.AddFrame("default", tex);
		}

		SpriteFrames = frames;
	}
}
