using FuiCs;

namespace FuiCs.Browser;

/// <summary>A scroll view over a tall list: wheel and programmatic scrolling, and the extent and viewport the host reports.</summary>
internal static class ScrollPage
{
    public const int ItemCount = 30;
    public const float ItemHeight = 40.0f;

    public static Node Build()
    {
        var offsetLog = Status("offset-log", "0");
        var extentLog = Status("extent-log", "0");
        var list = FlexBox.Column().NodeId("scroll-content");
        for (var index = 0; index < ItemCount; ++index)
        {
            list.Child(new FlexBox()
                .NodeId("item-" + index.ToString())
                .Width(260.0f)
                .Height(ItemHeight)
                .Background(index % 2 == 0 ? LayoutPage.Blue : LayoutPage.Green));
        }
        var scroll = new ScrollView()
            .NodeId("scroll")
            .Width(300.0f)
            .Height(200.0f)
            .HorizontalScrollEnabled(false)
            .OnScrollChanged(e =>
            {
                offsetLog.Text = ((int)e.VerticalOffset).ToString();
                extentLog.Text = ((int)e.ExtentHeight).ToString() + "/" + ((int)e.ViewportHeight).ToString();
            })
            .Child(list);

        var jump = new FlexBox().NodeId("jump").Width(80.0f).Height(30.0f).Background(LayoutPage.Red).Cursor(CursorStyle.Pointer);
        jump.OnPointerClick(_ => scroll.ScrollTo(0.0f, 400.0f));

        return FlexBox.Column(scroll, new FlexBox().Height(10.0f), jump, offsetLog, extentLog)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static Text Status(string id, string initial) => new Text(initial).FontSize(14.0f).NodeId(id);
}
