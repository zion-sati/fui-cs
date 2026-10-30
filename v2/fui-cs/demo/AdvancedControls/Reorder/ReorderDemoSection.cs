using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls.Reorder;

/// <summary>
/// Drag-and-drop reorder of retained rows inside an inner <see cref="ScrollBox"/>: insertion markers, a drag preview
/// that follows the pointer, cursor feedback, and auto-scroll while the pointer rests near the viewport's edge. The
/// drag manager re-evaluates drop targets only on pointer events, so auto-scroll runs on a timer and moves the
/// insertion preview itself while the list scrolls under a still pointer.
/// </summary>
internal sealed class ReorderDemoSection : IDisposable
{
    private const float PreviewWidth = 272.0f;
    private const float PreviewHeight = 116.0f;
    private const float PreviewOffsetX = 2.0f;
    private const float PreviewOffsetY = 2.0f;
    private const float PreviewMargin = 12.0f;
    private static readonly TimeSpan AutoScrollDelay = TimeSpan.FromMilliseconds(16);

    private readonly List<ReorderRowView> rows = [];
    private readonly List<ReorderDemoItem> items = ReorderDemoLogic.CreateItems();
    private readonly ScrollBox scrollBox;
    private readonly Text orderStatusText = new DemoText("", DemoTextRecipe.StatusValue).FontSize(15.0f).NodeId("reorder-order");
    private readonly Text dragStatusText = new DemoText("", DemoTextRecipe.StatusSupporting).FontSize(15.0f).NodeId("reorder-drag-status");
    private readonly Text viewportStatusText = new DemoText("", DemoTextRecipe.StatusSupporting).FontSize(15.0f).NodeId("reorder-viewport-status");
    private readonly Text hintText = new DemoText(
            "Drag a grip with a mouse, or touch and hold before moving. Release to drop. Hold near the viewport edge (or just beyond it) to auto-scroll the inner ScrollBox while the outer route page stays scrollable.",
            DemoTextRecipe.Hint)
        .FontSize(15.0f)
        .MaxLines(4);
    // The preview's text is not selectable, so the pointer passes through it to the rows: the preview trails the pointer
    // by a frame, and a fast move would otherwise land on its text and lose the drop target until the next move.
    private readonly Text previewCaptionText = new DemoText("Dragging", DemoTextRecipe.Hint, selectable: false).FontSize(13.0f);
    private readonly Text previewTitleText = new DemoText("", DemoTextRecipe.Body, selectable: false).FontSize(16.0f);
    private readonly Text previewDetailText = new DemoText("", DemoTextRecipe.Hint, selectable: false).FontSize(14.0f).MaxLines(2);
    private readonly Text previewEffectText = new DemoText("", DemoTextRecipe.StatusValue, selectable: false).FontSize(13.0f);
    private readonly FlexBox previewGhost;
    private readonly FlexBox endMarker = new FlexBox()
        .FillWidth()
        .Height(ReorderDemoLogic.MarkerHeight)
        .CornerRadius(ReorderDemoLogic.MarkerHeight * 0.5f)
        .Opacity(0.0f);
    private readonly Text endDropText = new DemoText("Drop at end of reorder list", DemoTextRecipe.Hint).FontSize(14.0f);
    private readonly FlexBox endDropZone;
    private readonly FlexBox sectionRoot;
    private Theme theme = Theme.Active.Value;
    private string? activeDragItemId;
    private int rawInsertionIndex = -1;
    private float autoScrollDeltaY;
    private string dragStatusMessage = "Reorder drag status: idle";
    private float previewPointerX = float.NaN;
    private float previewPointerY = float.NaN;
    private DragDropEffects previewEffect = DragDropEffects.None;
    private int previewInsertionSlot = -1;
    private ITimer? autoScrollTimer;
    private readonly List<IDisposable> subscriptions = [];

