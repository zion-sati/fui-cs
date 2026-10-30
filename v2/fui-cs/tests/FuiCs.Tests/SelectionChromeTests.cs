using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class SelectionChromeTests
{
    private const HostCapability Browser = HostCapability.ClipboardWrite | HostCapability.NewBrowsingContext | HostCapability.OpenExternalUri;

    private static (LayoutAbi Abi, Text Text) Mount()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f, HostCapabilities = Browser });
        var text = new Text("Hello wide world");
        Application.Mount(FlexBox.Column(new FlexBox().Height(200.0f), text));
        abi.Bounds[text.Handle] = new Rect(100.0f, 200.0f, 300.0f, 20.0f);
        // "wide" spans x 150 to 190 on the line at y 200, 20 px tall.
        abi.TextRangeRects[text.Handle] = [150.0f, 200.0f, 40.0f, 20.0f];
        return (abi, text);
    }

    private static void Touch(ulong handle, PointerEventType type, float x, float y) =>
        HostInput.Pointer(type, handle, x, y, pointerType: PointerType.Touch);

    private static string ToolbarLabels()
    {
        var labels = new List<string>();
        foreach (var item in TextSelectionToolbar.Items)
        {
            labels.Add(item.Label);
        }
        return string.Join("|", labels);
    }

    [Test]
    public async Task ATouchSelectionShowsHandlesAtItsEndsAndTheToolbarAboveIt()
    {
        var (_, text) = Mount();

        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var host = Application.ShellSelectionHandleHost!;
        var start = (FlexBox)host.Children[0];
        var end = (FlexBox)host.Children[1];
        var panel = TextSelectionToolbar.Panel!;

        await Assert.That(SelectionHandles.IsVisible).IsTrue();
        await Assert.That(start.Position).IsEqualTo(new Point(150.0f - 72.0f, 220.0f - 25.0f));
        await Assert.That(end.Position).IsEqualTo(new Point(190.0f - 18.0f, 220.0f - 25.0f));
        await Assert.That(ToolbarLabels()).IsEqualTo("Copy|Select all");
        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
        await Assert.That(panel.Position.Y < 200.0f).IsTrue();
        await Assert.That(TextSelectionToolbar.Items[0].Payload).IsEqualTo("wide");
    }

    [Test]
    public async Task AMouseSelectionShowsNoChromeUnlessHandlesAreAlwaysOn()
    {
        var (_, text) = Mount();

        HostInput.Pointer(PointerEventType.Down, text.Handle, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var mouse = (SelectionHandles.IsVisible, TextSelectionToolbar.IsVisible);
        SelectionHandles.Mode = SelectionHandleMode.Always;
        HostEvents.OnSelectionChanged(text.Handle, 6, 11);
        var always = SelectionHandles.IsVisible;
        SelectionHandles.Mode = SelectionHandleMode.Disabled;

        await Assert.That(mouse).IsEqualTo((false, false));
        await Assert.That(always).IsTrue();
        await Assert.That(SelectionHandles.IsVisible).IsFalse();
    }

    [Test]
    public async Task CopyCopiesTheSelectionClearsItAndClosesTheToolbar()
    {
        var (abi, text) = Mount();
        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);

        TextSelectionToolbar.Activate(0);

        await Assert.That(abi.HasCall("fui_copy_text", "wide")).IsTrue();
        await Assert.That(abi.CountOf("ui_clear_current_selection")).IsEqualTo(1);
        await Assert.That(TextSelectionToolbar.IsVisible).IsFalse();
    }

    [Test]
    public async Task APressOutsideTheToolbarClosesItAndGoesNoFurther()
    {
        var (abi, text) = Mount();
        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var panel = TextSelectionToolbar.Panel!;
        abi.Bounds[panel.Handle] = new Rect(panel.Position.X, panel.Position.Y, 200.0f, 36.0f);
        var presses = 0;
        text.PointerDown += _ => presses++;

        var consumed = Touch2(text.Handle, 500.0f, 500.0f);

        await Assert.That(consumed).IsTrue();
        await Assert.That(TextSelectionToolbar.IsVisible).IsFalse();
        await Assert.That(presses).IsEqualTo(0);
    }

    [Test]
    public async Task ALongPressOnTextSelectsAWordAndOnALinkShowsItsItems()
    {
        var (abi, text) = Mount();
        var link = new NavLink("/docs/").Child(new FlexBox().Width(40.0f).Height(20.0f));
        ((FlexBox)text.Parent!).Child(link);

        HostEvents.OnLongPressEvent(text.Handle, 160.0f, 210.0f, 1, (uint)PointerType.Touch, 0, 600);
        var selectedWord = abi.HasCall("ui_select_word_at", (object)text.Handle, 160.0f, 210.0f);
        HostEvents.OnLongPressEvent(link.Handle, 20.0f, 250.0f, 1, (uint)PointerType.Touch, 0, 600);

        await Assert.That(selectedWord).IsTrue();
        await Assert.That(ToolbarLabels()).IsEqualTo("New Tab|Open");
        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
    }

    [Test]
    public async Task ACrossSelectionsSelectAllTargetsTheLongPressedText()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f, HostCapabilities = Browser });
        var first = new Text("First line");
        var area = new SelectionArea().Child(first).Child(new Text("Second line"));
        Application.Mount(FlexBox.Column(new FlexBox().Height(200.0f), area));
        abi.CrossSelectionEndpointRects = [100.0f, 200.0f, 2.0f, 20.0f, 180.0f, 220.0f, 2.0f, 20.0f];

        // A long press starts with a touch press, which tells the chrome the selection is a touch one.
        Touch(first.Handle, PointerEventType.Down, 120.0f, 210.0f);
        HostEvents.OnLongPressEvent(first.Handle, 120.0f, 210.0f, 1, (uint)PointerType.Touch, 0, 600);
        HostEvents.OnCrossSelectionChanged(area.Handle, "First");
        var end = (FlexBox)Application.ShellSelectionHandleHost!.Children[1];

        await Assert.That(ToolbarLabels()).IsEqualTo("Copy|Select all");
        await Assert.That(TextSelectionToolbar.Items[1].TargetHandle).IsEqualTo(first.Handle);
        await Assert.That(TextSelectionToolbar.Items[0].Payload).IsEqualTo("First");
        await Assert.That(end.Position).IsEqualTo(new Point(182.0f - 18.0f, 240.0f - 25.0f));
    }

    [Test]
    public async Task AHandleDragEndsOnReleaseEvenWithoutTheCapture()
    {
        var (abi, text) = Mount();
        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var end = (FlexBox)Application.ShellSelectionHandleHost!.Children[1];
        abi.Bounds[end.Handle] = new Rect(172.0f, 195.0f, 90.0f, 90.0f);
        Touch(end.Handle, PointerEventType.Down, 200.0f, 230.0f);

        EventRouter.ReleasePointer(end.Handle);
        Touch(text.Handle, PointerEventType.Up, 240.0f, 210.0f);

        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
    }

    [Test]
    public async Task DraggingAHandleHidesTheToolbarUntilTheDragEnds()
    {
        var (abi, text) = Mount();
        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var end = (FlexBox)Application.ShellSelectionHandleHost!.Children[1];
        abi.Bounds[end.Handle] = new Rect(172.0f, 195.0f, 90.0f, 90.0f);

        Touch(end.Handle, PointerEventType.Down, 200.0f, 230.0f);
        var duringDrag = (TextSelectionToolbar.IsVisible, abi.HasCall("ui_begin_selection_endpoint_drag", (object)text.Handle, 1u));
        abi.TextRangeRects[text.Handle] = [150.0f, 200.0f, 80.0f, 20.0f];
        HostEvents.OnSelectionChanged(text.Handle, 6, 15);
        Touch(end.Handle, PointerEventType.Move, 240.0f, 230.0f);
        Touch(end.Handle, PointerEventType.Up, 240.0f, 230.0f);

        await Assert.That(duringDrag).IsEqualTo((false, true));
        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
        await Assert.That(end.Position).IsEqualTo(new Point(230.0f - 18.0f, 220.0f - 25.0f));
    }

    [Test]
    public async Task MoreThanThreeItemsOverflowIntoAPanelWithABackButton()
    {
        var (_, _) = Mount();
        var items = new List<MenuItem> { new("One"), new("Two"), MenuItem.Separator, new("Three"), new("Four"), new("Five") };

        TextSelectionToolbar.ShowItemsAt(items, 300.0f, 300.0f);
        var row = TextSelectionToolbar.Panel!.Children.Count;
        TextSelectionToolbar.Activate(-1);
        var overflowShown = (TextSelectionToolbar.IsOverflowVisible, TextSelectionToolbar.IsVisible);
        TextSelectionToolbar.Activate(-2);

        // Three actions and More, with dividers between them; the menu's section separator is left out.
        await Assert.That(ToolbarLabels()).IsEqualTo("One|Two|Three|Four|Five");
        await Assert.That(row).IsEqualTo(7);
        await Assert.That(overflowShown).IsEqualTo((true, false));
        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
        await Assert.That(TextSelectionToolbar.IsOverflowVisible).IsFalse();
    }

    [Test]
    public async Task AnOverflowPanelWithoutRoomBelowOpensRightAboveTheRow()
    {
        var (_, _) = Mount();
        var items = new List<MenuItem> { new("One"), new("Two"), new("Three"), new("Four"), new("Five") };
        var itemHeight = Theme.Active.Value.ContextMenu.Item.Height;

        TextSelectionToolbar.ShowItemsAt(items, 300.0f, 560.0f);
        var row = TextSelectionToolbar.Panel!.Position;
        TextSelectionToolbar.Activate(-1);
        var overflow = TextSelectionToolbar.OverflowPanel!;

        // Two overflow items and the back button, with dividers between them.
        await Assert.That(overflow.Height).IsEqualTo(Length.Pixels((itemHeight * 3.0f) + 2.0f));
        await Assert.That(overflow.Position.Y).IsEqualTo(row.Y - ((itemHeight * 3.0f) + 2.0f) - 4.0f);
    }

    [Test]
    public async Task ScrollingThePageMovesTheChromeButTheToolbarsOwnListDoesNotHideIt()
    {
        var (abi, text) = Mount();
        var link = new NavLink("/docs/").Child(new FlexBox().Width(40.0f).Height(20.0f));
        ((FlexBox)text.Parent!).Child(link);
        HostEvents.OnLongPressEvent(link.Handle, 20.0f, 250.0f, 1, (uint)PointerType.Touch, 0, 600);
        var overflowView = TextSelectionToolbar.OverflowPanel!.Children[0];

        HostEvents.OnScroll(overflowView.Handle, 0.0f, 0.0f, 168.0f, 36.0f, 168.0f, 36.0f);
        var afterOwnScroll = TextSelectionToolbar.IsVisible;
        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        abi.TextRangeRects[text.Handle] = [150.0f, 150.0f, 40.0f, 20.0f];
        HostEvents.OnScroll(text.Parent!.Handle, 0.0f, 50.0f, 800.0f, 1200.0f, 800.0f, 600.0f);
        var start = (FlexBox)Application.ShellSelectionHandleHost!.Children[0];

        await Assert.That(afterOwnScroll).IsTrue();
        await Assert.That(start.Position).IsEqualTo(new Point(150.0f - 72.0f, 170.0f - 25.0f));
    }

    [Test]
    public async Task OverlaysStackInAFixedOrderWhicheverIsCreatedFirst()
    {
        var (_, text) = Mount();
        var menu = Application.ShellContextMenu!;
        var focus = Application.ShellFocusHost!;

        Touch(text.Handle, PointerEventType.Down, 160.0f, 210.0f);
        HostEvents.OnSelectionChanged(text.Handle, 6, 10);
        var shell = menu.Parent!;
        var order = new List<string>();
        foreach (var child in shell.Children)
        {
            order.Add(ReferenceEquals(child, Application.ShellSelectionHandleHost) ? "handles"
                : ReferenceEquals(child, Application.ShellTextToolbarHost) ? "toolbar"
                : ReferenceEquals(child, focus) ? "focus"
                : ReferenceEquals(child, menu) ? "menu"
                : "root");
        }

        await Assert.That(string.Join("|", order)).IsEqualTo("root|handles|toolbar|focus|menu");
    }

    /// <summary>A touch press, returning whether the router consumed it.</summary>
    private static bool Touch2(ulong handle, float x, float y) =>
        HostInput.Pointer(PointerEventType.Down, handle, x, y, pointerType: PointerType.Touch);
}
