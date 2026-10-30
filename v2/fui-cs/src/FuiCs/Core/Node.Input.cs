namespace FuiCs;

public abstract partial class Node
{
    private Action<PointerClickEventArgs>? pointerClick;
    private Action<PointerClickEventArgs>? pointerDoubleClick;
    private Action<PointerClickEventArgs>? pointerTripleClick;
    private Action<PointerEventArgs>? pointerDown;
    private Action<PointerEventArgs>? pointerMove;
    private Action<PointerEventArgs>? pointerUp;
    private Action<PointerEventArgs>? pointerEnter;
    private Action<PointerEventArgs>? pointerLeave;
    private Action<PointerEventArgs>? pointerCancel;
    private Action<WheelEventArgs>? wheel;
    private Action<GestureEventArgs>? panGesture;
    private Action<GestureEventArgs>? pinchGesture;
    private Action<LongPressEventArgs>? longPress;
    private LongPressGesture? longPressRecognizer;
    private GestureIntent gestureIntent;
    private DragGesture? dragGesture;
    private bool dragClickPending;
    private int dragClickPendingCount;
    private bool clickPending;
    private int clickPendingCount;

    /// <summary>A primary press and release on this node. Subscribing makes the node interactive.</summary>
    public event Action<PointerClickEventArgs>? PointerClick
    {
        add => AddInteractiveHandler(ref pointerClick, value);
        remove => pointerClick -= value;
    }

    /// <summary>Raised on a double click.</summary>
    public event Action<PointerClickEventArgs>? PointerDoubleClick
    {
        add => AddInteractiveHandler(ref pointerDoubleClick, value);
        remove => pointerDoubleClick -= value;
    }

    /// <summary>Raised on a triple click.</summary>
    public event Action<PointerClickEventArgs>? PointerTripleClick
    {
        add => AddInteractiveHandler(ref pointerTripleClick, value);
        remove => pointerTripleClick -= value;
    }

    /// <summary>Raised when a pointer presses over the node.</summary>
    public event Action<PointerEventArgs>? PointerDown
    {
        add => AddInteractiveHandler(ref pointerDown, value);
        remove => pointerDown -= value;
    }

    /// <summary>Raised when a pointer moves over the node.</summary>
    public event Action<PointerEventArgs>? PointerMove
    {
        add => AddInteractiveHandler(ref pointerMove, value);
        remove => pointerMove -= value;
    }

    /// <summary>Raised when a pointer releases over the node.</summary>
    public event Action<PointerEventArgs>? PointerUp
    {
        add => AddInteractiveHandler(ref pointerUp, value);
        remove => pointerUp -= value;
    }

    /// <summary>Raised when a pointer enters the node.</summary>
    public event Action<PointerEventArgs>? PointerEnter
    {
        add => AddInteractiveHandler(ref pointerEnter, value);
        remove => pointerEnter -= value;
    }

    /// <summary>Raised when a pointer leaves the node.</summary>
    public event Action<PointerEventArgs>? PointerLeave
    {
        add => AddInteractiveHandler(ref pointerLeave, value);
        remove => pointerLeave -= value;
    }

    /// <summary>Raised when the host cancels a pointer, as when a touch becomes a scroll.</summary>
    public event Action<PointerEventArgs>? PointerCancel
    {
        add => AddInteractiveHandler(ref pointerCancel, value);
        remove => pointerCancel -= value;
    }

    /// <summary>Raised when the wheel or trackpad scrolls over the node.</summary>
    public event Action<WheelEventArgs>? Wheel
    {
        add => AddInteractiveHandler(ref wheel, value);
        remove => wheel -= value;
    }

    /// <summary>Pan gestures on this node. Subscribing claims pan for the node.</summary>
    public event Action<GestureEventArgs>? PanGesture
    {
        add
        {
            panGesture += value;
            UpdateGestureIntent();
        }
        remove
        {
            panGesture -= value;
            UpdateGestureIntent();
        }
    }

