using FuiCs.Bindings;

namespace FuiCs;

/// <summary>When text selections show draggable handles.</summary>
public enum SelectionHandleMode
{
    /// <summary>For selections made by touch, or on a coarse-pointer device before any input.</summary>
    Auto,

    /// <summary>For every selection, including mouse ones.</summary>
    Always,

    /// <summary>Never; the toolbar does not show either.</summary>
    Disabled,
}

/// <summary>
/// The draggable handles at the two ends of a text selection, for touch. They follow the selection of a text node or
/// a selection area's cross-node selection, and dragging one moves that end of the selection.
/// </summary>
public static class SelectionHandles
{
    private const uint HandleColor = 0x0A84FFFF;
    private const float HitTargetSize = 90.0f;
    private const float HitTargetPadding = 25.0f;
    private const float KnobSize = 18.0f;
    private const float ShoulderSize = 8.0f;
    private const float StartStemX = 72.0f;
    private const float EndStemX = 18.0f;

    private static HandleNode? startHandle;
    private static HandleNode? endHandle;
    private static ulong activeHandle;
    private static uint activeStart;
    private static uint activeEnd;
    private static bool usesRangeGeometry;
    private static bool usesCrossGeometry;
    private static PointerType lastPointerType = PointerType.Unknown;
    private static Side? draggingSide;
    private static Side? draggingVisualSide;
    private static Point? startAnchor;
    private static Point? endAnchor;
    private static Point? stationaryAnchor;

    private enum Side
    {
        Start = 0,
        End = 1,
    }

    /// <summary>When the selection handles show; <see cref="SelectionHandleMode.Auto"/> shows them for touch selections.</summary>
    public static SelectionHandleMode Mode
    {
        get;
        set
        {
            field = value;
            if (value == SelectionHandleMode.Disabled)
            {
                Clear();
            }
        }
    }

    internal static bool IsVisible => activeHandle != 0;

    internal static void RecordPointerEvent(PointerEventType eventType, PointerType pointerType)
    {
        if (eventType == PointerEventType.Down)
        {
            lastPointerType = pointerType;
        }
    }

    internal static void HandleSelectionChanged(ulong handle, uint start, uint end)
    {
        if (start == end || !ShouldShow())
        {
            if (start == end && draggingSide is not null && activeHandle == handle)
            {
                (activeStart, activeEnd) = (start, end);
                EnsureHost();
                PositionRangeHandles();
                ShowNonDraggedHandles();
                return;
            }
            Clear();
            return;
        }
        (activeHandle, activeStart, activeEnd) = (handle, start, end);
        (usesRangeGeometry, usesCrossGeometry) = (true, false);
        EnsureHost();
        if (!PositionRangeHandles())
        {
            Clear();
            return;
        }
        ShowAfterUpdate();
    }

    internal static void HandleCrossSelectionChanged(ulong handle, string text)
    {
        if (text.Length == 0 || !ShouldShow())
        {
            if (text.Length == 0 && draggingSide is not null && activeHandle == handle)
            {
                (activeStart, activeEnd) = (0, 0);
                EnsureHost();
                ShowNonDraggedHandles();
                return;
            }
            Clear();
            return;
        }
        (activeHandle, activeStart, activeEnd) = (handle, 0, (uint)text.Length);
        (usesRangeGeometry, usesCrossGeometry) = (true, true);
        EnsureHost();
        if (!PositionCrossSelectionHandles())
        {
            (usesRangeGeometry, usesCrossGeometry) = (false, false);
            startHandle!.Position = new Point(0.0f, 0.0f);
            endHandle!.Position = new Point(HitTargetSize + 8.0f, 0.0f);
        }
        ShowAfterUpdate();
    }

    /// <summary>Moves the handles to the selection's current geometry, after a scroll or layout change.</summary>
    internal static void RefreshActiveGeometry()
    {
        if (activeHandle == 0 || !usesRangeGeometry)
        {
            return;
        }
        if (!(usesCrossGeometry ? PositionCrossSelectionHandles() : PositionRangeHandles()))
        {
            Clear();
        }
    }

    /// <summary>Routes the pointer to the handle being dragged, wherever it is; returns whether it took the event.</summary>
    internal static bool RouteActiveHandleDrag(PointerEventArgs e)
    {
        if (draggingSide is null)
        {
            return false;
        }
        switch (e.EventType)
        {
            case PointerEventType.Move:
                RefreshActiveGeometry();
                ShowNonDraggedHandles();
                return true;
            case PointerEventType.Up or PointerEventType.Cancel:
                EndHandleDrag();
                return true;
            default:
                return false;
        }
    }

