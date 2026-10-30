using System.Collections.Generic;
using System.Globalization;

using FuiCs;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// Dropdowns in each state the specs exercise: a plain one with a readout of its changes, a disabled one, a long list
/// that scrolls after five rows, a templated one with tall green rows, and one whose list only fits by clamping to the
/// viewport.
/// </summary>
internal static class DropdownShowcase
{
    public static FlexBox Build()
    {
        var readout = new Text("Dropdown: Balanced (0)").FontSize(14.0f).NodeId("dropdown-readout");
        var plain = new Dropdown()
            .Items(new DropdownItem("balanced", "Balanced"), new DropdownItem("quality", "Quality first"), new DropdownItem("speed", "Speed first"))
            .Width(220.0f)
            .NodeId("dropdown-plain")
            .OnChanged(e => readout.Text = "Dropdown: " + e.Item.Label + " (" + e.Index.ToString(CultureInfo.InvariantCulture) + ")");
        var disabled = new Dropdown()
            .Items(new DropdownItem("off", "Unavailable"))
            .Width(220.0f)
            .NodeId("dropdown-disabled")
            .Enabled(false);
        var tallRows = new DropdownSizing { OptionHeight = 44.0f, OptionFontSize = 18.0f };
        var templated = new Dropdown()
            .Items(new DropdownItem("north", "North"), new DropdownItem("south", "South"), new DropdownItem("east", "East"))
            .OptionRowTemplate(new DropdownOptionRowTemplate(_ => DropdownOptionRowTemplate.Default.CreatePresenter(tallRows)))
            .Colors(new DropdownColors { Accent = 0x16A34AFFu })
            .Width(220.0f)
            .NodeId("dropdown-templated");

        return FlexBox.Column(
                new Text("Dropdowns").FontSize(20.0f).FontWeight(FontWeight.Bold),
                Gap(12.0f),
                FlexBox.Row(plain, Gap(16.0f), readout).AlignItems(AlignItems.Center),
                Gap(12.0f),
                disabled,
                Gap(12.0f),
                new Dropdown().Items(Numbered("Item", 30)).MaxVisibleItems(5).Width(220.0f).NodeId("dropdown-long"),
                Gap(12.0f),
                templated,
                Gap(12.0f),
                new Dropdown().Items(Numbered("Row", 60)).Width(220.0f).NodeId("dropdown-clamped"))
            .AlignItems(AlignItems.Start);
    }

    private static List<DropdownItem> Numbered(string prefix, int count)
    {
        var items = new List<DropdownItem>(count);
        for (var index = 1; index <= count; ++index)
        {
            var label = prefix + " " + index.ToString(CultureInfo.InvariantCulture);
            items.Add(new DropdownItem(label.ToLowerInvariant().Replace(' ', '-'), label));
        }
        return items;
    }

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
