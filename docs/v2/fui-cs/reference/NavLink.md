# NavLink

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A link around any content. A primary click, a tap or Enter navigates to [`NavLink.Href`](NavLink.md); a middle click, the platform's primary shortcut modifier (Ctrl or ⌘) or [`NavLink.OpenInNewTab`](NavLink.md) opens it in a new tab. While the link is hovered or focused, the host previews its URL, as browsers do.

Fluent members: [`NavLinkExtensions`](NavLinkExtensions.md).

## Properties

- `Href`: The link's target.
- `InteractionState`: Whether the link is hovered, pressed, focused and enabled, for styling its content.
- `OpenInNewTab`: Whether every activation opens a new tab.

## Methods

- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleBubbledPointerEvent(PointerEventArgs)`: Hovering and pressing the link's content count as the link's, as they do for a browser's links.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `PinPreviewForContextMenu()`: Keeps the URL preview showing while a context menu for the link is open.

## Events

- `InteractionStateChanged`: Raised when hover, press, focus or enabled state changes, to restyle the link's content.
- `Navigate`: Raised after the link navigates.
