using FuiCs;

namespace FuiCs.Browser;

/// <summary>Touch text selection: selectable text with a readout of its selection, a selection area and a link.</summary>
internal static class SelectionPage
{
    public static Node Build()
    {
        var textLog = new Text("none").FontSize(13.0f).NodeId("text-selection-log").Selectable(false);
        var paragraph = new Text("Press and hold a word in this sentence").FontSize(22.0f).NodeId("paragraph");
        // The text is ASCII, so its UTF-8 selection offsets are string offsets too.
        paragraph.SelectionChanged += e =>
        {
            var start = (int)System.Math.Min(e.Start, e.End);
            var length = (int)System.Math.Max(e.Start, e.End) - start;
            textLog.Text = length == 0 ? "none" : paragraph.Text.Substring(start, length);
        };

        var areaLog = new Text("none").FontSize(13.0f).NodeId("area-selection-log").Selectable(false);
        var area = new SelectionArea().NodeId("selection-area")
            .Child(new Text("Alpha bravo charlie").FontSize(22.0f).NodeId("area-first"))
            .Child(new Text("Delta echo foxtrot").FontSize(22.0f).NodeId("area-second"));
        area.SelectedTextChanged += e => areaLog.Text = e.Text.Length == 0 ? "none" : e.Text.Replace('\n', '|');

        var link = new NavLink("?page=selection#linked").NodeId("selection-link")
            .Child(new Text("A link to hold").FontSize(18.0f).TextColor(LayoutPage.Blue).Selectable(false));
        // A long press here offers the link's items and the image's, more than the toolbar's row holds.
        var imageLink = new NavLink("?page=selection#image").NodeId("selection-image-link")
            .Child(Image.Load(ImagePage.Halves).NodeId("selection-image").Width(80.0f));

        return FlexBox.Column(
                new FlexBox().Height(120.0f),
                paragraph,
                textLog,
                new FlexBox().Height(60.0f),
                area,
                areaLog,
                new FlexBox().Height(40.0f),
                link,
                new FlexBox().Height(20.0f),
                imageLink)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }
}
