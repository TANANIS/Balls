using Godot;

/*
 * Bullet sensor:
 * - Moves forward for a limited lifetime.
 * - On first valid hit, submits DamageRequest to CombatSystem.
 * - Never applies damage directly.
 */
public partial class Bullet : Area2D
{
	[Export] public float LifeTime = 1.5f;
	[Export] public string DamageTag = "bullet";

	private Vector2 _dir = Vector2.Right;
	private float _speed = 900f;
	private int _damage = 1;
	private Node _source;
	private float _lifeTimer = 0f;
	private bool _hasHit = false;
	private CombatSystem _combat;

	public void InitFromPlayer(Node source, Vector2 dir, float speed, int damage)
	{
		_source = source;
		_dir = dir == Vector2.Zero ? Vector2.Right : dir.Normalized();
		_speed = speed;
		_damage = damage;
	}

	public override void _Ready()
	{
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		_lifeTimer += dt;
		if (_lifeTimer >= LifeTime)
		{
			QueueFree();
			return;
		}

		GlobalPosition += _dir * _speed * dt;
	}
}
