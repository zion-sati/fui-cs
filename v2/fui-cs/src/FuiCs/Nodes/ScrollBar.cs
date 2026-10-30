namespace FuiCs;

/// <summary>When a <see cref="ScrollBox"/> shows a scroll bar, as WPF's ScrollViewer does.</summary>
public enum ScrollBarVisibility
{
    /// <summary>Shown while the content is larger than the viewport on the bar's axis.</summary>
    Auto,

    /// <summary>Always shown.</summary>
    Visible,

    /// <summary>Never shown; the axis still scrolls if it is enabled.</summary>
    Hidden,
}

/// <summary>
/// A scroll bar for one axis of a <see cref="ScrollState"/>: a track holding a thumb sized to the visible part of the
/// content. Dragging the thumb, or pressing the track, scrolls the bound view. It is not a node itself:
/// <see cref="Root"/> is the track to place in a layout. A <see cref="ScrollBox"/> creates and binds its own.
/// </summary>
public sealed class ScrollBar : IDisposable
{
    private const float DefaultThickness = 8.0f;
    private const float DefaultMinThumbLength = 18.0f;

    private readonly ScrollState state;
    private readonly ScrollBarTrack track;
    private readonly FlexBox strip;
    private readonly FlexBox leading;
    private readonly ScrollBarThumb thumb;
    private readonly FlexBox trailing;
    private readonly List<IDisposable> subscriptions = [];
    private readonly bool interactive;
    private ScrollView? target;
    private float dragStartOffset;
    private float trackThickness = DefaultThickness;
    private float thumbThickness = DefaultThickness;
    private float minThumbLength = DefaultMinThumbLength;
    private float trackCornerRadius;
    private float thumbCornerRadius;
    private Color? trackColor;
    private Color? thumbColor;
    private bool chromeVisible = true;

    /// <summary>A bar for <paramref name="state"/>'s offset and sizes, along <paramref name="orientation"/>.</summary>
    public ScrollBar(ScrollState state, Orientation orientation = Orientation.Vertical)
    {
        ArgumentNullException.ThrowIfNull(state);
        this.state = state;
        Orientation = orientation == Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
        interactive = !Platform.IsCoarsePointer;
        leading = new FlexBox();
        thumb = new ScrollBarThumb(this, interactive);
        trailing = new FlexBox();
        strip = new FlexBox()
            .FlexDirection(Orientation == Orientation.Horizontal ? FlexDirection.Row : FlexDirection.Column)
            .AlignItems(AlignItems.Center)
            .Child(leading)
            .Child(thumb)
            .Child(trailing);
        track = new ScrollBarTrack(this, interactive);
        track.ClipToBounds(true).Child(strip);
        var (offset, content, viewport) = Orientation == Orientation.Horizontal
            ? (state.OffsetX, state.ContentWidth, state.ViewportWidth)
            : (state.OffsetY, state.ContentHeight, state.ViewportHeight);
        subscriptions.Add(offset.Subscribe(_ => SyncThumb()));
        subscriptions.Add(content.Subscribe(_ => SyncThumb()));
        subscriptions.Add(viewport.Subscribe(_ => SyncThumb()));
        subscriptions.Add(Theme.Active.Subscribe(_ => ApplyColors()));
        ApplyColors();
        ApplyGeometry();
        SyncThumb();
    }

    /// <summary>Raised when the bar's thickness changes, so the layout around it can make room.</summary>
    internal event Action? ChromeMetricsChanged;

    /// <summary>The track: the node to place in a layout.</summary>
    public FlexBox Root => track;

    /// <summary>Whether the bar scrolls vertically or sideways.</summary>
    public Orientation Orientation { get; }

    /// <summary>The room the bar takes across its axis: <see cref="TrackThickness"/>.</summary>
    public float Thickness => trackThickness;

