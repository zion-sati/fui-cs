using FuiCs.Bindings;

namespace FuiCs;

/// <summary>An option of a <see cref="Dropdown"/>: its value and the label the field and list show.</summary>
public sealed record DropdownItem(string Value, string Label)
{
    /// <summary>An item whose label is its value.</summary>
    public DropdownItem(string value)
        : this(value, value)
    {
    }
}

/// <summary>
/// Picks one of <see cref="Items"/> from a list that opens under its field. A press, Enter, Space or the down arrow
/// opens the list; while it is open the arrows, Home and End move the highlight, Enter or a release on a row selects,
/// and Escape, a press outside the list, or scrolling the field out of view closes it. Focus stays on the field. Its
/// selection persists across history navigation.
/// </summary>
public class Dropdown : FlexBox, ISelectablePopupListOwner
{
    private const float DefaultPanelBackgroundBlur = 10.0f;

    private static readonly PersistedValue<Dropdown, int> PersistedSelection = new(
        "dropdown-selected-index",
        dropdown => dropdown.selectedIndex,
        (dropdown, index) => dropdown.RestoreSelectedIndex(index),
        PersistedCodecs.Int32)
    {
        ShouldCapture = dropdown => dropdown.selectedIndex >= 0,
    };

    private static Dropdown? active;
    private static bool scrollHookRegistered;

    private readonly SelectablePopupList popupList;
    private readonly List<DropdownItem> items = [];
    private DropdownFieldPresenter fieldPresenter;
    private DropdownChevronPresenter chevronPresenter;
    private DropdownFieldTemplate? fieldTemplate;
    private DropdownChevronTemplate? chevronTemplate;
    private DropdownSizing? sizing;
    private DropdownColors? colors;
    private Color? popupPanelColor;
    private float? popupPanelBackgroundBlur;
    private IDisposable? keyFilter;
    private int selectedIndex = -1;
    private bool pointerPressed;
    private bool hovered;
    private bool focused;
    private bool disposed;

