namespace FuiCs;

/// <summary>An RGBA color packed as 0xRRGGBBAA, the layout the host expects. A <see cref="uint"/> converts implicitly.</summary>
public readonly record struct Color(uint Rgba)
{
    /// <summary>Fully transparent black.</summary>
    public static Color Transparent => default;

    /// <summary>The red channel.</summary>
    public byte R => (byte)(Rgba >> 24);

    /// <summary>The green channel.</summary>
    public byte G => (byte)(Rgba >> 16);

    /// <summary>The blue channel.</summary>
    public byte B => (byte)(Rgba >> 8);

    /// <summary>The alpha channel; 255 is opaque.</summary>
    public byte A => (byte)Rgba;

    /// <summary>A color from its channels.</summary>
    public static Color FromRgba(byte r, byte g, byte b, byte a) => new(((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a);

    /// <summary>An opaque color from its channels.</summary>
    public static Color FromRgb(byte r, byte g, byte b) => FromRgba(r, g, b, 0xFF);

    /// <summary>An opaque color from hue in degrees and saturation and lightness from 0 to 1.</summary>
    public static Color FromHsl(float hue, float saturation, float lightness)
    {
        var wrapped = hue % 360.0f;
        var h = (wrapped < 0.0f ? wrapped + 360.0f : wrapped) / 360.0f;
        var s = Math.Clamp(saturation, 0.0f, 1.0f);
        var l = Math.Clamp(lightness, 0.0f, 1.0f);
        if (s <= 0.0f)
        {
            var channel = (byte)(l * 255.0f);
            return FromRgb(channel, channel, channel);
        }
        var q = l < 0.5f ? l * (1.0f + s) : l + s - (l * s);
        var p = (2.0f * l) - q;
        return FromRgb(
            (byte)(Math.Clamp(HueToRgb(p, q, h + (1.0f / 3.0f)), 0.0f, 1.0f) * 255.0f),
            (byte)(Math.Clamp(HueToRgb(p, q, h), 0.0f, 1.0f) * 255.0f),
            (byte)(Math.Clamp(HueToRgb(p, q, h - (1.0f / 3.0f)), 0.0f, 1.0f) * 255.0f));
    }

    /// <summary>Blends each channel toward <paramref name="to"/> by <paramref name="amount"/>, from 0 (this color) to 1.</summary>
    public Color Mix(Color to, float amount) =>
        FromRgba(MixChannel(R, to.R, amount), MixChannel(G, to.G, amount), MixChannel(B, to.B, amount), MixChannel(A, to.A, amount));

    /// <summary>A color from a 0xRRGGBBAA value.</summary>
    public static implicit operator Color(uint rgba) => new(rgba);

    private static byte MixChannel(byte from, byte to, float amount)
    {
        var weight = Math.Clamp(amount, 0.0f, 1.0f);
        return (byte)MathF.Floor(from + ((to - from) * weight) + 0.5f);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0.0f)
        {
            t += 1.0f;
        }
        if (t > 1.0f)
        {
            t -= 1.0f;
        }
        if (t < 1.0f / 6.0f)
        {
            return p + ((q - p) * 6.0f * t);
        }
        if (t < 1.0f / 2.0f)
        {
            return q;
        }
        return t < 2.0f / 3.0f ? p + ((q - p) * ((2.0f / 3.0f) - t) * 6.0f) : p;
    }
}

/// <summary>A length in pixels or percent of the parent, or <see cref="Auto"/>. A <see cref="float"/> converts implicitly as pixels.</summary>
public readonly record struct Length(float Value, Unit Unit = Unit.Pixel)
{
    /// <summary>Sized by its content or the layout.</summary>
    public static Length Auto => new(0.0f, Unit.Auto);

    /// <summary>A length in pixels.</summary>
    public static Length Pixels(float value) => new(value, Unit.Pixel);

    /// <summary>A percentage of the parent's size.</summary>
    public static Length Percent(float value) => new(value, Unit.Percent);

    /// <summary>A length in pixels.</summary>
    public static implicit operator Length(float pixels) => new(pixels);
}

/// <summary>Space on each side of a box, as for padding and margin.</summary>
public readonly record struct Thickness(float Left, float Top, float Right, float Bottom)
{
    /// <summary>The same space on every side.</summary>
    public Thickness(float uniform)
        : this(uniform, uniform, uniform, uniform)
    {
    }

    /// <summary>The same space on the left and right, and on the top and bottom.</summary>
    public Thickness(float horizontal, float vertical)
        : this(horizontal, vertical, horizontal, vertical)
    {
    }
}

/// <summary>The radius of each corner of a box.</summary>
public readonly record struct CornerRadius(float TopLeft, float TopRight, float BottomRight, float BottomLeft)
{
    /// <summary>The same radius at every corner.</summary>
    public CornerRadius(float uniform)
        : this(uniform, uniform, uniform, uniform)
    {
    }
}

/// <summary>A box outline; dashed and dotted borders alternate <see cref="DashOn"/> drawn and <see cref="DashOff"/> skipped pixels.</summary>
public readonly record struct Border(float Width, Color Color, BorderStyle Style = BorderStyle.Solid, float DashOn = 0.0f, float DashOff = 0.0f)
{
    /// <summary>A solid border.</summary>
    public static Border Solid(float width, Color color) => new(width, color);

    /// <summary>A dashed border: <paramref name="dashOn"/> pixels drawn, <paramref name="dashOff"/> skipped.</summary>
    public static Border Dashed(float width, Color color, float dashOn, float dashOff) => new(width, color, BorderStyle.Dashed, dashOn, dashOff);

    /// <summary>A dotted border: <paramref name="dashOn"/> pixels drawn, <paramref name="dashOff"/> skipped.</summary>
    public static Border Dotted(float width, Color color, float dashOn, float dashOff) => new(width, color, BorderStyle.Dotted, dashOn, dashOff);
}

/// <summary>A shadow behind a box: its color, offset, blur and spread.</summary>
public readonly record struct DropShadow(Color Color, float OffsetX, float OffsetY, float BlurSigma, float Spread = 0.0f);

/// <summary>A color at a position along a gradient, from 0 to 1.</summary>
public readonly record struct GradientStop(float Offset, Color Color);

/// <summary>
/// A gradient from (<see cref="StartX"/>, <see cref="StartY"/>) to (<see cref="EndX"/>, <see cref="EndY"/>) through
/// <see cref="Stops"/>. The points are relative to the node's top-left corner, as CSS gradients and WPF's default
/// mapping are, and the stops carry their own alpha.
/// </summary>
public sealed class LinearGradient(float startX, float startY, float endX, float endY, IReadOnlyList<GradientStop> stops)
{
    /// <summary>The start point's x, from the node's left edge.</summary>
    public float StartX { get; } = startX;

    /// <summary>The start point's y, from the node's top edge.</summary>
    public float StartY { get; } = startY;

    /// <summary>The end point's x, from the node's left edge.</summary>
    public float EndX { get; } = endX;

    /// <summary>The end point's y, from the node's top edge.</summary>
    public float EndY { get; } = endY;

    /// <summary>The gradient's colors in order.</summary>
    public IReadOnlyList<GradientStop> Stops { get; } = stops;
}
