using FuiCs.Bindings;

namespace FuiCs;

/// <summary>Where an anchored popup opens relative to its anchor.</summary>
public enum PopupPlacement
{
    /// <summary>Below the anchor, or above it when only that fits.</summary>
    Auto = 0,
    /// <summary>Below the anchor.</summary>
    Bottom = 1,
    /// <summary>Above the anchor.</summary>
    Top = 2,
    /// <summary>Over the anchor, aligned to its top edge.</summary>
    Overlap = 3,
}

/// <summary>
/// Presents a surface node in a full-viewport overlay under a portal root. The overlay and surface are built
/// once and reattached on every show; showing pushes an accessibility scope that hiding removes.
/// </summary>
internal sealed class PopupPresenter : IDisposable
{
    private readonly Portal root;
    private readonly FlexBox surface;
    private readonly FlexBox? semanticScopeNode;
    private uint semanticScopeToken;
    private float edgePadding = 8.0f;
    private float anchorGap = 4.0f;
    private Color backdropColor = Color.Transparent;
    private float backgroundBlur;

    public PopupPresenter(Portal root, FlexBox surface)
        : this(root, surface, surface)
    {
    }

    public PopupPresenter(Portal root, FlexBox surface, FlexBox? semanticScopeNode)
    {
        this.root = root;
        this.surface = surface;
        this.semanticScopeNode = semanticScopeNode;
        Overlay = new FlexBox()
            .PositionAbsolute()
            .Position(0.0f, 0.0f)
            .Width(100.0f, Unit.Percent)
            .Height(100.0f, Unit.Percent)
            .Child(surface);
        surface.PositionType = PositionType.Absolute;
        ApplyBackdropStyle();
    }

    /// <summary>The full-viewport node that holds the surface and paints the backdrop.</summary>
    public FlexBox Overlay { get; }

    public bool IsOpen { get; private set; }

    public float SurfaceX { get; private set; }

    public float SurfaceY { get; private set; }

    public PopupPlacement Placement { get; set; } = PopupPlacement.Auto;

    /// <summary>The minimum distance between the surface and the viewport edges. Negative values clamp to 0.</summary>
    public float EdgePadding
    {
        get => edgePadding;
        set => edgePadding = NonNegative(value, "PopupPresenter.EdgePadding");
    }

    /// <summary>The space between an anchor and an anchored surface. Negative values clamp to 0.</summary>
    public float AnchorGap
    {
        get => anchorGap;
        set => anchorGap = NonNegative(value, "PopupPresenter.AnchorGap");
    }

    public Color BackdropColor
    {
        get => backdropColor;
        set
        {
            backdropColor = value;
            ApplyBackdropStyle();
        }
    }

    /// <summary>Blur applied behind the backdrop. Negative values clamp to 0.</summary>
    public float BackgroundBlur
    {
        get => backgroundBlur;
        set
        {
            backgroundBlur = NonNegative(value, "PopupPresenter.BackgroundBlur");
            ApplyBackdropStyle();
        }
    }

    /// <summary>Moves the overlay to cover the viewport, offsetting it by the root's own position.</summary>
    public void SyncOverlayBounds()
    {
        var rootBounds = root.IsBuilt && Ui.TryGetBounds(root.Handle, out var bounds) ? bounds : default;
        Overlay.Position = new Point(-rootBounds.X, -rootBounds.Y);
        Overlay.Width = Ui.GetViewportWidth();
        Overlay.Height = Ui.GetViewportHeight();
    }

    /// <summary>Shows the surface next to an anchor rectangle, kept inside the viewport.</summary>
    public void ShowAnchored(
        float anchorX,
        float anchorY,
        float anchorWidth,
        float anchorHeight,
        float surfaceWidth,
        float surfaceHeight,
        PopupPlacement? placement = null)
    {
        if (!root.IsBuilt)
        {
            Logger.Warn("Layout", "PopupPresenter.ShowAnchored was called before the root was built.");
            return;
        }
        SyncOverlayBounds();
        var (maxX, maxY) = MaxSurfacePosition(surfaceWidth, surfaceHeight);
        var clampedHeight = surfaceHeight > 0.0f ? surfaceHeight : 1.0f;
        var belowY = anchorY + anchorHeight + anchorGap;
        var aboveY = anchorY - clampedHeight - anchorGap;
        var panelY = (placement ?? Placement) switch
        {
            PopupPlacement.Top => aboveY,
            PopupPlacement.Overlap => anchorY,
            PopupPlacement.Auto when belowY > maxY && aboveY >= edgePadding => aboveY,
            _ => belowY,
        };
        SetSurfacePosition(Math.Max(edgePadding, Math.Min(anchorX, maxX)), Math.Max(edgePadding, Math.Min(panelY, maxY)));
        Attach();
    }

    /// <summary>Shows the surface with its top-left corner at a point, kept inside the viewport.</summary>
    public void ShowAtPoint(float x, float y, float surfaceWidth, float surfaceHeight)
    {
        if (!root.IsBuilt)
        {
            Logger.Warn("Layout", "PopupPresenter.ShowAtPoint was called before the root was built.");
            return;
        }
        SyncOverlayBounds();
        var (maxX, maxY) = MaxSurfacePosition(surfaceWidth, surfaceHeight);
        SetSurfacePosition(Math.Max(edgePadding, Math.Min(x, maxX)), Math.Max(edgePadding, Math.Min(y, maxY)));
        Attach();
    }

    /// <summary>Detaches the overlay without deleting it, so the next show reuses its native nodes.</summary>
    public void Hide()
    {
        if (!IsOpen && Overlay.Parent is null)
        {
            return;
        }
        root.RemoveChild(Overlay);
        IsOpen = false;
        if (semanticScopeToken != 0)
        {
            Ui.RemoveSemanticScope(semanticScopeToken);
            semanticScopeToken = 0;
        }
    }

    public void Dispose()
    {
        Hide();
        if (Overlay.IsBuilt)
        {
            Overlay.Dispose();
        }
    }

    private static float NonNegative(float value, string member)
    {
        if (value >= 0.0f)
        {
            return value;
        }
        Logger.Warn("Layout", member + " received a negative value; clamping to 0.");
        return 0.0f;
    }

    private (float MaxX, float MaxY) MaxSurfacePosition(float surfaceWidth, float surfaceHeight)
    {
        var width = surfaceWidth > 0.0f ? surfaceWidth : 1.0f;
        var height = surfaceHeight > 0.0f ? surfaceHeight : 1.0f;
        return (
            Math.Max(edgePadding, Ui.GetViewportWidth() - width - edgePadding),
            Math.Max(edgePadding, Ui.GetViewportHeight() - height - edgePadding));
    }

    private void Attach()
    {
        root.AddChild(Overlay);
        IsOpen = true;
        if (semanticScopeToken == 0 && semanticScopeNode is { IsBuilt: true } scopeNode)
        {
            semanticScopeToken = Ui.PushSemanticScope(scopeNode.Handle);
        }
    }

    private void ApplyBackdropStyle()
    {
        Overlay.Background = backdropColor;
        Overlay.BackgroundBlur = backgroundBlur;
    }

    private void SetSurfacePosition(float x, float y)
    {
        SurfaceX = x;
        SurfaceY = y;
        surface.Position = new Point(x, y);
    }
}
