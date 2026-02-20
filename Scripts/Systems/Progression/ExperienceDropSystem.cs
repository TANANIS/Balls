using Godot;

public partial class ExperienceDropSystem : Node
{
	[Export] public PackedScene ExperiencePickupScene;

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
		GetTree().CurrentScene.AddChild(pickup2D);
	}
}
