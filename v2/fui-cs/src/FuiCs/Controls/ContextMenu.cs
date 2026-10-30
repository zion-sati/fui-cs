using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A menu of <see cref="Items"/> shown at a point over the page. Choosing an item runs it and hides the menu; Escape,
/// or a press outside the menu, hides it. The application shows a built-in menu on right-click; a node can replace
/// it through <see cref="Node.ContextMenuOpening"/>. Mount the menu before showing it.
/// </summary>
public class ContextMenu : Portal
{
    private const float EdgePadding = 8.0f;
    private const float PanelPadding = 4.0f;

    private static ContextMenu? active;

    private readonly FlexBox panel;
    private readonly PopupPresenter presenter;
    private readonly List<ContextMenuEntry> entries = [];
    private readonly List<ContextMenuSeparator> separators = [];
    private IDisposable? keyFilter;
    private float menuWidth = 220.0f;
    private float? itemHeight;
    private Thickness? itemPadding;
    private Color? panelColor;
    private Border? panelBorder;
    private float? panelCornerRadius;
    private DropShadow? panelShadow;
    private float panelBackgroundBlur = 10.0f;
    private Color? itemColor;
    private Color? itemHoverColor;
    private Color? itemTextColor;
    private float? itemCornerRadius;
    private FontFamily? itemFontFamily;
    private FontWeight? itemFontWeight;
    private FontStyle? itemFontStyle;
    private float? itemFontSize;
    private Color? separatorColor;

    /// <summary>An empty menu; fill Items, mount it, then show it.</summary>
    public ContextMenu()
    {
        panel = new FlexBox().PositionAbsolute().Padding(PanelPadding).GridSharedSizeScope();
        presenter = new PopupPresenter(this, panel);
        PositionType = FuiCs.PositionType.Absolute;
        Position = new Point(0.0f, 0.0f);
        Width = Length.Percent(100.0f);
        Height = Length.Percent(100.0f);
        presenter.Overlay.PointerUp += HandleOverlayPointerUp;
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Refresh();
    }

    /// <summary>Raised when the menu shows and hides.</summary>
    public event Action<VisibilityChangedEventArgs>? VisibilityChanged;

    /// <summary>The open menu, if any.</summary>
    public static ContextMenu? Active => active;

    /// <summary>The items the next <see cref="Show"/> displays.</summary>
    public List<MenuItem> Items { get; } = [];

