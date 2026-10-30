using FuiCs.Bindings;

namespace FuiCs;

/// <summary>The control a <see cref="SelectablePopupList"/> lists options for: a dropdown or a combo box.</summary>
internal interface ISelectablePopupListOwner
{
    int PopupListItemCount { get; }

    bool IsPopupListEnabled { get; }

    string GetPopupListItemLabel(int index);

    bool IsPopupListItemSelected(int index);

    void PopupListHighlightIndex(int index);

    void PopupListActivateIndex(int index);

    void PopupListPointerDown(int index);

    void PopupListPointerUp(int index);
}

/// <summary>
/// A popup panel of option rows under a portal, anchored to its owner's trigger. Rows are created once per item slot
/// and reused, the panel shows at most <see cref="MaxVisibleItems"/> rows and stays inside the viewport, and the
/// highlighted row scrolls into view.
/// </summary>
internal sealed class SelectablePopupList : IDisposable
{
    public const float PanelPadding = 4.0f;

    private const float PanelEdgePadding = 8.0f;

    private readonly ISelectablePopupListOwner owner;
    private readonly List<OptionNode> optionNodes = [];
    private DropdownOptionRowTemplate? optionRowTemplate;
    private DropdownSizing? sizing;
    private DropdownColors? colors;
    private int? maxVisibleItems;
    private float? popupWidth;

    public SelectablePopupList(ISelectablePopupListOwner owner)
    {
        this.owner = owner;
        Root = new Portal().PositionAbsolute().Position(0.0f, 0.0f).Width(100.0f, Unit.Percent).Height(100.0f, Unit.Percent);
        OptionsHost = new FlexBox().FlexDirection(FlexDirection.Column);
        OptionsHost.SemanticRole = SemanticRole.List;
        OptionsHost.SemanticLabel = "Dropdown options";
        ScrollBox = new ScrollBox()
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Child(OptionsHost);
        Panel = new FlexBox().PositionAbsolute().FlexDirection(FlexDirection.Column).Child(ScrollBox);
        // The list adds list semantics without hiding the trigger or editor from the semantic tree.
        Presenter = new PopupPresenter(Root, Panel, null);
    }

    public Portal Root { get; }

    public FlexBox Panel { get; }

    public PopupPresenter Presenter { get; }

    public ScrollBox ScrollBox { get; }

    public FlexBox OptionsHost { get; }

    public bool IsOpen => Presenter.IsOpen;

    public int HighlightedIndex { get; private set; } = -1;

    /// <summary>The most rows the panel shows before it scrolls; null shows every row.</summary>
    public int? MaxVisibleItems
    {
        get => maxVisibleItems;
        set
        {
            if (value is <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The visible item count must be positive, or null for every item.");
            }
            maxVisibleItems = value;
            RefreshPanelLayout();
        }
    }

    /// <summary>The panel's width; null matches the trigger.</summary>
    public float? PopupWidth
    {
        get => popupWidth;
        set
        {
            if (value is <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The popup width must be positive, or null to match the trigger.");
            }
            popupWidth = value;
            RefreshPanelLayout();
        }
    }

    public DropdownSizing? Sizing
    {
        get => sizing;
        set
        {
            sizing = value;
            foreach (var node in optionNodes)
            {
                node.ApplyTemplate(optionRowTemplate, sizing);
            }
            RefreshPanelLayout();
        }
    }

