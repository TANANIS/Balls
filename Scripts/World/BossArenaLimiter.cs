using Godot;

public partial class BossArenaLimiter : Node2D
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public bool Enabled = true;
	[Export] public float InitialRadius = 1720f;
	[Export] public float FinalRadius = 1360f;
	[Export] public float ShrinkDurationSeconds = 22f;
	[Export] public float BorderWidth = 14f;
	[Export] public Color BorderColor = new Color(0.98f, 0.22f, 0.22f, 0.95f);
	[Export] public float EdgeRepelDistance = 56f;
	[Export] public float EdgeRepelSpeed = 620f;

	private Player _player;
	private Node2D _enemiesRoot;
	private Enemy _activeBoss;
	private bool _arenaActive;
	private Vector2 _center = Vector2.Zero;
	private float _radius;
	private float _shrinkTimer;

	public override void _Ready()
	{
		ZIndex = 120;
		Visible = false;
		ResolveRefs();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled || GetTree().Paused)
			return;

		ResolveRefs();
		RefreshBossState();
		if (!_arenaActive)
			return;

		float dt = (float)delta;
		_shrinkTimer += dt;
		float duration = Mathf.Max(0.1f, ShrinkDurationSeconds);
		float t = Mathf.Clamp(_shrinkTimer / duration, 0f, 1f);
		_radius = Mathf.Lerp(Mathf.Max(40f, InitialRadius), Mathf.Max(40f, FinalRadius), t);

		ConstrainPlayerInsideArena();
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!_arenaActive)
			return;

		DrawArc(_center, _radius, 0f, Mathf.Tau, 96, BorderColor, Mathf.Max(1f, BorderWidth));
	}

	private void ResolveRefs()
	{
		if (!IsInstanceValid(_player))
			_player = GetNodeOrNull<Player>(PlayerPath);
		if (!IsInstanceValid(_enemiesRoot))
			_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
	}

	private void RefreshBossState()
	{
		if (!IsBossAlive(_activeBoss))
			_activeBoss = FindActiveMiniBoss();

		if (IsBossAlive(_activeBoss))
		{
			if (_arenaActive)
				return;

			ActivateArena();
			return;
		}

		if (_arenaActive)
			DeactivateArena();
	}

	private void ActivateArena()
	{
		if (!IsInstanceValid(_player))
			return;

		_arenaActive = true;
		Visible = true;
		_shrinkTimer = 0f;
		_radius = Mathf.Max(40f, InitialRadius);
		_center = _player.GlobalPosition;
	}

	private void DeactivateArena()
	{
		_arenaActive = false;
		Visible = false;
		_radius = 0f;
		_shrinkTimer = 0f;
		_activeBoss = null;
	}

	private void ConstrainPlayerInsideArena()
	{
		if (!IsInstanceValid(_player))
			return;

		Vector2 delta = _player.GlobalPosition - _center;
		float dist = delta.Length();
		if (dist <= 0.001f)
			return;

		Vector2 outward = delta / dist;
		float repelBand = Mathf.Clamp(EdgeRepelDistance, 0f, _radius * 0.8f);
		float innerSafeRadius = Mathf.Max(0f, _radius - repelBand);

		if (dist > _radius)
		{
			// Hard clamp outside positions and kick the player back inward.
			_player.GlobalPosition = _center + outward * _radius;
			_player.Velocity = -outward * Mathf.Max(120f, EdgeRepelSpeed);
			return;
		}

		if (dist <= innerSafeRadius || repelBand <= 0.001f)
			return;

		// Soft repel near edge so the player feels pushed away from the boundary.
		float edgeT = Mathf.Clamp((dist - innerSafeRadius) / repelBand, 0f, 1f);
		float push = Mathf.Lerp(90f, Mathf.Max(120f, EdgeRepelSpeed), edgeT);
		Vector2 inwardVel = -outward * push;
		float outwardSpeed = Mathf.Max(0f, _player.Velocity.Dot(outward));
		if (outwardSpeed > 0f)
			_player.Velocity = _player.Velocity - (outward * outwardSpeed);
		_player.Velocity = _player.Velocity.Lerp(inwardVel, 0.35f);
	}

	private Enemy FindActiveMiniBoss()
	{
		if (!IsInstanceValid(_enemiesRoot))
			return null;

		foreach (Node child in _enemiesRoot.GetChildren())
		{
			if (child is not Enemy enemy)
				continue;
			if (!IsBossTag(enemy))
				continue;
			if (!IsBossAlive(enemy))
				continue;
			return enemy;
		}

		return null;
	}

	private static bool IsBossTag(Enemy enemy)
	{
		if (enemy == null)
			return false;

		string name = enemy.Name.ToString().ToLowerInvariant();
		if (name.Contains("minibosshex") || name.Contains("miniboss"))
			return true;

		string path = enemy.SceneFilePath?.ToLowerInvariant() ?? string.Empty;
		return path.Contains("minibosshex");
	}

	private static bool IsBossAlive(Enemy enemy)
	{
		if (!IsInstanceValid(enemy))
			return false;
		if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
			return false;
		return true;
	}

}
