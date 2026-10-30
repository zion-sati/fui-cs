# ContextMenu

class in `FuiCs`: [`Portal`](Portal.md)

A menu of [`ContextMenu.Items`](ContextMenu.md) shown at a point over the page. Choosing an item runs it and hides the menu; Escape, or a press outside the menu, hides it. The application shows a built-in menu on right-click; a node can replace it through [`Node.ContextMenuOpening`](Node.md). Mount the menu before showing it.

Fluent members: [`ContextMenuExtensions`](ContextMenuExtensions.md).

## Properties

- `Active`: The open menu, if any.
- `BackdropColor`: The backdrop over the page while the menu is open.
- `BackgroundBlur`: The blur of the page behind the backdrop.
- `IsOpen`: Whether the menu is showing.
- `ItemColor`: The items' fill; null follows the theme.
- `ItemCornerRadius`: The items' corner radius; null follows the theme.
- `ItemFontFamily`: The items' font family; null follows the theme.
- `ItemFontSize`: The items' font size; null follows the theme.
- `ItemFontStyle`: The items' font style; upright when null.
- `ItemFontWeight`: The items' font weight; regular when null.
- `ItemHeight`: The height of each item; null follows the theme.
- `ItemHoverColor`: The fill of the item under the pointer; null follows the theme.
- `ItemPadding`: The items' padding; null follows the theme.
- `Items`: The items the next [`ContextMenu.Show`](ContextMenu.md) displays.
- `ItemTextColor`: The items' text color; null follows the theme.
- `MenuWidth`: The menu's width in pixels.
- `PanelBackgroundBlur`: The blur of the page behind the panel.
- `PanelBorder`: The panel's border; null follows the theme.
- `PanelColor`: The panel's fill; null follows the theme.
- `PanelCornerRadius`: The panel's corner radius; null follows the theme.
- `PanelShadow`: The panel's shadow; null follows the theme.
- `SeparatorColor`: The dividers' color; null follows the theme.

## Methods

- `DismissForOutsidePointerDown(float, float)`: Hides the open menu when a press lands outside its panel; returns whether it did.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleOverlayPointerUp(PointerEventArgs)`: A primary release on the backdrop runs the item under it or hides the menu; other releases, such as the one of the right-click that opened the menu, are ignored.
- `Hide()`: Hides the menu.
- `HideActive()`: Hides the open menu, if any.
- `Show(Node, float, float)`: Shows [`ContextMenu.Items`](ContextMenu.md) at `x`, `y`, relative to `target` or to the page when it is null, kept inside the viewport.

## Events

- `VisibilityChanged`: Raised when the menu shows and hides.
