using System;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// The Stage 2 route: a card that combines layout, text, rich text, an image, an SVG, a scroll list, a custom-drawn
/// gauge, theme switching and asset readiness.
/// </summary>
internal static class ShowcasePage
{
    public static Node Build()
    {
        var gauge = new Gauge();
        gauge.NodeId("gauge").Width(160.0f).Height(100.0f);
        var bump = Button("bump", "Raise the gauge");
        bump.OnPointerClick(_ => gauge.Value = gauge.Value >= 1.0f ? 0.0f : gauge.Value + 0.25f);

        var theme = Button("theme-toggle", "Switch theme");
        theme.OnPointerClick(_ => Theme.UseCustom(Theme.IsDarkMode ? Theme.DefaultLight : Theme.DefaultDark));

        var assets = new Text("Loading assets").FontSize(14.0f).NodeId("asset-state");
        var loaded = 0;
        var image = Image.Load(ImagePage.Halves).NodeId("showcase-image").Width(120.0f);
        var vector = Svg.Load(ImagePage.Block, LayoutPage.Green).NodeId("showcase-svg").Height(60.0f);
        void Ready(AssetLoadState state)
        {
            if (state == AssetLoadState.Ready && ++loaded == 2)
            {
                assets.Text = "Image and SVG ready";
            }
        }
        image.OnAssetStateChanged(Ready);
        vector.OnAssetStateChanged(Ready);

        var list = FlexBox.Column();
        for (var index = 1; index <= 20; ++index)
        {
            list.Child(new FlexBox().Padding(8.0f, 6.0f).Child(new Text("Row " + index.ToString()).FontSize(16.0f)));
        }

        var card = FlexBox.Column(
                new Text("Stage 2 in C#").FontSize(28.0f).FontWeight(FontWeight.Bold).NodeId("title"),
                new RichText(
                        new Span("Retained nodes, "),
                        new Span("rich text").Bold().Color(0x2563EBFF),
                        new Span(", images, scrolling and "),
                        new Span("custom drawing").Italic().Background(0xFDE04780))
                    .FontSize(16.0f)
                    .NodeId("intro"),
                FlexBox.Row(image, Spacer(), vector, Spacer(), FlexBox.Column(gauge, bump)).AlignItems(AlignItems.Center),
                assets,
                new ScrollView().NodeId("showcase-list").Height(140.0f).FillWidth().HorizontalScrollEnabled(false).Child(list),
                theme)
            .NodeId("showcase-card")
            .Padding(24.0f)
            .Width(520.0f);

        return FlexBox.Column(card).Padding(20.0f).AlignItems(AlignItems.Start);
    }

    private static FlexBox Spacer() => new FlexBox().Width(24.0f);

    private static FlexBox Button(string id, string label) =>
        new FlexBox()
            .NodeId(id)
            .Padding(12.0f, 6.0f)
            .Margin(0.0f, 8.0f, 0.0f, 0.0f)
            .CornerRadius(6.0f)
            .Background(LayoutPage.Blue)
            .Cursor(CursorStyle.Pointer)
            .SemanticRole(SemanticRole.Button)
            .SemanticLabel(label)
            .Child(new Text(label).FontSize(14.0f).TextColor(0xFFFFFFFF));

    /// <summary>
    /// A half-circle gauge whose filled arc follows <see cref="Value"/> from 0 to 1. Its paths live as long as the
    /// gauge: the host draws them after <see cref="OnRender"/> returns.
    /// </summary>
    private sealed class Gauge : CustomDrawable
    {
        private const float CenterX = 80.0f;
        private const float CenterY = 90.0f;
        private const float Radius = 70.0f;

        private readonly PathGeometry track = Arc(1.0f);
        private PathGeometry filled = Arc(0.25f);
        private float value = 0.25f;

        public float Value
        {
            get => value;
            set
            {
                this.value = Math.Clamp(value, 0.0f, 1.0f);
                filled.Dispose();
                filled = Arc(this.value);
                InvalidateVisual();
            }
        }

        public override void Dispose()
        {
            track.Dispose();
            filled.Dispose();
            base.Dispose();
        }

        protected override void OnRender(DrawContext context)
        {
            context.DrawPath(track, Paint.Stroke(0x94A3B8FF, 12.0f));
            if (value > 0.0f)
            {
                context.DrawPath(filled, Paint.Stroke(0x16A34AFF, 12.0f));
            }
        }

        /// <summary>The upper half-circle from its left end, swept through <paramref name="fraction"/> of it.</summary>
        private static PathGeometry Arc(float fraction)
        {
            const int steps = 32;
            var path = new PathGeometry().MoveTo(CenterX - Radius, CenterY);
            var count = Math.Max(1, (int)MathF.Ceiling(steps * fraction));
            for (var step = 1; step <= count; ++step)
            {
                var angle = MathF.PI * (1.0f - (fraction * step / count));
                path.LineTo(CenterX + (Radius * MathF.Cos(angle)), CenterY - (Radius * MathF.Sin(angle)));
            }
            return path;
        }
    }
}
