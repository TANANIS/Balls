using System;

public sealed class EconomyTuning
{
	public int ScoreDivisor { get; set; } = 100;
	public float SoftCap { get; set; } = 120f;
	public float TailLinear { get; set; } = 0.10f;

	public int PerfectClearBonus { get; set; } = 12;
	public int FirstClearCharacterBonus { get; set; } = 15;
	public int FirstClearGlobalBonus { get; set; } = 40;
	public int FlatBonus { get; set; }

	public void Normalize()
	{
		ScoreDivisor = Math.Max(1, ScoreDivisor);
		SoftCap = Math.Max(1f, SoftCap);
		TailLinear = Math.Clamp(TailLinear, 0f, 1f);
		PerfectClearBonus = Math.Max(0, PerfectClearBonus);
		FirstClearCharacterBonus = Math.Max(0, FirstClearCharacterBonus);
		FirstClearGlobalBonus = Math.Max(0, FirstClearGlobalBonus);
		FlatBonus = Math.Max(0, FlatBonus);
	}
}
