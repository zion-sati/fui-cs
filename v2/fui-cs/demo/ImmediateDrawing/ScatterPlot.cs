using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>Four points mapped from values in -1 to 1, joined by faint lines and labelled with their coordinates.</summary>
internal sealed class ScatterPlot : CustomDrawable
{
    private static readonly Color[] DotColors =
    [
        DrawingColors.Rgba(58, 108, 197, 255),
        DrawingColors.Rgba(58, 197, 108, 255),
        DrawingColors.Rgba(197, 108, 58, 255),
        DrawingColors.Rgba(255, 180, 60, 255),
    ];

    private static readonly Color Joint = DrawingColors.Rgba(255, 255, 255, 30);

    private readonly float[] xs = new float[4];
    private readonly float[] ys = new float[4];
    private readonly TextLayout title;
    private readonly DynamicTextLayout[] labels;

    public ScatterPlot()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Scatter plot");
        labels =
        [
            PlotText.MonoLabel(this, DrawingColors.Rgba(220, 232, 255, 230)),
            PlotText.MonoLabel(this, DrawingColors.Rgba(220, 255, 232, 230)),
            PlotText.MonoLabel(this, DrawingColors.Rgba(255, 232, 220, 230)),
            PlotText.MonoLabel(this, DrawingColors.Rgba(255, 238, 190, 230)),
        ];
    }

    public void PushValues(float a, float b, float c, float d)
    {
        // Values in -1 to 1 map onto the card, inside a 30 px margin.
        var size = GetBounds().Width;
        const float pad = 30.0f;
        var scale = (size - (pad * 2.0f)) / 2.0f;
        var center = size / 2.0f;
        xs[0] = center + (a * scale);
        ys[0] = center + (b * scale);
        xs[1] = center + (c * scale);
        ys[1] = center + (a * scale * 0.7f);
        xs[2] = center + (b * scale);
        ys[2] = center + (d * scale);
        xs[3] = center + (d * scale);
        ys[3] = center + (c * scale * 0.7f);
        labels[0].TrySetText(PlotText.FormatTenths(a) + "," + PlotText.FormatTenths(b));
        labels[1].TrySetText(PlotText.FormatTenths(c) + "," + PlotText.FormatTenths(a * 0.7f));
        labels[2].TrySetText(PlotText.FormatTenths(b) + "," + PlotText.FormatTenths(d));
        labels[3].TrySetText(PlotText.FormatTenths(d) + "," + PlotText.FormatTenths(c * 0.7f));
        InvalidateVisual();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        for (var index = 0; index < 4; index += 1)
        {
            var next = (index + 1) % 4;
            context.DrawLine(xs[index], ys[index], xs[next], ys[next], Joint, 0.5f);
        }
        for (var index = 0; index < 4; index += 1)
        {
            context.DrawCircle(xs[index], ys[index], 6.0f, Paint.Fill(DotColors[index]));
        }
        for (var index = 0; index < 4; index += 1)
        {
            PlotText.DrawLabel(context, labels[index], xs[index] + 8.0f, ys[index] - 24.0f);
        }
    }
}
