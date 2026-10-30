namespace FuiCs;

/// <summary>The resolved look of a context menu's items.</summary>
internal readonly record struct MenuItemStyle(
    float Height,
    Thickness Padding,
    float CornerRadius,
    Color TextColor,
    Color MutedTextColor,
    Color Background,
    Color HoverBackground,
    FontFamily FontFamily,
    float FontSize,
    FontWeight FontWeight,
    FontStyle FontStyle);

/// <summary>A context menu row: the label, and the shortcut hint in a column shared by every row of the menu.</summary>
internal sealed class ContextMenuEntry : Grid
{
    private readonly ContextMenu menu;
    private readonly TextCore label;
    private readonly TextCore shortcut;
    private MenuItem? item;
    private MenuItemStyle style;
    private bool hovered;
    private bool pressed;

    public ContextMenuEntry(ContextMenu menu)
    {
        this.menu = menu;
        label = new TextCore().Overflow(TextOverflow.Ellipsis).Selectable(false);
        shortcut = new TextCore().TextAlign(TextAlign.Left).Selectable(false);
        Width = Length.Percent(100.0f);
        SemanticRole = FuiCs.SemanticRole.Button;
        RequireInteractive();
        this.Columns(GridLength.Star(), GridLength.Auto).Rows(GridLength.Star()).ColumnSharedSizeGroup(1, "ContextMenuShortcutColumn");
        Place(label, 0, 0);
        Place(shortcut, 0, 1);
    }

    public MenuItem? Item => item;

    public void Show(MenuItem next, MenuItemStyle nextStyle)
    {
        item = next;
        hovered = false;
        pressed = false;
        SemanticLabel = next.Label;
        SemanticDisabled = !next.IsEnabled;
        Cursor = next.IsEnabled ? CursorStyle.Pointer : CursorStyle.Default;
        label.Text = next.Label;
        shortcut.Text = next.ShortcutLabel ?? string.Empty;
        Apply(nextStyle);
    }

    public void Apply(MenuItemStyle nextStyle)
    {
        style = nextStyle;
        var enabled = item?.IsEnabled ?? true;
        Height = style.Height;
        Padding = style.Padding;
        CornerRadius = new CornerRadius(style.CornerRadius);
        Background = hovered && enabled ? style.HoverBackground : style.Background;
        foreach (var text in (ReadOnlySpan<TextCore>)[label, shortcut])
        {
            text.FontFamily = style.FontFamily;
            text.FontWeight = style.FontWeight;
            text.FontStyle = style.FontStyle;
            text.FontSize = style.FontSize;
        }
        label.TextColor = enabled ? style.TextColor : style.MutedTextColor;
        shortcut.TextColor = style.MutedTextColor;
    }

    /// <summary>
    /// Highlights under the pointer and runs the item on a primary release over it. A release over a disabled item
    /// does nothing and keeps the menu open.
    /// </summary>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        var enabled = item?.IsEnabled ?? false;
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                hovered = enabled;
                Apply(style);
                break;
            case PointerEventType.Leave:
                hovered = false;
                pressed = false;
                Apply(style);
                break;
            case PointerEventType.Down:
                pressed = enabled && e.IsPrimaryActivation;
                break;
            case PointerEventType.Up:
                var over = GetBounds().Contains(e.SceneX, e.SceneY);
                var activate = pressed && enabled && over;
                pressed = false;
                e.Handled |= over;
                if (activate)
                {
                    menu.Invoke(item!);
                }
                break;
            case PointerEventType.Cancel:
                pressed = false;
                break;
        }
    }
}

/// <summary>A thin rule between groups of context menu items.</summary>
internal sealed class ContextMenuSeparator : FlexBox
{
    public const float RowHeight = 9.0f;

    private readonly FlexBox line = new FlexBox().FillWidth().Height(1.0f);

    public ContextMenuSeparator()
    {
        Width = Length.Percent(100.0f);
        Height = RowHeight;
        Padding = new Thickness(0.0f, 4.0f);
        AddChild(line);
    }

    public Color Color
    {
        set => line.Background = value;
    }
}
