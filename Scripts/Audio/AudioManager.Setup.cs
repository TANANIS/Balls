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
		_bgmPlayer.Finished -= OnBgmFinished;
		_bgmPlayer.Finished += OnBgmFinished;

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
		_bgmMenuTracks.Clear();
		_bgmGameplayTracks.Clear();
		_bgmResultTracks.Clear();
		AddBgmTrack(_bgmMenuTracks, "res://Assets/Sound/Bgm/Alternates/bgm_menu_alt_01.mp3");
		AddBgmTrack(_bgmMenuTracks, "res://Assets/Sound/Bgm/Alternates/bgm_menu_alt_02.mp3");
		AddBgmTrack(_bgmMenuTracks, "res://Assets/Sound/Bgm/Alternates/bgm_menu_alt_03.mp3");
		AddBgmTrack(_bgmGameplayTracks, "res://Assets/Sound/Bgm/bgm_gameplay.mp3");
		AddBgmTrack(_bgmGameplayTracks, "res://Assets/Sound/Bgm/Alternates/bgm_gameplay_alt_01.mp3");
		AddBgmTrack(_bgmGameplayTracks, "res://Assets/Sound/Bgm/Alternates/bgm_gameplay_alt_02.mp3");
		AddBgmTrack(_bgmGameplayTracks, "res://Assets/Sound/Bgm/Alternates/bgm_gameplay_alt_03.mp3");
		AddBgmTrack(_bgmGameplayTracks, "res://Assets/Sound/Bgm/Alternates/bgm_gameplay_alt_04.mp3");
		AddBgmTrack(_bgmResultTracks, "res://Assets/Sound/Bgm/bgm_result.mp3");
		_bgmMenu = _bgmMenuTracks.Count > 0 ? _bgmMenuTracks[0] : null;
		_bgmGameplay = _bgmGameplayTracks.Count > 0 ? _bgmGameplayTracks[0] : null;
		_bgmResult = _bgmResultTracks.Count > 0 ? _bgmResultTracks[0] : null;
		ApplyBgmLoopSettings();

		_sfxUiButton = GD.Load<AudioStream>("res://Assets/Sound/UI/sfx_ui_button.wav");
		_sfxUiExit = GD.Load<AudioStream>("res://Assets/Sound/UI/sfx_ui_exit.wav");
		_sfxUiUpgradeSelect = GD.Load<AudioStream>("res://Assets/Sound/UI/sfx_ui_upgrade_select.wav");

		_sfxPlayerDash = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_dash.wav");
		_sfxPlayerFire = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_fire_wizard.wav");
		_sfxPlayerFirePriest = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_fire_priest.wav");
		_sfxPlayerFireArcher = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_fire_archer.wav");
		_sfxPlayerMelee = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_melee.wav");
		_sfxPlayerUpgrade = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_upgrade.wav");
		_sfxPlayerExpPickup = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_exp_pickup.wav");
		_sfxPlayerHitEnemy = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_hit_enemy.wav");
		_sfxPlayerElementalBurst = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_elemental_burst.wav");
		_sfxPlayerGetHit = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_get_hit.wav");
		_sfxPlayerDie = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_die.wav");
		_sfxPlayerOneHp = GD.Load<AudioStream>("res://Assets/Sound/Player/sfx_player_one_hp.wav");

		if (_sfxPlayerFirePriest == null)
			_sfxPlayerFirePriest = _sfxPlayerFire;
		if (_sfxPlayerFireArcher == null)
			_sfxPlayerFireArcher = _sfxPlayerFire;
		if (_sfxPlayerExpPickup == null)
			_sfxPlayerExpPickup = _sfxPlayerUpgrade;
		if (_sfxPlayerHitEnemy == null)
			_sfxPlayerHitEnemy = _sfxPlayerFire;
		if (_sfxPlayerElementalBurst == null)
			_sfxPlayerElementalBurst = _sfxPlayerHitEnemy;

		_enemyDeathSfxByScene.Clear();
		_enemyDeathSfxByScene["res://Enemies/Slime.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/Enemies/sfx_enemy_slime_die.wav");
		_enemyDeathSfxByScene["res://Enemies/Orc.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/Enemies/sfx_enemy_orc_die.wav");
		_enemyDeathSfxByScene["res://Enemies/EliteOrc.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/Enemies/sfx_enemy_elite_orc_die.wav");
		_enemyDeathSfxByScene["res://Enemies/Werebear.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/Enemies/sfx_enemy_werebear_die.wav");
		_enemyDeathSfxByScene["res://Enemies/Lancer.tscn"] = GD.Load<AudioStream>("res://Assets/Sound/Enemies/sfx_enemy_lancer_die.wav");

		ApplySfxLoopSettings();
	}

	private void BindCombatEvents()
	{
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0 && list[0] is CombatSystem combat)
			combat.EnemyKilled += OnEnemyKilled;
	}

	private void ApplyBgmLoopSettings()
	{
		foreach (AudioStream track in _bgmMenuTracks)
			SetBgmLoop(track, loop: false);
		foreach (AudioStream track in _bgmGameplayTracks)
			SetBgmLoop(track, loop: false);
		foreach (AudioStream track in _bgmResultTracks)
			SetBgmLoop(track, loop: false);
	}

	private static void AddBgmTrack(System.Collections.Generic.List<AudioStream> tracks, string path)
	{
		AudioStream stream = GD.Load<AudioStream>(path);
		if (stream != null)
			tracks.Add(stream);
	}

	private static void SetBgmLoop(AudioStream stream, bool loop)
	{
		if (stream is AudioStreamMP3 mp3)
		{
			mp3.Loop = loop;
			return;
		}

		if (stream is AudioStreamOggVorbis ogg)
		{
			ogg.Loop = loop;
			return;
		}

		if (stream is AudioStreamWav wav)
			wav.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
	}

	private void ApplySfxLoopSettings()
	{
		SetBgmLoop(_sfxUiButton, loop: false);
		SetBgmLoop(_sfxUiExit, loop: false);
		SetBgmLoop(_sfxUiUpgradeSelect, loop: false);
		SetBgmLoop(_sfxPlayerDash, loop: false);
		SetBgmLoop(_sfxPlayerFire, loop: false);
		SetBgmLoop(_sfxPlayerFirePriest, loop: false);
		SetBgmLoop(_sfxPlayerFireArcher, loop: false);
		SetBgmLoop(_sfxPlayerMelee, loop: false);
		SetBgmLoop(_sfxPlayerUpgrade, loop: false);
		SetBgmLoop(_sfxPlayerExpPickup, loop: false);
		SetBgmLoop(_sfxPlayerHitEnemy, loop: false);
		SetBgmLoop(_sfxPlayerElementalBurst, loop: false);
		SetBgmLoop(_sfxPlayerGetHit, loop: false);
		SetBgmLoop(_sfxPlayerDie, loop: false);
		SetBgmLoop(_sfxPlayerOneHp, loop: false);

		foreach (AudioStream stream in _enemyDeathSfxByScene.Values)
			SetBgmLoop(stream, loop: false);
	}
}

