public sealed class GameFlowUiSharedState
{
	public GameFlowUiSettingsModel Settings { get; } = new();
	public CharacterDefinition SelectedCharacterDefinition { get; set; }
	public EventLoadoutDraftModel EventLoadoutDraft { get; } = new();
}

public sealed class GameFlowUiSettingsModel
{
	public float BgmPercent { get; set; } = 50f;
	public float SfxPercent { get; set; } = 80f;
	public int WindowModeIndex { get; set; }
	public int WindowSizeIndex { get; set; }
	public int LanguageIndex { get; set; }
	public string Locale { get; set; } = "en";
	public bool AutoAimEnabled { get; set; } = true;
}

public sealed class EventLoadoutDraftModel
{
	private readonly EventLoadoutDraftSlotModel[] _slots =
	{
		new(),
		new(),
		new(),
		new()
	};

	public EventLoadoutDraftSlotModel GetSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= _slots.Length)
			return null;
		return _slots[slotIndex];
	}

	public void SetSlot(int slotIndex, string domainId, string eventId, string eventName)
	{
		EventLoadoutDraftSlotModel slot = GetSlot(slotIndex);
		if (slot == null)
			return;

		slot.DomainId = domainId ?? string.Empty;
		slot.EventId = eventId ?? string.Empty;
		slot.EventName = eventName ?? string.Empty;
	}

	public void Clear()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].DomainId = string.Empty;
			_slots[i].EventId = string.Empty;
			_slots[i].EventName = string.Empty;
		}
	}
}

public sealed class EventLoadoutDraftSlotModel
{
	public string DomainId { get; set; } = string.Empty;
	public string EventId { get; set; } = string.Empty;
	public string EventName { get; set; } = string.Empty;
}
