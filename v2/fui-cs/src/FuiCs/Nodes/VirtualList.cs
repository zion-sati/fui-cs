using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A vertically scrolling list of fixed-height items that builds only the rows in view. A pool of rows, each a
/// selection area holding an item container, is rebound to the visible items as the list scrolls: <see cref="BindItem"/>
/// fills a row's container for an item index, reusing what it built there before. Spacers above and below the pool
/// keep the content height that of every item. The list is a selection barrier, and a text selection follows its item
/// when the item moves to another pooled row.
/// </summary>
public class VirtualList : FlexBox
{
    private const int DefaultMaxVisibleItems = 20;
    private const int PoolOverscanItems = 2;

    private readonly FlexBox topSpacer = new FlexBox().Width(100.0f, Unit.Percent).Height(0.0f);
    private readonly FlexBox bottomSpacer = new FlexBox().Width(100.0f, Unit.Percent).Height(0.0f);
    private readonly SelectionArea[] pool;
    private readonly int[] poolItemIndexByRow;
    private readonly List<IDisposable> subscriptions = [];
    private Action<FlexBox, int>? bindItem;
    private int totalItems;
    private int currentFirstVisibleIndex = -1;
    private int currentLastVisibleIndex = -1;

    /// <summary>A list of <paramref name="totalItems"/> items, each <paramref name="itemHeight"/> high, with rows for up to <paramref name="maxVisible"/> in view.</summary>
    public VirtualList(int totalItems, float itemHeight, int maxVisible = DefaultMaxVisibleItems)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalItems);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxVisible);
        this.totalItems = totalItems;
        ItemHeight = itemHeight;
        pool = new SelectionArea[maxVisible + PoolOverscanItems];
        poolItemIndexByRow = new int[pool.Length];
        var content = FlexBox.Column(topSpacer).Width(100.0f, Unit.Percent);
        for (var index = 0; index < pool.Length; ++index)
        {
            pool[index] = new SelectionArea().Width(100.0f, Unit.Percent).Height(0.0f).Child(new FlexBox().FillSize());
            poolItemIndexByRow[index] = -1;
            content.Child(pool[index]);
        }
        content.Child(bottomSpacer);
        ScrollState = new ScrollState();
        ScrollBox = new ScrollBox(ScrollState)
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .ScrollOffset(ScrollState.OffsetX.Value, ScrollState.OffsetY.Value)
            .ScrollContentSize(null, TotalContentHeight)
            .FillSize()
            .Child(content);
        ScrollState.ContentHeight.Value = TotalContentHeight;
        FlexDirection = FuiCs.FlexDirection.Column;
        AddChild(ScrollBox);
        this.FillSize();
        subscriptions.Add(ScrollState.OffsetY.Subscribe(_ => RebuildVisibleRange(commit: true)));
        subscriptions.Add(ScrollState.ViewportHeight.Subscribe(_ => RebuildVisibleRange(commit: true)));
        subscriptions.Add(ScrollState.ContentHeight.Subscribe(_ => RebuildVisibleRange(commit: true)));
    }

    /// <summary>The list's offsets and sizes.</summary>
    public ScrollState ScrollState { get; }

    /// <summary>The scroll box the rows scroll in; it carries the list's node id and persisted offset.</summary>
    public ScrollBox ScrollBox { get; }

    /// <summary>How many items the list has.</summary>
    public int TotalItems => totalItems;

    /// <summary>Every item's height in pixels.</summary>
    public float ItemHeight { get; }

    /// <summary>The height of every item together.</summary>
    public float TotalContentHeight => totalItems * ItemHeight;

    /// <summary>The first item in view.</summary>
    public int FirstVisibleIndex => Math.Max(currentFirstVisibleIndex, 0);

    /// <summary>How many items have rows bound to them.</summary>
    public int RenderedItemCount =>
        currentFirstVisibleIndex < 0 || currentLastVisibleIndex < currentFirstVisibleIndex ? 0 : currentLastVisibleIndex - currentFirstVisibleIndex;

    /// <summary>
    /// Fills a row's container for an item index; the container keeps what earlier calls built in it, so a binder
    /// creates its nodes once and updates them. Setting it rebinds the rows in view.
    /// </summary>
    /// <exception cref="InvalidOperationException">The list renders a row before a binder is set.</exception>
    public Action<FlexBox, int>? BindItem
    {
        get => bindItem;
        set
        {
            bindItem = value;
            currentFirstVisibleIndex = -1;
            currentLastVisibleIndex = -1;
            RebuildVisibleRange(commit: true);
        }
    }

    /// <summary>The list's id, which its scroll box carries so the scroll offset persists under it.</summary>
    public override string? NodeId
    {
        get => ScrollBox.NodeId;
        set => ScrollBox.NodeId = value;
    }

    /// <summary>Whether the scroll offset is saved with the page's UI state under the list's id.</summary>
    public bool PersistScroll
    {
        get => ScrollBox.PersistScroll;
        set => ScrollBox.PersistScroll = value;
    }

    /// <summary>A pixel width also seeds the viewport width, before the host reports it.</summary>
    public override Length Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            if (value.Unit == Unit.Pixel)
            {
                ScrollState.ViewportWidth.Value = value.Value;
            }
        }
    }

    /// <summary>A pixel height also seeds the viewport height, before the host reports it.</summary>
    public override Length Height
    {
        get => base.Height;
        set
        {
            base.Height = value;
            if (value.Unit == Unit.Pixel)
            {
                ScrollState.ViewportHeight.Value = value.Value;
            }
        }
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        var handle = base.Build();
        if (currentFirstVisibleIndex < 0 && totalItems > 0)
        {
            RebuildVisibleRange(commit: false);
        }
        Ui.SetSelectionAreaBarrier(handle, true);
        return handle;
    }

    /// <summary>Changes the item count, keeping the offset within the new content.</summary>
    public void UpdateItemCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        totalItems = count;
        ScrollState.ContentHeight.Value = TotalContentHeight;
        ScrollBox.SetScrollContentSize(null, TotalContentHeight);
        currentFirstVisibleIndex = -1;
        currentLastVisibleIndex = -1;
        var maxOffset = Math.Max(TotalContentHeight - Math.Max(ScrollState.ViewportHeight.Value, 0.0f), 0.0f);
        if (ScrollState.OffsetY.Value > maxOffset)
        {
            ScrollBox.SetScrollOffset(ScrollState.OffsetX.Value, maxOffset);
            return;
        }
        RebuildVisibleRange(commit: true);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
        base.Dispose();
    }

    private void RebuildVisibleRange(bool commit)
    {
        if (totalItems <= 0)
        {
            if (currentFirstVisibleIndex == 0 && currentLastVisibleIndex == 0)
            {
                return;
            }
            currentFirstVisibleIndex = 0;
            currentLastVisibleIndex = 0;
            topSpacer.Height = new Length(0.0f);
            bottomSpacer.Height = new Length(0.0f);
            for (var index = 0; index < pool.Length; ++index)
            {
                HidePoolRow(index);
            }
            CommitIfBuilt(commit);
            return;
        }
        var first = Math.Clamp((int)MathF.Floor(ScrollState.OffsetY.Value / ItemHeight), 0, totalItems);
        var viewportHeight = ScrollState.ViewportHeight.Value > 0.0f ? ScrollState.ViewportHeight.Value : ItemHeight;
        var visibleCount = Math.Clamp((int)MathF.Ceiling(viewportHeight / ItemHeight) + 1, 1, pool.Length);
        var last = Math.Min(first + visibleCount, totalItems);
        if (first == currentFirstVisibleIndex && last == currentLastVisibleIndex)
        {
            return;
        }
        currentFirstVisibleIndex = first;
        currentLastVisibleIndex = last;
        topSpacer.Height = new Length(first * ItemHeight);
        bottomSpacer.Height = new Length(Math.Max(TotalContentHeight - (last * ItemHeight), 0.0f));

        var previousItemIndexByRow = (int[])poolItemIndexByRow.Clone();
        var visibleItems = last - first;
        for (var row = 0; row < pool.Length; ++row)
        {
            var previous = previousItemIndexByRow[row];
            if (previous != -1 && (previous < first || previous >= last))
            {
                ClearRowSelection(pool[row]);
            }
        }
        for (var row = 0; row < pool.Length; ++row)
        {
            if (row < visibleItems)
            {
                var item = first + row;
                pool[row].Height = new Length(ItemHeight);
                RenderItem((FlexBox)pool[row].Children[0], item);
                poolItemIndexByRow[row] = item;
            }
            else
            {
                HidePoolRow(row);
            }
        }
        // A selection in an item that moved to another row follows it there.
        for (var row = 0; row < visibleItems; ++row)
        {
            var previousRow = Array.IndexOf(previousItemIndexByRow, first + row);
            if (previousRow != -1 && previousRow != row)
            {
                RetargetRowSelection(pool[previousRow], pool[row]);
            }
        }
        CommitIfBuilt(commit);
    }

    private void RenderItem(FlexBox container, int index)
    {
        if (bindItem is null)
        {
            throw new InvalidOperationException("VirtualList: item renderer not configured. Set BindItem (or call OnBindItem) after construction.");
        }
        bindItem(container, index);
    }

    private void HidePoolRow(int row)
    {
        var rowArea = pool[row];
        if (poolItemIndexByRow[row] != -1)
        {
            ClearRowSelection(rowArea);
            poolItemIndexByRow[row] = -1;
        }
        rowArea.Height = new Length(0.0f);
        if (rowArea.Children.Count > 0)
        {
            ClearItemNode(rowArea.Children[0]);
        }
    }

    private void ClearRowSelection(SelectionArea rowArea)
    {
        if (IsBuilt)
        {
            ClearSelection(rowArea);
        }
    }

    private void RetargetRowSelection(SelectionArea from, SelectionArea to)
    {
        if (!IsBuilt)
        {
            return;
        }
        var fromTexts = new List<Text>();
        var toTexts = new List<Text>();
        CollectTexts(from, fromTexts);
        CollectTexts(to, toTexts);
        for (var index = 0; index < Math.Min(fromTexts.Count, toTexts.Count); ++index)
        {
            var fromText = fromTexts[index];
            var toText = toTexts[index];
            if (fromText.IsBuilt && toText.IsBuilt && fromText.Handle != toText.Handle)
            {
                Ui.RetargetSelection(fromText.Handle, toText.Handle);
            }
        }
    }

    private static void CollectTexts(Node node, List<Text> texts)
    {
        if (node is Text text)
        {
            texts.Add(text);
            return;
        }
        foreach (var child in node.Children)
        {
            CollectTexts(child, texts);
        }
    }

    private static void ClearSelection(Node node)
    {
        if (node is Text text)
        {
            if (text.IsBuilt)
            {
                Ui.ClearSelection(text.Handle);
            }
            return;
        }
        foreach (var child in node.Children)
        {
            ClearSelection(child);
        }
    }

    /// <summary>Empties a hidden row's text and labels, so a recycled row shows nothing stale.</summary>
    private static void ClearItemNode(Node node)
    {
        node.SemanticLabel = string.Empty;
        if (node is Text text)
        {
            text.Text = string.Empty;
        }
        foreach (var child in node.Children)
        {
            ClearItemNode(child);
        }
    }

    private void CommitIfBuilt(bool commit)
    {
        if (commit && IsBuilt)
        {
            FrameScheduler.MarkNeedsCommit();
        }
    }
}

/// <summary>Fluent configuration for <see cref="VirtualList"/>.</summary>
public static class VirtualListExtensions
{
    extension<TList>(TList list) where TList : VirtualList
    {
        /// <summary>Sets <see cref="VirtualList.BindItem"/>, which fills a row's container for an item index.</summary>
        public TList OnBindItem(Action<FlexBox, int> bindItem)
        {
            ArgumentNullException.ThrowIfNull(bindItem);
            list.BindItem = bindItem;
            return list;
        }

        /// <summary>Saves the list's scroll offset with the page's state.</summary>
        public TList PersistScroll(bool persist = true)
        {
            list.PersistScroll = persist;
            return list;
        }
    }
}
