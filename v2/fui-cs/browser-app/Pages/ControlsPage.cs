using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// Stage 3 controls in the real host. Each control reports its activations in a status line next to it so specs can
/// read the effect of real pointer and keyboard input.
/// </summary>
internal static class ControlsPage
{
    public static Node Build()
    {
        var saveCount = Status("save-count", "0");
        var save = new Button("Save").NodeId("save").OnClick(_ => saveCount.Text = Increment(saveCount.Text));
        var hoverLog = Status("hover-log", "out");
        save.OnHoverChanged(e => hoverLog.Text = e.Hovered ? "in" : "out");

        var offCount = Status("off-count", "0");
        var off = new Button("Off").NodeId("off").Enabled(false).OnClick(_ => offCount.Text = Increment(offCount.Text));

        var custom = new Button("Custom").NodeId("custom").Background(LayoutPage.Red);
        var colored = new Button("Colored")
            .NodeId("colored")
            .Colors(new ButtonColors { Background = LayoutPage.Green, BackgroundHover = LayoutPage.Blue });

        var agreeState = Status("agree-state", "False");
        var agree = new Checkbox("Agree").NodeId("agree").OnChanged(e => agreeState.Text = Describe(e.IsChecked));
        var mixedState = Status("mixed-state", "False");
        var mixed = new Checkbox("Three states").NodeId("mixed").IsThreeState().OnChanged(e => mixedState.Text = Describe(e.IsChecked));
        var locked = new Checkbox("Locked").NodeId("locked").Enabled(false);

        var sizeState = Status("size-state", "none");
        var size = new RadioGroup()
            .NodeId("size")
            .Option("s", "Small")
            .Option("m", "Medium")
            .Option("l", "Large")
            .OnChanged(e => sizeState.Text = e.SelectedValue ?? "none");
        size.Radios[1].Enabled(false);

        var wifiState = Status("wifi-state", "off");
        var wifi = new Switch("Wi-Fi").NodeId("wifi").OnChanged(e => wifiState.Text = e.IsChecked ? "on" : "off");

        Tag(agree, "agree-box");
        Tag(mixed, "mixed-box");
        Tag(wifi, "wifi-track");
        for (var index = 0; index < size.Radios.Count; ++index)
        {
            size.Radios[index].NodeId("size-" + size.Radios[index].Value);
            Tag(size.Radios[index], "size-" + size.Radios[index].Value + "-ring");
        }

        return FlexBox.Column(
                FlexBox.Row(save, Gap(), saveCount, Gap(), hoverLog).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(off, Gap(), offCount).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(custom, Gap(), colored).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(agree, Gap(), agreeState, Gap(), mixed, Gap(), mixedState, Gap(), locked).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(size, Gap(), sizeState).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(wifi, Gap(), wifiState).AlignItems(AlignItems.Center))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    /// <summary>Names a control's indicator so specs can sample its pixels.</summary>
    private static void Tag(PressableLabeledControl control, string id) => control.Children[0].NodeId(id);

    private static string Describe(bool? isChecked) => isChecked switch { true => "True", false => "False", null => "Mixed" };

    private static string Increment(string count) => (int.Parse(count) + 1).ToString();

    private static FlexBox Gap() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id, string text) => new Text(text).FontSize(14.0f).NodeId(id);
}
