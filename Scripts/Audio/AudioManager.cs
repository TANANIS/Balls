using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	[Export] public float BgmVolumeDb = 0f;
	[Export] public float SfxVolumeDb = 0f;

	private enum BgmPlaylist
	{
		None,
		Menu,
		Gameplay,
		Result
	}

	private AudioStreamPlayer _bgmPlayer;
	private readonly List<AudioStreamPlayer> _sfxPlayers = new();
	private int _sfxIndex = 0;

	private AudioStream _bgmMenu;
	private AudioStream _bgmGameplay;
	private AudioStream _bgmResult;
	private readonly List<AudioStream> _bgmMenuTracks = new();
	private readonly List<AudioStream> _bgmGameplayTracks = new();
	private readonly List<AudioStream> _bgmResultTracks = new();
	private readonly RandomNumberGenerator _bgmRng = new();
	private BgmPlaylist _currentBgmPlaylist = BgmPlaylist.None;
	private AudioStream _currentBgmTrack;
	private AudioStream _sfxUiButton;
	private AudioStream _sfxUiExit;
	private AudioStream _sfxUiUpgradeSelect;
	private AudioStream _sfxPlayerDash;
	private AudioStream _sfxPlayerFire;
	private AudioStream _sfxPlayerFirePriest;
	private AudioStream _sfxPlayerFireArcher;
	private AudioStream _sfxPlayerMelee;
	private AudioStream _sfxPlayerUpgrade;
	private AudioStream _sfxPlayerExpPickup;
	private AudioStream _sfxPlayerHitEnemy;
	private AudioStream _sfxPlayerElementalBurst;
	private AudioStream _sfxPlayerGetHit;
	private AudioStream _sfxPlayerDie;
	private AudioStream _sfxPlayerOneHp;

	private AudioStreamPlayer _lowHpLoopPlayer;
	private readonly Dictionary<string, AudioStream> _enemyDeathSfxByScene = new();

	public override void _EnterTree()
	{
		Instance = this;
		AddToGroup(RuntimeGroups.AudioManager);
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_bgmRng.Randomize();
		EnsurePlayers();
		LoadStreams();
		BindCombatEvents();
	}

	public void PlayBgmMenu() => StartBgmPlaylist(BgmPlaylist.Menu);
	public void PlayBgmGameplay() => StartBgmPlaylist(BgmPlaylist.Gameplay);
	public void PlayBgmResult() => StartBgmPlaylist(BgmPlaylist.Result);

	public void PlaySfxUiButton() => PlaySfx(_sfxUiButton);
	public void PlaySfxUiExit() => PlaySfx(_sfxUiExit);
	public void PlaySfxUiUpgradeSelect() => PlaySfx(_sfxUiUpgradeSelect);

	public void PlaySfxPlayerDash() => PlaySfx(_sfxPlayerDash);
	public void PlaySfxPlayerFire() => PlaySfx(_sfxPlayerFire, -6f);
	public void PlaySfxPlayerFirePriest() => PlaySfx(_sfxPlayerFirePriest, -6f);
	public void PlaySfxPlayerFireArcher() => PlaySfx(_sfxPlayerFireArcher, -6f);
	public void PlaySfxPlayerMelee() => PlaySfx(_sfxPlayerMelee);
	public void PlaySfxPlayerUpgrade() => PlaySfx(_sfxPlayerUpgrade);
	public void PlaySfxPlayerExpPickup() => PlaySfx(_sfxPlayerExpPickup);
	public void PlaySfxPlayerHitEnemy() => PlaySfx(_sfxPlayerHitEnemy, -5f);
	public void PlaySfxPlayerElementalBurst() => PlaySfx(_sfxPlayerElementalBurst, -3f);
	public void PlaySfxPlayerGetHit() => PlaySfx(_sfxPlayerGetHit);
	public void PlaySfxPlayerDie() => PlaySfx(_sfxPlayerDie, +6f);

	public float GetBgmVolumeLinear()
	{
		return Mathf.Clamp(Mathf.DbToLinear(BgmVolumeDb), 0f, 1f);
	}

	public float GetSfxVolumeLinear()
	{
		return Mathf.Clamp(Mathf.DbToLinear(SfxVolumeDb), 0f, 1f);
	}

	public void SetBgmVolumeLinear(float linear)
	{
		BgmVolumeDb = LinearToDb(linear);
		if (_bgmPlayer != null)
			_bgmPlayer.VolumeDb = BgmVolumeDb;
	}

	public void SetSfxVolumeLinear(float linear)
	{
		SfxVolumeDb = LinearToDb(linear);
		foreach (AudioStreamPlayer player in _sfxPlayers)
			player.VolumeDb = SfxVolumeDb;
		if (_lowHpLoopPlayer != null)
			_lowHpLoopPlayer.VolumeDb = SfxVolumeDb;
	}

	private static float LinearToDb(float linear)
	{
		float clamped = Mathf.Clamp(linear, 0f, 1f);
		if (clamped <= 0.0001f)
			return -80f;
		return Mathf.LinearToDb(clamped);
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

	private void StartBgmPlaylist(BgmPlaylist playlist)
	{
		if (_bgmPlayer == null)
			return;

		if (_currentBgmPlaylist != playlist)
		{
			_currentBgmPlaylist = playlist;
			_currentBgmTrack = null;
			PlayRandomBgmFromCurrentPlaylist();
			return;
		}

		if (!_bgmPlayer.Playing)
			PlayRandomBgmFromCurrentPlaylist();
	}

	private void OnBgmFinished()
	{
		PlayRandomBgmFromCurrentPlaylist();
	}

	private void PlayRandomBgmFromCurrentPlaylist()
	{
		if (_bgmPlayer == null)
			return;

		var tracks = GetTracksForPlaylist(_currentBgmPlaylist);
		if (tracks == null || tracks.Count == 0)
			return;

		AudioStream next = PickRandomTrack(tracks, _currentBgmTrack);
		if (next == null)
			return;

		_currentBgmTrack = next;
		_bgmPlayer.Stream = next;
		_bgmPlayer.Play();
	}

	private List<AudioStream> GetTracksForPlaylist(BgmPlaylist playlist)
	{
		return playlist switch
		{
			BgmPlaylist.Menu => _bgmMenuTracks,
			BgmPlaylist.Gameplay => _bgmGameplayTracks,
			BgmPlaylist.Result => _bgmResultTracks,
			_ => null
		};
	}

	private AudioStream PickRandomTrack(List<AudioStream> tracks, AudioStream previous)
	{
		if (tracks == null || tracks.Count == 0)
			return null;

		if (tracks.Count == 1)
			return tracks[0];

		int fallbackIndex = Mathf.Clamp((int)_bgmRng.RandiRange(0, tracks.Count - 1), 0, tracks.Count - 1);
		AudioStream fallback = tracks[fallbackIndex];

		for (int i = 0; i < 8; i++)
		{
			int idx = Mathf.Clamp((int)_bgmRng.RandiRange(0, tracks.Count - 1), 0, tracks.Count - 1);
			AudioStream candidate = tracks[idx];
			if (candidate != previous)
				return candidate;
		}

		return fallback;
	}

	private void PlaySfx(AudioStream stream, float volumeDbOffset = 0f)
	{
		if (stream == null)
			return;
		// Defensive: ensure one-shot SFX never keep source loop flags.
		SetBgmLoop(stream, loop: false);

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
