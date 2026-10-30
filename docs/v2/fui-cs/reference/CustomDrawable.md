# CustomDrawable

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A box that draws itself with a [`DrawContext`](DrawContext.md), as WPF elements do in `OnRender`. It keeps FlexBox layout and children, but the host paints the drawing in place of the box style: background, border and shadow are not painted, so draw them in [`CustomDrawable.OnRender`](CustomDrawable.md). The host calls [`CustomDrawable.OnRender`](CustomDrawable.md) when it paints the node: once after it is first shown, then after each [`CustomDrawable.InvalidateVisual`](CustomDrawable.md); there is no continuous repaint.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `Dispatch(ulong, uint)`: Renders the custom node with this handle into the host canvas the render pass is drawing.
- `InvalidateVisual()`: Requests a repaint on the next frame. A node that is not visible on screen is not repainted.
- `OnRender(DrawContext)`: Draws the node. Coordinates are relative to its top-left corner, clipped to its rounded bounds.
