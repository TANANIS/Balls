using Godot;

public sealed class PlayerAttackTimeline
{
	public enum State
	{
		Idle,
		Windup,
		Burst
	}

	private State _state = State.Idle;
	private float _windupTimer = 0f;
	private float _windupDuration = 0f;
	private float _fireAtNormalized = 0.60f;
	private bool _fired = false;

	private Vector2 _windupAimFallbackDir = Vector2.Right;
	private float _shotSpeed = 0f;
	private int _shotBaseDamage = 1;

	private int _burstShotsRemaining = 0;
	private float _burstTimer = 0f;
	private float _burstInterval = 0.08f;
	private Vector2 _burstDir = Vector2.Right;

	public State Current => _state;
	public bool IsBusy => _state != State.Idle;

	public void Reset()
	{
		_state = State.Idle;
		_windupTimer = 0f;
		_windupDuration = 0f;
		_fired = false;
		_windupAimFallbackDir = Vector2.Right;
		_shotSpeed = 0f;
		_shotBaseDamage = 1;
		_burstShotsRemaining = 0;
		_burstTimer = 0f;
		_burstDir = Vector2.Right;
	}

	public void BeginWindup(
		float durationSeconds,
		float fireAtNormalized,
		Vector2 aimFallbackDir,
		float shotSpeed,
		int shotBaseDamage,
		int burstExtraShots,
		float burstIntervalSeconds)
	{
		_state = State.Windup;
		_windupTimer = 0f;
		_windupDuration = Mathf.Max(0.05f, durationSeconds);
		_fireAtNormalized = Mathf.Clamp(fireAtNormalized, 0.05f, 0.95f);
		_fired = false;
		_windupAimFallbackDir = aimFallbackDir;
		_shotSpeed = shotSpeed;
		_shotBaseDamage = Mathf.Max(1, shotBaseDamage);
		_burstShotsRemaining = Mathf.Max(0, burstExtraShots);
		_burstInterval = Mathf.Clamp(burstIntervalSeconds, 0.01f, 0.5f);
		_burstTimer = 0f;
		_burstDir = aimFallbackDir;
	}

	public void BeginBurst(
		Vector2 burstDir,
		float shotSpeed,
		int shotBaseDamage,
		int burstShots,
		float burstIntervalSeconds)
	{
		_state = State.Burst;
		_burstDir = burstDir;
		_shotSpeed = shotSpeed;
		_shotBaseDamage = Mathf.Max(1, shotBaseDamage);
		_burstShotsRemaining = Mathf.Max(0, burstShots);
		_burstInterval = Mathf.Clamp(burstIntervalSeconds, 0.01f, 0.5f);
		_burstTimer = _burstInterval;
		_fired = true;
	}

	public void Tick(
		float dt,
		bool aimAtFireMoment,
		bool aimEachBurstShot,
		System.Func<Vector2, Vector2> resolveCurrentAimDirection,
		System.Action<Vector2, float, int> fireVolley)
	{
		if (_state == State.Idle)
			return;

		if (_state == State.Windup)
		{
			_windupTimer += dt;
			float fireAt = _windupDuration * _fireAtNormalized;
			if (!_fired && _windupTimer >= fireAt)
			{
				Vector2 fireDir = aimAtFireMoment
					? resolveCurrentAimDirection(_windupAimFallbackDir)
					: _windupAimFallbackDir;
				fireVolley(fireDir, _shotSpeed, _shotBaseDamage);
				_burstDir = fireDir;
				_fired = true;

				if (_burstShotsRemaining > 0)
				{
					_state = State.Burst;
					_burstTimer = _burstInterval;
				}
			}

			if (_state == State.Windup && _windupTimer >= _windupDuration)
				_state = State.Idle;
			return;
		}

		if (_state != State.Burst)
			return;

		if (_burstShotsRemaining <= 0)
		{
			_state = State.Idle;
			return;
		}

		_burstTimer -= dt;
		if (_burstTimer > 0f)
			return;

		Vector2 dir = _burstDir;
		if (aimEachBurstShot)
			dir = resolveCurrentAimDirection(_burstDir);
		fireVolley(dir, _shotSpeed, _shotBaseDamage);
		_burstShotsRemaining--;

		if (_burstShotsRemaining > 0)
			_burstTimer = _burstInterval;
		else
			_state = State.Idle;
	}
}
