using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class RangeControlTests
{
    private static ThemeColors Colors => Theme.Active.Value.Colors;

    private static bool Rejects(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    private static FlexBox Fill(ProgressBar bar) => (FlexBox)bar.Children[0];

    /// <summary>The default slider parts: track, fill and thumb.</summary>
    private static FlexBox Part(Slider slider, int index) => (FlexBox)slider.Children[0].Children[index];

    [Test]
    public async Task AProgressBarFillsInProportionToItsValue()
    {
        TestRuntime.Start(new LayoutAbi());
        var bar = new ProgressBar(25.0f);

        var quarter = Fill(bar).Width;
        bar.Value = 150.0f;

        await Assert.That(bar.Width).IsEqualTo((Length)220.0f);
        await Assert.That(bar.Height).IsEqualTo((Length)14.0f);
        await Assert.That(quarter).IsEqualTo((Length)54.5f);
        await Assert.That(bar.Value).IsEqualTo(100.0f);
        await Assert.That(Fill(bar).Width).IsEqualTo((Length)218.0f);
        await Assert.That(Fill(bar).Height).IsEqualTo((Length)12.0f);
        await Assert.That(bar.SemanticValueRange).IsEqualTo((SemanticValueRange?)new SemanticValueRange(100.0f, 0.0f, 100.0f));
        await Assert.That(bar.Background).IsEqualTo(Colors.ScrollbarTrack);
        await Assert.That(Fill(bar).Background).IsEqualTo(Colors.Accent);
        await Assert.That(bar.CornerRadius).IsEqualTo(new CornerRadius(7.0f));
        await Assert.That(Fill(bar).CornerRadius).IsEqualTo(new CornerRadius(6.0f));
    }

    [Test]
    public async Task AVerticalProgressBarFillsFromTheBottom()
    {
        TestRuntime.Start(new LayoutAbi());
        var bar = new ProgressBar(50.0f).Orientation(Orientation.Vertical).Length(102.0f).Thickness(10.0f);

        await Assert.That(bar.Width).IsEqualTo((Length)10.0f);
        await Assert.That(bar.Height).IsEqualTo((Length)102.0f);
        await Assert.That(bar.JustifyContent).IsEqualTo(JustifyContent.End);
        await Assert.That(Fill(bar).Height).IsEqualTo((Length)50.0f);
        await Assert.That(bar.SemanticOrientation).IsEqualTo(Orientation.Vertical);
    }

    [Test]
    public async Task ProgressBarRangeColorsAndCornersFollowTheirSettings()
    {
        TestRuntime.Start(new LayoutAbi());
        var red = new Color(0xFF0000FF);
        var bar = new ProgressBar(5.0f).Maximum(10.0f).Minimum(20.0f).TrackColor(red).FillColor(red).CornerRadius(3.0f);

        await Assert.That(bar.Maximum).IsEqualTo(20.0f);
        await Assert.That(bar.Value).IsEqualTo(20.0f);
        await Assert.That(bar.Background).IsEqualTo(red);
        await Assert.That(Fill(bar).Background).IsEqualTo(red);
        await Assert.That(bar.CornerRadius).IsEqualTo(new CornerRadius(3.0f));
        await Assert.That(Fill(bar).CornerRadius).IsEqualTo(new CornerRadius(2.0f));
        await Assert.That(Rejects(() => bar.Length = 0.0f)).IsTrue();
        await Assert.That(Rejects(() => bar.Thickness = -1.0f)).IsTrue();
    }

    [Test]
    public async Task ASliderIsAFocusableSliderSizedAroundItsTrack()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var slider = new Slider(30.0f);

        var handle = slider.Build();

        await Assert.That(abi.HasCall("ui_set_semantic_role", handle, SemanticRole.Slider)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_focusable", handle, true)).IsTrue();
        await Assert.That(slider.Width).IsEqualTo((Length)190.0f);
        await Assert.That(slider.Height).IsEqualTo((Length)30.0f);
        await Assert.That(slider.SemanticValueRange).IsEqualTo((SemanticValueRange?)new SemanticValueRange(30.0f, 0.0f, 100.0f));
        await Assert.That(Part(slider, 1).Width).IsEqualTo((Length)(162.0f * 0.3f));
        await Assert.That(Part(slider, 2).Background).IsEqualTo(Colors.Accent);
    }

    [Test]
    public async Task ValuesSnapToTheStepAndStayInRange()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var changes = new List<float>();
        var slider = new Slider().Step(5.0f).OnChanged(e => changes.Add(e.Value));

        slider.Value = 12.0f;
        slider.Value = 13.0f;
        slider.Value = 500.0f;
        slider.Minimum = 200.0f;

        await Assert.That(string.Join(",", changes)).IsEqualTo("10,15,100,200");
        await Assert.That(slider.Maximum).IsEqualTo(200.0f);
        await Assert.That(abi.Named("ui_request_semantic_announcement").Count).IsEqualTo(0);
        await Assert.That(Rejects(() => slider.Step = 0.0f)).IsTrue();
    }

    [Test]
    public async Task ArrowKeysHomeAndEndStepTheFocusedSlider()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var horizontal = new Slider(50.0f).Step(10.0f);
        var vertical = new Slider(50.0f).Step(10.0f).Orientation(Orientation.Vertical);
        Application.Mount(FlexBox.Column(horizontal, vertical));
        var values = new List<float>();

        HostEvents.OnFocusChanged(horizontal.Handle, true);
        foreach (var key in new[] { "ArrowRight", "ArrowLeft", "ArrowLeft", "ArrowUp", "End", "Home" })
        {
            HostInput.Key(key);
            values.Add(horizontal.Value);
        }
        HostEvents.OnFocusChanged(vertical.Handle, true);
        HostInput.Key("ArrowUp");
        HostInput.Key("ArrowRight");

        await Assert.That(string.Join(",", values)).IsEqualTo("60,50,40,40,100,0");
        await Assert.That(vertical.Value).IsEqualTo(60.0f);
        await Assert.That(abi.Named("ui_request_semantic_announcement").Count).IsEqualTo(6);
    }

    [Test]
    public async Task PressingAndDraggingSetTheValueUnderThePointer()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var slider = new Slider();
        var handle = slider.Build();
        abi.Bounds[handle] = new Rect(100.0f, 50.0f, 190.0f, 30.0f);
        var dragging = new List<bool>();

        // The track spans 162 pixels from the thumb's center at 100 + 5 + 9.
        HostInput.Pointer(PointerEventType.Down, handle, 100.0f + 14.0f + 81.0f, 60.0f);
        var pressed = slider.Value;
        dragging.Add(slider.IsDragging);
        HostInput.Pointer(PointerEventType.Move, handle, 100.0f + 14.0f + 40.5f, 60.0f);
        var dragged = slider.Value;
        HostInput.Pointer(PointerEventType.Move, handle, 900.0f, 60.0f);
        HostInput.Pointer(PointerEventType.Up, handle, 900.0f, 60.0f);
        dragging.Add(slider.IsDragging);

        await Assert.That(pressed).IsEqualTo(50.0f);
        await Assert.That(dragged).IsEqualTo(25.0f);
        await Assert.That(slider.Value).IsEqualTo(100.0f);
        await Assert.That(string.Join(",", dragging)).IsEqualTo("True,False");
    }

    [Test]
    public async Task AVerticalSliderIncreasesUpwards()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var slider = new Slider().Orientation(Orientation.Vertical);
        var handle = slider.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 30.0f, 190.0f);

        HostInput.Pointer(PointerEventType.Down, handle, 15.0f, 14.0f + 40.5f);
        HostInput.Pointer(PointerEventType.Up, handle, 15.0f, 14.0f + 40.5f);

        await Assert.That(slider.Value).IsEqualTo(75.0f);
        await Assert.That(slider.Width).IsEqualTo((Length)30.0f);
        await Assert.That(slider.Height).IsEqualTo((Length)190.0f);
    }

    [Test]
    public async Task ADisabledSliderFadesAndIgnoresInput()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var slider = new Slider(20.0f).Enabled(false);
        var handle = slider.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 190.0f, 30.0f);

        HostInput.Pointer(PointerEventType.Down, handle, 150.0f, 15.0f);
        HostInput.Pointer(PointerEventType.Up, handle, 150.0f, 15.0f);
        HostEvents.OnFocusChanged(handle, true);
        HostInput.Key("End");

        await Assert.That(slider.Value).IsEqualTo(20.0f);
        await Assert.That(slider.Opacity).IsEqualTo(0.6f);
        await Assert.That(abi.HasCall("ui_set_semantic_disabled", handle, true, true)).IsTrue();
    }

    [Test]
    public async Task SliderValuesPersist()
    {
        TestRuntime.Start(new LayoutAbi());
        var slider = new Slider(40.0f).NodeId("volume");
        Application.Mount(FlexBox.Column(slider));

        HostEvents.CapturePersistedUiState();
        slider.Value = 0.0f;
        HostEvents.RestorePersistedUiState();

        await Assert.That(slider.Value).IsEqualTo(40.0f);
    }

    [Test]
    public async Task SizingAndTemplatesReplaceTheParts()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var slider = new Slider().Sizing(new SliderSizing { ThumbSize = 24.0f, TrackThickness = 40.0f });
        var thick = Part(slider, 0).Height;
        var height = slider.Height;
        var states = new List<SliderVisualState>();
        slider.Template(new SliderTemplate(_ => new RecordingParts(states)));
        var handle = slider.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 200.0f, 30.0f);

        HostInput.Pointer(PointerEventType.Down, handle, 100.0f, 15.0f);

        await Assert.That(height).IsEqualTo((Length)36.0f);
        await Assert.That(thick).IsEqualTo((Length)24.0f);
        await Assert.That(slider.Children.Count).IsEqualTo(1);
        await Assert.That(states[^1].IsDragging).IsTrue();
        await Assert.That(Rejects(() => _ = new SliderSizing { ThumbSize = 0.0f })).IsTrue();
    }

    private sealed class RecordingParts(List<SliderVisualState> states) : SliderPresenter(new FlexBox(), 10.0f)
    {
        public override void Layout(SliderVisualState state, float length)
        {
        }

        public override void Apply(Theme theme, SliderVisualState state, SliderColors? colors) => states.Add(state);
    }
}
