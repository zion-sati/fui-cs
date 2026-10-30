using FuiCs;

namespace FuiCs.Browser;

/// <summary>FlexBox layout and styling, laid out by the host and read back by the layout spec.</summary>
internal static class LayoutPage
{
    public static readonly Color Red = 0xE11D48FF;
    public static readonly Color Blue = 0x2563EBFF;
    public static readonly Color Green = 0x16A34AFF;
    public static readonly Color Grey = 0xCBD5E1FF;

    public static Node Build()
    {
        var growing = Box("toggle-target", 100.0f, 30.0f, Green);
        var toggle = Box("toggle", 60.0f, 30.0f, Blue).Cursor(CursorStyle.Pointer);
        toggle.OnPointerClick(_ => growing.Width = growing.Width.Value == 100.0f ? Length.Pixels(200.0f) : Length.Pixels(100.0f));

        return FlexBox.Column(
                FlexBox.Row(
                        Box("fixed-a", 100.0f, 30.0f, Red),
                        Box("fixed-b", 60.0f, 30.0f, Blue).Margin(10.0f, 0.0f, 0.0f, 0.0f),
                        Box("fixed-c", 80.0f, 30.0f, Green))
                    .NodeId("fixed-row"),
                FlexBox.Row(
                        Box("fill-before", 100.0f, 30.0f, Red),
                        new FlexBox().NodeId("fill").FillWidth().Height(30.0f).Background(Blue),
                        Box("fill-after", 50.0f, 30.0f, Green))
                    .NodeId("fill-row")
                    .Width(400.0f),
                FlexBox.Row(
                        new FlexBox().NodeId("percent-25").FillWidthPercent(25.0f).Height(20.0f).Background(Red),
                        new FlexBox().NodeId("percent-75").FillWidthPercent(75.0f).Height(20.0f).Background(Blue))
                    .NodeId("percent-row")
                    .Width(400.0f),
                FlexBox.Row(Box("centered", 100.0f, 20.0f, Green))
                    .NodeId("center-row")
                    .Width(400.0f)
                    .JustifyContent(JustifyContent.Center)
                    .Background(Grey),
                FlexBox.Column(Box("end-aligned", 50.0f, 20.0f, Red))
                    .NodeId("end-column")
                    .Width(200.0f)
                    .AlignItems(AlignItems.End)
                    .Background(Grey),
                FlexBox.Row(
                        Box("min-clamped", 50.0f, 20.0f, Red).MinWidth(80.0f),
                        Box("max-clamped", 300.0f, 20.0f, Blue).MaxWidth(120.0f))
                    .NodeId("clamp-row"),
                FlexBox.Row(
                        Box("wrap-1", 60.0f, 20.0f, Red),
                        Box("wrap-2", 60.0f, 20.0f, Blue),
                        Box("wrap-3", 60.0f, 20.0f, Green),
                        Box("wrap-4", 60.0f, 20.0f, Red),
                        Box("wrap-5", 60.0f, 20.0f, Blue))
                    .NodeId("wrap-row")
                    .Width(200.0f)
                    .FlexWrap(FlexWrap.Wrap),
                FlexBox.Row(
                        Box("collapse-a", 50.0f, 20.0f, Red),
                        Box("collapse-b", 50.0f, 20.0f, Blue).Visibility(Visibility.Collapsed),
                        Box("collapse-c", 50.0f, 20.0f, Green))
                    .NodeId("collapse-row"),
                FlexBox.Row(
                        new FlexBox()
                            .NodeId("absolute-host")
                            .Width(200.0f)
                            .Height(60.0f)
                            .Background(Grey)
                            .Padding(8.0f)
                            .Child(Box("absolute-child", 20.0f, 20.0f, Red).PositionAbsolute().Position(30.0f, 25.0f)),
                        new FlexBox()
                            .NodeId("clip-host")
                            .Width(60.0f)
                            .Height(60.0f)
                            .Margin(10.0f, 0.0f, 0.0f, 0.0f)
                            .ClipToBounds(true)
                            .Child(Box("clipped", 100.0f, 100.0f, Blue)),
                        new FlexBox()
                            .NodeId("styled")
                            .Width(80.0f)
                            .Height(60.0f)
                            .Margin(10.0f, 0.0f, 0.0f, 0.0f)
                            .Background(Red)
                            .Border(6.0f, Blue)
                            .CornerRadius(8.0f),
                        new FlexBox()
                            .NodeId("padded")
                            .AlignSelf(AlignSelf.Start)
                            .Margin(10.0f, 0.0f, 0.0f, 0.0f)
                            .Padding(12.0f, 6.0f)
                            .Background(Grey)
                            .Child(Box("padded-child", 30.0f, 30.0f, Green)),
                        new FlexBox().NodeId("stretched").Width(20.0f).Margin(10.0f, 0.0f, 0.0f, 0.0f).Background(Green))
                    .NodeId("box-row"),
                FlexBox.Row(growing, toggle).NodeId("toggle-row"),
                FlexBox.Row(
                        new FlexBox().NodeId("faded").Width(40.0f).Height(40.0f).Background(Grey).Opacity(0.5f)
                            .Child(Box("faded-child", 20.0f, 20.0f, Blue)))
                    .NodeId("faded-row"))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Box(string id, float width, float height, Color color) =>
        new FlexBox().NodeId(id).Width(width).Height(height).Background(color);
}
