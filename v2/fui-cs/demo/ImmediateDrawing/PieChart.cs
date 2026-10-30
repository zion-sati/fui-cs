using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>Four wedges sized by the pushed values, drawn as line fans from the center.</summary>
internal sealed class PieChart : CustomDrawable
{
    private static readonly Color[] WedgeColors =
    [
        DrawingColors.Rgba(58, 108, 197, 220),
        DrawingColors.Rgba(58, 197, 108, 220),
        DrawingColors.Rgba(197, 108, 58, 220),
        DrawingColors.Rgba(158, 58, 197, 220),
    ];

    private readonly float[] values = new float[4];
    private readonly TextLayout title;

    public PieChart()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Pie chart");
    }

    public void PushValues(float a, float b, float c, float d)
    {
        values[0] = a;
        values[1] = b;
        values[2] = c;
        values[3] = d;
        InvalidateVisual();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        var center = size / 2.0f;
        var radius = size * 0.4f;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        var total = values[0] + values[1] + values[2] + values[3];
        if (total <= 0.0f)
        {
            return;
        }
        var startAngle = -90.0f;
        const int steps = 10;
        for (var wedge = 0; wedge < 4; wedge += 1)
        {
            var sweep = (values[wedge] / total) * 360.0f;
            if (sweep < 1.0f)
            {
                startAngle += sweep;
                continue;
            }
            var step = sweep / steps;
            for (var i = 0; i < steps; i += 1)
            {
                var a0 = (startAngle + (step * i)) * Math.PI / 180.0;
                var a1 = (startAngle + (step * (i + 1))) * Math.PI / 180.0;
                var x1 = center + (radius * (float)Math.Cos(a0));
                var y1 = center + (radius * (float)Math.Sin(a0));
                var x2 = center + (radius * (float)Math.Cos(a1));
                var y2 = center + (radius * (float)Math.Sin(a1));
                context.DrawLine(center, center, x1, y1, WedgeColors[wedge], 1.0f);
                context.DrawLine(center, center, x2, y2, WedgeColors[wedge], 1.0f);
                context.DrawLine(x1, y1, x2, y2, WedgeColors[wedge], 1.0f);
            }
            startAngle += sweep;
        }
        context.DrawCircle(center, center, radius, Paint.Stroke(DrawingColors.Gray, 1.0f));
    }
}
