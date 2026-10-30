using FuiCs.Bindings;

namespace FuiCs;

/// <summary>Which items a combo box lists for its text.</summary>
public enum ComboBoxFilterMode
{
    /// <summary>Every item.</summary>
    None = 0,

    /// <summary>Items that start with the text, ignoring case.</summary>
    StartsWith = 1,

    /// <summary>Items that contain the text, ignoring case.</summary>
    Contains = 2,
}

/// <summary>What a combo box does with its text when the list closes or the field loses focus.</summary>
public enum ComboBoxCommitMode
{
    /// <summary>Keeps the text as typed.</summary>
    KeepText = 0,

    /// <summary>Puts back the last selected item's text.</summary>
    RevertToSelection = 1,

    /// <summary>Selects the item that matches the text, ignoring case.</summary>
    SelectExactMatch = 2,
}

/// <summary>An item of a <see cref="ComboBox"/>.</summary>
public sealed record ComboBoxItem(string Value);

/// <summary>
/// A text field with a list of <see cref="Items"/> that filters as the user types. The list opens from the chevron,
/// a press on the field's edge, or the arrows; while it is open the arrows, Home and End move the highlight, Enter or
/// a release on a row selects, and Escape, a press outside, blurring or scrolling the field out of view closes it.
/// </summary>
public class ComboBox : FlexBox, ISelectablePopupListOwner
{
    private const float DefaultPanelBackgroundBlur = 10.0f;
    private const float DefaultFieldHeight = 32.0f;
    private const float DefaultChevronBoxSize = 32.0f;
    private const float FieldBorderWidth = 2.0f;

    private static ComboBox? active;
    private static bool scrollHookRegistered;

    private readonly TextInput editor;
    private readonly FlexBox chevronHost;
    private readonly SelectablePopupList popupList;
    private readonly List<ComboBoxItem> items = [];
    private readonly List<int> filteredIndices = [];
    private DropdownChevronPresenter chevronPresenter;
    private DropdownChevronTemplate? chevronTemplate;
    private DropdownSizing? sizing;
    private DropdownColors? colors;
    private Color? popupPanelColor;
    private float? popupPanelBackgroundBlur;
    private IDisposable? keyFilter;
    private string text;
    private string lastAutoCompleteText = string.Empty;
    private int selectedIndex = -1;
    private int committedSelectedIndex = -1;
    private ComboBoxFilterMode filterMode = ComboBoxFilterMode.Contains;
    private bool allowCustomValue = true;
    private bool popupPointerPressed;
    private bool pointerPressed;
    private bool hovered;
    private bool focused;
    private bool wrapperFocused;
    private bool editorFocused;
    private bool deferredBlurClosePending;
    private bool suppressEditorChanged;
    private bool disposed;

