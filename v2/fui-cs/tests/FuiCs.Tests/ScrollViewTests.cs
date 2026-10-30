using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ScrollViewTests
{
    private static readonly string[] ScrollCalls =
    [
        "ui_create_node", "ui_set_interactive", "ui_set_scroll_enabled", "ui_set_smooth_scrolling", "ui_set_scroll_friction",
        "ui_set_scroll_content_size", "ui_set_width", "ui_set_height", "ui_set_flex_basis", "ui_set_scroll_offset", "ui_node_add_child",
    ];

    [Test]
    public async Task BuildSendsScrollSettingsSizeInitialOffsetAndContent()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView()
            .Width(300.0f)
            .Height(200.0f)
            .Friction(0.9f)
            .FlexBasis(120.0f)
            .ScrollContentSize(null, 1000.0f)
            .ScrollOffset(0.0f, 40.0f)
            .Child(new TestNode());

        var handle = scroll.Build();

        await Assert.That(abi.Sequence(ScrollCalls)).IsEqualTo(
            "ui_create_node,ui_set_interactive,ui_set_scroll_enabled,ui_set_smooth_scrolling,ui_set_scroll_friction,"
            + "ui_set_scroll_content_size,ui_set_width,ui_set_height,ui_set_flex_basis,ui_set_scroll_offset,ui_create_node,ui_node_add_child");
        await Assert.That(abi.HasCall("ui_create_node", NodeType.ScrollView)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_scroll_enabled", handle, true, true)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_scroll_content_size", handle, -1.0f, 1000.0f)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_scroll_offset", handle, 0.0f, 40.0f)).IsTrue();
        await Assert.That(abi.CountOf("ui_clear_momentum_scroll")).IsEqualTo(0);
        await Assert.That(scroll.ViewportWidth).IsEqualTo(300.0f);
        await Assert.That(scroll.ExtentHeight).IsEqualTo(1000.0f);
    }

    [Test]
    public async Task HostScrollsUpdateTheViewAndRaiseScrollChangedOncePerChange()
    {
        TestRuntime.Start(new LayoutAbi());
        var changes = new List<string>();
        var scroll = new ScrollView().OnScrollChanged(e => changes.Add(e.VerticalOffset + "/" + e.ExtentHeight + "/" + e.ViewportHeight));
        var handle = scroll.Build();

        HostEvents.OnScroll(handle, 0.0f, 25.0f, 300.0f, 900.0f, 300.0f, 200.0f);
        HostEvents.OnScroll(handle, 0.0f, 25.0f, 300.0f, 900.0f, 300.0f, 200.0f);
        HostEvents.OnScroll(handle, 0.0f, 50.0f, 300.0f, 900.0f, 300.0f, 200.0f);

        await Assert.That(string.Join(",", changes)).IsEqualTo("25/900/200,50/900/200");
        await Assert.That(scroll.VerticalOffset).IsEqualTo(50.0f);
        await Assert.That(scroll.ExtentWidth).IsEqualTo(300.0f);
        await Assert.That(scroll.ViewportWidth).IsEqualTo(300.0f);
    }

    [Test]
    public async Task ProgrammaticScrollsTakeOverMomentumAndSwallowTheHostAcknowledgement()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var offsets = new List<float>();
        var scroll = new ScrollView().OnScrollChanged(e => offsets.Add(e.VerticalOffset));
        var handle = scroll.Build();
        abi.Calls.Clear();

        scroll.ScrollTo(0.0f, 120.0f);
        HostEvents.OnScroll(handle, 0.0f, 120.4f, 0.0f, 0.0f, 0.0f, 0.0f);
        var afterAcknowledgement = scroll.VerticalOffset;
        HostEvents.OnScroll(handle, 0.0f, 90.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        scroll.ScrollTo(0.0f, 90.0f);

        await Assert.That(abi.Sequence("ui_clear_momentum_scroll", "ui_set_scroll_offset", "request_render"))
            .IsEqualTo("ui_clear_momentum_scroll,ui_set_scroll_offset,request_render,ui_set_scroll_offset");
        await Assert.That(afterAcknowledgement).IsEqualTo(120.0f);
        await Assert.That(string.Join(",", offsets)).IsEqualTo("120,90");
    }

    [Test]
    public async Task RuntimeOffsetsKeepMomentum()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView();
        var handle = scroll.Build();

        scroll.SetRuntimeScrollOffset(0.0f, 64.0f);

        await Assert.That(abi.HasCall("ui_set_scroll_offset", handle, 0.0f, 64.0f)).IsTrue();
        await Assert.That(abi.CountOf("ui_clear_momentum_scroll")).IsEqualTo(0);
    }

    [Test]
    public async Task AxesFrictionAndSmoothingUpdateTheBuiltView()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView();
        var handle = scroll.Build();
        abi.Calls.Clear();

        scroll.HorizontalScrollEnabled(false).SmoothScrolling(false).Friction(0.5f);

        await Assert.That(abi.HasCall("ui_set_scroll_enabled", handle, false, true)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_smooth_scrolling", handle, false)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_scroll_friction", handle, 0.5f)).IsTrue();
        await Assert.That(scroll.IsHorizontalScrollEnabled).IsFalse();
        await Assert.That(scroll.IsVerticalScrollEnabled).IsTrue();
    }

    [Test]
    public async Task ScrollViewsAndFlexBoxesShareTheFluentContentMembers()
    {
        TestRuntime.Start(new LayoutAbi());
        var first = new TestNode();
        var second = new TestNode();

        var scroll = new ScrollView().Child(first).Children(first, second);
        var box = new FlexBox().Children(new TestNode()).Child(new ScrollView());

        await Assert.That(scroll.Children.Count).IsEqualTo(2);
        await Assert.That(box.Children.Count).IsEqualTo(2);
        await Assert.That(ReferenceEquals(second.Parent, scroll)).IsTrue();
    }
}
