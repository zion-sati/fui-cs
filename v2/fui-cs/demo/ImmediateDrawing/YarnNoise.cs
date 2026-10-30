using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>Smoothed 3D value noise from an integer hash, and the strand's warm-to-cool color ramp.</summary>
internal static class YarnNoise
{
    public static float Value(float x, float y, float z)
    {
        var ix = (int)Math.Floor(x);
        var iy = (int)Math.Floor(y);
        var iz = (int)Math.Floor(z);
        var ux = Fade(x - ix);
        var uy = Fade(y - iy);
        var uz = Fade(z - iz);

        var x00 = Lerp(Hash(ix, iy, iz), Hash(ix + 1, iy, iz), ux);
        var x10 = Lerp(Hash(ix, iy + 1, iz), Hash(ix + 1, iy + 1, iz), ux);
        var x01 = Lerp(Hash(ix, iy, iz + 1), Hash(ix + 1, iy, iz + 1), ux);
        var x11 = Lerp(Hash(ix, iy + 1, iz + 1), Hash(ix + 1, iy + 1, iz + 1), ux);
        return Lerp(Lerp(x00, x10, uy), Lerp(x01, x11, uy), uz);
    }

    public static Color Color(float t)
    {
        var wave = (float)Math.Sin(t * Math.PI * 2.0);
        var warm = Math.Clamp(0.5f + (wave * 0.5f), 0.0f, 1.0f);
        var cool = 1.0f - warm;
        return DrawingColors.Rgba(
            (byte)Math.Round((116.0 * cool) + (255.0 * warm)),
            (byte)Math.Round((222.0 * cool) + (132.0 * warm)),
            (byte)Math.Round((255.0 * cool) + (150.0 * warm)),
            230);
    }

    private static float Fade(float t) => t * t * t * ((t * ((t * 6.0f) - 15.0f)) + 10.0f);

    private static float Lerp(float a, float b, float t) => a + ((b - a) * t);

    private static float Hash(int ix, int iy, int iz)
    {
        // 32-bit wrapping arithmetic, as the hash was designed for.
        unchecked
        {
            var h = (ix * 374761393) + (iy * 668265263) + (iz * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0x7fffffff) / 2147483647.0f;
        }
    }
}
