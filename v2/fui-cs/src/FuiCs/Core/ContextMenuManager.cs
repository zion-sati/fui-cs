using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// Answers the host's context-menu requests: a node's <see cref="Node.ContextMenuOpening"/> handler, or the
/// application's built-in menu with the items that fit the target (copy and select all for text, opening an image,
/// copying the page selection) and the page's history and reload items.
/// </summary>
public static class ContextMenuManager
{
    private static readonly List<ulong> pressedHandles = [];
    private static readonly List<ulong> selectionHandles = [];
    private static string selectionText = string.Empty;
    private static NavLink? previewedLink;
    private static ContextMenu? appMenu;

    /// <summary>
    /// Shows the built-in context menu (the selection, link, image, history and reload items) in
    /// <paramref name="menu"/>, a menu in the application's tree that the app styles, instead of the shell's own menu;
    /// null restores the shell's. Mounting a new root, which disposes the old tree, restores it too.
    /// </summary>
    public static void SetMenu(ContextMenu? menu)
    {
        ReleaseLinkPreview();
        appMenu = menu;
        Application.Unmounting -= ClearMenu;
        if (menu is not null)
        {
            Application.Unmounting += ClearMenu;
        }
    }

    internal static void Reset()
    {
        previewedLink = null;
        pressedHandles.Clear();
        selectionHandles.Clear();
        selectionText = string.Empty;
        appMenu = null;
    }

    /// <summary>Remembers the nodes a press crossed, to tell later whether a request is over the page selection.</summary>
    internal static void TrackPointerEvent(PointerEventType eventType, ulong handle)
    {
        if (eventType == PointerEventType.Down)
        {
            pressedHandles.Clear();
        }
        if (handle != 0 && !pressedHandles.Contains(handle))
        {
            pressedHandles.Add(handle);
        }
    }

    internal static void HandleSelectionChanged(string text)
    {
        selectionText = text;
        selectionHandles.Clear();
        if (text.Length > 0)
        {
            selectionHandles.AddRange(pressedHandles);
        }
    }

    internal static bool CanShow(ulong handle)
    {
        for (var node = EventRouter.ResolveNode(handle); node is not null; node = node.Parent)
        {
            if (!node.IsContextMenuEnabled)
            {
                return false;
            }
        }
        return true;
    }

    internal static void Show(ulong handle, float x, float y)
    {
        var target = EventRouter.ResolveNode(handle);
        for (var node = target; node is not null; node = node.Parent)
        {
            if (!node.IsContextMenuEnabled || node.RaiseContextMenuOpening(new ContextMenuEventArgs(target, x, y)))
            {
                return;
            }
        }
        var items = BuildItems(handle, target, x, y);
        var navigation = new List<MenuItem>();
        var family = Platform.Family;
        if (Platform.Supports(HostCapability.BrowserHistory) && Runtime.Abi.HostCanNavigateBack())
        {
            navigation.Add(new MenuItem("Back", ContextMenuAction.NavigateBack) { ShortcutLabel = HistoryShortcut(family, forward: false) });
        }
        if (Platform.Supports(HostCapability.BrowserHistory) && Runtime.Abi.HostCanNavigateForward())
        {
            navigation.Add(new MenuItem("Forward", ContextMenuAction.NavigateForward) { ShortcutLabel = HistoryShortcut(family, forward: true) });
        }
        if (Platform.Supports(HostCapability.Reload))
        {
            navigation.Add(new MenuItem("Reload Page", ContextMenuAction.ReloadPage) { ShortcutLabel = KeyboardShortcuts.FormatPrimaryLabel("r") });
        }
        AppendSection(items, navigation);
        if (items.Count == 0 || (appMenu ?? Application.ShellContextMenu) is not { } menu)
        {
            ReleaseLinkPreview();
            return;
        }
        menu.VisibilityChanged -= OnMenuVisibilityChanged;
        menu.VisibilityChanged += OnMenuVisibilityChanged;
        menu.Items.Clear();
        menu.Items.AddRange(items);
        menu.Show(null, x, y);
    }

    /// <summary>
    /// The built-in items for a long press over <paramref name="handle"/>: the context menu's items without the page's
    /// history and reload items.
    /// </summary>
    internal static List<MenuItem> BuildLongPressItems(ulong handle, float x, float y)
    {
        var target = EventRouter.ResolveNode(handle);
        for (var node = target; node is not null; node = node.Parent)
        {
            if (!node.IsContextMenuEnabled)
            {
                return [];
            }
        }
        return BuildItems(handle, target, x, y, clearSelectionOnMiss: false);
    }

    /// <summary>Shows the long-press items over <paramref name="handle"/> in the text selection toolbar; returns whether there were any.</summary>
    internal static bool ShowForLongPress(ulong handle, float x, float y)
    {
        var items = BuildLongPressItems(handle, x, y);
        if (items.Count == 0)
        {
            ReleaseLinkPreview();
            return false;
        }
        return TextSelectionToolbar.ShowItemsAt(items, x, y);
    }

