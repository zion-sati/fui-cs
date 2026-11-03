using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A scroll view scrolling, or its content or viewport changing size.</summary>
public sealed class ScrollChangedEventArgs(
    float horizontalOffset,
    float verticalOffset,
    float extentWidth,
    float extentHeight,
    float viewportWidth,
    float viewportHeight) : EventArgs
{
    /// <summary>The horizontal offset.</summary>
    public float HorizontalOffset { get; } = horizontalOffset;

    /// <summary>The vertical offset.</summary>
    public float VerticalOffset { get; } = verticalOffset;

    /// <summary>The content's width.</summary>
    public float ExtentWidth { get; } = extentWidth;

    /// <summary>The content's height.</summary>
    public float ExtentHeight { get; } = extentHeight;

    /// <summary>The viewport's width.</summary>
    public float ViewportWidth { get; } = viewportWidth;

    /// <summary>The viewport's height.</summary>
    public float ViewportHeight { get; } = viewportHeight;
}

/// <summary>
/// A native scroll viewport over its content. The host scrolls it with wheel, touch and momentum and reports
/// every change; <see cref="ScrollTo"/> moves it from code. Offsets, extent and viewport follow WPF's ScrollViewer.
/// </summary>
public class ScrollView : Node, ISizable, IContentContainer
{
    /// <summary>How far, in pixels, the host may settle from a programmatic offset and still acknowledge it.</summary>
    private const float ProgrammaticScrollTolerance = 0.5f;

    private readonly NodeSize size;
    private float flexBasis = float.NaN;
    private bool horizontalScrollEnabled = true;
    private bool verticalScrollEnabled = true;
    private float? friction;
    private bool smoothScrolling = true;
    private bool wheelLatching = true;
    private bool hasOffset;
    private bool hasScrollContentSize;
    private float? scrollContentWidth;
    private float? scrollContentHeight;
    private (float X, float Y)? pendingProgrammaticOffset;
    private Animation? offsetTransition;
    private bool persistedRestorePending;

    static ScrollView() => KeyboardScrollTracker.Install();

    /// <summary>Creates a scroll view that scrolls vertically.</summary>
    public ScrollView()
    {
        size = new NodeSize(this);
        // Blank areas of the viewport start scroll gestures, not only its interactive descendants.
        RequireInteractive();
    }

    /// <summary>Raised when the offset, extent or viewport changes, whether the host or code scrolled.</summary>
    public event Action<ScrollChangedEventArgs>? ScrollChanged;

    /// <summary>The view's offsets and sizes as signals; a <see cref="ScrollBox"/> shares it with its scroll bars.</summary>
    public ScrollState ScrollState { get; private set; } = new();

    /// <summary>When set, the offset persists under this node's id and setting instead of the view's own.</summary>
    internal Node? PersistenceOwner { get; set; }

    /// <summary>The horizontal offset, in pixels.</summary>
    public float HorizontalOffset { get; private set; }

    /// <summary>The vertical offset, in pixels.</summary>
    public float VerticalOffset { get; private set; }

    /// <summary>The scrollable content width the host last reported, or the explicit scroll content width.</summary>
    public float ExtentWidth { get; private set; }

    /// <summary>The content's height.</summary>
    public float ExtentHeight { get; private set; }

    /// <summary>The visible width the host last reported, or a fixed pixel <see cref="Width"/> before then.</summary>
    public float ViewportWidth { get; private set; }

    /// <summary>The viewport's height.</summary>
    public float ViewportHeight { get; private set; }

    /// <summary>Whether the view scrolls sideways.</summary>
    public bool IsHorizontalScrollEnabled
    {
        get => horizontalScrollEnabled;
        set
        {
            horizontalScrollEnabled = value;
            SendScrollEnabled();
        }
    }

    /// <summary>Whether the view scrolls vertically.</summary>
    public bool IsVerticalScrollEnabled
    {
        get => verticalScrollEnabled;
        set
        {
            verticalScrollEnabled = value;
            SendScrollEnabled();
        }
    }

