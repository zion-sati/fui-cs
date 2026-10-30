using System;
using System.Collections.Generic;

namespace FuiCs.Demo.AdvancedControls.Reorder;

/// <summary>One row of the reorder list.</summary>
internal sealed record ReorderDemoItem(string Id, string Label, string Detail);

/// <summary>The rows a viewport shows, by index; an empty list has <see cref="LastVisibleIndex"/> -1.</summary>
internal readonly record struct ReorderVisibleRange(int FirstVisibleIndex = 0, int LastVisibleIndex = -1);

/// <summary>
/// The reorder list's geometry and insertion math, apart from its nodes. Each row is a slot: an insertion marker above
/// the row's card. An insertion index is the slot the pointer is over, from 0 to the item count (the end slot), counted
/// before the dragged item is taken out of the list.
/// </summary>
internal static class ReorderDemoLogic
{
    public const string DragFormat = "application/x-effindom-reorder-item-id";
    public const float MarkerHeight = 8.0f;
    public const float RowBodyHeight = 72.0f;
    public const float SlotHeight = MarkerHeight + RowBodyHeight;
    public const float EndSlotHeight = 44.0f;
    public const float ViewportHeight = 248.0f;
    public const float AutoScrollEdgeZone = 48.0f;
    public const float AutoScrollMaxOutside = 120.0f;
    public const float AutoScrollMinStep = 4.0f;
    public const float AutoScrollMaxStep = 34.0f;

    public static List<ReorderDemoItem> CreateItems() =>
    [
        new("core-rename", "Document Core rename", "Keep Tier 1 references consistently renamed to Core."),
        new("font-cache", "Audit font shard cache", "Check cache bounds, eviction, and diagnostics before release."),
        new("drag-demo", "Add drag reorder demo", "Prove the phase-4 drag/drop controller on a real routed sample."),
        new("nested-scroll", "Write nested scroll test", "Lock drop-target routing when a scrollable list sits inside a scrolled route."),
        new("key-router", "Split key router", "Keep keyboard routing focused instead of growing another god file."),
        new("semantics", "Tighten semantic ordering", "Make sure reorder status reads in the same order the canvas shows."),
        new("find-mirror", "Probe find mirror fallback", "Double-check the hidden DOM mirror when large text windows shift."),
        new("drop-cursor", "Validate drop cursor states", "Keep drag affordances coherent while the source stays captured."),
    ];

    public static int FindItemIndex(IReadOnlyList<ReorderDemoItem> items, string itemId)
    {
        for (var index = 0; index < items.Count; ++index)
        {
            if (items[index].Id == itemId)
            {
                return index;
            }
        }
        return -1;
    }

    public static float ContentHeight(int itemCount) => (Math.Max(itemCount, 0) * SlotHeight) + EndSlotHeight;

    public static ReorderVisibleRange VisibleRange(int itemCount, float offsetY, float viewportHeight)
    {
        if (itemCount <= 0)
        {
            return new ReorderVisibleRange(0, -1);
        }
        var first = Math.Clamp((int)MathF.Floor(offsetY / SlotHeight), 0, itemCount - 1);
        var effectiveViewportHeight = viewportHeight > 0.0f ? viewportHeight : ViewportHeight;
        var last = (int)MathF.Floor((offsetY + effectiveViewportHeight - 1.0f) / SlotHeight);
        last = Math.Min(Math.Max(last, first), itemCount - 1);
        return new ReorderVisibleRange(first, last);
    }

    /// <summary>The item's index after the move: the insertion slot, less one when the dragged item came before it.</summary>
    public static int NormalizeInsertionIndex(int sourceIndex, int rawInsertionIndex, int itemCount)
    {
        if (itemCount <= 0)
        {
            return 0;
        }
        var normalized = Math.Clamp(rawInsertionIndex, 0, itemCount);
        if (sourceIndex >= 0 && sourceIndex < itemCount && sourceIndex < normalized)
        {
            normalized -= 1;
        }
        return Math.Clamp(normalized, 0, itemCount - 1);
    }

