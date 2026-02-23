using System;

public sealed class PerfectClearRecord
{
	public int Score { get; set; }
	public long UnixTime { get; set; }
	public string CharacterName { get; set; } = string.Empty;

	public void Normalize()
	{
		Score = Math.Max(0, Score);
		UnixTime = Math.Max(0L, UnixTime);
		CharacterName = string.IsNullOrWhiteSpace(CharacterName) ? "Unknown" : CharacterName.Trim();
	}
}