    public DropdownColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            SyncOptionVisuals();
        }
    }

    public DropdownOptionRowTemplate? OptionRowTemplate
    {
        get => optionRowTemplate;
        set
        {
            optionRowTemplate = value;
            foreach (var node in optionNodes)
            {
                node.ApplyTemplate(optionRowTemplate, sizing);
            }
            RefreshPanelLayout();
            SyncOptionVisuals();
        }
    }

    /// <summary>Opens the panel by a trigger rectangle in page coordinates; false when it has no items or no root.</summary>
    public bool Open(Rect trigger, int initialHighlightIndex)
    {
        if (IsOpen || owner.PopupListItemCount == 0 || !Root.IsBuilt)
        {
            return false;
        }
        EnsureOptionNodes();
        RebuildPanel();
        SetHighlightedIndex(initialHighlightIndex);
        PositionPanel(trigger);
        return true;
    }

    /// <summary>Rebuilds an open panel for the owner's current items, keeping the highlight inside them.</summary>
    public void RefreshOpen(Rect trigger, int highlightedIndex)
    {
        if (!IsOpen)
        {
            return;
        }
        EnsureOptionNodes();
        RebuildPanel();
        var count = owner.PopupListItemCount;
        var next = highlightedIndex;
        if (next >= count)
        {
            next = count > 0 ? count - 1 : -1;
        }
        if (next < 0 && count > 0)
        {
            next = 0;
        }
        SetHighlightedIndex(next);
        PositionPanel(trigger);
    }

    public void Close() => Presenter.Hide();

    public void Clear()
    {
        Close();
        HighlightedIndex = -1;
    }

    public void Dispose() => Presenter.Dispose();

    public void SetHighlightedIndex(int index)
    {
        HighlightedIndex = index;
        SyncOptionVisuals();
        EnsureHighlightedVisible();
    }

    /// <summary>Highlights an item; an index outside the items, or the highlighted one, changes nothing.</summary>
    public void HighlightIndex(int index)
    {
        if (index < 0 || index >= owner.PopupListItemCount || HighlightedIndex == index)
        {
            return;
        }
        SetHighlightedIndex(index);
    }

    /// <summary>Moves the highlight by <paramref name="delta"/>, wrapping at either end.</summary>
    public void MoveHighlight(int delta)
    {
        var count = owner.PopupListItemCount;
        if (count == 0)
        {
            return;
        }
        var next = Math.Max(HighlightedIndex, 0) + delta;
        if (next < 0)
        {
            next = count - 1;
        }
        else if (next >= count)
        {
            next = 0;
        }
        HighlightIndex(next);
    }

    public void RefreshPanelLayout()
    {
        OptionsHost.Width = Length.Percent(100.0f);
        OptionsHost.Height = owner.PopupListItemCount * OptionRowHeight;
        ScrollBox.Width = Length.Percent(100.0f);
        ScrollBox.Height = VisibleListHeight;
        if (IsOpen)
        {
            EnsureHighlightedVisible();
        }
    }

    public void SyncOptionVisuals()
    {
        EnsureOptionNodes();
        var theme = Theme.Active.Value;
        var enabled = owner.IsPopupListEnabled;
        for (var index = 0; index < owner.PopupListItemCount; ++index)
        {
            optionNodes[index].ApplyTheme(theme, index == HighlightedIndex, owner.IsPopupListItemSelected(index), enabled, colors);
        }
    }

    private float OptionRowHeight
    {
        get
        {
            if (optionNodes.Count > 0)
            {
                return optionNodes[0].RowHeight;
            }
            return optionRowTemplate is null && sizing?.OptionHeight is { } height ? height : DefaultDropdownOptionRowPresenter.ResolveMetrics(null).Height;
        }
    }

    /// <summary>The panel's height for its visible rows and padding, kept inside the viewport.</summary>
    private float PanelOuterHeight
    {
        get
        {
            var count = owner.PopupListItemCount;
            var visible = maxVisibleItems is { } limit && count > limit ? limit : count;
            var height = (visible * OptionRowHeight) + (PanelPadding * 2.0f);
            return Math.Min(height, Math.Max(PanelEdgePadding, Ui.GetViewportHeight() - (PanelEdgePadding * 2.0f)));
        }
    }

    private float VisibleListHeight => Math.Max(0.0f, PanelOuterHeight - (PanelPadding * 2.0f));

    private void PositionPanel(Rect trigger)
    {
        var width = popupWidth ?? trigger.Width;
        var height = PanelOuterHeight;
        Panel.Width = width;
        Panel.Height = height;
        ScrollBox.Width = Length.Percent(100.0f);
        ScrollBox.Height = VisibleListHeight;
        Presenter.ShowAnchored(trigger.X, trigger.Y, trigger.Width, trigger.Height, width, height);
        // The highlight was set before the panel showed; bring it into view now (the reference leaves it scrolled away).
        EnsureHighlightedVisible();
    }

    private void RebuildPanel()
    {
        foreach (var node in optionNodes)
        {
            OptionsHost.RemoveChild(node);
        }
        for (var index = 0; index < owner.PopupListItemCount; ++index)
        {
            var node = optionNodes[index];
            node.SetLabel(owner.GetPopupListItemLabel(index));
            OptionsHost.AddChild(node);
        }
        RefreshPanelLayout();
    }

    private void EnsureOptionNodes()
    {
        while (optionNodes.Count < owner.PopupListItemCount)
        {
            optionNodes.Add(new OptionNode(owner, optionNodes.Count, optionRowTemplate, sizing));
        }
    }

    private void EnsureHighlightedVisible()
    {
        if (!IsOpen || HighlightedIndex < 0)
        {
            return;
        }
        var visibleHeight = VisibleListHeight;
        if (visibleHeight <= 0.0f)
        {
            return;
        }
        var rowHeight = OptionRowHeight;
        var top = HighlightedIndex * rowHeight;
        var bottom = top + rowHeight;
        var offset = ScrollBox.ScrollState.OffsetY.Value;
        if (top < offset)
        {
            offset = top;
        }
        else if (bottom > offset + visibleHeight)
        {
            offset = bottom - visibleHeight;
        }
        ScrollBox.SetScrollOffset(0.0f, offset);
    }

    /// <summary>
    /// One option row: hovering highlights its item, and a release activates it. The template's root and label route
    /// their pointer input here too, so a template may make them interactive.
    /// </summary>
    private sealed class OptionNode : FlexBox
    {
        private readonly ISelectablePopupListOwner owner;
        private readonly int slotIndex;
        private DropdownOptionRowPresenter presenter;
        private string label = string.Empty;

        public OptionNode(ISelectablePopupListOwner owner, int slotIndex, DropdownOptionRowTemplate? template, DropdownSizing? sizing)
        {
            this.owner = owner;
            this.slotIndex = slotIndex;
            presenter = (template ?? DropdownOptionRowTemplate.Default).CreatePresenter(sizing);
            SemanticRole = FuiCs.SemanticRole.ListItem;
            Width = Length.Percent(100.0f);
            Cursor = CursorStyle.Pointer;
            SetFocusable(false, 0);
            RequireInteractive();
            AddChild(presenter.Root);
            BindPresenter();
        }

        public float RowHeight => presenter.Metrics.Height;

        public void SetLabel(string text)
        {
            label = text;
            SemanticLabel = text;
            presenter.Label.Text = text;
        }

        public void ApplyTemplate(DropdownOptionRowTemplate? template, DropdownSizing? sizing)
        {
            var previous = presenter;
            presenter = (template ?? DropdownOptionRowTemplate.Default).CreatePresenter(sizing);
            RemoveChild(previous.Root);
            AddChild(presenter.Root);
            previous.Root.Dispose();
            presenter.Label.Text = label;
            BindPresenter();
        }

        public void ApplyTheme(Theme theme, bool highlighted, bool selected, bool enabled, DropdownColors? colors)
        {
            SemanticSelected = selected;
            SemanticDisabled = !enabled;
            presenter.Apply(theme, new DropdownOptionRowVisualState(highlighted, selected, enabled), colors);
        }

        protected internal override void HandlePointerEvent(PointerEventArgs e)
        {
            base.HandlePointerEvent(e);
            HandleRowPointer(e);
        }

        private void HandleRowPointer(PointerEventArgs e)
        {
            switch (e.EventType)
            {
                case PointerEventType.Enter:
                    owner.PopupListHighlightIndex(slotIndex);
                    break;
                case PointerEventType.Down:
                    CapturePointer();
                    owner.PopupListPointerDown(slotIndex);
                    break;
                case PointerEventType.Up:
                    ReleasePointer();
                    owner.PopupListPointerUp(slotIndex);
                    owner.PopupListActivateIndex(slotIndex);
                    break;
                case PointerEventType.Cancel:
                    ReleasePointer();
                    owner.PopupListPointerUp(slotIndex);
                    break;
            }
        }

        private void HandleNestedPointer(PointerEventArgs e)
        {
            HandleRowPointer(e);
            e.Handled = true;
        }

        private void BindPresenter()
        {
            Height = presenter.Metrics.Height;
            presenter.Root.FillSize();
            foreach (var node in (ReadOnlySpan<Node>)[presenter.Root, presenter.Label])
            {
                node.Cursor = CursorStyle.Pointer;
                node.PointerEnter += HandleNestedPointer;
                node.PointerDown += HandleNestedPointer;
                node.PointerUp += HandleNestedPointer;
                node.PointerCancel += HandleNestedPointer;
            }
        }
    }
}
