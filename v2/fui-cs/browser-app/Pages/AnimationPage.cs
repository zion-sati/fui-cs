using System;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>Opacity and background transitions, a counted animation, animated scrolling and the device pixel ratio.</summary>
internal static class AnimationPage
{
    private static readonly AnimationTiming Quick = new(TimeSpan.FromMilliseconds(250), Easings.CubicInOut);

    public static Node Build()
    {
        var faded = false;
        var fadeBox = new FlexBox()
            .NodeId("fade-box")
            .Width(80.0f)
            .Height(40.0f)
            .Background(LayoutPage.Blue)
            .Transitions(new NodeTransitions { Opacity = Quick, Background = Quick });
        var fadeToggle = Button("fade-toggle");
        fadeToggle.OnPointerClick(_ =>
        {
            faded = !faded;
            fadeBox.Opacity = faded ? 0.2f : 1.0f;
            fadeBox.Background = faded ? LayoutPage.Green : LayoutPage.Blue;
        });

        var samples = new Text("samples 0").FontSize(14.0f).NodeId("sample-log");
        var countStart = Button("count-start");
        countStart.OnPointerClick(_ =>
        {
            var count = 0;
            Animation.AnimateFloat(0.0f, 1.0f, new AnimationTiming(TimeSpan.FromMilliseconds(300)), value =>
            {
                count++;
                samples.Text = value >= 1.0f ? "done " + count.ToString() : "samples " + count.ToString();
            });
        });

        var scrollLog = new Text("scroll 0").FontSize(14.0f).NodeId("scroll-log");
        var scrollSteps = 0;
        var scrollBox = new ScrollBox()
            .NodeId("animated-scroll")
            .Width(160.0f)
            .Height(80.0f)
            .Child(new FlexBox().Width(140.0f).Height(600.0f).Background(LayoutPage.Grey));
        scrollBox.ScrollChanged += e =>
        {
            scrollSteps++;
            scrollLog.Text = "scroll " + e.VerticalOffset.ToString() + " steps " + scrollSteps.ToString();
        };
        var scrollDown = Button("scroll-down");
        scrollDown.OnPointerClick(_ =>
        {
            scrollSteps = 0;
            scrollBox.ScrollToAnimated(0.0f, 300.0f, Quick);
        });

        var dpr = new Text("dpr ?").FontSize(14.0f).NodeId("dpr-log");
        FrameScheduler.OnLoaded(loaded => dpr.Text = "dpr " + Application.DevicePixelRatio.ToString());

        return FlexBox.Column(fadeToggle, fadeBox, countStart, samples, scrollDown, scrollBox, scrollLog, dpr)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Button(string id) =>
        new FlexBox().NodeId(id).Width(80.0f).Height(24.0f).Background(LayoutPage.Grey).Cursor(CursorStyle.Pointer);
}
