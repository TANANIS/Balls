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
		TickRegen(dt);
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f) return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void SetDebugNoDamage(bool enabled)
	{
		_debugNoDamage = enabled;
	}

	public void TakeDamage(int amount, object source)
	{
		// Minimal safeguard in case someone bypasses CombatSystem
		if (_isDead) return;
		if (_debugNoDamage) return;
		if (IsInvincible) return;
		if (_shieldEnabled && _shieldCooldownTimer <= 0f)
		{
			_shieldCooldownTimer = _shieldCooldownSeconds;
			TriggerShieldHitFlash();
			RefreshShieldVisual(force: true);
			return;
		}

		int appliedDamage = Mathf.Max(0, amount);
		if (appliedDamage <= 0)
			return;

		_hp -= appliedDamage;
		Damaged?.Invoke(appliedDamage, source);
		TriggerDamageFeedback();
		AudioManager.Instance?.PlaySfxPlayerGetHit();
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
		ResetVisualRuntimeState();
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

		int before = _hp;
		_hp = Mathf.Min(MaxHp, _hp + amount);
		int healed = _hp - before;
		if (healed > 0)
			TryPlayPriestHealVfx();
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

		_regenTimer = RegenIntervalSeconds;
		Heal(RegenAmount);
	}

	public void DebugSetCurrentHp(int hp)
	{
		_hp = Mathf.Clamp(hp, 0, Mathf.Max(1, MaxHp));
		_isDead = _hp <= 0;
		if (!_isDead)
			_invincibleTimer = 0f;
		else
			AudioManager.Instance?.StopLowHpLoop();

		UpdateLowHpAudio();
	}
}
