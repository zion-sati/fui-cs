using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ScrollBoxTests
{
    private static (LayoutAbi Abi, ScrollBox Box, FlexBox Content) Mount(ScrollBarVisibility visibility = ScrollBarVisibility.Auto)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var content = new FlexBox().Width(200.0f).Height(400.0f);
        var box = new ScrollBox().NodeId("list").Width(200.0f).Height(100.0f).VerticalScrollBarVisibility(visibility).Child(content);
        Application.Mount(FlexBox.Column(box));
        return (abi, box, content);
    }

    /// <summary>The host reports the view's offset, content and viewport, as it does after layout and on scroll.</summary>
    private static void Report(ScrollBox box, float offsetY, float contentHeight = 400.0f) =>
        HostEvents.OnScroll(box.Viewport.Handle, 0.0f, offsetY, 192.0f, contentHeight, 192.0f, 100.0f);

    private static FlexBox Thumb(ScrollBar bar) => (FlexBox)bar.Root.Children[0].Children[1];

    [Test]
    public async Task TheBoxHoldsTheViewAndBarsInTheirRowsAndContentGoesIntoTheView()
    {
        var (_, box, content) = Mount();

        var top = box.Children[0];
        var bottom = box.Children[1];

        await Assert.That(box.Children.Count).IsEqualTo(2);
        await Assert.That(ReferenceEquals(top.Children[0], box.Viewport)).IsTrue();
        await Assert.That(ReferenceEquals(top.Children[2], box.VerticalScrollBar.Root)).IsTrue();
        await Assert.That(ReferenceEquals(bottom.Children[0], box.HorizontalScrollBar.Root)).IsTrue();
        await Assert.That(top.Children.Count).IsEqualTo(3);
        await Assert.That(bottom.Children.Count).IsEqualTo(2);
        await Assert.That(ReferenceEquals(content.Parent, box.Viewport)).IsTrue();
    }

    [Test]
    public async Task TheBoxItsChromeAndItsContentScrollTheViewWhenWheeled()
    {
        var (abi, box, content) = Mount();
        var view = box.Viewport.Handle;
        var top = box.Children[0];

        foreach (var node in new Node[] { box, top, box.Children[1], top.Children[1], box.Children[1].Children[1], content, Thumb(box.VerticalScrollBar), box.VerticalScrollBar.Root })
        {
            await Assert.That(abi.HasCall("ui_set_scroll_proxy_target", (object)node.Handle, view)).IsTrue();
        }
    }

    [Test]
    public async Task TheVerticalBarShowsOnlyWhileTheContentOverflowsInAutoMode()
    {
        var (_, box, _) = Mount();

        var before = box.VerticalScrollBar.Root.Width;
        Report(box, 0.0f);
        var overflowing = box.VerticalScrollBar.Root.Width;
        Report(box, 0.0f, contentHeight: 90.0f);

        await Assert.That(before).IsEqualTo((Length)0.0f);
        await Assert.That(overflowing).IsEqualTo((Length)8.0f);
        await Assert.That(box.VerticalScrollBar.Root.Width).IsEqualTo((Length)0.0f);
        await Assert.That(box.HorizontalScrollBar.Root.Height).IsEqualTo((Length)0.0f);
    }

    [Test]
    public async Task VisibleAndHiddenOverrideTheOverflowAndADisabledAxisShowsNoBar()
    {
        var (_, box, _) = Mount(ScrollBarVisibility.Visible);

        var visible = box.VerticalScrollBar.Root.Width;
        box.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        var corner = (FlexBox)box.Children[1].Children[1];
        var cornerSize = (corner.Width, corner.Height);
        box.IsHorizontalScrollEnabled = false;
        Report(box, 0.0f);
        box.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

        await Assert.That(visible).IsEqualTo((Length)8.0f);
        await Assert.That(cornerSize).IsEqualTo(((Length)8.0f, (Length)8.0f));
        await Assert.That(box.HorizontalScrollBar.Root.Height).IsEqualTo((Length)0.0f);
        await Assert.That(box.VerticalScrollBar.Root.Width).IsEqualTo((Length)0.0f);
    }

    [Test]
    public async Task TheThumbIsSizedToTheVisiblePartAndFollowsTheOffset()
    {
        var (_, box, _) = Mount();
        var bar = box.VerticalScrollBar;

        Report(box, 150.0f);
        var leading = (FlexBox)bar.Root.Children[0].Children[0];

        await Assert.That(Thumb(bar).Height).IsEqualTo((Length)25.0f);
        await Assert.That(leading.Height).IsEqualTo((Length)37.5f);
        await Assert.That(box.ScrollState.OffsetY.Value).IsEqualTo(150.0f);
    }

    [Test]
    public async Task DraggingTheThumbScrollsTheViewAndKeepsGoingOutsideTheTrack()
    {
        var (abi, box, _) = Mount();
        Report(box, 0.0f);
        var thumb = Thumb(box.VerticalScrollBar);
        abi.Bounds[thumb.Handle] = new Rect(192.0f, 0.0f, 8.0f, 25.0f);

        HostInput.Pointer(PointerEventType.Down, thumb.Handle, 196.0f, 10.0f);
        HostInput.Pointer(PointerEventType.Move, thumb.Handle, 196.0f, 40.0f);
        var dragged = box.VerticalOffset;
        HostInput.Pointer(PointerEventType.Move, thumb.Handle, 600.0f, 500.0f);
        HostInput.Pointer(PointerEventType.Up, thumb.Handle, 600.0f, 500.0f);

        // 30 px of thumb travel over 75 px of track scrolls 30 / 75 of the 300 px range.
        await Assert.That(dragged).IsEqualTo(120.0f);
        await Assert.That(box.VerticalOffset).IsEqualTo(300.0f);
        await Assert.That(abi.HasCall("ui_set_scroll_offset", (object)box.Viewport.Handle, 0.0f, 120.0f)).IsTrue();
        await Assert.That(box.VerticalScrollBar.IsDragging).IsFalse();
    }

    [Test]
    public async Task PressingTheTrackCentersTheThumbOnThePress()
    {
        var (abi, box, _) = Mount();
        Report(box, 0.0f);
        var track = box.VerticalScrollBar.Root;
        abi.Bounds[track.Handle] = new Rect(192.0f, 0.0f, 8.0f, 100.0f);

        HostInput.Pointer(PointerEventType.Down, track.Handle, 196.0f, 50.0f);

        // The thumb's center lands on 50: its top at 37.5 of 75 px of travel, half of the 300 px range.
        await Assert.That(box.VerticalOffset).IsEqualTo(150.0f);
    }

    [Test]
    public async Task KeysScrollTheViewAroundTheFocusedNode()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var button = new Button("In the list");
        var box = new ScrollBox().Width(200.0f).Height(100.0f).Child(FlexBox.Column(button, new FlexBox().Height(380.0f)));
        Application.Mount(FlexBox.Column(box));
        Report(box, 0.0f);
        HostEvents.OnFocusChanged(button.Handle, true);
        var offsets = new List<float>();

        foreach (var key in new[] { "ArrowDown", "PageDown", "End", "ArrowUp", "Home" })
        {
            HostInput.Key(key);
            offsets.Add(box.VerticalOffset);
        }

        await Assert.That(string.Join(",", offsets)).IsEqualTo("40,100,300,260,0");
        await Assert.That(abi.HasCall("ui_set_scroll_offset", (object)box.Viewport.Handle, 0.0f, 300.0f)).IsTrue();
    }

    [Test]
    public async Task WithNothingFocusedKeysScrollTheViewLastClicked()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var first = new ScrollBox().Width(200.0f).Height(100.0f).Child(new FlexBox().Height(400.0f));
        var item = new FlexBox().Height(400.0f);
        var second = new ScrollBox().Width(200.0f).Height(100.0f).Child(item);
        Application.Mount(FlexBox.Column(first, second));
        // The host lays out every node; the views are ordered by where their outermost ancestors sit.
        for (Node? node = first.Parent; node is not null; node = node.Parent)
        {
            abi.Bounds[node.Handle] = new Rect(0.0f, 0.0f, 800.0f, 600.0f);
        }
        abi.Bounds[first.Handle] = new Rect(0.0f, 0.0f, 200.0f, 100.0f);
        abi.Bounds[second.Handle] = new Rect(0.0f, 100.0f, 200.0f, 100.0f);
        abi.Bounds[first.Viewport.Handle] = new Rect(0.0f, 0.0f, 200.0f, 100.0f);
        abi.Bounds[second.Viewport.Handle] = new Rect(0.0f, 100.0f, 200.0f, 100.0f);
        Report(first, 0.0f);
        Report(second, 0.0f);

        HostInput.Key("ArrowDown");
        var afterFirstKey = (first.VerticalOffset, second.VerticalOffset);
        HostInput.Pointer(PointerEventType.Up, item.Handle, 50.0f, 150.0f);
        HostInput.Key("ArrowDown");

        await Assert.That(afterFirstKey).IsEqualTo((40.0f, 0.0f));
        await Assert.That(second.VerticalOffset).IsEqualTo(40.0f);
        await Assert.That(first.VerticalOffset).IsEqualTo(40.0f);
    }

    [Test]
    public async Task FocusedTextKeepsHomeAndEnd()
    {
        TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var text = new Text("Words").Selectable(true);
        var box = new ScrollBox().Width(200.0f).Height(100.0f).Child(FlexBox.Column(text, new FlexBox().Height(380.0f)));
        Application.Mount(FlexBox.Column(box));
        Report(box, 0.0f);
        HostEvents.OnFocusChanged(text.Handle, true);

        var handled = HostInput.Key("End");
        HostInput.Key("PageDown");

        await Assert.That(handled).IsFalse();
        await Assert.That(box.VerticalOffset).IsEqualTo(60.0f);
    }

    [Test]
    public async Task TheOffsetPersistsUnderTheBoxsId()
    {
        var (abi, box, _) = Mount();
        Report(box, 120.0f);

        HostEvents.CapturePersistedUiState();
        Report(box, 0.0f);
        HostEvents.RestorePersistedUiState();

        await Assert.That(abi.PersistedScroll["list"]).IsEqualTo((0.0f, 120.0f));
        await Assert.That(box.VerticalOffset).IsEqualTo(120.0f);
    }

    [Test]
    public async Task ASelectionAreaAndItsBarriersReachTheHostAndTheAreaReportsTheSelection()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var plain = new Text("plain");
        var chosen = new Text("not selectable").Selectable(false);
        var link = new Text("in a link");
        var shielded = new Text("shielded");
        var area = new SelectionArea().Child(plain).Child(chosen)
            .Child(new FlexBox().Cursor(CursorStyle.Pointer).Child(link))
            .Child(new AntiSelectionArea().Child(shielded));
        var reported = new List<string>();
        area.SelectedTextChanged += e => reported.Add(e.Text);
        Application.Mount(area);

        HostEvents.OnCrossSelectionChanged(area.Handle, "plain");

        // Text is selectable unless it says otherwise; the host keeps a barrier's text out of the area's selection.
        await Assert.That(plain.Selectable).IsTrue();
        await Assert.That(chosen.Selectable).IsFalse();
        await Assert.That(link.Selectable).IsTrue();
        await Assert.That(shielded.Selectable).IsTrue();
        await Assert.That(abi.HasCall("ui_set_selection_area", (object)area.Handle, true)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_selection_area_barrier", (object)area.Children[3].Handle, true)).IsTrue();
        await Assert.That(area.SelectedText).IsEqualTo("plain");
        await Assert.That(string.Join(",", reported)).IsEqualTo("plain");
    }

    [Test]
    public async Task BarStylesSetThicknessCornersAndAMinimumThumbOnEitherAxis()
    {
        var (_, box, _) = Mount(ScrollBarVisibility.Visible);
        var vertical = box.VerticalScrollBar
            .TrackThickness(12.0f)
            .ThumbThickness(6.0f)
            .MinThumbLength(40.0f)
            .TrackCornerRadius(6.0f)
            .ThumbCornerRadius(-3.0f);
        box.HorizontalScrollEnabled().HorizontalScrollBarVisibility(ScrollBarVisibility.Visible);
        var horizontal = box.HorizontalScrollBar.TrackThickness(10.0f).ThumbThickness(4.0f);
        // A long list: the thumb would be 2 px at 100 / 5000 of the track, so the minimum applies.
        Report(box, 0.0f, 5000.0f);

        await Assert.That(vertical.Root.Width).IsEqualTo(new Length(12.0f));
        await Assert.That(Thumb(vertical).Width).IsEqualTo(new Length(6.0f));
        await Assert.That(Thumb(vertical).Height).IsEqualTo(new Length(40.0f));
        await Assert.That(vertical.Root.CornerRadius).IsEqualTo(new CornerRadius(6.0f));
        await Assert.That(Thumb(vertical).CornerRadius).IsEqualTo(new CornerRadius(0.0f));
        await Assert.That(horizontal.Root.Height).IsEqualTo(new Length(10.0f));
        await Assert.That(Thumb(horizontal).Height).IsEqualTo(new Length(4.0f));
    }

    [Test]
    public async Task BarColorsFollowTheThemeUntilSetAndRejectNonPositiveSizes()
    {
        var (_, box, _) = Mount(ScrollBarVisibility.Visible);
        var bar = box.VerticalScrollBar;
        Theme.UseCustom(Theme.DefaultLight);
        var light = (bar.Root.Background, Thumb(bar).Background);
        Theme.UseCustom(Theme.DefaultDark);
        var dark = (bar.Root.Background, Thumb(bar).Background);

        bar.TrackColor(0x11223344u).ThumbColor(0x55667788u);
        Theme.UseCustom(Theme.DefaultLight);

        await Assert.That(light).IsEqualTo((Theme.DefaultLight.Colors.ScrollbarTrack, Theme.DefaultLight.Colors.ScrollbarThumb));
        await Assert.That(dark).IsEqualTo((Theme.DefaultDark.Colors.ScrollbarTrack, Theme.DefaultDark.Colors.ScrollbarThumb));
        await Assert.That((bar.Root.Background, Thumb(bar).Background)).IsEqualTo(((Color)0x11223344u, (Color)0x55667788u));
        await Assert.That(Rejects(() => bar.TrackThickness = 0.0f)).IsTrue();
        await Assert.That(Rejects(() => bar.ThumbThickness = -1.0f)).IsTrue();
        await Assert.That(Rejects(() => bar.MinThumbLength = 0.0f)).IsTrue();
    }

    private static bool Rejects(System.Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return true;
        }
    }
}