    private static void ClearMenu() => appMenu = null;

    private static void OnMenuVisibilityChanged(VisibilityChangedEventArgs e)
    {
        if (!e.Visible)
        {
            ReleaseLinkPreview();
        }
    }

    private static void ReleaseLinkPreview()
    {
        previewedLink?.ReleasePreviewForContextMenu();
        previewedLink = null;
    }

    private static List<MenuItem> BuildItems(ulong handle, Node? target, float x, float y, bool clearSelectionOnMiss = true)
    {
        ReleaseLinkPreview();
        var items = new List<MenuItem>();
        var link = Nearest<NavLink>(target);
        if (link is not null)
        {
            link.PinPreviewForContextMenu();
            previewedLink = link;
            var linkItems = new List<MenuItem>();
            if (Platform.Supports(HostCapability.NewBrowsingContext))
            {
                linkItems.Add(new MenuItem("New Tab", ContextMenuAction.OpenLinkInNewTab, link.Href));
            }
            if (Platform.Supports(HostCapability.OpenExternalUri))
            {
                linkItems.Add(new MenuItem("Open", ContextMenuAction.OpenLink, link.Href));
            }
            AppendSection(items, linkItems);
        }
        var selectionHit = Ui.IsPointInSelection(x, y) || (handle != 0 && selectionHandles.Contains(handle));
        var text = TextTarget(target);
        if (text is not null)
        {
            AppendSection(items, TextSection(text, selectionHit ? selectionText : string.Empty));
        }
        var imageUrl = ImageUrl(target, x, y);
        if (imageUrl is not null)
        {
            var image = new List<MenuItem>();
            if (Platform.Supports(HostCapability.NewBrowsingContext))
            {
                image.Add(new MenuItem("New Tab", ContextMenuAction.OpenImageInNewTab, imageUrl));
            }
            if (Platform.Supports(HostCapability.OpenExternalUri))
            {
                image.Add(new MenuItem("Open", ContextMenuAction.OpenImage, imageUrl));
            }
            AppendSection(items, image);
        }
        if (selectionHit && text is null && selectionText.Length > 0 && Platform.Supports(HostCapability.ClipboardWrite))
        {
            AppendSection(items, [new MenuItem("Copy", ContextMenuAction.CopyCurrentSelection, selectionText)]);
        }
        else if (clearSelectionOnMiss && !selectionHit && text is null && imageUrl is null && link is null)
        {
            selectionText = string.Empty;
            selectionHandles.Clear();
            Ui.ClearCurrentSelection();
        }
        return items;
    }

    /// <summary>
    /// Copy and Select All for selectable text, where Copy takes the page selection when the request is over it; editable
    /// text has Undo, Redo, Cut, Copy, Paste and Select All instead.
    /// </summary>
    private static List<MenuItem> TextSection(TextCore target, string currentSelection)
    {
        var items = new List<MenuItem>();
        if (!target.IsBuilt || !target.Selectable)
        {
            return items;
        }
        if (target.IsEditableText)
        {
            return EditableTextSection(target);
        }
        var handle = target.Handle;
        var hasText = target.Text.Length > 0;
        var selectAll = new MenuItem("Select All", ContextMenuAction.SelectAllText)
        {
            ShortcutLabel = KeyboardShortcuts.FormatPrimaryLabel("a"),
            IsEnabled = hasText,
            TargetHandle = handle,
        };
        if (Platform.Supports(HostCapability.ClipboardWrite))
        {
            var copyShortcut = KeyboardShortcuts.FormatPrimaryLabel("c");
            // The selection is copied as it is now: pressing the menu item can clear it before Copy runs.
            var selected = currentSelection.Length > 0 ? currentSelection : target.SelectedText;
            items.Add(selected.Length > 0
                ? new MenuItem("Copy", ContextMenuAction.CopyCurrentSelection, selected) { ShortcutLabel = copyShortcut }
                : new MenuItem("Copy", ContextMenuAction.CopyCurrentSelection)
                {
                    ShortcutLabel = copyShortcut,
                    IsEnabled = Ui.HasTextSelection(handle),
                    TargetHandle = handle,
                });
        }
        items.Add(selectAll);
        return items;
    }

