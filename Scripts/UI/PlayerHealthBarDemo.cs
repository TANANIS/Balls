using Godot;

public partial class PlayerHealthBarDemo : Control
{
	[Export] public NodePath PlayerPath = new NodePath("/root/Game/Player");

	private Label _hpLabel;
	private PlayerHealth _playerHealth;
	private float _resolveTimer = 0f;
	private string _lastHpText = string.Empty;

	public override void _Ready()
	{
		_hpLabel = GetNodeOrNull<Label>("VBox/HpLabel");
		ResolvePlayerHealth();
	}

	public override void _Process(double delta)
	{
		if (!IsInstanceValid(_playerHealth))
		{
			_resolveTimer -= (float)delta;
			if (_resolveTimer <= 0f)
			{
				_resolveTimer = 0.5f;
				ResolvePlayerHealth();
			}
			return;
		}

		int maxHp = Mathf.Max(1, _playerHealth.MaxHp);
		int hp = Mathf.Clamp(_playerHealth.Hp, 0, maxHp);
		RefreshHpText(hp, maxHp);
	}

	private void ResolvePlayerHealth()
	{
		Player player = GetNodeOrNull<Player>(PlayerPath);
		if (!IsInstanceValid(player))
			player = FindPlayerInSceneTree();
		_playerHealth = player?.GetNodeOrNull<PlayerHealth>("Health");
		if (!IsInstanceValid(_playerHealth))
		{
			if (_hpLabel != null)
				_hpLabel.Text = "HP --/--";
		}
	}

	private Player FindPlayerInSceneTree()
	{
		Node current = GetTree().CurrentScene;
		if (current == null)
			return null;
		Node found = current.FindChild("Player", recursive: true, owned: false);
		return found as Player;
	}

	private void RefreshHpText(int hp, int maxHp)
	{
		if (_hpLabel == null)
			return;
		string text = $"HP {hp}/{maxHp}";
		if (text == _lastHpText)
			return;
		_lastHpText = text;
		_hpLabel.Text = text;
	}
}
