using FuiCs;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>The media card's app-owned bitmap: a soft red disc crossed by a white diagonal, in premultiplied RGBA.</summary>
internal static class DashboardBitmap
{
    public const int Size = 96;

    public static Bitmap Create()
    {
        var bitmap = new Bitmap(Size, Size);
        var pixels = bitmap.Pixels;
        var center = (Size - 1.0f) * 0.5f;
        const float radiusSquared = 34.0f * 34.0f;
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                var dx = x - center;
                var dy = y - center;
                var distanceSquared = (dx * dx) + (dy * dy);
                if (distanceSquared <= radiusSquared)
                {
                    var falloff = 1.0f - (distanceSquared / radiusSquared);
                    SetPremultiplied(pixels, x, y, 0xff, 0x40, 0x40, (byte)(64.0f + (falloff * 144.0f)));
                }
                if (x is >= 20 and <= 76 && y is >= 20 and <= 76 && x + y is >= 93 and <= 99)
                {
                    SetPremultiplied(pixels, x, y, 0xff, 0xff, 0xff, 0xd8);
                }
            }
        }
        bitmap.Commit();
        return bitmap;
    }

    private static byte Premultiply(byte channel, byte alpha) => (byte)(((channel * alpha) + 127) / 255);

    private static void SetPremultiplied(byte[] pixels, int x, int y, byte red, byte green, byte blue, byte alpha)
    {
        var offset = (x + (y * Size)) * 4;
        pixels[offset] = Premultiply(red, alpha);
        pixels[offset + 1] = Premultiply(green, alpha);
        pixels[offset + 2] = Premultiply(blue, alpha);
        pixels[offset + 3] = alpha;
    }
}
