using System.Text;

namespace FuiCs;

/// <summary>A run of rich text. Unset properties inherit the <see cref="RichText"/>'s base style.</summary>
public sealed class Span(string text)
{
    /// <summary>The span's text.</summary>
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));

    /// <summary>The span's text color; null uses the rich text's.</summary>
    public Color? Color { get; set; }

    /// <summary>The fill behind the span; null for none.</summary>
    public Color? Background { get; set; }

    /// <summary>Underlines the span.</summary>
    public bool Underline { get; set; }

    /// <summary>Strikes through the span.</summary>
    public bool Strikethrough { get; set; }

    /// <summary>The span's font family; null uses the rich text's.</summary>
    public FontFamily? FontFamily { get; set; }

    /// <summary>The span's font size; null uses the rich text's.</summary>
    public float? FontSize { get; set; }

    /// <summary>The span's font weight; null uses the rich text's.</summary>
    public FontWeight? FontWeight { get; set; }

    /// <summary>The span's font style; null uses the rich text's.</summary>
    public FontStyle? FontStyle { get; set; }

    internal uint DecorationFlags => (Underline ? 1u : 0u) | (Strikethrough ? 2u : 0u);
}

/// <summary>Text made of styled <see cref="Span"/>s, sent to the host as one string with style runs.</summary>
public class RichText : Text
{
    private const int StyleRunWords = 7;

    private readonly List<Span> spans = [];
    private bool hasBaseFont;
    private FontFamily? baseFamily;
    private float? baseFontSize;
    private FontWeight? baseFontWeight;
    private FontStyle? baseFontStyle;
    private Color? baseColor;
    private Composition? applied;

    /// <summary>Rich text of <paramref name="spans"/>.</summary>
    public RichText(params ReadOnlySpan<Span> spans)
        : base("")
    {
        SetSpans(spans);
    }

    /// <summary>Rich text of one plain span.</summary>
    public static RichText FromText(string text) => new(new Span(text));

    /// <summary>The spans in order.</summary>
    public IReadOnlyList<Span> Spans => spans;

    /// <summary>Replaces the spans with a single unstyled span.</summary>
    public override string Text
    {
        get => base.Text;
        set => SetSpans(new Span(value));
    }

    /// <inheritdoc/>
    public override FontFamily? FontFamily
    {
        get => baseFamily;
        set
        {
            hasBaseFont = true;
            baseFamily = value;
            Rebuild();
        }
    }

    /// <inheritdoc/>
    public override FontWeight FontWeight
    {
        get => baseFontWeight ?? FuiCs.FontWeight.Regular;
        set
        {
            hasBaseFont = true;
            baseFontWeight = value;
            Rebuild();
        }
    }

    /// <inheritdoc/>
    public override FontStyle FontStyle
    {
        get => baseFontStyle ?? FuiCs.FontStyle.Normal;
        set
        {
            hasBaseFont = true;
            baseFontStyle = value;
            Rebuild();
        }
    }

    /// <inheritdoc/>
    public override float FontSize
    {
        get => baseFontSize ?? 0.0f;
        set
        {
            hasBaseFont = true;
            baseFontSize = value;
            Rebuild();
        }
    }

    /// <inheritdoc/>
    public override Color? TextColor
    {
        get => baseColor;
        set
        {
            baseColor = value;
            Rebuild();
        }
    }

    /// <summary>
    /// Spans without their own color, size or family take them from the theme, so the runs are rebuilt when the theme
    /// changes any of them. Only what changed is sent: resending unchanged text and runs would discard the shaping a
    /// detached node was prepared with for <see cref="Bitmap.Render(Node, float, float, float)"/>.
    /// </summary>
    protected override void OnThemeChanged(Theme theme)
    {
        base.OnThemeChanged(theme);
        Rebuild();
    }

    /// <summary>Replaces the spans.</summary>
    public void SetSpans(params ReadOnlySpan<Span> items)
    {
        spans.Clear();
        foreach (var span in items)
        {
            spans.Add(span ?? throw new ArgumentNullException(nameof(items)));
        }
        Rebuild();
    }

    /// <summary>Appends a span.</summary>
    public void AddSpan(Span span)
    {
        ArgumentNullException.ThrowIfNull(span);
        spans.Add(span);
        Rebuild();
    }

