using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>Tooltip owners for the harness debug hooks: a button and the first row of a scroll box.</summary>
internal static class ToolTipPage
{
    public static Node Build()
    {
        var rows = FlexBox.Column();
        for (var index = 1; index <= 12; ++index)
        {
            var row = new FlexBox().NodeId("tip-line-" + index.ToString(CultureInfo.InvariantCulture)).Height(24.0f).Padding(8.0f, 4.0f)
                .Child(new Text("Line " + index.ToString(CultureInfo.InvariantCulture)).FontSize(13.0f).Selectable(false));
            rows.Child(index == 1 ? row.ToolTip("The first line") : row);
        }
        return FlexBox.Column(
                new FlexBox().Height(80.0f),
                new Button("Save").NodeId("tip-button").ToolTip("Save the file"),
                new FlexBox().Height(40.0f),
                new ScrollBox().NodeId("tip-scroll").Width(200.0f).Height(96.0f).PersistScroll(false).Child(rows))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }
}
