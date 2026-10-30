using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// ScrollBoxes in the real host: a list and a wide strip nested in a page box, readouts of their offsets, and a
/// selection area with a barrier. The scroll bar thumbs get node ids so specs can find them.
/// </summary>
internal static class ScrollBoxPage
{
    public const float RowHeight = 30.0f;
    public const int RowCount = 40;

    public static Node Build()
    {
        var listLog = Status("list-log");
        var rows = FlexBox.Column();
        for (var index = 1; index <= RowCount; ++index)
        {
            rows.Child(new FlexBox().NodeId("row-" + index.ToString(CultureInfo.InvariantCulture)).Height(RowHeight).Padding(8.0f, 4.0f)
                .Background(index % 2 == 0 ? 0xE2E8F0FF : 0xF8FAFCFF)
                .Child(new Text("Row " + index.ToString(CultureInfo.InvariantCulture)).FontSize(14.0f).Selectable(false)));
        }
        var list = new ScrollBox().NodeId("list-box").Width(300.0f).Height(200.0f).PersistScroll(false).Child(rows)
            .OnScrollChanged(e => listLog.Text = Describe(e));
        Tag(list.VerticalScrollBar, "list-thumb");

        var wideLog = Status("wide-log");
        var strip = FlexBox.Row().Height(60.0f);
        for (var index = 0; index < 10; ++index)
        {
            strip.Child(new FlexBox().Width(100.0f).Height(60.0f).Background(index % 2 == 0 ? LayoutPage.Blue : LayoutPage.Green));
        }
        var wide = new ScrollBox().NodeId("wide-box").Width(300.0f).Height(80.0f).PersistScroll(false)
            .VerticalScrollEnabled(false).Child(strip)
            .OnScrollChanged(e => wideLog.Text = Describe(e));
        Tag(wide.HorizontalScrollBar, "wide-thumb");

        var selectedLog = Status("selected-log");
        var area = new SelectionArea().NodeId("area")
            .Child(new Text("First selectable line").FontSize(16.0f).NodeId("first-line"))
            .Child(new Text("Second selectable line").FontSize(16.0f).NodeId("second-line"))
            .Child(new AntiSelectionArea().Child(new Text("Outside the selection").FontSize(16.0f).NodeId("shielded-line")));
        area.SelectedTextChanged += e => selectedLog.Text = e.Text.Length == 0 ? "none" : e.Text.Replace('\n', '|');

        var pageLog = Status("page-log");
        var content = FlexBox.Column(
                FlexBox.Row(list, Gap(), FlexBox.Column(listLog, wideLog, pageLog, selectedLog)),
                Gap(),
                wide,
                Gap(),
                area,
                new FlexBox().NodeId("page-filler").Height(700.0f))
            .Padding(20.0f);
        var page = new ScrollBox().NodeId("page-box").FillWidth().FillHeight().PersistScroll(false).Child(content)
            .OnScrollChanged(e => pageLog.Text = "page " + Describe(e));
        return page;
    }

    private static void Tag(ScrollBar bar, string thumbId) => bar.Root.Children[0].Children[1].NodeId(thumbId);

    private static string Describe(ScrollChangedEventArgs e) =>
        string.Create(CultureInfo.InvariantCulture, $"{e.HorizontalOffset:0},{e.VerticalOffset:0}");

    private static FlexBox Gap() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id) => new Text("0,0").FontSize(13.0f).NodeId(id).Selectable(false);
}
