using Godot;

public abstract partial class EnemyBehaviorModule : Node
{
	[Export] public bool Active = true;

	public virtual void OnInitialized(Enemy enemy) { }

	public abstract Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta);
}
