using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>Sliders and progress bars in the real host; each slider reports its value in a status line.</summary>
internal static class RangePage
{
    public static Node Build()
    {
        var progress = new ProgressBar(20.0f).NodeId("progress");
        var volumeState = Status("volume-state", "20");
        var volume = new Slider(20.0f).NodeId("volume").OnChanged(e =>
        {
            volumeState.Text = Format(e.Value);
            progress.Value = e.Value;
        });

        var stepsState = Status("steps-state", "0");
        var steps = new Slider().NodeId("steps").Maximum(10.0f).Step(2.0f).Length(120.0f).OnChanged(e => stepsState.Text = Format(e.Value));

        var levelState = Status("level-state", "0");
        var level = new Slider().NodeId("level").Orientation(Orientation.Vertical).Length(120.0f).OnChanged(e => levelState.Text = Format(e.Value));
        var levelBar = new ProgressBar(60.0f).NodeId("level-bar").Orientation(Orientation.Vertical).Length(120.0f);

        var lockedState = Status("locked-state", "50");
        var locked = new Slider(50.0f).NodeId("locked").Enabled(false).OnChanged(e => lockedState.Text = Format(e.Value));

        return FlexBox.Column(
                FlexBox.Row(volume, Gap(), volumeState).AlignItems(AlignItems.Center),
                Gap(),
                progress,
                Gap(),
                FlexBox.Row(steps, Gap(), stepsState).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(level, Gap(), levelState, Gap(), levelBar).AlignItems(AlignItems.Center),
                Gap(),
                FlexBox.Row(locked, Gap(), lockedState).AlignItems(AlignItems.Center))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static string Format(float value) => value.ToString(CultureInfo.InvariantCulture);

    private static FlexBox Gap() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id, string text) => new Text(text).FontSize(14.0f).NodeId(id);
}
