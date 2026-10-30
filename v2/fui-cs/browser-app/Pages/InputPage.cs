using System;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// Pointer, wheel, keyboard, focus, long-press, touch-pan and drag-and-drop input from the real host. Each target
/// writes what it received into a status text the input spec reads back through the host's semantics.
/// </summary>
internal static class InputPage
{
    public static readonly Color Orange = 0xF97316FF;

    public static Node Build()
    {
        var clickLog = Status("click-log", "0/0");
        var clicks = 0;
        var doubleClicks = 0;
        var click = Target("click-target", LayoutPage.Blue)
            .OnPointerClick(_ => clickLog.Text = (++clicks).ToString() + "/" + doubleClicks.ToString())
            .OnPointerDoubleClick(_ => clickLog.Text = clicks.ToString() + "/" + (++doubleClicks).ToString());

        var hoverLog = Status("hover-log", "out");
        var hover = Target("hover-target", LayoutPage.Blue);
        hover.OnPointerEnter(_ =>
            {
                hover.Background = Orange;
                hoverLog.Text = "in";
            })
            .OnPointerLeave(_ =>
            {
                hover.Background = LayoutPage.Blue;
                hoverLog.Text = "out";
            });

        var disabledLog = Status("disabled-log", "idle");
        var disabled = Target("disabled-target", LayoutPage.Grey)
            .Enabled(false)
            .OnPointerClick(_ => disabledLog.Text = "clicked");

        var bubbleLog = Status("bubble-log", "");
        var bubbleChild = Target("bubble-child", LayoutPage.Red, 60.0f, 40.0f)
            .OnPointerClick(_ => bubbleLog.Text += "child>");
        var bubbleParent = new FlexBox()
            .NodeId("bubble-parent")
            .Width(140.0f)
            .Height(70.0f)
            .Padding(15.0f)
            .Background(LayoutPage.Grey)
            .Child(bubbleChild)
            .OnPointerClick(_ => bubbleLog.Text += "parent");

        var stopLog = Status("stop-log", "");
        var stopChild = Target("stop-child", LayoutPage.Red, 60.0f, 40.0f)
            .OnPointerClick(e =>
            {
                stopLog.Text += "child";
                e.Handled = true;
            });
        var stopParent = new FlexBox()
            .NodeId("stop-parent")
            .Width(140.0f)
            .Height(70.0f)
            .Padding(15.0f)
            .Background(LayoutPage.Grey)
            .Child(stopChild)
            .OnPointerClick(_ => stopLog.Text += ">parent");

        var wheelLog = Status("wheel-log", "none");
        var wheel = Target("wheel-target", LayoutPage.Green)
            .OnWheel(e => wheelLog.Text = e.DeltaY > 0.0f ? "down" : "up");

        var focusLog = Status("focus-log", "blurred");
        var keyLog = Status("key-log", "none");
        var keys = Target("key-target", LayoutPage.Blue)
            .Focusable(true)
            .OnFocusChanged(e => focusLog.Text = e.Focused ? "focused" : "blurred")
            .OnKeyDown(e => keyLog.Text = e.Key);

        var longPressLog = Status("long-press-log", "none");
        var longPress = Target("long-press-target", LayoutPage.Red)
            .OnLongPress(_ => longPressLog.Text = "pressed");

        var panLog = Status("pan-log", "none");
        var pannedRight = false;
        var pan = Target("pan-target", LayoutPage.Green)
            .OnPanGesture(e =>
            {
                pannedRight |= e.DeltaX > 0.0f;
                panLog.Text = e.Phase == GestureEventPhase.End ? (pannedRight ? "right-ended" : "ended") : "panning";
            });

        var pinchLog = Status("pinch-log", "none");
        var grew = false;
        var pinch = Target("pinch-target", LayoutPage.Red)
            .OnPinchGesture(e =>
            {
                grew |= e.Scale > 1.0f;
                pinchLog.Text = e.Phase == GestureEventPhase.End ? (grew ? "grew-ended" : "ended") : "pinching";
            });

        var dropLog = Status("drop-log", "none");
        var dragSource = Target("drag-source", LayoutPage.Blue)
            .DragAllowedEffects(DragDropEffects.Copy)
            .DragData(() => new DragDataObject().SetText("payload"));
        var dropTarget = Target("drop-target", LayoutPage.Grey);
        var dropped = false;
        // The target always gets DragLeave when a drag ends over it, after Drop when it accepted the data.
        dropTarget
            .AllowDrop()
            .OnDragEnter(_ => dropTarget.Background = Orange)
            .OnDragLeave(_ => dropTarget.Background = dropped ? LayoutPage.Green : LayoutPage.Grey)
            .OnDragOver(e => e.Proposal = new DropProposal(DragDropEffects.Copy))
            .OnDrop(e =>
            {
                dropped = true;
                dropLog.Text = e.Session.Data.GetText() ?? "no text";
            });

        return FlexBox.Row(
                FlexBox.Column(
                    Row(click, hover, disabled),
                    Row(bubbleParent, stopParent, wheel),
                    Row(keys, longPress, pan),
                    Row(dragSource, new FlexBox().Width(120.0f), dropTarget),
                    Row(pinch)),
                FlexBox.Column(
                        clickLog, hoverLog, disabledLog, bubbleLog, stopLog, wheelLog, focusLog, keyLog, longPressLog,
                        panLog, dropLog, pinchLog)
                    .Margin(20.0f, 0.0f, 0.0f, 0.0f))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Target(string id, Color color, float width = 120.0f, float height = 60.0f) =>
        new FlexBox().NodeId(id).Width(width).Height(height).Background(color).Cursor(CursorStyle.Pointer);

    private static FlexBox Row(params ReadOnlySpan<Node> children) =>
        FlexBox.Row(children).Margin(0.0f, 0.0f, 0.0f, 20.0f).AlignItems(AlignItems.Start);

    private static Text Status(string id, string initial) => new Text(initial).FontSize(14.0f).NodeId(id);
}
