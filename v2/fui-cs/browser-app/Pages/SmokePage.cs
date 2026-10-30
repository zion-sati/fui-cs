using FuiCs;

namespace FuiCs.Browser;

internal static class SmokePage
{
    public static readonly Color Blue = 0x006CFFFF;
    public static readonly Color Orange = 0xFF8800FF;

    /// <summary>
    /// A box that changes color when clicked, above a row of text in the theme's colors. An orange underline takes
    /// the width a <see cref="TextLayout"/> measures for "left". The page scrolls, and the caption reports that it has.
    /// </summary>
    public static Node Build()
    {
        var underline = new FlexBox().Width(0.0f).Height(3.0f).Background(Orange);
        new TextLayout("left").FontSize(28.0f).OnReady(e => underline.Width(e.Layout.MeasuredWidth));
        var box = new FlexBox()
            .Width(120.0f)
            .Height(96.0f)
            .Background(Blue)
            .Cursor(CursorStyle.Pointer)
            .NodeId("smoke-box");
        box.OnPointerClick(_ => box.Background = box.Background == Blue ? Orange : Blue);
        var content = FlexBox.Column(
                box,
                new FlexBox().Height(24.0f),
                FlexBox.Row(
                    new Text("left").FontSize(28.0f),
                    new FlexBox().Width(32.0f).Height(1.0f),
                    new Text("right").FontSize(28.0f)),
                new FlexBox().Height(6.0f),
                underline,
                new FlexBox().Height(240.0f))
            .Padding(24.0f);
        return new ScrollView()
            .Width(100.0f, Unit.Percent)
            .Height(100.0f, Unit.Percent)
            .HorizontalScrollEnabled(false)
            .OnScrollChanged(e =>
            {
                if (e.VerticalOffset > 0.0f)
                {
                    Application.SetCaption("fui-cs smoke (scrolled)");
                }
            })
            .Child(content);
    }
}
