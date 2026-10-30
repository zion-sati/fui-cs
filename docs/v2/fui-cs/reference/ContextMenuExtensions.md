# ContextMenuExtensions

class in `FuiCs`

Fluent configuration for [`ContextMenu`](ContextMenu.md) and nodes' context menus.

## Methods

- `IsContextMenuEnabled(bool)`: Sets [`Node.IsContextMenuEnabled`](Node.md).
- `ItemHeight(float)`: Sets [`ContextMenu.ItemHeight`](ContextMenu.md).
- `ItemHoverColor(Color)`: Sets [`ContextMenu.ItemHoverColor`](ContextMenu.md).
- `Items(ReadOnlySpan<MenuItem>)`: Replaces the menu's items.
- `MenuWidth(float)`: Sets [`ContextMenu.MenuWidth`](ContextMenu.md).
- `OnContextMenuOpening(Action<ContextMenuEventArgs>)`: Adds a handler to [`Node.ContextMenuOpening`](Node.md).
- `OnVisibilityChanged(Action<VisibilityChangedEventArgs>)`: Adds a handler to [`ContextMenu.VisibilityChanged`](ContextMenu.md).
- `PanelColor(Color)`: Sets [`ContextMenu.PanelColor`](ContextMenu.md).
