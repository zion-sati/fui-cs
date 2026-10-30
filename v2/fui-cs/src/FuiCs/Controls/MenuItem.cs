namespace FuiCs;

/// <summary>A built-in action a menu item runs through the host.</summary>
public enum ContextMenuAction
{
    /// <summary>The item runs only its <see cref="MenuItem.Click"/> handlers.</summary>
    None,

    /// <summary>Copies <see cref="MenuItem.Payload"/>, the target text's selection, or the page selection.</summary>
    CopyCurrentSelection,

    /// <summary>Reloads the page.</summary>
    ReloadPage,
    /// <summary>Goes back in the browser's history.</summary>
    NavigateBack,
    /// <summary>Goes forward in the browser's history.</summary>
    NavigateForward,

    /// <summary>Navigates to the link in <see cref="MenuItem.Payload"/>.</summary>
    OpenLink,

    /// <summary>Opens the link in <see cref="MenuItem.Payload"/> in a new tab.</summary>
    OpenLinkInNewTab,

    /// <summary>Navigates to the image URL in <see cref="MenuItem.Payload"/>.</summary>
    OpenImage,

    /// <summary>Opens the image URL in <see cref="MenuItem.Payload"/> in a new tab.</summary>
    OpenImageInNewTab,

    /// <summary>Selects all of the target text.</summary>
    SelectAllText,

    /// <summary>Undoes the target editable text's last edit.</summary>
    UndoTextEdit,

    /// <summary>Redoes the target editable text's last undone edit.</summary>
    RedoTextEdit,

    /// <summary>Copies the target editable text's selection, then deletes it.</summary>
    CutTextSelection,

    /// <summary>Pastes the clipboard into the target editable text.</summary>
    PasteText,
}

/// <summary>
/// An entry of a <see cref="ContextMenu"/>: a label, an optional shortcut hint, and the action it runs when chosen.
/// <see cref="Separator"/> draws a divider between groups.
/// </summary>
public sealed class MenuItem
{
    private MenuItem(string label, ContextMenuAction action, bool isSeparator)
    {
        ArgumentNullException.ThrowIfNull(label);
        Label = label;
        Action = action;
        IsSeparator = isSeparator;
    }

    /// <summary>An item that raises <see cref="Click"/>, running <paramref name="click"/> if given.</summary>
    public MenuItem(string label, Action? click = null)
        : this(label, ContextMenuAction.None, isSeparator: false)
    {
        if (click is not null)
        {
            Click += click;
        }
    }

    /// <summary>An item that runs a built-in <paramref name="action"/>, then raises <see cref="Click"/>.</summary>
    public MenuItem(string label, ContextMenuAction action, string? payload = null)
        : this(label, action, isSeparator: false)
    {
        Payload = payload;
    }

    /// <summary>A divider between groups of items; leading, trailing and repeated dividers are not drawn.</summary>
    public static MenuItem Separator => new(string.Empty, ContextMenuAction.None, isSeparator: true);

    /// <summary>Raised when the user chooses the item, after its built-in action runs and the menu hides.</summary>
    public event Action? Click;

    /// <summary>The item's text.</summary>
    public string Label { get; }

    /// <summary>The built-in action the item runs, or None.</summary>
    public ContextMenuAction Action { get; }

    /// <summary>The text or URL a built-in action uses.</summary>
    public string? Payload { get; init; }

    /// <summary>A shortcut hint drawn after the label, such as "Ctrl+C".</summary>
    public string? ShortcutLabel { get; init; }

    /// <summary>Whether the item can be chosen; a disabled item shows muted.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Whether the item is a divider.</summary>
    public bool IsSeparator { get; }

    /// <summary>The text node a text action applies to.</summary>
    internal ulong TargetHandle { get; init; }

    /// <summary>The target's selection, in UTF-8 offsets, when the menu opened; Cut deletes it if the snapshot is gone.</summary>
    internal (uint Start, uint End) SelectionRange { get; init; }

    /// <summary>Returns keyboard focus to the target editor after the action, as an editor's own menu items do.</summary>
    internal bool FocusTargetAfterAction { get; init; }

    internal void Invoke()
    {
        ContextMenuActions.Run(this);
        Click?.Invoke();
    }
}
