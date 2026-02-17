using Godot;
using System;

public partial class UpgradeSystem : Node
{
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public UpgradeCatalog Catalog;

	// Cached player modules that receive upgrade effects.
	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;
	private PlayerDash _dash;
	private PlayerHealth _playerHealth;
	private int _appliedUpgradeCount = 0;

	public int AppliedUpgradeCount => _appliedUpgradeCount;

	public override void _EnterTree()
	{
		AddToGroup("UpgradeSystem");
	}

	public override void _Ready()
	{
		// Resolve player and cache all upgrade targets once.
		var player = GetNodeOrNull<Player>(PlayerPath);
		if (player == null)
		{
			DebugSystem.Error("[UpgradeSystem] Player not found.");
			return;
		}

		_primaryAttack = player.GetNodeOrNull<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = player.GetNodeOrNull<PlayerMelee>("SecondaryAttack");
		_dash = player.GetNodeOrNull<PlayerDash>("Dash");
		_playerHealth = player.GetNodeOrNull<PlayerHealth>("Health");
	}

	public void ApplyUpgrade(UpgradeId id)
	{
		// One place where all numeric gameplay mutations are applied.
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
			case UpgradeId.MaxHpUp:
				_playerHealth?.AddMaxHp(1);
				break;
		}

		_appliedUpgradeCount++;
		DebugSystem.Log("[UpgradeSystem] Applied upgrade: " + id);
		DebugSystem.Log("[UpgradeSystem] Applied count: " + _appliedUpgradeCount);
	}
}
