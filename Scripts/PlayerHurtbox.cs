using Godot;
using System;


public partial class PlayerHurtbox : Area2D
{
	[Export] public int EnemyHitboxLayerBit = 7; // 你的 EnemyHitbox 在 layer_7
	private Player _player;

	private bool _damageEnabled = true;
	private uint _maskBeforeDisable;

	public override void _Ready()
	{
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

	/// <summary>
	/// 允許/禁止受傷（例如 Dash i-frame）
	/// </summary>
	public void SetDamageEnabled(bool enabled)
	{
		if (_damageEnabled == enabled) return;
		_damageEnabled = enabled;

		if (!enabled)
		{
			_maskBeforeDisable = CollisionMask;
			// 只關掉 EnemyHitbox 的 mask bit，其他偵測保留
			SetCollisionMaskValue(EnemyHitboxLayerBit, false);
		}
		else
		{
			CollisionMask = _maskBeforeDisable;
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (!_damageEnabled) return;

		// 只吃 EnemyHitbox
		if (area is EnemyHitbox hitbox)
		{
			_player.TryTakeDamage(hitbox.Damage, hitbox.GlobalPosition);
		}
	}
}
