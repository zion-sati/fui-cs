using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>Four phase-shifted sine traces whose amplitudes follow the pushed values; the third is highlighted.</summary>
internal sealed class Waveform : CustomDrawable
{
    private static readonly Color Line = DrawingColors.Rgba(58, 197, 158, 255);
    private static readonly Color Dim = DrawingColors.Rgba(58, 197, 158, 60);

    private readonly float[] values = new float[4];
    private readonly TextLayout title;

    public Waveform()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Waveform");
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
        const float pad = 14.0f;
        var mid = size / 2.0f;
        var amplitude = size * 0.33f;
        const int steps = 40;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        for (var trace = 0; trace < 4; trace += 1)
        {
            var phase = trace * 1.2f;
            var color = trace == 2 ? Line : Dim;
            var scale = values[trace] / 100.0f;
            for (var i = 1; i < steps; i += 1)
            {
                var x0 = pad + ((float)(i - 1) / (steps - 1) * (size - (pad * 2.0f)));
                var x1 = pad + ((float)i / (steps - 1) * (size - (pad * 2.0f)));
                var t0 = ((i - 1) * 0.3f) + phase;
                var t1 = (i * 0.3f) + phase;
                context.DrawLine(
                    x0,
                    mid + ((float)Math.Sin(t0) * amplitude * scale),
                    x1,
                    mid + ((float)Math.Sin(t1) * amplitude * scale),
                    color,
                    1.5f);
            }
        }
    }
}
