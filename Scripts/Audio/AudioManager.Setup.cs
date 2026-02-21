using Godot;

public partial class AudioManager
{
	private void EnsurePlayers()
	{
		// Create required players if scene does not provide them.
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

	private void LoadStreams()
	{
		// Centralized asset mapping so call sites only call Play* APIs.
		_bgmMenu = GD.Load<AudioStream>("res://Assets/Sound/bgm_menu.mp3");
		_bgmGameplay = GD.Load<AudioStream>("res://Assets/Sound/bgm_gameplay.mp3");
		ApplyBgmLoopSettings();

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

	private void ApplyBgmLoopSettings()
	{
		if (_bgmMenu is AudioStreamMP3 menuMp3)
			menuMp3.Loop = true;
		if (_bgmGameplay is AudioStreamMP3 gameplayMp3)
			gameplayMp3.Loop = true;
	}
}
