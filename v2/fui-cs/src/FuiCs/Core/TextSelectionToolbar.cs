using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// The touch toolbar over a text selection, and over a long-pressed link or image: up to three actions in a row,
/// with the rest behind a More button in a scrolling panel. Editable text has Cut, Copy, Paste and Select all. Copy
/// clears the selection; tapping outside dismisses it.
/// </summary>
internal static class TextSelectionToolbar
{
    private const float ToolbarMargin = 8.0f;
    private const float EdgeMargin = 8.0f;
    private const float ReadOnlyButtonWidth = 96.0f;
    private const float EditableButtonWidth = 72.0f;
    private const float OverflowButtonWidth = 44.0f;
    private const float OverflowMenuWidth = 168.0f;
    private const float OverflowMenuMaxHeight = 184.0f;
    private const float HorizontalPadding = 4.0f;
    private const int MaxHorizontalActions = 3;
    private const int OverflowSlot = -1;
    private const int BackSlot = -2;

    private static readonly List<MenuItem> items = [];
    private static FlexBox? panel;
    private static FlexBox? overflowPanel;
    private static ScrollView? overflowScroll;
    private static ulong activeHandle;
    private static uint activeStart;
    private static uint activeEnd;
    private static string crossSelectionText = string.Empty;
    private static ulong crossSelectAllTarget;
    private static ulong pendingCrossSelectionTextHandle;
    private static bool hiddenForHandleDrag;
    private static bool overflowVisible;
    private static float overflowHeight;
    private static (float X, float Y, float Width) lastPanel = (EdgeMargin, EdgeMargin, 0.0f);

    public static bool IsVisible => panel is { IsVisible: true };

    public static bool IsOverflowVisible => overflowPanel is { IsVisible: true };

    public static IReadOnlyList<MenuItem> Items => items;

    public static FlexBox? Panel => panel;

    public static FlexBox? OverflowPanel => overflowPanel;

    /// <summary>
    /// Shows <paramref name="toolbarItems"/> at a point, as for a long press; returns whether there were any. The
    /// toolbar draws its own dividers, so the menu's section separators are left out.
    /// </summary>
    public static bool ShowItemsAt(List<MenuItem> toolbarItems, float x, float y)
    {
        toolbarItems.RemoveAll(static item => item.IsSeparator);
        if (toolbarItems.Count == 0)
        {
            Clear();
            return false;
        }
        (activeHandle, activeStart, activeEnd) = (0, 0, 0);
        (crossSelectionText, crossSelectAllTarget, pendingCrossSelectionTextHandle, hiddenForHandleDrag) = (string.Empty, 0, 0, false);
        if (!EnsureHost())
        {
            return false;
        }
        items.Clear();
        items.AddRange(toolbarItems);
        ApplyItems(editable: false);
        PositionAtPoint(x, y);
        return true;
    }

    /// <summary>The text node a long press is selecting a word in, so Select all can target it once the area reports.</summary>
    public static void SetPendingCrossSelectionTextHandle(ulong handle) => pendingCrossSelectionTextHandle = handle;

    public static void HandleSelectionChanged(Node target, uint start, uint end, bool chromeVisible)
    {
        crossSelectionText = string.Empty;
        var text = target as TextCore;
        var textHandle = text is { IsBuilt: true } ? text.Handle : 0;
        if (start == end && hiddenForHandleDrag && textHandle != 0 && activeHandle == textHandle)
        {
            (activeStart, activeEnd) = (start, end);
            Hide();
            return;
        }
        if (!chromeVisible || start == end || text is null || !EnsureHost())
        {
            Clear();
            return;
        }
        (activeHandle, activeStart, activeEnd) = (textHandle, start, end);
        items.Clear();
        var selected = text.TextBetween(start, end);
        var payload = selected.Length > 0 ? selected : null;
        var editable = text.IsEditableText;
        if (editable)
        {
            items.Add(new MenuItem("Cut", ContextMenuAction.CutTextSelection, payload)
            {
                TargetHandle = textHandle,
                SelectionRange = (start, end),
                FocusTargetAfterAction = true,
            });
        }
        items.Add(new MenuItem("Copy", ContextMenuAction.CopyCurrentSelection, payload) { TargetHandle = textHandle, FocusTargetAfterAction = editable });
        if (editable)
        {
            items.Add(new MenuItem("Paste", ContextMenuAction.PasteText) { TargetHandle = textHandle, FocusTargetAfterAction = true });
        }
        items.Add(new MenuItem("Select all", ContextMenuAction.SelectAllText)
        {
            IsEnabled = text.Text.Length > 0,
            TargetHandle = textHandle,
            FocusTargetAfterAction = editable,
        });
        ApplyItems(editable);
        PositionForTextRange(textHandle, start, end);
    }

