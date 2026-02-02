using Godot;
using System;

public partial class EnemyHitbox : Area2D
{

	[Export] public int Damage = 1;

	private Enemy _enemy;
	// Called when the node enters the scene tree for the first time.
	// public override void _Ready()
	// {
	// 	_enemy = GetParentOrNull<Enemy>();
	// 	AreaEntered += OnAreaEntered;
	// }

	// private void OnAreaEntered(Area2D area)
	// {
	// 	// 只打 PlayerHurtbox
	// 	// 建議你給 Player Hurtbox 加 group: "PlayerHurtbox"
	// 	if (!area.IsInGroup("PlayerHurtbox")) return;

	// 	if (_enemy == null) return;

	// 	// 冷卻中：不造成傷害，也不觸發退避（避免貼臉瘋狂刷新）
	// 	if (!_enemy.CanDealDamage()) return;

	// 	// 通知 Enemy：我打到了，去退開 + 進冷卻
	// 	_enemy.NotifyHitPlayer();

	// 	// 這裡不直接扣血，扣血留給 PlayerHurtbox 去做（責任分離）
	// 	// 你也可以在這裡直接扣，但會跟你的 PlayerHurtbox 重複。
	// }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
