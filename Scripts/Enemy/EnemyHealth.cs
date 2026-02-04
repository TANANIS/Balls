using Godot;

public partial class EnemyHealth : Node
{
	[Export] public int MaxHp = 3;
	[Export] public float HurtIFrame = 0.05f;

	private int _hp;
	private bool _isDead;
	private float _invincibleTimer;
	private Enemy _ownerEnemy;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	public override void _Ready()
	{
		_hp = MaxHp;
		_ownerEnemy = GetParent() as Enemy;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_invincibleTimer > 0f)
			_invincibleTimer -= (float)delta;
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f)
			return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void TakeDamage(int amount, object source)
	{
		if (_isDead || IsInvincible || amount <= 0)
			return;

		_hp -= amount;
		_ownerEnemy?.NotifyDamaged(amount, source);

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp > 0)
			return;

		_isDead = true;
		_ownerEnemy?.NotifyDeath(source);
	}
}
