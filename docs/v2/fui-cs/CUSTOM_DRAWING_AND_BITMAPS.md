# FUI-CS Custom Drawing and Bitmaps

Two ways to draw what the built-in nodes do not: a `CustomDrawable` paints
itself each time it is invalidated, and a `Bitmap` holds pixels you write or
draw once and show as an image.

## CustomDrawable

Subclass `CustomDrawable` and override `OnRender(DrawContext)`, as WPF
elements override `OnRender`. The node keeps `FlexBox` layout and children,
but its drawing replaces the box style: background, border and shadow are not
painted, so draw them yourself.

```csharp
sealed class Meter : CustomDrawable
{
    private float level;

    public Meter() => this.Width(240.0f).Height(24.0f);

    public float Level
    {
        get => level;
        set
        {
            level = Math.Clamp(value, 0.0f, 1.0f);
            InvalidateVisual();
        }
    }

    protected override void OnRender(DrawContext context)
    {
        var bounds = GetBounds();
        context.DrawRoundRect(0.0f, 0.0f, bounds.Width, bounds.Height, 6.0f, 6.0f, Paint.Fill(Theme.Active.Value.Colors.ScrollbarTrack));
        context.DrawRoundRect(0.0f, 0.0f, bounds.Width * level, bounds.Height, 6.0f, 6.0f, Paint.Fill(Theme.Active.Value.Colors.Accent));
    }
}
```

The host calls `OnRender` when it paints the node: once after the node is
first shown, then after each `InvalidateVisual()`. There is no continuous
repaint; an animation invalidates on each step, for example from
`Animation.AnimateFloat` or a `PeriodicTimer`. A node off screen is not
repainted. Coordinates are relative to the node's top-left corner, and the
drawing is clipped to the node's rounded bounds.

## DrawContext

| Group | Members |
|---|---|
| State | `Save()`, `Restore()`, `Translate(x, y)`, `Scale(sx, sy)`, `Rotate(degrees)` |
| Clipping | `ClipRect`, `ClipRoundRect` (one radius or a `CornerRadius`) |
| Shapes | `DrawRect`, `DrawRoundRect`, `DrawCircle`, `DrawLine(x1, y1, x2, y2, color, width)`, `DrawPath(path, paint)` |
| Text | `DrawTextLayout(layout, x, y)`, `DrawTextNode(node, x, y)` |
| Images | `DrawImage(texture, x, y, w, h)` with a `TextureAsset` or a texture id, optionally with an `ImageSampling` (`Linear`, `Nearest`, ...); `DrawImage(bitmap, x, y, w, h, sampling)`; `DrawSvg(svg, x, y, w, h)` with an `SvgAsset` or id |

A `Paint` fills, strokes or both: `Paint.Fill(color)`,
`Paint.Stroke(color, width)`, `Paint.FilledStroke(fill, stroke, width)`.

`PathGeometry` builds a path with `MoveTo`, `LineTo`, `QuadTo`, `CubicTo`,
`AddRect`, `AddCircle` and `Close`. It holds a host path, so create it once,
reuse it across renders and dispose it with its owner.

## Text in a drawing

A `TextLayout` shapes text outside the tree. It is ready once the page has
loaded and its fonts are available; invalidate from `OnReady` and draw only
while `IsReady`:

```csharp
sealed class Chart : CustomDrawable
{
    private readonly TextLayout title;

    public Chart()
    {
        this.Width(300.0f).Height(200.0f);
        title = new TextLayout("Throughput").FontSize(13.0f).Width(180.0f).Height(24.0f)
            .OnReady(_ => InvalidateVisual());
    }

    protected override void OnRender(DrawContext context)
    {
        if (title.IsReady)
        {
            context.DrawTextLayout(title, 16.0f, 24.0f);
        }
    }
}
```

`DynamicTextLayout` suits short text that changes often, such as counters and
clocks: the host prepares the glyphs of a fixed `Charset` once, so new text is
ready at once. `DynamicTextLayout.Numeric()` holds digits, `TrySetValue`
formats a number with `Precision`, `Prefix` and `Suffix`, and `TrySetText`
sets text. Text outside the charset is shaped normally, or rejected under
`CharsetOverflow = DynamicTextOverflow.Reject`.

## Bitmap

A `Bitmap` is an RGBA pixel buffer committed to a texture:

- write `Pixels` (four bytes per pixel, row by row) directly;
- or draw on its `Canvas`, a `DrawContext` over an offscreen canvas;
- then `Commit()` uploads it and returns its `TextureId`.

Show it with `new Image(bitmap.TextureId)` or draw it with
`context.DrawImage(bitmap, x, y, width, height, sampling)`. `AddDirtyRect`
limits an upload to changed regions (up to sixteen; without any, a commit
uploads everything). Once the `Canvas` has been used, each commit first reads
the drawing back into `Pixels`. Dispose the bitmap to release its texture.

```csharp
var bitmap = new Bitmap(256, 256);
var pixels = bitmap.Pixels;
for (var i = 0; i < pixels.Length; i += 4)
{
    pixels[i] = (byte)(i / 4 % 256);  // R
    pixels[i + 1] = 0x40;             // G
    pixels[i + 2] = 0xC0;             // B
    pixels[i + 3] = 0xFF;             // A
}
var image = new Image(bitmap.Commit()).Width(256.0f).Height(256.0f);
```

## Rendering text into a bitmap

`Render(node)` draws a built text node into the pixels, and `Render(layout)` a
ready `TextLayout`, each at a position and scale. A node's text has to be
shaped first:

1. build the node after the page has loaded (mounting resets the native tree);
2. call `Bitmap.WhenTextReady(node, handler)`, which prepares its text and runs
   the handler once its fonts have loaded and the page has committed;
3. call `Render` from the handler, or from a later `OnRender`, then `Commit`.

`Render` returns false when the shaping has not been committed yet, or when
the node changed after it was prepared (it is prepared again); render again on
a later frame. The demo's immediate drawing route
(`demo/ImmediateDrawing/`) paints into a bitmap and draws charts with live
labels this way.
