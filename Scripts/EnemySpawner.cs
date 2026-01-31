using Godot;
using System;

public partial class EnemySpawner : Node
{

	[Export] public PackedScene EnemyScene;
	[Export] public float SpawnInterval = 1.0f;
	[Export] public float SpawnRadius = 520f; // 離玩家多遠生成（大概螢幕外）


	private float _timer;
	private Node2D _player;
	private Node _enemiesContainer;

	public override void _Ready()
	{
		_player = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");

		_enemiesContainer = GetTree().CurrentScene.GetNodeOrNull("Enemies") ?? GetTree().CurrentScene;

		if (_player == null) GD.PrintErr("Spawner: Player not found (name should be 'Player').");
		if (EnemyScene == null) GD.PrintErr("Spawner: EnemyScene is NULL. Assign Enemy.tscn in Inspector.");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null || EnemyScene == null) return;

		_timer += (float)delta;
		if (_timer < SpawnInterval) return;
		_timer = 0f;

		SpawnOne();
	}

	private void SpawnOne()
	{
		//GD.Print("[Spawner] SpawnOne called");

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.Target = _player;



		float angle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 offset = new(Mathf.Cos(angle), Mathf.Sin(angle));
		offset *= SpawnRadius;

		enemy.GlobalPosition = _player.GlobalPosition + offset;

		//GD.Print($"[Spawn] player={_player.GlobalPosition} enemySpawn={enemy.GlobalPosition} dist={(enemy.GlobalPosition - _player.GlobalPosition).Length()}");

		_enemiesContainer.AddChild(enemy);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