    /// <inheritdoc/>
    public override void SetFontStack(FontStack stack, float fontSize)
    {
        ArgumentNullException.ThrowIfNull(stack);
        hasBaseFont = true;
        baseFamily = FuiCs.FontFamily.WithRegular(stack);
        baseFontSize = fontSize;
        Rebuild();
    }

    internal override IReadOnlyList<uint> RequiredFontIds()
    {
        var fontIds = new List<uint>(base.RequiredFontIds());
        var (family, weight, style) = BaseFont();
        foreach (var span in spans)
        {
            FontRegistry.AddUnique(fontIds, (span.FontFamily ?? family).Resolve(span.FontWeight ?? weight, span.FontStyle ?? style));
        }
        return fontIds;
    }

    private (FontFamily Family, FontWeight Weight, FontStyle Style) BaseFont() => (
        baseFamily ?? Theme.Active.Value.Fonts.BodyFamily,
        baseFontWeight ?? FuiCs.FontWeight.Regular,
        baseFontStyle ?? FuiCs.FontStyle.Normal);

    private void Rebuild() => Apply(Compose());

    /// <summary>Sends the parts of <paramref name="composition"/> that differ from the last one applied.</summary>
    private void Apply(Composition composition)
    {
        var last = applied;
        applied = composition;
        if (composition.SetsFont && (last is null || !last.SetsFont || last.FontId != composition.FontId || !last.FontSize.Equals(composition.FontSize)))
        {
            SetFontId(composition.FontId, composition.FontSize);
        }
        if (last is null || last.Color != composition.Color)
        {
            base.TextColor = composition.Color;
        }
        // The host takes new text with its runs, so a text change resends both.
        var textChanged = last is null || last.Text != composition.Text;
        if (textChanged)
        {
            base.Text = composition.Text;
        }
        if (textChanged || !SameRuns(last!.Runs, composition.Runs))
        {
            SetTextStyleRuns(composition.Runs);
        }
    }

    private static bool SameRuns(uint[]? left, uint[]? right) =>
        left is null ? right is null : right is not null && left.AsSpan().SequenceEqual(right);

    /// <summary>The font, color, merged text and style runs the spans resolve to under the current theme.</summary>
    private Composition Compose()
    {
        var theme = Theme.Active.Value;
        var (family, weight, style) = BaseFont();
        var size = baseFontSize ?? theme.Fonts.SizeBody;
        var color = baseColor ?? theme.Colors.TextPrimary;
        var merged = new StringBuilder();
        foreach (var span in spans)
        {
            merged.Append(span.Text);
        }
        return new Composition(
            hasBaseFont || spans.Count > 0,
            family.Resolve(weight, style),
            size,
            color,
            merged.ToString(),
            spans.Count == 0 ? null : StyleRuns(family, weight, style, size, color));
    }

    private uint[] StyleRuns(FontFamily family, FontWeight weight, FontStyle style, float size, Color color)
    {
        var words = new uint[spans.Count * StyleRunWords];
        var byteOffset = 0u;
        for (var index = 0; index < spans.Count; ++index)
        {
            var span = spans[index];
            var start = byteOffset;
            var end = start + (uint)Encoding.UTF8.GetByteCount(span.Text);
            var fontId = (span.FontFamily ?? family).Resolve(span.FontWeight ?? weight, span.FontStyle ?? style);
            if (fontId == 0)
            {
                Logger.Warn("Typography", "A RichText span resolved font id 0; check the configured FontFamily or font id.");
            }
            var word = index * StyleRunWords;
            words[word] = start;
            words[word + 1] = end;
            words[word + 2] = fontId;
            words[word + 3] = BitConverter.SingleToUInt32Bits(span.FontSize ?? size);
            words[word + 4] = (span.Color ?? color).Rgba;
            words[word + 5] = (span.Background ?? FuiCs.Color.Transparent).Rgba;
            words[word + 6] = span.DecorationFlags;
            byteOffset = end;
        }
        return words;
    }

    private sealed record Composition(bool SetsFont, uint FontId, float FontSize, Color Color, string Text, uint[]? Runs);
}