    public static void HandleCrossSelectionChanged(ulong handle, string text, bool chromeVisible)
    {
        if (!chromeVisible || text.Length == 0)
        {
            if (text.Length == 0 && hiddenForHandleDrag && activeHandle == handle)
            {
                (activeStart, activeEnd, crossSelectionText) = (0, 0, string.Empty);
                Hide();
                return;
            }
            Clear();
            return;
        }
        if (!EnsureHost())
        {
            return;
        }
        var previous = (activeHandle, crossSelectionText, crossSelectAllTarget);
        (activeHandle, activeStart, activeEnd, crossSelectionText) = (handle, 0, (uint)text.Length, text);
        var selectAllTarget = pendingCrossSelectionTextHandle;
        pendingCrossSelectionTextHandle = 0;
        if (selectAllTarget == 0 && previous.activeHandle == handle && previous.crossSelectionText == text)
        {
            selectAllTarget = previous.crossSelectAllTarget;
        }
        crossSelectAllTarget = selectAllTarget == 0 ? handle : selectAllTarget;
        items.Clear();
        items.Add(new MenuItem("Copy", ContextMenuAction.CopyCurrentSelection, text));
        items.Add(new MenuItem("Select all", ContextMenuAction.SelectAllText) { TargetHandle = crossSelectAllTarget });
        ApplyItems(editable: false);
        PositionForCrossSelection(handle);
    }

    public static void RefreshActiveGeometry(bool chromeVisible)
    {
        if (!chromeVisible || activeHandle == 0 || hiddenForHandleDrag)
        {
            Hide();
            return;
        }
        if (crossSelectionText.Length > 0)
        {
            PositionForCrossSelection(activeHandle);
        }
        else
        {
            PositionForTextRange(activeHandle, activeStart, activeEnd);
        }
    }

    public static void HideForHandleDrag()
    {
        hiddenForHandleDrag = true;
        overflowVisible = false;
        Hide();
    }

    public static void ShowAfterHandleDrag(bool chromeVisible)
    {
        hiddenForHandleDrag = false;
        RefreshActiveGeometry(chromeVisible);
    }

    /// <summary>Runs the item in <paramref name="slot"/>, or opens or closes the overflow panel.</summary>
    public static void Activate(int slot)
    {
        if (slot == OverflowSlot)
        {
            ShowOverflowMenu();
            return;
        }
        if (slot == BackSlot)
        {
            ShowHorizontalMenu();
            return;
        }
        if (slot < 0 || slot >= items.Count)
        {
            return;
        }
        var item = items[slot];
        item.Invoke();
        if (item.Action == ContextMenuAction.CopyCurrentSelection)
        {
            Ui.ClearCurrentSelection();
            Clear();
        }
        else if (item.Action != ContextMenuAction.SelectAllText)
        {
            Hide();
        }
    }

    /// <summary>Clears the toolbar when a press lands outside it; returns whether it did.</summary>
    public static bool DismissForOutsidePointerDown(float x, float y)
    {
        if (items.Count == 0 || Hits(panel, x, y) || Hits(overflowPanel, x, y))
        {
            return false;
        }
        Clear();
        return true;
    }

    public static void Clear()
    {
        (activeHandle, activeStart, activeEnd) = (0, 0, 0);
        (crossSelectionText, crossSelectAllTarget, pendingCrossSelectionTextHandle) = (string.Empty, 0, 0);
        items.Clear();
        (hiddenForHandleDrag, overflowVisible) = (false, false);
        Hide();
    }

