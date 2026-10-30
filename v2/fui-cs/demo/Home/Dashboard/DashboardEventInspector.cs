using System;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>
/// A child panel inside a parent that log the pointer, wheel, click, gesture and long-press events they see. Toggles
/// make either side mark events handled, disable the child, and attach pan, pinch and long-press recognizers.
/// </summary>
internal sealed class DashboardEventInspector : FlexBox
{
    private readonly DemoButton childHandledButton = new DemoButton("Child handled: off").Width(172.0f);
    private readonly DemoButton parentHandledButton = new DemoButton("Parent handled: off").Width(182.0f);
    private readonly DemoButton childDisabledButton = new DemoButton("Child disabled: off").Width(184.0f);
    private readonly DemoButton gesturePanButton = new DemoButton("Pan gesture: off").Width(156.0f);
    private readonly DemoButton gesturePinchButton = new DemoButton("Pinch gesture: off").Width(168.0f);
    private readonly DemoButton gestureLongPressButton = new DemoButton("Long press: off").Width(166.0f);
    private readonly DemoButton longPressConfigButton = new DemoButton("Long config: default").Width(206.0f);
    private readonly DemoButton parentGesturePanButton = new DemoButton("Parent pan: off").Width(166.0f);
    private readonly DemoButton parentGesturePinchButton = new DemoButton("Parent pinch: off").Width(178.0f);
    private readonly DemoButton parentGestureLongPressButton = new DemoButton("Parent long: off").Width(174.0f);
    private readonly Text statusText = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);
    private readonly Text logText = new DashboardText("", 13.0f, DemoTextRecipe.Hint);
    private readonly FlexBox childPanel = new DemoSurface(DemoSurfaceRecipe.CalloutInset).Width(DemoTokens.FullSize, Unit.Percent).Height(70.0f).Padding(12.0f, 12.0f, 12.0f, 12.0f);
    private readonly Text childLabel = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);
    private readonly string[] log =
    [
        "Wheel, click, drag, or touch the child panel",
        "Toggle child handled to stop bubbling",
        "Pointer logs include local coords and pointer metadata",
        "Right-click logs here, then opens context menu unless handled",
        "",
        "",
    ];

    private bool childHandles;
    private bool parentHandles;
    private bool childDisabled;
    private bool childPan;
    private bool childPinch;
    private bool childLongPress;
    private bool fastLongPress;
    private bool parentPan;
    private bool parentPinch;
    private bool parentLongPress;
    private int eventSerial;

    public DashboardEventInspector()
    {
        this.Width(DemoTokens.FullSize, Unit.Percent).SemanticLabel("Wheel event inspector");
        var toggles = new FlexBox().FlexDirection(FlexDirection.Row).FlexWrap(FlexWrap.Wrap).Width(DemoTokens.FullSize, Unit.Percent);
        foreach (var button in new[]
        {
            childHandledButton, parentHandledButton, childDisabledButton, gesturePanButton, gesturePinchButton,
            gestureLongPressButton, longPressConfigButton, parentGesturePanButton, parentGesturePinchButton, parentGestureLongPressButton,
        })
        {
            toggles.Child(button.Margin(0.0f, 6.0f, 6.0f, 0.0f));
        }
        // At least their usual height, and taller when narrow lines wrap.
        statusText.MinHeight(94.0f);
        logText.MinHeight(148.0f);
        childHandledButton.Click += _ => Toggle(ref childHandles);
        parentHandledButton.Click += _ => Toggle(ref parentHandles);
        childDisabledButton.Click += _ =>
        {
            childDisabled = !childDisabled;
            childPanel.IsEnabled = !childDisabled;
            Refresh();
        };
        gesturePanButton.Click += _ => Toggle(ref childPan);
        gesturePinchButton.Click += _ => Toggle(ref childPinch);
        gestureLongPressButton.Click += _ => Toggle(ref childLongPress);
        longPressConfigButton.Click += _ => Toggle(ref fastLongPress);
        parentGesturePanButton.Click += _ => Toggle(ref parentPan);
        parentGesturePinchButton.Click += _ => Toggle(ref parentPinch);
        parentGestureLongPressButton.Click += _ => Toggle(ref parentLongPress);

        childPanel.Child(childLabel);
        childPanel.Wheel += e => LogWheel("child", e, childHandles);
        childPanel.PointerDown += e => LogPointer("child", e, childHandles);
        childPanel.PointerMove += e => LogPointer("child", e, childHandles);
        childPanel.PointerUp += e => LogPointer("child", e, childHandles);
        childPanel.PointerCancel += e => LogPointer("child", e, childHandles);
        childPanel.PointerClick += e => PushLog("child raw click count=" + e.ClickCount.ToString(CultureInfo.InvariantCulture));
        childPanel.PointerDoubleClick += _ => PushLog("child exact raw double-click");
        childPanel.PointerTripleClick += _ => PushLog("child exact raw triple-click");
        Wheel += e => LogWheel("parent", e, parentHandles);
        PointerDown += e => LogPointer("parent", e, parentHandles);
        PointerMove += e => LogPointer("parent", e, parentHandles);
        PointerUp += e => LogPointer("parent", e, parentHandles);
        PointerCancel += e => LogPointer("parent", e, parentHandles);

        this.Child(FlexBox.Column(
                new DashboardText("Input event inspector", 20.0f, DemoTextRecipe.SectionTitle),
                new FlexBox().Height(8.0f),
                new DashboardText("A child panel bubbles pointer, wheel, and gesture events to its parent until a handler marks e.handled.", 14.0f, DemoTextRecipe.Supporting),
                new FlexBox().Height(12.0f),
                toggles,
                new FlexBox().Height(8.0f),
                statusText,
                childPanel,
                new FlexBox().Height(10.0f),
                logText)
            .Width(DemoTokens.FullSize, Unit.Percent));
        Refresh();
    }

    private static string PointerEventName(PointerEventType eventType) => eventType switch
    {
        PointerEventType.Down => "down",
        PointerEventType.Move => "move",
        PointerEventType.Up => "up",
        PointerEventType.Enter => "enter",
        PointerEventType.Leave => "leave",
        PointerEventType.Cancel => "cancel",
        _ => "pointer",
    };

    private static string PhaseName(GestureEventPhase phase) => phase switch
    {
        GestureEventPhase.Begin => "begin",
        GestureEventPhase.Update => "update",
        GestureEventPhase.End => "end",
        GestureEventPhase.Cancel => "cancel",
        _ => "gesture",
    };

    private static string KindName(GestureEventKind kind) => kind switch
    {
        GestureEventKind.Pan => "pan",
        GestureEventKind.Pinch => "pinch",
        _ => "gesture",
    };

    private static string PointerTypeName(PointerType pointerType) => pointerType switch
    {
        PointerType.Mouse => "mouse",
        PointerType.Touch => "touch",
        PointerType.Pen => "pen",
        _ => "unknown",
    };

    private static string Hundredths(float value) => ((int)(value * 100.0f)).ToString(CultureInfo.InvariantCulture);

    private static string Whole(float value) => ((int)value).ToString(CultureInfo.InvariantCulture);

    private static string RecognizerName(bool pan, bool pinch, bool longPress) => (pan, pinch, longPress) switch
    {
        (true, true, true) => "pan + pinch + long",
        (true, true, false) => "pan + pinch",
        (true, false, true) => "pan + long",
        (false, true, true) => "pinch + long",
        (true, false, false) => "pan",
        (false, true, false) => "pinch",
        (false, false, true) => "long press",
        _ => "none",
    };

    private void Toggle(ref bool flag)
    {
        flag = !flag;
        Refresh();
    }

    private void LogWheel(string side, WheelEventArgs e, bool handles)
    {
        PushLog(side + " wheel x=" + Whole(e.X) + " y=" + Whole(e.Y) + " dx=" + Whole(e.DeltaX) + " dy=" + Whole(e.DeltaY) + (handles ? " handled" : " unhandled"));
        if (handles)
        {
            e.Handled = true;
        }
    }

    private void LogPointer(string side, PointerEventArgs e, bool handles)
    {
        var clickCount = e.EventType == PointerEventType.Down && e.ClickCount > 0 ? " count=" + e.ClickCount.ToString(CultureInfo.InvariantCulture) : "";
        PushLog(side + " " + PointerEventName(e.EventType)
            + " x=" + Whole(e.X) + " y=" + Whole(e.Y)
            + " id=" + e.PointerId.ToString(CultureInfo.InvariantCulture)
            + " " + PointerTypeName(e.PointerType)
            + " b=" + ((int)e.Button).ToString(CultureInfo.InvariantCulture)
            + " bs=" + ((uint)e.Buttons).ToString(CultureInfo.InvariantCulture)
            + " p=" + Hundredths(e.Pressure)
            + clickCount
            + (handles ? " handled" : " unhandled"));
        if (handles)
        {
            e.Handled = true;
        }
    }

    private void LogGesture(string side, GestureEventArgs e, bool handles)
    {
        PushLog(side + " gesture " + PhaseName(e.Phase) + " " + KindName(e.Kind)
            + " x=" + Whole(e.X) + " y=" + Whole(e.Y) + " dx=" + Whole(e.DeltaX) + " dy=" + Whole(e.DeltaY)
            + " s=" + Hundredths(e.Scale)
            + (handles ? " handled" : " unhandled"));
        if (handles)
        {
            e.Handled = true;
        }
    }

    private void LogLongPress(string side, LongPressEventArgs e, bool handles)
    {
        PushLog(side + " long press x=" + Whole(e.X) + " y=" + Whole(e.Y)
            + " id=" + e.PointerId.ToString(CultureInfo.InvariantCulture)
            + " " + PointerTypeName(e.PointerType)
            + " ms=" + e.DurationMs.ToString(CultureInfo.InvariantCulture)
            + (handles ? " handled" : " unhandled"));
        if (handles)
        {
            e.Handled = true;
        }
    }

    private void PushLog(string line)
    {
        eventSerial += 1;
        Array.Copy(log, 0, log, 1, log.Length - 1);
        log[0] = "#" + eventSerial.ToString(CultureInfo.InvariantCulture) + " " + line;
        Refresh();
    }

    private void OnChildGesture(GestureEventArgs e) => LogGesture("child", e, childHandles);

    private void OnParentGesture(GestureEventArgs e) => LogGesture("parent", e, parentHandles);

    private LongPressGesture CreateLongPress(bool child)
    {
        var gesture = LongPressGesture.Create();
        if (fastLongPress)
        {
            gesture.MinimumDuration(250).MovementTolerance(18.0f);
        }
        return gesture.OnRecognized(e => LogLongPress(child ? "child" : "parent", e, child ? childHandles : parentHandles));
    }

    private void Refresh()
    {
        // Each refresh detaches the recognizers before attaching the ones turned on, so none is attached twice.
        childPanel.PanGesture -= OnChildGesture;
        childPanel.PinchGesture -= OnChildGesture;
        PanGesture -= OnParentGesture;
        PinchGesture -= OnParentGesture;
        if (childPan)
        {
            childPanel.PanGesture += OnChildGesture;
        }
        if (childPinch)
        {
            childPanel.PinchGesture += OnChildGesture;
        }
        childPanel.LongPressRecognizer = childLongPress ? CreateLongPress(true) : null;
        if (parentPan)
        {
            PanGesture += OnParentGesture;
        }
        if (parentPinch)
        {
            PinchGesture += OnParentGesture;
        }
        LongPressRecognizer = parentLongPress ? CreateLongPress(false) : null;

        childHandledButton.Label = childHandles ? "Child handled: on" : "Child handled: off";
        parentHandledButton.Label = parentHandles ? "Parent handled: on" : "Parent handled: off";
        childDisabledButton.Label = childDisabled ? "Child disabled: on" : "Child disabled: off";
        gesturePanButton.Label = childPan ? "Child pan: on" : "Child pan: off";
        gesturePinchButton.Label = childPinch ? "Child pinch: on" : "Child pinch: off";
        gestureLongPressButton.Label = childLongPress ? "Child long: on" : "Child long: off";
        longPressConfigButton.Label = fastLongPress ? "Long config: fast" : "Long config: default";
        parentGesturePanButton.Label = parentPan ? "Parent pan: on" : "Parent pan: off";
        parentGesturePinchButton.Label = parentPinch ? "Parent pinch: on" : "Parent pinch: off";
        parentGestureLongPressButton.Label = parentLongPress ? "Parent long: on" : "Parent long: off";
        var childState = childDisabled ? "disabled" : childHandles ? "handles events" : "does not handle";
        statusText.Text = "Child: " + childState
            + "\nParent: " + (parentHandles ? "parent handles" : "parent observes")
            + "\nChild rec: " + RecognizerName(childPan, childPinch, childLongPress)
            + "\nParent rec: " + RecognizerName(parentPan, parentPinch, parentLongPress)
            + "\nLong press: " + (fastLongPress ? "250ms / 18px" : "500ms / 10px");
        childLabel.Text = childDisabled ? "Disabled child: events start at parent" : "Child wheel, pointer, and gesture zone";
        logText.Text = string.Join("\n", log);
    }
}
