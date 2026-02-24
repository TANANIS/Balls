using Godot;

public partial class EnemyDashBehavior : EnemyBehaviorModule
{
	[Export] public float ChaseSpeedMultiplier = 0.8f;
	[Export] public float DashSpeedMultiplier = 2.4f;
	[Export] public float TriggerDistance = 280f;
	[Export] public float WindupDuration = 0.28f;
	[Export] public float DashDuration = 0.18f;
	[Export] public float DashCooldown = 1.05f;
	[Export] public float AimPredictionSeconds = 0.20f;
	[Export] public float DashSteerStrength = 6.2f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float ChainDashChance = 0.45f;
	[Export] public int MaxChainCount = 1;
	[Export] public float ChainWindupMultiplier = 0.58f;
	[Export] public float MinAimDistance = 12f;
	[Export] public bool BindAnimationToDashState = true;
	[Export] public StringName MoveAnimation = "walk";
	[Export] public StringName AttackAnimation = "attack";
	[Export] public StringName HurtAnimation = "hurt";
	[Export] public StringName DeathAnimation = "death";

	private enum DashState
	{
		Chase,
		Windup,
		Dash,
		Cooldown
	}

	private DashState _state = DashState.Chase;
	private float _stateTimer = 0f;
	private Vector2 _dashDirection = Vector2.Right;
	private int _chainCount = 0;
	private AnimatedSprite2D _animatedSprite;
	private readonly RandomNumberGenerator _rng = new();

	public override void OnInitialized(Enemy enemy)
	{
		_rng.Randomize();
		_state = DashState.Chase;
		_stateTimer = 0f;
		_chainCount = 0;
		_animatedSprite = enemy?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? enemy?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		PlayStateAnimation();
	}

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		float dt = (float)delta;
		Vector2 toPlayer = GetPredictedAimVector(enemy, player);
		float distance = toPlayer.Length();

		switch (_state)
		{
			case DashState.Chase:
				if (distance <= TriggerDistance)
				{
					_dashDirection = distance > MinAimDistance ? toPlayer / distance : Vector2.Right;
					SetState(DashState.Windup);
					_stateTimer = Mathf.Max(0.02f, WindupDuration * (_chainCount > 0 ? ChainWindupMultiplier : 1f));
					return Vector2.Zero;
				}

				return GetChaseVelocity(enemy, toPlayer, distance);

			case DashState.Windup:
				_stateTimer -= dt;
				if (_stateTimer <= 0f)
				{
					SetState(DashState.Dash);
					_stateTimer = DashDuration;
				}
				return Vector2.Zero;

			case DashState.Dash:
				_stateTimer -= dt;
				Vector2 aimVector = GetPredictedAimVector(enemy, player);
				if (aimVector.LengthSquared() > 0.001f)
				{
					Vector2 targetDir = aimVector.Normalized();
					_dashDirection = _dashDirection.Slerp(targetDir, Mathf.Max(0f, DashSteerStrength) * dt).Normalized();
				}

				if (_stateTimer <= 0f)
				{
					Vector2 checkVector = GetPredictedAimVector(enemy, player);
					float checkDistance = checkVector.Length();
					bool canChain = _chainCount < Mathf.Max(0, MaxChainCount)
						&& checkDistance <= TriggerDistance * 1.25f
						&& _rng.Randf() <= Mathf.Clamp(ChainDashChance, 0f, 1f);

					if (canChain)
					{
						_chainCount++;
						_dashDirection = checkDistance > MinAimDistance ? checkVector / checkDistance : _dashDirection;
						SetState(DashState.Windup);
						_stateTimer = Mathf.Max(0.02f, WindupDuration * Mathf.Max(0.2f, ChainWindupMultiplier));
						return Vector2.Zero;
					}

					SetState(DashState.Cooldown);
					_stateTimer = DashCooldown;
					_chainCount = 0;
				}
				return _dashDirection * enemy.MaxSpeed * DashSpeedMultiplier;

			case DashState.Cooldown:
				_stateTimer -= dt;
				if (_stateTimer <= 0f)
					SetState(DashState.Chase);
				return GetChaseVelocity(enemy, toPlayer, distance);
		}

		return Vector2.Zero;
	}

	private Vector2 GetChaseVelocity(Enemy enemy, Vector2 toPlayer, float distance)
	{
		if (distance < 0.0001f)
			return Vector2.Zero;

		return (toPlayer / distance) * enemy.MaxSpeed * ChaseSpeedMultiplier;
	}

	private Vector2 GetPredictedAimVector(Enemy enemy, Node2D player)
	{
		Vector2 target = player.GlobalPosition;
		if (player is CharacterBody2D movingPlayer)
			target += movingPlayer.Velocity * Mathf.Max(0f, AimPredictionSeconds);
		return target - enemy.GlobalPosition;
	}

	private void SetState(DashState state)
	{
		if (_state == state)
			return;
		_state = state;
		PlayStateAnimation();
	}

	private void PlayStateAnimation()
	{
		if (!BindAnimationToDashState || _animatedSprite?.SpriteFrames == null)
			return;
		if (IsBlockingAnimationPlaying())
			return;

		StringName animation = (_state == DashState.Windup || _state == DashState.Dash)
			? AttackAnimation
			: MoveAnimation;

		if (animation.IsEmpty || !_animatedSprite.SpriteFrames.HasAnimation(animation))
			return;
		if (_animatedSprite.Animation == animation && _animatedSprite.IsPlaying())
			return;

		_animatedSprite.Play(animation);
	}

	private bool IsBlockingAnimationPlaying()
	{
		if (_animatedSprite == null || !_animatedSprite.IsPlaying())
			return false;

		StringName current = _animatedSprite.Animation;
		if (!HurtAnimation.IsEmpty && current == HurtAnimation)
			return true;
		if (!DeathAnimation.IsEmpty && current == DeathAnimation)
			return true;

		return false;
	}
}
