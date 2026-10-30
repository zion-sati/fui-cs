namespace FuiCs;

/// <summary>A drag starting, once the pointer has moved past the threshold.</summary>
public sealed class DragStartedEventArgs(float x = 0.0f, float y = 0.0f, KeyModifier modifiers = 0) : EventArgs
{
    /// <summary>The pointer, relative to the node.</summary>
    public float X { get; } = x;
    /// <summary>The pointer, relative to the node.</summary>
    public float Y { get; } = y;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
}

/// <summary>A drag moving.</summary>
public sealed class DragDeltaEventArgs(
    float x = 0.0f,
    float y = 0.0f,
    float deltaX = 0.0f,
    float deltaY = 0.0f,
    float totalDeltaX = 0.0f,
    float totalDeltaY = 0.0f,
    KeyModifier modifiers = 0) : EventArgs
{
    /// <summary>The pointer, relative to the node.</summary>
    public float X { get; } = x;
    /// <summary>The pointer, relative to the node.</summary>
    public float Y { get; } = y;
    /// <summary>The movement since the last event.</summary>
    public float DeltaX { get; } = deltaX;
    /// <summary>The movement since the last event.</summary>
    public float DeltaY { get; } = deltaY;
    /// <summary>The movement since the drag started.</summary>
    public float TotalDeltaX { get; } = totalDeltaX;
    /// <summary>The movement since the drag started.</summary>
    public float TotalDeltaY { get; } = totalDeltaY;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
}

/// <summary>A drag ending.</summary>
public sealed class DragGestureCompletedEventArgs(
    float x = 0.0f,
    float y = 0.0f,
    float totalDeltaX = 0.0f,
    float totalDeltaY = 0.0f,
    KeyModifier modifiers = 0,
    bool cancelled = false) : EventArgs
{
    /// <summary>The pointer, relative to the node.</summary>
    public float X { get; } = x;
    /// <summary>The pointer, relative to the node.</summary>
    public float Y { get; } = y;
    /// <summary>The movement since the drag started.</summary>
    public float TotalDeltaX { get; } = totalDeltaX;
    /// <summary>The movement since the drag started.</summary>
    public float TotalDeltaY { get; } = totalDeltaY;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>Whether the drag was cancelled rather than released.</summary>
    public bool Cancelled { get; } = cancelled;
}

/// <summary>
/// Recognizes a pointer drag on <paramref name="host"/>: captures the pointer on press, starts once the
/// pointer moves past the threshold (or on long press), and reports deltas until release or cancel.
/// Raising any of the events schedules a commit.
/// </summary>
public sealed class DragGesture(Node host)
{
    private const float DefaultThreshold = 4.0f;

    private float threshold = DefaultThreshold;
    private bool pointerDown;
    private bool dragStarted;
    private bool awaitingLongPress;
    private bool pointerCaptured;
    private float startX;
    private float startY;
    private float lastPointerX;
    private float lastPointerY;
    private float lastDispatchedX;
    private float lastDispatchedY;
    private KeyModifier lastModifiers;

    /// <summary>Raised when a drag starts or ends, with whether one is in progress.</summary>
    public event Action<bool>? DraggingChanged;

    /// <summary>Raised when the drag starts.</summary>
    public event Action<DragStartedEventArgs>? Started;

    /// <summary>Raised as the drag moves.</summary>
    public event Action<DragDeltaEventArgs>? Delta;

    /// <summary>Raised when the drag ends or is cancelled.</summary>
    public event Action<DragGestureCompletedEventArgs>? Completed;

    /// <summary>Whether a drag is in progress.</summary>
    public bool IsDragging { get; private set; }

    /// <summary>Sets how far, in pixels, the pointer must move before a drag starts.</summary>
    public DragGesture Threshold(float value)
    {
        threshold = value > 0.0f ? value : 0.0f;
        return this;
    }

    /// <summary>
    /// Feeds a press; with <paramref name="waitForLongPress"/>, the drag starts only from a long press.
    /// </summary>
    public void HandlePointerDown(float x, float y, KeyModifier modifiers = 0, bool waitForLongPress = false)
    {
        if (pointerDown)
        {
            Cancel();
        }
        pointerDown = true;
        dragStarted = false;
        awaitingLongPress = waitForLongPress;
        startX = x;
        startY = y;
        lastPointerX = x;
        lastPointerY = y;
        lastDispatchedX = x;
        lastDispatchedY = y;
        lastModifiers = modifiers;
        if (!waitForLongPress)
        {
            CapturePointer();
        }
        if (!waitForLongPress && threshold <= 0.0f)
        {
            BeginDrag(x, y, modifiers);
        }
    }

