using Godot;
using System;

public partial class MainScence : Node2D
{
	[Export] public bool PrintOnReady = true;
	[Export] public int CheckLayer = 1; // 想查第幾層就改這個

	public override void _Ready()
	{
		if (!PrintOnReady) return;
		PrintNodesUsingLayer(CheckLayer);
	}

	private void PrintNodesUsingLayer(int layer)
	{
		GD.Print($"=== CollisionAudit: nodes with Layer {layer} enabled ===");

		int count = 0;
		foreach (var node in GetTree().CurrentScene.GetChildren(true))
		{
			if (node is CollisionObject2D col)
			{
				if (col.GetCollisionLayerValue(layer))
				{
					GD.Print($"[Layer{layer}] {col.GetPath()}  (type={col.GetType().Name})  layerMask={col.CollisionLayer}  mask={col.CollisionMask}");
					count++;
				}
			}
		}

		GD.Print($"=== Total: {count} nodes ===");
	}
}
