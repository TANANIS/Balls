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

	private void PlayBgm(AudioStream stream)
	{
		if (_bgmPlayer == null || stream == null)
			return;

		if (_bgmPlayer.Stream != stream)
			_bgmPlayer.Stream = stream;

		if (!_bgmPlayer.Playing)
			_bgmPlayer.Play();
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
