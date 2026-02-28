using System;
using System.Collections.Generic;

public partial class GameFlowUI
{
	private void RefreshEventUnlockUi()
	{
		if (_startEventUnlockPanel == null)
			return;

		if (_startEventUnlockTitleLabel != null)
			_startEventUnlockTitleLabel.Text = TrOrDefault("UI.META.EVENT_PURCHASE.TITLE", "Order Archive: Event Purchases", "秩序檔案：事件購買");
		if (_startEventUnlockIntroLabel != null)
			_startEventUnlockIntroLabel.Text = TrOrDefault(
				"UI.META.EVENT_PURCHASE.INTRO_DOMAIN_POWER",
				"Infuse domain shards into each deity remnant to buy domain power (+3 each); each slot consumes 1 domain power.",
				"消耗神域碎片灌注神明殘骸，購買該神域力量（每次 +3）；每個災厄槽位會消耗 1 點神域力量。");
		if (_startEventUnlockEventSectionTitleLabel != null)
			_startEventUnlockEventSectionTitleLabel.Text = TrOrDefault("UI.META.EVENT_PURCHASE.EVENTS_DOMAIN", "Domain Power Purchases", "神域力量灌注");
		if (_startEventUnlockHybridSectionTitleLabel != null)
			_startEventUnlockHybridSectionTitleLabel.Text = TrOrDefault("UI.META.EVENT_UNLOCK.HYBRIDS", "Hybrid Variants", "混成變體");
		if (_startEventUnlockBackButton != null)
			_startEventUnlockBackButton.Text = TrOrDefault("UI.COMMON.BACK", "Back", "返回");
		if (_startEventUnlockContinueButton != null)
			_startEventUnlockContinueButton.Text = TrOrDefault("UI.META.EVENT_UNLOCK.NEXT_LOADOUT", "Next: Event Loadout", "下一步：事件配置");

		int ice = MetaProgressionService.Instance.GetDomainShardBalance("Ice");
		int spacetime = MetaProgressionService.Instance.GetDomainShardBalance("Spacetime");
		int war = MetaProgressionService.Instance.GetDomainShardBalance("War");
		if (_startEventUnlockWalletLabel != null)
		{
			string walletPrefix = TrOrDefault("UI.META.EVENT_UNLOCK.SHARD_WALLET", "Shard Wallet", "碎片庫存");
			string iceLabel = TrOrDefault("UI.DOMAIN.ICE", "Ice", "冰");
			string spacetimeLabel = TrOrDefault("UI.DOMAIN.SPACETIME", "Spacetime", "時空");
			string warLabel = TrOrDefault("UI.DOMAIN.WAR", "War", "戰爭");
			_startEventUnlockWalletLabel.Text = $"{walletPrefix}: {iceLabel} {ice} | {spacetimeLabel} {spacetime} | {warLabel} {war}";
		}

		RefreshUnlockEntryRows(_eventUnlockRows, isHybrid: false);
		RefreshUnlockEntryRows(_hybridUnlockRows, isHybrid: true);
		UpdateEventUnlockSectionMinHeights();
	}

	private void RefreshUnlockEntryRows(List<UnlockEntryRow> rows, bool isHybrid)
	{
		foreach (UnlockEntryRow row in rows)
		{
			if (row == null)
				continue;

			if (isHybrid)
			{
				RefreshHybridUnlockRow(row);
				continue;
			}

			RefreshDomainPowerRow(row);
		}
	}

	private void RefreshDomainPowerRow(UnlockEntryRow row)
	{
		string domainId = ProgressionDefs.NormalizeDomainId(row.EntryId);
		bool canPurchase = MetaProgressionService.Instance.CanPurchaseDomainPower(domainId, out int cost, out int bundle);
		int power = MetaProgressionService.Instance.GetDomainPowerCount(domainId);
		int balance = MetaProgressionService.Instance.GetDomainShardBalance(domainId);
		int missing = Math.Max(0, cost - balance);

		string deityLabel = GetLocalizedDeityLabel(domainId);
		string domainDesc = GetLocalizedDomainFlavor(domainId);
		string ownedEventsText = BuildDomainEventSummaryText(domainId);
		if (row.NameLabel != null)
			row.NameLabel.Text = deityLabel;
		if (row.DetailLabel != null)
			row.DetailLabel.Text = $"{domainDesc}\n{ownedEventsText}";
		if (row.CostLabel != null)
			row.CostLabel.Text = TrOrDefault("UI.META.EVENT_UNLOCK.COST_PLUS", "Cost {0} (+{1})", "花費 {0}（+{1}）")
				.Replace("{0}", cost.ToString())
				.Replace("{1}", bundle.ToString());
		if (row.StatusLabel != null)
		{
			row.StatusLabel.Text = canPurchase
					? TrOrDefault("UI.META.EVENT_UNLOCK.READY", "Ready", "可購買")
					: $"{TrOrDefault("UI.META.EVENT_UNLOCK.MISSING", "Missing", "不足")} {missing}";
			row.StatusLabel.Text += $"\n{TrOrDefault("UI.META.EVENT_UNLOCK.OWNED", "Owned x{0}", "持有 x{0}").Replace("{0}", power.ToString())}";
		}
		if (row.ActionButton != null)
		{
			row.ActionButton.Text = TrOrDefault("UI.META.EVENT_UNLOCK.INFUSE", "Infuse", "灌注");
			row.ActionButton.Disabled = !canPurchase;
		}
		if (row.DetailToggleButton != null)
		{
			bool expanded = _eventUnlockExpandedByEntryId.TryGetValue(row.EntryId, out bool isOpen) && isOpen;
			row.DetailToggleButton.Text = expanded
				? TrOrDefault("UI.META.EVENT_UNLOCK.DETAIL_HIDE", "Hide", "收合")
				: TrOrDefault("UI.META.EVENT_UNLOCK.DETAIL_SHOW", "Show", "展開");
			if (row.DetailLabel != null)
				row.DetailLabel.Visible = expanded;
		}
	}