    /// <summary>The track's thickness in pixels.</summary>
    public float TrackThickness
    {
        get => trackThickness;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            if (trackThickness == value)
            {
                return;
            }
            trackThickness = value;
            ApplyGeometry();
            ChromeMetricsChanged?.Invoke();
        }
    }

    /// <summary>The thumb's thickness in pixels.</summary>
    public float ThumbThickness
    {
        get => thumbThickness;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            thumbThickness = value;
            ApplyGeometry();
        }
    }

    /// <summary>The thumb's shortest length along the axis, however long the content.</summary>
    public float MinThumbLength
    {
        get => minThumbLength;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            minThumbLength = value;
            SyncThumb();
        }
    }

    /// <summary>The track's corner radius.</summary>
    public float TrackCornerRadius
    {
        get => trackCornerRadius;
        set
        {
            trackCornerRadius = Math.Max(0.0f, value);
            track.CornerRadius = new CornerRadius(trackCornerRadius);
        }
    }

    /// <summary>The thumb's corner radius.</summary>
    public float ThumbCornerRadius
    {
        get => thumbCornerRadius;
        set
        {
            thumbCornerRadius = Math.Max(0.0f, value);
            thumb.CornerRadius = new CornerRadius(thumbCornerRadius);
        }
    }

    /// <summary>The track's color; null follows the theme.</summary>
    public Color? TrackColor
    {
        get => trackColor;
        set
        {
            trackColor = value;
            ApplyColors();
        }
    }

    /// <summary>The thumb's color; null follows the theme.</summary>
    public Color? ThumbColor
    {
        get => thumbColor;
        set
        {
            thumbColor = value;
            ApplyColors();
        }
    }

    /// <summary>Whether the thumb is being dragged.</summary>
    public bool IsDragging => thumb.IsDragging;

    /// <summary>Whether the bar shows; a hidden bar takes no room and cancels a thumb drag.</summary>
    internal bool IsChromeVisible
    {
        get => chromeVisible;
        set
        {
            if (chromeVisible == value)
            {
                return;
            }
            chromeVisible = value;
            if (!value)
            {
                thumb.CancelDrag();
            }
            ApplyGeometry();
            SyncThumb();
            ApplyThumbCursor();
        }
    }

    /// <summary>Releases the bar's subscriptions and nodes.</summary>
    public void Dispose()
    {
        thumb.CancelDrag();
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
    }

    /// <summary>Makes the bar scroll <paramref name="view"/>, and makes wheel input over the bar scroll it too.</summary>
    internal void BindTarget(ScrollView view)
    {
        target = view;
        BindProxy(view.Handle);
    }

    internal void ClearTarget(ScrollView view)
    {
        if (ReferenceEquals(target, view))
        {
            target = null;
            BindProxy(0);
        }
    }

    internal bool CanStartThumbDrag()
    {
        var metrics = Measure();
        return chromeVisible && metrics.MaxOffset > 0.0f && metrics.MaxThumbOffset > 0.0f;
    }

    internal void HandleThumbDragStarted()
    {
        dragStartOffset = AxisOffset;
        ApplyThumbCursor();
    }

    internal void HandleThumbDragCompleted() => ApplyThumbCursor();

    internal void HandleThumbDragDelta(DragDeltaEventArgs e)
    {
        var metrics = Measure();
        if (!thumb.IsDragging || metrics.MaxOffset <= 0.0f || metrics.MaxThumbOffset <= 0.0f)
        {
            return;
        }
        var delta = Orientation == Orientation.Horizontal ? e.TotalDeltaX : e.TotalDeltaY;
        SetScrollOffset(dragStartOffset + (delta * metrics.MaxOffset / metrics.MaxThumbOffset), metrics.MaxOffset);
    }

    /// <summary>A press on the track centers the thumb on the press, as far as it can go.</summary>
    internal void HandleTrackPress(float trackX, float trackY)
    {
        var metrics = Measure();
        if (thumb.IsDragging || metrics.MaxOffset <= 0.0f || metrics.MaxThumbOffset <= 0.0f)
        {
            return;
        }
        var pointer = Orientation == Orientation.Horizontal ? trackX : trackY;
        var thumbOffset = Math.Clamp(pointer - (metrics.ThumbLength * 0.5f), 0.0f, metrics.MaxThumbOffset);
        SetScrollOffset(thumbOffset / metrics.MaxThumbOffset * metrics.MaxOffset, metrics.MaxOffset);
    }

    private float AxisOffset => Orientation == Orientation.Horizontal ? state.OffsetX.Value : state.OffsetY.Value;

    private void BindProxy(ulong handle)
    {
        foreach (var node in (ReadOnlySpan<Node>)[track, strip, leading, thumb, trailing])
        {
            node.BindScrollProxyTarget(handle);
        }
    }

    private void SetScrollOffset(float offset, float maxOffset)
    {
        var clamped = Math.Clamp(offset, 0.0f, maxOffset);
        var (x, y) = Orientation == Orientation.Horizontal ? (clamped, state.OffsetY.Value) : (state.OffsetX.Value, clamped);
        if (target is { } view)
        {
            view.SetRuntimeScrollOffset(x, y);
        }
        else
        {
            state.OffsetX.Value = x;
            state.OffsetY.Value = y;
        }
    }

    private ScrollMetrics Measure()
    {
        var horizontal = Orientation == Orientation.Horizontal;
        var viewport = Math.Max(0.0f, horizontal ? state.ViewportWidth.Value : state.ViewportHeight.Value);
        var content = Math.Max(viewport, horizontal ? state.ContentWidth.Value : state.ContentHeight.Value);
        var maxOffset = content - viewport;
        var rawThumb = content > 0.0f ? viewport * (viewport / content) : 0.0f;
        var thumbLength = viewport > 0.0f ? Math.Clamp(rawThumb, Math.Min(minThumbLength, viewport), viewport) : 0.0f;
        var maxThumbOffset = Math.Max(0.0f, viewport - thumbLength);
        var thumbOffset = maxOffset > 0.0f && maxThumbOffset > 0.0f
            ? Math.Clamp(AxisOffset / maxOffset * maxThumbOffset, 0.0f, maxThumbOffset)
            : 0.0f;
        return new ScrollMetrics(viewport, maxOffset, thumbLength, maxThumbOffset, thumbOffset);
    }

    /// <summary>Sizes the track to the viewport and places the thumb between its two spacers.</summary>
    private void SyncThumb()
    {
        var metrics = chromeVisible ? Measure() : default;
        var trailingLength = Math.Max(0.0f, metrics.Viewport - metrics.ThumbLength - metrics.ThumbOffset);
        if (Orientation == Orientation.Horizontal)
        {
            track.Width = metrics.Viewport;
            strip.Width = metrics.Viewport;
            leading.Width = metrics.ThumbOffset;
            thumb.Width = metrics.ThumbLength;
            trailing.Width = trailingLength;
            return;
        }
        track.Height = metrics.Viewport;
        strip.Height = metrics.Viewport;
        leading.Height = metrics.ThumbOffset;
        thumb.Height = metrics.ThumbLength;
        trailing.Height = trailingLength;
    }

    private void ApplyGeometry()
    {
        var trackAcross = chromeVisible ? trackThickness : 0.0f;
        var thumbAcross = chromeVisible ? thumbThickness : 0.0f;
        if (Orientation == Orientation.Horizontal)
        {
            track.Height = trackAcross;
            strip.Height = trackAcross;
            leading.Height = trackAcross;
            trailing.Height = trackAcross;
            thumb.Height = thumbAcross;
            return;
        }
        track.Width = trackAcross;
        strip.Width = trackAcross;
        leading.Width = trackAcross;
        trailing.Width = trackAcross;
        thumb.Width = thumbAcross;
    }

    private void ApplyColors()
    {
        var colors = Theme.Active.Value.Colors;
        track.Background = trackColor ?? colors.ScrollbarTrack;
        thumb.Background = thumbColor ?? colors.ScrollbarThumb;
    }

    private void ApplyThumbCursor()
    {
        if (interactive)
        {
            thumb.Cursor = !chromeVisible ? CursorStyle.Default : thumb.IsDragging ? CursorStyle.Grabbing : CursorStyle.Grab;
        }
    }

    private readonly record struct ScrollMetrics(float Viewport, float MaxOffset, float ThumbLength, float MaxThumbOffset, float ThumbOffset);

    /// <summary>The track: a press centers the thumb on it.</summary>
    private sealed class ScrollBarTrack : FlexBox
    {
        private readonly ScrollBar owner;
        private readonly bool interactive;

        public ScrollBarTrack(ScrollBar owner, bool interactive)
        {
            this.owner = owner;
            this.interactive = interactive;
            PointerDown += static _ => { };
        }

        protected internal override void HandlePointerEvent(PointerEventArgs e)
        {
            base.HandlePointerEvent(e);
            if (interactive && e.EventType == PointerEventType.Down)
            {
                owner.HandleTrackPress(e.X, e.Y);
            }
        }
    }

    /// <summary>The thumb: dragging it scrolls, and the drag keeps the pointer when it leaves the track.</summary>
    private sealed class ScrollBarThumb : FlexBox
    {
        private readonly ScrollBar owner;
        private readonly bool interactive;
        private readonly DragGesture drag;

        public ScrollBarThumb(ScrollBar owner, bool interactive)
        {
            this.owner = owner;
            this.interactive = interactive;
            drag = new DragGesture(this).Threshold(0.0f);
            drag.Started += _ => owner.HandleThumbDragStarted();
            drag.Delta += owner.HandleThumbDragDelta;
            drag.Completed += _ => owner.HandleThumbDragCompleted();
            PointerDown += static _ => { };
            if (interactive)
            {
                Cursor = CursorStyle.Grab;
            }
        }

        public bool IsDragging => drag.IsDragging;

        public void CancelDrag() => drag.Cancel();

        /// <summary>Drags in page coordinates: the thumb itself moves while it is dragged.</summary>
        protected internal override void HandlePointerEvent(PointerEventArgs e)
        {
            base.HandlePointerEvent(e);
            if (!interactive)
            {
                return;
            }
            switch (e.EventType)
            {
                case PointerEventType.Down when owner.CanStartThumbDrag():
                    drag.HandlePointerDown(e.SceneX, e.SceneY, e.Modifiers);
                    e.Handled = true;
                    break;
                case PointerEventType.Move:
                    drag.HandlePointerMove(e.SceneX, e.SceneY, e.Modifiers);
                    break;
                case PointerEventType.Up:
                    drag.HandlePointerUp(e.SceneX, e.SceneY, e.Modifiers);
                    break;
                case PointerEventType.Cancel:
                    drag.Cancel();
                    break;
            }
        }
    }
}
