using FuiCs;

namespace FuiCs.Browser;

/// <summary>Popups anchored below or above a button, shown at a clamped point, and dismissed from the backdrop.</summary>
internal static class PopupPage
{
    public const float SurfaceWidth = 200.0f;
    public const float SurfaceHeight = 100.0f;

    public static Node Build()
    {
        var popup = new Popup
        {
            BackdropColor = 0x0F172A40,
        };
        popup.Surface.NodeId("popup-surface").Background(LayoutPage.Grey).Padding(12.0f);
        popup.Child(new Text("Popup content").FontSize(16.0f).NodeId("popup-text"));

        var top = Button("anchor-top");
        top.OnPointerClick(_ => ShowAnchored(popup, top));
        var bottom = Button("anchor-bottom");
        bottom.OnPointerClick(_ => ShowAnchored(popup, bottom));
        var point = Button("point-button");
        point.OnPointerClick(_ => popup.ShowAtPoint(780.0f, 590.0f, SurfaceWidth, SurfaceHeight));

        return new FlexBox()
            .NodeId("popup-page")
            .Width(100.0f, Unit.Percent)
            .Height(100.0f, Unit.Percent)
            .Padding(20.0f)
            .FlexDirection(FlexDirection.Column)
            .JustifyContent(JustifyContent.Start)
            .AlignItems(AlignItems.Start)
            .Children(
                FlexBox.Row(top, new FlexBox().Width(20.0f), point),
                new FlexBox().Height(460.0f),
                bottom,
                popup);
    }

    private static void ShowAnchored(Popup popup, Node anchor)
    {
        var bounds = anchor.GetBounds();
        popup.ShowAnchored(bounds.X, bounds.Y, bounds.Width, bounds.Height, SurfaceWidth, SurfaceHeight);
    }

    private static FlexBox Button(string id) =>
        new FlexBox().NodeId(id).Width(120.0f).Height(40.0f).Background(LayoutPage.Blue).Cursor(CursorStyle.Pointer);
}
