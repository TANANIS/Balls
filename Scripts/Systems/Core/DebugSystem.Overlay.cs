using Godot;
using System.Text;

public partial class DebugSystem
{
	private void CreateOverlayUi()
	{
		// Build minimal runtime overlay UI tree.
		_overlayLayer = new CanvasLayer();
		_overlayLayer.Name = "DebugOverlay";
		_overlayLayer.Layer = 100;
		AddChild(_overlayLayer);

		_overlayPanel = new Panel();
		_overlayPanel.Name = "Background";
		_overlayPanel.Position = new Vector2(8, 8);
		_overlayPanel.Size = new Vector2(Mathf.Max(280, OverlayWidth), Mathf.Max(140, OverlayHeight));
		_overlayPanel.Modulate = OverlayBackgroundColor;
		_overlayLayer.AddChild(_overlayPanel);

		_overlayText = new RichTextLabel();
		_overlayText.Name = "Text";
		_overlayText.BbcodeEnabled = false;
		_overlayText.FitContent = false;
		_overlayText.ScrollActive = true;
		_overlayText.ScrollFollowing = true;
		_overlayText.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_overlayText.Position = new Vector2(8, 8);
		_overlayText.Size = _overlayPanel.Size - new Vector2(16, 16);
		_overlayPanel.AddChild(_overlayText);
	}

	private void AppendOverlayLine(string line)
	{
		// Keep a capped ring-style queue of recent logs.
		_overlayLines.Enqueue(line);
		while (_overlayLines.Count > Mathf.Max(1, OverlayMaxLines))
			_overlayLines.Dequeue();

		if (_overlayText == null)
			return;

		var sb = new StringBuilder();
		foreach (string item in _overlayLines)
		{
			sb.Append(item);
			sb.Append('\n');
		}

		_overlayText.Text = sb.ToString();
	}

	private void UpdateOverlayVisibility()
	{
		if (_overlayLayer != null)
			_overlayLayer.Visible = _overlayVisible;
	}
}