    /// <summary>Moves the item to the insertion slot; false when it is not in the list or would not move.</summary>
    public static bool MoveItem(List<ReorderDemoItem> items, string itemId, int rawInsertionIndex)
    {
        var sourceIndex = FindItemIndex(items, itemId);
        if (sourceIndex < 0 || items.Count <= 1)
        {
            return false;
        }
        var targetIndex = NormalizeInsertionIndex(sourceIndex, rawInsertionIndex, items.Count);
        if (targetIndex == sourceIndex)
        {
            return false;
        }
        var moved = items[sourceIndex];
        items.RemoveAt(sourceIndex);
        items.Insert(targetIndex, moved);
        return true;
    }

    public static float MaxScrollOffset(int itemCount, float viewportHeight) =>
        Math.Max(ContentHeight(itemCount) - viewportHeight, 0.0f);

    /// <summary>
    /// -1 to scroll up, 1 to scroll down, 0 to stay: an insertion at the first or last visible row scrolls when rows
    /// lie beyond it, and so does the end slot while the last row is out of view.
    /// </summary>
    public static int EdgeAutoScrollDirection(int rawInsertionIndex, int itemCount, ReorderVisibleRange visibleRange)
    {
        if (itemCount <= 0 || visibleRange.LastVisibleIndex < 0)
        {
            return 0;
        }
        if (rawInsertionIndex <= visibleRange.FirstVisibleIndex && visibleRange.FirstVisibleIndex > 0)
        {
            return -1;
        }
        if (rawInsertionIndex >= visibleRange.LastVisibleIndex && visibleRange.LastVisibleIndex < itemCount - 1)
        {
            return 1;
        }
        return rawInsertionIndex >= itemCount && visibleRange.LastVisibleIndex < itemCount - 1 ? 1 : 0;
    }

    /// <summary>The insertion slot at the edge the list scrolls toward, or -1 for no direction.</summary>
    public static int EdgeInsertionIndex(int direction, int itemCount, ReorderVisibleRange visibleRange)
    {
        if (direction < 0)
        {
            return visibleRange.FirstVisibleIndex;
        }
        if (direction > 0)
        {
            return Math.Min(visibleRange.LastVisibleIndex + 1, itemCount);
        }
        return -1;
    }

    /// <summary>
    /// How far one auto-scroll step moves for a pointer at <paramref name="pointerY"/>: nothing outside the edge zones,
    /// and faster the deeper the pointer is in a zone, past the viewport's edge included. Negative scrolls up.
    /// </summary>
    public static float PointerAutoScrollDelta(float pointerY, float viewportTopY, float viewportHeight)
    {
        if (viewportHeight <= 0.0f)
        {
            return 0.0f;
        }
        var topZoneBottom = viewportTopY + AutoScrollEdgeZone;
        if (pointerY <= topZoneBottom)
        {
            return -AutoScrollStep(topZoneBottom - pointerY);
        }
        var bottomZoneTop = viewportTopY + viewportHeight - AutoScrollEdgeZone;
        return pointerY >= bottomZoneTop ? AutoScrollStep(pointerY - bottomZoneTop) : 0.0f;
    }

    public static float NextAutoScrollOffset(float currentOffsetY, float deltaY, int itemCount, float viewportHeight) =>
        deltaY == 0.0f ? currentOffsetY : Math.Clamp(currentOffsetY + deltaY, 0.0f, MaxScrollOffset(itemCount, viewportHeight));

    private static float AutoScrollStep(float activationDistance)
    {
        if (activationDistance <= 0.0f)
        {
            return 0.0f;
        }
        var normalized = Math.Clamp(activationDistance / (AutoScrollEdgeZone + AutoScrollMaxOutside), 0.0f, 1.0f);
        return AutoScrollMinStep + ((AutoScrollMaxStep - AutoScrollMinStep) * normalized * normalized);
    }
}
