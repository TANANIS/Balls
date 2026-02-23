using System;

public static class PowerCurve
{
	public static int ComputeSoftCappedCurrency(int baseCurrency, EconomyTuning tuning)
	{
		if (tuning == null)
			tuning = new EconomyTuning();
		tuning.Normalize();

		baseCurrency = Math.Max(0, baseCurrency);
		float softCap = tuning.SoftCap;
		float expTerm = 1f - MathF.Exp(-baseCurrency / softCap);
		float curved = (softCap * expTerm) + (tuning.TailLinear * baseCurrency);
		return Math.Max(0, (int)MathF.Floor(curved));
	}
}
