using Godot;

public partial class EnemyDebugEventModule : EnemyEventModule
{
	[Export] public string Prefix = "[EnemyEvent]";

	private string EnemyName(Enemy enemy) => enemy != null ? enemy.Name : "UnknownEnemy";

	public override void OnSpawned(Enemy enemy)
	{
		DebugSystem.Log($"{Prefix} Spawned -> {EnemyName(enemy)}");
	}

	public override void OnDamaged(Enemy enemy, int amount, object source)
	{
		string sourceName = source is Node node ? node.Name : "UnknownSource";
		DebugSystem.Log($"{Prefix} Damaged -> {EnemyName(enemy)}");
		DebugSystem.Log($"{Prefix} Damage={amount}, Source={sourceName}");
	}

	public override void OnHitPlayer(Enemy enemy, Node playerTarget)
	{
		string targetName = playerTarget != null ? playerTarget.Name : "UnknownTarget";
		DebugSystem.Log($"{Prefix} HitPlayer -> {EnemyName(enemy)} -> {targetName}");
	}

	public override void OnDeath(Enemy enemy, object killer)
	{
		string killerName = killer is Node node ? node.Name : "UnknownKiller";
		DebugSystem.Log($"{Prefix} Death -> {EnemyName(enemy)}, Killer={killerName}");
	}
}
