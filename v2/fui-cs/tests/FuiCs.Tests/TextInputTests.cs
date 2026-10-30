using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class TextInputTests
{
    private static (LayoutAbi Abi, TextInput Input) Mounted(TextInput input)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        Application.Mount(FlexBox.Column(input));
        return (abi, input);
    }

    private static bool Throws<TException>(Action action)
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

    [Test]
    public async Task TheDefaultTemplateStylesTheShellAndTheEditorFollowsTheTheme()
    {
        var (_, input) = Mounted(new TextInput("hello"));
        var theme = Theme.Active.Value;

        await Assert.That(input.Background).IsEqualTo(theme.Colors.Surface);
        await Assert.That(input.Border).IsEqualTo(new Border(1.0f, theme.Colors.Border));
        await Assert.That(input.Padding).IsEqualTo(new Thickness(theme.Spacing.Md, theme.Spacing.Sm));
        await Assert.That(input.Cursor).IsEqualTo(CursorStyle.Text);
        await Assert.That(input.Editor.Height).IsEqualTo(Length.Pixels(theme.Fonts.SizeBody + theme.Spacing.Sm));
        await Assert.That(input.Editor.FontSize).IsEqualTo(theme.Fonts.SizeBody);
        await Assert.That(input.Editor.CaretColor).IsEqualTo(theme.Colors.Accent);
        await Assert.That(input.Editor.IsEditableText).IsTrue();

        input.Colors = new TextInputColors { Background = 0x101010FFu, Caret = 0x22C55EFFu, TextPrimary = 0x334155FFu };
        input.FontSize = 20.0f;
        await Assert.That(input.Background).IsEqualTo((Color)0x101010FFu);
        await Assert.That(input.Editor.CaretColor).IsEqualTo((Color)0x22C55EFFu);
        await Assert.That(input.Editor.TextColor).IsEqualTo((Color)0x334155FFu);
        await Assert.That(input.Editor.Height).IsEqualTo(Length.Pixels(20.0f + theme.Spacing.Sm));

        var applied = new List<TextInputVisualState>();
        input.Template = new TextInputTemplate(() => new RecordingPresenter(applied));
        input.IsEnabled = false;
        await Assert.That(applied[^1]).IsEqualTo(new TextInputVisualState(false, false, false));
        await Assert.That(input.Editor.IsEditableText).IsFalse();
        await Assert.That(input.Editor.IsSelectableText).IsFalse();
        await Assert.That(input.Editor.TextColor).IsEqualTo(theme.Colors.TextMuted);
    }

    [Test]
    public async Task ThePlaceholderShowsWhileTheTextIsEmptyAndLabelsTheEditor()
    {
        var (abi, input) = Mounted(new TextInput().Placeholder("Search"));
        var editor = input.Editor.Handle;

        await Assert.That(input.Children.Count).IsEqualTo(2);
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)editor, "Search")).IsTrue();

        input.Text = "query";
        await Assert.That(input.Children.Count).IsEqualTo(1);
        input.Text = string.Empty;
        await Assert.That(input.Children.Count).IsEqualTo(2);

        input.Placeholder = string.Empty;
        await Assert.That(input.Children.Count).IsEqualTo(1);
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)editor, "Text input")).IsTrue();
        input.IsPassword = true;
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)editor, "Password input")).IsTrue();
        input.IsPassword = false;
        input.Text = "Read me";
        input.IsReadOnly = true;
        input.Text = "Read me";
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)editor, "Read me")).IsTrue();
        input.SemanticLabel = "Name";
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)editor, "Name")).IsTrue();
    }

    [Test]
    public async Task SelectionsCountCharactersAndReachTheRuntimeAsUtf8Offsets()
    {
        // é takes two UTF-8 bytes and the emoji four (and two UTF-16 units).
        var (abi, input) = Mounted(new TextInput("hé😀x"));
        var editor = input.Editor.Handle;

        await Assert.That(input.SelectionStart).IsEqualTo(4);
        await Assert.That(input.SelectionEndByteOffset).IsEqualTo(8u);
        await Assert.That(abi.HasCall("ui_set_text_selection_range", (object)editor, 8u, 8u)).IsTrue();

        input.SetSelection(1, 3);
        await Assert.That(input.SelectionStartByteOffset).IsEqualTo(1u);
        await Assert.That(input.SelectionEndByteOffset).IsEqualTo(7u);
        await Assert.That(abi.HasCall("ui_set_text_selection_range", (object)editor, 1u, 7u)).IsTrue();

        input.SetCaret(99);
        await Assert.That(input.SelectionStart).IsEqualTo(4);
        input.SetCaret(2);
        input.CaretToEnd();
        await Assert.That(input.SelectionEnd).IsEqualTo(4);
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => input.SetSelection(-1, 0))).IsTrue();

        var selections = new List<string>();
        input.SelectionChanged += e => selections.Add(e.Start + "-" + e.End);
        HostEvents.OnSelectionChanged(editor, 3, 7);
        await Assert.That(input.SelectionStart).IsEqualTo(2);
        await Assert.That(input.SelectionEnd).IsEqualTo(3);
        await Assert.That(string.Join(",", selections)).IsEqualTo("2-3");
    }

    [Test]
    public async Task HostEditsRaiseTextChangedOnceAndCodeChangesRaiseNothing()
    {
        var (_, input) = Mounted(new TextInput("abc"));
        var changes = new List<string>();
        input.TextChanged += e => changes.Add(e.Text);

        input.Text = "code";
        HostEvents.OnTextChanged(input.Editor.Handle, "typed");
        HostEvents.OnTextChanged(input.Editor.Handle, "typed");
        HostEvents.OnTextReplaced(input.Editor.Handle, 0, 1, "T");

        await Assert.That(string.Join(",", changes)).IsEqualTo("typed,Typed");
        await Assert.That(input.Text).IsEqualTo("Typed");
        // The caret was at the end of "code"; the shorter text clamps it.
        await Assert.That(input.SelectionStart).IsEqualTo(4);
        HostEvents.OnTextChanged(input.Editor.Handle, "ab");
        await Assert.That(input.SelectionStart).IsEqualTo(2);
    }

    [Test]
    public async Task LimitsReadOnlyPasswordAndAutofillReachTheEditor()
    {
        var (abi, input) = Mounted(new TextInput());
        var editor = input.Editor.Handle;

        input.MaxChars = 5;
        await Assert.That(abi.HasCall("ui_set_text_limits", (object)editor, 5, 1)).IsTrue();
        input.MaxChars = null;
        await Assert.That(abi.HasCall("ui_set_text_limits", (object)editor, int.MaxValue, 1)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => input.MaxChars = -1)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => input.FontSize = 0.0f)).IsTrue();

        input.IsReadOnly = true;
        await Assert.That(abi.HasCall("ui_set_editable", (object)editor, false)).IsTrue();
        await Assert.That(input.Editor.IsSelectableText).IsTrue();

        input.IsPassword = true;
        input.HostAutofill = "current-password";
        await Assert.That(abi.HasCall("ui_set_text_obscured", (object)editor, true)).IsTrue();
        await Assert.That(abi.HasCall("fui_register_text_input_metadata", (object)editor, true, "current-password")).IsTrue();
        input.HostAutofill = string.Empty;
        await Assert.That(abi.HasCall("fui_register_text_input_metadata", (object)editor, true, "")).IsTrue();
        await Assert.That(abi.HasCall("ui_set_selection_area_barrier", (object)input.Handle, true)).IsTrue();
    }

    [Test]
    public async Task TheEditorsFocusIsTheInputsAndAPressOnTheShellFocusesIt()
    {
        var (abi, input) = Mounted(new TextInput("abc"));
        var focus = new List<bool>();
        input.FocusChanged += e => focus.Add(e.Focused);
        input.SetSelection(0, 1);

        HostInput.Pointer(PointerEventType.Down, input.Handle);
        await Assert.That(abi.HasCall("ui_request_focus", (object)input.Editor.Handle)).IsTrue();

        HostEvents.OnFocusChanged(input.Editor.Handle, true);
        HostEvents.OnFocusChanged(input.Editor.Handle, true);
        await Assert.That(input.IsFocused).IsTrue();
        HostEvents.OnFocusChanged(input.Editor.Handle, false);
        await Assert.That(input.IsFocused).IsFalse();
        await Assert.That(string.Join(",", focus)).IsEqualTo("True,False");

        input.IsEnabled = false;
        abi.Calls.Clear();
        HostInput.Pointer(PointerEventType.Down, input.Handle);
        input.Focus();
        await Assert.That(abi.CountOf("ui_request_focus")).IsEqualTo(0);
    }

    [Test]
    public async Task NavigationKeysAreConsumedAndShortcutsPassThrough()
    {
        var (_, input) = Mounted(new TextInput("abc"));
        HostEvents.OnFocusChanged(input.Editor.Handle, true);

        await Assert.That(HostInput.Key("ArrowLeft")).IsTrue();
        await Assert.That(HostInput.Key("End", KeyEventType.Down, KeyModifier.Shift)).IsTrue();
        await Assert.That(HostInput.Key("PageDown")).IsTrue();
        await Assert.That(HostInput.Key("c", KeyEventType.Down, KeyModifier.Ctrl)).IsFalse();
        await Assert.That(HostInput.Key("ArrowLeft", KeyEventType.Down, KeyModifier.Alt)).IsFalse();
        // Without AcceptsTab, Tab is left to move focus.
        HostInput.Key("Tab");
        await Assert.That(input.Text).IsEqualTo("abc");
    }

    [Test]
    public async Task AnInputThatAcceptsTabsInsertsOneWithinItsLimit()
    {
        var (abi, input) = Mounted(new TextInput("ab😀").AcceptsTab());
        var changes = new List<string>();
        var selections = new List<string>();
        input.TextChanged += e => changes.Add(e.Text);
        input.SelectionChanged += e => selections.Add(e.Start + "-" + e.End);
        HostEvents.OnFocusChanged(input.Editor.Handle, true);
        input.SetSelection(1, 2);

        await Assert.That(HostInput.Key("Tab")).IsTrue();
        await Assert.That(input.Text).IsEqualTo("a\t😀");
        await Assert.That(abi.HasCall("ui_replace_text_range", (object)input.Editor.Handle, 1u, 2u, "\t", 2u)).IsTrue();
        await Assert.That(string.Join(",", changes)).IsEqualTo("a\t😀");
        await Assert.That(string.Join(",", selections)).IsEqualTo("2-2");

        input.MaxChars = 3;
        await Assert.That(HostInput.Key("Tab")).IsTrue();
        await Assert.That(input.Text).IsEqualTo("a\t😀");
        input.IsReadOnly = true;
        input.MaxChars = null;
        HostInput.Key("Tab");
        await Assert.That(input.Text).IsEqualTo("a\t😀");
    }

    [Test]
    public async Task TheTextPersistsUnlessItIsAPassword()
    {
        var (abi, input) = Mounted(new TextInput("draft"));
        input.NodeId = "note";
        var changes = new List<string>();
        input.TextChanged += e => changes.Add(e.Text);

        HostEvents.CapturePersistedUiState();
        input.Text = "changed";
        HostEvents.RestorePersistedUiState();
        await Assert.That(abi.PersistedText[("note", "text-input-value")]).IsEqualTo((1u, "draft"));
        await Assert.That(input.Text).IsEqualTo("draft");
        await Assert.That(string.Join(",", changes)).IsEqualTo("draft");

        abi.PersistedText.Clear();
        input.IsPassword = true;
        HostEvents.CapturePersistedUiState();
        await Assert.That(abi.PersistedText.Count).IsEqualTo(0);
    }

    [Test]
    public async Task DisposingTwiceIsHarmless()
    {
        var (_, input) = Mounted(new TextInput("text").Placeholder("Hint"));

        input.Dispose();
        input.Dispose();

        await Assert.That(input.IsBuilt).IsFalse();
        await Assert.That(input.Editor.IsBuilt).IsFalse();
    }

    private sealed class RecordingPresenter(List<TextInputVisualState> applied) : TextInputPresenter
    {
        public override void Apply(Theme theme, TextInputVisualState state, TextInputColors? colors)
        {
            applied.Add(state);
            Host.Opacity = state.IsEnabled ? 1.0f : 0.5f;
        }
    }
}
