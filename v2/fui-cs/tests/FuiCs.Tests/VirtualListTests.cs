using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class VirtualListTests
{
    /// <summary>A binder that records the indices it binds and labels each row's text "Item n".</summary>
    private static Action<FlexBox, int> Tracking(List<int> rendered) => (container, index) =>
    {
        rendered.Add(index);
        if (container.Children.Count == 0)
        {
            container.Child(new Text());
        }
        var text = (Text)container.Children[0];
        text.Text = "Item " + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        text.SemanticLabel = text.Text;
    };

    [Test]
    public async Task OnlyTheVisibleWindowOfAFixedHeightListIsBound()
    {
        TestRuntime.Start(new LayoutAbi());
        var rendered = new List<int>();
        var list = new VirtualList(10000, 20.0f).OnBindItem(Tracking(rendered)).Width(180.0f).Height(100.0f);

        list.Build();

        var lastWindow = rendered.GetRange(rendered.Count - 6, 6);
        await Assert.That(string.Join(",", lastWindow)).IsEqualTo("0,1,2,3,4,5");
        await Assert.That(list.FirstVisibleIndex).IsEqualTo(0);
        await Assert.That(list.RenderedItemCount).IsEqualTo(6);
        list.Dispose();
    }

    [Test]
    public async Task ScrollingRebindsPooledRowsWithoutCreatingNodes()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var rendered = new List<int>();
        var list = new VirtualList(10000, 20.0f).OnBindItem(Tracking(rendered)).Width(180.0f).Height(100.0f);
        list.Build();
        rendered.Clear();
        abi.Calls.Clear();

        list.ScrollState.OffsetY.Value = 60.0f;

        await Assert.That(string.Join(",", rendered)).IsEqualTo("3,4,5,6,7,8");
        await Assert.That(list.FirstVisibleIndex).IsEqualTo(3);
        await Assert.That(abi.CountOf("ui_create_node")).IsEqualTo(0);
        await Assert.That(abi.CountOf("ui_delete_node")).IsEqualTo(0);
        await Assert.That(abi.CountOf("ui_set_text") > 0).IsTrue();
        await Assert.That(abi.CountOf("ui_set_height") > 0).IsTrue();
        // Items 0 to 2 left the window, so their selections are cleared; items 3 to 5 moved rows, so theirs follow them.
        await Assert.That(abi.CountOf("ui_clear_selection") > 0).IsTrue();
        await Assert.That(abi.CountOf("ui_retarget_selection") > 0).IsTrue();
        list.Dispose();
    }

    [Test]
    public async Task TheContentHeightFollowsTheItemCountAndTheWindowShrinksWithIt()
    {
        TestRuntime.Start(new LayoutAbi());
        var list = new VirtualList(10000, 24.0f).OnBindItem(Tracking([])).Height(120.0f);
        var before = (list.ScrollState.ContentHeight.Value, list.RenderedItemCount);

        list.UpdateItemCount(5);

        await Assert.That(before).IsEqualTo((240000.0f, 6));
        await Assert.That((list.ScrollState.ContentHeight.Value, list.RenderedItemCount)).IsEqualTo((120.0f, 5));
        list.Dispose();
    }

    [Test]
    public async Task ShrinkingTheListPullsTheOffsetBackIntoTheContent()
    {
        TestRuntime.Start(new LayoutAbi());
        var list = new VirtualList(100, 20.0f).OnBindItem(Tracking([])).Height(100.0f);
        var handle = list.ScrollBox.Viewport.Build();
        HostEvents.OnScroll(handle, 0.0f, 1000.0f, 180.0f, 2000.0f, 180.0f, 100.0f);

        list.UpdateItemCount(10);

        // Ten items are 200 px tall, so the view can scroll 100 px at most.
        await Assert.That(list.ScrollBox.VerticalOffset).IsEqualTo(100.0f);
        list.Dispose();
    }

    [Test]
    public async Task TheListIsASelectionBarrierAndItsIdAndPersistenceAreItsScrollBoxs()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var list = new VirtualList(10, 20.0f).OnBindItem(Tracking([])).NodeId("sidebar").PersistScroll(false);

        var handle = list.Build();

        await Assert.That(abi.HasCall("ui_set_selection_area_barrier", handle, true)).IsTrue();
        await Assert.That(list.ScrollBox.NodeId).IsEqualTo("sidebar");
        await Assert.That(list.ScrollBox.PersistScroll).IsFalse();
        await Assert.That(abi.HasCall("ui_set_node_id", list.ScrollBox.Handle, "sidebar")).IsTrue();
        list.Dispose();
    }

    [Test]
    public async Task RenderingWithoutABinderOrWithBadSizesThrows()
    {
        TestRuntime.Start(new LayoutAbi());
        var unbound = new VirtualList(10, 20.0f);

        await Assert.That(Throws<InvalidOperationException>(() => unbound.Height(100.0f))).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => new VirtualList(10, 0.0f))).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => new VirtualList(-1, 20.0f))).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => unbound.UpdateItemCount(-1))).IsTrue();
    }

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }
}
