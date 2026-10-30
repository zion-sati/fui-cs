namespace FuiCs;

/// <summary>A tab view's selection change: the tab that was selected and the one that is, or -1 and null for none.</summary>
public sealed class TabSelectionChangedEventArgs(int previousIndex, int selectedIndex, TabItem? previousItem, TabItem? selectedItem) : EventArgs
{
    /// <summary>The previously selected tab's index, or -1.</summary>
    public int PreviousIndex { get; } = previousIndex;

    /// <summary>The selected tab's index, or -1.</summary>
    public int SelectedIndex { get; } = selectedIndex;

    /// <summary>The previously selected tab, if any.</summary>
    public TabItem? PreviousItem { get; } = previousItem;

    /// <summary>The selected tab, if any.</summary>
    public TabItem? SelectedItem { get; } = selectedItem;
}

/// <summary>
/// One tab of a <see cref="TabView"/>: its label, description and enabled state, and its content, a retained view
/// created the first time the tab is selected and kept until the tab is removed.
/// </summary>
public sealed class TabItem
{
    private string label;
    private string description = string.Empty;
    private bool isEnabled = true;
    private Func<RetainedView>? factory;
    private RetainedView? preparedView;
    private RetainedView? retainedView;
    private TabView? owner;

    /// <summary>
    /// A tab labelled <paramref name="label"/> whose content <paramref name="content"/> creates on first selection.
    /// </summary>
    public TabItem(string label, Func<RetainedView>? content = null)
    {
        ArgumentNullException.ThrowIfNull(label);
        this.label = label;
        factory = content;
    }

    /// <summary>The tab's label.</summary>
    public string Label
    {
        get => label;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            label = value;
            owner?.ItemMetadataChanged(this);
        }
    }

    /// <summary>A longer description of the tab, for the application's own tab strip; the view does not show it.</summary>
    public string Description
    {
        get => description;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            description = value;
            owner?.ItemMetadataChanged(this);
        }
    }

    /// <summary>Whether the tab can be selected; disabling the selected tab selects the nearest enabled one.</summary>
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value)
            {
                return;
            }
            isEnabled = value;
            owner?.ItemEnabledChanged(this);
        }
    }

    /// <summary>Whether the tab's content has been created.</summary>
    public bool IsMaterialized => retainedView is not null;

    /// <summary>Sets the factory that creates the tab's content when it is first selected.</summary>
    /// <exception cref="InvalidOperationException">The content was already created.</exception>
    public TabItem Content(Func<RetainedView> content)
    {
        ArgumentNullException.ThrowIfNull(content);
        ThrowIfMaterialized();
        preparedView = null;
        factory = content;
        return this;
    }

    /// <summary>Sets content created ahead of selection.</summary>
    /// <exception cref="InvalidOperationException">The content was already created.</exception>
    public TabItem ContentView(RetainedView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        ThrowIfMaterialized();
        preparedView = view;
        factory = null;
        return this;
    }

    internal void BindOwner(TabView? tabView) => owner = tabView;

    /// <summary>The tab's content, created now if it does not exist yet: an empty box without a factory.</summary>
    internal RetainedView ResolveView()
    {
        if (retainedView is not null)
        {
            return retainedView;
        }
        var resolved = preparedView ?? factory?.Invoke() ?? new RetainedView(new FlexBox().FillSize());
        preparedView = null;
        retainedView = resolved;
        return resolved;
    }

    internal RetainedView? ExistingView => retainedView;

    internal void DisposeContent()
    {
        retainedView?.Dispose();
        if (preparedView is not null && !ReferenceEquals(preparedView, retainedView))
        {
            preparedView.Dispose();
        }
        retainedView = null;
        preparedView = null;
    }

    private void ThrowIfMaterialized()
    {
        if (IsMaterialized)
        {
            throw new InvalidOperationException("A tab's content cannot be replaced after it has been created.");
        }
    }
}

/// <summary>
/// Shows the content of one of its tabs at a time, in <see cref="ContentPresenter"/>. It has no tab strip: the app
/// builds its own selector and sets <see cref="SelectedIndex"/>. The first enabled tab is selected, a tab's content is
/// created when it is first selected and deactivated rather than disposed while another tab shows, and removing or
/// disabling the selected tab selects the nearest enabled one after it, then before it.
/// </summary>
public class TabView : FlexBox
{
    private const int NoPendingSelection = -2;

