using Godot;

public partial class Enemy
{
	private void ResolvePlayer()
	{
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
	}

	private void ResolveBehavior()
	{
		_behavior = GetNodeOrNull<EnemyBehaviorModule>(BehaviorPath);
		if (_behavior == null)
		{
			foreach (Node child in GetChildren())
			{
				if (child is EnemyBehaviorModule module)
				{
					_behavior = module;
					break;
				}
			}
		}

		_behavior?.OnInitialized(this);
	}

	private void ResolveSeparation()
	{
		_separation = GetNodeOrNull<EnemySeparationModule>(SeparationPath);
		if (_separation == null)
		{
			foreach (Node child in GetChildren())
			{
				if (child is EnemySeparationModule module)
				{
					_separation = module;
					break;
				}
			}
		}
	}

	private void ResolveEvents()
	{
		_events.Clear();

		Node root = GetNodeOrNull<Node>(EventsPath);
		if (root != null)
		{
			foreach (Node child in root.GetChildren())
			{
				if (child is EnemyEventModule evt)
					_events.Add(evt);
			}
		}
		else
		{
			foreach (Node child in GetChildren())
			{
				if (child is EnemyEventModule evt)
					_events.Add(evt);
			}
		}

		foreach (EnemyEventModule evt in _events)
			evt.OnInitialized(this);
	}
}
