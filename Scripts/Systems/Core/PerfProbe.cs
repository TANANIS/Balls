using Godot;
using System;
using System.Text;

public partial class PerfProbe : Node
{
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public float DurationSeconds = 300f;
	[Export] public bool StartOnReady = true;
	[Export] public bool RunWhilePaused = false;
	[Export] public string OutputPath = "user://perf_probe.csv";

	private Node _enemies;
	private float _elapsed;
	private bool _running;
	private int _maxEnemies;
	private long _enemyCountSum;
	private int _enemyCountSamples;
	private float _fpsSum;
	private int _fpsSamples;
	private long _totalSpawns;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_enemies = GetNodeOrNull<Node>(EnemiesPath);
		if (_enemies != null)
			_enemies.ChildEnteredTree += OnEnemySpawned;

		if (StartOnReady)
			Start();
	}

	public void Start()
	{
		_elapsed = 0f;
		_running = true;
		_maxEnemies = 0;
		_enemyCountSum = 0;
		_enemyCountSamples = 0;
		_fpsSum = 0f;
		_fpsSamples = 0;
		_totalSpawns = 0;
	}

	public override void _Process(double delta)
	{
		if (!_running)
			return;
		if (!RunWhilePaused && GetTree().Paused)
			return;

		_elapsed += (float)delta;

		int enemies = _enemies != null ? _enemies.GetChildCount() : 0;
		_enemyCountSum += enemies;
		_enemyCountSamples++;
		if (enemies > _maxEnemies)
			_maxEnemies = enemies;

		float fps = (float)Engine.GetFramesPerSecond();
		if (fps > 0f)
		{
			_fpsSum += fps;
			_fpsSamples++;
		}

		if (_elapsed >= DurationSeconds)
		{
			_running = false;
			WriteReport();
		}
	}

	private void OnEnemySpawned(Node child)
	{
		_totalSpawns++;
	}

	private void WriteReport()
	{
		float avgFps = _fpsSamples > 0 ? _fpsSum / _fpsSamples : 0f;
		float avgEnemies = _enemyCountSamples > 0 ? (float)_enemyCountSum / _enemyCountSamples : 0f;

		var sb = new StringBuilder();
		sb.AppendLine("duration_sec,avg_fps,max_enemies,avg_enemies,total_spawns,created_at_utc");
		sb.AppendLine(string.Format(
			System.Globalization.CultureInfo.InvariantCulture,
			"{0},{1:F2},{2},{3:F2},{4},{5}",
			DurationSeconds,
			avgFps,
			_maxEnemies,
			avgEnemies,
			_totalSpawns,
			DateTime.UtcNow.ToString("o")
		));

		using var file = FileAccess.Open(OutputPath, FileAccess.ModeFlags.Write);
		if (file != null)
			file.StoreString(sb.ToString());

		DebugSystem.Log($"[PerfProbe] Done. avg_fps={avgFps:F2} max_enemies={_maxEnemies} avg_enemies={avgEnemies:F2} total_spawns={_totalSpawns} -> {OutputPath}");
	}
}
