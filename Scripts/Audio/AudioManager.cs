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
