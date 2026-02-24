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
			|| name.Contains("lancer", StringComparison.Ordinal)
			|| path.Contains("lancer", StringComparison.Ordinal);
	}
}
