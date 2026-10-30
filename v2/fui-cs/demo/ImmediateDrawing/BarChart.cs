using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>Four bars, each with a live numeric label riding its top.</summary>
internal sealed class BarChart : CustomDrawable
{
    private static readonly Color[] BarColors =
    [
        DrawingColors.Rgba(58, 108, 197, 180),
        DrawingColors.Rgba(58, 197, 108, 180),
        DrawingColors.Rgba(197, 108, 58, 180),
        DrawingColors.Rgba(158, 58, 197, 180),
    ];

    private readonly float[] values = new float[4];
    private readonly TextLayout title;
    private readonly DynamicTextLayout[] labels;

    public BarChart()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Bar chart");
        labels =
        [
            PlotText.NumericLabel(this, DrawingColors.Rgba(220, 232, 255, 230)),
            PlotText.NumericLabel(this, DrawingColors.Rgba(220, 255, 232, 230)),
            PlotText.NumericLabel(this, DrawingColors.Rgba(255, 232, 220, 230)),
            PlotText.NumericLabel(this, DrawingColors.Rgba(245, 220, 255, 230)),
        ];
    }

    public void PushValues(float a, float b, float c, float d)
    {
        values[0] = a;
        values[1] = b;
        values[2] = c;
        values[3] = d;
        for (var index = 0; index < 4; index += 1)
        {
            labels[index].TrySetValue(values[index]);
        }
        InvalidateVisual();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        const float pad = 14.0f;
        const float barWidth = 48.0f;
        const float gap = 16.0f;
        // The tallest bar leaves room above it for the title and its label.
        const float headroom = 76.0f;
        var baseY = size - pad;
        var plotHeight = baseY - headroom;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        for (var index = 0; index < 4; index += 1)
        {
            var x = pad + ((barWidth + gap) * index);
            var height = (values[index] / 100.0f) * plotHeight;
            context.DrawRect(x, baseY - height, barWidth, height, Paint.Fill(BarColors[index]));
        }
        for (var index = 0; index < 4; index += 1)
        {
            var x = pad + ((barWidth + gap) * index);
            var height = (values[index] / 100.0f) * plotHeight;
            PlotText.DrawLabel(context, labels[index], x + 2.0f, baseY - height - 28.0f);
        }
    }
}