    private readonly List<TabItem> items = [];
    private int selectedIndex = -1;
    private bool selecting;
    private int pendingSelection = NoPendingSelection;
    private bool pendingEmit;

    /// <summary>A tab view holding <paramref name="items"/>, selecting the first enabled one.</summary>
    public TabView(params ReadOnlySpan<TabItem> items)
    {
        ContentPresenter = new FlexBox().FillSize();
        ContentPresenter.SemanticRole = SemanticRole.TabPanel;
        ContentPresenter.SemanticLabel = "Tab panel";
        FlexDirection = FuiCs.FlexDirection.Column;
        AddChild(ContentPresenter);
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    /// <summary>Raised when the selected tab changes, including to none.</summary>
    public event Action<TabSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>The box the selected tab's content is shown in, as the tab panel.</summary>
    public FlexBox ContentPresenter { get; }

    /// <summary>The tabs in order.</summary>
    public IReadOnlyList<TabItem> Items => items;

    /// <summary>
    /// The selected tab's index, or -1 when no tab is enabled. Selecting a disabled tab does nothing; a change raised
    /// from <see cref="SelectionChanged"/> takes effect after the current one finishes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not that of a tab.</exception>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, items.Count);
            Select(value, emit: true, previousOverride: null);
        }
    }

    /// <summary>The selected tab, if any.</summary>
    public TabItem? SelectedItem => selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : null;

    /// <summary>Adds a tab; the first enabled tab added to a view without a selection is selected.</summary>
    public TabView AddItem(TabItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (items.Contains(item))
        {
            return this;
        }
        item.BindOwner(this);
        items.Add(item);
        if (selectedIndex < 0 && item.IsEnabled)
        {
            Select(items.Count - 1, emit: false, previousOverride: null);
        }
        return this;
    }

    /// <summary>Removes <paramref name="item"/>, disposing its content.</summary>
    public TabView RemoveItem(TabItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = items.IndexOf(item);
        if (index >= 0)
        {
            RemoveItemAt(index);
        }
        return this;
    }

    /// <summary>Removes the tab at <paramref name="index"/> and disposes its content.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not that of a tab.</exception>
    public TabView RemoveItemAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, items.Count);
        var removed = items[index];
        var wasSelected = index == selectedIndex;
        if (wasSelected)
        {
            DetachActiveView();
        }
        removed.BindOwner(null);
        items.RemoveAt(index);
        if (wasSelected)
        {
            ReplaceSelection(index, removed);
        }
        else if (selectedIndex > index)
        {
            --selectedIndex;
        }
        removed.DisposeContent();
        return this;
    }

    /// <summary>Removes every tab, disposing their content.</summary>
    public TabView ClearItems()
    {
        var previousIndex = selectedIndex;
        var previousItem = SelectedItem;
        DetachActiveView();
        foreach (var item in items)
        {
            item.BindOwner(null);
            item.DisposeContent();
        }
        items.Clear();
        selectedIndex = -1;
        if (previousItem is not null)
        {
            RaiseSelectionChanged(previousIndex, -1, previousItem, null);
        }
        return this;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        DetachActiveView();
        foreach (var item in items)
        {
            item.BindOwner(null);
            item.DisposeContent();
        }
        base.Dispose();
    }

    internal void ItemEnabledChanged(TabItem item)
    {
        var index = items.IndexOf(item);
        if (index < 0)
        {
            return;
        }
        if (!item.IsEnabled && index == selectedIndex)
        {
            DetachActiveView();
            ReplaceSelection(index, item);
        }
        else if (item.IsEnabled && selectedIndex < 0)
        {
            Select(index, emit: true, previousOverride: null);
        }
    }

    internal void ItemMetadataChanged(TabItem item)
    {
        if (items.IndexOf(item) == selectedIndex && selectedIndex >= 0)
        {
            ContentPresenter.SemanticLabel = item.Label + " tab panel";
        }
    }

    /// <summary>After the selected tab at <paramref name="index"/> went away: the nearest enabled tab, or none.</summary>
    private void ReplaceSelection(int index, TabItem previous)
    {
        var previousIndex = selectedIndex;
        selectedIndex = -1;
        var replacement = FindReplacementIndex(index);
        if (replacement >= 0)
        {
            Select(replacement, emit: true, previousOverride: (previousIndex, previous));
        }
        else
        {
            RaiseSelectionChanged(previousIndex, -1, previous, null);
        }
    }

    private void Select(int index, bool emit, (int Index, TabItem Item)? previousOverride)
    {
        if (!IsEnabledIndex(index))
        {
            return;
        }
        if (selecting)
        {
            pendingSelection = index;
            pendingEmit = emit;
            return;
        }
        selecting = true;
        var requested = index;
        var requestedEmit = emit;
        var iterations = 0;
        while (requested >= 0 && IsEnabledIndex(requested))
        {
            var previousIndex = previousOverride?.Index ?? selectedIndex;
            var previousItem = previousOverride is { } previous ? previous.Item : SelectedItem;
            previousOverride = null;
            if (selectedIndex != requested)
            {
                DetachActiveView();
                selectedIndex = requested;
                var item = items[requested];
                var view = item.ResolveView();
                ContentPresenter.AddChild(view.Root);
                ContentPresenter.SemanticLabel = item.Label + " tab panel";
                view.Activate();
                if (requestedEmit)
                {
                    RaiseSelectionChanged(previousIndex, requested, previousItem, item);
                }
            }
            if (pendingSelection == NoPendingSelection)
            {
                break;
            }
            requested = pendingSelection;
            requestedEmit = pendingEmit;
            pendingSelection = NoPendingSelection;
            if (++iterations > items.Count + 8)
            {
                Logger.Warn("Lifecycle", "TabView stopped a re-entrant selection loop.");
                break;
            }
        }
        pendingSelection = NoPendingSelection;
        selecting = false;
    }

    private void DetachActiveView()
    {
        if (SelectedItem?.ExistingView is not { } view)
        {
            return;
        }
        view.Deactivate();
        ContentPresenter.RemoveChild(view.Root);
    }

    private void RaiseSelectionChanged(int previousIndex, int index, TabItem? previousItem, TabItem? item) =>
        SelectionChanged?.Invoke(new TabSelectionChangedEventArgs(previousIndex, index, previousItem, item));

    private bool IsEnabledIndex(int index) => index >= 0 && index < items.Count && items[index].IsEnabled;

    private int FindReplacementIndex(int index)
    {
        for (var cursor = index; cursor < items.Count; ++cursor)
        {
            if (IsEnabledIndex(cursor))
            {
                return cursor;
            }
        }
        for (var cursor = index - 1; cursor >= 0; --cursor)
        {
            if (IsEnabledIndex(cursor))
            {
                return cursor;
            }
        }
        return -1;
    }
}

