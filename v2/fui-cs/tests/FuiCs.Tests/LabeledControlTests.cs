using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class LabeledControlTests
{
    private static ThemeColors Colors => Theme.Active.Value.Colors;

    /// <summary>Builds <paramref name="node"/> at 0,0 in a 120 x 30 box, so presses at 10,10 land on it.</summary>
    private static ulong Mount(LayoutAbi abi, Node node)
    {
        var handle = node.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 120.0f, 30.0f);
        return handle;
    }

    private static bool Rejects<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }

    private static FlexBox Indicator(PressableLabeledControl control) => (FlexBox)control.Children[0];

    private static TextCore LabelOf(PressableLabeledControl control) => (TextCore)control.Children[2].Children[0];

    [Test]
    public async Task ACheckboxIsAFocusableUncheckedCheckboxWithItsLabel()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var checkbox = new Checkbox("Remember me");

        var handle = checkbox.Build();

        await Assert.That(abi.HasCall("ui_set_semantic_role", handle, SemanticRole.Checkbox)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_checked", handle, SemanticCheckedState.False)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_focusable", handle, true)).IsTrue();
        await Assert.That(LabelOf(checkbox).Text).IsEqualTo("Remember me");
        await Assert.That(LabelOf(checkbox).TextColor).IsEqualTo((Color?)Colors.TextPrimary);
        await Assert.That(Indicator(checkbox).Width).IsEqualTo((Length)20.0f);
        await Assert.That(Indicator(checkbox).Background).IsEqualTo(Colors.Surface);
        await Assert.That(checkbox.IsChecked == false).IsTrue();
    }

    [Test]
    public async Task AClickTogglesTheCheckboxThenRaisesClick()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var log = new List<string>();
        var checkbox = new Checkbox("A")
            .OnChanged(e => log.Add("changed " + e.IsChecked))
            .OnClick(_ => log.Add("click"));
        var handle = Mount(abi, checkbox);

        HostInput.Click(handle, 10.0f, 10.0f);

        await Assert.That(string.Join("|", log)).IsEqualTo("changed True|click");
        await Assert.That(checkbox.IsChecked == true).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_checked", handle, SemanticCheckedState.True)).IsTrue();
        await Assert.That(abi.Named("ui_request_semantic_announcement").Count).IsEqualTo(1);
        await Assert.That(Indicator(checkbox).Background).IsEqualTo(Colors.Accent);
    }

    [Test]
    public async Task AThreeStateCheckboxCyclesThroughIndeterminate()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var checkbox = new Checkbox("A").IsThreeState();
        var handle = Mount(abi, checkbox);
        var states = new List<bool?>();
        checkbox.Changed += e => states.Add(e.IsChecked);

        for (var click = 0; click < 3; ++click)
        {
            HostInput.Click(handle, 10.0f, 10.0f);
        }

        await Assert.That(string.Join(",", states)).IsEqualTo("True,,False");
        await Assert.That(abi.HasCall("ui_set_semantic_checked", handle, SemanticCheckedState.Mixed)).IsTrue();
    }

    [Test]
    public async Task OnlyAThreeStateCheckboxKeepsIndeterminate()
    {
        TestRuntime.Start(new LayoutAbi());
        var twoState = new Checkbox("A").IsChecked(true);
        twoState.IsChecked = null;
        var threeState = new Checkbox("B").IsThreeState().IsChecked(null);
        var states = new List<bool?>();
        threeState.Changed += e => states.Add(e.IsChecked);
        threeState.IsThreeState = false;

        await Assert.That(twoState.IsChecked is false).IsTrue();
        await Assert.That(threeState.IsChecked is false).IsTrue();
        await Assert.That(string.Join(",", states)).IsEqualTo("False");
    }

    [Test]
    public async Task SpaceTogglesOnReleaseAndOtherKeysDoNot()
    {
        TestRuntime.Start(new LayoutAbi());
        var toggle = new Switch("Wi-Fi");
        var handle = toggle.Build();
        HostEvents.OnFocusChanged(handle, true);

        HostInput.Key(" ");
        var afterDown = toggle.IsChecked;
        var pressed = toggle.IsPressed;
        HostInput.Key(" ", KeyEventType.Up);
        HostInput.Key("Enter");
        HostInput.Key("Enter", KeyEventType.Up);
        HostInput.Key(" ", modifiers: KeyModifier.Shift);
        HostInput.Key(" ", KeyEventType.Up, KeyModifier.Shift);

        await Assert.That(afterDown).IsFalse();
        await Assert.That(pressed).IsTrue();
        await Assert.That(toggle.IsChecked == true).IsTrue();
        await Assert.That(toggle.IsPressed).IsFalse();
    }

    [Test]
    public async Task ASwitchMovesItsThumbAndFillsTheTrackWhenOn()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var toggle = new Switch("Wi-Fi");
        toggle.Build();
        var track = Indicator(toggle);
        var thumb = (FlexBox)track.Children[0];

        toggle.IsChecked = true;

        await Assert.That(track.Background).IsEqualTo(Colors.Accent);
        await Assert.That(abi.HasCall("ui_set_position", (object)thumb.Handle, 21.0f, 2.0f)).IsTrue();
        await Assert.That(abi.Named("ui_request_semantic_announcement").Count).IsEqualTo(0);
    }

    [Test]
    public async Task ADisabledControlFadesIgnoresInputAndMutesItsLabel()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var muted = new Color(0x777777FF);
        var checkbox = new Checkbox("Off").Colors(new LabeledControlColors { TextMuted = muted });
        var handle = Mount(abi, checkbox);

        checkbox.Enabled(false);
        HostInput.Click(handle, 10.0f, 10.0f);
        HostEvents.OnFocusChanged(handle, true);
        HostInput.Key(" ");
        HostInput.Key(" ", KeyEventType.Up);

        await Assert.That(checkbox.IsChecked == false).IsTrue();
        await Assert.That(checkbox.Opacity).IsEqualTo(0.6f);
        await Assert.That(checkbox.Cursor).IsEqualTo(CursorStyle.Default);
        await Assert.That(LabelOf(checkbox).TextColor).IsEqualTo((Color?)muted);
    }

    [Test]
    public async Task AControlDisabledBeforeItIsBuiltIsSemanticallyDisabled()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var checkbox = new Checkbox("Locked").Enabled(false);

        var handle = checkbox.Build();

        await Assert.That(abi.HasCall("ui_set_semantic_disabled", handle, true, true)).IsTrue();
    }

    [Test]
    public async Task APressReleasedOutsideDoesNotToggle()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var checkbox = new Checkbox("A");
        var handle = Mount(abi, checkbox);

        HostInput.Pointer(PointerEventType.Down, handle, 10.0f, 10.0f);
        HostInput.Pointer(PointerEventType.Move, handle, 300.0f, 10.0f);
        var pressedOutside = checkbox.IsPressed;
        HostInput.Pointer(PointerEventType.Up, handle, 300.0f, 10.0f);

        await Assert.That(pressedOutside).IsFalse();
        await Assert.That(checkbox.IsChecked == false).IsTrue();
    }

    [Test]
    public async Task ARadioGroupKeepsOneSelection()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var changes = new List<string>();
        var group = new RadioGroup()
            .Option("s", "Small")
            .Option("m", "Medium")
            .Option("l", "Large")
            .OnChanged(e => changes.Add(e.SelectedIndex + ":" + e.SelectedValue));
        group.Build();
        var small = group.Radios[0];
        var large = group.Radios[2];
        abi.Bounds[small.Handle] = new Rect(0.0f, 0.0f, 120.0f, 30.0f);
        abi.Bounds[large.Handle] = new Rect(0.0f, 60.0f, 120.0f, 30.0f);

        HostInput.Click(small.Handle, 10.0f, 10.0f);
        HostInput.Click(large.Handle, 10.0f, 70.0f);
        HostInput.Click(large.Handle, 10.0f, 70.0f);

        await Assert.That(string.Join("|", changes)).IsEqualTo("0:s|2:l");
        await Assert.That(small.IsChecked == false).IsTrue();
        await Assert.That(large.IsChecked == true).IsTrue();
        await Assert.That(group.SelectedValue).IsEqualTo("l");
        await Assert.That(abi.HasCall("ui_set_semantic_role", group.Handle, SemanticRole.RadioGroup)).IsTrue();
    }

    [Test]
    public async Task ArrowKeysHomeAndEndMoveTheSelectionPastDisabledRadios()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var group = new RadioGroup().Option("a").Option("b").Option("c").SelectedIndex(0);
        group.Radios[1].Enabled(false);
        group.Build();
        HostEvents.OnFocusChanged(group.Radios[0].Handle, true);
        var selections = new List<int>();

        // The host moves focus to the radio the group asked for.
        foreach (var key in new[] { "ArrowDown", "ArrowDown", "ArrowUp", "End", "Home" })
        {
            HostInput.Key(key);
            selections.Add(group.SelectedIndex);
            HostEvents.OnFocusChanged(group.SelectedRadio!.Handle, true);
        }

        await Assert.That(string.Join(",", selections)).IsEqualTo("2,0,2,2,0");
        await Assert.That(abi.HasCall("ui_request_focus", (object)group.Radios[2].Handle)).IsTrue();
    }

    [Test]
    public async Task SelectedIndexAndIsCheckedKeepTheGroupConsistent()
    {
        TestRuntime.Start(new LayoutAbi());
        var group = new RadioGroup().Option("a").Option("b");

        group.SelectedIndex = 1;
        var afterIndex = group.Radios[1].IsChecked;
        group.Radios[0].IsChecked = true;
        var afterChecked = (group.SelectedIndex, group.Radios[1].IsChecked);
        group.Radios[0].IsChecked = false;

        await Assert.That(afterIndex).IsTrue();
        await Assert.That(afterChecked).IsEqualTo((0, false));
        await Assert.That(group.SelectedIndex).IsEqualTo(-1);
        await Assert.That(group.SelectedValue).IsNull();
        await Assert.That(Rejects<ArgumentOutOfRangeException>(() => group.SelectedIndex = 2)).IsTrue();
        await Assert.That(Rejects<InvalidOperationException>(() => new RadioGroup().AddRadio(group.Radios[0]))).IsTrue();
    }

    [Test]
    public async Task ADisabledRadioIsNotSelectedFromCode()
    {
        TestRuntime.Start(new LayoutAbi());
        var group = new RadioGroup().Option("a").Option("b").SelectedIndex(0);
        group.Radios[1].IsEnabled = false;

        group.SelectedIndex = 1;
        group.Radios[1].IsChecked = true;

        await Assert.That(group.SelectedIndex).IsEqualTo(0);
        await Assert.That(group.Radios[1].IsChecked).IsFalse();
    }

    [Test]
    public async Task AddingCheckedRadiosKeepsTheFirstSelection()
    {
        TestRuntime.Start(new LayoutAbi());

        var group = new RadioGroup().Radio(new RadioButton("a").IsChecked(true), new RadioButton("b").IsChecked(true));

        await Assert.That(group.SelectedIndex).IsEqualTo(0);
        await Assert.That(group.Radios[1].IsChecked == false).IsTrue();
    }

    [Test]
    public async Task CheckboxesSwitchesAndGroupsPersistTheirValues()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var checkbox = new Checkbox("a").NodeId("agree").IsThreeState().IsChecked(null);
        var toggle = new Switch("b").NodeId("wifi").IsChecked(true);
        var group = new RadioGroup().NodeId("size").Option("s").Option("m").SelectedIndex(1);
        Application.Mount(FlexBox.Column(checkbox, toggle, group));

        HostEvents.CapturePersistedUiState();
        checkbox.IsChecked = false;
        toggle.IsChecked = false;
        group.SelectedIndex = 0;
        HostEvents.RestorePersistedUiState();

        await Assert.That(abi.PersistedText[("agree", "checkbox-checked-state")]).IsEqualTo((1u, "3"));
        await Assert.That(checkbox.IsChecked).IsNull();
        await Assert.That(toggle.IsChecked == true).IsTrue();
        await Assert.That(group.SelectedValue).IsEqualTo("m");
    }

    [Test]
    public async Task SizingResizesTheIndicatorAndLabel()
    {
        TestRuntime.Start(new LayoutAbi());
        var radio = new RadioButton("a");
        var toggle = new Switch("b");

        radio.Sizing(new LabeledControlSizing { IndicatorSize = 30.0f, LabelFontSize = 20.0f });
        toggle.Sizing = new LabeledControlSizing { IndicatorSize = 52.0f };

        await Assert.That(Indicator(radio).Width).IsEqualTo((Length)30.0f);
        await Assert.That(LabelOf(radio).FontSize).IsEqualTo(20.0f);
        await Assert.That(Indicator(toggle).Width).IsEqualTo((Length)88.0f);
        await Assert.That(radio.Children.Count).IsEqualTo(3);
        await Assert.That(Rejects<ArgumentOutOfRangeException>(() => _ = new LabeledControlSizing { IndicatorSize = 0.0f })).IsTrue();
    }

    [Test]
    public async Task ATemplateBuildsTheIndicatorAndSeesTheState()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var states = new List<IndicatorVisualState>();
        var checkbox = new Checkbox("A").Template(new IndicatorTemplate(_ => new RecordingIndicator(states)));
        var handle = Mount(abi, checkbox);

        HostInput.Pointer(PointerEventType.Enter, handle, 10.0f, 10.0f);
        HostInput.Click(handle, 10.0f, 10.0f);

        await Assert.That(states[^1].IsChecked == true).IsTrue();
        await Assert.That(states[^1].IsHovered).IsTrue();
    }

    private sealed class RecordingIndicator(List<IndicatorVisualState> states) : IndicatorPresenter(new FlexBox())
    {
        public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors) => states.Add(state);
    }
}