    /// <summary>Feeds a pointer move.</summary>
    public void HandlePointerMove(float x, float y, KeyModifier modifiers = 0)
    {
        if (!pointerDown)
        {
            return;
        }
        lastPointerX = x;
        lastPointerY = y;
        lastModifiers = modifiers;
        if (awaitingLongPress)
        {
            return;
        }
        if (!dragStarted)
        {
            if (!HasExceededThreshold(x - startX, y - startY))
            {
                return;
            }
            BeginDrag(x, y, modifiers);
        }
        EmitDelta(x, y, modifiers);
    }

    /// <summary>Feeds a long press, which starts a drag waiting for one; returns whether it did.</summary>
    public bool HandleLongPress(float x, float y, KeyModifier modifiers = 0)
    {
        if (!pointerDown)
        {
            pointerDown = true;
            dragStarted = false;
            startX = x;
            startY = y;
            lastDispatchedX = x;
            lastDispatchedY = y;
        }
        awaitingLongPress = false;
        lastPointerX = x;
        lastPointerY = y;
        lastModifiers = modifiers;
        CapturePointer();
        BeginDrag(x, y, modifiers);
        return true;
    }

    /// <summary>Feeds a release, which ends the drag.</summary>
    public void HandlePointerUp(float x, float y, KeyModifier modifiers = 0)
    {
        if (!pointerDown)
        {
            return;
        }
        lastPointerX = x;
        lastPointerY = y;
        lastModifiers = modifiers;
        if (dragStarted)
        {
            EmitDelta(x, y, modifiers);
            SetDragging(false);
            Raise(Completed, new DragGestureCompletedEventArgs(x, y, x - startX, y - startY, modifiers, false));
        }
        pointerDown = false;
        dragStarted = false;
        awaitingLongPress = false;
        ReleasePointer();
    }

    /// <summary>Cancels the drag in progress, if any.</summary>
    public void Cancel()
    {
        if (!pointerDown)
        {
            return;
        }
        if (dragStarted)
        {
            SetDragging(false);
            Raise(Completed, new DragGestureCompletedEventArgs(
                lastPointerX, lastPointerY, lastPointerX - startX, lastPointerY - startY, lastModifiers, true));
        }
        pointerDown = false;
        dragStarted = false;
        awaitingLongPress = false;
        ReleasePointer();
    }

    private void BeginDrag(float x, float y, KeyModifier modifiers)
    {
        if (dragStarted)
        {
            return;
        }
        dragStarted = true;
        lastDispatchedX = startX;
        lastDispatchedY = startY;
        SetDragging(true);
        Raise(Started, new DragStartedEventArgs(x, y, modifiers));
    }

    private void EmitDelta(float x, float y, KeyModifier modifiers)
    {
        if (x == lastDispatchedX && y == lastDispatchedY)
        {
            return;
        }
        var deltaX = x - lastDispatchedX;
        var deltaY = y - lastDispatchedY;
        lastDispatchedX = x;
        lastDispatchedY = y;
        Raise(Delta, new DragDeltaEventArgs(x, y, deltaX, deltaY, x - startX, y - startY, modifiers));
    }

    private void SetDragging(bool dragging)
    {
        if (IsDragging == dragging)
        {
            return;
        }
        IsDragging = dragging;
        Raise(DraggingChanged, dragging);
    }

    private static void Raise<TArgs>(Action<TArgs>? handlers, TArgs args)
    {
        if (handlers is null)
        {
            return;
        }
        handlers(args);
        FrameScheduler.MarkNeedsCommit();
    }

    private bool HasExceededThreshold(float totalDeltaX, float totalDeltaY)
    {
        if (threshold <= 0.0f)
        {
            return true;
        }
        return (totalDeltaX * totalDeltaX) + (totalDeltaY * totalDeltaY) >= threshold * threshold;
    }

    private void CapturePointer()
    {
        if (pointerCaptured)
        {
            return;
        }
        pointerCaptured = true;
        host.CapturePointer();
    }

    private void ReleasePointer()
    {
        if (!pointerCaptured)
        {
            return;
        }
        pointerCaptured = false;
        host.ReleasePointer();
    }
}
