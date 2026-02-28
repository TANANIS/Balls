using Godot;

public partial class ExperienceDropSystem : Node
{
	[Export] public PackedScene ExperiencePickupScene;
	[Export] public int SwarmExperience = 1;
	[Export] public int ChargerExperience = 2;
	[Export] public int TankExperience = 3;
	[Export] public int EliteExperience = 5;
	[Export] public int MiniBossExperience = 10;

	private CombatSystem _combatSystem;
	private bool _boundToCombat;

	public override void _EnterTree()
	{
		AddToGroup(RuntimeGroups.ExperienceDropSystem);
	}

	public override void _Ready()
	{
		TryBindCombatSystem();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_boundToCombat)
			TryBindCombatSystem();
	}

	public override void _ExitTree()
	{
		if (_boundToCombat && _combatSystem != null)
			_combatSystem.EnemyKilled -= OnEnemyKilled;
		_boundToCombat = false;
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (ExperiencePickupScene == null || target == null)
			return;
		if (source is not Player)
			return;
		if (target is not EnemyHurtbox hurtbox)
			return;

		Node enemy = hurtbox.GetParent();
		if (enemy is not Node2D enemy2D)
			return;

		Vector2 spawnPos = enemy2D.GlobalPosition;
		int experienceValue = ResolveExperienceValue(enemy);
		CallDeferred(nameof(SpawnPickupDeferred), spawnPos, experienceValue);
	}

	private void SpawnPickupDeferred(Vector2 spawnPos, int experienceValue)
	{
		if (ExperiencePickupScene == null)
			return;

		Node pickup = ExperiencePickupScene.Instantiate();
		if (pickup is not Node2D pickup2D)
		{
			pickup?.QueueFree();
			return;
		}

		pickup2D.GlobalPosition = spawnPos;
		if (pickup is ExperiencePickup expPickup)
			expPickup.ExperienceValue = Mathf.Max(1, experienceValue);

		Node root = GetTree()?.CurrentScene;
		if (root == null)
		{
			pickup2D.QueueFree();
			return;
		}

		root.AddChild(pickup2D);
	}

	private int ResolveExperienceValue(Node enemy)
	{
		if (enemy == null)
			return Mathf.Max(1, SwarmExperience);

		string scenePath = string.Empty;
		if (enemy is Node enemyNode)
			scenePath = enemyNode.SceneFilePath?.ToLowerInvariant() ?? string.Empty;
		string name = enemy.Name?.ToString().ToLowerInvariant() ?? string.Empty;

		if (EnemyTagRules.IsMiniBoss(name, scenePath))
			return Mathf.Max(1, MiniBossExperience);
		if (scenePath.Contains("werebear") || name.Contains("elite") || name.Contains("werebear"))
			return Mathf.Max(1, EliteExperience);
		if (scenePath.Contains("eliteorc") || name.Contains("tank"))
			return Mathf.Max(1, TankExperience);
		if (scenePath.Contains("orc") || name.Contains("charger") || name.Contains("orc"))
			return Mathf.Max(1, ChargerExperience);

		if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health)
		{
			if (health.MaxHp >= 100)
				return Mathf.Max(1, MiniBossExperience);
			if (health.MaxHp >= 22)
				return Mathf.Max(1, TankExperience);
			if (health.MaxHp >= 16)
				return Mathf.Max(1, EliteExperience);
			if (health.MaxHp >= 10)
				return Mathf.Max(1, ChargerExperience);
		}

		return Mathf.Max(1, SwarmExperience);
	}

	private void TryBindCombatSystem()
	{
		if (_boundToCombat)
			return;

		var list = GetTree().GetNodesInGroup(RuntimeGroups.CombatSystem);
		if (list.Count > 0)
			_combatSystem = list[0] as CombatSystem;
		if (_combatSystem == null)
			return;

		_combatSystem.EnemyKilled += OnEnemyKilled;
		_boundToCombat = true;
	}
}
