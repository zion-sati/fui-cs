using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ButtonTests
{
    private static ThemeColors Colors => Theme.Active.Value.Colors;

    /// <summary>Builds the button at 0,0 in an 80 x 30 box, so presses at 10,10 land on it.</summary>
    private static ulong Mount(LayoutAbi abi, Button button)
    {
        var handle = button.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 80.0f, 30.0f);
        return handle;
    }

    [Test]
    public async Task AButtonIsAFocusableButtonInTheThemesAccent()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var button = new Button("Save");

        var handle = button.Build();

        await Assert.That(abi.HasCall("ui_set_semantic_role", handle, SemanticRole.Button)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_label", handle, "Save")).IsTrue();
        await Assert.That(abi.HasCall("ui_set_focusable", handle, true)).IsTrue();
        await Assert.That(button.Cursor).IsEqualTo(CursorStyle.Pointer);
        await Assert.That(button.Background).IsEqualTo(Colors.Accent);
        await Assert.That(button.Padding).IsEqualTo(new Thickness(16.0f, 8.0f, 16.0f, 8.0f));
        await Assert.That(button.CornerRadius).IsEqualTo(new CornerRadius(8.0f));
        await Assert.That(button.Border).IsEqualTo(new Border(1.0f, Colors.Border));
        await Assert.That(abi.HasCall("ui_set_text_color", (object)button.Children[0].Children[0].Handle, Colors.TextOnAccent.Rgba)).IsTrue();
    }

    [Test]
    public async Task APrimaryPressAndReleaseClicksAndTheBackgroundFollowsTheState()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);
        var handle = Mount(abi, button);

        HostInput.Pointer(PointerEventType.Enter, handle, 10.0f, 10.0f);
        var hovered = button.Background;
        HostInput.Pointer(PointerEventType.Down, handle, 10.0f, 10.0f);
        var pressed = button.Background;
        HostInput.Pointer(PointerEventType.Up, handle, 10.0f, 10.0f);

        await Assert.That(hovered).IsEqualTo(Colors.AccentHovered);
        await Assert.That(pressed).IsEqualTo(Colors.AccentPressed);
        await Assert.That(button.Background).IsEqualTo(Colors.AccentHovered);
        await Assert.That(clicks).IsEqualTo(1);
    }

    [Test]
    public async Task DisablingAHoveredButtonReportsTheHoverEnding()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var button = new Button("Go");
        var handle = Mount(abi, button);
        var changes = new List<bool>();
        button.HoverChanged += e => changes.Add(e.Hovered);

        HostInput.Pointer(PointerEventType.Enter, handle, 10.0f, 10.0f);
        button.IsEnabled = false;

        await Assert.That(string.Join(",", changes)).IsEqualTo("True,False");
    }

    [Test]
    public async Task ACapturedPressLooksPressedOnlyOverTheButtonAndClicksOnlyThere()
    {
        // The host keeps routing a pressed pointer to the button after it leaves; Leave arrives after the release.
        var abi = TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);
        var handle = Mount(abi, button);

        HostInput.Pointer(PointerEventType.Down, handle, 10.0f, 10.0f);
        HostInput.Pointer(PointerEventType.Move, handle, 200.0f, 10.0f);
        var pressedOutside = button.IsPressed;
        HostInput.Pointer(PointerEventType.Move, handle, 20.0f, 10.0f);
        var pressedBackOver = button.IsPressed;
        HostInput.Pointer(PointerEventType.Move, handle, 200.0f, 10.0f);
        HostInput.Pointer(PointerEventType.Up, handle, 200.0f, 10.0f);
        HostInput.Pointer(PointerEventType.Leave, handle, 200.0f, 10.0f);

        await Assert.That(pressedOutside).IsFalse();
        await Assert.That(pressedBackOver).IsTrue();
        await Assert.That(clicks).IsEqualTo(0);
        await Assert.That(button.IsPressed).IsFalse();
    }

    [Test]
    public async Task OnlyPrimaryPressesClick()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);
        var handle = Mount(abi, button);

        HostInput.Pointer(PointerEventType.Down, handle, 10.0f, 10.0f, button: (int)PointerButton.Secondary);
        var pressed = button.IsPressed;
        HostInput.Pointer(PointerEventType.Up, handle, 10.0f, 10.0f, button: (int)PointerButton.Secondary);

        await Assert.That(pressed).IsFalse();
        await Assert.That(clicks).IsEqualTo(0);
    }

    [Test]
    public async Task EnterAndSpaceActivateTheFocusedButtonOnRelease()
    {
        TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);
        var handle = button.Build();
        HostEvents.OnFocusChanged(handle, true);

        HostInput.Key("Enter");
        var afterDown = clicks;
        HostInput.Key("Enter", KeyEventType.Up);
        HostInput.Key(" ");
        HostInput.Key(" ", KeyEventType.Up);
        HostInput.Key("Enter", modifiers: KeyModifier.Ctrl);
        HostInput.Key("Enter", KeyEventType.Up, KeyModifier.Ctrl);

        await Assert.That(afterDown).IsEqualTo(0);
        await Assert.That(clicks).IsEqualTo(2);
    }

    [Test]
    public async Task LosingFocusCancelsAKeyboardPress()
    {
        TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);
        var handle = button.Build();
        HostEvents.OnFocusChanged(handle, true);

        HostInput.Key("Enter");
        HostEvents.OnFocusChanged(handle, false);
        HostInput.Key("Enter", KeyEventType.Up);

        await Assert.That(clicks).IsEqualTo(0);
        await Assert.That(button.IsPressed).IsFalse();
    }

    [Test]
    public async Task ADisabledButtonFadesIgnoresInputAndUsesTheMutedColor()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var muted = new Color(0x777777FF);
        var button = new Button("Off").Colors(new ButtonColors { TextMuted = muted }).OnClick(_ => clicks++);
        var handle = Mount(abi, button);

        button.Enabled(false);
        HostInput.Click(handle, 10.0f, 10.0f);
        var label = (TextCore)button.Children[0].Children[0];

        await Assert.That(clicks).IsEqualTo(0);
        await Assert.That(button.Opacity).IsEqualTo(0.38f);
        await Assert.That(button.Cursor).IsEqualTo(CursorStyle.Default);
        await Assert.That(label.TextColor).IsEqualTo((Color?)muted);
        button.Enabled(true);
        await Assert.That(button.Opacity).IsEqualTo(1.0f);
    }

    [Test]
    public async Task OverridesAndColorsLayerOverTheTheme()
    {
        TestRuntime.Start(new LayoutAbi());
        var red = new Color(0xFF0000FF);
        var blue = new Color(0x0000FFFF);
        var overridden = new Button("A").Background(red);
        var colored = new Button("B").Colors(new ButtonColors { Background = blue });
        var startHover = Colors.AccentHovered;
        var oHandle = overridden.Build();
        var cHandle = colored.Build();

        HostInput.Pointer(PointerEventType.Enter, oHandle);
        HostInput.Pointer(PointerEventType.Enter, cHandle);
        var overriddenHover = overridden.Background;
        var coloredHover = colored.Background;
        Theme.UseCustom(Theme.DefaultLight);

        await Assert.That(overriddenHover).IsEqualTo(startHover);
        await Assert.That(coloredHover).IsEqualTo(blue);
        await Assert.That(overridden.Background).IsEqualTo(Theme.DefaultLight.Colors.AccentHovered);
        HostInput.Pointer(PointerEventType.Leave, oHandle);
        await Assert.That(overridden.Background).IsEqualTo(red);
    }

    [Test]
    public async Task ATemplateBuildsTheContentAndSeesTheState()
    {
        TestRuntime.Start(new LayoutAbi());
        var states = new List<ButtonVisualState>();
        var icon = new FlexBox().Width(12.0f).Height(12.0f);
        var button = new Button("Icon").Template(new ButtonTemplate(() =>
        {
            var label = new TextCore();
            return new RecordingPresenter(FlexBox.Row(icon, label), label, states);
        }));
        var handle = button.Build();

        HostInput.Pointer(PointerEventType.Enter, handle);

        await Assert.That(ReferenceEquals(icon.Parent?.Parent, button)).IsTrue();
        await Assert.That(((TextCore)button.Children[0].Children[1]).Text).IsEqualTo("Icon");
        await Assert.That(button.Children.Count).IsEqualTo(1);
        await Assert.That(states[^1].IsHovered).IsTrue();
    }

    [Test]
    public async Task PerformClickRaisesClick()
    {
        TestRuntime.Start(new LayoutAbi());
        var clicks = 0;
        var button = new Button("Go").OnClick(_ => clicks++);

        button.PerformClick();

        await Assert.That(clicks).IsEqualTo(1);
    }

    private sealed class RecordingPresenter(FlexBox root, TextCore label, List<ButtonVisualState> states) : ButtonPresenter(root, label)
    {
        public override void Apply(Theme theme, ButtonVisualState state, ButtonColors? colors) => states.Add(state);
    }
}