    public static void Reset()
    {
        panel = null;
        overflowPanel = null;
        overflowScroll = null;
        Clear();
        lastPanel = (EdgeMargin, EdgeMargin, 0.0f);
    }

    private static float ItemHeight => Theme.Active.Value.ContextMenu.Item.Height;

    private static bool EnsureHost()
    {
        if (panel is not null)
        {
            return true;
        }
        if (Application.ShellTextToolbarHost is not { } host)
        {
            return false;
        }
        Application.Unmounting -= Reset;
        Application.Unmounting += Reset;
        panel = StylePanel(new FlexBox().PositionAbsolute().FlexDirection(FlexDirection.Row).AlignItems(AlignItems.Center)
            .Padding(HorizontalPadding, 4.0f));
        panel.Visibility = Visibility.Collapsed;
        panel.NodeId = "fui-text-toolbar";
        // A bare scroll view: the list scrolls by touch on short viewports and needs no scroll bar.
        overflowScroll = new ScrollView().HorizontalScrollEnabled(false).PersistScroll(false).Width(Length.Percent(100.0f));
        overflowScroll.PreserveSelectionOnPointerDown = true;
        overflowPanel = StylePanel(new FlexBox().PositionAbsolute().Width(OverflowMenuWidth).Child(overflowScroll));
        overflowPanel.Visibility = Visibility.Collapsed;
        overflowPanel.NodeId = "fui-text-toolbar-overflow";
        host.AddChild(panel);
        host.AddChild(overflowPanel);
        return true;
    }

    private static FlexBox StylePanel(FlexBox node)
    {
        var menu = Theme.Active.Value.ContextMenu;
        node.Background = menu.PanelBackground;
        node.BackgroundBlur = 10.0f;
        node.CornerRadius = new CornerRadius(menu.PanelCornerRadius);
        node.Border = new Border(1.0f, menu.PanelBorderColor);
        node.DropShadow = new DropShadow(menu.PanelShadowColor, 0.0f, menu.ShadowOffsetY, menu.ShadowBlur, menu.ShadowSpread);
        node.PreserveSelectionOnPointerDown = true;
        return node;
    }

    /// <summary>Fills the row with up to three actions, and the overflow panel with the rest and a back button.</summary>
    private static void ApplyItems(bool editable)
    {
        var buttonWidth = editable ? EditableButtonWidth : ReadOnlyButtonWidth;
        var hasOverflow = items.Count > MaxHorizontalActions;
        var visible = hasOverflow ? MaxHorizontalActions : items.Count;
        var row = new List<Node>();
        for (var index = 0; index < visible; ++index)
        {
            if (index > 0)
            {
                row.Add(Separator(vertical: true));
            }
            row.Add(new ToolbarButton(items[index].Label, buttonWidth, index));
        }
        if (hasOverflow)
        {
            row.Add(Separator(vertical: true));
            row.Add(new ToolbarButton("More", OverflowButtonWidth, OverflowSlot));
        }
        StylePanel(panel!);
        panel!.Height = ItemHeight + 8.0f;
        panel.Width = ToolbarWidth(buttonWidth, items.Count);
        panel.SetContent([.. row]);

        var overflowCount = hasOverflow ? items.Count - MaxHorizontalActions : 0;
        var column = new List<Node>();
        for (var index = 0; index < overflowCount; ++index)
        {
            column.Add(new ToolbarButton(items[MaxHorizontalActions + index].Label, OverflowMenuWidth, MaxHorizontalActions + index));
            column.Add(Separator(vertical: false));
        }
        column.Add(new ToolbarButton("<", OverflowMenuWidth, BackSlot));
        var rows = overflowCount + 1;
        var contentHeight = (ItemHeight * rows) + (rows - 1);
        var panelHeight = Math.Min(contentHeight, Math.Max(ItemHeight, Math.Min(OverflowMenuMaxHeight, Ui.GetViewportHeight() - (EdgeMargin * 2.0f))));
        StylePanel(overflowPanel!);
        overflowHeight = panelHeight;
        overflowPanel!.Height = panelHeight;
        overflowScroll!.Height = panelHeight;
        overflowScroll.SetScrollContentSize(null, contentHeight);
        overflowScroll.SetContent([FlexBox.Column([.. column]).FillWidth()]);
        ShowHorizontalMenu();
    }

