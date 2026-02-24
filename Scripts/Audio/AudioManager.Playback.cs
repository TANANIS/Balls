using Godot;

public partial class AudioManager
{
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

	private System.Collections.Generic.List<AudioStream> GetTracksForPlaylist(BgmPlaylist playlist)
	{
		return playlist switch
		{
			BgmPlaylist.Menu => _bgmMenuTracks,
			BgmPlaylist.Gameplay => _bgmGameplayTracks,
			BgmPlaylist.Result => _bgmResultTracks,
			_ => null
		};
	}

	private AudioStream PickRandomTrack(System.Collections.Generic.List<AudioStream> tracks, AudioStream previous)
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
