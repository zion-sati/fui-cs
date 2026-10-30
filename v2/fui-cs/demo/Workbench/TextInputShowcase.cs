using System.Globalization;

using FuiCs;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// Text inputs in each state the specs exercise, with readouts of the first one's value, selection (in characters and
/// UTF-8 bytes), focus and event counts: a placeholder input limited to 24 characters, a password, a read-only
/// input, a disabled one, and one in custom colors.
/// </summary>
internal static class TextInputShowcase
{
    public static FlexBox Build()
    {
        var changes = 0;
        var selections = 0;
        var value = new Text("Value: <empty>").FontSize(14.0f).NodeId("text-input-value");
        var selection = new Text("Selection: 0-0 (bytes 0-0)").FontSize(14.0f).NodeId("text-input-selection");
        var focus = new Text("Focus: blurred").FontSize(14.0f).NodeId("text-input-focus");
        var counts = new Text("Changes: 0 • Selections: 0").FontSize(14.0f).NodeId("text-input-counts");
        var input = new TextInput().Placeholder("Type here").MaxChars(24).Width(260.0f).NodeId("text-input-plain");
        input.TextChanged += e =>
        {
            value.Text = "Value: " + (e.Text.Length == 0 ? "<empty>" : "\"" + e.Text + "\"");
            counts.Text = "Changes: " + Format(++changes) + " • Selections: " + Format(selections);
        };
        input.SelectionChanged += e =>
        {
            selection.Text = "Selection: " + Format((int)e.Start) + "-" + Format((int)e.End)
                + " (bytes " + Format((int)input.SelectionStartByteOffset) + "-" + Format((int)input.SelectionEndByteOffset) + ")";
            counts.Text = "Changes: " + Format(changes) + " • Selections: " + Format(++selections);
        };
        input.FocusChanged += e => focus.Text = "Focus: " + (e.Focused ? "focused" : "blurred");

        return FlexBox.Column(
                new Text("Text inputs").FontSize(20.0f).FontWeight(FontWeight.Bold),
                Gap(12.0f),
                input,
                Gap(8.0f),
                value,
                Gap(4.0f),
                selection,
                Gap(4.0f),
                focus,
                Gap(4.0f),
                counts,
                Gap(12.0f),
                new TextInput("hunter2").Password().HostAutofill("current-password").Width(260.0f).NodeId("text-input-password"),
                Gap(8.0f),
                new TextInput("Read-only text").ReadOnly().Width(260.0f).NodeId("text-input-readonly"),
                Gap(8.0f),
                new TextInput("Disabled text").Enabled(false).Width(260.0f).NodeId("text-input-disabled"),
                Gap(8.0f),
                new TextInput().Placeholder("Custom colors")
                    .Colors(new TextInputColors { Background = 0x0F172AFFu, TextPrimary = 0xF8FAFCFFu, Placeholder = 0x94A3B8FFu, Caret = 0x22C55EFFu, Border = 0x22C55EFFu })
                    .Width(260.0f)
                    .NodeId("text-input-colored"))
            .AlignItems(AlignItems.Start);
    }

    private static string Format(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
