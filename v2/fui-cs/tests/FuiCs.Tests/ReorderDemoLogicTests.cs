using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Demo.AdvancedControls.Reorder;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ReorderDemoLogicTests
{
    private const float ViewportHeight = ReorderDemoLogic.ViewportHeight;

    [Test]
    public async Task InsertionSlotsAreNormalizedAfterTheDraggedItemIsTakenOut()
    {
        await Assert.That(ReorderDemoLogic.NormalizeInsertionIndex(1, 5, 8)).IsEqualTo(4);
        await Assert.That(ReorderDemoLogic.NormalizeInsertionIndex(5, 1, 8)).IsEqualTo(1);
        await Assert.That(ReorderDemoLogic.NormalizeInsertionIndex(7, 8, 8)).IsEqualTo(7);
    }

    [Test]
    public async Task ItemsMoveToTheirInsertionSlots()
    {
        var items = ReorderDemoLogic.CreateItems();

        var movedDown = ReorderDemoLogic.MoveItem(items, "drag-demo", 7);
        var afterDown = Labels(items);
        var downIndex = ReorderDemoLogic.FindItemIndex(items, "drag-demo");
        var movedUp = ReorderDemoLogic.MoveItem(items, "drag-demo", 0);

        await Assert.That(movedDown).IsTrue();
        await Assert.That(afterDown).IsEqualTo(
            "Document Core rename | Audit font shard cache | Write nested scroll test | Split key router | Tighten semantic ordering | Probe find mirror fallback | Add drag reorder demo | Validate drop cursor states");
        await Assert.That(downIndex).IsEqualTo(6);
        await Assert.That(movedUp).IsTrue();
        await Assert.That(ReorderDemoLogic.FindItemIndex(items, "drag-demo")).IsEqualTo(0);
        await Assert.That(ReorderDemoLogic.MoveItem(items, "drag-demo", 0)).IsFalse();
        await Assert.That(ReorderDemoLogic.MoveItem(items, "missing", 3)).IsFalse();
    }

    [Test]
    public async Task TheViewportGivesTheVisibleRowsAndTheEdgeToScrollToward()
    {
        var top = ReorderDemoLogic.VisibleRange(8, 0.0f, ViewportHeight);
        var middle = ReorderDemoLogic.VisibleRange(8, 160.0f, ViewportHeight);

        await Assert.That(top).IsEqualTo(new ReorderVisibleRange(0, 3));
        await Assert.That(ReorderDemoLogic.EdgeAutoScrollDirection(0, 8, top)).IsEqualTo(0);
        await Assert.That(ReorderDemoLogic.EdgeAutoScrollDirection(3, 8, top)).IsEqualTo(1);
        await Assert.That(ReorderDemoLogic.EdgeInsertionIndex(1, 8, top)).IsEqualTo(4);
        await Assert.That(middle).IsEqualTo(new ReorderVisibleRange(2, 5));
        await Assert.That(ReorderDemoLogic.EdgeAutoScrollDirection(2, 8, middle)).IsEqualTo(-1);
        await Assert.That(ReorderDemoLogic.EdgeInsertionIndex(-1, 8, middle)).IsEqualTo(2);
        await Assert.That(ReorderDemoLogic.VisibleRange(0, 0.0f, ViewportHeight)).IsEqualTo(new ReorderVisibleRange(0, -1));
    }

    [Test]
    public async Task AutoScrollStaysWithinTheContent()
    {
        var contentHeight = ReorderDemoLogic.ContentHeight(8);

        await Assert.That(ReorderDemoLogic.NextAutoScrollOffset(0.0f, -18.0f, 8, ViewportHeight)).IsEqualTo(0.0f);
        await Assert.That(ReorderDemoLogic.NextAutoScrollOffset(0.0f, 18.0f, 8, ViewportHeight) > 0.0f).IsTrue();
        await Assert.That(ReorderDemoLogic.NextAutoScrollOffset(contentHeight, 18.0f, 8, ViewportHeight)).IsEqualTo(contentHeight - ViewportHeight);
    }

    [Test]
    public async Task AutoScrollSpeedsUpAsThePointerNearsAndPassesTheEdge()
    {
        const float viewportTop = 120.0f;
        const float viewportBottom = viewportTop + ViewportHeight;
        var bottomInner = Delta(viewportBottom - ReorderDemoLogic.AutoScrollEdgeZone + 4.0f);
        var bottomNearEdge = Delta(viewportBottom - 1.0f);
        var bottomOutside = Delta(viewportBottom + 36.0f);
        var topInner = Delta(viewportTop + ReorderDemoLogic.AutoScrollEdgeZone - 4.0f);
        var topNearEdge = Delta(viewportTop + 1.0f);
        var topOutside = Delta(viewportTop - 36.0f);

        await Assert.That(bottomInner > 0.0f).IsTrue();
        await Assert.That(bottomNearEdge > bottomInner).IsTrue();
        await Assert.That(bottomOutside > bottomNearEdge).IsTrue();
        await Assert.That(topInner < 0.0f).IsTrue();
        await Assert.That(topNearEdge < topInner).IsTrue();
        await Assert.That(topOutside < topNearEdge).IsTrue();
        await Assert.That(Delta(viewportTop + (ViewportHeight / 2.0f))).IsEqualTo(0.0f);

        static float Delta(float pointerY) => ReorderDemoLogic.PointerAutoScrollDelta(pointerY, viewportTop, ViewportHeight);
    }

    private static string Labels(List<ReorderDemoItem> items) => string.Join(" | ", items.ConvertAll(item => item.Label));
}
