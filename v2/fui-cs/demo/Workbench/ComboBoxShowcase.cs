using System.Globalization;

using FuiCs;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// Combo boxes in each mode the specs exercise: one that keeps custom text, with a readout of its text, selection,
/// filtered count and last event; one that only takes an exact match on commit; one that opens on focus; one that
/// autocompletes; and one with templated rows.
/// </summary>
internal static class ComboBoxShowcase
{
    private static readonly string[] Cities = ["Melbourne", "Sydney", "Singapore", "San Francisco", "Seattle", "Tokyo", "Osaka", "London", "Paris", "Berlin"];

    public static FlexBox Build()
    {
        var readout = new Text(string.Empty).FontSize(14.0f).NodeId("combo-readout");
        var lastEvent = new Text("Last event: none").FontSize(14.0f).NodeId("combo-event");
        var custom = new ComboBox().Placeholder("Search city").Items(Cities).MaxVisibleItems(5).Width(240.0f).NodeId("combo-custom");
        void Show() => readout.Text = "Text: \"" + custom.Text + "\" • Selected: " + (custom.SelectedItem?.Value ?? "none")
            + " (" + custom.SelectedIndex.ToString(CultureInfo.InvariantCulture) + ") • Filtered: "
            + custom.FilteredCount.ToString(CultureInfo.InvariantCulture);
        custom.TextChanged += e =>
        {
            lastEvent.Text = "Last event: text \"" + e.Text + "\"";
            Show();
        };
        custom.Changed += e =>
        {
            lastEvent.Text = "Last event: changed " + e.Item.Value + " (" + e.Index.ToString(CultureInfo.InvariantCulture) + ")";
            Show();
        };
        Show();
        var tallRows = new DropdownSizing { OptionHeight = 40.0f };

        return FlexBox.Column(
                new Text("Combo boxes").FontSize(20.0f).FontWeight(FontWeight.Bold),
                Gap(12.0f),
                custom,
                Gap(8.0f),
                readout,
                Gap(4.0f),
                lastEvent,
                Gap(12.0f),
                new ComboBox().Items(Cities).AllowCustomValue(false).CommitMode(ComboBoxCommitMode.SelectExactMatch).Width(240.0f).NodeId("combo-exact"),
                Gap(8.0f),
                new ComboBox().Placeholder("Opens on focus").Items(Cities).OpenOnFocus().MaxVisibleItems(4).Width(240.0f).NodeId("combo-open-on-focus"),
                Gap(8.0f),
                new ComboBox().Placeholder("Autocomplete").Items(Cities).AutoComplete().FilterMode(ComboBoxFilterMode.StartsWith).Width(240.0f).NodeId("combo-autocomplete"),
                Gap(8.0f),
                new ComboBox().Placeholder("Templated").Items(Cities)
                    .OptionRowTemplate(new DropdownOptionRowTemplate(_ => DropdownOptionRowTemplate.Default.CreatePresenter(tallRows)))
                    .Colors(new DropdownColors { Accent = 0x16A34AFFu })
                    .MaxVisibleItems(4)
                    .Width(240.0f)
                    .NodeId("combo-templated"))
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
