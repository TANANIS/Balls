using Godot;

public static class GroupServiceResolver
{
	public static T ResolveFirstInGroup<T>(Node owner, string groupName, T current) where T : Node
	{
		if (GodotObject.IsInstanceValid(current))
			return current;
		if (!GodotObject.IsInstanceValid(owner) || string.IsNullOrWhiteSpace(groupName))
			return null;

		var list = owner.GetTree().GetNodesInGroup(groupName);
		foreach (Node node in list)
		{
			if (node is T service)
				return service;
		}

		return null;
	}
}
