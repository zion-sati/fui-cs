using FuiCs.Bindings;

namespace FuiCs;

internal static partial class EventRouter
{
    private static readonly List<KeyFilter> keyFilters = [];

    /// <summary>Raised on every host scroll, before the scrolled node is notified.</summary>
    public static event Action? Scrolled;

    /// <summary>
    /// Runs <paramref name="handler"/> for every key event before the focused node sees it, newest filter first.
    /// Returning true consumes the event. Disposing the result removes the filter.
    /// </summary>
    public static IDisposable PushKeyFilter(Func<KeyEventType, string, KeyModifier, bool> handler)
    {
        var filter = new KeyFilter(handler);
        keyFilters.Add(filter);
        return filter;
    }

    /// <summary>
    /// Set while a menu is open: a primary press that it dismisses (returns true for) is consumed, so it does not reach
    /// the page under the menu.
    /// </summary>
    public static Func<float, float, bool>? PointerDownOutside { get; set; }

    /// <summary>Raised for every pointer release with the node under it and its page point.</summary>
    public static event Action<Node?, float, float>? PointerReleased;

    /// <summary>Asked about a key press that no key filter or focused node handled; returns whether it handled it.</summary>
    public static event Func<string, KeyModifier, bool>? UnhandledKeyDown;

    public static bool DispatchUnhandledKeyDown(string key, KeyModifier modifiers) => UnhandledKeyDown?.Invoke(key, modifiers) ?? false;

    public static bool DispatchPointerEvent(ulong handle, PointerEventArgs e)
    {
        var eventType = e.EventType;
        FocusVisibility.ShowForPointerEvent(eventType);
        SelectionHandles.RecordPointerEvent(eventType, e.PointerType);
        var pointedNode = ResolveNode(handle);
        if (eventType == PointerEventType.Up)
        {
            PointerReleased?.Invoke(pointedNode, e.SceneX, e.SceneY);
        }
        if (eventType == PointerEventType.Down && e.IsPrimaryActivation && PointerDownOutside?.Invoke(e.SceneX, e.SceneY) == true)
        {
            DragDropManager.HandlePointerEvent(null, eventType, e.SceneX, e.SceneY, e.Modifiers);
            ApplyCurrentCursor();
            return true;
        }
        // A press outside the selection toolbar closes it, unless it lands on chrome that keeps the selection.
        if (eventType == PointerEventType.Down && !PreservesSelection(pointedNode) && TextSelectionToolbar.DismissForOutsidePointerDown(e.SceneX, e.SceneY))
        {
            DragDropManager.HandlePointerEvent(null, eventType, e.SceneX, e.SceneY, e.Modifiers);
            ApplyCurrentCursor();
            return true;
        }
        if (eventType == PointerEventType.Down && e.Button == PointerButton.Primary && e.PointerType != PointerType.Touch)
        {
            selectionCursorHandle = ResolveSelectionCursorNode(pointedNode)?.Handle ?? InvalidHandle;
        }
        else if (eventType is PointerEventType.Up or PointerEventType.Cancel)
        {
            selectionCursorHandle = InvalidHandle;
        }
        if (eventType is PointerEventType.Move or PointerEventType.Up or PointerEventType.Cancel)
        {
            if (ResolveNode(capturedPointerHandle) is { } captured)
            {
                var capturedHandle = capturedPointerHandle;
                captured.HandlePointerEvent(e);
                BubblePointerEvent(captured.Parent, e);
                DragDropManager.HandlePointerEvent(pointedNode, eventType, e.SceneX, e.SceneY, e.Modifiers);
                if (eventType is PointerEventType.Up or PointerEventType.Cancel && capturedPointerHandle == capturedHandle)
                {
                    capturedPointerHandle = InvalidHandle;
                }
                ApplyCurrentCursor();
                return e.Handled;
            }
            capturedPointerHandle = InvalidHandle;
        }
        // A selection handle drag follows the pointer even when the handle no longer holds the capture.
        if (eventType is PointerEventType.Move or PointerEventType.Up or PointerEventType.Cancel && SelectionHandles.RouteActiveHandleDrag(e))
        {
            DragDropManager.HandlePointerEvent(pointedNode, eventType, e.SceneX, e.SceneY, e.Modifiers);
            ApplyCurrentCursor();
            return true;
        }
        if (pointedNode is null)
        {
            if (eventType == PointerEventType.Leave)
            {
                ClearHoverStack();
            }
            DragDropManager.HandlePointerEvent(null, eventType, e.SceneX, e.SceneY, e.Modifiers);
            ApplyCurrentCursor();
            return false;
        }
        if (eventType == PointerEventType.Enter)
        {
            PushHover(pointedNode);
        }
        else if (eventType == PointerEventType.Leave)
        {
            PopHover(pointedNode);
        }
        pointedNode.HandlePointerEvent(e);
        BubblePointerEvent(pointedNode.Parent, e);
        DragDropManager.HandlePointerEvent(pointedNode, eventType, e.SceneX, e.SceneY, e.Modifiers);
        ApplyCurrentCursor();
        return e.Handled;
    }