    internal static void Clear()
    {
        if (draggingSide is not null)
        {
            EndHandleDrag();
        }
        (activeHandle, activeStart, activeEnd) = (0, 0, 0);
        (usesRangeGeometry, usesCrossGeometry) = (false, false);
        (draggingSide, draggingVisualSide, stationaryAnchor) = (null, null, null);
        HideHandles();
    }

    /// <summary>Ends a handle drag, hides the handles and forgets them, as they went with the shell that hosted them.</summary>
    internal static void ClearShell()
    {
        Clear();
        (startHandle, endHandle) = (null, null);
        (startAnchor, endAnchor, stationaryAnchor) = (null, null, null);
    }

    internal static void Reset()
    {
        startHandle = null;
        endHandle = null;
        (activeHandle, activeStart, activeEnd) = (0, 0, 0);
        (usesRangeGeometry, usesCrossGeometry) = (false, false);
        Mode = SelectionHandleMode.Auto;
        lastPointerType = PointerType.Unknown;
        (draggingSide, draggingVisualSide) = (null, null);
        (startAnchor, endAnchor, stationaryAnchor) = (null, null, null);
    }

    private static bool ShouldShow() => Mode switch
    {
        SelectionHandleMode.Disabled => false,
        SelectionHandleMode.Always => true,
        _ => lastPointerType == PointerType.Touch || (lastPointerType == PointerType.Unknown && Platform.IsCoarsePointer),
    };

    private static void EnsureHost()
    {
        if (startHandle is not null || Application.ShellSelectionHandleHost is not { } host)
        {
            return;
        }
        startHandle = new HandleNode(Side.Start) { NodeId = "fui-selection-start-handle" };
        endHandle = new HandleNode(Side.End) { NodeId = "fui-selection-end-handle" };
        host.AddChild(startHandle);
        host.AddChild(endHandle);
        Application.Unmounting -= ClearShell;
        Application.Unmounting += ClearShell;
    }

    private static void ShowAfterUpdate()
    {
        if (draggingSide is not null)
        {
            ShowNonDraggedHandles();
            return;
        }
        SetVisibility(startHandle, Visibility.Normal);
        SetVisibility(endHandle, Visibility.Normal);
    }

    private static void ShowNonDraggedHandles()
    {
        SetVisibility(startHandle, draggingVisualSide == Side.Start ? Visibility.Hidden : Visibility.Normal);
        SetVisibility(endHandle, draggingVisualSide == Side.End ? Visibility.Hidden : Visibility.Normal);
    }

    /// <summary>Hides both hit targets, leaving their parts visible for the next show.</summary>
    private static void HideHandles()
    {
        SetVisibility(startHandle, Visibility.Normal);
        SetVisibility(endHandle, Visibility.Normal);
        startHandle?.Visibility = Visibility.Hidden;
        endHandle?.Visibility = Visibility.Hidden;
    }

    /// <summary>Shows the handle's hit target, with its knob and shoulder shown or hidden.</summary>
    private static void SetVisibility(HandleNode? handle, Visibility chrome)
    {
        if (handle is null)
        {
            return;
        }
        handle.Visibility = Visibility.Normal;
        foreach (var child in handle.Children)
        {
            child.Visibility = chrome;
        }
    }

    private static bool BeginHandleDrag(Side visualSide, PointerType pointerType)
    {
        if (pointerType is not (PointerType.Touch or PointerType.Pen) || activeHandle == 0)
        {
            return false;
        }
        stationaryAnchor = visualSide == Side.Start ? endAnchor : startAnchor;
        if (stationaryAnchor is null || (visualSide == Side.Start ? startHandle : endHandle) is not { } handle)
        {
            stationaryAnchor = null;
            return false;
        }
        var forward = activeStart <= activeEnd;
        var side = (visualSide == Side.Start) == forward ? Side.Start : Side.End;
        (draggingSide, draggingVisualSide) = (side, visualSide);
        if (!Ui.BeginSelectionEndpointDrag(activeHandle, (uint)side))
        {
            (draggingSide, draggingVisualSide, stationaryAnchor) = (null, null, null);
            return false;
        }
        TextSelectionToolbar.HideForHandleDrag();
        handle.CapturePointer();
        SetHitTestVisible(handle, false);
        SetVisibility(handle, Visibility.Hidden);
        SetVisibility(visualSide == Side.Start ? endHandle : startHandle, Visibility.Normal);
        return true;
    }

    private static void EndHandleDrag()
    {
        var handle = draggingVisualSide == Side.Start ? startHandle : draggingVisualSide == Side.End ? endHandle : null;
        if (handle is not null)
        {
            handle.ReleasePointer();
            SetHitTestVisible(handle, true);
        }
        (draggingSide, draggingVisualSide, stationaryAnchor) = (null, null, null);
        if (activeStart == activeEnd)
        {
            Clear();
            return;
        }
        RefreshActiveGeometry();
        SetVisibility(startHandle, Visibility.Normal);
        SetVisibility(endHandle, Visibility.Normal);
        TextSelectionToolbar.ShowAfterHandleDrag(IsVisible);
    }

