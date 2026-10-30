using FuiCs;

namespace FuiCs.Browser;

/// <summary>A custom-drawn node that draws every primitive at known positions, read back pixel by pixel by the drawing spec.</summary>
internal static class DrawingPage
{
    public static readonly Color Red = 0xDC2626FF;
    public static readonly Color Purple = 0x7C3AEDFF;
    public static readonly Color Green = 0x16A34AFF;
    public static readonly Color Blue = 0x2563EBFF;
    public static readonly Color Orange = 0xF97316FF;
    public static readonly Color Black = 0x000000FF;
    public static readonly Color Yellow = 0xFDE047FF;
    public static readonly Color Rose = 0xE11D48FF;

    public static Node Build()
    {
        var sketch = new Sketch();
        sketch.NodeId("sketch").Width(400.0f).Height(300.0f).Cursor(CursorStyle.Pointer);
        sketch.OnPointerClick(_ => sketch.ToggleColor());
        return FlexBox.Column(sketch, new FlexBox().Height(20.0f), PatternImage()).Padding(20.0f).AlignItems(AlignItems.Start);
    }

    /// <summary>A bitmap drawn through its canvas, committed once and shown as an image.</summary>
    private static Image PatternImage()
    {
        var bitmap = new Bitmap(60, 60);
        bitmap.Canvas.DrawRect(0.0f, 0.0f, 60.0f, 60.0f, Paint.Fill(Yellow));
        bitmap.Canvas.DrawCircle(30.0f, 30.0f, 20.0f, Paint.Fill(Purple));
        var image = new Image(bitmap.Commit()).NodeId("pattern");
        image.Width = 60.0f;
        return image;
    }

    private sealed class Sketch : CustomDrawable
    {
        private readonly PathGeometry triangle = new PathGeometry().MoveTo(130.0f, 150.0f).LineTo(190.0f, 90.0f).LineTo(250.0f, 150.0f).Close();
        private readonly TextLayout label;
        private readonly Image texture;
        private readonly Svg vector;
        private Color rectangle = Red;

        public Sketch()
        {
            label = new TextLayout("Drawn text").FontSize(20.0f).TextColor(Black).OnReady(_ => InvalidateVisual());
            texture = Image.Load(ImagePage.Halves).OnAssetStateChanged(_ => InvalidateVisual());
            vector = Svg.Load(ImagePage.Block).OnAssetStateChanged(_ => InvalidateVisual());
        }

        public void ToggleColor()
        {
            rectangle = rectangle == Red ? Purple : Red;
            InvalidateVisual();
        }

        protected override void OnRender(DrawContext context)
        {
            context.DrawRect(0.0f, 0.0f, 400.0f, 300.0f, Paint.Fill(0xFFFFFFFF));
            context.DrawRect(10.0f, 10.0f, 100.0f, 60.0f, Paint.Fill(rectangle));
            context.DrawCircle(170.0f, 40.0f, 30.0f, Paint.Fill(Green));
            context.DrawLine(220.0f, 10.0f, 320.0f, 70.0f, Blue, 4.0f);
            context.DrawRoundRect(10.0f, 90.0f, 100.0f, 60.0f, 12.0f, 12.0f, Paint.FilledStroke(Orange, Black, 4.0f));
            context.DrawPath(triangle, Paint.Fill(Red));
            context.Save();
            context.Translate(320.0f, 120.0f);
            context.Rotate(45.0f);
            context.DrawRect(-15.0f, -15.0f, 30.0f, 30.0f, Paint.Fill(Blue));
            context.Restore();
            // A square and a circle cut by a rounded clip.
            context.Save();
            context.ClipRoundRect(220.0f, 170.0f, 80.0f, 80.0f, 16.0f);
            context.DrawRect(210.0f, 160.0f, 100.0f, 100.0f, Paint.Fill(Yellow));
            context.DrawCircle(300.0f, 250.0f, 40.0f, Paint.Fill(Rose));
            context.Restore();
            if (label.IsReady)
            {
                context.DrawTextLayout(label, 10.0f, 170.0f);
            }
            if (texture.AssetState == AssetLoadState.Ready)
            {
                context.DrawImage(texture.TextureId, 10.0f, 220.0f, 80.0f, 40.0f, ImageSampling.Nearest);
            }
            if (vector.AssetState == AssetLoadState.Ready)
            {
                context.DrawSvg(vector.SvgId, 120.0f, 220.0f, 60.0f, 30.0f);
            }
        }
    }
}
