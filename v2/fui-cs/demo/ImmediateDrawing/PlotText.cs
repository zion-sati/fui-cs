using System;
using System.Globalization;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>A plot's title and its live labels: text layouts that repaint their plot once their fonts are ready.</summary>
internal static class PlotText
{
    private const string NumberCharset = "0123456789.-, ";

    public static TextLayout Title(CustomDrawable owner, string title) => new TextLayout(title)
        .FontFamily(Theme.Active.Value.Fonts.BodyFamily)
        .FontSize(13.0f)
        .TextColor(DrawingColors.Rgba(235, 238, 245, 210))
        .Width(180.0f)
        .Height(24.0f)
        .OnReady(_ => owner.InvalidateVisual());

    public static void DrawTitle(DrawContext context, TextLayout title)
    {
        if (title.IsReady)
        {
            context.DrawTextLayout(title, 16.0f, 24.0f);
        }
    }

    /// <summary>A label for text from a fixed set of characters, in the theme's monospace family.</summary>
    public static DynamicTextLayout MonoLabel(CustomDrawable owner, Color color) =>
        Label(owner, new DynamicTextLayout(NumberCharset), color, Theme.Active.Value.Fonts.MonoFamily);

    /// <summary>A label for whole numbers set with <see cref="DynamicTextLayout.TrySetValue"/>.</summary>
    public static DynamicTextLayout NumericLabel(CustomDrawable owner, Color color, int precision = 0) =>
        Label(owner, DynamicTextLayout.Numeric().Precision(precision), color, Theme.Active.Value.Fonts.BodyFamily);

    /// <summary>Draws the label on a dark, rounded backing sized to its text.</summary>
    public static void DrawLabel(DrawContext context, DynamicTextLayout label, float x, float y)
    {
        if (!label.IsReady)
        {
            return;
        }
        var width = label.Measure().Width + 10.0f;
        context.DrawRoundRect(x, y, width, 22.0f, 5.0f, 5.0f, Paint.Fill(DrawingColors.Rgba(8, 10, 18, 225)));
        context.DrawTextLayout(label, x + 5.0f, y + 4.0f);
    }

    /// <summary>The value rounded to tenths, always with one decimal digit.</summary>
    public static string FormatTenths(float value)
    {
        var scaled = (int)MathF.Round(value * 10.0f);
        var sign = "";
        if (scaled < 0)
        {
            sign = "-";
            scaled = -scaled;
        }
        return sign + (scaled / 10).ToString(CultureInfo.InvariantCulture) + "." + (scaled % 10).ToString(CultureInfo.InvariantCulture);
    }

    private static DynamicTextLayout Label(CustomDrawable owner, DynamicTextLayout label, Color color, FontFamily family) => label
        .FontFamily(family)
        .FontSize(12.0f)
        .TextColor(color)
        .Width(72.0f)
        .Height(20.0f)
        .OnReady(_ => owner.InvalidateVisual());
}
