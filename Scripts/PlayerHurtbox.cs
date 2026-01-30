using Godot;
using System;


public partial class PlayerHurtbox : Area2D
{
	private Player _player;

	public override void _Ready()
	{
		// 往父節點一路找 Player（比 NodePath 省心，少一堆設定錯誤）
		_player = FindParentPlayer();
		if (_player == null)
		{
			GD.PrintErr("[PlayerHurtbox] Cannot find Player in parents. Please make Hurtbox a child of Player.");
			return;
		}

		AreaEntered += OnAreaEntered;
	}

	private Player FindParentPlayer()
	{
		Node n = GetParent();
		while (n != null)
		{
			if (n is Player p) return p;
			n = n.GetParent();
		}
		return null;
	}

	private void OnAreaEntered(Area2D area)
	{
		// 只吃 EnemyHitbox（不要用名字字串判斷，會害你以後改名直接爆炸）
		if (area is EnemyHitbox hitbox)
		{
			_player.TryTakeDamage(hitbox.Damage, hitbox.GlobalPosition);
		}
	}
}
