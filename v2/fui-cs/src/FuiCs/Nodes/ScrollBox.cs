namespace FuiCs;

/// <summary>
/// A <see cref="ScrollView"/> with scroll bars, like WPF's ScrollViewer: content added to the box goes into the view,
/// and each bar shows according to its <see cref="ScrollBarVisibility"/>. Wheel input over the bars, the gutter and
/// the corner scrolls the view, and its offset persists under the box's node id.
/// </summary>
public class ScrollBox : FlexBox
{
    private const float OverflowTolerance = 0.5f;

    private readonly FlexBox topRow;
    private readonly FlexBox bottomRow;
    private readonly FlexBox verticalGutter;
    private readonly FlexBox corner;
    private readonly List<IDisposable> subscriptions = [];
    private ScrollBarVisibility verticalVisibility = ScrollBarVisibility.Auto;
    private ScrollBarVisibility horizontalVisibility = ScrollBarVisibility.Auto;
    private float scrollBarGutter;

    /// <summary>
    /// Creates a box over <paramref name="viewport"/> (a new <see cref="ScrollView"/> by default), sharing
    /// <paramref name="scrollState"/> (a new one by default) between the view and the bars.
    /// </summary>
    public ScrollBox(ScrollState? scrollState = null, ScrollView? viewport = null)
    {
        ScrollState = scrollState ?? new ScrollState();
        Viewport = viewport ?? new ScrollView();
        Viewport.BindScrollState(ScrollState);
        Viewport.FillWidth = true;
        Viewport.FillHeight = true;
        Viewport.PersistenceOwner = this;
        VerticalScrollBar = new ScrollBar(ScrollState, Orientation.Vertical);
        HorizontalScrollBar = new ScrollBar(ScrollState, Orientation.Horizontal);
        verticalGutter = HitTarget(new FlexBox().Width(0.0f).FillHeight());
        corner = HitTarget(new FlexBox().Width(0.0f).Height(0.0f));
        topRow = HitTarget(FlexBox.Row(Viewport, verticalGutter, VerticalScrollBar.Root).FillSize());
        bottomRow = HitTarget(FlexBox.Row(HorizontalScrollBar.Root, corner).FillWidth());
        AddChild(topRow);
        AddChild(bottomRow);
        foreach (var signal in (ReadOnlySpan<Signal<float>>)[ScrollState.ContentWidth, ScrollState.ContentHeight, ScrollState.ViewportWidth, ScrollState.ViewportHeight])
        {
            subscriptions.Add(signal.Subscribe(_ => RefreshChrome()));
        }
        VerticalScrollBar.ChromeMetricsChanged += RefreshChrome;
        HorizontalScrollBar.ChromeMetricsChanged += RefreshChrome;
        RefreshChrome();
    }

    /// <summary>Raised when the view scrolls or its content or viewport size changes.</summary>
    public event Action<ScrollChangedEventArgs>? ScrollChanged
    {
        add => Viewport.ScrollChanged += value;
        remove => Viewport.ScrollChanged -= value;
    }

    /// <summary>The offsets and sizes the view and the bars share.</summary>
    public ScrollState ScrollState { get; }

    /// <summary>The scroll view holding the content.</summary>
    public ScrollView Viewport { get; }

    /// <summary>The vertical bar.</summary>
    public ScrollBar VerticalScrollBar { get; }

    /// <summary>The horizontal bar.</summary>
    public ScrollBar HorizontalScrollBar { get; }

    /// <summary>The horizontal offset, in pixels.</summary>
    public float HorizontalOffset => Viewport.HorizontalOffset;

    /// <summary>The vertical offset, in pixels.</summary>
    public float VerticalOffset => Viewport.VerticalOffset;

    /// <summary>Whether the content scrolls sideways.</summary>
    public bool IsHorizontalScrollEnabled
    {
        get => Viewport.IsHorizontalScrollEnabled;
        set
        {
            Viewport.IsHorizontalScrollEnabled = value;
            RefreshChrome();
        }
    }

    /// <summary>Whether the content scrolls vertically.</summary>
    public bool IsVerticalScrollEnabled
    {
        get => Viewport.IsVerticalScrollEnabled;
        set
        {
            Viewport.IsVerticalScrollEnabled = value;
            RefreshChrome();
        }
    }

    /// <summary>Animates wheel and keyboard scrolling toward its target instead of jumping.</summary>
    public bool SmoothScrolling
    {
        get => Viewport.SmoothScrolling;
        set => Viewport.SmoothScrolling = value;
    }

    /// <summary>
    /// Whether a mouse-wheel run that starts in the box stays in it at the edges until the wheel pauses (true by
    /// default); see <see cref="ScrollView.WheelLatching"/>.
    /// </summary>
    public bool WheelLatching
    {
        get => Viewport.WheelLatching;
        set => Viewport.WheelLatching = value;
    }

    /// <summary>Whether the offset is saved with the page's UI state under the box's <see cref="Node.NodeId"/>.</summary>
    public bool PersistScroll
    {
        get => Viewport.PersistScroll;
        set => Viewport.PersistScroll = value;
    }