    /// <summary>
    /// An editor's items. The host keeps a snapshot of its selection, so Cut and Copy still act on it after pressing the
    /// menu moves the editor's focus; each item returns focus to the editor.
    /// </summary>
    private static List<MenuItem> EditableTextSection(TextCore target)
    {
        var handle = target.Handle;
        Runtime.Abi.HostFreezeTextSelectionSnapshot(handle);
        // A text input tracks its editor's selection, including one set from code; plain editable text reports its own.
        var (start, end, content) = target is TextInputEditor editor
            ? (editor.Owner.SelectionStartByteOffset, editor.Owner.SelectionEndByteOffset, editor.Owner.Text)
            : (target.SelectionStart, target.SelectionEnd, target.Text);
        var hasSelection = Ui.HasTextSelection(handle) || Runtime.Abi.HostHasTextSelectionSnapshot(handle) || start != end;
        var selected = hasSelection ? target.TextBetween(start, end) : string.Empty;
        var payload = selected.Length > 0 ? selected : null;
        var items = new List<MenuItem>
        {
            EditorItem("Undo", ContextMenuAction.UndoTextEdit, KeyboardShortcuts.UndoLabel(), Ui.CanUndoTextEdit(handle), handle),
            EditorItem("Redo", ContextMenuAction.RedoTextEdit, KeyboardShortcuts.RedoLabel(), Ui.CanRedoTextEdit(handle), handle),
            MenuItem.Separator,
        };
        if (Platform.Supports(HostCapability.ClipboardWrite))
        {
            items.Add(EditorItem("Cut", ContextMenuAction.CutTextSelection, KeyboardShortcuts.FormatPrimaryLabel("x"), hasSelection, handle, payload, (start, end)));
            items.Add(EditorItem("Copy", ContextMenuAction.CopyCurrentSelection, KeyboardShortcuts.FormatPrimaryLabel("c"), hasSelection, handle, payload));
        }
        if (Platform.Supports(HostCapability.ClipboardRead))
        {
            items.Add(EditorItem("Paste", ContextMenuAction.PasteText, KeyboardShortcuts.FormatPrimaryLabel("v"), enabled: true, handle));
        }
        items.Add(EditorItem("Select All", ContextMenuAction.SelectAllText, KeyboardShortcuts.FormatPrimaryLabel("a"), content.Length > 0, handle));
        return items;
    }

    private static MenuItem EditorItem(
        string label,
        ContextMenuAction action,
        string shortcut,
        bool enabled,
        ulong handle,
        string? payload = null,
        (uint Start, uint End) selectionRange = default) =>
        new(label, action, payload)
        {
            ShortcutLabel = shortcut,
            IsEnabled = enabled,
            TargetHandle = handle,
            SelectionRange = selectionRange,
            FocusTargetAfterAction = true,
        };

    /// <summary>
    /// The URL of the image or SVG at or around the target, or one under the point among its descendants. Images are
    /// not hit targets, so with no target under the pointer the search starts from the page root.
    /// </summary>
    private static string? ImageUrl(Node? target, float x, float y)
    {
        if (target is null)
        {
            return Application.MountedRoot is { } root ? BrowsableImageUrl(root) ?? DescendantImageUrl(root, x, y) : null;
        }
        for (var node = target; node is not null; node = node.Parent)
        {
            if (BrowsableImageUrl(node) is { } url)
            {
                return url;
            }
        }
        return target is null ? null : DescendantImageUrl(target, x, y);
    }

    private static string? DescendantImageUrl(Node node, float x, float y)
    {
        foreach (var child in node.Children)
        {
            if (!child.IsBuilt || !child.GetBounds().Contains(x, y))
            {
                continue;
            }
            if ((BrowsableImageUrl(child) ?? DescendantImageUrl(child, x, y)) is { } url)
            {
                return url;
            }
        }
        return null;
    }

    /// <summary>An image's or SVG's URL, unless it is inline data the browser cannot open.</summary>
    private static string? BrowsableImageUrl(Node node)
    {
        var url = node switch
        {
            Image image => image.Source,
            Svg svg => svg.Source,
            _ => null,
        };
        return url is { Length: > 0 } && !url.StartsWith("data:", StringComparison.Ordinal) && !url.StartsWith("blob:", StringComparison.Ordinal)
            ? url
            : null;
    }

    /// <summary>The nearest text, where a text input stands for its editor, so a press on its padding or placeholder targets it.</summary>
    private static TextCore? TextTarget(Node? node)
    {
        for (; node is not null; node = node.Parent)
        {
            if (node.InnerEditor is { } editor)
            {
                return editor;
            }
            if (node is TextCore text)
            {
                return text;
            }
        }
        return null;
    }

    private static T? Nearest<T>(Node? node)
        where T : Node
    {
        for (; node is not null; node = node.Parent)
        {
            if (node is T match)
            {
                return match;
            }
        }
        return null;
    }

    private static void AppendSection(List<MenuItem> items, List<MenuItem> section)
    {
        if (section.Count == 0)
        {
            return;
        }
        if (items.Count > 0)
        {
            items.Add(MenuItem.Separator);
        }
        items.AddRange(section);
    }

    private static string HistoryShortcut(PlatformFamily family, bool forward) => family == PlatformFamily.Apple
        ? KeyboardShortcuts.FormatLabel(forward ? "]" : "[", KeyModifier.Meta, family)
        : KeyboardShortcuts.FormatLabel(forward ? "ArrowRight" : "ArrowLeft", KeyModifier.Alt, family);
}
