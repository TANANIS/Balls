public sealed class PlayerStateMachine
{
	public enum State
	{
		Idle,
		Move,
		Attack,
		Hurt,
		Dash,
		Death
	}

	private State _current = State.Idle;

	public State Current => _current;

	public void Reset()
	{
		_current = State.Idle;
	}

	public void Force(State state)
	{
		_current = state;
	}

	public State Evaluate(
		bool isDeathLocked,
		bool hasHurt,
		bool isDashActive,
		bool hasAttack,
		bool hasMoveInput)
	{
		if (isDeathLocked)
			return _current = State.Death;
		if (hasHurt)
			return _current = State.Hurt;
		if (isDashActive)
			return _current = State.Dash;
		if (hasAttack)
			return _current = State.Attack;
		if (hasMoveInput)
			return _current = State.Move;
		return _current = State.Idle;
	}
}