	private void RefreshHybridUnlockRow(UnlockEntryRow row)
	{
		bool unlocked = MetaProgressionService.Instance.IsHybridVariantUnlocked(row.EntryId);
		bool canUnlock = MetaProgressionService.Instance.CanUnlockHybridVariant(row.EntryId, out string domainId, out int cost);
		domainId = ProgressionDefs.NormalizeDomainId(domainId);
		if (string.IsNullOrWhiteSpace(domainId) && ProgressionDefs.TryGetHybridVariant(row.EntryId, out HybridVariantDef variantDef))
			domainId = ProgressionDefs.NormalizeDomainId(variantDef.DomainId);

		int balance = MetaProgressionService.Instance.GetDomainShardBalance(domainId);
		int missing = Math.Max(0, cost - balance);

		string displayName = GetLocalizedHybridDisplayName(row.EntryId);
		string domainLabel = GetLocalizedDomainLabel(domainId);
		if (row.NameLabel != null)
			row.NameLabel.Text = $"{displayName} [{domainLabel}]";
		if (row.CostLabel != null)
			row.CostLabel.Text = TrOrDefault("UI.META.EVENT_UNLOCK.COST_SHORT", "Cost {0}", "花費 {0}").Replace("{0}", cost.ToString());
		if (row.StatusLabel != null)
		{
			row.StatusLabel.Text = unlocked
				? TrOrDefault("UI.META.UNLOCKED", "Unlocked", "已解鎖")
				: canUnlock
					? TrOrDefault("UI.META.EVENT_UNLOCK.READY", "Ready", "可購買")
					: $"{TrOrDefault("UI.META.EVENT_UNLOCK.MISSING", "Missing", "不足")} {missing}";
		}
		if (row.ActionButton != null)
		{
			row.ActionButton.Text = unlocked
				? TrOrDefault("UI.META.UNLOCKED", "Unlocked", "已解鎖")
				: TrOrDefault("UI.META.UNLOCK", "Unlock", "解鎖");
			row.ActionButton.Disabled = unlocked || !canUnlock;
		}
	}

	private string GetLocalizedDomainLabel(string domainId)
	{
		return domainId switch
		{
			"Ice" => TrOrDefault("UI.DOMAIN.ICE", "Ice", "冰"),
			"Spacetime" => TrOrDefault("UI.DOMAIN.SPACETIME", "Spacetime", "時空"),
			"War" => TrOrDefault("UI.DOMAIN.WAR", "War", "戰爭"),
			_ => TrOrDefault("UI.COMMON.UNKNOWN", "Unknown", "未知")
		};
	}

	private string GetLocalizedEventDisplayName(string eventId)
	{
		return eventId switch
		{
			"EVT_ICE_ICESTORM" => TrOrDefault("UI.EVENT.NAME.EVT_ICE_ICESTORM", "IceStorm", "霜暴"),
			"EVT_ICE_FROZEN_PULSE" => TrOrDefault("UI.EVENT.NAME.EVT_ICE_FROZEN_PULSE", "Frozen Pulse", "冰脈衝"),
			"EVT_SPACE_GRAVITY_WELL" => TrOrDefault("UI.EVENT.NAME.EVT_SPACE_GRAVITY_WELL", "Gravity Well", "引力井"),
			"EVT_SPACE_EVENT_HORIZON" => TrOrDefault("UI.EVENT.NAME.EVT_SPACE_EVENT_HORIZON", "Event Horizon", "事件視界"),
			"EVT_WAR_BLOOD_TIDE" => TrOrDefault("UI.EVENT.NAME.EVT_WAR_BLOOD_TIDE", "Blood Tide", "血潮"),
			"EVT_WAR_BERSERK_MARK" => TrOrDefault("UI.EVENT.NAME.EVT_WAR_BERSERK_MARK", "Berserk Mark", "狂戰印記"),
			_ => eventId
		};
	}

