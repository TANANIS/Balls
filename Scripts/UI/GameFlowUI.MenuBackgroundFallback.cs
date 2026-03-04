using Godot;

public partial class GameFlowUI
{
	private ColorRect _menuClearFallbackRect;

	private void EnsureMenuClearFallbackNode()
	{
		if (GodotObject.IsInstanceValid(_menuClearFallbackRect))
			return;

		Control panelsRoot = GetNodeOrNull<Control>("Panels");
		if (panelsRoot == null)
			return;

		_menuClearFallbackRect = panelsRoot.GetNodeOrNull<ColorRect>("MenuClearFallback");
		if (!GodotObject.IsInstanceValid(_menuClearFallbackRect))
		{
			_menuClearFallbackRect = new ColorRect
			{
				Name = "MenuClearFallback",
				Color = new Color(0f, 0f, 0f, 1f),
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			panelsRoot.AddChild(_menuClearFallbackRect);
		}

		_menuClearFallbackRect.AnchorLeft = 0f;
		_menuClearFallbackRect.AnchorTop = 0f;
		_menuClearFallbackRect.AnchorRight = 1f;
		_menuClearFallbackRect.AnchorBottom = 1f;
		_menuClearFallbackRect.OffsetLeft = 0f;
		_menuClearFallbackRect.OffsetTop = 0f;
		_menuClearFallbackRect.OffsetRight = 0f;
		_menuClearFallbackRect.OffsetBottom = 0f;
		_menuClearFallbackRect.Visible = false;
		panelsRoot.MoveChild(_menuClearFallbackRect, 0);
	}

	private void UpdateMenuBackgroundFallbackClear()
	{
		if (!GodotObject.IsInstanceValid(_menuClearFallbackRect))
			return;

		bool shouldRunInMenu = !_started;
		if (!shouldRunInMenu)
		{
			_menuClearFallbackRect.Visible = false;
			return;
		}

		bool titleBgVisible = IsCanvasItemVisible(GetNodeOrNull<CanvasItem>(BootBackgroundPath));
		bool mainBgVisible = IsCanvasItemVisible(_startMainPageController?.MainBackground);
		bool worldMenuBgVisible = IsCanvasItemVisible(_menuBackground);
		_menuClearFallbackRect.Visible = !(titleBgVisible || mainBgVisible || worldMenuBgVisible);
	}

	private static bool IsCanvasItemVisible(CanvasItem item)
	{
		return GodotObject.IsInstanceValid(item) && item.IsVisibleInTree();
	}
}