    public ReorderDemoSection()
    {
        var scrollContent = FlexBox.Column().FillWidth();
        scrollBox = new DemoScrollBox()
            .NodeId("reorder-viewport")
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .PersistScroll(false)
            .FillWidth()
            .Height(ReorderDemoLogic.ViewportHeight)
            .SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder demo viewport");
        scrollBox.Child(scrollContent);

        for (var index = 0; index < items.Count; ++index)
        {
            var row = new ReorderRowView(this, index);
            row.BindItem(items[index]);
            rows.Add(row);
            scrollContent.Child(row.Slot);
        }

        endDropZone = new FlexBox()
            .FillWidth()
            .Height(ReorderDemoLogic.EndSlotHeight)
            .AllowDrop()
            .OnDragEnter(e => e.Proposal = PreviewInsertion(e, items.Count))
            .OnDragOver(e => e.Proposal = PreviewInsertion(e, items.Count))
            .OnDragLeave(HandleTargetLeave)
            .OnDrop(DropAtPreview)
            .Child(endMarker)
            .Child(new FlexBox()
                .FillWidth()
                .Height(36.0f)
                .CornerRadius(14.0f)
                .JustifyContent(JustifyContent.Center)
                .AlignItems(AlignItems.Center)
                .Child(endDropText));
        scrollContent.Child(endDropZone);
        scrollBox.ScrollContentSize(null, ReorderDemoLogic.ContentHeight(items.Count));

        previewGhost = new FlexBox()
            .PositionAbsolute()
            .Width(PreviewWidth)
            .Padding(14.0f)
            .CornerRadius(18.0f)
            .Child(FlexBox.Column(
                    previewCaptionText,
                    Spacer(6.0f),
                    previewTitleText,
                    Spacer(4.0f),
                    previewDetailText,
                    Spacer(10.0f),
                    previewEffectText)
                .FillWidth())
            .Opacity(0.0f)
            .Visibility(Visibility.Hidden)
            .SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder drag preview");
        var previewPortal = new Portal().PositionAbsolute().Position(0.0f, 0.0f).FillSize().Child(previewGhost);

        var body = FlexBox.Column(
                scrollBox,
                Spacer(14.0f),
                orderStatusText,
                Spacer(6.0f),
                dragStatusText,
                Spacer(6.0f),
                viewportStatusText,
                Spacer(10.0f),
                hintText)
            .FillWidth();
        sectionRoot = new FlexBox().NodeId("reorder-section").FillWidth().Child(body).Child(previewPortal);

        subscriptions.Add(scrollBox.ScrollState.OffsetY.Subscribe(_ => SyncViewportStatus()));
        subscriptions.Add(scrollBox.ScrollState.ViewportHeight.Subscribe(_ => SyncViewportStatus()));
        SyncAll();
        subscriptions.Add(Theme.Active.Subscribe(next =>
        {
            theme = next;
            ApplyTheme();
        }));
    }

    /// <summary>Stops the edge auto-scroll and releases the section's subscriptions, when its route is disposed.</summary>
    public void Dispose()
    {
        StopAutoScroll();
        autoScrollTimer?.Dispose();
        autoScrollTimer = null;
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
    }

    /// <summary>The section card: its title, description and the reorder list with its status lines.</summary>
    public RoutePageSection BuildSection() => RoutePageSection.Create(
        "Drag-and-drop reorder / Custom control",
        "Use the drag/drop session API to reorder retained rows inside an inner ScrollBox, including insertion markers, drag-preview polish, cursor feedback, and edge autoscroll.",
        sectionRoot);

    internal DragDataObject BeginDrag(ReorderDemoItem item)
    {
        SelectionArea.ClearCurrentSelection();
        activeDragItemId = item.Id;
        var sourceIndex = ReorderDemoLogic.FindItemIndex(items, item.Id);
        rawInsertionIndex = sourceIndex >= 0 ? sourceIndex : -1;
        ClearPreview();
        dragStatusMessage = "Reorder drag status: dragging " + item.Label;
        SyncAll();
        return new DragDataObject().SetFormat(ReorderDemoLogic.DragFormat, item.Id).SetText(item.Label);
    }

