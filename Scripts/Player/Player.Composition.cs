using Godot;

public partial class Player
{
	private void ResolveModules()
	{
		// Hard dependencies expected in player scene tree.
		_health = GetNode<PlayerHealth>("Health");
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_primaryAttack = GetNode<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = GetNode<PlayerMelee>("SecondaryAttack");
	}

	private void BindSignals()
	{
		if (_health != null)
			_health.Died += OnDied;
	}

	private void SetupModules()
	{
		// Pass player context to child behavior modules.
		_movement.Setup(this);
		_dash.Setup(this);
		_primaryAttack.Setup(this);
		_secondaryAttack.Setup(this);
	}

	private void OnDied()
	{
		if (_deathLogged)
			return;
		_deathLogged = true;
		DebugSystem.Log("[Player] Died.");
	}
}
