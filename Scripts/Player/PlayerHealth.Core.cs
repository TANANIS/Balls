using Godot;

public partial class PlayerHealth
{
	public override void _Ready()
	{
		_hp = MaxHp;
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
		EnsureShieldVisual();
		RefreshShieldVisual(force: true);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;
		if (_shieldCooldownTimer > 0f)
			_shieldCooldownTimer -= dt;
		RefreshShieldVisual();
		TickDebugInvincibleToggle();
		TickRegen(dt);
	}

	private void TickDebugInvincibleToggle()
	{
		if (!EnableDebugInvincibleToggle)
		{
			_togglePressedLastFrame = false;
			return;
		}

		bool pressed = Input.IsPhysicalKeyPressed(DebugInvincibleToggleKey);
		if (pressed && !_togglePressedLastFrame)
		{
			_debugInvincible = !_debugInvincible;
			DebugSystem.Log(_debugInvincible
				? "[PlayerHealth] Debug invincible ON."
				: "[PlayerHealth] Debug invincible OFF.");
		}

		_togglePressedLastFrame = pressed;
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f) return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void TakeDamage(int amount, object source)
	{
		// Minimal safeguard in case someone bypasses CombatSystem
		if (_isDead) return;
		if (IsInvincible) return;
		if (_shieldEnabled && _shieldCooldownTimer <= 0f)
		{
			_shieldCooldownTimer = _shieldCooldownSeconds;
			TriggerShieldHitFlash();
			RefreshShieldVisual(force: true);
			DebugSystem.Log("[PlayerHealth] Shield absorbed damage.");
			return;
		}

		_hp -= amount;
		TriggerDamageFeedback();
		AudioManager.Instance?.PlaySfxPlayerGetHit();
		DebugSystem.Log($"[PlayerHealth] Took {amount} damage. HP: {_hp}/{MaxHp}");
		if (RegenAmount > 0 && RegenIntervalSeconds > 0f)
			_regenTimer = RegenIntervalSeconds;

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0 && !_isDead)
		{
			_isDead = true;
			AudioManager.Instance?.StopLowHpLoop();
			AudioManager.Instance?.PlaySfxPlayerDie();
			Died?.Invoke();
		}
		else
		{
			UpdateLowHpAudio();
		}
	}

	public void ResetToFull()
	{
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
		_shieldEnabled = false;
		_shieldCooldownSeconds = 0f;
		_shieldCooldownTimer = 0f;
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
		RefreshShieldVisual(force: true);
		UpdateLowHpAudio();
	}

	public void AddMaxHp(int amount, bool healByAmount = true)
	{
		if (amount <= 0) return;

		MaxHp += amount;
		if (healByAmount)
			_hp += amount;

		if (_hp > MaxHp)
			_hp = MaxHp;

		UpdateLowHpAudio();
	}

	public void Heal(int amount)
	{
		if (amount <= 0 || _isDead)
			return;
		if (_hp >= MaxHp)
			return;

		_hp = Mathf.Min(MaxHp, _hp + amount);
		UpdateLowHpAudio();
	}

	private void UpdateLowHpAudio()
	{
		if (_isDead)
			return;

		if (_hp <= 1)
			AudioManager.Instance?.StartLowHpLoop();
		else
			AudioManager.Instance?.StopLowHpLoop();
	}

	public void SetBaseStats(int maxHp, float hurtIFrame, bool refill = true)
	{
		MaxHp = Mathf.Max(1, maxHp);
		HurtIFrame = Mathf.Max(0f, hurtIFrame);
		_shieldEnabled = false;
		_shieldCooldownSeconds = 0f;
		_shieldCooldownTimer = 0f;
		RefreshShieldVisual(force: true);
		if (refill)
		{
			_hp = MaxHp;
			_isDead = false;
			_invincibleTimer = 0f;
			UpdateLowHpAudio();
		}
		else if (_hp > MaxHp)
		{
			_hp = MaxHp;
		}
	}

	public void SetRegen(int amount, float intervalSeconds)
	{
		RegenAmount = Mathf.Max(0, amount);
		RegenIntervalSeconds = Mathf.Max(0f, intervalSeconds);
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
	}

	private void TickRegen(float dt)
	{
		if (_isDead || RegenAmount <= 0 || RegenIntervalSeconds <= 0f)
			return;
		if (_hp >= MaxHp)
			return;

		_regenTimer -= dt;
		if (_regenTimer > 0f)
			return;

		_hp = Mathf.Min(MaxHp, _hp + RegenAmount);
		_regenTimer = RegenIntervalSeconds;
		UpdateLowHpAudio();
	}
}