    /// <summary>A combo box with <paramref name="text"/> in its field.</summary>
    public ComboBox(string text = "")
    {
        ArgumentNullException.ThrowIfNull(text);
        EnsureScrollHook();
        this.text = text;
        editor = new TextInput(text) { Template = EditorTemplate };
        editor.FillWidth();
        chevronPresenter = DropdownChevronTemplate.Default.CreatePresenter(null);
        chevronHost = new FlexBox().Width(DefaultChevronBoxSize).Height(100.0f, Unit.Percent).AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center);
        popupList = new SelectablePopupList(this);
        RebuildFilteredIndices();
        popupList.Presenter.Overlay.PointerClick += _ => Close();
        editor.TextChanged += e => HandleEditorTextChanged(e.Text);
        editor.FocusChanged += e => HandleEditorFocusChanged(e.Focused);
        // The reference listened for the editor's key presses, which counted every key as handled and kept Tab from
        // leaving the combo box; the override reports only the keys it handles.
        editor.KeyDownOverride = HandleEditorKeyDown;
        editor.Editor.EditorCommandKeys = true;
        chevronHost.AddChild(chevronPresenter.Root);
        chevronHost.PointerClick += _ => ToggleFromChevron();
        chevronHost.PointerEnter += _ =>
        {
            hovered = true;
            Refresh();
        };
        chevronHost.PointerLeave += _ =>
        {
            pointerPressed = false;
            hovered = false;
            Refresh();
        };
        chevronHost.PointerDown += _ =>
        {
            pointerPressed = true;
            FocusEditorFromChevron();
            Refresh();
        };
        chevronHost.PointerUp += _ =>
        {
            pointerPressed = false;
            Refresh();
        };
        SemanticRole = FuiCs.SemanticRole.ComboBox;
        SetFocusable(true, 0);
        RequireInteractive();
        ReflectSemanticDisabledFromEnabled();
        Cursor = CursorStyle.Text;
        FlexDirection = FuiCs.FlexDirection.Row;
        AlignItems = FuiCs.AlignItems.Center;
        AddChild(editor);
        AddChild(chevronHost);
        AddChild(popupList.Root);
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => SyncFocusChrome(Theme.Active.Value)));
        SemanticExpanded = false;
        DefaultSemanticLabel = "Combo box";
        Refresh();
    }

    /// <summary>Raised when the user selects another item; not when code sets <see cref="SelectedIndex"/>.</summary>
    public event Action<ComboBoxChangedEventArgs<ComboBoxItem>>? Changed;

    /// <summary>Raised when the user edits the text, or committing the text replaces it; not when code sets <see cref="Text"/>.</summary>
    public event Action<TextChangedEventArgs>? TextChanged;

    /// <summary>The open combo box, if any.</summary>
    public static ComboBox? Active => active;

    /// <summary>The items the list filters. Assigning them closes the list and selects the item matching the text.</summary>
    public IReadOnlyList<ComboBoxItem> Items
    {
        get => items;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Close();
            items.Clear();
            items.AddRange(value);
            SyncSelectionFromText();
            RebuildFilteredIndices();
            popupList.RefreshPanelLayout();
            popupList.SyncOptionVisuals();
            SyncSemanticLabel();
        }
    }

    /// <summary>The field's text. Setting it does not raise <see cref="TextChanged"/>.</summary>
    public string Text
    {
        get => text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetText(value, emit: false);
        }
    }

    /// <summary>Shown in the field while its text is empty.</summary>
    public string Placeholder
    {
        get => editor.Placeholder;
        set
        {
            editor.Placeholder = value;
            SyncSemanticLabel();
        }
    }

    /// <summary>The selected item's index, or -1 for none. Setting it puts the item's text in the field and raises nothing.</summary>
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
    public ComboBoxItem? SelectedItem => selectedIndex >= 0 ? items[selectedIndex] : null;

    /// <summary>How many items the list shows for the current text.</summary>
    public int FilteredCount => filteredIndices.Count;

    /// <summary>The highlighted row of the open list, or -1.</summary>
    public int HighlightedIndex => popupList.HighlightedIndex;

    /// <summary>Whether the list is open.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Text that matches no item clears the selection; when false, the last match stays selected.</summary>
    public bool AllowCustomValue
    {
        get => allowCustomValue;
        set
        {
            allowCustomValue = value;
            SyncSelectionFromText();
        }
    }

    /// <summary>Completes the typed text with the first item that starts with it, selecting the completed part.</summary>
    public bool AutoComplete { get; set; }

    /// <summary>Which items the list shows for the text.</summary>
    public ComboBoxFilterMode FilterMode
    {
        get => filterMode;
        set
        {
            filterMode = value;
            RebuildFilteredIndices();
            RefreshPopupAfterFilter();
            popupList.SyncOptionVisuals();
        }
    }

    /// <summary>What happens to the text when the list closes or the field blurs.</summary>
    public ComboBoxCommitMode CommitMode { get; set; } = ComboBoxCommitMode.KeepText;

    /// <summary>Opens the list when the field gains focus.</summary>
    public bool OpenOnFocus { get; set; }

    /// <summary>Opens the list, or keeps it open, as the user types. On by default.</summary>
    public bool StaysOpenOnEdit { get; set; } = true;

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

    /// <summary>The field's height and font size, the chevron's box, and the list's rows.</summary>
    public DropdownSizing? Sizing
    {
        get => sizing;
        set
        {
            sizing = value;
            ReplaceChevronPresenter();
            popupList.Sizing = value;
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

    /// <summary>Builds the chevron; null uses <see cref="DropdownChevronTemplate.Default"/>.</summary>
    public DropdownChevronTemplate? ChevronTemplate
    {
        get => chevronTemplate;
        set
        {
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

    internal TextInput Editor => editor;

    internal SelectablePopupList PopupList => popupList;

    int ISelectablePopupListOwner.PopupListItemCount => filteredIndices.Count;

    bool ISelectablePopupListOwner.IsPopupListEnabled => IsEnabled;

    private static TextInputTemplate EditorTemplate { get; } = new(static () => new EditorPresenter());

    /// <summary>Closes the open combo box, if any.</summary>
    public static void HideActive() => active?.Close();

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
                editor.Focus();
                Refresh();
                break;
            case PointerEventType.Up when pointerPressed:
                pointerPressed = false;
                ToggleOpen();
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
            popupList.MoveHighlight(-1);
            return true;
        }
        return handled;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        wrapperFocused = focused;
        if (!focused && !IsOpen)
        {
            pointerPressed = false;
        }
        SyncFocusedState();
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        editor.IsEnabled = IsEnabled;
        if (!IsEnabled)
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

    /// <summary>Closes the open combo box once its field leaves the scroll view around it, or the viewport.</summary>
    private static void DismissActiveIfTriggerScrolledAway()
    {
        if (active is { } comboBox && !comboBox.IsTriggerVisible())
        {
            comboBox.Close();
        }
    }

    private static bool Matches(string value, string query, ComboBoxFilterMode mode) => mode switch
    {
        ComboBoxFilterMode.StartsWith => value.StartsWith(query, StringComparison.OrdinalIgnoreCase),
        ComboBoxFilterMode.Contains => value.Contains(query, StringComparison.OrdinalIgnoreCase),
        _ => true,
    };

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

    private void ToggleFromChevron()
    {
        if (!IsEnabled)
        {
            return;
        }
        FocusEditorFromChevron();
        ToggleOpen();
        Refresh();
    }

    private void FocusEditorFromChevron()
    {
        editor.CaretToEnd();
        editor.Focus();
        editor.CaretToEnd();
    }

    private void ToggleOpen()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>The editor's keys: the arrows open the list or move its highlight, and Enter and Escape act on the open list.</summary>
    private bool HandleEditorKeyDown(string key, KeyModifier modifiers)
    {
        if (!IsEnabled || modifiers != KeyModifier.None)
        {
            return IsTextNavigationKey(key, modifiers);
        }
        switch (key)
        {
            case "ArrowDown":
                if (IsOpen)
                {
                    popupList.MoveHighlight(1);
                }
                else
                {
                    Open();
                }
                return true;
            case "ArrowUp":
                if (!IsOpen)
                {
                    Open();
                }
                popupList.MoveHighlight(-1);
                return true;
            case "Enter" when IsOpen:
                SelectHighlighted();
                return true;
            case "Escape" when IsOpen:
                Close();
                return true;
            default:
                return IsTextNavigationKey(key, modifiers);
        }
    }

    private static bool IsTextNavigationKey(string key, KeyModifier modifiers) =>
        (modifiers & (KeyModifier.Ctrl | KeyModifier.Alt | KeyModifier.Meta)) == KeyModifier.None
        && key is "ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown" or "Home" or "End" or "PageUp" or "PageDown";

    private void HandleEditorTextChanged(string value)
    {
        if (suppressEditorChanged)
        {
            return;
        }
        var next = value;
        // After a completion, typing replaces the selected suffix and shortens the text; compare with what the user
        // typed (the reference compared with the completed text, so completion stopped after the first character).
        var deleting = value.Length < (lastAutoCompleteText.Length > 0 ? lastAutoCompleteText : text).Length;
        var complete = AutoComplete && value.Length > 0 && !deleting && value != lastAutoCompleteText;
        lastAutoCompleteText = string.Empty;
        if (complete && FindAutoCompleteMatch(value) is var match && match >= 0)
        {
            var completed = items[match].Value;
            if (completed.Length > value.Length)
            {
                next = completed;
                lastAutoCompleteText = value;
                suppressEditorChanged = true;
                editor.Text = completed;
                editor.SetSelection(TextOffsets.CharacterCount(value), TextOffsets.CharacterCount(completed));
                suppressEditorChanged = false;
            }
        }
        text = next;
        SyncSelectionFromText();
        RebuildFilteredIndices();
        if (filteredIndices.Count == 0)
        {
            Close();
        }
        else if (StaysOpenOnEdit)
        {
            if (IsOpen)
            {
                RefreshOpenPopup(0);
            }
            else
            {
                Open();
            }
        }
        RefreshPopupAfterFilter();
        popupList.SyncOptionVisuals();
        SyncSemanticLabel();
        TextChanged?.Invoke(new TextChangedEventArgs(next));
    }

    private void HandleEditorFocusChanged(bool isFocused)
    {
        editorFocused = isFocused;
        if (isFocused && OpenOnFocus)
        {
            Open();
        }
        if (!isFocused && !IsOpen)
        {
            CommitCurrentText();
            pointerPressed = false;
        }
        SyncFocusedState();
    }

    /// <summary>
    /// Follows focus moving between the field and its editor. Losing both closes the list after the next commit,
    /// unless a press on a row holds it, so focus moving from one to the other does not close it in between.
    /// </summary>
    private void SyncFocusedState()
    {
        var next = wrapperFocused || editorFocused;
        if (!next && !popupPointerPressed && !deferredBlurClosePending)
        {
            deferredBlurClosePending = true;
            FrameScheduler.AfterNextCommit(FireDeferredBlurClose);
            FrameScheduler.MarkNeedsCommit();
        }
        if (focused == next)
        {
            return;
        }
        focused = next;
        Refresh();
    }

    private void FireDeferredBlurClose()
    {
        deferredBlurClosePending = false;
        if (!wrapperFocused && !editorFocused && !popupPointerPressed && !disposed)
        {
            pointerPressed = false;
            Close();
        }
    }

    private void SetText(string value, bool emit)
    {
        if (text == value)
        {
            return;
        }
        text = value;
        suppressEditorChanged = true;
        editor.Text = value;
        suppressEditorChanged = false;
        SyncSelectionFromText();
        RebuildFilteredIndices();
        RefreshPopupAfterFilter();
        popupList.SyncOptionVisuals();
        SyncSemanticLabel();
        if (emit)
        {
            TextChanged?.Invoke(new TextChangedEventArgs(value));
        }
    }

    private void SetSelectedIndex(int index, bool emit)
    {
        if (index == -1)
        {
            selectedIndex = -1;
            committedSelectedIndex = -1;
            popupList.SetHighlightedIndex(-1);
            SyncSemanticLabel();
            return;
        }
        // Typing an exact match already moved the selection silently; the commit is what changed (the reference compares
        // with the silent selection, so a typed match never raised Changed).
        var changed = committedSelectedIndex != index;
        selectedIndex = index;
        committedSelectedIndex = index;
        SetText(items[index].Value, emit: false);
        editor.CaretToEnd();
        RebuildFilteredIndices();
        popupList.SetHighlightedIndex(VisibleIndexOf(index));
        SyncSemanticLabel();
        if (emit && changed)
        {
            RequestSemanticAnnouncement();
            Changed?.Invoke(new ComboBoxChangedEventArgs<ComboBoxItem>(items[index], index));
        }
    }

    private void SyncSelectionFromText()
    {
        var exact = FindExactMatch(text);
        if (exact >= 0)
        {
            selectedIndex = exact;
        }
        else if (allowCustomValue)
        {
            selectedIndex = -1;
        }
    }

    private void CommitCurrentText()
    {
        switch (CommitMode)
        {
            case ComboBoxCommitMode.SelectExactMatch when FindExactMatch(text) is var exact && exact >= 0:
                SetSelectedIndex(exact, emit: true);
                break;
            case ComboBoxCommitMode.RevertToSelection when committedSelectedIndex >= 0 && committedSelectedIndex < items.Count:
                SetText(items[committedSelectedIndex].Value, emit: true);
                editor.CaretToEnd();
                selectedIndex = committedSelectedIndex;
                break;
        }
    }

    private void Open()
    {
        if (IsOpen || filteredIndices.Count == 0 || !IsBuilt)
        {
            return;
        }
        var initialHighlight = selectedIndex >= 0 ? VisibleIndexOf(selectedIndex) : 0;
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
        deferredBlurClosePending = false;
        popupPointerPressed = false;
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
        CommitCurrentText();
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
                popupList.HighlightIndex(filteredIndices.Count - 1);
                return true;
            case "ArrowDown":
                popupList.MoveHighlight(1);
                return true;
            case "ArrowUp":
                popupList.MoveHighlight(-1);
                return true;
            default:
                return false;
        }
    }

    private void RefreshPopupAfterFilter()
    {
        if (IsOpen)
        {
            RefreshOpenPopup(popupList.HighlightedIndex);
        }
        else
        {
            popupList.RefreshPanelLayout();
        }
    }

    private void RefreshOpenPopup(int highlightedIndex)
    {
        if (!IsBuilt)
        {
            popupList.RefreshPanelLayout();
            return;
        }
        popupList.RefreshOpen(GetBounds(), highlightedIndex);
    }

    private void SelectHighlighted()
    {
        var highlighted = popupList.HighlightedIndex;
        if (highlighted < 0 || highlighted >= filteredIndices.Count)
        {
            return;
        }
        SetSelectedIndex(filteredIndices[highlighted], emit: true);
        Close();
    }

    private void RebuildFilteredIndices()
    {
        filteredIndices.Clear();
        for (var index = 0; index < items.Count; ++index)
        {
            if (text.Length == 0 || Matches(items[index].Value, text, filterMode))
            {
                filteredIndices.Add(index);
            }
        }
        if (popupList.HighlightedIndex >= filteredIndices.Count)
        {
            popupList.SetHighlightedIndex(filteredIndices.Count - 1);
        }
    }

    private int FindAutoCompleteMatch(string value) => items.FindIndex(item => item.Value.StartsWith(value, StringComparison.OrdinalIgnoreCase));

    private int FindExactMatch(string value) => items.FindIndex(item => string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase));

    /// <summary>The row an item shows on, or the first row when the filter hides it, or -1 when nothing shows.</summary>
    private int VisibleIndexOf(int itemIndex)
    {
        var row = filteredIndices.IndexOf(itemIndex);
        return row >= 0 ? row : filteredIndices.Count > 0 ? 0 : -1;
    }

    private void ReplaceChevronPresenter()
    {
        var previous = chevronPresenter;
        chevronPresenter = (chevronTemplate ?? DropdownChevronTemplate.Default).CreatePresenter(sizing);
        chevronHost.RemoveChild(previous.Root);
        chevronHost.AddChild(chevronPresenter.Root);
        previous.Root.Dispose();
    }

    private void SyncSemanticLabel() => DefaultSemanticLabel = text.Length > 0 ? text : "Combo box";

    private void Refresh()
    {
        if (disposed)
        {
            return;
        }
        var theme = Theme.Active.Value;
        var menu = theme.ContextMenu;
        var fieldHeight = sizing?.FieldHeight ?? DefaultFieldHeight;
        var contentHeight = Math.Max(0.0f, fieldHeight - (FieldBorderWidth * 2.0f));
        Cursor = IsEnabled ? CursorStyle.Text : CursorStyle.Default;
        Opacity = IsEnabled ? 1.0f : 0.6f;
        Height = fieldHeight;
        CornerRadius = new CornerRadius(theme.Spacing.Sm);
        Border = new Border(FieldBorderWidth, colors?.Border ?? theme.Colors.Border);
        Padding = new Thickness(16.0f, 0.0f, 8.0f, 0.0f);
        Background = colors?.Background ?? theme.Colors.Surface;
        editor.Height = contentHeight;
        editor.FontSize = sizing?.FieldFontSize ?? theme.Fonts.SizeBody;
        editor.LineHeight = contentHeight;
        editor.Colors = colors is null ? null : new TextInputColors
        {
            Background = colors.Background,
            TextPrimary = colors.TextPrimary,
            Placeholder = colors.Placeholder,
            Border = colors.Border,
            Accent = colors.Accent,
            Caret = colors.Accent ?? theme.Colors.Accent,
        };
        chevronHost.Width = sizing?.ChevronBoxSize ?? DefaultChevronBoxSize;
        chevronHost.Height = contentHeight;
        chevronHost.Cursor = IsEnabled ? CursorStyle.Pointer : CursorStyle.Default;
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
        SyncFocusChrome(theme);
    }

    private void SyncFocusChrome(Theme theme)
    {
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(theme.Spacing.Sm));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }

    string ISelectablePopupListOwner.GetPopupListItemLabel(int index) => items[filteredIndices[index]].Value;

    bool ISelectablePopupListOwner.IsPopupListItemSelected(int index) =>
        index >= 0 && index < filteredIndices.Count && filteredIndices[index] == selectedIndex;

    void ISelectablePopupListOwner.PopupListHighlightIndex(int index) => popupList.HighlightIndex(index);

    void ISelectablePopupListOwner.PopupListActivateIndex(int index)
    {
        popupList.HighlightIndex(index);
        SelectHighlighted();
    }

    void ISelectablePopupListOwner.PopupListPointerDown(int index) => popupPointerPressed = true;

    void ISelectablePopupListOwner.PopupListPointerUp(int index)
    {
        popupPointerPressed = false;
        SyncFocusedState();
    }

    /// <summary>The editor's shell inside the combo box: transparent and unpadded, centered on the field.</summary>
    private sealed class EditorPresenter : TextInputPresenter
    {
        public override void Apply(Theme theme, TextInputVisualState state, TextInputColors? colors)
        {
            var editableCursor = state.IsEnabled ? CursorStyle.Text : CursorStyle.Default;
            Host.Background = Color.Transparent;
            Host.CornerRadius = new CornerRadius(0.0f);
            Host.Border = new Border(0.0f, Color.Transparent);
            Host.Padding = new Thickness(0.0f);
            Host.AlignItems = FuiCs.AlignItems.Center;
            Host.Cursor = editableCursor;
            Host.Opacity = state.IsEnabled ? 1.0f : 0.6f;
            EditorHost.Cursor = editableCursor;
            PlaceholderHost.Position = new Point(0.0f, 0.0f);
            PlaceholderHost.Width = Length.Percent(100.0f);
            PlaceholderHost.Cursor = editableCursor;
        }
    }
}
