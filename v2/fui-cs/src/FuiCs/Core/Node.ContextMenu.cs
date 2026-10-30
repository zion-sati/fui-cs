namespace FuiCs;

/// <summary>A request for a context menu over <see cref="Target"/> at a page point.</summary>
public sealed class ContextMenuEventArgs(Node? target, float x, float y) : EventArgs
{
    /// <summary>The node under the pointer.</summary>
    public Node? Target { get; } = target;

    /// <summary>The request's point in page coordinates.</summary>
    public float X { get; } = x;

    /// <summary>The request's point in page coordinates.</summary>
    public float Y { get; } = y;
}

public abstract partial class Node
{
    private Action<ContextMenuEventArgs>? contextMenuOpening;

    /// <summary>
    /// Raised instead of the built-in context menu when the user asks for one over this node or a descendant that
    /// has no handler of its own; show a <see cref="ContextMenu"/> or other UI from it.
    /// </summary>
    public event Action<ContextMenuEventArgs>? ContextMenuOpening
    {
        add
        {
            contextMenuOpening += value;
            RequireInteractive();
        }
        remove => contextMenuOpening -= value;
    }

    /// <summary>Whether a context menu may open over this node and its descendants; when false, none does.</summary>
    public bool IsContextMenuEnabled
    {
        get;
        set
        {
            field = value;
            if (!value)
            {
                // The host asks about the node under the pointer only if it is a hit target.
                RequireInteractive();
            }
        }
    } = true;

    /// <summary>Raises <see cref="ContextMenuOpening"/>; returns whether the node has a handler.</summary>
    internal bool RaiseContextMenuOpening(ContextMenuEventArgs e)
    {
        if (contextMenuOpening is not { } handlers)
        {
            return false;
        }
        handlers(e);
        return true;
    }
}
