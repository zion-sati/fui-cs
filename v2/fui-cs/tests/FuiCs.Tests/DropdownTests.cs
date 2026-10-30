using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DropdownTests
{
    private static readonly Rect Trigger = new(40.0f, 100.0f, 200.0f, 32.0f);

    private static (LayoutAbi Abi, Dropdown Dropdown) Mounted(int itemCount = 5, float viewportHeight = 600.0f)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = viewportHeight });
        var items = new List<DropdownItem>();
        for (var index = 0; index < itemCount; ++index)
        {
            items.Add(new DropdownItem("v" + index, "Option " + index));
        }
        var dropdown = new Dropdown().Items(items);
        Application.Mount(FlexBox.Column(dropdown));
        abi.Bounds[dropdown.Handle] = Trigger;
        return (abi, dropdown);
    }

    private static List<Node> Rows(Dropdown dropdown)
    {
        var rows = new List<Node>();
        foreach (var row in dropdown.PopupList.OptionsHost.Children)
        {
            rows.Add(row);
        }
        return rows;
    }

    /// <summary>The label text inside a row, which takes the row's pointer input.</summary>
    private static Node RowLabel(Node row)
    {
        for (var node = row; ; node = node.Children[0])
        {
            if (node is Text)
            {
                return node;
            }
        }
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
    public async Task ThePanelShowsTheVisibleRowsUnderTheTriggerAtItsWidth()
    {
        var (_, dropdown) = Mounted(itemCount: 10);
        dropdown.MaxVisibleItems = 4;

        HostInput.Click(dropdown.Handle);
        var list = dropdown.PopupList;

        await Assert.That(dropdown.IsOpen).IsTrue();
        await Assert.That(list.Panel.Width).IsEqualTo(Length.Pixels(200.0f));
        // Four 34 px rows inside 4 px of padding, below the trigger with the 4 px anchor gap.
        await Assert.That(list.Panel.Height).IsEqualTo(Length.Pixels(144.0f));
        await Assert.That(list.ScrollBox.Height).IsEqualTo(Length.Pixels(136.0f));
        await Assert.That(list.OptionsHost.Height).IsEqualTo(Length.Pixels(340.0f));
        await Assert.That(list.Presenter.SurfaceX).IsEqualTo(40.0f);
        await Assert.That(list.Presenter.SurfaceY).IsEqualTo(136.0f);
        await Assert.That(Rows(dropdown).Count).IsEqualTo(10);

        HostInput.Key("Escape");
        dropdown.PopupWidth = 320.0f;
        dropdown.MaxVisibleItems = null;
        HostInput.Click(dropdown.Handle);

        await Assert.That(list.Panel.Width).IsEqualTo(Length.Pixels(320.0f));
        await Assert.That(list.Panel.Height).IsEqualTo(Length.Pixels(348.0f));
    }

    [Test]
    public async Task AShortViewportClampsThePanelAndScrollsTheHighlightIntoView()
    {
        var (_, dropdown) = Mounted(itemCount: 10, viewportHeight: 120.0f);
        dropdown.SelectedIndex = 6;

        HostEvents.OnFocusChanged(dropdown.Handle, true);
        HostInput.Key("Enter");
        var list = dropdown.PopupList;

        // The viewport keeps 8 px clear at each edge: 104 px of panel, 96 px of rows.
        await Assert.That(list.Panel.Height).IsEqualTo(Length.Pixels(104.0f));
        await Assert.That(list.HighlightedIndex).IsEqualTo(6);
        // Row 6 ends at 238 px, so the list scrolls until it shows.
        await Assert.That(list.ScrollBox.VerticalOffset).IsEqualTo(142.0f);

        HostInput.Key("Home");
        await Assert.That(list.ScrollBox.VerticalOffset).IsEqualTo(0.0f);
    }

    [Test]
    public async Task TheKeyboardOpensMovesSelectsAndCloses()
    {
        var (abi, dropdown) = Mounted();
        var changes = new List<string>();
        dropdown.OnChanged(e => changes.Add(e.Index + ":" + e.Item.Value));
        HostEvents.OnFocusChanged(dropdown.Handle, true);

        HostInput.Key("ArrowDown");
        await Assert.That(dropdown.IsOpen).IsTrue();
        await Assert.That(dropdown.HighlightedIndex).IsEqualTo(0);
        await Assert.That(abi.HasCall("ui_set_semantic_expanded", (object)dropdown.Handle, true, true)).IsTrue();

        HostInput.Key("ArrowUp");
        await Assert.That(dropdown.HighlightedIndex).IsEqualTo(4);
        HostInput.Key("ArrowDown");
        await Assert.That(dropdown.HighlightedIndex).IsEqualTo(0);
        HostInput.Key("End");
        HostInput.Key("ArrowUp");
        HostInput.Key("Enter");

        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(3);
        await Assert.That(string.Join(",", changes)).IsEqualTo("3:v3");
        await Assert.That(dropdown.PopupList.Presenter.IsOpen).IsFalse();

        // Up opens on the item above the selection; Escape closes without changing it.
        HostInput.Key("ArrowUp");
        await Assert.That(dropdown.HighlightedIndex).IsEqualTo(2);
        HostInput.Key("Escape");
        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(3);

        // Space opens too; choosing the selected item again raises nothing.
        HostInput.Key(" ");
        HostInput.Key("Enter");
        await Assert.That(string.Join(",", changes)).IsEqualTo("3:v3");
    }

    [Test]
    public async Task ThePointerHighlightsAndSelectsRowsAndAClickOutsideCloses()
    {
        var (abi, dropdown) = Mounted();
        DropdownChangedEventArgs<DropdownItem>? changed = null;
        dropdown.Changed += e => changed = e;

        HostInput.Click(dropdown.Handle);
        var label = RowLabel(Rows(dropdown)[2]);
        HostInput.Pointer(PointerEventType.Enter, label.Handle);
        await Assert.That(dropdown.HighlightedIndex).IsEqualTo(2);

        HostInput.Click(label.Handle);
        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(changed!.Index).IsEqualTo(2);
        await Assert.That(changed.Item).IsEqualTo(new DropdownItem("v2", "Option 2"));
        await Assert.That(dropdown.PopupList.OptionsHost.Children.Count).IsEqualTo(5);

        HostInput.Click(dropdown.Handle);
        await Assert.That(dropdown.IsOpen).IsTrue();
        var overlay = dropdown.PopupList.Presenter.Overlay;
        abi.Bounds[overlay.Handle] = new Rect(0.0f, 0.0f, 800.0f, 600.0f);
        HostInput.Click(overlay.Handle, 600.0f, 500.0f);
        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(2);
    }

    [Test]
    public async Task TheFieldShowsTheSelectedLabelAndCodeSelectsWithoutRaisingChanged()
    {
        var (abi, dropdown) = Mounted();
        var raised = 0;
        dropdown.Changed += _ => raised++;
        var value = (Text)dropdown.Children[0].Children[0].Children[0];

        await Assert.That(dropdown.SelectedIndex).IsEqualTo(0);
        await Assert.That(value.Text).IsEqualTo("Option 0");

        dropdown.SelectedIndex = 4;
        await Assert.That(value.Text).IsEqualTo("Option 4");
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)dropdown.Handle, "Option 4")).IsTrue();
        dropdown.SelectedIndex = -1;
        await Assert.That(value.Text).IsEqualTo(string.Empty);
        await Assert.That(dropdown.SelectedItem).IsNull();
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)dropdown.Handle, "Dropdown")).IsTrue();

        // New items select the first and raise nothing either; an empty list selects nothing.
        dropdown.SelectedIndex = 2;
        dropdown.Items = [new DropdownItem("a"), new DropdownItem("b")];
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(0);
        await Assert.That(value.Text).IsEqualTo("a");
        dropdown.Items = [];
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(-1);
        HostInput.Click(dropdown.Handle);
        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(raised).IsEqualTo(0);
    }

    [Test]
    public async Task OutOfRangeValuesSetInCodeThrow()
    {
        var (_, dropdown) = Mounted();

        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.SelectedIndex = 5)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.SelectedIndex = -2)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.HighlightIndex(7))).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.MaxVisibleItems = 0)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.PopupWidth = 0.0f)).IsTrue();
        await Assert.That(Throws<ArgumentOutOfRangeException>(() => dropdown.PopupPanelBackgroundBlur = -1.0f)).IsTrue();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(0);
    }

    [Test]
    public async Task ADisabledDropdownDoesNotOpenAndDisablingClosesIt()
    {
        var (_, dropdown) = Mounted();

        HostInput.Click(dropdown.Handle);
        await Assert.That(dropdown.IsOpen).IsTrue();
        dropdown.IsEnabled = false;
        await Assert.That(dropdown.IsOpen).IsFalse();
        await Assert.That(Dropdown.Active).IsNull();

        HostInput.Click(dropdown.Handle);
        HostEvents.OnFocusChanged(dropdown.Handle, true);
        HostInput.Key("Enter");
        await Assert.That(dropdown.IsOpen).IsFalse();
    }

    [Test]
    public async Task OpeningAnotherDropdownClosesTheFirst()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var first = new Dropdown().Items([new DropdownItem("a"), new DropdownItem("b")]);
        var second = new Dropdown().Items([new DropdownItem("x"), new DropdownItem("y")]);
        Application.Mount(FlexBox.Column(first, second));
        abi.Bounds[first.Handle] = Trigger;
        abi.Bounds[second.Handle] = Trigger with { Y = 200.0f };

        HostInput.Click(first.Handle);
        // Tab moves focus on without closing the list, and the next field opens from the keyboard.
        HostEvents.OnFocusChanged(first.Handle, false);
        HostEvents.OnFocusChanged(second.Handle, true);
        await Assert.That(first.IsOpen).IsTrue();
        HostInput.Key(" ");

        await Assert.That(second.IsOpen).IsTrue();
        await Assert.That(first.IsOpen).IsFalse();
        await Assert.That(ReferenceEquals(Dropdown.Active, second)).IsTrue();
        Dropdown.HideActive();
        await Assert.That(second.IsOpen).IsFalse();
    }

    [Test]
    public async Task ScrollingTheTriggerOutOfItsScrollViewCloses()
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        var dropdown = new Dropdown().Items([new DropdownItem("a"), new DropdownItem("b")]);
        var scroll = new ScrollView().Width(300.0f).Height(200.0f).Child(FlexBox.Column(dropdown));
        Application.Mount(FlexBox.Column(scroll));
        abi.Bounds[scroll.Handle] = new Rect(0.0f, 50.0f, 300.0f, 200.0f);
        abi.Bounds[dropdown.Handle] = new Rect(0.0f, 60.0f, 200.0f, 32.0f);

        HostInput.Click(dropdown.Handle);
        HostEvents.OnScroll(scroll.Handle, 0.0f, 5.0f, 300.0f, 600.0f, 300.0f, 200.0f);
        await Assert.That(dropdown.IsOpen).IsTrue();

        abi.Bounds[dropdown.Handle] = new Rect(0.0f, 10.0f, 200.0f, 32.0f);
        HostEvents.OnScroll(scroll.Handle, 0.0f, 50.0f, 300.0f, 600.0f, 300.0f, 200.0f);
        await Assert.That(dropdown.IsOpen).IsFalse();
    }

    [Test]
    public async Task ARestoredSelectionRaisesChangedAndClampsToTheItems()
    {
        var (abi, dropdown) = Mounted();
        dropdown.NodeId = "size";
        var changes = new List<int>();
        dropdown.Changed += e => changes.Add(e.Index);

        dropdown.SelectedIndex = 3;
        HostEvents.CapturePersistedUiState();
        await Assert.That(abi.PersistedText[("size", "dropdown-selected-index")]).IsEqualTo((1u, "3"));

        dropdown.SelectedIndex = 0;
        HostEvents.RestorePersistedUiState();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(3);

        abi.PersistedText[("size", "dropdown-selected-index")] = (1u, "9");
        HostEvents.RestorePersistedUiState();
        await Assert.That(dropdown.SelectedIndex).IsEqualTo(4);
        await Assert.That(string.Join(",", changes)).IsEqualTo("3,4");
    }

    [Test]
    public async Task TemplatesAndSizingRebuildTheFieldAndRows()
    {
        var (_, dropdown) = Mounted();
        var sizings = new List<DropdownSizing?>();
        dropdown.OptionRowTemplate = new DropdownOptionRowTemplate(sizing =>
        {
            sizings.Add(sizing);
            return DropdownOptionRowTemplate.Default.CreatePresenter(new DropdownSizing { OptionHeight = 50.0f });
        });
        var sizing = new DropdownSizing { FieldHeight = 44.0f };
        dropdown.Sizing = sizing;
        dropdown.SelectedIndex = 1;

        HostInput.Click(dropdown.Handle);
        var value = (Text)dropdown.Children[0].Children[0].Children[0];

        await Assert.That(value.Text).IsEqualTo("Option 1");
        await Assert.That(dropdown.PopupList.Panel.Height).IsEqualTo(Length.Pixels(258.0f));
        await Assert.That(sizings.Count > 0 && ReferenceEquals(sizings[^1], sizing)).IsTrue();
        await Assert.That(((FlexBox)Rows(dropdown)[0]).Height).IsEqualTo(Length.Pixels(50.0f));
    }

    [Test]
    public async Task DisposingAnOpenDropdownClosesItAndReleasesItsKeys()
    {
        var (_, dropdown) = Mounted();
        HostInput.Click(dropdown.Handle);

        dropdown.Dispose();

        await Assert.That(Dropdown.Active).IsNull();
        await Assert.That(HostInput.Key("Escape")).IsFalse();
    }
}
