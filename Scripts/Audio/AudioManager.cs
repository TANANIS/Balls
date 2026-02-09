using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	[Export] public float BgmVolumeDb = 0f;
	[Export] public float SfxVolumeDb = 0f;

	private AudioStreamPlayer _bgmPlayer;
	private readonly List<AudioStreamPlayer> _sfxPlayers = new();
	private int _sfxIndex = 0;

	private AudioStream _bgmMenu;
	private AudioStream _bgmGameplay;
	private AudioStream _sfxUiButton;
	private AudioStream _sfxUiExit;
	private AudioStream _sfxUiUpgradeSelect;
	private AudioStream _sfxPlayerDash;
	private AudioStream _sfxPlayerFire;
	private AudioStream _sfxPlayerMelee;
	private AudioStream _sfxPlayerUpgrade;
	private AudioStream _sfxPlayerGetHit;
	private AudioStream _sfxPlayerDie;
	private AudioStream _sfxPlayerOneHp;

	private AudioStreamPlayer _lowHpLoopPlayer;

	private readonly Dictionary<string, AudioStream> _enemyDeathSfxByScene = new();

	public override void _EnterTree()
	{
		Instance = this;
		AddToGroup("AudioManager");
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		EnsurePlayers();
		LoadStreams();
		BindCombatEvents();
	}

	private void EnsurePlayers()
	{
		_bgmPlayer = GetNodeOrNull<AudioStreamPlayer>("BgmPlayer");
		if (_bgmPlayer == null)
		{
			_bgmPlayer = new AudioStreamPlayer { Name = "BgmPlayer" };
			AddChild(_bgmPlayer);
		}

		_bgmPlayer.VolumeDb = BgmVolumeDb;
		_bgmPlayer.ProcessMode = ProcessModeEnum.Always;

		EnsureSfxPool();

		_lowHpLoopPlayer = GetNodeOrNull<AudioStreamPlayer>("LowHpLoopPlayer");
		if (_lowHpLoopPlayer == null)
		{
			_lowHpLoopPlayer = new AudioStreamPlayer { Name = "LowHpLoopPlayer" };
			AddChild(_lowHpLoopPlayer);
		}
		_lowHpLoopPlayer.ProcessMode = ProcessModeEnum.Always;
		_lowHpLoopPlayer.VolumeDb = SfxVolumeDb;
	}

	private void LoadStreams()
	{
		_bgmMenu = GD.Load<AudioStream>("res://Assets/Sound/bgm_menu.mp3");
		_bgmGameplay = GD.Load<AudioStream>("res://Assets/Sound/bgm_gameplay.mp3");

		_sfxUiButton = GD.Load<AudioStream>("res://Assets/Sound/sfx_ui_button.wav");
		_sfxUiExit = GD.Load<AudioStream>("res://Assets/Sound/sfx_ui_exit.wav");
		_sfxUiUpgradeSelect = GD.Load<AudioStream>("res://Assets/Sound/sfx_ui_upgrade_select.wav");

		_sfxPlayerDash = GD.Load<AudioStream>("res://Assets/Sound/sfx_player_dash.wav");
		_sfxPlayerFire = GD.Load<AudioStream>("res://Assets/Sound/sfx_player_fire.wav");
		_sfxPlayerMelee = GD.Load<AudioStream>("res://Assets/Sound/sfx_player_melee.wav");
		_sfxPlayerUpgrade = GD.Load<AudioStream>("res://Assets/Sound/sfx_player_upgrade.wav");
		_sfxPlayerGetHit = GD.Load<AudioStream>("res://Assets/Sound/PlayerGetHit.wav");
		_sfxPlayerDie = GD.Load<AudioStream>("res://Assets/Sound/PlayerDie.wav");
		_sfxPlayerOneHp = GD.Load<AudioStream>("res://Assets/Sound/PlayerOneHp.wav");

		_enemyDeathSfxByScene["res://Enemies/SwarmCircle.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/EnemiesDies/sfx_enemy_swarm_circle_die.wav");
		_enemyDeathSfxByScene["res://Enemies/ChargerTriangle.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/EnemiesDies/sfx_enemy_charger_triangle_die.wav");
		_enemyDeathSfxByScene["res://Enemies/TankSquare.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/EnemiesDies/sfx_enemy_tank_square_die.wav");
		_enemyDeathSfxByScene["res://Enemies/EliteSwarmCircle.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/EnemiesDies/sfx_enemy_elite_swarm_circle_die.wav");
		_enemyDeathSfxByScene["res://Enemies/MiniBossHex.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/EnemiesDies/sfx_enemy_miniboss_hex_die.wav");
	}

	private void BindCombatEvents()
	{
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0 && list[0] is CombatSystem combat)
			combat.EnemyKilled += OnEnemyKilled;
	}

	private void OnEnemyKilled(Node source, Node target)
	{
		if (target == null)
			return;

		Node enemy = target.GetParent();
		if (enemy == null)
			return;

		string scenePath = enemy.SceneFilePath;
		if (_enemyDeathSfxByScene.TryGetValue(scenePath, out AudioStream stream))
			PlaySfx(stream);
	}

	private void PlaySfx(AudioStream stream, float volumeDbOffset = 0f)
	{
		if (stream == null)
			return;

		if (_sfxPlayers.Count == 0)
			EnsureSfxPool();

		if (_sfxPlayers.Count == 0)
			return;

		_sfxIndex = (_sfxIndex + 1) % _sfxPlayers.Count;
		var player = _sfxPlayers[_sfxIndex];
		player.VolumeDb = SfxVolumeDb + volumeDbOffset;
		player.Stream = stream;
		player.Play();
	}

	private void EnsureSfxPool()
	{
		if (_sfxPlayers.Count > 0)
			return;

		for (int i = 0; i < 8; i++)
		{
			var sfx = new AudioStreamPlayer { Name = $"SfxPlayer{i}" };
			sfx.ProcessMode = ProcessModeEnum.Always;
			sfx.VolumeDb = SfxVolumeDb;
			AddChild(sfx);
			_sfxPlayers.Add(sfx);
		}
	}

	private void PlayBgm(AudioStream stream)
	{
		if (_bgmPlayer == null || stream == null)
			return;

		if (_bgmPlayer.Stream != stream)
			_bgmPlayer.Stream = stream;

		if (!_bgmPlayer.Playing)
			_bgmPlayer.Play();
	}

	public void PlayBgmMenu() => PlayBgm(_bgmMenu);
	public void PlayBgmGameplay() => PlayBgm(_bgmGameplay);

	public void PlaySfxUiButton() => PlaySfx(_sfxUiButton);
	public void PlaySfxUiExit() => PlaySfx(_sfxUiExit);
	public void PlaySfxUiUpgradeSelect() => PlaySfx(_sfxUiUpgradeSelect);

	public void PlaySfxPlayerDash() => PlaySfx(_sfxPlayerDash);
	public void PlaySfxPlayerFire() => PlaySfx(_sfxPlayerFire, -6f);
	public void PlaySfxPlayerMelee() => PlaySfx(_sfxPlayerMelee);
	public void PlaySfxPlayerUpgrade() => PlaySfx(_sfxPlayerUpgrade);
	public void PlaySfxPlayerGetHit() => PlaySfx(_sfxPlayerGetHit);
	public void PlaySfxPlayerDie() => PlaySfx(_sfxPlayerDie, +6f);

	public void StartLowHpLoop()
	{
		if (_lowHpLoopPlayer == null || _sfxPlayerOneHp == null)
			return;

		if (_lowHpLoopPlayer.Stream != _sfxPlayerOneHp)
			_lowHpLoopPlayer.Stream = _sfxPlayerOneHp;

		if (_sfxPlayerOneHp is AudioStreamWav wav && wav.LoopMode == AudioStreamWav.LoopModeEnum.Disabled)
			wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;

		if (!_lowHpLoopPlayer.Playing)
			_lowHpLoopPlayer.Play();
	}

	public void StopLowHpLoop()
	{
		if (_lowHpLoopPlayer == null)
			return;

		if (_lowHpLoopPlayer.Playing)
			_lowHpLoopPlayer.Stop();
	}
}
