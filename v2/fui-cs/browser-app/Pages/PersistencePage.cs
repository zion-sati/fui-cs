using FuiCs;

namespace FuiCs.Browser;

/// <summary>A counter and a scroll list whose state the host saves with the history entry and restores on return.</summary>
internal static class PersistencePage
{
    public static Node Build()
    {
        var count = 0;
        var counter = new Text("0").FontSize(20.0f).NodeId("counter");
        counter.PersistState(new PersistedValue<Text, int>(
            "count",
            _ => count,
            (text, value) =>
            {
                count = value;
                text.Text = value.ToString();
            },
            PersistedCodecs.Int32));
        var increment = new FlexBox().NodeId("increment").Width(80.0f).Height(30.0f).Background(LayoutPage.Blue).Cursor(CursorStyle.Pointer);
        increment.OnPointerClick(_ => counter.Text = (++count).ToString());

        var offset = new Text("0").FontSize(14.0f).NodeId("offset-log");
        var list = FlexBox.Column();
        for (var index = 0; index < 30; ++index)
        {
            list.Child(new FlexBox().Width(200.0f).Height(40.0f).Background(index % 2 == 0 ? LayoutPage.Blue : LayoutPage.Green));
        }
        var scroll = new ScrollView()
            .NodeId("persist-list")
            .Width(240.0f)
            .Height(200.0f)
            .HorizontalScrollEnabled(false)
            .OnScrollChanged(e => offset.Text = ((int)e.VerticalOffset).ToString())
            .Child(list);

        return FlexBox.Column(counter, increment, scroll, offset).Padding(20.0f).AlignItems(AlignItems.Start);
    }
}
