using Godot;
using System;
using System.Collections.Generic;

public partial class RunContext : Node
{
	private const string FallbackCharacterPath = "res://Data/Characters/RangedCharacter.tres";

	public static RunContext Instance { get; private set; }

	[Export] public CharacterDefinition DefaultCharacter;

	public CharacterDefinition SelectedCharacter { get; private set; }
	public EventLoadoutPlan CurrentEventLoadoutPlan { get; private set; }

	public override void _Ready()
	{
		InputMapBootstrap.EnsureDefaultMappings();
		InputRebindService.Initialize();

		Instance = this;
		if (DefaultCharacter == null)
			DefaultCharacter = GD.Load<CharacterDefinition>(FallbackCharacterPath);
		CharacterStatsCsvService.ApplyTo(DefaultCharacter);
		if (SelectedCharacter == null)
			SelectedCharacter = DefaultCharacter;
		CharacterStatsCsvService.ApplyTo(SelectedCharacter);

		SelectedCharacter = ResolveSelectableCharacterOrDefault(SelectedCharacter);
	}

	public void SetSelectedCharacter(CharacterDefinition character)
	{
		CharacterStatsCsvService.ApplyTo(character);
		SelectedCharacter = ResolveSelectableCharacterOrDefault(character);
	}

	public CharacterDefinition GetSelectedOrDefault()
	{
		return SelectedCharacter ?? DefaultCharacter;
	}

	public void SetEventLoadoutPlan(EventLoadoutPlan plan)
	{
		CurrentEventLoadoutPlan = plan?.Clone();
	}

	public EventLoadoutPlan GetEventLoadoutPlan()
	{
		return CurrentEventLoadoutPlan?.Clone();
	}

	public void ClearEventLoadoutPlan()
	{
		CurrentEventLoadoutPlan = null;
	}

	private CharacterDefinition ResolveSelectableCharacterOrDefault(CharacterDefinition candidate)
	{
		CharacterDefinition fallback = DefaultCharacter ?? candidate;
		CharacterStatsCsvService.ApplyTo(candidate);
		CharacterStatsCsvService.ApplyTo(fallback);
		if (candidate != null && MetaProgressionService.Instance.IsCharacterUnlocked(candidate.CharacterId))
			return candidate;
		if (fallback != null && MetaProgressionService.Instance.IsCharacterUnlocked(fallback.CharacterId))
			return fallback;
		return fallback;
	}
}

public sealed class EventLoadoutPlan
{
	public List<EventLoadoutSlot> Slots { get; } = new();
	public float EstimatedTimeIntensity { get; set; }
	public int EstimatedShardReward { get; set; }
	public string Notes { get; set; } = string.Empty;

	public EventLoadoutPlan Clone()
	{
		var clone = new EventLoadoutPlan
		{
			EstimatedTimeIntensity = EstimatedTimeIntensity,
			EstimatedShardReward = EstimatedShardReward,
			Notes = Notes
		};
		foreach (EventLoadoutSlot slot in Slots)
		{
			if (slot == null)
				continue;
			clone.Slots.Add(slot.Clone());
		}

		return clone;
	}
}

public sealed class EventLoadoutSlot
{
	public int SlotIndex { get; set; }
	public int ResolvedTierIndex { get; set; }
	public string DomainId { get; set; } = string.Empty;
	public string EventId { get; set; } = string.Empty;
	public string EventName { get; set; } = string.Empty;
	public string DistortionLevel { get; set; } = "D0";
	public string AffinityWithPrevious { get; set; } = "-";
	public bool HybridVariantTriggered { get; set; }
	public string HybridVariantId { get; set; } = string.Empty;
	public bool DomainForcedByConsumable { get; set; }

	public EventLoadoutSlot Clone()
	{
		return new EventLoadoutSlot
		{
			SlotIndex = SlotIndex,
			ResolvedTierIndex = ResolvedTierIndex,
			DomainId = DomainId,
			EventId = EventId,
			EventName = EventName,
			DistortionLevel = DistortionLevel,
			AffinityWithPrevious = AffinityWithPrevious,
			HybridVariantTriggered = HybridVariantTriggered,
			HybridVariantId = HybridVariantId,
			DomainForcedByConsumable = DomainForcedByConsumable
		};
	}
}
