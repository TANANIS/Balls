using Godot;

public partial class EventDirector
{
	private const float HybridToastDurationSeconds = 2.8f;

	private float _hybridToastRemainingSeconds;
	private string _hybridToastText = string.Empty;

	public string ActiveEventHintText { get; private set; } = string.Empty;
	public string ActiveHybridToastText => _hybridToastRemainingSeconds > 0f ? _hybridToastText : string.Empty;

	public void FillTelegraphSnapshot(EventTelegraphSnapshot snapshot)
	{
		_eventRunner?.FillTelegraphSnapshot(snapshot);
	}

	private void OnEventActivatedRuntimeUi(EventLoadoutSlot slot)
	{
		ActiveEventHintText = _eventRunner?.BuildEventHintTextForUi() ?? string.Empty;

		if (slot?.HybridVariantTriggered == true)
		{
			_hybridToastText = GetHybridToastText(slot.HybridVariantId);
			_hybridToastRemainingSeconds = HybridToastDurationSeconds;
		}
	}

	private void TickRuntimeUiTimers(float dt)
	{
		_hybridToastRemainingSeconds = Mathf.Max(0f, _hybridToastRemainingSeconds - Mathf.Max(0f, dt));
		if (_hybridToastRemainingSeconds <= 0f)
			_hybridToastText = string.Empty;
	}

	private void ClearRuntimeUiState()
	{
		ActiveEventHintText = string.Empty;
		_hybridToastText = string.Empty;
		_hybridToastRemainingSeconds = 0f;
	}

	private static string GetHybridToastText(string hybridVariantId)
	{
		return hybridVariantId switch
		{
			"HYB_ICE_SPACE_GLACIAL_HORIZON" => "HYBRID TRIGGERED: GLACIAL HORIZON",
			"HYB_SPACE_WAR_WARP_ASSAULT" => "HYBRID TRIGGERED: WARP ASSAULT",
			_ => "HYBRID TRIGGERED"
		};
	}
}
