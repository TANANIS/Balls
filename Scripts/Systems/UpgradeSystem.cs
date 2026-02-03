using Godot;
using System;

public enum UpgradeId
{
	PrimaryDamageUp,
	PrimaryFasterFire,
	PrimaryProjectileSpeedUp,
	SecondaryDamageUp,
	SecondaryRangeUp,
	SecondaryWiderArc,
	SecondaryFaster,
	DashFasterCooldown,
	DashSpeedUp,
	DashLonger
}

public partial class UpgradeSystem : Node
{
	[Export] public NodePath PlayerPath = new NodePath("../../Player");

	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;
	private PlayerDash _dash;

	public override void _Ready()
	{
		var player = GetNodeOrNull<Player>(PlayerPath);
		if (player == null)
		{
			DebugSystem.Error("[UpgradeSystem] Player not found.");
			return;
		}

		_primaryAttack = player.GetNodeOrNull<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = player.GetNodeOrNull<PlayerMelee>("SecondaryAttack");
		_dash = player.GetNodeOrNull<PlayerDash>("Dash");
	}

	public void ApplyUpgrade(UpgradeId id)
	{
		switch (id)
		{
			case UpgradeId.PrimaryDamageUp:
				_primaryAttack?.AddDamage(1);
				break;
			case UpgradeId.PrimaryFasterFire:
				_primaryAttack?.MultiplyCooldown(0.88f);
				break;
			case UpgradeId.PrimaryProjectileSpeedUp:
				_primaryAttack?.AddProjectileSpeed(120f);
				break;
			case UpgradeId.SecondaryDamageUp:
				_secondaryAttack?.AddDamage(1);
				break;
			case UpgradeId.SecondaryRangeUp:
				_secondaryAttack?.AddRange(10f);
				break;
			case UpgradeId.SecondaryWiderArc:
				_secondaryAttack?.AddArcDegrees(15f);
				break;
			case UpgradeId.SecondaryFaster:
				_secondaryAttack?.MultiplyCooldown(0.88f);
				break;
			case UpgradeId.DashFasterCooldown:
				_dash?.MultiplyCooldown(0.88f);
				break;
			case UpgradeId.DashSpeedUp:
				_dash?.AddSpeed(90f);
				break;
			case UpgradeId.DashLonger:
				_dash?.AddDuration(0.03f);
				break;
		}

		DebugSystem.Log("[UpgradeSystem] Applied upgrade: " + id);
	}
}
