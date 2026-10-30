namespace FuiCs;

/// <summary>How a shape is filled and stroked. A transparent color draws nothing for that part.</summary>
public readonly record struct Paint(Color FillColor, Color StrokeColor = default, float StrokeWidth = 0.0f)
{
    /// <summary>A fill with <paramref name="color"/>.</summary>
    public static Paint Fill(Color color) => new(color);

    /// <summary>An outline <paramref name="width"/> pixels wide.</summary>
    public static Paint Stroke(Color color, float width) => new(Color.Transparent, color, width);

    /// <summary>A fill with an outline.</summary>
    public static Paint FilledStroke(Color fill, Color stroke, float strokeWidth) => new(fill, stroke, strokeWidth);

    /// <summary>Whether the paint fills.</summary>
    public bool HasFill => FillColor.A != 0;

    /// <summary>Whether the paint outlines.</summary>
    public bool HasStroke => StrokeWidth > 0.0f && StrokeColor.A != 0;
}
