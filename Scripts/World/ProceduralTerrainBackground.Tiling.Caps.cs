using Godot;
using System.Collections.Generic;

public partial class ProceduralTerrainBackground
{
	private void ApplyDiagDrivenCap(
		MaskView mask,
		Vector2I source,
		TileTopology t,
		Dictionary<Vector2I, CapBits> requestedCaps)
	{
		// CAP is directly coupled to diagonal coastline topology.
		if (t.OpenCount != 2)
			return;

		if (t.OpenN && t.OpenE)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Left, Vector2I.Up + Vector2I.Left, CapBits.TopRight);
			return;
		}

		if (t.OpenN && t.OpenW)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Right, Vector2I.Up + Vector2I.Right, CapBits.TopLeft);
			return;
		}

		if (t.OpenS && t.OpenE)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Left, Vector2I.Down + Vector2I.Left, CapBits.BottomRight);
			return;
		}

		if (t.OpenS && t.OpenW)
		{
			RequestDiagCap(mask, source, requestedCaps, Vector2I.Right, Vector2I.Down + Vector2I.Right, CapBits.BottomLeft);
		}
	}

	private static void ApplyFillCornerCap(Vector2I source, TileTopology t, Dictionary<Vector2I, CapBits> requestedCaps)
	{
		// Fill tile inner-corner cap: if one diagonal is grass while both adjacent cardinals are dirt,
		// add a small grass nib on the fill tile. This fixes missing caps between edge/diag joins.
		if (t.OpenCount != 0)
			return;

		if (!t.NE && t.N && t.E)
			AddCapRequest(requestedCaps, source, CapBits.TopRight);
		if (!t.NW && t.N && t.W)
			AddCapRequest(requestedCaps, source, CapBits.TopLeft);
		if (!t.SW && t.S && t.W)
			AddCapRequest(requestedCaps, source, CapBits.BottomLeft);
		if (!t.SE && t.S && t.E)
			AddCapRequest(requestedCaps, source, CapBits.BottomRight);
	}

	private static void RequestDiagCap(
		MaskView mask,
		Vector2I source,
		Dictionary<Vector2I, CapBits> requestedCaps,
		Vector2I preferredSideOffset,
		Vector2I endpointDiagonalOffset,
		CapBits capBit)
	{
		Vector2I sideAnchor = source + preferredSideOffset;
		bool sideIsDirt = mask.IsDirt(sideAnchor);
		bool sideCanReceive = false;
		if (sideIsDirt)
		{
			TileTopology sideTopo = BuildTopology(mask, sideAnchor);
			// Prevent caps from spilling onto edge/diag tiles (e.g. repeated cap on E:s chains).
			sideCanReceive = sideTopo.OpenCount == 0;
		}

		if (sideCanReceive)
			AddCapRequest(requestedCaps, sideAnchor, capBit);

		bool endpointDiagonalIsDirt = mask.IsDirt(source + endpointDiagonalOffset);
		// Only fallback to source when there is no side dirt tile.
		// If side is dirt but not cap-eligible (edge/diag), skipping fallback avoids noisy caps beside E:n/E:s chains.
		if (!sideIsDirt && !endpointDiagonalIsDirt)
			AddCapRequest(requestedCaps, source, capBit);
	}

	private static void AddCapRequest(Dictionary<Vector2I, CapBits> requestedCaps, Vector2I anchor, CapBits capBit)
	{
		if (requestedCaps.TryGetValue(anchor, out CapBits existing))
			requestedCaps[anchor] = existing | capBit;
		else
			requestedCaps[anchor] = capBit;
	}

	private Texture2D ResolveCapTexture(CapBits bits)
	{
		if ((bits & (CapBits.TopLeft | CapBits.BottomRight)) == (CapBits.TopLeft | CapBits.BottomRight))
			return _capGrassNwSe ?? _capGrassNe ?? _capGrassSe;

		if ((bits & (CapBits.TopRight | CapBits.BottomLeft)) == (CapBits.TopRight | CapBits.BottomLeft))
			return _capGrassNeSw ?? _capGrassNw ?? _capGrassSw;

		if ((bits & CapBits.TopRight) != 0)
			return _capGrassNw;
		if ((bits & CapBits.TopLeft) != 0)
			return _capGrassNe;
		if ((bits & CapBits.BottomLeft) != 0)
			return _capGrassSw;
		if ((bits & CapBits.BottomRight) != 0)
			return _capGrassSe;

		return null;
	}
}
