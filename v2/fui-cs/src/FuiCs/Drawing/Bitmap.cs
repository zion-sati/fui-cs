using FuiCs.Bindings;

namespace FuiCs;

/// <summary>The node whose text <see cref="Bitmap.WhenTextReady"/> reports ready to render.</summary>
public sealed class BitmapTextReadyEventArgs(Node node) : EventArgs
{
    /// <summary>The text node whose shaping is ready.</summary>
    public Node Node { get; } = node;
}

/// <summary>
/// An RGBA pixel buffer committed to a texture: write <see cref="Pixels"/>, draw on <see cref="Canvas"/> or render
/// text into it, then <see cref="Commit"/> uploads it under <see cref="TextureId"/> for an <see cref="Image"/> or a
/// draw call. Dirty rectangles limit what a commit uploads.
/// </summary>
public sealed class Bitmap : IDisposable
{
    private const int MaxDirtyRects = 16;

    private readonly List<(int X, int Y, int Width, int Height)> dirtyRects = [];
    private byte[] pixels;
    private uint offscreenId;
    private DrawContext? canvas;
    private bool canvasUsed;

    /// <exception cref="ArgumentOutOfRangeException">A side is not positive, or the pixels would not fit one array.</exception>
    public Bitmap(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if ((long)width * height * 4 > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The bitmap's pixels would not fit one array.");
        }
        Width = width;
        Height = height;
        TextureId = AssetRegistry.Textures.AllocateId();
        pixels = new byte[width * height * 4];
        offscreenId = Runtime.Abi.HostCanvasCreateOffscreen((uint)width, (uint)height);
    }

    /// <summary>The width in pixels.</summary>
    public int Width { get; }

    /// <summary>The height in pixels.</summary>
    public int Height { get; }

    /// <summary>The texture the bitmap commits to, for <see cref="Image.TextureId"/> or a draw call.</summary>
    public uint TextureId { get; }

    /// <summary>Whether the bitmap has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>The RGBA pixels, row by row, four bytes each; changes show at the next <see cref="Commit"/>.</summary>
    public byte[] Pixels
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pixels;
        }
    }

    /// <summary>
    /// A drawing surface over the bitmap's offscreen canvas. Once it is used, each <see cref="Commit"/> reads what was
    /// drawn back over <see cref="Pixels"/>.
    /// </summary>
    public DrawContext Canvas
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            canvasUsed = true;
            return canvas ??= new DrawContext(Runtime.Abi.HostGetOffscreenCanvasToken(offscreenId));
        }
    }

    /// <summary>Whether regions are marked for the next upload.</summary>
    public bool HasDirtyRects => dirtyRects.Count > 0;

    /// <summary>
    /// Renders a built text node into <see cref="Pixels"/>; returns false when the host cannot, when the shaping
    /// <see cref="PrepareText"/> queued has not been committed yet, or when the node changed after its text was
    /// prepared, which discards a detached node's shaping (it is prepared again). Render once
    /// <see cref="WhenTextReady"/> reports the node's text ready, and again on a later frame after a false.
    /// </summary>
    public bool Render(Node node, float x = 0.0f, float y = 0.0f, float scale = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(node);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (!node.IsBuilt)
        {
            return false;
        }
        if (node.TextPreparedAtMutation is { } prepared && prepared != node.RetainedMutationCount)
        {
            PrepareText(node);
            return false;
        }
        if (node.TextPreparedAtMutation is not null && node.TextPreparedAtCommit == FrameScheduler.CommitCount)
        {
            return false;
        }
        var rendered = Runtime.Abi.UiRenderNodeToRgba(node.Handle, (uint)Width, (uint)Height, scale, x, y);
        if (rendered.Length != pixels.Length)
        {
            return false;
        }
        rendered.CopyTo(pixels, 0);
        return true;
    }

    /// <exception cref="InvalidOperationException">The layout is not ready; render it after its <c>OnReady</c> callback.</exception>
    public bool Render(TextLayout layout, float x = 0.0f, float y = 0.0f, float scale = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (!layout.IsReady)
        {
            throw new InvalidOperationException("The TextLayout is not ready; render it after its OnReady callback.");
        }
        return Render(layout.DrawNode(), x, y, scale);
    }

    /// <summary>Queues text shaping for a built text node before the next commit.</summary>
    public static void PrepareText(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.IsBuilt)
        {
            Ui.PrepareNode(node.Handle);
            node.TextPreparedAtMutation = node.RetainedMutationCount;
            node.TextPreparedAtCommit = FrameScheduler.CommitCount;
            FrameScheduler.MarkNeedsCommit();
        }
    }

    /// <summary>
    /// Runs <paramref name="handler"/> once the node's fonts have loaded and the page has committed, after preparing
    /// its text. Build the node after the page has loaded: mounting the page resets the native tree.
    /// </summary>
    public static void WhenTextReady(Node node, Action<BitmapTextReadyEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(handler);
        FontFace.WhenFontsLoaded(node.RequiredFontIds(), _ => FrameScheduler.OnLoaded(_ =>
        {
            PrepareText(node);
            handler(new BitmapTextReadyEventArgs(node));
        }));
    }

    /// <summary>
    /// Marks a region to upload at the next commit, clamped to the bitmap; without any, a commit uploads the whole
    /// buffer. Up to sixteen regions are kept.
    /// </summary>
    public void AddDirtyRect(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0 || x < 0 || y < 0 || x >= Width || y >= Height || dirtyRects.Count >= MaxDirtyRects)
        {
            return;
        }
        dirtyRects.Add((x, y, Math.Min(width, Width - x), Math.Min(height, Height - y)));
    }

    /// <summary>Forgets the marked regions, so the next upload sends every pixel.</summary>
    public void ClearDirtyRects() => dirtyRects.Clear();

    /// <summary>Uploads the pixels (first reading back what <see cref="Canvas"/> drew) to the texture; returns <see cref="TextureId"/>.</summary>
    public uint Commit()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (canvasUsed)
        {
            canvas?.Flush();
            Runtime.Abi.HostReadOffscreenPixels(offscreenId, (uint)Width, (uint)Height).CopyTo(pixels, 0);
        }
        if (dirtyRects.Count > 0)
        {
            foreach (var (x, y, width, height) in dirtyRects)
            {
                var region = new byte[width * height * 4];
                for (var row = 0; row < height; ++row)
                {
                    Array.Copy(pixels, (((y + row) * Width) + x) * 4, region, row * width * 4, width * 4);
                }
                Runtime.Abi.HostBitmapCommitDirty(TextureId, region, (uint)Width, (uint)Height, (uint)x, (uint)y, (uint)width, (uint)height);
            }
            dirtyRects.Clear();
        }
        else
        {
            Runtime.Abi.HostBitmapCommit(TextureId, pixels, (uint)Width, (uint)Height);
        }
        AssetRegistry.Textures.MarkLoaded(TextureId, Width, Height);
        return TextureId;
    }

    /// <summary>Releases the texture, canvas and pixels.</summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;
        if (offscreenId != 0)
        {
            Runtime.Abi.HostCanvasDestroyOffscreen(offscreenId);
        }
        Runtime.Abi.HostBitmapRelease(TextureId);
        pixels = [];
        canvas = null;
    }
}
