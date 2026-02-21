using Godot;

public partial class SpawnSystem
{
	private void UpdatePhaseTailMiniBossSchedule(float dt)
	{
		if (_spawnFreezeTimer > 0f)
		{
			_spawnFreezeTimer -= dt;
			if (_spawnFreezeTimer <= 0f && _pendingPhaseMiniBossIndex >= 0)
			{
				SpawnPhaseMiniBoss(_pendingPhaseMiniBossIndex);
				_pendingPhaseMiniBossIndex = -1;
			}
			return;
		}

		if (!UsePhaseTailMiniBossSchedule)
			return;

		float[] schedule =
		{
			Mathf.Max(1f, Phase1MiniBossAtSeconds),
			Mathf.Max(1f, Phase2MiniBossAtSeconds),
			Mathf.Max(1f, Phase3MiniBossAtSeconds),
			Mathf.Max(1f, Phase4MiniBossAtSeconds)
		};

		for (int i = 0; i < schedule.Length; i++)
		{
			if (_phaseMiniBossSpawned[i])
				continue;
			if (_survivalSeconds < schedule[i])
				continue;

			_phaseMiniBossSpawned[i] = true;
			_pendingPhaseMiniBossIndex = i;
			_spawnFreezeTimer = Mathf.Max(0f, PhaseMiniBossFreezeSeconds);
			DebugSystem.Log($"[SpawnSystem] Phase tail MiniBoss scheduled: stage={i + 1}, freeze={_spawnFreezeTimer:F2}s");
			return;
		}
	}

	private bool IsInPhaseTailPrepWindow()
	{
		if (!UsePhaseTailMiniBossSchedule)
			return false;
		if (_spawnFreezeTimer > 0f)
			return false;

		float prepSeconds = Mathf.Max(1f, PhaseTailPrepSeconds);
		float[] schedule =
		{
			Mathf.Max(1f, Phase1MiniBossAtSeconds),
			Mathf.Max(1f, Phase2MiniBossAtSeconds),
			Mathf.Max(1f, Phase3MiniBossAtSeconds),
			Mathf.Max(1f, Phase4MiniBossAtSeconds)
		};

		for (int i = 0; i < schedule.Length; i++)
		{
			if (_phaseMiniBossSpawned[i])
				continue;

			float until = schedule[i] - _survivalSeconds;
			if (until <= 0f)
				continue;
			return until <= prepSeconds;
		}

		return false;
	}

	private void SpawnPhaseMiniBoss(int phaseIndex)
	{
		if (!_enemyDefinitions.TryGetValue(MiniBossEnemyId, out EnemyDefinition def) || def.Scene == null)
		{
			DebugSystem.Warn($"[SpawnSystem] MiniBoss definition missing: {MiniBossEnemyId}");
			return;
		}

		if (def.Scene.Instantiate() is not Node2D miniBoss)
		{
			DebugSystem.Warn("[SpawnSystem] MiniBoss scene root is not Node2D.");
			return;
		}

		int stage = Mathf.Clamp(phaseIndex + 1, 1, 4);
		float scaleMult = Mathf.Max(0.5f, PhaseMiniBossScaleBase + ((stage - 1) * PhaseMiniBossScaleStep));
		miniBoss.Scale *= scaleMult;
		miniBoss.GlobalPosition = GetSpawnPositionAroundPlayer();
		miniBoss.Name = $"MiniBossHex_Stage{stage}";

		if (miniBoss.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health)
		{
			int hp = Mathf.Max(1, PhaseMiniBossHpBase + ((stage - 1) * PhaseMiniBossHpStep));
			health.SetMaxHpAndRefill(hp);
		}

		if (miniBoss.GetNodeOrNull<EnemyHitbox>("Hitbox") is EnemyHitbox hitbox)
		{
			hitbox.ContactDamage = Mathf.Max(1, PhaseMiniBossContactDamageBase + ((stage - 1) * PhaseMiniBossContactDamageStep));
		}

		_enemiesRoot.AddChild(miniBoss);
		DebugSystem.Log($"[SpawnSystem] MiniBoss spawned: stage={stage}, scale={scaleMult:F2}");
	}
}