    internal void CompleteDrag(string itemId, DragDropEffects effect)
    {
        var itemLabel = FindItem(itemId)?.Label ?? "item";
        activeDragItemId = null;
        rawInsertionIndex = -1;
        ClearPreview();
        StopAutoScroll();
        dragStatusMessage = effect == DragDropEffects.Move
            ? "Reorder drag status: moved " + itemLabel + " to slot " + Slot(ReorderDemoLogic.FindItemIndex(items, itemId))
            : "Reorder drag status: cancelled " + itemLabel;
        SyncAll();
    }

    /// <summary>Previews the drop at <paramref name="insertionIndex"/> and accepts it as a move.</summary>
    internal DropProposal PreviewInsertion(DragEventArgs args, int insertionIndex)
    {
        var itemId = DraggedItemId(args);
        var sourceIndex = itemId is null ? -1 : ReorderDemoLogic.FindItemIndex(items, itemId);
        if (itemId is null || sourceIndex < 0)
        {
            StopAutoScroll();
            return DropProposal.None;
        }
        activeDragItemId = itemId;
        rawInsertionIndex = insertionIndex;
        SetAutoScrollDelta(PointerAutoScrollDelta(args.Y));
        var normalizedIndex = ReorderDemoLogic.NormalizeInsertionIndex(sourceIndex, insertionIndex, items.Count);
        previewPointerX = args.X;
        previewPointerY = args.Y;
        previewEffect = DragDropEffects.Move;
        previewInsertionSlot = normalizedIndex;
        dragStatusMessage = "Reorder drag status: preview slot " + Slot(normalizedIndex);
        SyncAll();
        return new DropProposal(DragDropEffects.Move, ShowInsertionMarker: true);
    }

    /// <summary>
    /// The pointer left a slot: near the viewport's edge the preview moves to the edge the list scrolls toward;
    /// elsewhere the drag has no drop position until it reaches another slot.
    /// </summary>
    internal void HandleTargetLeave(DragEventArgs args)
    {
        if (activeDragItemId is null)
        {
            return;
        }
        previewPointerX = args.X;
        previewPointerY = args.Y;
        var visibleRange = VisibleRange();
        SetAutoScrollDelta(PointerAutoScrollDelta(args.Y));
        if (autoScrollDeltaY == 0.0f)
        {
            rawInsertionIndex = -1;
            previewEffect = DragDropEffects.None;
            previewInsertionSlot = -1;
            dragStatusMessage = "Reorder drag status: dragging " + ActiveDragLabel();
        }
        else
        {
            var direction = autoScrollDeltaY < 0.0f ? -1 : 1;
            rawInsertionIndex = ReorderDemoLogic.EdgeInsertionIndex(direction, items.Count, visibleRange);
            var sourceIndex = ReorderDemoLogic.FindItemIndex(items, activeDragItemId);
            previewEffect = DragDropEffects.Move;
            previewInsertionSlot = rawInsertionIndex < 0 || sourceIndex < 0
                ? -1
                : ReorderDemoLogic.NormalizeInsertionIndex(sourceIndex, rawInsertionIndex, items.Count);
            dragStatusMessage = "Reorder drag status: auto-scrolling " + (direction < 0 ? "up" : "down");
        }
        SyncAll();
    }

    internal void DropAtPreview(DragEventArgs args)
    {
        var itemId = DraggedItemId(args);
        if (itemId is null || rawInsertionIndex < 0)
        {
            return;
        }
        ReorderDemoLogic.MoveItem(items, itemId, rawInsertionIndex);
        for (var index = 0; index < rows.Count; ++index)
        {
            rows[index].BindItem(items[index]);
        }
        SyncAll();
    }

    private static FlexBox Spacer(float height) => new FlexBox().FillWidth().Height(height);

    private static string Slot(int index) => (index + 1).ToString(CultureInfo.InvariantCulture);

