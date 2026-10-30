using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DialogTests
{
    private static (LayoutAbi Abi, Dialog Dialog, Button Opener) Mount()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var opener = new Button("Open");
        var dialog = new Dialog("Delete file?", "This cannot be undone.");
        Application.Mount(FlexBox.Column(opener, dialog).Padding(20.0f));
        abi.Calls.Clear();
        return (abi, dialog, opener);
    }

    private static void Press(string key)
    {
        HostInput.Key(key);
        HostInput.Key(key, KeyEventType.Up);
    }

    [Test]
    public async Task ShowingAttachesTheCardInASemanticScope()
    {
        var (abi, dialog, _) = Mount();

        dialog.Show();
        var card = dialog.TitleText.Parent!;

        await Assert.That(dialog.IsOpen).IsTrue();
        await Assert.That(ReferenceEquals(Dialog.Active, dialog)).IsTrue();
        await Assert.That(abi.Named("ui_push_semantic_scope").Count).IsEqualTo(1);
        await Assert.That(abi.HasCall("ui_set_semantic_role", card.Handle, SemanticRole.Dialog)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_label", card.Handle, "Delete file?")).IsTrue();
        await Assert.That(dialog.BodyText.Text).IsEqualTo("This cannot be undone.");
    }

    [Test]
    public async Task TheBackdropCoversTheViewportWhereverTheDialogIsMounted()
    {
        var (abi, dialog, _) = Mount();
        abi.Bounds[dialog.Handle] = new Rect(20.0f, 60.0f, 0.0f, 0.0f);

        dialog.Show();
        var overlay = (FlexBox)dialog.TitleText.Parent!.Parent!;

        await Assert.That(overlay.Position).IsEqualTo(new Point(-20.0f, -60.0f));
        await Assert.That(overlay.Width).IsEqualTo((Length)800.0f);
        await Assert.That(overlay.Height).IsEqualTo((Length)600.0f);
    }

    [Test]
    public async Task TheButtonsAcceptAndCancelAndHideTheDialog()
    {
        var (abi, dialog, _) = Mount();
        var log = new List<string>();
        dialog.OnAccepted(() => log.Add("accepted")).OnCancelled(() => log.Add("cancelled"));

        dialog.Show();
        dialog.AcceptButton.PerformClick();
        var openAfterAccept = dialog.IsOpen;
        dialog.Show();
        dialog.CancelButton.PerformClick();

        await Assert.That(string.Join(",", log)).IsEqualTo("accepted,cancelled");
        await Assert.That(openAfterAccept).IsFalse();
        await Assert.That(dialog.IsOpen).IsFalse();
        await Assert.That(Dialog.Active).IsNull();
        await Assert.That(abi.Named("ui_remove_semantic_scope").Count).IsEqualTo(2);
    }

    [Test]
    public async Task EnterAcceptsAndEscapeCancelsUnlessAButtonHasFocus()
    {
        var (_, dialog, _) = Mount();
        var log = new List<string>();
        dialog.OnAccepted(() => log.Add("accepted")).OnCancelled(() => log.Add("cancelled"));

        dialog.Show();
        HostInput.Key("Enter");
        var pressedDuringKey = dialog.AcceptButton.IsPressed;
        HostInput.Key("Enter", KeyEventType.Up);
        dialog.Show();
        Press("Escape");
        dialog.Show();
        HostEvents.OnFocusChanged(dialog.CancelButton.Handle, true);
        Press("Enter");

        await Assert.That(pressedDuringKey).IsTrue();
        await Assert.That(string.Join(",", log)).IsEqualTo("accepted,cancelled,cancelled");
    }

    [Test]
    public async Task ABackdropClickCancelsButAClickOnTheCardDoesNot()
    {
        var (abi, dialog, _) = Mount();
        var cancelled = 0;
        dialog.Cancelled += () => cancelled++;
        dialog.Show();
        var card = dialog.TitleText.Parent!;
        var overlay = card.Parent!;
        abi.Bounds[overlay.Handle] = new Rect(0.0f, 0.0f, 800.0f, 600.0f);
        abi.Bounds[card.Handle] = new Rect(190.0f, 200.0f, 420.0f, 200.0f);

        HostInput.Click(overlay.Handle, 300.0f, 300.0f);
        var afterCardClick = cancelled;
        HostInput.Click(overlay.Handle, 20.0f, 20.0f);

        await Assert.That(afterCardClick).IsEqualTo(0);
        await Assert.That(cancelled).IsEqualTo(1);
    }

    [Test]
    public async Task FocusMovesToTheAcceptButtonAfterTheFrameAndReturnsOnClose()
    {
        var (abi, dialog, opener) = Mount();
        var shown = 0;
        dialog.Shown += () => shown++;
        HostEvents.OnFocusChanged(opener.Handle, true);

        dialog.Show();
        var shownBeforeCommit = shown;
        FrameScheduler.FlushCommit();
        var focusedAccept = abi.HasCall("ui_request_focus", (object)dialog.AcceptButton.Handle);
        Press("Escape");

        await Assert.That(shownBeforeCommit).IsEqualTo(0);
        await Assert.That(shown).IsEqualTo(1);
        await Assert.That(focusedAccept).IsTrue();
        await Assert.That(abi.Named("ui_request_focus")[^1].Arguments[0]).IsEqualTo((object)opener.Handle);
    }

    [Test]
    public async Task ShowAsyncCompletesWithTheResult()
    {
        var (_, dialog, _) = Mount();

        var accepted = dialog.ShowAsync();
        Press("Enter");
        var cancelled = dialog.ShowAsync();
        Dialog.CancelActive();

        await Assert.That(await accepted).IsTrue();
        await Assert.That(await cancelled).IsFalse();
    }

    [Test]
    public async Task BodyContentAndStyleOverridesApply()
    {
        var (_, dialog, _) = Mount();
        var field = new FlexBox().Width(100.0f).Height(20.0f);
        var red = new Color(0xFF0000FF);

        dialog.BodyContent(field).CardColor(red).BackdropColor(red).CardCornerRadius(4.0f);
        dialog.Show();
        var card = (FlexBox)dialog.TitleText.Parent!;
        var overlay = (FlexBox)card.Parent!;
        Theme.UseCustom(Theme.DefaultLight);

        await Assert.That(ReferenceEquals(field.Parent?.Parent, card)).IsTrue();
        await Assert.That(card.Background).IsEqualTo(red);
        await Assert.That(overlay.Background).IsEqualTo(red);
        await Assert.That(card.CornerRadius).IsEqualTo(new CornerRadius(4.0f));
        await Assert.That(card.Border).IsEqualTo(new Border(1.0f, Theme.DefaultLight.Colors.Border));
        await Assert.That(dialog.TitleText.TextColor).IsEqualTo((Color?)Theme.DefaultLight.Colors.TextPrimary);
    }

    [Test]
    public async Task AFormPressesItsButtonsOnlyWhileActive()
    {
        TestRuntime.Start(new LayoutAbi());
        var clicks = new List<string>();
        var ok = new Button("OK").OnClick(_ => clicks.Add("ok"));
        var cancel = new Button("Cancel").OnClick(_ => clicks.Add("cancel"));
        var form = new Form().DefaultButton(ok).CancelButton(cancel).Child(ok).Child(cancel);
        Application.Mount(form);

        Press("Enter");
        form.Activate();
        Press("Escape");
        HostInput.Key("Enter", modifiers: KeyModifier.Shift);
        HostInput.Key("Enter", KeyEventType.Up, KeyModifier.Shift);
        form.Deactivate();
        Press("Enter");

        await Assert.That(string.Join(",", clicks)).IsEqualTo("cancel,ok");
        await Assert.That(form.IsActive).IsFalse();
    }
}
