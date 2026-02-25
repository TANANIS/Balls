using Godot;

public partial class DebugCheatSystem
{
	private void SetMenuVisible(bool visible)
	{
		if (_panel == null)
			return;

		bool currentlyVisible = _panel.Visible;
		if (visible && !currentlyVisible)
		{
			_pauseStateBeforeDebugMenu = GetTree()?.Paused ?? false;
			if (!_pauseStateBeforeDebugMenu && GetTree() != null)
			{
				GetTree().Paused = true;
				_pausedByDebugMenu = true;
			}
			else
			{
				_pausedByDebugMenu = false;
			}
		}
		else if (!visible && currentlyVisible)
		{
			if (_pausedByDebugMenu && GetTree() != null)
				GetTree().Paused = _pauseStateBeforeDebugMenu;
			_pausedByDebugMenu = false;
		}

		_panel.Visible = visible;
	}
}