    private static string? DraggedItemId(DragEventArgs args) => args.Session.Data.GetFormat(ReorderDemoLogic.DragFormat);

    private void HandleAutoScrollTimer()
    {
        if (activeDragItemId is null || autoScrollDeltaY == 0.0f)
        {
            StopAutoScroll();
            return;
        }
        var currentOffset = scrollBox.ScrollState.OffsetY.Value;
        var nextOffset = ReorderDemoLogic.NextAutoScrollOffset(currentOffset, autoScrollDeltaY, items.Count, ViewportHeight());
        if (nextOffset == currentOffset)
        {
            StopAutoScroll();
            return;
        }
        scrollBox.SetScrollOffset(0.0f, nextOffset);
        var direction = autoScrollDeltaY < 0.0f ? -1 : 1;
        rawInsertionIndex = ReorderDemoLogic.EdgeInsertionIndex(direction, items.Count, VisibleRange());
        dragStatusMessage = "Reorder drag status: auto-scrolling " + (direction < 0 ? "up" : "down");
        SyncAll();
        ArmAutoScrollTimer();
    }

    private void ClearPreview()
    {
        previewPointerX = float.NaN;
        previewPointerY = float.NaN;
        previewEffect = DragDropEffects.None;
        previewInsertionSlot = -1;
    }

    private string OrderSummary() => "Reorder order: " + string.Join(" | ", items.ConvertAll(item => item.Label));

    private string ActiveDragLabel() => FindItem(activeDragItemId)?.Label ?? "item";

    private ReorderDemoItem? FindItem(string? itemId)
    {
        var index = itemId is null ? -1 : ReorderDemoLogic.FindItemIndex(items, itemId);
        return index < 0 ? null : items[index];
    }

    private void SyncAll()
    {
        orderStatusText.Text = OrderSummary();
        dragStatusText.Text = dragStatusMessage;
        SyncViewportStatus();
        SyncPreviewGhost();
        ApplyTheme();
    }

    private void SyncViewportStatus()
    {
        var visibleRange = VisibleRange();
        var firstVisible = visibleRange.LastVisibleIndex < 0 ? 0 : visibleRange.FirstVisibleIndex + 1;
        var lastVisible = visibleRange.LastVisibleIndex < 0 ? 0 : visibleRange.LastVisibleIndex + 1;
        viewportStatusText.Text = string.Create(
            CultureInfo.InvariantCulture,
            $"Reorder viewport status: offset {(int)scrollBox.ScrollState.OffsetY.Value} | visible {firstVisible}-{lastVisible}");
    }

    /// <summary>Shows the drag preview just below and right of the pointer, kept inside the section.</summary>
    private void SyncPreviewGhost()
    {
        var item = FindItem(activeDragItemId);
        if (item is null || float.IsNaN(previewPointerX) || float.IsNaN(previewPointerY))
        {
            previewGhost.Visibility(Visibility.Hidden).Opacity(0.0f);
            return;
        }
        previewTitleText.Text = item.Label;
        previewDetailText.Text = item.Detail;
        previewEffectText.Text = previewEffect == DragDropEffects.Move && previewInsertionSlot >= 0
            ? "Drop to move to slot " + Slot(previewInsertionSlot)
            : "Release outside the list to cancel";
        previewGhost.SemanticLabel = "Reorder drag preview for " + item.Label;
        var bounds = sectionRoot.GetBounds();
        var sectionWidth = bounds.Width > 0.0f ? bounds.Width : FallbackExtent(Application.ViewportWidth.Value, PreviewWidth);
        var sectionHeight = bounds.Height > 0.0f ? bounds.Height : FallbackExtent(Application.ViewportHeight.Value, PreviewHeight);
        var pointer = sectionRoot.AbsoluteToLocalPosition(previewPointerX, previewPointerY);
        var maxX = Math.Max(PreviewMargin, sectionWidth - PreviewWidth - PreviewMargin);
        var maxY = Math.Max(PreviewMargin, sectionHeight - PreviewHeight - PreviewMargin);
        previewGhost
            .Position(Math.Clamp(pointer.X + PreviewOffsetX, PreviewMargin, maxX), Math.Clamp(pointer.Y + PreviewOffsetY, PreviewMargin, maxY))
            .Visibility(Visibility.Normal)
            .Opacity(0.96f);
    }

