using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class EditableTextMenuTests
{
    private const HostCapability Clipboard = HostCapability.ClipboardRead | HostCapability.ClipboardWrite;

    private static (LayoutAbi Abi, TextInput Input) Mount(HostCapability capabilities = Clipboard)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f, HostCapabilities = capabilities });
        var input = new TextInput("Café crème");
        Application.Mount(FlexBox.Column(new FlexBox().Height(200.0f), input));
        abi.Bounds[input.Editor.Handle] = new Rect(100.0f, 200.0f, 300.0f, 24.0f);
        abi.TextRangeRects[input.Editor.Handle] = [150.0f, 202.0f, 50.0f, 20.0f];
        return (abi, input);
    }

    private static List<ContextMenuEntry> Entries(ContextMenu menu)
    {
        var entries = new List<ContextMenuEntry>();
        void Collect(Node node)
        {
            foreach (var child in node.Children)
            {
                if (child is ContextMenuEntry entry)
                {
                    entries.Add(entry);
                }
                Collect(child);
            }
        }
        Collect(menu);
        return entries;
    }

    private static string Labels(IReadOnlyList<MenuItem> items)
    {
        var labels = new List<string>();
        foreach (var item in items)
        {
            labels.Add(item.IsSeparator ? "-" : item.Label + (item.IsEnabled ? "" : "(off)"));
        }
        return string.Join("|", labels);
    }

    private static void Choose(LayoutAbi abi, ContextMenuEntry entry)
    {
        abi.Bounds[entry.Parent!.Handle] = new Rect(96.0f, 96.0f, 240.0f, 300.0f);
        abi.Bounds[entry.Handle] = new Rect(100.0f, 100.0f, 200.0f, 28.0f);
        HostInput.Pointer(PointerEventType.Down, entry.Handle, 150.0f, 110.0f);
        HostInput.Pointer(PointerEventType.Up, entry.Handle, 150.0f, 110.0f);
    }

    private static ContextMenuEntry EntryLabelled(ContextMenu menu, string label) => Entries(menu).Find(entry => entry.Item?.Label == label)!;

    [Test]
    public async Task AnEditorsMenuOffersUndoRedoCutCopyPasteAndSelectAll()
    {
        var (abi, input) = Mount();
        var editor = input.Editor.Handle;
        // UTF-8 offsets 6..12 select "crème": "é" takes two bytes.
        HostEvents.OnSelectionChanged(editor, 6, 12);

        HostEvents.OnContextMenu(editor, 160.0f, 210.0f);
        var menu = Application.ShellContextMenu!;

        await Assert.That(Labels(menu.Items)).IsEqualTo("Undo(off)|Redo(off)|-|Cut|Copy|Paste|Select All");
        await Assert.That(menu.Items[3].Payload).IsEqualTo("crème");
        await Assert.That(menu.Items[3].SelectionRange).IsEqualTo((6u, 12u));
        await Assert.That(menu.Items[3].ShortcutLabel).IsEqualTo(KeyboardShortcuts.FormatPrimaryLabel("x"));
        await Assert.That(menu.Items[0].ShortcutLabel).IsEqualTo(KeyboardShortcuts.UndoLabel());
        await Assert.That(abi.HasCall("fui_freeze_text_selection_snapshot", (object)editor)).IsTrue();
    }

    [Test]
    public async Task WithoutASelectionCutAndCopyAreOffAndWithoutClipboardReadThereIsNoPaste()
    {
        var (_, input) = Mount(HostCapability.ClipboardWrite);
        input.SetCaret(2);

        HostEvents.OnContextMenu(input.Editor.Handle, 160.0f, 210.0f);

        await Assert.That(Labels(Application.ShellContextMenu!.Items)).IsEqualTo("Undo(off)|Redo(off)|-|Cut(off)|Copy(off)|Select All");
    }

    [Test]
    public async Task ARequestOverTheInputsPlaceholderTargetsItsEditor()
    {
        var (_, input) = Mount();
        input.Text = string.Empty;
        input.Placeholder = "Type here";
        var placeholderHost = input.Children[1];

        HostEvents.OnContextMenu(placeholderHost.Handle, 160.0f, 210.0f);

        await Assert.That(Labels(Application.ShellContextMenu!.Items)).IsEqualTo("Undo(off)|Redo(off)|-|Cut(off)|Copy(off)|Paste|Select All(off)");
        await Assert.That(Application.ShellContextMenu!.Items[5].TargetHandle).IsEqualTo(input.Editor.Handle);
    }

    [Test]
    public async Task CutWritesTheCapturedTextThenFallsBackToTheRuntimeAndRefocusesTheEditor()
    {
        var (abi, input) = Mount();
        var editor = input.Editor.Handle;
        HostEvents.OnSelectionChanged(editor, 6, 12);
        HostEvents.OnContextMenu(editor, 160.0f, 210.0f);
        var menu = Application.ShellContextMenu!;

        Choose(abi, EntryLabelled(menu, "Cut"));

        await Assert.That(abi.Sequence(
                "fui_copy_text",
                "fui_cut_text_selection_snapshot",
                "fui_delete_focused_text_range",
                "fui_cut_focused_text_selection",
                "ui_cut_text_selection",
                "fui_commit_text_action_focus"))
            .IsEqualTo("fui_copy_text,fui_cut_text_selection_snapshot,fui_delete_focused_text_range,fui_cut_focused_text_selection,ui_cut_text_selection,fui_commit_text_action_focus");
        await Assert.That(abi.HasCall("fui_copy_text", "crème")).IsTrue();
        await Assert.That(abi.HasCall("fui_delete_focused_text_range", 6u, 12u)).IsTrue();
        await Assert.That(menu.IsOpen).IsFalse();
    }

    [Test]
    public async Task PasteAndSelectAllActOnTheEditorAndRefocusIt()
    {
        var (abi, input) = Mount();
        var editor = input.Editor.Handle;

        HostEvents.OnContextMenu(editor, 160.0f, 210.0f);
        Choose(abi, EntryLabelled(Application.ShellContextMenu!, "Paste"));
        HostEvents.OnContextMenu(editor, 160.0f, 210.0f);
        Choose(abi, EntryLabelled(Application.ShellContextMenu!, "Select All"));

        await Assert.That(abi.Sequence("ui_paste_text", "fui_commit_text_action_focus", "ui_select_all_text", "fui_commit_text_action_focus"))
            .IsEqualTo("ui_paste_text,fui_commit_text_action_focus,ui_select_all_text,fui_commit_text_action_focus");
    }

    [Test]
    public async Task ATouchSelectionInAnEditorShowsCutCopyPasteAndSelectAll()
    {
        var (abi, input) = Mount();
        var editor = input.Editor.Handle;

        HostInput.Pointer(PointerEventType.Down, editor, 160.0f, 210.0f, pointerType: PointerType.Touch);
        HostEvents.OnSelectionChanged(editor, 6, 12);

        var labels = new List<string>();
        foreach (var item in TextSelectionToolbar.Items)
        {
            labels.Add(item.Label);
        }
        await Assert.That(string.Join("|", labels)).IsEqualTo("Cut|Copy|Paste|Select all");
        await Assert.That(TextSelectionToolbar.IsVisible).IsTrue();
        await Assert.That(TextSelectionToolbar.Items[0].Payload).IsEqualTo("crème");

        TextSelectionToolbar.Activate(2);
        await Assert.That(abi.Sequence("ui_paste_text", "fui_commit_text_action_focus")).IsEqualTo("ui_paste_text,fui_commit_text_action_focus");
    }
}
