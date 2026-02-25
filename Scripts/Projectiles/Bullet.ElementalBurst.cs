using Godot;
using System.Collections.Generic;

public partial class Bullet
{
	private void TryTriggerElementalBurstExplosion(Node hitTarget)
	{
		if (!_isElementalBurstShot || _elementalBurstDetonated)
			return;

		_elementalBurstDetonated = true;
		Vector2 center = (hitTarget as Node2D)?.GlobalPosition ?? GlobalPosition;
		SpawnElementalBurstRune(center, _elementalBurstRadiusRuntime);
		AudioManager.Instance?.PlaySfxPlayerElementalBurst();
		TryResolveCombatSystem();

		if (_combat != null && _source != null)
		{
			List<Node> targets = CollectElementalBurstTargets(center, _elementalBurstRadiusRuntime, _elementalBurstMaxTargetsRuntime);
			for (int i = 0; i < targets.Count; i++)
			{
				Node target = targets[i];
				if (target == null || target == _source)
					continue;
				if (target is not IDamageable)
					continue;

				var req = new DamageRequest(
					source: _source,
					target: target,
					baseDamage: Mathf.Max(1, _damage),
					worldPos: center,
					tag: "elemental_burst",
					damageScale: Mathf.Clamp(_damageScale * _elementalBurstDamageMultiplierRuntime, 0.001f, 10f)
				);

				_combat.RequestDamage(req);
			}
		}

		NotifyElementalBurstOwnerDetonated();
	}

	private void SpawnElementalBurstRune(Vector2 center, float radius)
	{
		Node parent = GetParent();
		if (parent == null)
			return;

		if (TrySpawnElementalBurstExplosionAnimation(parent, center, radius))
			return;

		// Fallback for environments where animated explosion frames are unavailable.
		if (!ResourceLoader.Exists(ElementalBurstExplosionRunePath))
			return;

		Texture2D texture = GD.Load<Texture2D>(ElementalBurstExplosionRunePath);
		if (texture == null)
			return;

		var rune = new Sprite2D
		{
			Texture = texture,
			Centered = true,
			GlobalPosition = center,
			ZIndex = 20
		};

		float maxDim = Mathf.Max(1f, Mathf.Max(texture.GetWidth(), texture.GetHeight()));
		float diameter = Mathf.Max(1f, radius * 2f);
		float baseScale = diameter / maxDim;
		rune.Scale = new Vector2(baseScale, baseScale);
		rune.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(ElementalBurstRuneStartAlpha, 0f, 1f));
		parent.AddChild(rune);

		float duration = Mathf.Max(0.05f, ElementalBurstRuneDurationSeconds);
		float expandScale = baseScale * Mathf.Max(1f, ElementalBurstRuneScaleExpand);
		Tween tween = rune.CreateTween();
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(Tween.EaseType.Out);
		tween.Parallel().TweenProperty(rune, "scale", new Vector2(expandScale, expandScale), duration);
		tween.Parallel().TweenProperty(rune, "modulate:a", 0f, duration);
		tween.Finished += rune.QueueFree;
	}

	private bool TrySpawnElementalBurstExplosionAnimation(Node parent, Vector2 center, float radius)
	{
		if (parent == null)
			return false;

		var frames = new SpriteFrames();
		frames.AddAnimation("default");
		frames.SetAnimationLoop("default", false);
		float fps = Mathf.Max(1f, ElementalBurstExplosionVfxFps);
		frames.SetAnimationSpeed("default", fps);

		float maxDim = 1f;
		int frameCount = 0;
		for (int i = 0; i < ElementalBurstExplosionFramePaths.Length; i++)
		{
			string path = ElementalBurstExplosionFramePaths[i];
			if (!ResourceLoader.Exists(path))
				continue;

			Texture2D tex = GD.Load<Texture2D>(path);
			if (tex == null)
				continue;

			frames.AddFrame("default", tex);
			frameCount++;
			maxDim = Mathf.Max(maxDim, Mathf.Max(tex.GetWidth(), tex.GetHeight()));
		}

		if (frameCount <= 0)
			return false;

		float diameter = Mathf.Max(1f, radius * 2f);
		float baseScale = diameter / maxDim;
		float expandScale = baseScale * Mathf.Max(1f, ElementalBurstRuneScaleExpand);

		var explosionFx = new AnimatedSprite2D
		{
			SpriteFrames = frames,
			Animation = "default",
			Centered = true,
			GlobalPosition = center,
			ZIndex = 20,
			Scale = new Vector2(baseScale, baseScale),
			Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(ElementalBurstRuneStartAlpha, 0f, 1f))
		};
		parent.AddChild(explosionFx);
		explosionFx.Play("default");

		// Play explosion in normal timing: no special last-frame hold/fade extension.
		float animationDuration = Mathf.Max(0.05f, frameCount / fps);

		Tween tween = explosionFx.CreateTween();
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(Tween.EaseType.Out);
		tween.Parallel().TweenProperty(explosionFx, "scale", new Vector2(expandScale, expandScale), animationDuration);
		tween.TweenInterval(animationDuration);
		tween.TweenCallback(Callable.From(explosionFx.QueueFree));
		return true;
	}

	private List<Node> CollectElementalBurstTargets(Vector2 center, float radius, int maxTargets)
	{
		var targets = new List<Node>();
		var world = GetWorld2D();
		if (world?.DirectSpaceState == null)
			return targets;

		var shape = new CircleShape2D
		{
			Radius = Mathf.Max(1f, radius)
		};

		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = shape,
			Transform = new Transform2D(0f, center),
			CollisionMask = CollisionMask,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var hits = world.DirectSpaceState.IntersectShape(query, Mathf.Max(maxTargets * 4, 16));
		var seen = new HashSet<ulong>();
		for (int i = 0; i < hits.Count; i++)
		{
			var hit = hits[i];
			if (!hit.ContainsKey("collider"))
				continue;

			GodotObject colliderObj = hit["collider"].AsGodotObject();
			if (colliderObj is not Node collider)
				continue;
			if (collider is not EnemyHurtbox)
				continue;

			ulong id = (ulong)collider.GetInstanceId();
			if (!seen.Add(id))
				continue;

			targets.Add(collider);
		}

		targets.Sort((a, b) =>
		{
			Vector2 aPos = (a as Node2D)?.GlobalPosition ?? center;
			Vector2 bPos = (b as Node2D)?.GlobalPosition ?? center;
			return aPos.DistanceSquaredTo(center).CompareTo(bPos.DistanceSquaredTo(center));
		});

		if (targets.Count > maxTargets)
			targets.RemoveRange(maxTargets, targets.Count - maxTargets);

		return targets;
	}

	private void NotifyElementalBurstOwnerDetonated()
	{
		if (_elementalBurstOwner == null)
			return;
		if (!IsInstanceValid(_elementalBurstOwner))
		{
			_elementalBurstOwner = null;
			return;
		}

		_elementalBurstOwner.CallDeferred("NotifyElementalBurstDetonated");
		_elementalBurstOwner = null;
	}
}
