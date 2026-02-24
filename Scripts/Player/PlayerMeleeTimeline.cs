using Godot;

public sealed class PlayerMeleeTimeline
{
	public enum State
	{
		Idle,
		Windup,
		Recover
	}

	private State _state = State.Idle;
	private float _windupTimer = 0f;
	private float _windupDuration = 0f;
	private float _hitAtNormalized = 0.55f;
	private bool _hitTriggered = false;
	private float _recoverTimer = 0f;

	private Vector2 _attackDir = Vector2.Right;
	private float _attackRange = 0f;
	private int _attackDamage = 1;

	public State Current => _state;
	public bool IsBusy => _state != State.Idle;

	public void Reset()
	{
		_state = State.Idle;
		_windupTimer = 0f;
		_windupDuration = 0f;
		_hitAtNormalized = 0.55f;
		_hitTriggered = false;
		_recoverTimer = 0f;
		_attackDir = Vector2.Right;
		_attackRange = 0f;
		_attackDamage = 1;
	}

	public void BeginAttack(
		float windupDurationSeconds,
		float hitAtNormalized,
		float recoverDurationSeconds,
		Vector2 attackDir,
		float attackRange,
		int attackDamage)
	{
		_state = State.Windup;
		_windupTimer = 0f;
		_windupDuration = Mathf.Max(0.01f, windupDurationSeconds);
		_hitAtNormalized = Mathf.Clamp(hitAtNormalized, 0.05f, 0.95f);
		_hitTriggered = false;
		_recoverTimer = Mathf.Max(0f, recoverDurationSeconds);
		_attackDir = attackDir;
		_attackRange = Mathf.Max(0f, attackRange);
		_attackDamage = Mathf.Max(1, attackDamage);
	}

	public void Tick(float dt, System.Action<Vector2, float, int> onHit)
	{
		if (_state == State.Idle)
			return;

		if (_state == State.Windup)
		{
			_windupTimer += dt;
			float hitAt = _windupDuration * _hitAtNormalized;
			if (!_hitTriggered && _windupTimer >= hitAt)
			{
				onHit?.Invoke(_attackDir, _attackRange, _attackDamage);
				_hitTriggered = true;
			}

			if (_windupTimer >= _windupDuration)
			{
				_state = _recoverTimer > 0f ? State.Recover : State.Idle;
			}
			return;
		}

		if (_state != State.Recover)
			return;

		_recoverTimer -= dt;
		if (_recoverTimer <= 0f)
			_state = State.Idle;
	}
}
