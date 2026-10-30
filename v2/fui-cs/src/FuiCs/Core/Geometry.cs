namespace FuiCs;

/// <summary>A point in scene pixels.</summary>
public readonly record struct Point(float X, float Y);

/// <summary>A rectangle in scene pixels.</summary>
public readonly record struct Rect(float X, float Y, float Width, float Height)
{
    /// <summary>Whether the point is inside the rectangle: left and top edges included, right and bottom excluded.</summary>
    public bool Contains(float x, float y) => x >= X && x < X + Width && y >= Y && y < Y + Height;
}

/// <summary>Measured text layout: size in pixels, first-line baseline, line count and widest line.</summary>
public readonly record struct TextMetrics(float Width, float Height, float Baseline, uint LineCount, float MaxLineWidth);
