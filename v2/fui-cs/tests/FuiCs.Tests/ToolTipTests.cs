using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ToolTipTests
{
    private static (LayoutAbi Abi, ManualTime Time, Button Save, Button Open) Mount()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var save = new Button("Save").ToolTip("Save the file");
        var open = new Button("Open").ToolTip(new ToolTip("Open a file") { Placement = PopupPlacement.Bottom, ShowDuration = default });
        Application.Mount(FlexBox.Column(new FlexBox().Height(200.0f), save, open));
        abi.Bounds[save.Handle] = new Rect(100.0f, 200.0f, 80.0f, 32.0f);
        abi.Bounds[open.Handle] = new Rect(100.0f, 240.0f, 80.0f, 32.0f);
        var time = new ManualTime();
        ToolTipManager.Time = time;
        return (abi, time, save, open);
    }

    private static void Hover(Node node, float x, float y) => HostInput.Pointer(PointerEventType.Enter, node.Handle, x, y);

    private static void Leave(Node node) => HostInput.Pointer(PointerEventType.Leave, node.Handle, 0.0f, 0.0f);

    [Test]
    public async Task HoveringShowsTheToolTipAfterItsDelayAtThePointerAndLeavingHidesIt()
    {
        var (_, time, save, _) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(699);
        var early = ToolTipManager.ActiveOwner;
        time.Advance(1);
        var shown = (ToolTipManager.ActiveOwner, ToolTipManager.Label!.Text, ToolTipManager.Panel!.Position);
        Leave(save);

        await Assert.That(early).IsNull();
        await Assert.That(ReferenceEquals(shown.ActiveOwner, save)).IsTrue();
        await Assert.That(shown.Text).IsEqualTo("Save the file");
        await Assert.That(shown.Position).IsEqualTo(new Point(120.0f, 210.0f));
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task MountingANewRootClearsTheToolTip()
    {
        var (abi, time, save, open) = Mount();
        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        Hover(open, 120.0f, 250.0f);

        var next = new Button("Next").ToolTip("Next page");
        Application.Mount(FlexBox.Column(next));
        var afterMount = (ToolTipManager.ActiveOwner, ToolTipManager.Panel);
        time.Advance(5000);
        var afterTimers = ToolTipManager.ActiveOwner;
        abi.Bounds[next.Handle] = new Rect(10.0f, 10.0f, 80.0f, 32.0f);
        Hover(next, 20.0f, 20.0f);
        time.Advance(700);

        await Assert.That(afterMount.ActiveOwner).IsNull();
        await Assert.That(afterMount.Panel).IsNull();
        await Assert.That(afterTimers).IsNull();
        await Assert.That(ReferenceEquals(ToolTipManager.ActiveOwner, next)).IsTrue();
        await Assert.That(ToolTipManager.Panel!.IsBuilt).IsTrue();
    }

    [Test]
    public async Task AShownToolTipLeavesKeyboardFocusAndSemanticsAlone()
    {
        var (abi, time, save, _) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);

        await Assert.That(ReferenceEquals(ToolTipManager.ActiveOwner, save)).IsTrue();
        await Assert.That(abi.CountOf("ui_push_semantic_scope")).IsEqualTo(0);
    }

    [Test]
    public async Task TheNextToolTipShowsAtOnceWithinTheBetweenShowDelay()
    {
        var (_, time, save, open) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        Leave(save);
        time.Advance(50);
        Hover(open, 120.0f, 250.0f);
        var quick = ToolTipManager.ActiveOwner;
        Leave(open);
        time.Advance(200);
        Hover(save, 120.0f, 210.0f);

        await Assert.That(ReferenceEquals(quick, open)).IsTrue();
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task APressOnTheOwnerAndTheShowDurationHideTheToolTip()
    {
        var (_, time, save, _) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        HostInput.Pointer(PointerEventType.Down, save.Handle, 120.0f, 210.0f);
        var afterPress = ToolTipManager.ActiveOwner;
        HostInput.Pointer(PointerEventType.Up, save.Handle, 120.0f, 210.0f);
        Leave(save);
        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        var again = ToolTipManager.ActiveOwner;
        // Shown again at once (within the between-show delay of the first); its five seconds run out, and it stays
        // hidden while the pointer rests.
        time.Advance(10_000);
        var afterDuration = ToolTipManager.ActiveOwner;
        Leave(save);
        Hover(save, 120.0f, 210.0f);
        time.Advance(700);

        await Assert.That(afterPress).IsNull();
        await Assert.That(ReferenceEquals(again, save)).IsTrue();
        await Assert.That(afterDuration).IsNull();
        await Assert.That(ReferenceEquals(ToolTipManager.ActiveOwner, save)).IsTrue();
    }

    [Test]
    public async Task KeyboardFocusShowsTheToolTipNextToTheOwnerButPointerFocusDoesNot()
    {
        var (_, time, save, open) = Mount();

        FocusVisibility.KeyboardFocusVisible.Value = true;
        HostEvents.OnFocusChanged(open.Handle, true);
        time.Advance(700);
        var keyboard = (ToolTipManager.ActiveOwner, ToolTipManager.Panel!.Position.Y);
        HostEvents.OnFocusChanged(open.Handle, false);
        var afterBlur = ToolTipManager.ActiveOwner;
        FocusVisibility.KeyboardFocusVisible.Value = false;
        HostEvents.OnFocusChanged(save.Handle, true);
        time.Advance(5000);

        // Placed below the owner: its bottom edge plus the anchor gap.
        await Assert.That(ReferenceEquals(keyboard.ActiveOwner, open)).IsTrue();
        await Assert.That(keyboard.Y).IsEqualTo(240.0f + 32.0f + 8.0f);
        await Assert.That(afterBlur).IsNull();
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task ScrollingHidesAPointerToolTipUntilThePointerEntersAgain()
    {
        var (_, time, save, _) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        HostEvents.OnScroll(save.Parent!.Handle, 0.0f, 20.0f, 800.0f, 1200.0f, 800.0f, 600.0f);
        var afterScroll = ToolTipManager.ActiveOwner;
        time.Advance(1000);
        var stillHidden = ToolTipManager.ActiveOwner;
        Leave(save);
        Hover(save, 120.0f, 210.0f);

        await Assert.That(afterScroll).IsNull();
        await Assert.That(stillHidden).IsNull();
        // Within the between-show delay of the last tooltip? No: a second passed, so it waits again.
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task DisposingTheOwnerHidesTheToolTipAndKeepsItsHostForTheNext()
    {
        var (_, time, save, open) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        save.Parent!.RemoveChild(save);
        save.Dispose();
        var afterDispose = (ToolTipManager.ActiveOwner, ToolTipManager.Panel!.IsDisposed);
        Hover(open, 120.0f, 250.0f);
        time.Advance(700);

        await Assert.That(afterDispose.ActiveOwner).IsNull();
        await Assert.That(afterDispose.IsDisposed).IsFalse();
        await Assert.That(ReferenceEquals(ToolTipManager.ActiveOwner, open)).IsTrue();
    }

    [Test]
    public async Task ChangingTheShownToolTipUpdatesItAndClearingItHidesIt()
    {
        var (_, time, save, _) = Mount();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        save.ToolTip = "Save as";
        var changed = ToolTipManager.Label!.Text;
        save.ToolTip = null;

        await Assert.That(changed).IsEqualTo("Save as");
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task DeactivatingAViewClearsItsToolTip()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var save = new Button("Save").ToolTip("Save the file");
        var view = new RetainedView(FlexBox.Column(save));
        Application.Mount(view.Root);
        abi.Bounds[save.Handle] = new Rect(100.0f, 200.0f, 80.0f, 32.0f);
        var time = new ManualTime();
        ToolTipManager.Time = time;
        view.Activate();

        Hover(save, 120.0f, 210.0f);
        time.Advance(700);
        var shown = ToolTipManager.ActiveOwner;
        view.Deactivate();

        await Assert.That(ReferenceEquals(shown, save)).IsTrue();
        await Assert.That(ToolTipManager.ActiveOwner).IsNull();
    }

    [Test]
    public async Task AnAppWithoutToolTipsNeverInstallsTheManager()
    {
        TestRuntime.Start(new LayoutAbi());
        var button = new Button("Plain");
        Application.Mount(FlexBox.Column(button));

        HostInput.Pointer(PointerEventType.Enter, button.Handle, 10.0f, 10.0f);

        await Assert.That(EventRouter.ToolTips).IsNull();
    }
}
