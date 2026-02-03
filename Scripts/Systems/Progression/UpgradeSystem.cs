using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeSystem : Node
{
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public UpgradeCatalog Catalog;

	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;
	private PlayerDash _dash;

	private static readonly List<UpgradeOptionData> FallbackOptions = new()
	{
		new UpgradeOptionData(UpgradeId.PrimaryDamageUp, "主武器傷害提升", "遠距射擊傷害 +1"),
		new UpgradeOptionData(UpgradeId.PrimaryFasterFire, "主武器加速", "遠距射擊冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.PrimaryProjectileSpeedUp, "主武器彈速提升", "子彈速度 +120"),
		new UpgradeOptionData(UpgradeId.SecondaryDamageUp, "近戰傷害提升", "近戰傷害 +1"),
		new UpgradeOptionData(UpgradeId.SecondaryRangeUp, "近戰範圍提升", "近戰範圍 +10"),
		new UpgradeOptionData(UpgradeId.SecondaryWiderArc, "近戰角度擴張", "近戰扇形角度 +15°"),
		new UpgradeOptionData(UpgradeId.SecondaryFaster, "近戰加速", "近戰冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.DashFasterCooldown, "衝刺加速", "衝刺冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.DashSpeedUp, "衝刺速度提升", "衝刺速度 +90"),
		new UpgradeOptionData(UpgradeId.DashLonger, "衝刺延長", "衝刺持續時間 +0.03 秒")
	};

	public override void _EnterTree()
	{
		AddToGroup("UpgradeSystem");
	}

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

	public bool TryPickTwo(RandomNumberGenerator rng, out UpgradeOptionData left, out UpgradeOptionData right)
	{
		var options = BuildOptionPool();
		if (options.Count < 2)
		{
			left = default;
			right = default;
			return false;
		}

		int leftIdx = rng.RandiRange(0, options.Count - 1);
		int rightIdx = rng.RandiRange(0, options.Count - 1);
		while (rightIdx == leftIdx)
			rightIdx = rng.RandiRange(0, options.Count - 1);

		left = options[leftIdx];
		right = options[rightIdx];
		return true;
	}

	private List<UpgradeOptionData> BuildOptionPool()
	{
		var pool = new List<UpgradeOptionData>();

		if (Catalog != null && Catalog.Entries != null)
		{
			foreach (var entry in Catalog.Entries)
			{
				if (entry == null)
					continue;
				if (string.IsNullOrWhiteSpace(entry.Title))
					continue;

				pool.Add(new UpgradeOptionData(entry.Id, entry.Title, entry.Description, entry.Icon));
			}
		}

		if (pool.Count == 0)
		{
			DebugSystem.Warn("[UpgradeSystem] Catalog missing/empty. Using fallback options.");
			pool.AddRange(FallbackOptions);
		}

		return pool;
	}

	public readonly struct UpgradeOptionData
	{
		public readonly UpgradeId Id;
		public readonly string Title;
		public readonly string Description;
		public readonly Texture2D Icon;

		public UpgradeOptionData(UpgradeId id, string title, string description, Texture2D icon = null)
		{
			Id = id;
			Title = title;
			Description = description;
			Icon = icon;
		}
	}
}