    /// <summary>Momentum friction; null keeps the host default.</summary>
    public float? Friction
    {
        get => friction;
        set
        {
            friction = value;
            if (IsBuilt && value is { } amount)
            {
                Ui.SetScrollFriction(Handle, amount);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>
    /// Whether the scroll offset is saved with the page's UI state and restored when the user returns to the history
    /// entry. Needs a <see cref="Node.NodeId"/>. On by default.
    /// </summary>
    public bool PersistScroll
    {
        get;
        set
        {
            field = value;
            if (!value)
            {
                persistedRestorePending = false;
            }
        }
    } = true;

    /// <summary>Animates wheel and keyboard scrolling toward its target instead of jumping.</summary>
    public bool SmoothScrolling
    {
        get => smoothScrolling;
        set
        {
            smoothScrolling = value;
            if (IsBuilt)
            {
                Ui.SetSmoothScrolling(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>
    /// Whether a mouse-wheel run that starts here stays here at the edges until the wheel pauses (true by default), as
    /// browsers latch wheel scrolling. When false, the rest of the run scrolls the nearest ancestor that can move.
    /// </summary>
    public bool WheelLatching
    {
        get => wheelLatching;
        set
        {
            wheelLatching = value;
            if (IsBuilt)
            {
                Ui.SetScrollWheelLatching(Handle, value);
            }
        }
    }

    /// <summary>The initial main-axis size before flexing; NaN until set.</summary>
    public float FlexBasis
    {
        get => flexBasis;
        set
        {
            flexBasis = value;
            if (IsBuilt)
            {
                Ui.SetFlexBasis(Handle, value);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <inheritdoc/>
    public Length Width
    {
        get => size.Width;
        set
        {
            size.Width = value;
            if (value.Unit == Unit.Pixel)
            {
                UpdateMetrics(ExtentWidth, ExtentHeight, value.Value, ViewportHeight);
            }
        }
    }

    /// <inheritdoc/>
    public Length Height
    {
        get => size.Height;
        set
        {
            size.Height = value;
            if (value.Unit == Unit.Pixel)
            {
                UpdateMetrics(ExtentWidth, ExtentHeight, ViewportWidth, value.Value);
            }
        }
    }

    /// <inheritdoc/>
    public bool FillWidth
    {
        get => size.FillWidth;
        set => size.FillWidth = value;
    }

    /// <inheritdoc/>
    public bool FillHeight
    {
        get => size.FillHeight;
        set => size.FillHeight = value;
    }

    /// <inheritdoc/>
    public float? FillWidthPercent
    {
        get => size.FillWidthPercent;
        set => size.FillWidthPercent = value;
    }

    /// <inheritdoc/>
    public float? FillHeightPercent
    {
        get => size.FillHeightPercent;
        set => size.FillHeightPercent = value;
    }

    /// <inheritdoc/>
    public Length MinWidth
    {
        get => size.MinWidth;
        set => size.MinWidth = value;
    }

    /// <inheritdoc/>
    public Length MaxWidth
    {
        get => size.MaxWidth;
        set => size.MaxWidth = value;
    }

    /// <inheritdoc/>
    public Length MinHeight
    {
        get => size.MinHeight;
        set => size.MinHeight = value;
    }

    /// <inheritdoc/>
    public Length MaxHeight
    {
        get => size.MaxHeight;
        set => size.MaxHeight = value;
    }

    /// <summary>Adds a node to the scrolled content.</summary>
    public void AddContent(Node node) => AddChild(node);

    /// <summary>Replaces the scrolled content.</summary>
    public void SetContent(params ReadOnlySpan<Node> nodes) => ReplaceChildren(nodes);

    /// <summary>
    /// Sets the scrollable content size instead of measuring the content, for content that is laid out lazily;
    /// a null dimension is measured.
    /// </summary>
    public void SetScrollContentSize(float? width, float? height)
    {
        hasScrollContentSize = true;
        scrollContentWidth = width;
        scrollContentHeight = height;
        // Only the content size changes; the viewport stays what the host or the state's owner last reported.
        if (width is { } contentWidth)
        {
            ExtentWidth = contentWidth;
            ScrollState.ContentWidth.Value = contentWidth;
        }
        if (height is { } contentHeight)
        {
            ExtentHeight = contentHeight;
            ScrollState.ContentHeight.Value = contentHeight;
        }
        if (IsBuilt)
        {
            Ui.SetScrollContentSize(Handle, width ?? -1.0f, height ?? -1.0f);
            NotifyRetainedMutation();
        }
    }

    /// <summary>Which property changes animate; only <see cref="NodeTransitions.ScrollOffset"/> applies to a scroll view.</summary>
    public NodeTransitions? Transitions { get; set; }

    /// <summary>Scrolls to the offset at once, taking over from any momentum scroll or animation in progress.</summary>
    public void ScrollTo(float horizontalOffset, float verticalOffset)
    {
        offsetTransition?.Cancel();
        if (IsBuilt && (HorizontalOffset != horizontalOffset || VerticalOffset != verticalOffset))
        {
            Ui.ClearMomentumScroll();
        }
        ApplyOffset(horizontalOffset, verticalOffset);
    }

    /// <summary>Animates to the offset with <paramref name="timing"/>; before the view is built, it is the initial offset.</summary>
    public void ScrollToAnimated(float horizontalOffset, float verticalOffset, AnimationTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);
        offsetTransition?.Cancel();
        if (!IsBuilt || (HorizontalOffset == horizontalOffset && VerticalOffset == verticalOffset))
        {
            ApplyOffset(horizontalOffset, verticalOffset);
            return;
        }
        Ui.ClearMomentumScroll();
        AnimateOffset(horizontalOffset, verticalOffset, timing);
    }

    /// <summary>
    /// Sets the offset, animating it on a built view with a <see cref="NodeTransitions.ScrollOffset"/> timing;
    /// before the view is built, it is the initial offset.
    /// </summary>
    public void SetScrollOffset(float horizontalOffset, float verticalOffset)
    {
        if (Transitions?.ScrollOffset is { } timing)
        {
            ScrollToAnimated(horizontalOffset, verticalOffset, timing);
            return;
        }
        ScrollTo(horizontalOffset, verticalOffset);
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        Handle = Ui.CreateNode(NodeType.ScrollView);
        ApplyNodeMetadata();
        FinishBuild();
        KeyboardScrollTracker.Register(this);
        Ui.SetScrollEnabled(Handle, horizontalScrollEnabled, verticalScrollEnabled);
        Ui.SetSmoothScrolling(Handle, smoothScrolling);
        Ui.SetScrollWheelLatching(Handle, wheelLatching);
        if (friction is { } amount)
        {
            Ui.SetScrollFriction(Handle, amount);
        }
        if (hasScrollContentSize)
        {
            Ui.SetScrollContentSize(Handle, scrollContentWidth ?? -1.0f, scrollContentHeight ?? -1.0f);
        }
        size.Apply(Handle);
        if (!float.IsNaN(flexBasis))
        {
            Ui.SetFlexBasis(Handle, flexBasis);
        }
        if (hasOffset)
        {
            pendingProgrammaticOffset = (HorizontalOffset, VerticalOffset);
            Ui.SetScrollOffset(Handle, HorizontalOffset, VerticalOffset);
        }
        BuildChildren();
        return Handle;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        offsetTransition?.Cancel();
        KeyboardScrollTracker.Unregister(this);
        base.Dispose();
    }

    internal override float DebugMainAxisPercentValue(bool isHorizontal) => size.MainAxisPercent(isHorizontal);

    /// <summary>Moves the offset for the framework's own scrolling (keyboard, scroll bars) without clearing momentum.</summary>
    internal void SetRuntimeScrollOffset(float horizontalOffset, float verticalOffset)
    {
        offsetTransition?.Cancel();
        ApplyOffset(horizontalOffset, verticalOffset);
    }

    /// <inheritdoc/>
    protected internal override void HandleScroll(
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight)
    {
        var metricsChanged = SetMetrics(contentWidth, contentHeight, viewportWidth, viewportHeight);
        if (TryRestorePersistedOffset())
        {
            return;
        }
        if (pendingProgrammaticOffset is { } pending)
        {
            pendingProgrammaticOffset = null;
            if (MathF.Abs(offsetX - pending.X) <= ProgrammaticScrollTolerance
                && MathF.Abs(offsetY - pending.Y) <= ProgrammaticScrollTolerance)
            {
                if (metricsChanged)
                {
                    RaiseScrollChanged();
                }
                return;
            }
        }
        // The user scrolled: their scroll wins over an animation in progress.
        offsetTransition?.Cancel();
        var offsetChanged = SetOffset(offsetX, offsetY);
        if (metricsChanged || offsetChanged)
        {
            RaiseScrollChanged();
        }
    }

    /// <inheritdoc/>
    protected override void CapturePersistedState()
    {
        base.CapturePersistedState();
        if (PersistScroll && !string.IsNullOrEmpty(PersistenceId))
        {
            PersistedUiState.StoreScrollOffset(PersistenceId, HorizontalOffset, VerticalOffset);
        }
    }

    /// <inheritdoc/>
    protected override void RestorePersistedState()
    {
        base.RestorePersistedState();
        if (!PersistScroll)
        {
            return;
        }
        persistedRestorePending = true;
        TryRestorePersistedOffset();
    }

    /// <summary>
    /// Restores a saved offset once the host has reported a viewport and enough content to scroll to it, clamped
    /// to the content; returns whether it did.
    /// </summary>
    private bool TryRestorePersistedOffset()
    {
        if (!persistedRestorePending || !PersistScroll)
        {
            return false;
        }
        if (string.IsNullOrEmpty(PersistenceId) || !PersistedUiState.TryLoadScrollOffset(PersistenceId, out var x, out var y))
        {
            persistedRestorePending = false;
            return false;
        }
        if (!CanRestoreAxis(x, ExtentWidth, ViewportWidth, horizontalScrollEnabled)
            || !CanRestoreAxis(y, ExtentHeight, ViewportHeight, verticalScrollEnabled))
        {
            return false;
        }
        persistedRestorePending = false;
        SetRuntimeScrollOffset(
            ClampAxis(x, ExtentWidth, ViewportWidth, horizontalScrollEnabled),
            ClampAxis(y, ExtentHeight, ViewportHeight, verticalScrollEnabled));
        return true;
    }

    private string? PersistenceId => (PersistenceOwner ?? this).NodeId;

    private static bool CanRestoreAxis(float offset, float extent, float viewport, bool enabled) =>
        !enabled || offset == 0.0f || (viewport > 0.0f && extent > viewport);

    private static float ClampAxis(float offset, float extent, float viewport, bool enabled) =>
        enabled ? Math.Clamp(offset, 0.0f, Math.Max(0.0f, extent - viewport)) : 0.0f;

    private void AnimateOffset(float x, float y, AnimationTiming timing)
    {
        var (fromX, fromY) = (HorizontalOffset, VerticalOffset);
        offsetTransition = Animation.AnimateFloat(0.0f, 1.0f, timing, progress =>
            ApplyOffset(fromX + ((x - fromX) * progress), fromY + ((y - fromY) * progress)));
    }

    private void ApplyOffset(float x, float y)
    {
        var changed = SetOffset(x, y);
        if (IsBuilt)
        {
            pendingProgrammaticOffset = (x, y);
            Ui.SetScrollOffset(Handle, x, y);
            NotifyRetainedMutation();
        }
        if (changed)
        {
            RaiseScrollChanged();
        }
    }

    private bool SetOffset(float x, float y)
    {
        hasOffset = true;
        if (HorizontalOffset == x && VerticalOffset == y)
        {
            return false;
        }
        HorizontalOffset = x;
        VerticalOffset = y;
        ScrollState.OffsetX.Value = x;
        ScrollState.OffsetY.Value = y;
        return true;
    }

    private bool SetMetrics(float extentWidth, float extentHeight, float viewportWidth, float viewportHeight)
    {
        if (ExtentWidth == extentWidth && ExtentHeight == extentHeight && ViewportWidth == viewportWidth && ViewportHeight == viewportHeight)
        {
            return false;
        }
        (ExtentWidth, ExtentHeight, ViewportWidth, ViewportHeight) = (extentWidth, extentHeight, viewportWidth, viewportHeight);
        ScrollState.ContentWidth.Value = extentWidth;
        ScrollState.ContentHeight.Value = extentHeight;
        ScrollState.ViewportWidth.Value = viewportWidth;
        ScrollState.ViewportHeight.Value = viewportHeight;
        return true;
    }

    /// <summary>Makes <paramref name="state"/> the view's scroll state and brings it up to date.</summary>
    internal void BindScrollState(ScrollState state)
    {
        ScrollState = state;
        state.OffsetX.Value = HorizontalOffset;
        state.OffsetY.Value = VerticalOffset;
        state.ContentWidth.Value = ExtentWidth;
        state.ContentHeight.Value = ExtentHeight;
        state.ViewportWidth.Value = ViewportWidth;
        state.ViewportHeight.Value = ViewportHeight;
    }

    private void UpdateMetrics(float extentWidth, float extentHeight, float viewportWidth, float viewportHeight)
    {
        if (SetMetrics(extentWidth, extentHeight, viewportWidth, viewportHeight))
        {
            RaiseScrollChanged();
        }
    }

    private void RaiseScrollChanged() =>
        ScrollChanged?.Invoke(new ScrollChangedEventArgs(HorizontalOffset, VerticalOffset, ExtentWidth, ExtentHeight, ViewportWidth, ViewportHeight));

    private void SendScrollEnabled()
    {
        if (IsBuilt)
        {
            Ui.SetScrollEnabled(Handle, horizontalScrollEnabled, verticalScrollEnabled);
            NotifyRetainedMutation();
        }
    }
}
