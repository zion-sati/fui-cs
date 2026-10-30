using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class FocusAdornerTests
{
    private static (LayoutAbi Abi, Button Button) Mount()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var button = new Button("Focus me");
        Application.Mount(FlexBox.Column(button));
        abi.Bounds[button.Handle] = new Rect(40.0f, 30.0f, 100.0f, 36.0f);
        return (abi, button);
    }

    private static FlexBox? Ring() => Application.ShellFocusHost is { Children.Count: > 0 } host ? (FlexBox)host.Children[0] : null;

    [Test]
    public async Task KeyboardFocusShowsARingOutsideTheControl()
    {
        var (_, button) = Mount();

        HostEvents.OnFocusChanged(button.Handle, true);
        Application.FlushRenders();
        var ring = Ring();
        var host = Application.ShellFocusHost!;

        await Assert.That(ring).IsNotNull();
        await Assert.That(host.Position).IsEqualTo(new Point(38.0f, 28.0f));
        await Assert.That(host.Width).IsEqualTo((Length)104.0f);
        await Assert.That(ring!.Border).IsEqualTo(new Border(2.0f, Theme.Active.Value.Colors.FocusRing));
        await Assert.That(ring.CornerRadius).IsEqualTo(new CornerRadius(10.0f));
    }

    [Test]
    public async Task MountingANewRootClearsTheRing()
    {
        var (abi, button) = Mount();
        HostEvents.OnFocusChanged(button.Handle, true);
        Application.FlushRenders();

        var next = new Button("Next");
        Application.Mount(FlexBox.Column(next));
        var afterMount = Ring();
        abi.Bounds[next.Handle] = new Rect(10.0f, 10.0f, 80.0f, 36.0f);
        HostEvents.OnFocusChanged(next.Handle, true);
        Application.FlushRenders();

        await Assert.That(afterMount).IsNull();
        await Assert.That(Ring()).IsNotNull();
        await Assert.That(Ring()!.IsBuilt).IsTrue();
        await Assert.That(Application.ShellFocusHost!.Position).IsEqualTo(new Point(8.0f, 8.0f));
    }

    [Test]
    public async Task TheRingFollowsItsOwnerAfterACommit()
    {
        var (abi, button) = Mount();
        HostEvents.OnFocusChanged(button.Handle, true);
        Application.FlushRenders();

        abi.Bounds[button.Handle] = new Rect(200.0f, 90.0f, 100.0f, 36.0f);
        FrameScheduler.MarkNeedsCommit();
        Application.FlushRenders();

        await Assert.That(Application.ShellFocusHost!.Position).IsEqualTo(new Point(198.0f, 88.0f));
    }

    [Test]
    public async Task APointerPressBlurOrDisableHidesTheRing()
    {
        var (_, button) = Mount();
        HostEvents.OnFocusChanged(button.Handle, true);
        Application.FlushRenders();

        HostInput.Pointer(PointerEventType.Down, button.Handle, 50.0f, 40.0f);
        var afterPress = Ring();
        HostInput.Key("Tab");
        HostInput.Key("Tab", KeyEventType.Up);
        var afterKey = Ring();
        button.Enabled(false);
        var afterDisable = Ring();

        await Assert.That(afterPress).IsNull();
        await Assert.That(afterKey).IsNotNull();
        await Assert.That(afterDisable).IsNull();
    }

    [Test]
    public async Task DisposingTheOwnerHidesTheRing()
    {
        var (_, button) = Mount();
        HostEvents.OnFocusChanged(button.Handle, true);
        Application.FlushRenders();

        button.Dispose();

        await Assert.That(Ring()).IsNull();
    }

    [Test]
    public async Task LabeledControlsAndSlidersShowTheRingToo()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var checkbox = new Checkbox("Agree");
        var slider = new Slider();
        Application.Mount(FlexBox.Column(checkbox, slider));
        abi.Bounds[checkbox.Handle] = new Rect(10.0f, 10.0f, 120.0f, 30.0f);
        abi.Bounds[slider.Handle] = new Rect(10.0f, 60.0f, 190.0f, 30.0f);

        HostEvents.OnFocusChanged(checkbox.Handle, true);
        Application.FlushRenders();
        var checkboxRing = Application.ShellFocusHost!.Position;
        HostEvents.OnFocusChanged(checkbox.Handle, false);
        HostEvents.OnFocusChanged(slider.Handle, true);
        Application.FlushRenders();

        await Assert.That(checkboxRing).IsEqualTo(new Point(8.0f, 8.0f));
        await Assert.That(Application.ShellFocusHost!.Position).IsEqualTo(new Point(8.0f, 58.0f));
    }
}
