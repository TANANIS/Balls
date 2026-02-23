using System;

public static class RewardCalculator
{
	public static RewardBreakdown Calculate(RunResult result, EconomyTuning tuning, MetaProgressionState state = null)
	{
		result ??= new RunResult();
		tuning ??= new EconomyTuning();
		tuning.Normalize();

		bool isDuplicate = state != null
			&& !string.IsNullOrWhiteSpace(result.RunId)
			&& state.SettledRunIds.Contains(result.RunId);
		if (isDuplicate)
		{
			return new RewardBreakdown
			{
				RunId = result.RunId,
				InputScore = Math.Max(0, result.Score),
				IsDuplicateRun = true
			};
		}

		int score = Math.Max(0, result.Score);
		int baseCurrency = score / tuning.ScoreDivisor;
		int softCapped = PowerCurve.ComputeSoftCappedCurrency(baseCurrency, tuning);

		int bonus = tuning.FlatBonus;
		if (result.IsPerfectClear)
			bonus += tuning.PerfectClearBonus;

		int firstClearBonus = 0;
		if (result.IsFirstClearForCharacter)
			firstClearBonus += tuning.FirstClearCharacterBonus;
		if (result.IsFirstClearGlobal)
			firstClearBonus += tuning.FirstClearGlobalBonus;

		int total = Math.Max(0, softCapped + bonus + firstClearBonus);
		return new RewardBreakdown
		{
			RunId = result.RunId,
			InputScore = score,
			BaseCurrency = baseCurrency,
			SoftCappedCurrency = softCapped,
			BonusCurrency = bonus,
			FirstClearBonus = firstClearBonus,
			TotalCurrency = total,
			IsDuplicateRun = false
		};
	}
}
