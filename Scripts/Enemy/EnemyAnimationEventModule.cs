using Godot;

/*
 * EnemyAnimationEventModule:
 * - Plays hurt/death animations from enemy lifecycle callbacks.
 * - Keeps movement animation as default state.
 * - Disables combat collision on death while waiting for despawn delay.
 */
public partial class EnemyAnimationEventModule : EnemyEventModule
{
	[Export] public StringName MoveAnimation = "walk";
	[Export] public StringName HurtAnimation = "hurt";
	[Export] public StringName DeathAnimation = "death";
	[Export(PropertyHint.Range, "0.02,1,0.01")] public float HurtReturnDelaySeconds = 0.14f;

	private Enemy _enemy;
	private EnemyHealth _health;
	private AnimatedSprite2D _animatedSprite;
	private Tween _recoverTween;
	private int _hurtToken = 0;

	public override void OnInitialized(Enemy enemy)
	{
		_enemy = enemy;
		_health = enemy.GetNodeOrNull<EnemyHealth>("Health");
		_animatedSprite = enemy.GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? enemy.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		PlayMove();
	}

	public override void OnSpawned(Enemy enemy)
	{
		PlayMove();
	}

	public override void OnDamaged(Enemy enemy, int amount, object source)
	{
		if (!Active || _animatedSprite == null)
			return;
		if (_health != null && _health.IsDead)
			return;

		if (HasAnimation(HurtAnimation))
			_animatedSprite.Play(HurtAnimation);

		int token = ++_hurtToken;
		_recoverTween?.Kill();
		_recoverTween = CreateTween();
		_recoverTween.TweenInterval(Mathf.Max(0.02f, HurtReturnDelaySeconds));
		_recoverTween.TweenCallback(Callable.From(() =>
		{
			if (!IsInstanceValid(_animatedSprite))
				return;
			if (_health != null && _health.IsDead)
				return;
			if (token != _hurtToken)
				return;
			PlayMove();
		}));
	}

	public override void OnDeath(Enemy enemy, object killer)
	{
		if (!Active)
			return;

		_recoverTween?.Kill();
		_hurtToken++;
		DisableDamageAreas(enemy);

		if (_animatedSprite != null && HasAnimation(DeathAnimation))
		{
			_animatedSprite.Play(DeathAnimation);
			_animatedSprite.SpeedScale = 1f;
		}
	}

	private void PlayMove()
	{
		if (_animatedSprite == null)
			return;
		if (HasAnimation(MoveAnimation))
			_animatedSprite.Play(MoveAnimation);
	}

	private bool HasAnimation(StringName name)
	{
		return _animatedSprite?.SpriteFrames != null
			&& !name.IsEmpty
			&& _animatedSprite.SpriteFrames.HasAnimation(name);
	}

	private static void DisableDamageAreas(Enemy enemy)
	{
		EnemyHitbox hitbox = enemy.GetNodeOrNull<EnemyHitbox>("Hitbox");
		if (hitbox != null)
		{
			hitbox.SetDeferred("monitoring", false);
			hitbox.SetDeferred("monitorable", false);
			hitbox.CallDeferred(Node.MethodName.SetPhysicsProcess, false);
		}

		EnemyHurtbox hurtbox = enemy.GetNodeOrNull<EnemyHurtbox>("Hurtbox");
		if (hurtbox != null)
		{
			hurtbox.SetDeferred("monitoring", false);
			hurtbox.SetDeferred("monitorable", false);
		}
	}
}
