using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

public partial class ScoreSystem : Node
{
	[Export] public string EnemyDefinitionsCsvPath = "res://Data/Director/EnemyDefinitions.csv";
	[Export] public int BaseScore = 10;
	[Export] public float CostExponent = 1.3f;

	private readonly Dictionary<string, float> _costByScenePath = new();
	private int _score;

	public int Score => _score;
	public event Action<int> ScoreChanged;

	public override void _EnterTree()
	{
		AddToGroup("ScoreSystem");
	}

	public override void _Ready()
	{
		LoadEnemyCosts();

		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0 && list[0] is CombatSystem combat)
			combat.EnemyKilled += OnEnemyKilled;
	}

	public void ResetScore()
	{
		_score = 0;
		ScoreChanged?.Invoke(_score);
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (target == null)
			return;

		Node enemy = target.GetParent();
		if (enemy == null)
			return;

		string scenePath = enemy.SceneFilePath;
		float cost = 1f;
		if (!_costByScenePath.TryGetValue(scenePath, out cost))
			cost = 1f;

		int points = Mathf.RoundToInt(BaseScore * Mathf.Pow(cost, CostExponent));
		AddScore(points);
	}

	private void AddScore(int amount)
	{
		if (amount <= 0)
			return;

		_score += amount;
		ScoreChanged?.Invoke(_score);
	}

	private void LoadEnemyCosts()
	{
		_costByScenePath.Clear();

		if (!FileAccess.FileExists(EnemyDefinitionsCsvPath))
			return;

		using var file = FileAccess.Open(EnemyDefinitionsCsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
			return;

		while (!file.EofReached())
		{
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line))
				continue;
			if (line.TrimStart().StartsWith("#"))
				continue;

			string[] cols = SplitCsvLine(line);
			if (cols.Length < 4)
				continue;

			string scenePath = cols[1].Trim();
			string costStr = cols[3].Trim();

			if (string.IsNullOrWhiteSpace(scenePath))
				continue;

			if (float.TryParse(costStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float cost))
			{
				if (cost <= 0f)
					cost = 1f;
				_costByScenePath[scenePath] = cost;
			}
		}
	}

	private static string[] SplitCsvLine(string line)
	{
		var result = new List<string>();
		bool inQuotes = false;
		var cur = new System.Text.StringBuilder();

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '\"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				result.Add(cur.ToString());
				cur.Clear();
				continue;
			}

			cur.Append(c);
		}

		result.Add(cur.ToString());
		return result.ToArray();
	}
}
