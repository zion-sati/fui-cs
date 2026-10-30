using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// Immediate-mode drawing into a host canvas, handed to <see cref="CustomDrawable.OnRender"/>. Commands are batched
/// and sent to the host in one call when the render pass ends.
/// </summary>
public sealed class DrawContext
{
    private const uint OpSave = 1;
    private const uint OpRestore = 2;
    private const uint OpTranslate = 3;
    private const uint OpScale = 4;
    private const uint OpRotate = 5;
    private const uint OpClipRect = 6;
    private const uint OpClipRoundRect = 7;
    private const uint OpDrawRect = 10;
    private const uint OpDrawCircle = 11;
    private const uint OpDrawLine = 12;
    private const uint OpDrawRoundRect = 13;
    private const uint OpDrawPath = 20;
    private const uint OpDrawTextNode = 30;
    private const uint OpDrawImage = 31;
    private const uint OpDrawSvg = 32;

    private readonly uint canvas;
    private readonly List<uint> words = [];

    internal DrawContext(uint canvas)
    {
        this.canvas = canvas;
    }

    /// <summary>The batched command words not yet sent to the host.</summary>
    internal IReadOnlyList<uint> PendingWords => words;

    /// <summary>Saves the transform and clip.</summary>
    public void Save() => words.Add(OpSave);

    /// <summary>Restores the last saved transform and clip.</summary>
    public void Restore() => words.Add(OpRestore);

    /// <summary>Moves the origin.</summary>
    public void Translate(float x, float y)
    {
        words.Add(OpTranslate);
        Push(x, y);
    }

    /// <summary>Scales subsequent drawing.</summary>
    public void Scale(float scaleX, float scaleY)
    {
        words.Add(OpScale);
        Push(scaleX, scaleY);
    }

    /// <summary>Rotates subsequent drawing by <paramref name="degrees"/>.</summary>
    public void Rotate(float degrees)
    {
        words.Add(OpRotate);
        Push(degrees);
    }

    /// <summary>Clips subsequent drawing to a rectangle.</summary>
    public void ClipRect(float x, float y, float width, float height)
    {
        words.Add(OpClipRect);
        Push(x, y, width, height);
    }

    /// <summary>Clips subsequent drawing to a rectangle with round corners.</summary>
    public void ClipRoundRect(float x, float y, float width, float height, float radius) =>
        ClipRoundRect(x, y, width, height, new CornerRadius(radius));

    /// <summary>Clips subsequent drawing to a rectangle with per-corner radii.</summary>
    public void ClipRoundRect(float x, float y, float width, float height, CornerRadius radius)
    {
        words.Add(OpClipRoundRect);
        Push(x, y, width, height);
        Push(radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
    }

    /// <summary>Draws a rectangle with <paramref name="paint"/>.</summary>
    public void DrawRect(float x, float y, float width, float height, Paint paint)
    {
        words.Add(OpDrawRect);
        Push(x, y, width, height);
        Push(paint);
    }

    /// <summary>Draws a circle with <paramref name="paint"/>.</summary>
    public void DrawCircle(float centerX, float centerY, float radius, Paint paint)
    {
        words.Add(OpDrawCircle);
        Push(centerX, centerY, radius);
        Push(paint);
    }

    /// <summary>Draws a line.</summary>
    public void DrawLine(float x1, float y1, float x2, float y2, Color color, float strokeWidth)
    {
        words.Add(OpDrawLine);
        Push(x1, y1, x2, y2);
        words.Add(color.Rgba);
        Push(strokeWidth);
    }

    /// <summary>Draws a rectangle with elliptical corners.</summary>
    public void DrawRoundRect(float x, float y, float width, float height, float radiusX, float radiusY, Paint paint)
    {
        words.Add(OpDrawRoundRect);
        Push(x, y, width, height);
        Push(radiusX, radiusY);
        Push(paint);
    }

    /// <summary>Draws <paramref name="path"/>. The host draws it after the render pass, so keep it undisposed until then.</summary>
    public void DrawPath(PathGeometry path, Paint paint)
    {
        ArgumentNullException.ThrowIfNull(path);
        words.Add(OpDrawPath);
        words.Add(path.LiveId());
        Push(paint);
    }

    /// <summary>Draws a built text node's shaped text with its top-left corner at the point.</summary>
    public void DrawTextNode(Node node, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(node);
        var handle = node.Handle;
        words.Add(OpDrawTextNode);
        words.Add((uint)handle);
        words.Add((uint)(handle >> 32));
        Push(x, y);
    }

    /// <exception cref="InvalidOperationException">The layout is not ready; draw it after its <c>OnReady</c> callback.</exception>
    public void DrawTextLayout(TextLayout layout, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (!layout.IsReady)
        {
            throw new InvalidOperationException("The TextLayout is not ready; draw it after its OnReady callback.");
        }
        DrawTextNode(layout.DrawNode(), x, y);
    }

    /// <summary>Draws a loaded texture into a rectangle.</summary>
    public void DrawImage(uint textureId, float x, float y, float width, float height) =>
        DrawImage(textureId, x, y, width, height, ImageSampling.Linear);

    /// <summary>Draws a loaded texture into a rectangle, sampled as <paramref name="sampling"/>.</summary>
    public void DrawImage(uint textureId, float x, float y, float width, float height, ImageSampling sampling)
    {
        words.Add(OpDrawImage);
        words.Add(textureId);
        Push(x, y, width, height);
        words.Add((uint)sampling.Kind);
        words.Add(sampling.MaxAnisotropy);
    }

    /// <summary>Draws a loaded SVG into a rectangle.</summary>
    public void DrawSvg(uint svgId, float x, float y, float width, float height)
    {
        words.Add(OpDrawSvg);
        words.Add(svgId);
        Push(x, y, width, height);
    }

    /// <summary>Draws a texture asset into a rectangle.</summary>
    public void DrawImage(TextureAsset texture, float x, float y, float width, float height) =>
        DrawImage(texture.Id, x, y, width, height, ImageSampling.Linear);

    /// <summary>Draws a texture asset into a rectangle, sampled as <paramref name="sampling"/>.</summary>
    public void DrawImage(TextureAsset texture, float x, float y, float width, float height, ImageSampling sampling) =>
        DrawImage(texture.Id, x, y, width, height, sampling);

    /// <summary>Draws an SVG asset into a rectangle.</summary>
    public void DrawSvg(SvgAsset svg, float x, float y, float width, float height) => DrawSvg(svg.Id, x, y, width, height);

    /// <summary>Draws a bitmap's last committed pixels.</summary>
    public void DrawImage(Bitmap bitmap, float x, float y, float width, float height, ImageSampling sampling) =>
        DrawImage(bitmap.TextureId, x, y, width, height, sampling);

    /// <summary>Sends the batched commands to the host canvas.</summary>
    internal void Flush()
    {
        if (words.Count == 0)
        {
            return;
        }
        Canvas.DrawBatch(canvas, [.. words]);
        words.Clear();
    }

    private void Push(Paint paint)
    {
        words.Add(paint.FillColor.Rgba);
        words.Add(paint.StrokeColor.Rgba);
        Push(paint.StrokeWidth);
    }

    private void Push(float value) => words.Add(BitConverter.SingleToUInt32Bits(value));

    private void Push(float first, float second)
    {
        Push(first);
        Push(second);
    }

    private void Push(float first, float second, float third)
    {
        Push(first);
        Push(second);
        Push(third);
    }

    private void Push(float first, float second, float third, float fourth)
    {
        Push(first, second);
        Push(third, fourth);
    }
}
