using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>The route's palette: every plot sits on a dark card on the dark page.</summary>
internal static class DrawingColors
{
    public static readonly Color Accent = Rgba(58, 108, 197, 255);
    public static readonly Color Gray = Rgba(200, 200, 200, 255);
    public static readonly Color Needle = Rgba(220, 50, 50, 255);
    public static readonly Color Card = Rgba(35, 35, 50, 255);
    public static readonly Color Page = Rgba(20, 20, 40, 255);

    public static Color Rgba(byte r, byte g, byte b, byte a) => Color.FromRgba(r, g, b, a);
}