    private static void SetHitTestVisible(HandleNode handle, bool visible)
    {
        if (handle.IsBuilt)
        {
            Ui.SetInteractive(handle.Handle, visible);
        }
    }

    private static bool PositionRangeHandles()
    {
        if (startHandle is null || endHandle is null || activeHandle == 0)
        {
            return false;
        }
        var rects = Runtime.Abi.UiReadTextRangeRects(activeHandle, Math.Min(activeStart, activeEnd), Math.Max(activeStart, activeEnd));
        if (rects.Length < 4)
        {
            return false;
        }
        var last = rects.Length - 4;
        var lower = new Point(rects[0], rects[1] + rects[3]);
        var upper = new Point(rects[last] + rects[last + 2], rects[last + 1] + rects[last + 3]);
        if (draggingSide is null)
        {
            PositionStart(lower);
            PositionEnd(upper);
            return true;
        }
        // While dragging, the end that moves is the one away from where the other handle was anchored.
        var forward = activeStart <= activeEnd;
        var movingIsLower = (draggingSide == Side.Start) == forward;
        var moving = movingIsLower ? lower : upper;
        var stationary = movingIsLower ? upper : lower;
        if (stationaryAnchor is { } anchor)
        {
            stationary = anchor;
            moving = DistanceSquared(upper, anchor) >= DistanceSquared(lower, anchor) ? upper : lower;
        }
        if (draggingVisualSide == Side.Start)
        {
            PositionStart(moving);
            PositionEnd(stationary);
        }
        else
        {
            PositionStart(stationary);
            PositionEnd(moving);
        }
        return true;
    }

    private static bool PositionCrossSelectionHandles()
    {
        if (startHandle is null || endHandle is null || activeHandle == 0)
        {
            return false;
        }
        var rects = Runtime.Abi.UiReadCrossSelectionEndpointRects(activeHandle);
        if (rects.Length < 8)
        {
            return false;
        }
        PositionStart(new Point(rects[0], rects[1] + rects[3]));
        PositionEnd(new Point(rects[4] + rects[6], rects[5] + rects[7]));
        return true;
    }

    private static void PositionStart(Point anchor)
    {
        startAnchor = anchor;
        startHandle!.Position = new Point(anchor.X - StartStemX, anchor.Y - HitTargetPadding);
    }

    private static void PositionEnd(Point anchor)
    {
        endAnchor = anchor;
        endHandle!.Position = new Point(anchor.X - EndStemX, anchor.Y - HitTargetPadding);
    }

    private static float DistanceSquared(Point a, Point b) => ((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y));

    /// <summary>A handle's 90 px hit target with its round knob and the square shoulder that points at the text.</summary>
    private sealed class HandleNode : FlexBox
    {
        private readonly Side side;

        public HandleNode(Side side)
        {
            this.side = side;
            var knobX = side == Side.Start ? StartStemX - KnobSize : EndStemX;
            var shoulderX = side == Side.Start ? knobX + KnobSize - ShoulderSize : knobX;
            PositionType = FuiCs.PositionType.Absolute;
            Width = HitTargetSize;
            Height = HitTargetSize;
            Background = Color.Transparent;
            PreserveSelectionOnPointerDown = true;
            Visibility = Visibility.Hidden;
            PointerDown += static _ => { };
            AddChild(Part(knobX, KnobSize).CornerRadius(KnobSize * 0.5f).Border(1.0f, 0x00000022));
            AddChild(Part(shoulderX, ShoulderSize));
        }

        protected internal override void HandlePointerEvent(PointerEventArgs e)
        {
            base.HandlePointerEvent(e);
            Route(e);
        }

        protected internal override bool HandleBubbledPointerEvent(PointerEventArgs e)
        {
            var handled = base.HandleBubbledPointerEvent(e);
            Route(e);
            return handled || e.Handled;
        }

        private static FlexBox Part(float x, float size)
        {
            var part = new FlexBox().PositionAbsolute().Position(x, HitTargetPadding).Width(size).Height(size).Background(HandleColor);
            part.PreserveSelectionOnPointerDown = true;
            part.PointerDown += static _ => { };
            return part;
        }

        private void Route(PointerEventArgs e)
        {
            if (e.EventType == PointerEventType.Down)
            {
                e.Handled |= BeginHandleDrag(side, e.PointerType);
            }
            else if (RouteActiveHandleDrag(e))
            {
                e.Handled = true;
            }
        }
    }
}