    private static float FallbackExtent(float viewport, float preview) => viewport > 0.0f ? viewport : preview + (PreviewMargin * 2.0f);

    private void ApplyTheme()
    {
        var colors = theme.Colors;
        DemoScrollBars.ApplyTheme(scrollBox, theme);
        foreach (var row in rows)
        {
            row.ApplyTheme(theme, activeDragItemId, rawInsertionIndex);
        }
        endMarker.Background(colors.Accent).Opacity(rawInsertionIndex == items.Count ? 1.0f : 0.0f);
        endDropZone.Background(DemoColors.CardBackgroundAlt(theme)).Border(1.0f, DemoColors.DividerColor(theme));
        endDropText.TextColor(DemoColors.MutedText(theme));
        orderStatusText.TextColor(DemoColors.PrimaryText(theme));
        dragStatusText.TextColor(DemoColors.MutedText(theme));
        viewportStatusText.TextColor(DemoColors.MutedText(theme));
        var accepting = previewEffect == DragDropEffects.Move;
        previewGhost
            .Background(colors.Surface)
            .Border(1.0f, accepting ? colors.Accent : DemoColors.DividerColor(theme))
            .DropShadow(theme.ToolTip.PanelShadowColor, 0.0f, theme.ToolTip.ShadowOffsetY, theme.ToolTip.ShadowBlur, theme.ToolTip.ShadowSpread);
        previewCaptionText.TextColor(DemoColors.MutedText(theme));
        previewTitleText.TextColor(DemoColors.PrimaryText(theme));
        previewDetailText.TextColor(DemoColors.MutedText(theme));
        previewEffectText.TextColor(accepting ? colors.Accent : DemoColors.PrimaryText(theme));
    }

    private ReorderVisibleRange VisibleRange() =>
        ReorderDemoLogic.VisibleRange(items.Count, scrollBox.ScrollState.OffsetY.Value, ViewportHeight());

    private float ViewportHeight()
    {
        var current = scrollBox.ScrollState.ViewportHeight.Value;
        return current > 0.0f ? current : ReorderDemoLogic.ViewportHeight;
    }

    private float PointerAutoScrollDelta(float pointerY)
    {
        var bounds = scrollBox.Viewport.GetBounds();
        return ReorderDemoLogic.PointerAutoScrollDelta(pointerY, bounds.Y, bounds.Height);
    }

    /// <summary>
    /// Starts auto-scrolling at <paramref name="nextDeltaY"/> per step, or stops it at zero. A pointer event that keeps
    /// the same speed re-arms the step instead, so the list steps only while the pointer rests.
    /// </summary>
    private void SetAutoScrollDelta(float nextDeltaY)
    {
        if (MathF.Abs(autoScrollDeltaY - nextDeltaY) <= 0.05f)
        {
            if (nextDeltaY != 0.0f)
            {
                ArmAutoScrollTimer();
            }
            return;
        }
        autoScrollDeltaY = nextDeltaY;
        if (nextDeltaY == 0.0f)
        {
            autoScrollTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return;
        }
        ArmAutoScrollTimer();
    }

    private void ArmAutoScrollTimer()
    {
        if (autoScrollTimer is null)
        {
            autoScrollTimer = TimeProvider.System.CreateTimer(_ => HandleAutoScrollTimer(), null, AutoScrollDelay, Timeout.InfiniteTimeSpan);
            return;
        }
        autoScrollTimer.Change(AutoScrollDelay, Timeout.InfiniteTimeSpan);
    }

    private void StopAutoScroll()
    {
        autoScrollDeltaY = 0.0f;
        autoScrollTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }
}