	private string GetLocalizedHybridDisplayName(string variantId)
	{
		return variantId switch
		{
			"HYB_ICE_SPACE_GLACIAL_HORIZON" => TrOrDefault("UI.HYBRID.NAME.HYB_ICE_SPACE_GLACIAL_HORIZON", "Glacial Horizon", "霜界視界"),
			"HYB_SPACE_WAR_WARP_ASSAULT" => TrOrDefault("UI.HYBRID.NAME.HYB_SPACE_WAR_WARP_ASSAULT", "Warp Assault", "扭曲突擊"),
			_ => variantId
		};
	}

	private string GetLocalizedDomainFlavor(string domainId)
	{
		return domainId switch
		{
			"Ice" => TrOrDefault("UI.META.EVENT_PURCHASE.DOMAIN_FLAVOR_ICE", "Slows and controls movement across the field.", "以緩速與地形控制壓縮走位空間。"),
			"Spacetime" => TrOrDefault("UI.META.EVENT_PURCHASE.DOMAIN_FLAVOR_SPACE", "Distorts space and range to disrupt positioning.", "扭曲空間與射程，打亂站位與節奏。"),
			"War" => TrOrDefault("UI.META.EVENT_PURCHASE.DOMAIN_FLAVOR_WAR", "Amplifies tempo and pressure with aggressive bursts.", "以高節奏與侵略性爆發強化戰局壓力。"),
			_ => TrOrDefault("UI.COMMON.UNKNOWN", "Unknown", "未知")
		};
	}

	private string GetLocalizedDeityLabel(string domainId)
	{
		return domainId switch
		{
			"Ice" => TrOrDefault("UI.META.EVENT_PURCHASE.DEITY_ICE", "Ice Deity", "冰神"),
			"Spacetime" => TrOrDefault("UI.META.EVENT_PURCHASE.DEITY_SPACETIME", "Spacetime Deity", "時空神"),
			"War" => TrOrDefault("UI.META.EVENT_PURCHASE.DEITY_WAR", "War Deity", "戰爭神"),
			_ => TrOrDefault("UI.COMMON.UNKNOWN", "Unknown", "未知")
		};
	}

	private string BuildDomainEventSummaryText(string domainId)
	{
		var entries = new List<string>();
		foreach ((string eventId, string eventName) in GetDomainEvents(domainId))
		{
			string eventLabel = GetLocalizedEventDisplayName(eventId);
			string eventDesc = GetLocalizedEventBriefDescription(eventId);
			entries.Add($"- {eventLabel}: {eventDesc}");
		}

		return string.Join('\n', entries);
	}

	private static IEnumerable<(string eventId, string eventName)> GetDomainEvents(string domainId)
	{
		return domainId switch
		{
			"Ice" => new[]
			{
				("EVT_ICE_ICESTORM", "IceStorm"),
				("EVT_ICE_FROZEN_PULSE", "Frozen Pulse")
			},
			"Spacetime" => new[]
			{
				("EVT_SPACE_EVENT_HORIZON", "Event Horizon"),
				("EVT_SPACE_GRAVITY_WELL", "Gravity Well")
			},
			"War" => new[]
			{
				("EVT_WAR_BLOOD_TIDE", "Blood Tide"),
				("EVT_WAR_BERSERK_MARK", "Berserk Mark")
			},
			_ => Array.Empty<(string eventId, string eventName)>()
		};
	}

	private string GetLocalizedEventBriefDescription(string eventId)
	{
		return eventId switch
		{
			"EVT_ICE_ICESTORM" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_ICE_ICESTORM", "Random ice zones slow all units and expand under distortion.", "隨機冰區減速全場單位，連續同神時覆蓋更大。"),
			"EVT_ICE_FROZEN_PULSE" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_ICE_FROZEN_PULSE", "Periodic freezing pulse rings from map center.", "地圖中心定期擴散冰脈衝，命中短暫緩速。"),
			"EVT_SPACE_EVENT_HORIZON" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_SPACE_EVENT_HORIZON", "Compression zone reduces movement and attack ranges.", "壓縮區降低移動與攻擊射程，干擾節奏。"),
			"EVT_SPACE_GRAVITY_WELL" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_SPACE_GRAVITY_WELL", "Gravity wells pull players and enemies into control points.", "引力井吸附敵我單位，形成高風險控場點。"),
			"EVT_WAR_BLOOD_TIDE" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_WAR_BLOOD_TIDE", "Directional war tide pushes elite-packed enemy waves.", "單側怪潮定向湧入，形成高壓推進線。"),
			"EVT_WAR_BERSERK_MARK" => TrOrDefault("UI.META.EVENT_PURCHASE.DESC_EVT_WAR_BERSERK_MARK", "Marked units gain high speed and attack frenzy.", "狂戰印記使敵我加速與攻速提升，失誤成本提高。"),
			_ => TrOrDefault("UI.COMMON.UNKNOWN", "Unknown", "未知")
		};
	}
}
