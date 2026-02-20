using Godot;

public partial class PlayerHealthBarDemo : Control
{
	[Export] public NodePath PlayerPath = new NodePath("/root/Game/Player");
	[Export] public Vector2 SegmentSize = new Vector2(22f, 14f);
	[Export] public int SegmentGap = 6;
	[Export] public Color FullColor = new Color(0.25f, 0.95f, 0.55f, 1f);
	[Export] public Color EmptyColor = new Color(0.2f, 0.25f, 0.33f, 0.9f);
	[Export] public Color BorderColor = new Color(0.1f, 0.9f, 1f, 0.95f);

	private Label _hpLabel;
	private HBoxContainer _segmentsRoot;
	private PlayerHealth _playerHealth;
	private float _resolveTimer = 0f;
	private int _cachedMaxHp = -1;
	private int _cachedHp = -1;

	public override void _Ready()
	{
		_hpLabel = GetNodeOrNull<Label>("VBox/HpLabel");
		_segmentsRoot = GetNodeOrNull<HBoxContainer>("VBox/Segments");
		if (_segmentsRoot != null)
			_segmentsRoot.AddThemeConstantOverride("separation", SegmentGap);
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

		if (maxHp != _cachedMaxHp)
		{
			RebuildSegments(maxHp);
			_cachedMaxHp = maxHp;
			_cachedHp = -1;
		}

		if (hp != _cachedHp)
		{
			RefreshSegments(hp, maxHp);
			_cachedHp = hp;
		}
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

	private void RebuildSegments(int maxHp)
	{
		if (_segmentsRoot == null)
			return;

		foreach (Node child in _segmentsRoot.GetChildren())
			child.QueueFree();

		for (int i = 0; i < maxHp; i++)
		{
			var segment = new ColorRect
			{
				CustomMinimumSize = SegmentSize,
				Color = EmptyColor
			};

			var border = new StyleBoxFlat
			{
				BgColor = Colors.Transparent,
				BorderColor = BorderColor,
				BorderWidthTop = 1,
				BorderWidthBottom = 1,
				BorderWidthLeft = 1,
				BorderWidthRight = 1,
				CornerRadiusTopLeft = 3,
				CornerRadiusTopRight = 3,
				CornerRadiusBottomLeft = 3,
				CornerRadiusBottomRight = 3
			};
			segment.AddThemeStyleboxOverride("panel", border);

			_segmentsRoot.AddChild(segment);
		}
	}

	private void RefreshSegments(int hp, int maxHp)
	{
		if (_hpLabel != null)
			_hpLabel.Text = $"HP {hp}/{maxHp}";
		if (_segmentsRoot == null)
			return;

		for (int i = 0; i < _segmentsRoot.GetChildCount(); i++)
		{
			if (_segmentsRoot.GetChild(i) is not ColorRect segment)
				continue;
			segment.Color = i < hp ? FullColor : EmptyColor;
		}
	}
}
