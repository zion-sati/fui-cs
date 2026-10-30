namespace FuiCs;

/// <summary>Keyboard focus, as WPF's <c>Keyboard</c> reports it.</summary>
public static class Keyboard
{
    /// <summary>The node that has keyboard focus, or null.</summary>
    public static Node? FocusedNode => EventRouter.FocusedNode;
}
