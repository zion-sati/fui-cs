using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// The workbench route, which only the specs visit: a few controls with a readout, the style, cursor, dropdown, text
/// input and combo box showcases and the way back to the dashboard, on a page that scrolls.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(Build);

    private static Node Build()
    {
        var count = 0;
        var readout = new Text("Pressed 0 times").FontSize(16.0f).NodeId("workbench-readout");
        var press = new Button("Press me").NodeId("press").ToolTip("Counts each press")
            .OnClick(_ => readout.Text = "Pressed " + (++count).ToString() + (count == 1 ? " time" : " times"));
        var notify = new Switch("Notifications").NodeId("notify");
        var progress = new ProgressBar(40.0f).Length(240.0f);
        var level = new Slider(40.0f).Length(240.0f).NodeId("level").OnChanged(e => progress.Value = e.Value);
        var viewport = new Text(string.Empty).FontSize(14.0f).NodeId("viewport");
        Application.ViewportWidth.Subscribe(width => viewport.Text = "Viewport " + ((int)width).ToString() + " px wide");

        var controls = FlexBox.Column(
                FlexBox.Row(
                        new Button("Dashboard").NodeId("back").OnClick(_ => Navigation.NavigateTo(DemoRoutes.Home)),
                        new FlexBox().Width(16.0f),
                        new Text("Workbench").FontSize(28.0f).FontWeight(FontWeight.Bold).NodeId("workbench-title"))
                    .AlignItems(AlignItems.Center),
                new FlexBox().Height(24.0f),
                FlexBox.Row(press, new FlexBox().Width(16.0f), readout).AlignItems(AlignItems.Center),
                new FlexBox().Height(16.0f),
                notify,
                new FlexBox().Height(16.0f),
                level,
                new FlexBox().Height(8.0f),
                progress,
                new FlexBox().Height(16.0f),
                viewport,
                new FlexBox().Height(24.0f),
                AssetShowcase.Build())
            .AlignItems(AlignItems.Start);
        // The showcases wrap onto further lines when the window is too narrow for them side by side.
        var showcases = FlexBox.Row(
                controls.Margin(0.0f, 0.0f, 48.0f, 32.0f),
                new FlexBox().Margin(0.0f, 0.0f, 48.0f, 32.0f).Child(StyleShowcase.Build()),
                new FlexBox().Margin(0.0f, 0.0f, 0.0f, 32.0f).Child(CursorShowcase.Build()))
            .NodeId("workbench")
            .FlexWrap(FlexWrap.Wrap)
            .Padding(32.0f, 32.0f, 32.0f, 0.0f)
            .AlignItems(AlignItems.Start);
        return new ScrollBox()
            .NodeId("workbench-page")
            .FillWidth()
            .FillHeight()
            .HorizontalScrollEnabled(false)
            .Child(FlexBox.Column(
                    showcases,
                    FlexBox.Row(
                            new FlexBox().Margin(0.0f, 0.0f, 48.0f, 32.0f).Child(DropdownShowcase.Build()),
                            new FlexBox().Margin(0.0f, 0.0f, 0.0f, 32.0f).Child(TextInputShowcase.Build()))
                        .FlexWrap(FlexWrap.Wrap)
                        .AlignItems(AlignItems.Start)
                        .Padding(32.0f, 0.0f, 32.0f, 32.0f),
                    ComboBoxShowcase.Build().Padding(32.0f, 0.0f, 32.0f, 32.0f))
                .FillWidth());
    }
}
