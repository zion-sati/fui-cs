using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>A virtual list of 100,000 rows, 32 px each, in a 320 px viewport, with its first visible index beside it.</summary>
internal static class VirtualListPage
{
    public static Node Build()
    {
        var status = new Text("first 0").FontSize(14.0f).NodeId("vlist-first").Selectable(false);
        var list = new VirtualList(100000, 32.0f)
            .OnBindItem(BindRow)
            .NodeId("vlist")
            .PersistScroll(false)
            .Width(300.0f)
            .Height(320.0f);
        list.ScrollState.OffsetY.Subscribe(_ => status.Text = "first " + list.FirstVisibleIndex.ToString(CultureInfo.InvariantCulture));
        return FlexBox.Row(list, new FlexBox().Width(16.0f), status).Padding(20.0f).AlignItems(AlignItems.Start);
    }

    private static void BindRow(FlexBox container, int index)
    {
        if (container.Children.Count == 0)
        {
            container.Padding(8.0f, 6.0f).Child(new Text().FontSize(14.0f));
        }
        var label = "Item " + index.ToString(CultureInfo.InvariantCulture);
        ((Text)container.Children[0]).Text = label;
        container.Background = (index & 1) == 0 ? 0xF1F5F9FFu : 0xE2E8F0FFu;
    }
}
