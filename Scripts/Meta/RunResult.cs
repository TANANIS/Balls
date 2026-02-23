using System;

public sealed class RunResult
{
	public string RunId { get; set; } = Guid.NewGuid().ToString("N");
	public int Score { get; set; }
	public string CharacterId { get; set; } = string.Empty;
	public bool IsPerfectClear { get; set; }
	public bool IsFirstClearForCharacter { get; set; }
	public bool IsFirstClearGlobal { get; set; }
}
