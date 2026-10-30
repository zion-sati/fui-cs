using System.Globalization;

using FuiCs;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// Visual style and responsive layout in one retained view: a gradient card with a shadow, a blurred glass panel,
/// accent swatches that re-theme the route, a row that becomes a column on narrow viewports, and a probe that reports
/// where it was clicked through the coordinate helpers.
/// </summary>
internal static class StyleShowcase
{
    /// <summary>Viewports narrower than this stack the responsive pair vertically.</summary>
    private const float NarrowWidth = 960.0f;

    public static Node Build()
    {
        var card = new FlexBox()
            .NodeId("gradient-card")
            .Width(260.0f)
            .Height(100.0f)
            .CornerRadius(16.0f)
            .Background(0x2563EBFF)
            .LinearGradient(0.0f, 0.0f, 260.0f, 0.0f, new GradientStop(0.0f, 0x2563EBFF), new GradientStop(1.0f, 0x16A34AFF))
            .DropShadow(0x0F172A66, 0.0f, 10.0f, 12.0f)
            .Padding(16.0f)
            .Child(new Text("Gradient, corners, shadow").FontSize(14.0f).TextColor(0xFFFFFFFF));

        var stripes = FlexBox.Row(Stripe(0xE11D48FF), Stripe(0xF59E0BFF), Stripe(0x2563EBFF), Stripe(0x16A34AFF)).Width(260.0f).Height(80.0f);
        var glass = new FlexBox()
            .NodeId("glass")
            .PositionAbsolute()
            .Position(30.0f, 20.0f)
            .Width(200.0f)
            .Height(40.0f)
            .CornerRadius(10.0f)
            .Background(0xFFFFFF55)
            .BackgroundBlur(10.0f)
            .Child(new Text("Background blur").FontSize(13.0f));
        var glassHost = new FlexBox().NodeId("glass-host").Width(260.0f).Height(80.0f).Child(stripes).Child(glass);

        var accentLog = new Text("accent default").FontSize(13.0f).NodeId("accent-log");
        var swatches = FlexBox.Row(
                Swatch("accent-green", 0x16A34AFF, "green", accentLog),
                Swatch("accent-purple", 0x7C3AEDFF, "purple", accentLog),
                Swatch("accent-orange", 0xEA580CFF, "orange", accentLog))
            .AlignItems(AlignItems.Center);
        var accentSample = new Button("Accent sample").NodeId("accent-sample");

        var pair = FlexBox.Row(
                new FlexBox().NodeId("pair-a").Width(120.0f).Height(40.0f).CornerRadius(8.0f).Background(0x94A3B8FF),
                new FlexBox().Width(12.0f).Height(12.0f),
                new FlexBox().NodeId("pair-b").Width(120.0f).Height(40.0f).CornerRadius(8.0f).Background(0x64748BFF))
            .NodeId("pair");
        Application.ViewportWidth.Subscribe(width =>
            pair.FlexDirection = width < NarrowWidth ? FlexDirection.Column : FlexDirection.Row);

        var probeLog = new Text("probe not clicked").FontSize(13.0f).NodeId("probe-log");
        var probe = new FlexBox()
            .NodeId("probe")
            .Width(160.0f)
            .Height(60.0f)
            .CornerRadius(8.0f)
            .Background(0xCBD5E1FF)
            .Cursor(CursorStyle.Move)
            .Child(new Text("Click to probe").FontSize(13.0f));
        probe.OnPointerClick(e =>
        {
            var local = probe.AbsoluteToLocalPosition(e.SceneX, e.SceneY);
            var back = probe.LocalToAbsolutePosition(local.X, local.Y);
            probeLog.Text = string.Create(
                CultureInfo.InvariantCulture,
                $"local {(int)local.X},{(int)local.Y} round trip {(back.X == e.SceneX && back.Y == e.SceneY ? "ok" : "off")}");
        });

        return FlexBox.Column(
                new Text("Style").FontSize(20.0f).FontWeight(FontWeight.Bold),
                Gap(12.0f),
                card,
                Gap(20.0f),
                glassHost,
                Gap(16.0f),
                swatches,
                Gap(8.0f),
                FlexBox.Row(accentSample, Gap(12.0f), accentLog).AlignItems(AlignItems.Center),
                Gap(16.0f),
                pair,
                Gap(16.0f),
                probe,
                Gap(6.0f),
                probeLog)
            .NodeId("style-showcase")
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Stripe(Color color) => new FlexBox().Width(65.0f).Height(80.0f).Background(color);

    private static Button Swatch(string id, Color color, string name, Text log) =>
        new Button(name).NodeId(id).Colors(new ButtonColors { Background = color }).Margin(0.0f, 0.0f, 8.0f, 0.0f).OnClick(_ =>
        {
            Theme.SetAccentColor(color);
            log.Text = "accent " + name;
        });

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
