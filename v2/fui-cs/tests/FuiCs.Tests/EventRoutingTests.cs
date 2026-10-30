using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class EventRoutingTests
{
    [Test]
    public async Task AClickRaisesPointerEventsAndAClickInNodeCoordinates()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var node = new TestNode();
        var log = new List<string>();
        node.PointerDown += e => log.Add("down " + e.X + "," + e.Y);
        node.PointerClick += e => log.Add("click " + e.X + "," + e.Y + " scene " + e.SceneX + "," + e.SceneY + " count " + e.ClickCount);
        var handle = node.Build();
        abi.Bounds[handle] = new Rect(10.0f, 20.0f, 100.0f, 50.0f);

        HostInput.Click(handle, 15.0f, 30.0f);

        await Assert.That(string.Join("|", log)).IsEqualTo("down 5,10|click 5,10 scene 15,30 count 1");
        await Assert.That(abi.Arguments("ui_set_interactive", 1)).IsEqualTo("True");
    }

    [Test]
    public async Task UnhandledEventsBubbleToAncestorsAndHandledOnesStop()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var parent = new TestNode();
        var child = new TestNode();
        parent.AddChild(child);
        var parentClicks = 0;
        parent.PointerClick += _ => parentClicks++;
        var childHandle = child.Build();
        abi.Bounds[parent.Build()] = new Rect(0.0f, 0.0f, 100.0f, 50.0f);
        abi.Bounds[childHandle] = new Rect(10.0f, 10.0f, 20.0f, 20.0f);

        HostInput.Click(childHandle, 15.0f, 15.0f);
        var afterUnhandled = parentClicks;
        child.PointerDown += e => e.Handled = true;
        HostInput.Click(childHandle, 15.0f, 15.0f);

        await Assert.That(afterUnhandled).IsEqualTo(1);
        await Assert.That(parentClicks).IsEqualTo(1);
    }

    [Test]
    public async Task ABubbledReleaseOutsideTheAncestorDoesNotClickIt()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var parent = new TestNode();
        var child = new TestNode();
        parent.AddChild(child);
        var parentClicks = 0;
        parent.PointerClick += _ => parentClicks++;
        var childHandle = child.Build();
        abi.Bounds[parent.Build()] = new Rect(0.0f, 0.0f, 100.0f, 50.0f);
        abi.Bounds[childHandle] = new Rect(10.0f, 10.0f, 20.0f, 20.0f);

        HostInput.Pointer(PointerEventType.Down, childHandle, 15.0f, 15.0f);
        HostInput.Pointer(PointerEventType.Up, childHandle, 60.0f, 40.0f);
        var insideParent = parentClicks;
        HostInput.Pointer(PointerEventType.Down, childHandle, 15.0f, 15.0f);
        HostInput.Pointer(PointerEventType.Up, childHandle, 300.0f, 40.0f);

        await Assert.That(insideParent).IsEqualTo(1);
        await Assert.That(parentClicks).IsEqualTo(1);
    }

    [Test]
    public async Task DisabledNodesDoNotReceivePointerEvents()
    {
        TestRuntime.Start();
        var parent = new TestNode();
        var child = new TestNode { IsEnabled = false };
        parent.AddChild(child);
        var childEvents = 0;
        var parentEvents = 0;
        child.PointerDown += _ => childEvents++;
        parent.PointerDown += _ => parentEvents++;
        parent.Build();

        HostInput.Pointer(PointerEventType.Down, child.Handle);

        await Assert.That(childEvents).IsEqualTo(0);
        await Assert.That(parentEvents).IsEqualTo(1);
    }

    [Test]
    public async Task EventsForDisposedNodesAreDropped()
    {
        TestRuntime.Start();
        var node = new TestNode();
        var events = 0;
        node.PointerDown += _ => events++;
        var handle = node.Build();
        node.Dispose();

        var handled = HostInput.Pointer(PointerEventType.Down, handle);

        await Assert.That(handled).IsFalse();
        await Assert.That(events).IsEqualTo(0);
    }

    [Test]
    public async Task DetachedNodesStopBubblingToTheirFormerParent()
    {
        TestRuntime.Start();
        var parent = new TestNode();
        var child = new TestNode();
        parent.AddChild(child);
        var parentEvents = 0;
        var childEvents = 0;
        parent.PointerDown += _ => parentEvents++;
        child.PointerDown += _ => childEvents++;
        parent.Build();

        parent.RemoveChild(child);
        HostInput.Pointer(PointerEventType.Down, child.Handle);

        await Assert.That(childEvents).IsEqualTo(1);
        await Assert.That(parentEvents).IsEqualTo(0);
    }

    [Test]
    public async Task ACapturedPointerDeliversMovesAndReleaseToTheCapturingNode()
    {
        var abi = TestRuntime.Start();
        var capturing = new TestNode();
        var other = new TestNode();
        var log = new List<string>();
        capturing.PointerMove += _ => log.Add("capturing move");
        capturing.PointerUp += _ => log.Add("capturing up");
        other.PointerMove += _ => log.Add("other move");
        capturing.Build();
        var otherHandle = other.Build();

        capturing.CapturePointer();
        HostInput.Pointer(PointerEventType.Move, otherHandle);
        HostInput.Pointer(PointerEventType.Up, otherHandle);
        HostInput.Pointer(PointerEventType.Move, otherHandle);

        await Assert.That(string.Join("|", log)).IsEqualTo("capturing move|capturing up|other move");
        await Assert.That(abi.Arguments("fui_set_pointer_capture", 0)).IsEqualTo("1");
    }

    [Test]
    public async Task AReleaseOutsideTheNodeDoesNotClick()
    {
        // The host keeps routing a pressed pointer to the node after it leaves, so the release arrives here.
        var abi = TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var node = new TestNode();
        node.PointerClick += _ => clicks++;
        var handle = node.Build();
        abi.Bounds[handle] = new Rect(10.0f, 10.0f, 40.0f, 20.0f);

        HostInput.Pointer(PointerEventType.Down, handle, 20.0f, 20.0f);
        HostInput.Pointer(PointerEventType.Up, handle, 80.0f, 20.0f);
        var afterOutside = clicks;
        HostInput.Pointer(PointerEventType.Down, handle, 20.0f, 20.0f);
        HostInput.Pointer(PointerEventType.Up, handle, 49.0f, 29.0f);

        await Assert.That(afterOutside).IsEqualTo(0);
        await Assert.That(clicks).IsEqualTo(1);
    }

    [Test]
    public async Task ADoubleClickRaisesClickThenDoubleClick()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var node = new TestNode();
        var log = new List<string>();
        node.PointerClick += e => log.Add("click " + e.ClickCount);
        node.PointerDoubleClick += e => log.Add("double " + e.ClickCount);
        node.PointerTripleClick += _ => log.Add("triple");
        var handle = node.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 40.0f, 20.0f);

        HostInput.Click(handle, 10.0f, 10.0f, clickCount: 2);

        await Assert.That(string.Join("|", log)).IsEqualTo("click 2|double 2");
    }

    [Test]
    public async Task HoveringAppliesTheHoveredNodesCursorExceptOnCoarsePointers()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var node = new TestNode { Cursor = CursorStyle.Pointer };
        var handle = node.Build();

        HostInput.Pointer(PointerEventType.Enter, handle);
        HostInput.Pointer(PointerEventType.Leave, handle);

        await Assert.That(abi.Arguments("fui_set_cursor", 0)).IsEqualTo("Pointer,Default");

        abi.CoarsePointer = true;
        abi.Calls.Clear();
        HostInput.Pointer(PointerEventType.Enter, handle);

        await Assert.That(abi.CountOf("fui_set_cursor")).IsEqualTo(0);
    }

    [Test]
    public async Task SelectableTextShowsAnIBeamUnlessAnAncestorSetsItsOwnCursor()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var linkText = new Text("Read the docs");
        var link = new NavLink("/docs/").Child(linkText);
        var plainText = new Text("Plain words");
        var probe = new FlexBox().Child(new Text("Probe words"));
        Application.Mount(FlexBox.Column(link, plainText, probe));
        var probeText = probe.Children[0];

        HostInput.Pointer(PointerEventType.Enter, link.Handle);
        HostInput.Pointer(PointerEventType.Enter, linkText.Handle);
        HostInput.Pointer(PointerEventType.Leave, linkText.Handle);
        HostInput.Pointer(PointerEventType.Leave, link.Handle);
        HostInput.Pointer(PointerEventType.Enter, plainText.Handle);
        HostInput.Pointer(PointerEventType.Leave, plainText.Handle);
        HostInput.Pointer(PointerEventType.Enter, probeText.Handle);
        probe.Cursor = CursorStyle.Move;

        // The link's text keeps the link's hand; a box that sets a cursor while its text is hovered takes over too.
        await Assert.That(abi.Arguments("fui_set_cursor", 0)).IsEqualTo("Pointer,Default,Text,Default,Text,Move");
    }

    [Test]
    public async Task KeyboardReportsTheFocusedNode()
    {
        TestRuntime.Start();
        var first = new TestNode();
        var second = new TestNode();
        first.Build();
        second.Build();

        var none = Keyboard.FocusedNode;
        HostEvents.OnFocusChanged(first.Handle, true);
        var firstFocused = first.IsFocused && ReferenceEquals(Keyboard.FocusedNode, first);
        HostEvents.OnFocusChanged(first.Handle, false);
        HostEvents.OnFocusChanged(second.Handle, true);

        await Assert.That(none).IsNull();
        await Assert.That(firstFocused).IsTrue();
        await Assert.That(first.IsFocused).IsFalse();
        await Assert.That(ReferenceEquals(Keyboard.FocusedNode, second)).IsTrue();
    }

    [Test]
    public async Task KeyEventsGoToKeyFiltersThenTheFocusedNode()
    {
        TestRuntime.Start();
        var node = new TestNode();
        var keys = new List<string>();
        node.KeyDown += e => keys.Add(e.Key);
        var handle = node.Build();

        var unfocused = HostInput.Key("a");
        HostEvents.OnFocusChanged(handle, true);
        var focused = HostInput.Key("b");
        var filter = EventRouter.PushKeyFilter((_, key, _) => key == "c");
        var filtered = HostInput.Key("c");
        filter.Dispose();
        var afterFilter = HostInput.Key("c");
        var keyUp = HostInput.Key("d", KeyEventType.Up);

        await Assert.That(unfocused).IsFalse();
        await Assert.That(focused).IsTrue();
        await Assert.That(filtered).IsTrue();
        await Assert.That(afterFilter).IsTrue();
        await Assert.That(keyUp).IsFalse();
        await Assert.That(string.Join(",", keys)).IsEqualTo("b,c");
        await Assert.That(ReferenceEquals(EventRouter.FocusedNode, node)).IsTrue();
    }

    [Test]
    public async Task WheelEventsBubbleToTheFirstHandlerThatHandlesThem()
    {
        TestRuntime.Start();
        var outer = new TestNode();
        var inner = new TestNode();
        outer.AddChild(inner);
        var log = new List<string>();
        inner.Wheel += _ => log.Add("inner");
        outer.Wheel += e =>
        {
            log.Add("outer " + e.DeltaY);
            e.Handled = true;
        };
        outer.Build();

        var handled = HostEvents.OnWheelEvent(inner.Handle, 0.0f, 0.0f, 0.0f, 3.0f, (uint)WheelDeltaMode.Line, 0);

        await Assert.That(handled).IsTrue();
        await Assert.That(string.Join("|", log)).IsEqualTo("inner|outer 3");
    }

    [Test]
    public async Task TheGestureOwnerIsTheNearestNodeThatClaimsPanOrPinch()
    {
        TestRuntime.Start();
        var owner = new TestNode();
        var child = new TestNode();
        owner.AddChild(child);
        var pans = new List<float>();
        void OnPan(GestureEventArgs e) => pans.Add(e.DeltaX);
        owner.PanGesture += OnPan;
        owner.Build();

        var resolved = HostEvents.ResolveGestureOwner(child.Handle);
        var intent = (GestureIntent)HostEvents.GetGestureIntent(owner.Handle);
        HostEvents.OnGestureEvent(child.Handle, (uint)GestureEventPhase.Update, (uint)GestureEventKind.Pan, 0.0f, 0.0f, 7.0f, 0.0f, 1.0f, 1);
        owner.PanGesture -= OnPan;

        await Assert.That(resolved).IsEqualTo(owner.Handle);
        await Assert.That(intent).IsEqualTo(GestureIntent.Pan);
        await Assert.That(string.Join(",", pans)).IsEqualTo("7");
        await Assert.That(HostEvents.ResolveGestureOwner(child.Handle)).IsEqualTo(0UL);
    }

    [Test]
    public async Task ALongPressRecognizerSetsTheTimingAndReceivesTheGesture()
    {
        TestRuntime.Start();
        var owner = new TestNode();
        var child = new TestNode();
        owner.AddChild(child);
        var recognized = new List<int>();
        owner.LongPressRecognizer = LongPressGesture.Create()
            .MinimumDuration(300)
            .MovementTolerance(4.0f)
            .OnRecognized(e => recognized.Add(e.DurationMs));
        owner.Build();

        var resolved = HostEvents.ResolveLongPressOwner(child.Handle);
        var duration = HostEvents.GetLongPressMinimumDurationMs(owner.Handle);
        var tolerance = HostEvents.GetLongPressMovementTolerance(owner.Handle);
        HostEvents.OnLongPressEvent(child.Handle, 0.0f, 0.0f, 1, (uint)PointerType.Touch, 0, 320);

        await Assert.That(resolved).IsEqualTo(owner.Handle);
        await Assert.That(duration).IsEqualTo(300);
        await Assert.That(tolerance).IsEqualTo(4.0f);
        await Assert.That(string.Join(",", recognized)).IsEqualTo("320");
        await Assert.That(HostEvents.GetLongPressMinimumDurationMs(child.Handle)).IsEqualTo(LongPressDefaults.MinimumDurationMs);
    }

    [Test]
    public async Task KeyboardFocusIndicatorsHideOnPressAndReturnOnNavigation()
    {
        TestRuntime.Start();
        var node = new TestNode();
        var handle = node.Build();

        HostInput.Pointer(PointerEventType.Down, handle);
        var afterPress = FocusVisibility.KeyboardFocusVisible.Value;
        HostInput.Key("ArrowLeft");
        var afterCaretKey = FocusVisibility.KeyboardFocusVisible.Value;
        HostInput.Key("Tab");
        var afterTab = FocusVisibility.KeyboardFocusVisible.Value;
        HostInput.Key("Escape");

        await Assert.That(afterPress).IsFalse();
        await Assert.That(afterCaretKey).IsFalse();
        await Assert.That(afterTab).IsTrue();
        await Assert.That(FocusVisibility.KeyboardFocusVisible.Value).IsFalse();
    }
}
