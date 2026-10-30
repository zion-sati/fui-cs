using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class AnimationTests
{
    private static readonly AnimationTiming Tenth = new(TimeSpan.FromMilliseconds(100));

    [Test]
    public async Task EasingsShapeProgress()
    {
        await Assert.That(Easings.Linear.Sample(1.5f)).IsEqualTo(1.0f);
        await Assert.That(Easings.CubicIn.Sample(0.5f)).IsEqualTo(0.125f);
        await Assert.That(Easings.CubicOut.Sample(0.5f)).IsEqualTo(0.875f);
        await Assert.That(Easings.CubicInOut.Sample(0.25f)).IsEqualTo(0.0625f);
        await Assert.That(Easings.CubicInOut.Sample(0.75f)).IsEqualTo(0.9375f);
        await Assert.That(Easings.QuadOut.Sample(0.5f)).IsEqualTo(0.75f);
        await Assert.That(new AnimationTiming(TimeSpan.FromMilliseconds(-5)).Duration).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task AnAnimationSamplesFromItsFirstFrameToItsEnd()
    {
        TestRuntime.Start(new LayoutAbi());
        var values = new List<float>();
        var animation = Animation.AnimateFloat(10.0f, 20.0f, Tenth, values.Add);
        var waiting = HostEvents.NeedsAnimationFrame();

        HostEvents.OnFrame(1000.0);
        HostEvents.OnFrame(1050.0);
        HostEvents.OnFrame(1100.0);

        await Assert.That(waiting).IsTrue();
        await Assert.That(string.Join(",", values)).IsEqualTo("10,15,20");
        await Assert.That(animation.IsRunning).IsFalse();
        await Assert.That(HostEvents.NeedsAnimationFrame()).IsFalse();
    }

    [Test]
    public async Task AnAnimationStartedAfterAFrameStartsAtOnceAndLongGapsCountAsAHundredMilliseconds()
    {
        TestRuntime.Start(new LayoutAbi());
        HostEvents.OnFrame(500.0);
        var values = new List<float>();
        Animation.AnimateFloat(0.0f, 1.0f, new AnimationTiming(TimeSpan.FromMilliseconds(400)), values.Add);

        HostEvents.OnFrame(5000.0);

        await Assert.That(string.Join(",", values)).IsEqualTo("0,0.25");
    }

    [Test]
    public async Task CancelStopsWhereItIsAndFinishJumpsToTheEnd()
    {
        TestRuntime.Start(new LayoutAbi());
        var cancelled = new List<float>();
        var finished = new List<Color>();
        var first = Animation.AnimateFloat(0.0f, 1.0f, Tenth, cancelled.Add);
        var second = Animation.AnimateColor(0x000000FFu, 0xFFFFFFFFu, Tenth, finished.Add);
        HostEvents.OnFrame(0.0);

        first.Cancel();
        second.Finish();
        HostEvents.OnFrame(50.0);

        await Assert.That(string.Join(",", cancelled)).IsEqualTo("0");
        await Assert.That(finished[^1]).IsEqualTo((Color)0xFFFFFFFFu);
        await Assert.That(HostEvents.NeedsAnimationFrame()).IsFalse();
    }

    [Test]
    public async Task AZeroDurationAnimationEndsOnItsFirstFrame()
    {
        TestRuntime.Start(new LayoutAbi());
        var values = new List<float>();
        var animation = Animation.AnimateFloat(0.0f, 5.0f, new AnimationTiming(TimeSpan.Zero), values.Add);

        HostEvents.OnFrame(10.0);

        await Assert.That(string.Join(",", values)).IsEqualTo("5");
        await Assert.That(animation.IsRunning).IsFalse();
    }

    [Test]
    public async Task ABuiltBoxAnimatesOpacityAndBackgroundThroughItsTransitions()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var box = new FlexBox().Transitions(new NodeTransitions { Opacity = Tenth, Background = Tenth }).Background(0x000000FFu);
        var unbuilt = new FlexBox().Transitions(new NodeTransitions { Opacity = Tenth }).Opacity(0.5f);
        box.Build();
        HostEvents.OnFrame(0.0);

        box.Opacity = 0.0f;
        box.Background = 0xFFFFFFFFu;
        var atStart = (box.Opacity, box.Background);
        HostEvents.OnFrame(50.0);
        var halfway = (box.Opacity, box.Background);
        HostEvents.OnFrame(100.0);

        await Assert.That(unbuilt.Opacity).IsEqualTo(0.5f);
        await Assert.That(atStart).IsEqualTo((1.0f, (Color)0x000000FFu));
        await Assert.That(halfway).IsEqualTo((0.5f, (Color)0x808080FFu));
        await Assert.That(box.Opacity).IsEqualTo(0.0f);
        await Assert.That(box.Background).IsEqualTo((Color)0xFFFFFFFFu);
        await Assert.That(abi.HasCall("ui_set_layer_effect", box.Handle, 0.5f, 0.0f, 0u)).IsTrue();
    }

    [Test]
    public async Task DisposingABoxCancelsItsTransitions()
    {
        TestRuntime.Start(new LayoutAbi());
        var box = new FlexBox().Transitions(new NodeTransitions { Opacity = Tenth });
        box.Build();
        HostEvents.OnFrame(0.0);
        box.Opacity = 0.0f;

        box.Dispose();

        await Assert.That(HostEvents.NeedsAnimationFrame()).IsFalse();
    }

    [Test]
    public async Task ScrollOffsetTransitionsAnimateUntilTheUserScrolls()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView().Transitions(new NodeTransitions { ScrollOffset = Tenth });
        var handle = scroll.Build();
        HostEvents.OnScroll(handle, 0.0f, 0.0f, 300.0f, 900.0f, 300.0f, 200.0f);
        HostEvents.OnFrame(0.0);

        scroll.SetScrollOffset(0.0f, 200.0f);
        HostEvents.OnFrame(50.0);
        var halfway = scroll.VerticalOffset;
        HostEvents.OnScroll(handle, 0.0f, 120.0f, 300.0f, 900.0f, 300.0f, 200.0f);
        HostEvents.OnScroll(handle, 0.0f, 130.0f, 300.0f, 900.0f, 300.0f, 200.0f);
        HostEvents.OnFrame(100.0);

        await Assert.That(halfway).IsEqualTo(100.0f);
        await Assert.That(abi.CountOf("ui_clear_momentum_scroll")).IsEqualTo(1);
        await Assert.That(scroll.VerticalOffset).IsEqualTo(130.0f);
        await Assert.That(HostEvents.NeedsAnimationFrame()).IsFalse();
    }

    [Test]
    public async Task ScrollToAnimatedOnAnUnbuiltViewSetsTheInitialOffset()
    {
        TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView();

        scroll.ScrollToAnimated(0.0f, 40.0f, Tenth);

        await Assert.That(scroll.VerticalOffset).IsEqualTo(40.0f);
        await Assert.That(HostEvents.NeedsAnimationFrame()).IsFalse();
    }
}
