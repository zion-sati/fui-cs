using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ComboBoxTests
{
    private static readonly Rect Field = new(40.0f, 100.0f, 240.0f, 32.0f);

    private static (LayoutAbi Abi, ComboBox ComboBox) Mounted(ComboBox comboBox)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        Application.Mount(FlexBox.Column(comboBox));
        abi.Bounds[comboBox.Handle] = Field;
        return (abi, comboBox);
    }

    private static ComboBox Cities() => new ComboBox().Items("Melbourne", "Sydney", "Brisbane", "Perth", "Adelaide");

    private static string Rows(ComboBox comboBox)
    {
        var labels = new List<string>();
        foreach (var row in comboBox.PopupList.OptionsHost.Children)
        {
            labels.Add(row.SemanticLabel ?? "");
        }
        return string.Join(",", labels);
    }

    /// <summary>Types into the editor as the host reports it.</summary>
    private static void Type(ComboBox comboBox, string text) => HostEvents.OnTextChanged(comboBox.Editor.Editor.Handle, text);

    private static void FocusEditor(ComboBox comboBox, bool focused) => HostEvents.OnFocusChanged(comboBox.Editor.Editor.Handle, focused);

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
    public async Task TypingFiltersTheListAndOpensIt()
    {
        var (_, comboBox) = Mounted(Cities());
        var texts = new List<string>();
        comboBox.TextChanged += e => texts.Add(e.Text);
        FocusEditor(comboBox, true);

        Type(comboBox, "e");
        await Assert.That(comboBox.IsOpen).IsTrue();
        await Assert.That(Rows(comboBox)).IsEqualTo("Melbourne,Sydney,Brisbane,Perth,Adelaide");
        Type(comboBox, "er");
        await Assert.That(comboBox.FilteredCount).IsEqualTo(1);
        await Assert.That(Rows(comboBox)).IsEqualTo("Perth");
        await Assert.That(comboBox.HighlightedIndex).IsEqualTo(0);

        // Nothing matches: the list closes.
        Type(comboBox, "erx");
        await Assert.That(comboBox.IsOpen).IsFalse();
        await Assert.That(string.Join(",", texts)).IsEqualTo("e,er,erx");

        comboBox.FilterMode = ComboBoxFilterMode.StartsWith;
        Type(comboBox, "s");
        await Assert.That(Rows(comboBox)).IsEqualTo("Sydney");
        comboBox.FilterMode = ComboBoxFilterMode.None;
        await Assert.That(comboBox.FilteredCount).IsEqualTo(5);
    }

    [Test]
    public async Task EnterSelectsTheHighlightedRowAndPutsItsTextInTheField()
    {
        var (_, comboBox) = Mounted(Cities());
        var changes = new List<string>();
        comboBox.Changed += e => changes.Add(e.Index + ":" + e.Item.Value);
        FocusEditor(comboBox, true);

        HostInput.Key("ArrowDown");
        await Assert.That(comboBox.IsOpen).IsTrue();
        HostInput.Key("ArrowDown");
        HostInput.Key("ArrowDown");
        HostInput.Key("Enter");

        await Assert.That(comboBox.IsOpen).IsFalse();
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(2);
        await Assert.That(comboBox.Text).IsEqualTo("Brisbane");
        await Assert.That(comboBox.Editor.Text).IsEqualTo("Brisbane");
        await Assert.That(string.Join(",", changes)).IsEqualTo("2:Brisbane");

        // The text filters the reopened list to the selection.
        HostInput.Key("ArrowDown");
        await Assert.That(Rows(comboBox)).IsEqualTo("Brisbane");
        HostInput.Key("Escape");
        await Assert.That(comboBox.IsOpen).IsFalse();
    }

    [Test]
    public async Task TabAndTypingKeysStayUnhandledSoFocusCanLeave()
    {
        var (_, comboBox) = Mounted(Cities());
        FocusEditor(comboBox, true);

        await Assert.That(HostInput.Key("Tab")).IsFalse();
        await Assert.That(HostInput.Key("a")).IsFalse();
        await Assert.That(HostInput.Key("ArrowLeft")).IsTrue();
        await Assert.That(HostInput.Key("ArrowDown")).IsTrue();
        await Assert.That(comboBox.IsOpen).IsTrue();
    }

    [Test]
    public async Task ThePointerSelectsARowAndTheChevronToggles()
    {
        var (abi, comboBox) = Mounted(Cities());
        var chevron = comboBox.Children[1];
        abi.Bounds[chevron.Handle] = new Rect(240.0f, 102.0f, 32.0f, 28.0f);

        HostInput.Click(chevron.Handle, 250.0f, 110.0f);
        await Assert.That(comboBox.IsOpen).IsTrue();
        await Assert.That(abi.HasCall("ui_request_focus", (object)comboBox.Editor.Editor.Handle)).IsTrue();
        HostInput.Click(chevron.Handle, 250.0f, 110.0f);
        await Assert.That(comboBox.IsOpen).IsFalse();

        HostInput.Click(comboBox.Handle, 44.0f, 110.0f);
        await Assert.That(comboBox.IsOpen).IsTrue();
        var row = comboBox.PopupList.OptionsHost.Children[3];
        HostInput.Pointer(PointerEventType.Down, row.Handle);
        HostInput.Pointer(PointerEventType.Up, row.Handle);
        await Assert.That(comboBox.SelectedItem).IsEqualTo(new ComboBoxItem("Perth"));
        await Assert.That(comboBox.IsOpen).IsFalse();
    }

    [Test]
    public async Task AutoCompleteFillsTheRestOfTheFirstMatchAndSelectsIt()
    {
        var (_, comboBox) = Mounted(Cities().AutoComplete());
        FocusEditor(comboBox, true);

        Type(comboBox, "Ad");
        await Assert.That(comboBox.Text).IsEqualTo("Adelaide");
        await Assert.That(comboBox.Editor.SelectionStart).IsEqualTo(2);
        await Assert.That(comboBox.Editor.SelectionEnd).IsEqualTo(8);
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(4);

        // Typing over the completed part completes again; deleting does not.
        Type(comboBox, "Ade");
        await Assert.That(comboBox.Text).IsEqualTo("Adelaide");
        await Assert.That(comboBox.Editor.SelectionStart).IsEqualTo(3);
        Type(comboBox, "Ad");
        await Assert.That(comboBox.Text).IsEqualTo("Ad");
        Type(comboBox, "A");
        await Assert.That(comboBox.Text).IsEqualTo("A");
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(-1);
    }

    [Test]
    public async Task BlurringCommitsTheTextByTheCommitMode()
    {
        var (_, comboBox) = Mounted(Cities().CommitMode(ComboBoxCommitMode.RevertToSelection));
        comboBox.SelectedIndex = 1;
        var texts = new List<string>();
        comboBox.TextChanged += e => texts.Add(e.Text);
        FocusEditor(comboBox, true);
        comboBox.StaysOpenOnEdit = false;

        Type(comboBox, "Syd-typo");
        FocusEditor(comboBox, false);
        await Assert.That(comboBox.Text).IsEqualTo("Sydney");
        await Assert.That(string.Join(",", texts)).IsEqualTo("Syd-typo,Sydney");

        comboBox.CommitMode = ComboBoxCommitMode.SelectExactMatch;
        var changes = new List<int>();
        comboBox.Changed += e => changes.Add(e.Index);
        FocusEditor(comboBox, true);
        Type(comboBox, "perth");
        FocusEditor(comboBox, false);
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(3);
        await Assert.That(comboBox.Text).IsEqualTo("Perth");
        await Assert.That(string.Join(",", changes)).IsEqualTo("3");
    }

    [Test]
    public async Task LosingFocusClosesTheListAfterTheNextCommitUnlessARowIsPressed()
    {
        var (_, comboBox) = Mounted(Cities());
        FocusEditor(comboBox, true);
        HostInput.Key("ArrowDown");
        await Assert.That(comboBox.IsOpen).IsTrue();

        // Focus moving from the editor to the field keeps the list open.
        FocusEditor(comboBox, false);
        HostEvents.OnFocusChanged(comboBox.Handle, true);
        FrameScheduler.FlushCommit();
        await Assert.That(comboBox.IsOpen).IsTrue();

        // A press on a row holds the list open through the blur it causes.
        var row = comboBox.PopupList.OptionsHost.Children[0];
        HostInput.Pointer(PointerEventType.Down, row.Handle);
        HostEvents.OnFocusChanged(comboBox.Handle, false);
        FrameScheduler.FlushCommit();
        await Assert.That(comboBox.IsOpen).IsTrue();
        HostInput.Pointer(PointerEventType.Cancel, row.Handle);
        FrameScheduler.FlushCommit();
        await Assert.That(comboBox.IsOpen).IsFalse();
    }

    [Test]
    public async Task CustomValuesClearTheSelectionUnlessDisallowed()
    {
        var (_, comboBox) = Mounted(Cities());
        comboBox.SelectedIndex = 0;
        FocusEditor(comboBox, true);

        Type(comboBox, "Hobart");
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(-1);
        comboBox.AllowCustomValue = false;
        comboBox.Text = "sydney";
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(1);
        comboBox.Text = "Darwin";
        await Assert.That(comboBox.SelectedIndex).IsEqualTo(1);
    }

    [Test]
    public async Task CodeChangesRaiseNothingAndMisuseThrows()
    {
        var (abi, comboBox) = Mounted(Cities().Placeholder("City"));
        var raised = 0;
        comboBox.Changed += _ => raised++;
        comboBox.TextChanged += _ => raised++;

        comboBox.SelectedIndex = 4;
        comboBox.Text = "Perth";
        comboBox.SelectedIndex = -1;
        await Assert.That(raised).IsEqualTo(0);
        await Assert.That(comboBox.Text).IsEqualTo("Perth");
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)comboBox.Handle, "Perth")).IsTrue();
        comboBox.Text = string.Empty;
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)comboBox.Handle, "Combo box")).IsTrue();

        await Assert.That(Throws<ArgumentOutOfRangeException>(() => comboBox.SelectedIndex = 5)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => comboBox.MaxVisibleItems = 0)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => comboBox.PopupPanelBackgroundBlur = -1.0f)).IsTrue();
    }

    [Test]
    public async Task ADisabledComboBoxDisablesItsEditorAndClosesItsList()
    {
        var (_, comboBox) = Mounted(Cities().OpenOnFocus());
        FocusEditor(comboBox, true);
        await Assert.That(comboBox.IsOpen).IsTrue();

        comboBox.IsEnabled = false;
        await Assert.That(comboBox.IsOpen).IsFalse();
        await Assert.That(comboBox.Editor.IsEnabled).IsFalse();
        await Assert.That(comboBox.Editor.Editor.IsEditableText).IsFalse();
        HostInput.Click(comboBox.Handle, 44.0f, 110.0f);
        await Assert.That(comboBox.IsOpen).IsFalse();
    }

    [Test]
    public async Task SizingAndColorsStyleTheFieldAndEditor()
    {
        var (_, comboBox) = Mounted(Cities());
        comboBox.Sizing = new DropdownSizing { FieldHeight = 44.0f, FieldFontSize = 18.0f, ChevronBoxSize = 40.0f };
        comboBox.Colors = new DropdownColors { Background = 0x111827FFu, Accent = 0x22C55EFFu };

        await Assert.That(comboBox.Height).IsEqualTo(Length.Pixels(44.0f));
        await Assert.That(comboBox.Background).IsEqualTo((Color)0x111827FFu);
        await Assert.That(comboBox.Editor.Height).IsEqualTo(Length.Pixels(40.0f));
        await Assert.That(comboBox.Editor.FontSize).IsEqualTo(18.0f);
        await Assert.That(comboBox.Editor.Editor.CaretColor).IsEqualTo((Color)0x22C55EFFu);
        await Assert.That(((FlexBox)comboBox.Children[1]).Width).IsEqualTo(Length.Pixels(40.0f));
    }
}
