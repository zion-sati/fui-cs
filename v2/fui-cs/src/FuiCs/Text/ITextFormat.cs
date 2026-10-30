namespace FuiCs;

/// <summary>The text and its formatting, shared by text nodes and <see cref="TextLayout"/>; one set of fluent members serves both.</summary>
public interface ITextFormat
{
    /// <summary>The text.</summary>
    string Text { get; set; }

    /// <summary>The family whose closest face to <see cref="FontWeight"/> and <see cref="FontStyle"/> is used; null for the theme's body family.</summary>
    FontFamily? FontFamily { get; set; }

    /// <summary>The weight of the face to use from the family.</summary>
    FontWeight FontWeight { get; set; }

    /// <summary>The style of the face to use from the family.</summary>
    FontStyle FontStyle { get; set; }

    /// <summary>The font size in pixels.</summary>
    float FontSize { get; set; }

    /// <summary>The line height in pixels; null for the font's own.</summary>
    float? LineHeight { get; set; }

    /// <summary>The text color; null follows the theme's primary text color.</summary>
    Color? TextColor { get; set; }

    /// <summary>How lines align horizontally.</summary>
    TextAlign TextAlign { get; set; }

    /// <summary>How the text aligns vertically in its box.</summary>
    TextVerticalAlign VerticalAlign { get; set; }

    /// <summary>The most lines shown; 0 for no limit.</summary>
    int MaxLines { get; set; }

    /// <summary>Wraps lines at the box's width.</summary>
    bool Wrapping { get; set; }

    /// <summary>How text past the box or line limit ends.</summary>
    TextOverflow Overflow { get; set; }

    /// <summary>Uses <paramref name="stack"/>, with its fallbacks, at <paramref name="fontSize"/>.</summary>
    void SetFontStack(FontStack stack, float fontSize);

    /// <summary>
    /// Limits the text to <paramref name="maxChars"/> characters and <paramref name="maxLines"/> lines (0 for no line
    /// limit).
    /// </summary>
    void SetTextLimits(int maxChars, int maxLines);
}
