using Godot;

public abstract partial class EnemyEventModule : Node
{
	[Export] public bool Active = true;

	public virtual void OnInitialized(Enemy enemy) { }
	public virtual void OnSpawned(Enemy enemy) { }
	public virtual void OnDamaged(Enemy enemy, int amount, object source) { }
	public virtual void OnHitPlayer(Enemy enemy, Node playerTarget) { }
	public virtual void OnDeath(Enemy enemy, object killer) { }
}
