using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// Maps native handles to their nodes and routes host input to them. Owns hover, pointer capture, focus
/// tracking and the host cursor.
/// </summary>
internal static partial class EventRouter
{
    private const ulong InvalidHandle = (ulong)HandleValue.Invalid;

    private static readonly Dictionary<uint, (Node Node, uint Generation)> nodes = [];
    private static readonly List<Node> hoverStack = [];
    private static ulong capturedPointerHandle = InvalidHandle;
    private static ulong selectionCursorHandle = InvalidHandle;
    private static CursorStyle currentCursorStyle = CursorStyle.Default;

    /// <summary>The node that has keyboard focus, if any.</summary>
    public static Node? FocusedNode { get; private set; }

    /// <summary>The live node for <paramref name="handle"/>, or null when the handle is stale or unknown.</summary>
    public static Node? GetRegisteredNode(ulong handle) => ResolveNode(handle);

    /// <summary>Raised when a built node's handle is released.</summary>
    internal static event Action<Node>? NodeDestroyed;

    /// <summary>The tooltip manager, installed when a node first gets a tooltip.</summary>
    internal static IToolTipRouting? ToolTips { get; set; }

    internal static void Register(ulong handle, Node node)
    {
        node.Handle = handle;
        nodes[HandleIndex(handle)] = (node, HandleGeneration(handle));
    }

    internal static void Unregister(ulong handle)
    {
        var node = ResolveNode(handle);
        if (node is not null)
        {
            DragDropManager.HandleNodeDestroyed(node);
            NodeDestroyed?.Invoke(node);
            RemoveFromHoverStack(node);
            if (ReferenceEquals(FocusedNode, node))
            {
                FocusedNode = null;
            }
        }
        if (capturedPointerHandle == handle)
        {
            capturedPointerHandle = InvalidHandle;
        }
        if (selectionCursorHandle == handle)
        {
            selectionCursorHandle = InvalidHandle;
        }
        nodes.Remove(HandleIndex(handle));
        ApplyCurrentCursor();
    }

    internal static void CapturePointer(ulong handle)
    {
        if (ResolveNode(handle) is null)
        {
            return;
        }
        capturedPointerHandle = handle;
        ApplyCurrentCursor();
    }

    internal static void ReleasePointer(ulong handle)
    {
        if (capturedPointerHandle == handle)
        {
            capturedPointerHandle = InvalidHandle;
            ApplyCurrentCursor();
        }
    }

    /// <summary>Drops capture, focus and hover held by nodes under <paramref name="root"/>, when a view is deactivated.</summary>
    internal static void DeactivateSubtree(Node root)
    {
        if (ResolveNode(capturedPointerHandle) is { } captured && BelongsToSubtree(captured, root))
        {
            capturedPointerHandle = InvalidHandle;
        }
        if (FocusedNode is not null && BelongsToSubtree(FocusedNode, root))
        {
            FocusedNode = null;
            Ui.RequestFocus(InvalidHandle);
        }
        for (var index = hoverStack.Count - 1; index >= 0; --index)
        {
            if (BelongsToSubtree(hoverStack[index], root))
            {
                hoverStack.RemoveAt(index);
            }
        }
        selectionCursorHandle = InvalidHandle;
        DragDropManager.Reset();
        ContextMenu.HideActive();
        ToolTips?.Clear();
        SelectionHandles.Clear();
        TextSelectionToolbar.Clear();
        ApplyCurrentCursor();
    }

    internal static void Reset()
    {
        nodes.Clear();
        capturedPointerHandle = InvalidHandle;
        selectionCursorHandle = InvalidHandle;
        hoverStack.Clear();
        FocusedNode = null;
        keyFilters.Clear();
        PointerDownOutside = null;
        KeyboardScrollTracker.Reset();
        FocusVisibility.Reset();
        DragDropManager.Reset();
        ApplyCursor(CursorStyle.Default);
    }

    internal static void RemoveFromHoverStack(Node node)
    {
        var index = hoverStack.LastIndexOf(node);
        if (index < 0)
        {
            return;
        }
        hoverStack.RemoveAt(index);
        ApplyCurrentCursor();
    }

    internal static void HandleCursorStyleChanged(Node node)
    {
        var captured = ResolveNode(capturedPointerHandle);
        if (ReferenceEquals(captured, node))
        {
            ApplyCurrentCursor();
            return;
        }
        if (captured is not null || hoverStack.Count == 0)
        {
            return;
        }
        // The hovered node's cursor, or an ancestor's that the hovered text's I-beam gives way to.
        for (Node? current = hoverStack[^1]; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, node))
            {
                ApplyCursor(CursorOf(hoverStack[^1]));
                return;
            }
        }
    }

    /// <summary>
    /// A node's cursor. Selectable text's automatic I-beam gives way to the nearest ancestor that sets its own cursor,
    /// as a browser shows a link's hand over the link's text.
    /// </summary>
    private static CursorStyle CursorOf(Node node)
    {
        if (node is not TextCore { HasAutomaticCursor: true })
        {
            return node.Cursor;
        }
        for (var ancestor = node.Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor.Cursor != CursorStyle.Default)
            {
                return ancestor.Cursor;
            }
        }
        return node.Cursor;
    }

    private static uint HandleIndex(ulong handle) => (uint)handle;

    private static uint HandleGeneration(ulong handle) => (uint)(handle >> 32);

    internal static Node? ResolveNode(ulong handle)
    {
        if (handle == InvalidHandle || !nodes.TryGetValue(HandleIndex(handle), out var entry))
        {
            return null;
        }
        return entry.Generation == HandleGeneration(handle) ? entry.Node : null;
    }

    private static bool BelongsToSubtree(Node node, Node root)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, root))
            {
                return true;
            }
        }
        return false;
    }

    private static void PushHover(Node node)
    {
        var existing = hoverStack.LastIndexOf(node);
        if (existing >= 0)
        {
            hoverStack.RemoveAt(existing);
        }
        hoverStack.Add(node);
        ApplyCurrentCursor();
    }

    private static void PopHover(Node node) => RemoveFromHoverStack(node);

    private static void ClearHoverStack()
    {
        hoverStack.Clear();
        ApplyCurrentCursor();
    }

    private static void ApplyCurrentCursor()
    {
        var dragCursor = DragDropManager.CursorOverrideStyle();
        if (dragCursor != CursorStyle.Default)
        {
            ApplyCursor(dragCursor);
            return;
        }
        if (ResolveNode(capturedPointerHandle) is { } captured)
        {
            ApplyCursor(CursorOf(captured));
            return;
        }
        if (ResolveNode(selectionCursorHandle) is { } selectionCursorNode)
        {
            ApplyCursor(CursorOf(selectionCursorNode));
            return;
        }
        ApplyCursor(hoverStack.Count == 0 ? CursorStyle.Default : CursorOf(hoverStack[^1]));
    }

    private static void ApplyCursor(CursorStyle style)
    {
        if (currentCursorStyle == style)
        {
            return;
        }
        currentCursorStyle = style;
        if (Runtime.Abi.HostIsCoarsePointer())
        {
            return;
        }
        Runtime.Abi.HostSetCursor(style);
    }
}
