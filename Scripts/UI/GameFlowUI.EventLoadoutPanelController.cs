using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class GameFlowUI
{
	private struct EventLoadoutDraftSlot
	{
		public string DomainId;
		public string EventId;
		public string EventName;
	}

	private static readonly string[] EventDomains = { "Ice", "Spacetime", "War" };
	private static readonly (string Id, string Name)[] IceEventPool =
	{
		("EVT_ICE_ICESTORM", "IceStorm"),
		("EVT_ICE_FROZEN_PULSE", "Frozen Pulse")
	};
	private static readonly (string Id, string Name)[] WarEventPool =
	{
		("EVT_WAR_BLOOD_TIDE", "Blood Tide"),
		("EVT_WAR_BERSERK_MARK", "Berserk Mark")
	};
	private static readonly (string Id, string Name)[] SpacetimeEventPool =
	{
		("EVT_SPACE_EVENT_HORIZON", "Event Horizon"),
		("EVT_SPACE_GRAVITY_WELL", "Gravity Well")
	};
	private static readonly float[] SlotBaseIntensity = { 12f, 20f, 25f, 35f };
	private const string EventLoadoutTitlePath = "Panels/StartPanel/Panel/EventLoadoutPanel/VBox/HeaderRow/Title";
	private const string EventLoadoutEstimatedIntensityLabelPath = "Panels/StartPanel/Panel/EventLoadoutPanel/VBox/EstimatedRow/EstimatedIntensityLabel";
	private const string EventLoadoutEstimatedRewardLabelPath = "Panels/StartPanel/Panel/EventLoadoutPanel/VBox/EstimatedRow/EstimatedRewardLabel";

	private GridContainer _startEventLoadoutSlotsGrid;
	private readonly Panel[] _startEventLoadoutSlotPanels = new Panel[4];
	private readonly Panel[] _startEventLoadoutIntensityPanels = new Panel[4];
	private readonly Panel[] _startEventLoadoutRewardPanels = new Panel[4];
	private readonly Label[] _startEventLoadoutTierTags = new Label[4];
	private readonly Label[] _startEventLoadoutDistortionTags = new Label[4];
	private readonly Label[] _startEventLoadoutIntensityValueLabels = new Label[4];
	private readonly Label[] _startEventLoadoutRewardValueLabels = new Label[4];
	private readonly Label[] _startEventLoadoutIconLabels = new Label[4];
	private readonly Label[] _startEventLoadoutRollFlashLabels = new Label[4];
	private readonly OptionButton[] _startEventLoadoutEventOptions = new OptionButton[4];
	private readonly bool[] _eventLoadoutDomainEditOpen = new bool[4];
	private readonly Tween[] _eventLoadoutSlotGlowTweens = new Tween[4];
	private readonly int[] _eventLoadoutRollAnimationVersion = new int[4];
	private readonly RandomNumberGenerator _eventLoadoutRng = new();
	private Label _startEventLoadoutEstimatedIntensityValueLabel;
	private Label _startEventLoadoutEstimatedRewardValueLabel;
	private float _cachedEstimatedIntensity;
	private int _cachedEstimatedReward;
	private bool _eventLoadoutDraftInitialized;
	private int _eventLoadoutActiveRollAnimations;

	private void ResolveEventLoadoutNodes()
	{
		_startEventLoadoutSlotsGrid = GetNodeOrNull<GridContainer>(StartEventLoadoutSlotsPath);
		for (int i = 0; i < 4; i++)
		{
			string slotBasePath = $"{StartEventLoadoutSlotsPath}/Slot{i + 1}/Margin/VBox";
			_startEventLoadoutSlotPanels[i] = GetNodeOrNull<Panel>($"{StartEventLoadoutSlotsPath}/Slot{i + 1}");
			_startEventLoadoutDomainOptions[i] = GetNodeOrNull<OptionButton>($"{slotBasePath}/Controls/DomainOption");
			_startEventLoadoutRollButtons[i] = GetNodeOrNull<Button>($"{slotBasePath}/Controls/RollButton");
			_startEventLoadoutResultLabels[i] = GetNodeOrNull<Label>($"{slotBasePath}/ResultLabel");
			_startEventLoadoutTierTags[i] = GetNodeOrNull<Label>($"{slotBasePath}/TopRow/TierTag");
			_startEventLoadoutDistortionTags[i] = GetNodeOrNull<Label>($"{slotBasePath}/TopRow/DistortionTag");
			_startEventLoadoutIntensityValueLabels[i] = GetNodeOrNull<Label>($"{slotBasePath}/Stats/IntensityValue");
			_startEventLoadoutRewardValueLabels[i] = GetNodeOrNull<Label>($"{slotBasePath}/Stats/RewardValue");
			_startEventLoadoutIconLabels[i] = GetNodeOrNull<Label>($"{slotBasePath}/IconLabel");
			EnsureEventLoadoutStatBoxes(slotBasePath, i);
			EnsureRollFlashLabel(i);

			EnsureEventLoadoutDomainOptionItems(_startEventLoadoutDomainOptions[i]);
			EnsureEventOptionForSlot(slotBasePath, i);
			if (_startEventLoadoutDomainOptions[i] != null)
				_startEventLoadoutDomainOptions[i].Visible = false;

			if (_startEventLoadoutRollButtons[i] != null)
			{
				_startEventLoadoutRollButtons[i].Visible = true;
				_startEventLoadoutRollButtons[i].Disabled = false;
			}
		}

		_startEventLoadoutEstimatedIntensityValueLabel =
			GetNodeOrNull<Label>("Panels/StartPanel/Panel/EventLoadoutPanel/VBox/EstimatedRow/EstimatedIntensityValue");
		_startEventLoadoutEstimatedRewardValueLabel =
			GetNodeOrNull<Label>("Panels/StartPanel/Panel/EventLoadoutPanel/VBox/EstimatedRow/EstimatedRewardValue");

		if (_startEventLoadoutRollAllButton != null)
		{
			_startEventLoadoutRollAllButton.Visible = false;
			_startEventLoadoutRollAllButton.Disabled = true;
		}

		RefreshEventLoadoutStaticTexts();
		_eventLoadoutRng.Randomize();
		UpdateEventLoadoutResponsiveLayout();
		RefreshEventLoadoutUi();
	}

	private void EnsureEventOptionForSlot(string slotBasePath, int slotIndex)
	{
		BoxContainer controls = GetNodeOrNull<BoxContainer>($"{slotBasePath}/Controls");
		if (controls == null)
			return;
		controls.AddThemeConstantOverride("separation", 4);

		OptionButton eventOption = controls.GetNodeOrNull<OptionButton>("EventOption");
		if (eventOption == null)
		{
			eventOption = new OptionButton
			{
				Name = "EventOption",
				CustomMinimumSize = new Vector2(0f, 34f),
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				FitToLongestItem = false
			};
			controls.AddChild(eventOption);
		}

		_startEventLoadoutEventOptions[slotIndex] = eventOption;
		eventOption.Visible = false;
		eventOption.Disabled = true;
	}

	private void EnsureEventLoadoutStatBoxes(string slotBasePath, int slotIndex)
	{
		VBoxContainer stats = GetNodeOrNull<VBoxContainer>($"{slotBasePath}/Stats");
		if (stats == null)
			return;

		stats.AddThemeConstantOverride("separation", 6);
		_startEventLoadoutIntensityPanels[slotIndex] = WrapStatLabelWithBox(stats, _startEventLoadoutIntensityValueLabels[slotIndex], "IntensityPanel");
		_startEventLoadoutRewardPanels[slotIndex] = WrapStatLabelWithBox(stats, _startEventLoadoutRewardValueLabels[slotIndex], "RewardPanel");
	}

	private static Panel WrapStatLabelWithBox(VBoxContainer statsRoot, Label label, string panelName)
	{
		if (statsRoot == null || label == null || string.IsNullOrWhiteSpace(panelName))
			return null;

		Panel panel = statsRoot.GetNodeOrNull<Panel>(panelName);
		if (panel == null)
		{
			panel = new Panel
			{
				Name = panelName,
				CustomMinimumSize = new Vector2(0f, 36f),
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			var boxStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.08f, 0.07f, 0.55f),
				BorderColor = new Color(0.62f, 0.49f, 0.31f, 0.35f),
				CornerRadiusTopLeft = 5,
				CornerRadiusTopRight = 5,
				CornerRadiusBottomRight = 5,
				CornerRadiusBottomLeft = 5,
				BorderWidthTop = 1,
				BorderWidthRight = 1,
				BorderWidthBottom = 1,
				BorderWidthLeft = 1
			};
			panel.AddThemeStyleboxOverride("panel", boxStyle);
			statsRoot.AddChild(panel);
		}

		MarginContainer margin = panel.GetNodeOrNull<MarginContainer>("Margin");
		if (margin == null)
		{
			margin = new MarginContainer
			{
				Name = "Margin"
			};
			margin.AnchorRight = 1f;
			margin.AnchorBottom = 1f;
			margin.GrowHorizontal = Control.GrowDirection.Both;
			margin.GrowVertical = Control.GrowDirection.Both;
			margin.AddThemeConstantOverride("margin_left", 6);
			margin.AddThemeConstantOverride("margin_top", 2);
			margin.AddThemeConstantOverride("margin_right", 6);
			margin.AddThemeConstantOverride("margin_bottom", 2);
			panel.AddChild(margin);
		}

		if (label.GetParent() != margin)
		{
			label.GetParent()?.RemoveChild(label);
			margin.AddChild(label);
		}

		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.VerticalAlignment = VerticalAlignment.Center;
		label.AutowrapMode = TextServer.AutowrapMode.Off;
		label.ClipText = true;
		return panel;
	}

	private void EnsureRollFlashLabel(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _startEventLoadoutSlotPanels.Length)
			return;

		Panel panel = _startEventLoadoutSlotPanels[slotIndex];
		if (panel == null)
			return;

		Label flash = panel.GetNodeOrNull<Label>("RollFlashLabel");
		if (flash == null)
		{
			flash = new Label
			{
				Name = "RollFlashLabel",
				Visible = false,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			flash.AddThemeFontSizeOverride("font_size", 30);
			flash.AddThemeColorOverride("font_color", new Color(1f, 0.96f, 0.84f, 0.98f));
			panel.AddChild(flash);
		}

		flash.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		flash.SizeFlagsVertical = Control.SizeFlags.Fill;
		flash.ZIndex = 20;
		_startEventLoadoutRollFlashLabels[slotIndex] = flash;
	}

	private void BindEventLoadoutSignals()
	{
		if (_startEventLoadoutBackButton != null)
			_startEventLoadoutBackButton.Pressed += OnEventLoadoutBackPressed;
		if (_startEventLoadoutStartRunButton != null)
			_startEventLoadoutStartRunButton.Pressed += OnEventLoadoutStartRunPressed;

		for (int i = 0; i < 4; i++)
		{
			int slotIndex = i;
			if (_startEventLoadoutDomainOptions[slotIndex] != null)
				_startEventLoadoutDomainOptions[slotIndex].ItemSelected += _ => OnEventLoadoutDomainChanged(slotIndex);
			if (_startEventLoadoutRollButtons[slotIndex] != null)
				_startEventLoadoutRollButtons[slotIndex].Pressed += () => OnEventLoadoutChangePressed(slotIndex);
		}
	}

	private void EnterEventLoadout()
	{
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = true;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: false, showEventLoadout: true);

		Array.Fill(_eventLoadoutDomainEditOpen, false);
		if (!_eventLoadoutDraftInitialized)
		{
			ResetLoadoutDomainFiltersToAny();
			RebuildAllEventOptions();
			_eventLoadoutDraftInitialized = true;
			PlayFullLoadoutRollAnimation();
		}

		SyncLoadoutDomainEditUi();
		UpdateEventLoadoutResponsiveLayout();
		RefreshEventLoadoutUi();
		_startEventLoadoutStartRunButton?.GrabFocus();
	}

	private void OnEventLoadoutBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startEventLoadoutOpen = false;
		if (_startEventUnlockPanel != null)
		{
			_startEventUnlockOpen = true;
			SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: false, showEventLoadout: false, showEventUnlock: true);
			_startEventUnlockContinueButton?.GrabFocus();
			return;
		}

		_startCharacterSelectOpen = true;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: true, showEventLoadout: false);
		_startCharacterConfirmButton?.GrabFocus();
	}

	private void OnEventLoadoutDomainChanged(int slotIndex)
	{
		RefreshEventLoadoutUi();
	}

	private void OnEventLoadoutChangePressed(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _eventLoadoutDomainEditOpen.Length)
			return;

		if (!_eventLoadoutDomainEditOpen[slotIndex])
		{
			_eventLoadoutDomainEditOpen[slotIndex] = true;
			SyncLoadoutDomainEditUi();
			RefreshEventLoadoutUi();
			return;
		}

		RebuildEventOptionsForSlot(slotIndex, preferCurrentEventId: string.Empty);
		_eventLoadoutDomainEditOpen[slotIndex] = false;
		PlaySingleSlotRollAnimation(slotIndex, animateAsReroll: true);
		SyncLoadoutDomainEditUi();
		RefreshEventLoadoutUi();
	}

	private void RebuildAllEventOptions()
	{
		Array.Fill(_eventLoadoutDraftSlots, default(EventLoadoutDraftSlot));
		for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
		{
			RebuildEventOptionsForSlot(i, preferCurrentEventId: string.Empty);
		}
	}

	private void RebuildEventOptionsForSlot(int slotIndex, string preferCurrentEventId)
	{
		if (slotIndex < 0 || slotIndex >= _startEventLoadoutEventOptions.Length)
			return;

		OptionButton option = _startEventLoadoutEventOptions[slotIndex];
		if (option == null)
			return;

		string domainFilter = ResolveSelectedDomainFilter(slotIndex);
		Dictionary<string, int> remainingDomainPower = BuildRemainingDomainPowerMapExcludingSlot(slotIndex);
		List<(string Id, string Name, string Domain, int Charges)> selectable = BuildSelectableEvents(domainFilter, remainingDomainPower);

		option.Clear();
		option.Disabled = selectable.Count <= 0;
		if (selectable.Count <= 0)
		{
			option.AddItem(TrOrDefault("UI.META.LOADOUT.NO_DOMAIN_POWER", "No Domain Power", "無可用神域力量"));
			_eventLoadoutDraftSlots[slotIndex] = default;
			return;
		}

		int selectedIndex = ResolvePreferredOrRandomSelectableIndex(selectable, preferCurrentEventId);
		(string selectedEventId, _, string selectedDomain, int selectedCharges) = selectable[selectedIndex];
		string selectedTitle = GetLocalizedEventDisplayName(selectedEventId);
		string selectedDomainLabel = GetLocalizedDomainLabel(selectedDomain);
		option.AddItem($"{selectedTitle} [{selectedDomainLabel}] x{selectedCharges}");
		option.SetItemMetadata(0, selectedEventId);
		option.Selected = 0;
		option.Disabled = false;
		ApplySelectedEventForSlot(slotIndex);
		option.Disabled = true;
	}

	private void ApplySelectedEventForSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _startEventLoadoutEventOptions.Length)
			return;

		OptionButton option = _startEventLoadoutEventOptions[slotIndex];
		if (option == null || option.ItemCount <= 0 || option.Disabled)
		{
			_eventLoadoutDraftSlots[slotIndex] = default;
			return;
		}

		int selected = Mathf.Clamp(option.Selected, 0, option.ItemCount - 1);
		Variant metadata = option.GetItemMetadata(selected);
		string eventId = metadata.AsString();
		if (string.IsNullOrWhiteSpace(eventId) || !TryGetEventDomainId(eventId, out string domainId))
		{
			_eventLoadoutDraftSlots[slotIndex] = default;
			return;
		}

		_eventLoadoutDraftSlots[slotIndex] = new EventLoadoutDraftSlot
		{
			DomainId = domainId,
			EventId = eventId,
			EventName = GetLocalizedEventDisplayName(eventId)
		};
	}

	private string ResolveSelectedDomainFilter(int slotIndex)
	{
		OptionButton option = _startEventLoadoutDomainOptions[slotIndex];
		if (option == null || option.Selected <= 0)
			return string.Empty;

		return option.Selected switch
		{
			1 => "Ice",
			2 => "Spacetime",
			3 => "War",
			_ => string.Empty
		};
	}

	private int ResolvePreferredOrRandomSelectableIndex(List<(string Id, string Name, string Domain, int Charges)> selectable, string preferCurrentEventId)
	{
		if (selectable == null || selectable.Count <= 0)
			return -1;

		if (!string.IsNullOrWhiteSpace(preferCurrentEventId))
		{
			for (int i = 0; i < selectable.Count; i++)
			{
				if (string.Equals(selectable[i].Id, preferCurrentEventId, StringComparison.Ordinal))
					return i;
			}
		}

		return _eventLoadoutRng.RandiRange(0, selectable.Count - 1);
	}

	private List<(string Id, string Name, string Domain, int Charges)> BuildSelectableEvents(string domainFilter, Dictionary<string, int> remainingDomainPowerByDomain = null)
	{
		var selectable = new List<(string Id, string Name, string Domain, int Charges)>();
		foreach (string domain in EventDomains)
		{
			if (!string.IsNullOrWhiteSpace(domainFilter) && !string.Equals(domainFilter, domain, StringComparison.Ordinal))
				continue;

			int remainingDomainPower = ResolveRemainingDomainPower(domain, remainingDomainPowerByDomain);
			if (remainingDomainPower <= 0)
				continue;

			foreach ((string eventId, string eventName) in GetOwnedEventPool(domain))
			{
				selectable.Add((eventId, eventName, domain, remainingDomainPower));
			}
		}

		return selectable;
	}

	private static bool TryGetEventDomainId(string eventId, out string domainId)
	{
		domainId = string.Empty;
		if (string.IsNullOrWhiteSpace(eventId))
			return false;
		if (!ProgressionDefs.TryGetEvent(eventId, out EventUnlockDef def) || def == null)
			return false;
		domainId = ProgressionDefs.NormalizeDomainId(def.DomainId);
		return !string.IsNullOrWhiteSpace(domainId);
	}

	private void OnEventLoadoutStartRunPressed()
	{
		if (!HasCompleteEventLoadout() || !ValidateEventLoadout(out _))
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}
		if (!CanConsumeSelectedDomainPower(out _))
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}
		if (!CanAffordTierDomainSigils(out _))
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}
		if (!TryConsumeSelectedDomainPower())
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}
		if (!TryConsumeTierDomainSigils())
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}

		CommitEventLoadoutPlanToRunContext();
		AudioManager.Instance?.PlaySfxUiButton();
		StartRun();
	}

	private (string Id, string Name)[] GetDomainEventPool(string domainId)
	{
		return domainId switch
		{
			"Ice" => IceEventPool,
			"War" => WarEventPool,
			"Spacetime" => SpacetimeEventPool,
			_ => Array.Empty<(string Id, string Name)>()
		};
	}

	private List<(string Id, string Name)> GetOwnedEventPool(string domainId)
	{
		var owned = new List<(string Id, string Name)>();
		foreach ((string Id, string Name) evt in GetDomainEventPool(domainId))
		{
			if (IsEventOwnedForDomainPool(evt.Id))
				owned.Add((evt.Id, evt.Name));
		}

		if (owned.Count > 0)
			return owned;

		// Fallback: keep domain playable even before talisman-based event unlock branch ships.
		owned.AddRange(GetDomainEventPool(domainId));
		return owned;
	}

	private bool IsEventOwnedForDomainPool(string eventId)
	{
		if (!ProgressionDefs.TryGetEvent(eventId, out EventUnlockDef def) || def == null)
			return false;
		if (def.IsDefaultUnlocked)
			return true;

		// Legacy compatibility: older saves may still encode unlocks in event charges.
		return MetaProgressionService.Instance.GetEventChargeCount(eventId) > 0;
	}

	private int ResolveRemainingDomainPower(string domainId, Dictionary<string, int> remainingDomainPowerByDomain)
	{
		if (remainingDomainPowerByDomain != null && remainingDomainPowerByDomain.TryGetValue(domainId, out int remaining))
			return Math.Max(0, remaining);
		return MetaProgressionService.Instance.GetDomainPowerCount(domainId);
	}

	private Dictionary<string, int> BuildRemainingDomainPowerMapExcludingSlot(int slotIndexToExclude)
	{
		var remaining = new Dictionary<string, int>(StringComparer.Ordinal)
		{
			["Ice"] = MetaProgressionService.Instance.GetDomainPowerCount("Ice"),
			["Spacetime"] = MetaProgressionService.Instance.GetDomainPowerCount("Spacetime"),
			["War"] = MetaProgressionService.Instance.GetDomainPowerCount("War")
		};

		for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
		{
			if (i == slotIndexToExclude)
				continue;
			string selectedDomain = _eventLoadoutDraftSlots[i].DomainId;
			if (string.IsNullOrWhiteSpace(selectedDomain))
				continue;
			if (!remaining.TryGetValue(selectedDomain, out int owned) || owned <= 0)
				continue;
			remaining[selectedDomain] = owned - 1;
		}

		return remaining;
	}

	private void RefreshEventLoadoutUi()
	{
		RefreshEventLoadoutStaticTexts();
		RefreshEventLoadoutDomainOptionTexts();
		SyncLoadoutDomainEditUi();
		UpdateEventLoadoutResponsiveLayout();

		int sigilT0 = MetaProgressionService.Instance.GetOrderSigilCountForTier(0);
		int sigilT1 = MetaProgressionService.Instance.GetOrderSigilCountForTier(1);
		int sigilT2 = MetaProgressionService.Instance.GetOrderSigilCountForTier(2);
		int sigilT3 = MetaProgressionService.Instance.GetOrderSigilCountForTier(3);
		int icePower = MetaProgressionService.Instance.GetDomainPowerCount("Ice");
		int spacetimePower = MetaProgressionService.Instance.GetDomainPowerCount("Spacetime");
		int warPower = MetaProgressionService.Instance.GetDomainPowerCount("War");
		if (_startEventLoadoutInventoryLabel != null)
			_startEventLoadoutInventoryLabel.Text = TrOrDefault(
				"UI.META.LOADOUT.INVENTORY_DOMAIN_POWER",
				"Order Sigils: T0 {0} | T1 {1} | T2 {2} | T3 {3} | Domain Power: Ice {4} / Space {5} / War {6}",
				"\u79e9\u5e8f\u5370\u8a18\uff1aT0 {0} | T1 {1} | T2 {2} | T3 {3} | \u795e\u57df\u529b\u91cf\uff1a\u51b0 {4} / \u6642\u7a7a {5} / \u6230\u722d {6}")
				.Replace("{0}", sigilT0.ToString())
				.Replace("{1}", sigilT1.ToString())
				.Replace("{2}", sigilT2.ToString())
				.Replace("{3}", sigilT3.ToString())
				.Replace("{4}", icePower.ToString())
				.Replace("{5}", spacetimePower.ToString())
				.Replace("{6}", warPower.ToString());

		float totalIntensity = 0f;
		float totalReward = 0f;
		var chainSb = new StringBuilder();

		for (int i = 0; i < _startEventLoadoutResultLabels.Length; i++)
		{
			Label resultLabel = _startEventLoadoutResultLabels[i];
			if (resultLabel == null)
				continue;
			if (_startEventLoadoutTierTags[i] != null)
				_startEventLoadoutTierTags[i].Text = GetLocalizedTierTag(i);

			EventLoadoutDraftSlot slot = _eventLoadoutDraftSlots[i];
			if (string.IsNullOrWhiteSpace(slot.EventId))
			{
				resultLabel.Text = TrOrDefault("UI.META.LOADOUT.SELECT_EVENT", "Select Event", "\u8acb\u9078\u64c7\u4e8b\u4ef6");
				if (_startEventLoadoutDistortionTags[i] != null)
				{
					_startEventLoadoutDistortionTags[i].Text = "--";
					_startEventLoadoutDistortionTags[i].Modulate = new Color(0.80f, 0.76f, 0.70f, 0.9f);
				}
				if (_startEventLoadoutIntensityValueLabels[i] != null)
					_startEventLoadoutIntensityValueLabels[i].Text = $"{TrOrDefault("UI.META.LOADOUT.INTENSITY_SHORT", "INT", "\u5f37\u5ea6")} 0";
				if (_startEventLoadoutRewardValueLabels[i] != null)
					_startEventLoadoutRewardValueLabels[i].Text = $"{TrOrDefault("UI.META.LOADOUT.REWARD_SHORT", "SHD", "\u788e\u7247")} 0";
				if (_startEventLoadoutIconLabels[i] != null)
				{
					_startEventLoadoutIconLabels[i].Text = "---";
					_startEventLoadoutIconLabels[i].Modulate = new Color(0.72f, 0.72f, 0.75f, 0.95f);
				}
				continue;
			}

			bool d1 = IsDistortionD1(i);
			float distortionTime = d1 ? 1.30f : 1.00f;
			float distortionReward = d1 ? 1.50f : 1.00f;

			string affinity = "-";
			float affinityTime = 1.0f;
			float affinityReward = 1.0f;
			if (i > 0)
			{
				affinity = GetAffinityRelation(_eventLoadoutDraftSlots[i - 1].DomainId, slot.DomainId);
				(affinityTime, affinityReward) = GetAffinityMultipliers(affinity);
				if (chainSb.Length > 0)
					chainSb.Append(" | ");
				chainSb.Append($"S{i}->{i + 1}: {GetLocalizedAffinityLabel(affinity)}");
			}

			float baseIntensity = GetBaseTimeIntensityForSlot(i, slot.EventId);
			float baseReward = GetBaseShardReward(slot.EventId);
			float finalIntensity = baseIntensity * distortionTime * affinityTime;
			float finalReward = baseReward * distortionReward * affinityReward;
			totalIntensity += finalIntensity;
			totalReward += finalReward;

			resultLabel.Text = GetLocalizedEventDisplayName(slot.EventId);
			resultLabel.Modulate = GetDomainColor(slot.DomainId);
			if (_startEventLoadoutDistortionTags[i] != null)
			{
				_startEventLoadoutDistortionTags[i].Text = d1 ? "D1" : "D0";
				_startEventLoadoutDistortionTags[i].Modulate = d1
					? new Color(0.98f, 0.44f, 0.38f, 1f)
					: new Color(0.83f, 0.79f, 0.71f, 0.95f);
			}
			if (_startEventLoadoutIntensityValueLabels[i] != null)
				_startEventLoadoutIntensityValueLabels[i].Text = $"{TrOrDefault("UI.META.LOADOUT.INTENSITY_SHORT", "INT", "\u5f37\u5ea6")} {Mathf.RoundToInt(finalIntensity)}";
			if (_startEventLoadoutRewardValueLabels[i] != null)
				_startEventLoadoutRewardValueLabels[i].Text = $"{TrOrDefault("UI.META.LOADOUT.REWARD_SHORT", "SHD", "\u788e\u7247")} {Mathf.RoundToInt(finalReward)}";
			if (_startEventLoadoutIconLabels[i] != null)
			{
				_startEventLoadoutIconLabels[i].Text = GetDomainGlyph(slot.DomainId);
				_startEventLoadoutIconLabels[i].Modulate = GetDomainColor(slot.DomainId);
			}
		}

		UpdateSameDomainGlowVisuals();

		if (_startEventLoadoutEstimatedIntensityValueLabel != null)
			_startEventLoadoutEstimatedIntensityValueLabel.Text = Mathf.RoundToInt(totalIntensity).ToString();
		if (_startEventLoadoutEstimatedRewardValueLabel != null)
			_startEventLoadoutEstimatedRewardValueLabel.Text = Mathf.RoundToInt(totalReward).ToString();
		_cachedEstimatedIntensity = totalIntensity;
		_cachedEstimatedReward = Mathf.RoundToInt(totalReward);

		bool complete = HasCompleteEventLoadout();
		bool valid = ValidateEventLoadout(out int invalidIndex);
		bool hasPendingDomainChange = HasPendingDomainReroll();
		bool canAffordDomainPower = CanConsumeSelectedDomainPower(out string missingChargeText);
		bool canAffordSigils = CanAffordTierDomainSigils(out string missingSigilText);
		if (_startEventLoadoutStartRunButton != null)
			_startEventLoadoutStartRunButton.Disabled = !(complete && valid && canAffordSigils && canAffordDomainPower && !hasPendingDomainChange);

		if (_startEventLoadoutSummaryLabel == null)
			return;

		var sb = new StringBuilder();
		sb.Append(TrOrDefault("UI.META.LOADOUT.RULE", "Rule: max two same-domain consecutive slots.", "\u898f\u5247\uff1a\u540c\u795e\u57df\u4e8b\u4ef6\u6700\u591a\u9023\u7e8c 2 \u683c\u3002"));
		if (!complete)
		{
			sb.Append("  ");
			sb.Append(TrOrDefault("UI.META.LOADOUT.STATUS_SELECT_ALL", "Status: select all 4 slots.", "\u72c0\u614b\uff1a\u8acb\u5148\u9078\u6eff 4 \u683c\u3002"));
		}
		else if (!valid)
		{
			sb.Append("  ");
			sb.Append(
				TrOrDefault("UI.META.LOADOUT.STATUS_INVALID", "Status: invalid chain at Slot {0}.", "\u72c0\u614b\uff1a\u7b2c {0} \u683c\u9023\u9396\u4e0d\u5408\u6cd5\u3002")
				.Replace("{0}", (invalidIndex + 1).ToString()));
		}
		else
		{
			sb.Append("  ");
			sb.Append(TrOrDefault("UI.META.LOADOUT.STATUS_READY", "Status: ready.", "\u72c0\u614b\uff1a\u53ef\u958b\u59cb\u3002"));
		}
		if (!canAffordSigils && !string.IsNullOrWhiteSpace(missingSigilText))
		{
			sb.Append('\n');
			sb.Append(TrOrDefault("UI.META.LOADOUT.MISSING", "Missing: {0}", "\u7f3a\u5c11\uff1a{0}").Replace("{0}", missingSigilText));
		}
		if (!canAffordDomainPower && !string.IsNullOrWhiteSpace(missingChargeText))
		{
			sb.Append('\n');
			sb.Append(TrOrDefault("UI.META.LOADOUT.MISSING_DOMAIN_POWER", "Missing Domain Power: {0}", "神域力量不足：{0}")
				.Replace("{0}", missingChargeText));
		}
		if (hasPendingDomainChange)
		{
			sb.Append('\n');
			sb.Append(TrOrDefault("UI.META.LOADOUT.PENDING_CHANGE", "Pending change: confirm slot reroll first.", "尚有待確認的更改，請先按下確認更改。"));
		}

		if (chainSb.Length > 0)
		{
			sb.Append('\n');
			sb.Append(chainSb);
		}

		_startEventLoadoutSummaryLabel.Text = sb.ToString();
	}

	private void PlayFullLoadoutRollAnimation()
	{
		for (int i = 0; i < _startEventLoadoutSlotPanels.Length; i++)
			PlaySingleSlotRollAnimation(i, animateAsReroll: false, delaySeconds: i * 0.08f);
	}

	private async void PlaySingleSlotRollAnimation(int slotIndex, bool animateAsReroll, float delaySeconds = 0f)
	{
		if (slotIndex < 0 || slotIndex >= _startEventLoadoutSlotPanels.Length)
			return;

		Panel panel = _startEventLoadoutSlotPanels[slotIndex];
		if (panel == null)
			return;

		Label resultLabel = _startEventLoadoutResultLabels[slotIndex];
		Label iconLabel = _startEventLoadoutIconLabels[slotIndex];
		Label flashLabel = _startEventLoadoutRollFlashLabels[slotIndex];
		int version = ++_eventLoadoutRollAnimationVersion[slotIndex];
		Vector2 startScale = animateAsReroll ? new Vector2(0.94f, 0.94f) : new Vector2(0.90f, 0.90f);
		Vector2 peakScale = animateAsReroll ? new Vector2(1.06f, 1.06f) : new Vector2(1.08f, 1.08f);

		if (delaySeconds > 0f)
			await ToSignal(GetTree().CreateTimer(delaySeconds, true), SceneTreeTimer.SignalName.Timeout);
		if (version != _eventLoadoutRollAnimationVersion[slotIndex])
			return;

		BeginLoadoutRollUiLock();
		try
		{
			panel.Scale = startScale;
			if (resultLabel != null && flashLabel != null)
				resultLabel.Visible = false;
			if (iconLabel != null)
				iconLabel.Modulate = new Color(iconLabel.Modulate.R, iconLabel.Modulate.G, iconLabel.Modulate.B, 0.25f);
			if (flashLabel != null)
			{
				float width = Mathf.Max(48f, panel.Size.X - 12f);
				float y = Mathf.Round((panel.Size.Y * 0.44f) - 28f);
				flashLabel.Position = new Vector2(6f, y);
				flashLabel.Size = new Vector2(width, 56f);
				flashLabel.Text = "???";
				flashLabel.Modulate = new Color(1f, 0.95f, 0.82f, 1f);
				flashLabel.Visible = true;
			}

			Tween tween = CreateTween();
			tween.SetPauseMode(Tween.TweenPauseMode.Process);
			tween.TweenProperty(panel, "scale", peakScale, 0.10f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(panel, "scale", Vector2.One, 0.14f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
			if (iconLabel != null)
				tween.Parallel().TweenProperty(iconLabel, "modulate:a", 1.0f, 0.22f);

			if (flashLabel != null)
			{
				List<string> rollNames = BuildRollAnimationNames(slotIndex);
				int rollSteps = animateAsReroll ? 7 : 9;
				float baseY = flashLabel.Position.Y;
				for (int step = 0; step < rollSteps; step++)
				{
					if (version != _eventLoadoutRollAnimationVersion[slotIndex])
						return;

					string token = rollNames.Count > 0
						? rollNames[_eventLoadoutRng.RandiRange(0, rollNames.Count - 1)]
						: "???";
					flashLabel.Text = token;
					flashLabel.Position = new Vector2(flashLabel.Position.X, baseY + ((step % 2 == 0) ? -8f : 8f));
					flashLabel.Modulate = new Color(1f, 0.94f, 0.80f, 0.76f);
					await ToSignal(GetTree().CreateTimer(0.038, true), SceneTreeTimer.SignalName.Timeout);
				}

				if (version != _eventLoadoutRollAnimationVersion[slotIndex])
					return;
				flashLabel.Position = new Vector2(flashLabel.Position.X, baseY);
				flashLabel.Text = GetResolvedSlotEventName(slotIndex);
				flashLabel.Modulate = new Color(1f, 0.98f, 0.88f, 1f);
				await ToSignal(GetTree().CreateTimer(0.09, true), SceneTreeTimer.SignalName.Timeout);
				flashLabel.Visible = false;
			}

			if (resultLabel != null)
			{
				resultLabel.Visible = true;
				resultLabel.Modulate = new Color(resultLabel.Modulate.R, resultLabel.Modulate.G, resultLabel.Modulate.B, 1f);
			}
		}
		finally
		{
			EndLoadoutRollUiLock();
		}
	}

	private void BeginLoadoutRollUiLock()
	{
		bool wasIdle = _eventLoadoutActiveRollAnimations <= 0;
		_eventLoadoutActiveRollAnimations++;
		if (wasIdle)
			AudioManager.Instance?.StartSfxUiSlotRollLoop();
		ApplyLoadoutRollUiLockVisuals();
	}

	private void EndLoadoutRollUiLock()
	{
		if (_eventLoadoutActiveRollAnimations <= 0)
		{
			_eventLoadoutActiveRollAnimations = 0;
			ApplyLoadoutRollUiLockVisuals();
			return;
		}

		_eventLoadoutActiveRollAnimations = Mathf.Max(0, _eventLoadoutActiveRollAnimations - 1);
		if (_eventLoadoutActiveRollAnimations == 0)
			AudioManager.Instance?.StopSfxUiSlotRollLoop(playStop: true);
		ApplyLoadoutRollUiLockVisuals();
	}

	private void ApplyLoadoutRollUiLockVisuals()
	{
		bool hide = _eventLoadoutActiveRollAnimations > 0;
		for (int i = 0; i < _startEventLoadoutIntensityPanels.Length; i++)
		{
			if (_startEventLoadoutIntensityPanels[i] != null)
				_startEventLoadoutIntensityPanels[i].Visible = !hide;
			if (_startEventLoadoutRewardPanels[i] != null)
				_startEventLoadoutRewardPanels[i].Visible = !hide;
			if (_startEventLoadoutRollButtons[i] != null)
				_startEventLoadoutRollButtons[i].Visible = !hide;
		}

		if (!hide)
			SyncLoadoutDomainEditUi();
	}

	private List<string> BuildRollAnimationNames(int slotIndex)
	{
		string domainFilter = ResolveSelectedDomainFilter(slotIndex);
		Dictionary<string, int> remaining = BuildRemainingDomainPowerMapExcludingSlot(slotIndex);
		List<(string Id, string Name, string Domain, int Charges)> selectable = BuildSelectableEvents(domainFilter, remaining);
		var names = new List<string>();
		for (int i = 0; i < selectable.Count; i++)
		{
			string name = GetLocalizedEventDisplayName(selectable[i].Id);
			if (!string.IsNullOrWhiteSpace(name))
				names.Add(name);
		}
		if (names.Count == 0)
			names.Add("???");
		return names;
	}

	private string GetResolvedSlotEventName(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _eventLoadoutDraftSlots.Length)
			return "???";
		string eventId = _eventLoadoutDraftSlots[slotIndex].EventId;
		if (string.IsNullOrWhiteSpace(eventId))
			return "???";
		return GetLocalizedEventDisplayName(eventId);
	}

	private void UpdateSameDomainGlowVisuals()
	{
		bool[] highlight = new bool[_eventLoadoutDraftSlots.Length];
		for (int i = 1; i < _eventLoadoutDraftSlots.Length; i++)
		{
			string left = _eventLoadoutDraftSlots[i - 1].DomainId;
			string right = _eventLoadoutDraftSlots[i].DomainId;
			if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
				continue;
			if (!string.Equals(left, right, StringComparison.Ordinal))
				continue;
			highlight[i - 1] = true;
			highlight[i] = true;
		}

		for (int i = 0; i < _startEventLoadoutSlotPanels.Length; i++)
		{
			Panel panel = _startEventLoadoutSlotPanels[i];
			if (panel == null)
				continue;

			if (_eventLoadoutSlotGlowTweens[i] != null && _eventLoadoutSlotGlowTweens[i].IsValid())
			{
				_eventLoadoutSlotGlowTweens[i].Kill();
				_eventLoadoutSlotGlowTweens[i] = null;
			}

			if (!highlight[i])
			{
				panel.Modulate = Colors.White;
				continue;
			}

			string domainId = _eventLoadoutDraftSlots[i].DomainId;
			Color glow = GetDomainColor(domainId).Lerp(Colors.White, 0.35f);
			panel.Modulate = Colors.White;

			Tween glowTween = CreateTween();
			glowTween.SetPauseMode(Tween.TweenPauseMode.Process);
			glowTween.SetLoops();
			glowTween.TweenProperty(panel, "modulate", glow, 0.34f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			glowTween.TweenProperty(panel, "modulate", Colors.White, 0.34f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			_eventLoadoutSlotGlowTweens[i] = glowTween;
		}
	}

	private void ResetEventLoadoutDraftState()
	{
		_eventLoadoutDraftInitialized = false;
		AudioManager.Instance?.StopSfxUiSlotRollLoop(playStop: false);
		_eventLoadoutActiveRollAnimations = 0;
		Array.Fill(_eventLoadoutDraftSlots, default(EventLoadoutDraftSlot));
		Array.Fill(_eventLoadoutDomainEditOpen, false);
		Array.Fill(_eventLoadoutRollAnimationVersion, 0);
		ResetLoadoutDomainFiltersToAny();
		for (int i = 0; i < _eventLoadoutSlotGlowTweens.Length; i++)
		{
			if (_eventLoadoutSlotGlowTweens[i] != null && _eventLoadoutSlotGlowTweens[i].IsValid())
				_eventLoadoutSlotGlowTweens[i].Kill();
			_eventLoadoutSlotGlowTweens[i] = null;
		}
		for (int i = 0; i < _startEventLoadoutSlotPanels.Length; i++)
		{
			if (_startEventLoadoutSlotPanels[i] != null)
				_startEventLoadoutSlotPanels[i].Modulate = Colors.White;
			if (_startEventLoadoutRollFlashLabels[i] != null)
				_startEventLoadoutRollFlashLabels[i].Visible = false;
		}
		ApplyLoadoutRollUiLockVisuals();
	}

	private void CommitEventLoadoutPlanToRunContext()
	{
		var plan = new EventLoadoutPlan
		{
			EstimatedTimeIntensity = _cachedEstimatedIntensity,
			EstimatedShardReward = _cachedEstimatedReward,
			Notes = TrOrDefault("UI.META.LOADOUT.NOTES", "Generated from Event Loadout UI", "\u7531\u4e8b\u4ef6\u914d\u7f6e\u4ecb\u9762\u5efa\u7acb")
		};

		for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
		{
			EventLoadoutDraftSlot draft = _eventLoadoutDraftSlots[i];
			if (string.IsNullOrWhiteSpace(draft.EventId))
				continue;
			string distortion = IsDistortionD1(i) ? "D1" : "D0";
			string affinity = i <= 0 ? "-" : GetAffinityRelation(_eventLoadoutDraftSlots[i - 1].DomainId, draft.DomainId);
			plan.Slots.Add(new EventLoadoutSlot
			{
				SlotIndex = i,
				ResolvedTierIndex = i,
				DomainId = draft.DomainId,
				EventId = draft.EventId,
				EventName = GetLocalizedEventDisplayName(draft.EventId),
				DistortionLevel = distortion,
				AffinityWithPrevious = affinity,
				DomainForcedByConsumable = IsDomainLockedByConsumable(i)
			});
		}

		RunContext.Instance?.SetEventLoadoutPlan(plan);
	}

	private static float GetBaseShardReward(string eventId)
	{
		return eventId switch
		{
			"EVT_ICE_ICESTORM" => 18f,
			"EVT_ICE_FROZEN_PULSE" => 22f,
			"EVT_WAR_BLOOD_TIDE" => 20f,
			"EVT_WAR_BERSERK_MARK" => 24f,
			"EVT_SPACE_EVENT_HORIZON" => 26f,
			"EVT_SPACE_GRAVITY_WELL" => 21f,
			_ => 18f
		};
	}

	private static float GetBaseTimeIntensityForSlot(int slotIndex, string eventId)
	{
		float baseByTier = slotIndex >= 0 && slotIndex < SlotBaseIntensity.Length ? SlotBaseIntensity[slotIndex] : 12f;
		float eventMod = eventId switch
		{
			"EVT_ICE_ICESTORM" => 1.00f,
			"EVT_ICE_FROZEN_PULSE" => 1.08f,
			"EVT_WAR_BLOOD_TIDE" => 1.04f,
			"EVT_WAR_BERSERK_MARK" => 1.12f,
			"EVT_SPACE_EVENT_HORIZON" => 1.10f,
			"EVT_SPACE_GRAVITY_WELL" => 1.02f,
			_ => 1.00f
		};

		return baseByTier * eventMod;
	}

	private bool HasCompleteEventLoadout()
	{
		for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(_eventLoadoutDraftSlots[i].EventId))
				return false;
		}

		return true;
	}

	private bool ValidateEventLoadout(out int invalidSlotIndex)
	{
		invalidSlotIndex = -1;
		Dictionary<string, int> domainCosts = BuildSelectedDomainPowerCostMap();
		foreach (KeyValuePair<string, int> pair in domainCosts)
		{
			if (MetaProgressionService.Instance.HasDomainPower(pair.Key, pair.Value))
				continue;

			for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
			{
				if (string.Equals(_eventLoadoutDraftSlots[i].DomainId, pair.Key, StringComparison.Ordinal))
				{
					invalidSlotIndex = i;
					return false;
				}
			}
		}

		for (int i = 2; i < _eventLoadoutDraftSlots.Length; i++)
		{
			string a = _eventLoadoutDraftSlots[i - 2].DomainId;
			string b = _eventLoadoutDraftSlots[i - 1].DomainId;
			string c = _eventLoadoutDraftSlots[i].DomainId;
			if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b) || string.IsNullOrWhiteSpace(c))
				continue;
			if (a == b && b == c)
			{
				invalidSlotIndex = i;
				return false;
			}
		}

		return true;
	}

	private bool IsDistortionD1(int slotIndex)
	{
		if (slotIndex <= 0 || slotIndex >= _eventLoadoutDraftSlots.Length)
			return false;
		return _eventLoadoutDraftSlots[slotIndex - 1].DomainId == _eventLoadoutDraftSlots[slotIndex].DomainId;
	}

	private static string GetAffinityRelation(string leftDomain, string rightDomain)
	{
		if (string.IsNullOrWhiteSpace(leftDomain) || string.IsNullOrWhiteSpace(rightDomain))
			return "-";
		if (leftDomain == rightDomain)
			return "Same Domain";
		if ((leftDomain == "Ice" && rightDomain == "War") || (leftDomain == "War" && rightDomain == "Ice"))
			return "Dissonance";
		if ((leftDomain == "Ice" && rightDomain == "Spacetime") || (leftDomain == "Spacetime" && rightDomain == "Ice"))
			return "Resonance";
		if ((leftDomain == "Spacetime" && rightDomain == "War") || (leftDomain == "War" && rightDomain == "Spacetime"))
			return "Resonance";
		return "Neutral";
	}

	private static (float TimeMultiplier, float RewardMultiplier) GetAffinityMultipliers(string affinity)
	{
		return affinity switch
		{
			"Resonance" => (1.20f, 1.30f),
			"Dissonance" => (0.85f, 0.80f),
			_ => (1.00f, 1.00f)
		};
	}

	private static string GetDomainGlyph(string domainId)
	{
		return domainId switch
		{
			"Ice" => "ICE",
			"War" => "WAR",
			"Spacetime" => "SPC",
			_ => "---"
		};
	}

	private static Color GetDomainColor(string domainId)
	{
		return domainId switch
		{
			"Ice" => new Color(0.56f, 0.82f, 1f, 1f),
			"War" => new Color(0.97f, 0.44f, 0.36f, 1f),
			"Spacetime" => new Color(0.76f, 0.54f, 1f, 1f),
			_ => new Color(0.9f, 0.86f, 0.78f, 1f)
		};
	}

	private bool IsDomainLockedByConsumable(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _startEventLoadoutDomainOptions.Length)
			return false;
		OptionButton option = _startEventLoadoutDomainOptions[slotIndex];
		return option != null && !_eventLoadoutDomainEditOpen[slotIndex] && option.Selected > 0;
	}

	private bool HasPendingDomainReroll()
	{
		for (int i = 0; i < _eventLoadoutDomainEditOpen.Length; i++)
		{
			if (_eventLoadoutDomainEditOpen[i])
				return true;
		}

		return false;
	}

	private bool CanAffordTierDomainSigils(out string missingText)
	{
		var missing = new List<string>();
		for (int tier = 0; tier < 4; tier++)
		{
			if (!IsDomainLockedByConsumable(tier))
				continue;
			if (!MetaProgressionService.Instance.CanSpendOrderSigilForTier(tier))
				missing.Add($"T{tier}x1");
		}

		missingText = string.Join(", ", missing);
		return missing.Count == 0;
	}

	private Dictionary<string, int> BuildSelectedDomainPowerCostMap()
	{
		var costs = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < _eventLoadoutDraftSlots.Length; i++)
		{
			string domainId = _eventLoadoutDraftSlots[i].DomainId;
			if (string.IsNullOrWhiteSpace(domainId))
				continue;
			if (costs.TryGetValue(domainId, out int existing))
				costs[domainId] = existing + 1;
			else
				costs[domainId] = 1;
		}

		return costs;
	}

	private bool CanConsumeSelectedDomainPower(out string missingText)
	{
		Dictionary<string, int> costs = BuildSelectedDomainPowerCostMap();
		var missing = new List<string>();
		foreach (KeyValuePair<string, int> pair in costs)
		{
			int owned = MetaProgressionService.Instance.GetDomainPowerCount(pair.Key);
			if (owned >= pair.Value)
				continue;
			string name = GetLocalizedDomainLabel(pair.Key);
			missing.Add($"{name} x{pair.Value - owned}");
		}

		missingText = string.Join(", ", missing);
		return missing.Count == 0;
	}

	private bool TryConsumeSelectedDomainPower()
	{
		Dictionary<string, int> costs = BuildSelectedDomainPowerCostMap();
		if (!MetaProgressionService.Instance.TryConsumeDomainPowerBatch(costs))
			return false;
		return true;
	}

	private bool TryConsumeTierDomainSigils()
	{
		if (!CanAffordTierDomainSigils(out _))
			return false;

		for (int tier = 0; tier < 4; tier++)
		{
			if (!IsDomainLockedByConsumable(tier))
				continue;
			if (!MetaProgressionService.Instance.TrySpendOrderSigilForTier(tier))
				return false;
		}

		return true;
	}

	private void RefreshEventLoadoutStaticTexts()
	{
		Label titleLabel = GetNodeOrNull<Label>(EventLoadoutTitlePath);
		if (titleLabel != null)
			titleLabel.Text = TrOrDefault("UI.META.LOADOUT.TITLE", "Arrange the Calamities", "\u5b89\u6392\u707d\u5384");

		Label estimatedIntensityLabel = GetNodeOrNull<Label>(EventLoadoutEstimatedIntensityLabelPath);
		if (estimatedIntensityLabel != null)
			estimatedIntensityLabel.Text = TrOrDefault("UI.META.LOADOUT.EST_INTENSITY", "Estimated Intensity:", "\u9810\u4f30\u5f37\u5ea6\uff1a");

		Label estimatedRewardLabel = GetNodeOrNull<Label>(EventLoadoutEstimatedRewardLabelPath);
		if (estimatedRewardLabel != null)
			estimatedRewardLabel.Text = TrOrDefault("UI.META.LOADOUT.EST_REWARD", "Estimated Shard Reward:", "\u9810\u4f30\u788e\u7247\u734e\u52f5\uff1a");

		if (_startEventLoadoutBackButton != null)
			_startEventLoadoutBackButton.Text = TrOrDefault("UI.COMMON.BACK", "Back", "\u8fd4\u56de");
		if (_startEventLoadoutStartRunButton != null)
			_startEventLoadoutStartRunButton.Text = TrOrDefault("UI.START.CONFIRM_START_RUN", "Start Run", "\u958b\u59cb\u672c\u5c40");
	}

	private void EnsureEventLoadoutDomainOptionItems(OptionButton option)
	{
		if (option == null)
			return;

		if (option.ItemCount <= 0)
		{
			option.AddItem("Any");
			option.AddItem("Ice");
			option.AddItem("Spacetime");
			option.AddItem("War");
			option.Selected = 0;
		}

		while (option.ItemCount < 4)
			option.AddItem(string.Empty);
	}

	private void ResetLoadoutDomainFiltersToAny()
	{
		for (int i = 0; i < _startEventLoadoutDomainOptions.Length; i++)
		{
			OptionButton option = _startEventLoadoutDomainOptions[i];
			if (option == null)
				continue;
			EnsureEventLoadoutDomainOptionItems(option);
			option.Selected = 0;
		}
	}

	private void SyncLoadoutDomainEditUi()
	{
		for (int i = 0; i < _startEventLoadoutDomainOptions.Length; i++)
		{
			OptionButton option = _startEventLoadoutDomainOptions[i];
			if (option != null)
			{
				option.Visible = _eventLoadoutDomainEditOpen[i];
				option.Disabled = !_eventLoadoutDomainEditOpen[i];
			}

			Button changeButton = _startEventLoadoutRollButtons[i];
			if (changeButton == null)
				continue;
			changeButton.Visible = true;
			changeButton.Disabled = false;
			changeButton.Text = _eventLoadoutDomainEditOpen[i]
				? TrOrDefault("UI.META.LOADOUT.CONFIRM_CHANGE", "Confirm Change", "確認更改")
				: TrOrDefault("UI.META.LOADOUT.CHANGE", "Change Calamity", "更改災厄");
		}
	}

	private void RefreshEventLoadoutDomainOptionTexts()
	{
		for (int i = 0; i < _startEventLoadoutDomainOptions.Length; i++)
		{
			OptionButton option = _startEventLoadoutDomainOptions[i];
			if (option == null)
				continue;

			EnsureEventLoadoutDomainOptionItems(option);
			option.SetItemText(0, TrOrDefault("UI.META.LOADOUT.ANY", "Any", "\u4efb\u610f"));
			option.SetItemText(1, GetLocalizedDomainLabel("Ice"));
			option.SetItemText(2, GetLocalizedDomainLabel("Spacetime"));
			option.SetItemText(3, GetLocalizedDomainLabel("War"));
		}
	}

	private string GetLocalizedTierTag(int slotIndex)
	{
		return slotIndex switch
		{
			0 => TrOrDefault("UI.META.LOADOUT.TIER0", "TIER 0", "\u7b2c 0 \u968e"),
			1 => TrOrDefault("UI.META.LOADOUT.TIER1", "TIER 1", "\u7b2c 1 \u968e"),
			2 => TrOrDefault("UI.META.LOADOUT.TIER2", "TIER 2", "\u7b2c 2 \u968e"),
			_ => TrOrDefault("UI.META.LOADOUT.TIER3", "TIER 3", "\u7b2c 3 \u968e")
		};
	}

	private string GetLocalizedAffinityLabel(string affinity)
	{
		return affinity switch
		{
			"Same Domain" => TrOrDefault("UI.META.LOADOUT.AFFINITY_SAME", "Same Domain", "\u540c\u795e\u57df"),
			"Resonance" => TrOrDefault("UI.META.LOADOUT.AFFINITY_RESONANCE", "Resonance", "\u5171\u9cf4"),
			"Dissonance" => TrOrDefault("UI.META.LOADOUT.AFFINITY_DISSONANCE", "Dissonance", "\u5931\u8ae7"),
			"Neutral" => TrOrDefault("UI.META.LOADOUT.AFFINITY_NEUTRAL", "Neutral", "\u4e2d\u7acb"),
			_ => affinity
		};
	}

	private void UpdateEventLoadoutResponsiveLayout()
	{
		if (_startEventLoadoutSlotsGrid == null)
			return;
		float width = _startEventLoadoutSlotsGrid.Size.X;
		int columns = width >= 700f ? 4 : width >= 430f ? 2 : 1;
		if (_startEventLoadoutSlotsGrid.Columns != columns)
			_startEventLoadoutSlotsGrid.Columns = columns;

		int resultFont = columns == 4 ? 18 : columns == 2 ? 24 : 26;
		int statFont = columns == 4 ? 14 : 18;
		int tagFont = columns == 4 ? 12 : 14;
		for (int i = 0; i < 4; i++)
		{
			_startEventLoadoutResultLabels[i]?.AddThemeFontSizeOverride("font_size", resultFont);
			_startEventLoadoutIntensityValueLabels[i]?.AddThemeFontSizeOverride("font_size", statFont);
			_startEventLoadoutRewardValueLabels[i]?.AddThemeFontSizeOverride("font_size", statFont);
			_startEventLoadoutTierTags[i]?.AddThemeFontSizeOverride("font_size", tagFont);
			_startEventLoadoutDistortionTags[i]?.AddThemeFontSizeOverride("font_size", tagFont);
			if (_startEventLoadoutEventOptions[i] != null)
				_startEventLoadoutEventOptions[i].CustomMinimumSize = new Vector2(columns == 4 ? 0f : 120f, 34f);
		}
	}
}