    /// <summary>Pinch gestures on this node. Subscribing claims pinch for the node.</summary>
    public event Action<GestureEventArgs>? PinchGesture
    {
        add
        {
            pinchGesture += value;
            UpdateGestureIntent();
        }
        remove
        {
            pinchGesture -= value;
            UpdateGestureIntent();
        }
    }

    /// <summary>A long press with the default duration and tolerance; use <see cref="LongPressRecognizer"/> to configure them.</summary>
    public event Action<LongPressEventArgs>? LongPress
    {
        add => AddInteractiveHandler(ref longPress, value);
        remove => longPress -= value;
    }

    /// <summary>Raised when the node gains or loses keyboard focus.</summary>
    public event Action<FocusChangedEventArgs>? FocusChanged;

    /// <summary>Raised when a key goes down while the node has focus.</summary>
    public event Action<KeyEventArgs>? KeyDown;

    /// <summary>Raised when a key goes up while the node has focus.</summary>
    public event Action<KeyEventArgs>? KeyUp;

    /// <summary>A configured long-press recognizer; its duration and tolerance apply to <see cref="LongPress"/> too.</summary>
    public LongPressGesture? LongPressRecognizer
    {
        get => longPressRecognizer;
        set
        {
            longPressRecognizer = value;
            if (value is not null)
            {
                RequireInteractive();
            }
        }
    }

    internal GestureIntent GestureIntentForRouting => gestureIntent;

    internal bool HasLongPressGestureForRouting =>
        longPress is not null || longPressRecognizer is { HasHandler: true } || HasDragSource;

    internal bool LongPressContinuesPointerEventsForRouting => HasDragSource;

    internal int LongPressMinimumDurationMsForRouting =>
        longPressRecognizer?.MinimumDurationMs ?? LongPressDefaults.MinimumDurationMs;

    internal float LongPressMovementToleranceForRouting =>
        longPressRecognizer?.MovementTolerancePx ?? LongPressDefaults.MovementTolerance;

    private bool HasPointerClickHandler =>
        pointerClick is not null || pointerDoubleClick is not null || pointerTripleClick is not null;