/// <summary>Fluent configuration for <see cref="TabView"/> and <see cref="TabItem"/>.</summary>
public static class TabViewExtensions
{
    extension<TTabs>(TTabs tabs) where TTabs : TabView
    {
        /// <summary>Replaces the tabs with <paramref name="items"/>.</summary>
        public TTabs Items(params ReadOnlySpan<TabItem> items)
        {
            tabs.ClearItems();
            foreach (var item in items)
            {
                tabs.AddItem(item);
            }
            return tabs;
        }

        /// <summary>Sets <see cref="TabView.SelectedIndex"/>.</summary>
        public TTabs SelectedIndex(int index)
        {
            tabs.SelectedIndex = index;
            return tabs;
        }

        /// <summary>Adds a handler to <see cref="TabView.SelectionChanged"/>.</summary>
        public TTabs OnSelectionChanged(Action<TabSelectionChangedEventArgs> handler)
        {
            tabs.SelectionChanged += handler;
            return tabs;
        }
    }

    extension(TabItem item)
    {
        /// <summary>Sets <see cref="TabItem.Description"/>.</summary>
        public TabItem Description(string description)
        {
            item.Description = description;
            return item;
        }

        /// <summary>Sets <see cref="TabItem.IsEnabled"/>.</summary>
        public TabItem IsEnabled(bool enabled)
        {
            item.IsEnabled = enabled;
            return item;
        }
    }
}
