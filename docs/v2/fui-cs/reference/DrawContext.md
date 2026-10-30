# DrawContext

class in `FuiCs`

Immediate-mode drawing into a host canvas, handed to [`CustomDrawable.OnRender`](CustomDrawable.md). Commands are batched and sent to the host in one call when the render pass ends.

## Properties

- `PendingWords`: The batched command words not yet sent to the host.

## Methods

- `ClipRect(float, float, float, float)`: Clips subsequent drawing to a rectangle.
- `ClipRoundRect(float, float, float, float, CornerRadius)`: Clips subsequent drawing to a rectangle with per-corner radii.
- `ClipRoundRect(float, float, float, float, float)`: Clips subsequent drawing to a rectangle with round corners.
- `DrawCircle(float, float, float, Paint)`: Draws a circle with `paint`.
- `DrawImage(Bitmap, float, float, float, float, ImageSampling)`: Draws a bitmap's last committed pixels.
- `DrawImage(TextureAsset, float, float, float, float, ImageSampling)`: Draws a texture asset into a rectangle, sampled as `sampling`.
- `DrawImage(TextureAsset, float, float, float, float)`: Draws a texture asset into a rectangle.
- `DrawImage(uint, float, float, float, float, ImageSampling)`: Draws a loaded texture into a rectangle, sampled as `sampling`.
- `DrawImage(uint, float, float, float, float)`: Draws a loaded texture into a rectangle.
- `DrawLine(float, float, float, float, Color, float)`: Draws a line.
- `DrawPath(PathGeometry, Paint)`: Draws `path`. The host draws it after the render pass, so keep it undisposed until then.
- `DrawRect(float, float, float, float, Paint)`: Draws a rectangle with `paint`.
- `DrawRoundRect(float, float, float, float, float, float, Paint)`: Draws a rectangle with elliptical corners.
- `DrawSvg(SvgAsset, float, float, float, float)`: Draws an SVG asset into a rectangle.
- `DrawSvg(uint, float, float, float, float)`: Draws a loaded SVG into a rectangle.
- `DrawTextLayout(TextLayout, float, float)`
- `DrawTextNode(Node, float, float)`: Draws a built text node's shaped text with its top-left corner at the point.
- `Flush()`: Sends the batched commands to the host canvas.
- `Restore()`: Restores the last saved transform and clip.
- `Rotate(float)`: Rotates subsequent drawing by `degrees`.
- `Save()`: Saves the transform and clip.
- `Scale(float, float)`: Scales subsequent drawing.
- `Translate(float, float)`: Moves the origin.