    /// <summary>When the vertical bar shows.</summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => verticalVisibility;
        set
        {
            verticalVisibility = value;
            RefreshChrome();
        }
    }

    /// <summary>When the horizontal bar shows.</summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => horizontalVisibility;
        set
        {
            horizontalVisibility = value;
            RefreshChrome();
        }
    }

    /// <summary>Room between the content and the vertical bar while the bar shows.</summary>
    public float ScrollBarGutter
    {
        get => scrollBarGutter;
        set
        {
            scrollBarGutter = Math.Max(0.0f, value);
            RefreshChrome();
        }
    }

    /// <inheritdoc/>
    public override void AddContent(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        ClearContentProxies();
        Viewport.AddContent(node);
        BindContentProxies();
    }

    /// <inheritdoc/>
    public override void SetContent(params ReadOnlySpan<Node> nodes)
    {
        ClearContentProxies();
        Viewport.SetContent(nodes);
        BindContentProxies();
    }

    /// <summary>Scrolls to the offset at once.</summary>
    public void ScrollTo(float horizontalOffset, float verticalOffset) => Viewport.ScrollTo(horizontalOffset, verticalOffset);

    /// <summary>Scrolls to the offset over <paramref name="timing"/>.</summary>
    public void ScrollToAnimated(float horizontalOffset, float verticalOffset, AnimationTiming timing) =>
        Viewport.ScrollToAnimated(horizontalOffset, verticalOffset, timing);

    /// <summary>Sets the offset, animated when the view has a scroll offset transition.</summary>
    public void SetScrollOffset(float horizontalOffset, float verticalOffset) => Viewport.SetScrollOffset(horizontalOffset, verticalOffset);

    /// <summary>The viewport's transitions: only <see cref="NodeTransitions.ScrollOffset"/> applies to a scroll box.</summary>
    public override NodeTransitions? Transitions
    {
        get => Viewport.Transitions;
        set => Viewport.Transitions = value;
    }

    /// <summary>
    /// Overrides the content's size for scrolling, as for a virtual list; null keeps the measured size.
    /// </summary>
    public void SetScrollContentSize(float? width, float? height) => Viewport.SetScrollContentSize(width, height);

    /// <inheritdoc/>
    public override ulong Build()
    {
        var handle = base.Build();
        BindChrome(Viewport.Handle);
        RefreshChrome();
        return handle;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        VerticalScrollBar.ClearTarget(Viewport);
        HorizontalScrollBar.ClearTarget(Viewport);
        BindChrome(0);
        VerticalScrollBar.Dispose();
        HorizontalScrollBar.Dispose();
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
        base.Dispose();
    }

    /// <summary>Makes a chrome node a hit target, so wheel input over it reaches its scroll proxy.</summary>
    private static FlexBox HitTarget(FlexBox node)
    {
        node.PointerDown += static _ => { };
        return node;
    }

    /// <summary>Points the bars, the box and its chrome at the view, so they scroll it.</summary>
    private void BindChrome(ulong viewportHandle)
    {
        if (viewportHandle != 0)
        {
            VerticalScrollBar.BindTarget(Viewport);
            HorizontalScrollBar.BindTarget(Viewport);
        }
        foreach (var node in (ReadOnlySpan<Node>)[this, topRow, bottomRow, verticalGutter, corner])
        {
            node.BindScrollProxyTarget(viewportHandle);
        }
        if (viewportHandle != 0)
        {
            BindContentProxies();
        }
        else
        {
            ClearContentProxies();
        }
    }

    private void BindContentProxies()
    {
        if (!Viewport.IsBuilt)
        {
            return;
        }
        foreach (var child in Viewport.Children)
        {
            child.BindScrollProxyTarget(Viewport.Handle);
        }
    }

    private void ClearContentProxies()
    {
        foreach (var child in Viewport.Children)
        {
            child.BindScrollProxyTarget(0);
        }
    }

    /// <summary>Shows each bar per its visibility and the overflow on its axis, and sizes the gutter and corner.</summary>
    private void RefreshChrome()
    {
        var showVertical = ShouldShow(verticalVisibility, IsVerticalScrollEnabled, ScrollState.ContentHeight.Value, ScrollState.ViewportHeight.Value);
        var showHorizontal = ShouldShow(horizontalVisibility, IsHorizontalScrollEnabled, ScrollState.ContentWidth.Value, ScrollState.ViewportWidth.Value);
        VerticalScrollBar.IsChromeVisible = showVertical;
        HorizontalScrollBar.IsChromeVisible = showHorizontal;
        verticalGutter.Width = showVertical ? scrollBarGutter : 0.0f;
        VerticalScrollBar.Root.Width = showVertical ? VerticalScrollBar.Thickness : 0.0f;
        var railHeight = showHorizontal ? HorizontalScrollBar.Thickness : 0.0f;
        bottomRow.Height = railHeight;
        HorizontalScrollBar.Root.Height = railHeight;
        corner.Width = showVertical ? scrollBarGutter + VerticalScrollBar.Thickness : 0.0f;
        corner.Height = railHeight;
    }

    private static bool ShouldShow(ScrollBarVisibility visibility, bool enabled, float content, float viewport) => enabled && visibility switch
    {
        ScrollBarVisibility.Visible => true,
        ScrollBarVisibility.Hidden => false,
        _ => viewport > 0.0f && content > 0.0f && content > viewport + OverflowTolerance,
    };
}
