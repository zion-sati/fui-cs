using System;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// The pointer cursor over each kind of target: controls and links show a pointer, selectable text an I-beam and a
/// scroll bar's thumb a grab hand, and a resize grip names its own; disabled and hidden controls leave the default.
/// </summary>
internal static class CursorShowcase
{
    private const float MinimumPanelWidth = 60.0f;
    private const float MaximumPanelWidth = 200.0f;

    public static Node Build()
    {
        var link = new NavLink(DemoRoutes.Home).NodeId("cursor-link")
            .Child(new Text("Dashboard link").FontSize(14.0f).TextColor(0x2563EBFF).Selectable(false));

        var rows = FlexBox.Column();
        for (var index = 1; index <= 12; ++index)
        {
            var row = new FlexBox().NodeId("cursor-line-" + index.ToString(CultureInfo.InvariantCulture)).Height(24.0f).Padding(8.0f, 4.0f)
                .Child(new Text("Line " + index.ToString(CultureInfo.InvariantCulture)).FontSize(13.0f).Selectable(false));
            rows.Child(index == 1 ? row.ToolTip("The first line") : row);
        }

        return FlexBox.Column(
                new Text("Cursors").FontSize(20.0f).FontWeight(FontWeight.Bold),
                Gap(12.0f),
                FlexBox.Row(new Button("Enabled").NodeId("cursor-button"), Gap(12.0f), new Button("Disabled").NodeId("cursor-disabled").Enabled(false)),
                Gap(12.0f),
                new Checkbox("Checkbox").NodeId("cursor-checkbox"),
                Gap(8.0f),
                new RadioGroup().NodeId("cursor-radio").Option("one", "Radio"),
                Gap(8.0f),
                link,
                Gap(8.0f),
                new Text("Selectable text").FontSize(14.0f).NodeId("cursor-text"),
                new Text("Plain text").FontSize(14.0f).NodeId("cursor-plain").Selectable(false),
                Gap(8.0f),
                FlexBox.Row(new Button("Hidden").NodeId("cursor-hidden").Visibility(Visibility.Hidden), Gap(12.0f), ResizablePanel()),
                Gap(12.0f),
                new ScrollBox().NodeId("cursor-scroll").Width(200.0f).Height(96.0f).PersistScroll(false).Child(rows))
            .NodeId("cursor-showcase")
            .AlignItems(AlignItems.Start);
    }

    /// <summary>A panel whose width follows a drag on its grip, which shows the east-west resize cursor.</summary>
    private static FlexBox ResizablePanel()
    {
        var panel = new FlexBox().NodeId("cursor-panel").Width(100.0f).Height(40.0f).CornerRadius(6.0f).Background(0xCBD5E1FF);
        var grip = new FlexBox().NodeId("cursor-grip").Width(10.0f).Height(40.0f).CornerRadius(3.0f).Background(0x64748BFF)
            .Cursor(CursorStyle.ResizeEW);
        var startWidth = 0.0f;
        var drag = new DragGesture(grip).Threshold(0.0f);
        drag.Started += _ => startWidth = panel.Width.Value;
        drag.Delta += e => panel.Width = Math.Clamp(startWidth + e.TotalDeltaX, MinimumPanelWidth, MaximumPanelWidth);
        grip.PointerDown += e => drag.HandlePointerDown(e.SceneX, e.SceneY, e.Modifiers);
        grip.PointerMove += e => drag.HandlePointerMove(e.SceneX, e.SceneY, e.Modifiers);
        grip.PointerUp += e => drag.HandlePointerUp(e.SceneX, e.SceneY, e.Modifiers);
        grip.PointerCancel += _ => drag.Cancel();
        return FlexBox.Row(panel, Gap(4.0f), grip).AlignItems(AlignItems.Center);
    }

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
