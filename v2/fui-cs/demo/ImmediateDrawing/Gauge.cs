using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>A needle over a ring, at <see cref="Value"/> from 0 to 100 across a 270-degree sweep.</summary>
internal sealed class Gauge : CustomDrawable
{
    private readonly TextLayout title;

    public Gauge()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Gauge");
        Value.Changed += _ => InvalidateVisual();
    }

    public Signal<float> Value { get; } = new(0.0f);

    protected override void OnRender(DrawContext context)
    {
        var value = Value.Value;
        var size = GetBounds().Width;
        var center = size / 2.0f;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        context.DrawCircle(center, center, size * 0.4f, Paint.Stroke(DrawingColors.Gray, 6.0f));
        var angle = (((value / 100.0f) * 270.0f) - 135.0f) * MathF.PI / 180.0f;
        context.DrawLine(
            center,
            center,
            center + (size * 0.3f * (float)Math.Cos(angle)),
            center + (size * 0.3f * (float)Math.Sin(angle)),
            DrawingColors.Needle,
            3.0f);
        context.DrawCircle(center, center, 6.0f, Paint.Fill(DrawingColors.Accent));
    }
}