    public static bool DispatchWheelEvent(ulong handle, WheelEventArgs e)
    {
        for (var node = ResolveNode(handle); node is not null; node = node.Parent)
        {
            if (node.IsEnabled && node.IsVisible && node.HandleWheelEvent(e))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>The nearest enabled, visible node at or above <paramref name="handle"/> that wants pan or pinch.</summary>
    public static ulong ResolveGestureOwner(ulong handle)
    {
        for (var node = ResolveNode(handle); node is not null; node = node.Parent)
        {
            if (node.IsEnabled && node.IsVisible && node.GestureIntentForRouting != GestureIntent.None)
            {
                return node.Handle;
            }
        }
        return InvalidHandle;
    }

    public static GestureIntent GetGestureIntent(ulong handle) => ResolveNode(handle)?.GestureIntentForRouting ?? GestureIntent.None;

    /// <summary>
    /// The nearest node that recognizes long press; otherwise the nearest selectable or editable text, which
    /// long press selects.
    /// </summary>
    public static ulong ResolveLongPressOwner(ulong handle)
    {
        for (var node = ResolveNode(handle); node is not null; node = node.Parent)
        {
            if (node.IsEnabled && node.IsVisible && node.HasLongPressGestureForRouting)
            {
                return node.Handle;
            }
        }
        // Without a handler, links, images, SVGs and selectable text own the press for their built-in items.
        for (var node = ResolveNode(handle); node is not null; node = node.Parent)
        {
            if (!node.IsEnabled || !node.IsVisible)
            {
                continue;
            }
            if (node is NavLink or Image or Svg || HasImageDescendant(node))
            {
                return node.Handle;
            }
            if (ActiveEditor(node) is { } editor)
            {
                return editor.Handle;
            }
            if (node.IsSelectableText || node.IsEditableText)
            {
                return node.Handle;
            }
        }
        return InvalidHandle;
    }

    private static bool HasImageDescendant(Node node)
    {
        foreach (var child in node.Children)
        {
            if (child.IsEnabled && child.IsVisible && (child is Image or Svg || HasImageDescendant(child)))
            {
                return true;
            }
        }
        return false;
    }

    public static bool LongPressContinuesPointerEvents(ulong handle) =>
        ResolveNode(handle)?.LongPressContinuesPointerEventsForRouting ?? false;

    public static int GetLongPressMinimumDurationMs(ulong handle) =>
        ResolveNode(handle)?.LongPressMinimumDurationMsForRouting ?? LongPressDefaults.MinimumDurationMs;

    public static float GetLongPressMovementTolerance(ulong handle) =>
        ResolveNode(handle)?.LongPressMovementToleranceForRouting ?? LongPressDefaults.MovementTolerance;

    /// <summary>
    /// Raises a long press on the node and its ancestors. Unhandled, a touch or pen long press selects the word under
    /// it in selectable text, or shows the built-in items (open a link or image) in the selection toolbar.
    /// </summary>
    public static bool DispatchLongPressEvent(ulong handle, LongPressEventArgs e)
    {
        var node = ResolveNode(handle);
        if (node is null)
        {
            return false;
        }
        node.HandleLongPressEvent(e);
        for (var current = node.Parent; current is not null && !e.Handled; current = current.Parent)
        {
            current.HandleLongPressEvent(e);
        }
        if (!e.Handled && e.PointerType is PointerType.Touch or PointerType.Pen && (node.IsSelectableText || node.IsEditableText))
        {
            TextSelectionToolbar.SetPendingCrossSelectionTextHandle(handle);
            e.Handled = Ui.SelectWordAt(handle, e.SceneX, e.SceneY);
            if (!e.Handled)
            {
                TextSelectionToolbar.SetPendingCrossSelectionTextHandle(0);
            }
        }
        if (!e.Handled)
        {
            e.Handled = ContextMenuManager.ShowForLongPress(handle, e.SceneX, e.SceneY);
        }
        return e.Handled;
    }

    public static bool DispatchGestureEvent(ulong handle, GestureEventArgs e)
    {
        var node = ResolveNode(handle);
        if (node is null)
        {
            return false;
        }
        node.HandleGestureEvent(e);
        for (var current = node.Parent; current is not null && !e.Handled; current = current.Parent)
        {
            current.HandleGestureEvent(e);
        }
        return e.Handled;
    }

    public static void DispatchFocusChanged(ulong handle, bool focused)
    {
        var node = ResolveNode(handle);
        if (node is null)
        {
            return;
        }
        if (focused)
        {
            FocusedNode = node;
        }
        else if (ReferenceEquals(FocusedNode, node))
        {
            FocusedNode = null;
        }
        node.HandleFocusChanged(focused);
        ToolTips?.FocusChanged(node, node.ToolTip, focused);
    }

    public static bool DispatchKeyEvent(ulong handle, KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var node = ResolveNode(handle);
        if (node is null)
        {
            FocusVisibility.ShowForKeyEvent(eventType, key, modifiers);
            return false;
        }
        var handled = node.HandleKeyEvent(eventType, key, modifiers);
        if (!handled || !IsPlainTab(key, modifiers))
        {
            FocusVisibility.ShowForKeyEvent(eventType, key, modifiers);
        }
        return handled;
    }

    public static bool DispatchGlobalKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        for (var index = keyFilters.Count - 1; index >= 0; --index)
        {
            if (keyFilters[index].Handler(eventType, key, modifiers))
            {
                if (!IsPlainTab(key, modifiers))
                {
                    FocusVisibility.ShowForKeyEvent(eventType, key, modifiers);
                }
                return true;
            }
        }
        FocusVisibility.ShowForKeyEvent(eventType, key, modifiers);
        return false;
    }

    public static void DispatchScroll(
        ulong handle,
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight)
    {
        Scrolled?.Invoke();
        var node = ResolveNode(handle);
        // The selection chrome follows page scrolling, but not its own: the toolbar's overflow list scrolls too.
        if (!IsInTextToolbar(node))
        {
            SelectionHandles.RefreshActiveGeometry();
            TextSelectionToolbar.RefreshActiveGeometry(SelectionHandles.IsVisible);
        }
        node?.HandleScroll(offsetX, offsetY, contentWidth, contentHeight, viewportWidth, viewportHeight);
    }

    private static bool IsInTextToolbar(Node? node)
    {
        var overlay = Application.ShellTextToolbarHost;
        for (; node is not null && overlay is not null; node = node.Parent)
        {
            if (ReferenceEquals(node, overlay))
            {
                return true;
            }
        }
        return false;
    }

    private static bool PreservesSelection(Node? node)
    {
        for (; node is not null; node = node.Parent)
        {
            if (node.PreserveSelectionOnPointerDown)
            {
                return true;
            }
        }
        return false;
    }

    public static void DispatchTextChanged(ulong handle, string text) => ResolveNode(handle)?.HandleTextChanged(text);

    public static void DispatchTextReplaced(ulong handle, uint start, uint end, string text) =>
        ResolveNode(handle)?.HandleTextReplaced(start, end, text);

    /// <summary>Updates the touch selection chrome for a text node's selection, then tells the node.</summary>
    public static void DispatchSelectionChanged(ulong handle, uint start, uint end)
    {
        if (ResolveNode(handle) is not { } node)
        {
            SelectionHandles.Clear();
            TextSelectionToolbar.Clear();
            return;
        }
        // A text input's selection chrome belongs to its editor.
        var chrome = node.InnerEditor ?? node;
        SelectionHandles.HandleSelectionChanged(chrome.Handle, start, end);
        TextSelectionToolbar.HandleSelectionChanged(chrome, start, end, SelectionHandles.IsVisible);
        node.HandleSelectionChanged(start, end);
    }

    /// <summary>Updates the touch selection chrome for a selection area's cross-node selection, then tells the area.</summary>
    public static void DispatchCrossSelectionChanged(ulong handle, string text)
    {
        if (ResolveNode(handle) is not { } node)
        {
            SelectionHandles.Clear();
            TextSelectionToolbar.Clear();
            return;
        }
        SelectionHandles.HandleCrossSelectionChanged(handle, text);
        TextSelectionToolbar.HandleCrossSelectionChanged(handle, text, SelectionHandles.IsVisible);
        node.HandleCrossSelectionChanged(text);
    }

    internal static bool BeginDragSession(Node source) => DragDropManager.BeginSession(source);

    internal static void CancelDragSession(DragSession session)
    {
        DragDropManager.CancelSession(session);
        ApplyCurrentCursor();
    }

    internal static void CancelDragSessionForSource(Node source)
    {
        DragDropManager.CancelSessionForSource(source);
        ApplyCurrentCursor();
    }

    private static void BubblePointerEvent(Node? parent, PointerEventArgs e)
    {
        for (var node = parent; node is not null && !e.Handled; node = node.Parent)
        {
            node.HandleBubbledPointerEvent(e);
        }
    }

    private static Node? ResolveSelectionCursorNode(Node? node)
    {
        if (node is null || !node.IsEnabled || !node.IsVisible)
        {
            return null;
        }
        return node.IsSelectableText || node.IsEditableText ? node : ActiveEditor(node);
    }

    /// <summary>A control's inner editor while it is enabled, visible and selectable or editable.</summary>
    private static TextCore? ActiveEditor(Node node) =>
        node.InnerEditor is { IsEnabled: true, IsVisible: true } editor && (editor.IsSelectableText || editor.IsEditableText) ? editor : null;

    private static bool IsPlainTab(string key, KeyModifier modifiers) => key == "Tab" && modifiers == KeyModifier.None;

    private sealed class KeyFilter(Func<KeyEventType, string, KeyModifier, bool> handler) : IDisposable
    {
        public Func<KeyEventType, string, KeyModifier, bool> Handler { get; } = handler;

        public void Dispose() => keyFilters.Remove(this);
    }
}