    /// <summary>An empty dropdown; assign its items.</summary>
    public Dropdown()
    {
        EnsureScrollHook();
        fieldPresenter = DropdownFieldTemplate.Default.CreatePresenter(null);
        chevronPresenter = DropdownChevronTemplate.Default.CreatePresenter(null);
        popupList = new SelectablePopupList(this);
        popupList.Presenter.Overlay.PointerClick += _ => Close();
        SemanticRole = FuiCs.SemanticRole.ComboBox;
        SetFocusable(true, 0);
        RequireInteractive();
        ReflectSemanticDisabledFromEnabled();
        Cursor = CursorStyle.Pointer;
        FlexDirection = FuiCs.FlexDirection.Row;
        AlignItems = FuiCs.AlignItems.Center;
        fieldPresenter.Root.FillWidth();
        fieldPresenter.ChevronHost.AddChild(chevronPresenter.Root);
        AddChild(fieldPresenter.Root);
        AddChild(popupList.Root);
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => Refresh()));
        SemanticExpanded = false;
        DefaultSemanticLabel = "Dropdown";
        RegisterPersistedState(PersistedSelection);
        Refresh();
    }

    /// <summary>Raised when the user selects another item, or a restored selection differs; not when code sets it.</summary>
    public event Action<DropdownChangedEventArgs<DropdownItem>>? Changed;

    /// <summary>The open dropdown, if any.</summary>
    public static Dropdown? Active => active;

    /// <summary>The options. Assigning them selects the first, or nothing when there are none.</summary>
    public IReadOnlyList<DropdownItem> Items
    {
        get => items;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Close();
            items.Clear();
            popupList.Clear();
            items.AddRange(value);
            selectedIndex = items.Count > 0 ? 0 : -1;
            popupList.RefreshPanelLayout();
            SyncValueLabel();
            Refresh();
        }
    }

    /// <summary>The selected item's index, or -1 for none. Setting it does not raise <see cref="Changed"/>.</summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value != -1 && (value < 0 || value >= items.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The index must be -1 or the index of an item.");
            }
            SetSelectedIndex(value, emit: false);
        }
    }

    /// <summary>The selected item, if any.</summary>
    public DropdownItem? SelectedItem => selectedIndex >= 0 ? items[selectedIndex] : null;

    /// <summary>The item the keyboard or pointer highlights in the open list, or -1.</summary>
    public int HighlightedIndex => popupList.HighlightedIndex;

    /// <summary>Whether the list is open.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>The most rows the list shows before it scrolls; null shows every row.</summary>
    public int? MaxVisibleItems
    {
        get => popupList.MaxVisibleItems;
        set => popupList.MaxVisibleItems = value;
    }

    /// <summary>The list's width; null matches the field.</summary>
    public float? PopupWidth
    {
        get => popupList.PopupWidth;
        set => popupList.PopupWidth = value;
    }

    /// <summary>The list panel's fill; null follows the theme's context menu panel.</summary>
    public Color? PopupPanelColor
    {
        get => popupPanelColor;
        set
        {
            popupPanelColor = value;
            Refresh();
        }
    }

    /// <summary>The blur of the page behind the list panel; null is the default blur.</summary>
    public float? PopupPanelBackgroundBlur
    {
        get => popupPanelBackgroundBlur;
        set
        {
            if (value is < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The blur must not be negative.");
            }
            popupPanelBackgroundBlur = value;
            Refresh();
        }
    }

    /// <summary>The field's and rows' sizes; the default templates' when null.</summary>
    public DropdownSizing? Sizing
    {
        get => sizing;
        set
        {
            Close();
            sizing = value;
            if (fieldTemplate is null)
            {
                ReplaceFieldPresenter();
            }
            else if (chevronTemplate is null)
            {
                ReplaceChevronPresenter();
            }
            popupList.Sizing = value;
            SyncValueLabel();
            Refresh();
        }
    }

    /// <summary>Colors over the theme's, carried into the field and list.</summary>
    public DropdownColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            popupList.Colors = value;
            Refresh();
        }
    }

    /// <summary>Builds the field; null uses <see cref="DropdownFieldTemplate.Default"/>.</summary>
    public DropdownFieldTemplate? FieldTemplate
    {
        get => fieldTemplate;
        set
        {
            Close();
            fieldTemplate = value;
            ReplaceFieldPresenter();
            SyncValueLabel();
            Refresh();
        }
    }

    /// <summary>Builds the chevron; null uses <see cref="DropdownChevronTemplate.Default"/>.</summary>
    public DropdownChevronTemplate? ChevronTemplate
    {
        get => chevronTemplate;
        set
        {
            Close();
            chevronTemplate = value;
            ReplaceChevronPresenter();
            Refresh();
        }
    }

    /// <summary>Builds the list's rows; null uses <see cref="DropdownOptionRowTemplate.Default"/>.</summary>
    public DropdownOptionRowTemplate? OptionRowTemplate
    {
        get => popupList.OptionRowTemplate;
        set
        {
            Close();
            popupList.OptionRowTemplate = value;
        }
    }

    internal SelectablePopupList PopupList => popupList;

    int ISelectablePopupListOwner.PopupListItemCount => items.Count;

    bool ISelectablePopupListOwner.IsPopupListEnabled => IsEnabled;

    /// <summary>Closes the open dropdown, if any.</summary>
    public static void HideActive() => active?.Close();

    /// <summary>Highlights an item of the open list.</summary>
    public void HighlightIndex(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "The index must be the index of an item.");
        }
        popupList.HighlightIndex(index);
    }

    /// <summary>Selects the highlighted item, raising <see cref="Changed"/> when it differs, and closes the list.</summary>
    public void SelectHighlighted()
    {
        var highlighted = popupList.HighlightedIndex;
        if (highlighted < 0 || highlighted >= items.Count)
        {
            return;
        }
        SetSelectedIndex(highlighted, emit: true);
        Close();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Close();
        if (!disposed)
        {
            disposed = true;
            popupList.Dispose();
            FocusAdorner.Hide(this);
        }
        base.Dispose();
    }

    /// <inheritdoc/>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        if (!IsEnabled)
        {
            return;
        }
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                hovered = true;
                Refresh();
                break;
            case PointerEventType.Leave:
                pointerPressed = false;
                hovered = false;
                Refresh();
                break;
            case PointerEventType.Down:
                pointerPressed = true;
                Refresh();
                break;
            case PointerEventType.Up when pointerPressed:
                pointerPressed = false;
                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
                Refresh();
                break;
        }
    }

    /// <inheritdoc/>
    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (!IsEnabled || modifiers != KeyModifier.None || eventType != KeyEventType.Down || IsOpen)
        {
            return handled;
        }
        if (key is "Enter" or " " or "ArrowDown")
        {
            Open();
            return true;
        }
        if (key == "ArrowUp")
        {
            Open();
            MoveHighlight(-1);
            return true;
        }
        return handled;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        this.focused = focused;
        if (!focused && !IsOpen)
        {
            pointerPressed = false;
        }
        Refresh();
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        if (!isEnabled)
        {
            pointerPressed = false;
            hovered = false;
            Close();
        }
        Refresh();
    }

    private static void EnsureScrollHook()
    {
        if (scrollHookRegistered)
        {
            return;
        }
        EventRouter.Scrolled += DismissActiveIfTriggerScrolledAway;
        scrollHookRegistered = true;
    }

    /// <summary>Closes the open dropdown once its field leaves the scroll view around it, or the viewport.</summary>
    private static void DismissActiveIfTriggerScrolledAway()
    {
        if (active is { } dropdown && !dropdown.IsTriggerVisible())
        {
            dropdown.Close();
        }
    }

    private bool IsTriggerVisible()
    {
        if (!IsBuilt)
        {
            return true;
        }
        var bounds = GetBounds();
        if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
        {
            return true;
        }
        var clip = new Rect(0.0f, 0.0f, Ui.GetViewportWidth(), Ui.GetViewportHeight());
        for (var node = Parent; node is not null; node = node.Parent)
        {
            if (node is ScrollView { IsBuilt: true } scrollView)
            {
                clip = scrollView.GetBounds();
                break;
            }
        }
        return bounds.X + bounds.Width > clip.X
            && bounds.Y + bounds.Height > clip.Y
            && bounds.X < clip.X + clip.Width
            && bounds.Y < clip.Y + clip.Height;
    }

    private void RestoreSelectedIndex(int index)
    {
        if (items.Count == 0)
        {
            return;
        }
        SetSelectedIndex(Math.Clamp(index, 0, items.Count - 1), emit: true);
    }

    private void SetSelectedIndex(int index, bool emit)
    {
        var changed = selectedIndex != index;
        selectedIndex = index;
        popupList.SetHighlightedIndex(index);
        SyncValueLabel();
        Refresh();
        if (emit && changed && index >= 0)
        {
            RequestSemanticAnnouncement();
            Changed?.Invoke(new DropdownChangedEventArgs<DropdownItem>(items[index], index));
        }
    }

    private void Open()
    {
        if (IsOpen || items.Count == 0 || !IsBuilt)
        {
            return;
        }
        var initialHighlight = selectedIndex >= 0 ? selectedIndex : 0;
        popupList.SetHighlightedIndex(initialHighlight);
        if (!popupList.Open(GetBounds(), initialHighlight))
        {
            return;
        }
        IsOpen = true;
        if (active is { } previous && !ReferenceEquals(previous, this))
        {
            previous.Close();
        }
        active = this;
        SemanticExpanded = true;
        RequestSemanticAnnouncement();
        keyFilter ??= EventRouter.PushKeyFilter(HandleOpenKey);
        Refresh();
    }

    private void Close()
    {
        if (!IsOpen && !popupList.IsOpen)
        {
            return;
        }
        popupList.Close();
        IsOpen = false;
        if (ReferenceEquals(active, this))
        {
            active = null;
        }
        SemanticExpanded = false;
        RequestSemanticAnnouncement();
        keyFilter?.Dispose();
        keyFilter = null;
        Refresh();
    }

    /// <summary>The keys of the open list, ahead of the focused node: Escape, Enter, Home, End and the arrows.</summary>
    private bool HandleOpenKey(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        if (!IsOpen || modifiers != KeyModifier.None || eventType != KeyEventType.Down)
        {
            return false;
        }
        switch (key)
        {
            case "Escape":
                Close();
                return true;
            case "Enter":
                SelectHighlighted();
                return true;
            case "Home":
                popupList.HighlightIndex(0);
                return true;
            case "End":
                popupList.HighlightIndex(items.Count - 1);
                return true;
            case "ArrowDown":
                MoveHighlight(1);
                return true;
            case "ArrowUp":
                MoveHighlight(-1);
                return true;
            default:
                return false;
        }
    }

    private void MoveHighlight(int delta)
    {
        if (popupList.HighlightedIndex < 0 && selectedIndex >= 0)
        {
            popupList.SetHighlightedIndex(selectedIndex);
        }
        popupList.MoveHighlight(delta);
    }

    private void SyncValueLabel()
    {
        var label = SelectedItem?.Label;
        fieldPresenter.ValueNode.Text = label ?? string.Empty;
        DefaultSemanticLabel = label ?? "Dropdown";
    }

    private void ReplaceFieldPresenter()
    {
        var previousField = fieldPresenter;
        fieldPresenter = (fieldTemplate ?? DropdownFieldTemplate.Default).CreatePresenter(sizing);
        chevronPresenter = (chevronTemplate ?? DropdownChevronTemplate.Default).CreatePresenter(sizing);
        fieldPresenter.Root.FillWidth();
        fieldPresenter.ChevronHost.AddChild(chevronPresenter.Root);
        ReplaceChildren([fieldPresenter.Root, popupList.Root]);
        // The previous chevron sits in the previous field and goes with it.
        previousField.Root.Dispose();
    }

    private void ReplaceChevronPresenter()
    {
        var previous = chevronPresenter;
        chevronPresenter = (chevronTemplate ?? DropdownChevronTemplate.Default).CreatePresenter(sizing);
        fieldPresenter.ChevronHost.RemoveChild(previous.Root);
        fieldPresenter.ChevronHost.AddChild(chevronPresenter.Root);
        previous.Root.Dispose();
    }

    private void Refresh()
    {
        if (disposed)
        {
            return;
        }
        var theme = Theme.Active.Value;
        var menu = theme.ContextMenu;
        Cursor = IsEnabled ? CursorStyle.Pointer : CursorStyle.Default;
        Opacity = IsEnabled ? 1.0f : 0.6f;
        fieldPresenter.Root.FillWidth();
        fieldPresenter.Apply(theme, new DropdownFieldVisualState(IsOpen, focused, IsEnabled, pointerPressed, SelectedItem?.Label ?? string.Empty), colors);
        chevronPresenter.Apply(theme, new DropdownChevronVisualState(IsOpen, hovered, IsEnabled));
        var panel = popupList.Panel;
        panel.Padding = new Thickness(SelectablePopupList.PanelPadding);
        panel.CornerRadius = new CornerRadius(theme.Spacing.Sm);
        panel.Background = popupPanelColor ?? menu.PanelBackground;
        panel.Border = new Border(1.0f, menu.PanelBorderColor);
        panel.BackgroundBlur = popupPanelBackgroundBlur ?? DefaultPanelBackgroundBlur;
        panel.DropShadow = new DropShadow(menu.PanelShadowColor, 0.0f, menu.ShadowOffsetY, menu.ShadowBlur, menu.ShadowSpread);
        popupList.ScrollBox.Background = Color.Transparent;
        popupList.SyncOptionVisuals();
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(theme.Spacing.Sm));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }

    string ISelectablePopupListOwner.GetPopupListItemLabel(int index) => items[index].Label;

    bool ISelectablePopupListOwner.IsPopupListItemSelected(int index) => index == selectedIndex;

    void ISelectablePopupListOwner.PopupListHighlightIndex(int index) => popupList.HighlightIndex(index);

    void ISelectablePopupListOwner.PopupListActivateIndex(int index)
    {
        popupList.HighlightIndex(index);
        SelectHighlighted();
    }

    void ISelectablePopupListOwner.PopupListPointerDown(int index)
    {
    }

    void ISelectablePopupListOwner.PopupListPointerUp(int index)
    {
    }
}
