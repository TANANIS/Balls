using System;

public static class EnemyTagRules
{
	public static bool IsMiniBoss(Enemy enemy)
	{
		if (enemy == null)
			return false;

		string name = enemy.Name.ToString().ToLowerInvariant();
		string path = enemy.SceneFilePath?.ToLowerInvariant() ?? string.Empty;
		return IsMiniBoss(name, path);
	}

	public static bool IsMiniBoss(string enemyNameLower, string scenePathLower)
	{
		string name = enemyNameLower ?? string.Empty;
		string path = scenePathLower ?? string.Empty;

		return name.Contains("miniboss", StringComparison.Ordinal)
			|| name.Contains("boss_lancer", StringComparison.Ordinal)
			|| name.Contains("boss_greatsword_skeleton", StringComparison.Ordinal)
			|| path.Contains("boss_lancer", StringComparison.Ordinal)
			|| path.Contains("boss_greatswordskeleton", StringComparison.Ordinal);
	}
}