    /// <summary>
    /// Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags.
    /// Overrides should call the base method.
    /// </summary>
    protected internal virtual void HandlePointerEvent(PointerEventArgs e)
    {
        if (!IsEnabled || !IsVisible)
        {
            return;
        }
        var bounds = GetBounds();
        var nodeX = e.SceneX - bounds.X;
        var nodeY = e.SceneY - bounds.Y;
        e.X = nodeX;
        e.Y = nodeY;
        // A press hides the node's tooltip and a move steers a pending one before handlers run; entering and leaving
        // reach the tooltip after them.
        var toolTips = EventRouter.ToolTips;
        if (e.EventType == PointerEventType.Down)
        {
            toolTips?.PointerDown(this);
        }
        else if (e.EventType == PointerEventType.Move)
        {
            toolTips?.PointerMove(this, e.SceneX, e.SceneY);
        }
        RaisePointerEvent(e);
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                if (!e.Handled)
                {
                    toolTips?.PointerEnter(this, ToolTip, e.SceneX, e.SceneY);
                }
                break;
            case PointerEventType.Down:
                if (!e.Handled)
                {
                    BeginPress(e, nodeX, nodeY);
                }
                break;
            case PointerEventType.Move:
                if (!e.Handled && dragGesture is { } moveGesture && (HasDragSource || moveGesture.IsDragging))
                {
                    moveGesture.HandlePointerMove(nodeX, nodeY, e.Modifiers);
                }
                break;
            case PointerEventType.Up:
                if (e.Handled)
                {
                    ClearPendingClicks();
                }
                else
                {
                    EndPress(e, nodeX, nodeY, bounds.Contains(e.SceneX, e.SceneY));
                }
                break;
            case PointerEventType.Leave:
                dragClickPending = false;
                clickPending = false;
                clickPendingCount = 0;
                toolTips?.PointerLeave(this);
                break;
            case PointerEventType.Cancel:
                CancelDragState();
                toolTips?.PointerLeave(this);
                break;
        }
    }

    /// <summary>Handles a pointer event bubbling up from a descendant; returns whether it is handled.</summary>
    protected internal virtual bool HandleBubbledPointerEvent(PointerEventArgs e)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }
        var bounds = GetBounds();
        e.X = e.SceneX - bounds.X;
        e.Y = e.SceneY - bounds.Y;
        RaisePointerEvent(e);
        if (e.Handled)
        {
            return true;
        }
        switch (e.EventType)
        {
            case PointerEventType.Down:
                clickPending = e.IsPrimaryActivation && HasPointerClickHandler;
                clickPendingCount = e.ClickCount;
                break;
            case PointerEventType.Up:
                // As for the node's own clicks, a release outside the node cancels the click.
                if (clickPending && bounds.Contains(e.SceneX, e.SceneY))
                {
                    FireClick(clickPendingCount, e);
                }
                clickPending = false;
                clickPendingCount = 0;
                break;
            case PointerEventType.Leave:
            case PointerEventType.Cancel:
                clickPending = false;
                clickPendingCount = 0;
                break;
        }
        return e.Handled;
    }

    /// <summary>Whether this node has keyboard focus.</summary>
    public virtual bool IsFocused => ReferenceEquals(EventRouter.FocusedNode, this);

    /// <summary>Called when the node gains or loses focus; raises <see cref="FocusChanged"/>.</summary>
    protected internal virtual void HandleFocusChanged(bool focused) => FocusChanged?.Invoke(new FocusChangedEventArgs(focused));

    /// <summary>Raises <see cref="FocusChanged"/>, for nodes whose focus is held by an inner node.</summary>
    protected void RaiseFocusChanged(FocusChangedEventArgs e) => FocusChanged?.Invoke(e);

    /// <summary>Handles a key event for the focused node; returns true when a key handler ran.</summary>
    protected internal virtual bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }
        var handlers = eventType switch
        {
            KeyEventType.Down => KeyDown,
            KeyEventType.Up => KeyUp,
            _ => null,
        };
        if (handlers is null)
        {
            return false;
        }
        handlers(new KeyEventArgs(eventType, key, modifiers));
        return true;
    }

    /// <summary>
    /// Called when the host scrolls the node, with its offset and the sizes of its content and viewport.
    /// </summary>
    protected internal virtual void HandleScroll(
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight)
    {
    }

    /// <summary>Called when the user replaces the node's whole text.</summary>
    protected internal virtual void HandleTextChanged(string text)
    {
    }

    /// <summary>Called when the user replaces a range of the node's text, given in UTF-8 offsets.</summary>
    protected internal virtual void HandleTextReplaced(uint utf8Start, uint utf8End, string text)
    {
    }

    /// <summary>Called when the node's selection changes, given in UTF-8 offsets.</summary>
    protected internal virtual void HandleSelectionChanged(uint utf8Start, uint utf8End)
    {
    }

    /// <summary>Called when a selection area's selection across nodes changes, with its text.</summary>
    protected internal virtual void HandleCrossSelectionChanged(string text)
    {
    }

    internal bool HandleWheelEvent(WheelEventArgs e)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }
        var bounds = GetBounds();
        e.X = e.SceneX - bounds.X;
        e.Y = e.SceneY - bounds.Y;
        wheel?.Invoke(e);
        return e.Handled;
    }

    internal bool HandleGestureEvent(GestureEventArgs e)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }
        var bounds = GetBounds();
        e.X = e.SceneX - bounds.X;
        e.Y = e.SceneY - bounds.Y;
        var handlers = e.Kind switch
        {
            GestureEventKind.Pan => panGesture,
            GestureEventKind.Pinch => pinchGesture,
            _ => null,
        };
        handlers?.Invoke(e);
        return e.Handled;
    }

    internal bool HandleLongPressEvent(LongPressEventArgs e)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }
        var bounds = GetBounds();
        e.X = e.SceneX - bounds.X;
        e.Y = e.SceneY - bounds.Y;
        longPress?.Invoke(e);
        longPressRecognizer?.Raise(e);
        if (!e.Handled && e.PointerType is PointerType.Touch or PointerType.Pen && HasDragSource && dragGesture is { } gesture)
        {
            e.Handled = gesture.HandleLongPress(e.X, e.Y, e.Modifiers);
        }
        return e.Handled;
    }

    partial void CancelDragState()
    {
        ClearPendingClicks();
        dragGesture?.Cancel();
    }

    private void AddInteractiveHandler<TArgs>(ref Action<TArgs>? handlers, Action<TArgs>? handler)
    {
        if (handler is null)
        {
            return;
        }
        handlers += handler;
        RequireInteractive();
    }

    private void UpdateGestureIntent()
    {
        var intent = GestureIntent.None;
        if (panGesture is not null)
        {
            intent |= GestureIntent.Pan;
        }
        if (pinchGesture is not null)
        {
            intent |= GestureIntent.Pinch;
        }
        gestureIntent = intent;
        if (intent != GestureIntent.None)
        {
            RequireInteractive();
        }
    }

    private void RaisePointerEvent(PointerEventArgs e)
    {
        var handlers = e.EventType switch
        {
            PointerEventType.Down => pointerDown,
            PointerEventType.Move => pointerMove,
            PointerEventType.Up => pointerUp,
            PointerEventType.Enter => pointerEnter,
            PointerEventType.Leave => pointerLeave,
            PointerEventType.Cancel => pointerCancel,
            _ => null,
        };
        handlers?.Invoke(e);
    }

    /// <summary>A press either arms a click or, on a drag source, starts the drag gesture with a click held back.</summary>
    private void BeginPress(PointerEventArgs e, float nodeX, float nodeY)
    {
        var isPrimary = e.IsPrimaryActivation;
        if (HasDragSource && dragGesture is { } gesture && isPrimary)
        {
            var waitForLongPress = e.PointerType is PointerType.Touch or PointerType.Pen;
            gesture.HandlePointerDown(nodeX, nodeY, e.Modifiers, waitForLongPress);
            dragClickPending = HasPointerClickHandler;
            dragClickPendingCount = e.ClickCount;
            clickPending = false;
            clickPendingCount = 0;
            return;
        }
        dragClickPending = false;
        dragClickPendingCount = 0;
        clickPending = isPrimary && HasPointerClickHandler;
        clickPendingCount = e.ClickCount;
    }

    /// <summary>
    /// A release over the node fires the armed click; a held-back click fires only if no drag started. The host
    /// keeps routing a pressed pointer to the node after it leaves, so a release elsewhere cancels the click, as in
    /// browsers and WPF.
    /// </summary>
    private void EndPress(PointerEventArgs e, float nodeX, float nodeY, bool isOver)
    {
        var gesture = dragGesture;
        var gestureActive = gesture is not null && (HasDragSource || gesture.IsDragging);
        var canFireHeldBackClick = isOver && dragClickPending && (!gestureActive || !gesture!.IsDragging);
        var canFireClick = isOver && clickPending;
        if (gestureActive)
        {
            gesture!.HandlePointerUp(nodeX, nodeY, e.Modifiers);
        }
        if (canFireClick)
        {
            FireClick(clickPendingCount, e);
        }
        clickPending = false;
        clickPendingCount = 0;
        if (canFireHeldBackClick)
        {
            FireClick(dragClickPendingCount, e);
        }
        dragClickPending = false;
        dragClickPendingCount = 0;
    }

    private void ClearPendingClicks()
    {
        dragClickPending = false;
        dragClickPendingCount = 0;
        clickPending = false;
        clickPendingCount = 0;
    }

    private void FireClick(int count, PointerEventArgs e)
    {
        var click = new PointerClickEventArgs(e.SceneX, e.SceneY, e.PointerType, e.Button, e.Buttons, e.Modifiers, count > 0 ? count : 1)
        {
            X = e.X,
            Y = e.Y,
        };
        pointerClick?.Invoke(click);
        if (click.ClickCount == 2)
        {
            pointerDoubleClick?.Invoke(click);
        }
        else if (click.ClickCount == 3)
        {
            pointerTripleClick?.Invoke(click);
        }
        e.Handled = click.Handled;
    }
}
