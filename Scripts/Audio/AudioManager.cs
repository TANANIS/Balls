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
		AddToGroup("AudioManager");
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
}