    private static FlexBox Separator(bool vertical)
    {
        var separator = vertical
            ? new FlexBox().Width(1.0f).Height(ItemHeight - 10.0f)
            : new FlexBox().FillWidth().Height(1.0f);
        separator.Background = Theme.Active.Value.ContextMenu.SeparatorColor;
        return separator;
    }

    private static float ToolbarWidth(float buttonWidth, int count)
    {
        var hasOverflow = count > MaxHorizontalActions;
        var actions = hasOverflow ? MaxHorizontalActions : count;
        var cells = hasOverflow ? actions + 1 : actions;
        return (buttonWidth * actions) + (hasOverflow ? OverflowButtonWidth : 0.0f) + (cells - 1) + (HorizontalPadding * 2.0f);
    }

    private static void PositionForTextRange(ulong handle, uint start, uint end)
    {
        var rects = start == end ? [] : Runtime.Abi.UiReadTextRangeRects(handle, Math.Min(start, end), Math.Max(start, end));
        if (rects.Length < 4)
        {
            Hide();
            return;
        }
        var last = rects.Length - 4;
        PositionAtSelection(rects[0], rects[1], rects[3], rects[last + 1] + rects[last + 3]);
    }

    private static void PositionForCrossSelection(ulong handle)
    {
        var rects = Runtime.Abi.UiReadCrossSelectionEndpointRects(handle);
        if (rects.Length < 8)
        {
            Hide();
            return;
        }
        PositionAtSelection(rects[0], rects[1], rects[3], rects[5] + rects[7]);
    }

    /// <summary>Centers the toolbar on the selection's start, above it, or below when there is no room.</summary>
    private static void PositionAtSelection(float startX, float topY, float startHeight, float bottomY)
    {
        if (panel is null || hiddenForHandleDrag)
        {
            return;
        }
        var width = ToolbarWidth(items.Count > 2 ? EditableButtonWidth : ReadOnlyButtonWidth, items.Count);
        var height = ItemHeight + 8.0f;
        var viewportHeight = Ui.GetViewportHeight();
        var y = topY - height - ToolbarMargin;
        if (y < EdgeMargin)
        {
            y = bottomY + ToolbarMargin + 12.0f;
            if (y + height > viewportHeight - EdgeMargin)
            {
                y = topY + (startHeight * 0.5f) - (height * 0.5f);
            }
        }
        Place(startX - (width * 0.5f), y, width, height);
    }

    private static void PositionAtPoint(float x, float y)
    {
        if (panel is null || hiddenForHandleDrag || items.Count == 0)
        {
            return;
        }
        var width = ToolbarWidth(ReadOnlyButtonWidth, items.Count);
        var height = ItemHeight + 8.0f;
        var top = y - height - ToolbarMargin;
        Place(x - (width * 0.5f), top < EdgeMargin ? y + ToolbarMargin : top, width, height);
    }

    private static void Place(float x, float y, float width, float height)
    {
        var maxX = Math.Max(EdgeMargin, Ui.GetViewportWidth() - width - EdgeMargin);
        var maxY = Math.Max(EdgeMargin, Ui.GetViewportHeight() - height - EdgeMargin);
        x = Math.Max(EdgeMargin, Math.Min(x, maxX));
        y = Math.Max(EdgeMargin, Math.Min(y, maxY));
        lastPanel = (x, y, width);
        panel!.Position = new Point(x, y);
        if (overflowVisible)
        {
            PositionOverflowPanel(x, y, width, height);
        }
        else
        {
            panel.Visibility = Visibility.Normal;
        }
    }

    private static void ShowOverflowMenu()
    {
        if (panel is null || overflowPanel is null || items.Count <= MaxHorizontalActions)
        {
            return;
        }
        overflowVisible = true;
        panel.Visibility = Visibility.Collapsed;
        PositionOverflowPanel(lastPanel.X, lastPanel.Y, lastPanel.Width, ItemHeight + 8.0f);
        overflowPanel.Visibility = Visibility.Normal;
    }

