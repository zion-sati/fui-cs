# Popup

class in `FuiCs`: [`Portal`](Portal.md)

A popup surface shown over the page at a point or next to an anchor, over an optional backdrop. Content added with `Child` goes on the surface; a backdrop click hides the popup unless [`Popup.DismissOnBackdropClick`](Popup.md) is off.

Fluent members: [`PopupExtensions`](PopupExtensions.md).

## Properties

- `AnchorGap`: The space between an anchor and the popup.
- `BackdropColor`: The veil over the page while the popup is open.
- `BackgroundBlur`: Blur behind the backdrop; [`Popup.Surface`](Popup.md) has its own `BackgroundBlur` for the panel.
- `DismissOnBackdropClick`: Closes the popup when the backdrop is clicked.
- `EdgePadding`: The least distance between the popup and the viewport's edges.
- `IsOpen`: Whether the popup is showing.
- `Placement`: Where the popup opens relative to its anchor.
- `Surface`: The panel that holds the popup's content.

## Methods

- `AddContent(Node)`: Adds `node` to the container's content.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `Hide()`: Closes the popup.
- `SetContent(ReadOnlySpan<Node>)`: Replaces the content with `nodes`.
- `ShowAnchored(float, float, float, float, float, float)`: Sizes the surface and shows it next to an anchor rectangle.
- `ShowAtPoint(float, float, float, float)`: Sizes the surface and shows it at a point.

## Events

- `Closed`: Raised when the popup hides, by [`Popup.Hide`](Popup.md) or a backdrop click.
- `Opened`: Raised when the popup shows after being hidden.