    /// <summary>Whether the menu is showing.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>The menu's width in pixels.</summary>
    public float MenuWidth
    {
        get => menuWidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            menuWidth = value;
            Refresh();
        }
    }

    /// <summary>The height of each item; null follows the theme.</summary>
    public float? ItemHeight
    {
        get => itemHeight;
        set
        {
            itemHeight = LabeledControlSizing.Positive(value);
            Refresh();
        }
    }

    /// <summary>The items' padding; null follows the theme.</summary>
    public Thickness? ItemPadding { get => itemPadding; set => Set(ref itemPadding, value); }

    /// <summary>The panel's fill; null follows the theme.</summary>
    public Color? PanelColor { get => panelColor; set => Set(ref panelColor, value); }

    /// <summary>The panel's border; null follows the theme.</summary>
    public Border? PanelBorder { get => panelBorder; set => Set(ref panelBorder, value); }

    /// <summary>The panel's corner radius; null follows the theme.</summary>
    public float? PanelCornerRadius { get => panelCornerRadius; set => Set(ref panelCornerRadius, value); }

    /// <summary>The panel's shadow; null follows the theme.</summary>
    public DropShadow? PanelShadow { get => panelShadow; set => Set(ref panelShadow, value); }

    /// <summary>The blur of the page behind the panel.</summary>
    public float PanelBackgroundBlur
    {
        get => panelBackgroundBlur;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            panelBackgroundBlur = value;
            Refresh();
        }
    }

    /// <summary>The backdrop over the page while the menu is open.</summary>
    public Color BackdropColor
    {
        get => presenter.BackdropColor;
        set => presenter.BackdropColor = value;
    }

    /// <summary>The blur of the page behind the backdrop.</summary>
    public override float BackgroundBlur
    {
        get => presenter.BackgroundBlur;
        set => presenter.BackgroundBlur = value;
    }

    /// <summary>The items' fill; null follows the theme.</summary>
    public Color? ItemColor { get => itemColor; set => Set(ref itemColor, value); }

    /// <summary>The fill of the item under the pointer; null follows the theme.</summary>
    public Color? ItemHoverColor { get => itemHoverColor; set => Set(ref itemHoverColor, value); }

    /// <summary>The items' text color; null follows the theme.</summary>
    public Color? ItemTextColor { get => itemTextColor; set => Set(ref itemTextColor, value); }

    /// <summary>The items' corner radius; null follows the theme.</summary>
    public float? ItemCornerRadius { get => itemCornerRadius; set => Set(ref itemCornerRadius, value); }

    /// <summary>The items' font family; null follows the theme.</summary>
    public FontFamily? ItemFontFamily { get => itemFontFamily; set => Set(ref itemFontFamily, value); }

    /// <summary>The items' font weight; regular when null.</summary>
    public FontWeight? ItemFontWeight { get => itemFontWeight; set => Set(ref itemFontWeight, value); }

    /// <summary>The items' font style; upright when null.</summary>
    public FontStyle? ItemFontStyle { get => itemFontStyle; set => Set(ref itemFontStyle, value); }

    /// <summary>The items' font size; null follows the theme.</summary>
    public float? ItemFontSize { get => itemFontSize; set => Set(ref itemFontSize, value); }

    /// <summary>The dividers' color; null follows the theme.</summary>
    public Color? SeparatorColor { get => separatorColor; set => Set(ref separatorColor, value); }

    /// <summary>Hides the open menu, if any.</summary>
    public static void HideActive() => active?.Hide();

    /// <summary>
    /// Shows <see cref="Items"/> at <paramref name="x"/>, <paramref name="y"/>, relative to <paramref name="target"/> or
    /// to the page when it is null, kept inside the viewport.
    /// </summary>
    public void Show(Node? target, float x, float y)
    {
        if (target is not null)
        {
            var bounds = target.GetBounds();
            x += bounds.X;
            y += bounds.Y;
        }
        ShowAt(x, y);
    }

    /// <summary>Hides the menu.</summary>
    public void Hide()
    {
        if (!IsOpen && !presenter.IsOpen)
        {
            return;
        }
        ClearPanel();
        presenter.Hide();
        IsOpen = false;
        if (ReferenceEquals(active, this))
        {
            active = null;
            EventRouter.PointerDownOutside = null;
        }
        keyFilter?.Dispose();
        keyFilter = null;
        VisibilityChanged?.Invoke(new VisibilityChangedEventArgs(false));
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Hide();
        presenter.Dispose();
        foreach (var entry in entries)
        {
            entry.Dispose();
        }
        foreach (var separator in separators)
        {
            separator.Dispose();
        }
        base.Dispose();
    }

    /// <summary>Hides the open menu when a press lands outside its panel; returns whether it did.</summary>
    internal static bool DismissForOutsidePointerDown(float sceneX, float sceneY)
    {
        if (active is not { IsOpen: true } menu || menu.panel.GetBounds().Contains(sceneX, sceneY))
        {
            return false;
        }
        menu.Hide();
        return true;
    }

    internal void Invoke(MenuItem item)
    {
        Hide();
        item.Invoke();
    }

    private void ShowAt(float x, float y)
    {
        ClearPanel();
        var style = ItemStyle(Theme.Active.Value);
        var height = PanelPadding * 2.0f;
        var entryCount = 0;
        var separatorCount = 0;
        var afterSeparator = true;
        for (var index = 0; index < Items.Count; ++index)
        {
            var item = Items[index];
            if (item.IsSeparator)
            {
                if (afterSeparator || index == Items.Count - 1)
                {
                    continue;
                }
                var separator = Pooled(separators, separatorCount++, static () => new ContextMenuSeparator());
                panel.AddChild(separator);
                height += ContextMenuSeparator.RowHeight;
                afterSeparator = true;
                continue;
            }
            var entry = Pooled(entries, entryCount++, () => new ContextMenuEntry(this));
            entry.Show(item, style);
            panel.AddChild(entry);
            height += style.Height;
            afterSeparator = false;
        }
        Refresh();
        var maxX = Math.Max(0.0f, Ui.GetViewportWidth() - menuWidth - EdgePadding);
        var maxY = Math.Max(0.0f, Ui.GetViewportHeight() - height - EdgePadding);
        presenter.ShowAtPoint(Math.Max(EdgePadding, Math.Min(x, maxX)), Math.Max(EdgePadding, Math.Min(y, maxY)), menuWidth, height);
        IsOpen = true;
        if (active is { } previous && !ReferenceEquals(previous, this))
        {
            previous.Hide();
        }
        active = this;
        EventRouter.PointerDownOutside = DismissForOutsidePointerDown;
        keyFilter ??= EventRouter.PushKeyFilter(HandleKey);
        VisibilityChanged?.Invoke(new VisibilityChangedEventArgs(true));
    }

    private static T Pooled<T>(List<T> pool, int index, Func<T> create)
    {
        if (index == pool.Count)
        {
            pool.Add(create());
        }
        return pool[index];
    }

    private bool HandleKey(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        if (eventType == KeyEventType.Down && key == "Escape")
        {
            Hide();
            return true;
        }
        return false;
    }

    /// <summary>
    /// A primary release on the backdrop runs the item under it or hides the menu; other releases, such as the one
    /// of the right-click that opened the menu, are ignored.
    /// </summary>
    private void HandleOverlayPointerUp(PointerEventArgs e)
    {
        if (!IsOpen || e.Handled)
        {
            return;
        }
        e.Handled = true;
        if (!e.IsPrimaryActivation)
        {
            return;
        }
        foreach (var entry in entries)
        {
            if (entry.Parent is not null && entry.Item is { IsEnabled: true } item && entry.GetBounds().Contains(e.SceneX, e.SceneY))
            {
                Invoke(item);
                return;
            }
        }
        Hide();
    }

    private void ClearPanel()
    {
        foreach (var entry in entries)
        {
            panel.RemoveChild(entry);
        }
        foreach (var separator in separators)
        {
            panel.RemoveChild(separator);
        }
    }

    private MenuItemStyle ItemStyle(Theme theme)
    {
        var item = theme.ContextMenu.Item;
        return new MenuItemStyle(
            itemHeight ?? item.Height,
            itemPadding ?? item.Padding,
            itemCornerRadius ?? item.CornerRadius,
            itemTextColor ?? item.TextColor,
            theme.Colors.TextMuted,
            itemColor ?? item.Background,
            itemHoverColor ?? item.HoverBackground,
            itemFontFamily ?? item.FontFamily,
            itemFontSize ?? item.FontSize,
            itemFontWeight ?? FuiCs.FontWeight.Regular,
            itemFontStyle ?? FuiCs.FontStyle.Normal);
    }

    private void Refresh()
    {
        var theme = Theme.Active.Value;
        var menuTheme = theme.ContextMenu;
        panel.Width = menuWidth;
        panel.Background = panelColor ?? menuTheme.PanelBackground;
        panel.BackgroundBlur = panelBackgroundBlur;
        panel.CornerRadius = new CornerRadius(panelCornerRadius ?? menuTheme.PanelCornerRadius);
        panel.Border = panelBorder ?? new Border(1.0f, menuTheme.PanelBorderColor);
        panel.DropShadow = panelShadow ?? new DropShadow(menuTheme.PanelShadowColor, 0.0f, menuTheme.ShadowOffsetY, menuTheme.ShadowBlur, menuTheme.ShadowSpread);
        var style = ItemStyle(theme);
        foreach (var entry in entries)
        {
            entry.Apply(style);
        }
        foreach (var separator in separators)
        {
            separator.Color = separatorColor ?? menuTheme.SeparatorColor;
        }
    }

    private void Set<T>(ref T field, T value)
    {
        field = value;
        Refresh();
    }
}