    private static void ShowHorizontalMenu()
    {
        overflowVisible = false;
        if (overflowPanel is not null)
        {
            overflowPanel.Visibility = Visibility.Collapsed;
        }
        if (panel is not null && items.Count > 0 && !hiddenForHandleDrag)
        {
            panel.Visibility = Visibility.Normal;
        }
    }

    /// <summary>
    /// Opens the overflow panel under the toolbar's right end, or above it when there is no room; placed by its own
    /// height, so a short panel above the toolbar sits next to it.
    /// </summary>
    private static void PositionOverflowPanel(float x, float y, float toolbarWidth, float toolbarHeight)
    {
        var viewportHeight = Ui.GetViewportHeight();
        var height = overflowHeight;
        var maxX = Math.Max(EdgeMargin, Ui.GetViewportWidth() - OverflowMenuWidth - EdgeMargin);
        var overflowY = y + toolbarHeight + 4.0f;
        if (overflowY + height > viewportHeight - EdgeMargin)
        {
            overflowY = y - height - 4.0f;
        }
        var maxY = Math.Max(EdgeMargin, viewportHeight - height - EdgeMargin);
        overflowPanel!.Position = new Point(
            Math.Max(EdgeMargin, Math.Min(x + toolbarWidth - OverflowMenuWidth, maxX)),
            Math.Max(EdgeMargin, Math.Min(overflowY, maxY)));
    }

    private static void Hide()
    {
        if (panel is not null)
        {
            panel.Visibility = Visibility.Collapsed;
        }
        if (overflowPanel is not null)
        {
            overflowPanel.Visibility = Visibility.Collapsed;
        }
    }

    private static bool Hits(FlexBox? node, float x, float y) => node is { IsVisible: true, IsBuilt: true } && node.GetBounds().Contains(x, y);

    /// <summary>A toolbar action, the More button (three dots) or the back button; a release on it runs it.</summary>
    private sealed class ToolbarButton : FlexBox
    {
        private readonly int slot;

        public ToolbarButton(string label, float width, int slot)
        {
            this.slot = slot;
            var item = Theme.Active.Value.ContextMenu.Item;
            Width = width;
            Height = item.Height;
            AlignItems = FuiCs.AlignItems.Center;
            JustifyContent = FuiCs.JustifyContent.Center;
            Padding = item.Padding;
            CornerRadius = new CornerRadius(item.CornerRadius);
            Background = item.Background;
            SemanticRole = FuiCs.SemanticRole.Button;
            SemanticLabel = label;
            PreserveSelectionOnPointerDown = true;
            PointerDown += static _ => { };
            if (slot == OverflowSlot)
            {
                var dots = FlexBox.Column().Width(16.0f).Height(item.Height).AlignItems(FuiCs.AlignItems.Center).JustifyContent(FuiCs.JustifyContent.Center);
                for (var index = 0; index < 3; ++index)
                {
                    dots.Child(new FlexBox().Width(3.0f).Height(3.0f).Margin(0.0f, 1.25f, 0.0f, 1.25f).CornerRadius(2.0f).Background(item.TextColor));
                }
                AddChild(dots);
            }
            else
            {
                var text = new TextCore(label).FontFamily(item.FontFamily).FontSize(item.FontSize).TextColor(item.TextColor)
                    .Overflow(TextOverflow.Ellipsis).Selectable(false);
                text.PreserveSelectionOnPointerDown = true;
                AddChild(text);
            }
        }

        protected internal override void HandlePointerEvent(PointerEventArgs e)
        {
            base.HandlePointerEvent(e);
            if (e.EventType == PointerEventType.Up)
            {
                e.Handled = true;
                Activate(slot);
            }
        }

        protected internal override bool HandleBubbledPointerEvent(PointerEventArgs e)
        {
            if (e.EventType == PointerEventType.Up)
            {
                e.Handled = true;
                Activate(slot);
                return true;
            }
            return base.HandleBubbledPointerEvent(e);
        }
    }
}
