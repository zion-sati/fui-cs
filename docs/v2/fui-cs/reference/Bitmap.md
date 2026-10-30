# Bitmap

class in `FuiCs`: `IDisposable`

An RGBA pixel buffer committed to a texture: write [`Bitmap.Pixels`](Bitmap.md), draw on [`Bitmap.Canvas`](Bitmap.md) or render text into it, then [`Bitmap.Commit`](Bitmap.md) uploads it under [`Bitmap.TextureId`](Bitmap.md) for an [`Image`](Image.md) or a draw call. Dirty rectangles limit what a commit uploads.

## Properties

- `Canvas`: A drawing surface over the bitmap's offscreen canvas. Once it is used, each [`Bitmap.Commit`](Bitmap.md) reads what was drawn back over [`Bitmap.Pixels`](Bitmap.md).
- `HasDirtyRects`: Whether regions are marked for the next upload.
- `Height`: The height in pixels.
- `IsDisposed`: Whether the bitmap has been disposed.
- `Pixels`: The RGBA pixels, row by row, four bytes each; changes show at the next [`Bitmap.Commit`](Bitmap.md).
- `TextureId`: The texture the bitmap commits to, for [`Image.TextureId`](Image.md) or a draw call.
- `Width`: The width in pixels.

## Methods

- `AddDirtyRect(int, int, int, int)`: Marks a region to upload at the next commit, clamped to the bitmap; without any, a commit uploads the whole buffer. Up to sixteen regions are kept.
- `ClearDirtyRects()`: Forgets the marked regions, so the next upload sends every pixel.
- `Commit()`: Uploads the pixels (first reading back what [`Bitmap.Canvas`](Bitmap.md) drew) to the texture; returns [`Bitmap.TextureId`](Bitmap.md).
- `Dispose()`: Releases the texture, canvas and pixels.
- `PrepareText(Node)`: Queues text shaping for a built text node before the next commit.
- `Render(Node, float, float, float)`: Renders a built text node into [`Bitmap.Pixels`](Bitmap.md); returns false when the host cannot, when the shaping [`Bitmap.PrepareText`](Bitmap.md) queued has not been committed yet, or when the node changed after its text was prepared, which discards a detached node's shaping (it is prepared again). Render once [`Bitmap.WhenTextReady`](Bitmap.md) reports the node's text ready, and again on a later frame after a false.
- `Render(TextLayout, float, float, float)`
- `WhenTextReady(Node, Action<BitmapTextReadyEventArgs>)`: Runs `handler` once the node's fonts have loaded and the page has committed, after preparing its text. Build the node after the page has loaded: mounting the page resets the native tree.
