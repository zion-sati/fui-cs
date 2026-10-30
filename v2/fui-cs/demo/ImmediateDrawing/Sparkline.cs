using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>The last 80 pushed values as a line, oldest at the left.</summary>
internal sealed class Sparkline : CustomDrawable
{
    private const int Capacity = 80;
    private static readonly Color Line = DrawingColors.Rgba(255, 180, 60, 255);

    private readonly float[] history = new float[Capacity];
    private readonly TextLayout title;
    private int writePosition;
    private int length;

    public Sparkline()
    {
        this.Width(300.0f).Height(300.0f);
        title = PlotText.Title(this, "Sparkline");
    }

    public void Push(float value)
    {
        history[writePosition] = value;
        writePosition = (writePosition + 1) % Capacity;
        if (length < Capacity)
        {
            length += 1;
        }
        InvalidateVisual();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        const float pad = 14.0f;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        if (length < 2)
        {
            return;
        }
        var stepX = (size - (pad * 2.0f)) / (length - 1);
        for (var i = 1; i < length; i += 1)
        {
            var previous = (writePosition - length + i - 1 + Capacity) % Capacity;
            var current = (writePosition - length + i + Capacity) % Capacity;
            var x0 = pad + (stepX * (i - 1));
            var y0 = size - pad - ((history[previous] / 100.0f) * (size - (pad * 2.0f)));
            var x1 = pad + (stepX * i);
            var y1 = size - pad - ((history[current] / 100.0f) * (size - (pad * 2.0f)));
            context.DrawLine(x0, y0, x1, y1, Line, 2.0f);
        }
    }
}
