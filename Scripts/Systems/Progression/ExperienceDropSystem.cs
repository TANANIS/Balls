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

	public override void _EnterTree()
	{
		AddToGroup("ExperienceDropSystem");
	}

	public override void _Ready()
	{
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combatSystem = list[0] as CombatSystem;

		if (_combatSystem != null)
			_combatSystem.EnemyKilled += OnEnemyKilled;
	}

	public override void _ExitTree()
	{
		if (_combatSystem != null)
			_combatSystem.EnemyKilled -= OnEnemyKilled;
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

		Node pickup = ExperiencePickupScene.Instantiate();
		if (pickup is not Node2D pickup2D)
			return;

		pickup2D.GlobalPosition = enemy2D.GlobalPosition;
		if (pickup is ExperiencePickup expPickup)
			expPickup.ExperienceValue = ResolveExperienceValue(enemy);
		GetTree().CurrentScene.AddChild(pickup2D);
	}

	private int ResolveExperienceValue(Node enemy)
	{
		if (enemy == null)
			return Mathf.Max(1, SwarmExperience);

		string scenePath = string.Empty;
		if (enemy is Node enemyNode)
			scenePath = enemyNode.SceneFilePath?.ToLowerInvariant() ?? string.Empty;
		string name = enemy.Name?.ToString().ToLowerInvariant() ?? string.Empty;

		if (scenePath.Contains("minibosshex") || name.Contains("miniboss"))
			return Mathf.Max(1, MiniBossExperience);
		if (scenePath.Contains("eliteswarmcircle") || name.Contains("elite"))
			return Mathf.Max(1, EliteExperience);
		if (scenePath.Contains("tanksquare") || name.Contains("tank"))
			return Mathf.Max(1, TankExperience);
		if (scenePath.Contains("chargertriangle") || name.Contains("charger"))
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
}
