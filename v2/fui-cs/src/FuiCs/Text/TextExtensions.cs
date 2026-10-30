namespace FuiCs;

/// <summary>Fluent configuration for text nodes, text layouts and rich-text spans.</summary>
public static class TextExtensions
{
    extension<TText>(TText text) where TText : class, ITextFormat
    {
        /// <summary>Sets <see cref="TextCore.Text"/>.</summary>
        public TText Text(string value)
        {
            text.Text = value;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.FontFamily"/>.</summary>
        public TText FontFamily(FontFamily family)
        {
            text.FontFamily = family;
            return text;
        }

        /// <summary>
        /// Renders with <paramref name="stack"/> at <paramref name="size"/> pixels instead of a family.
        /// </summary>
        public TText FontStack(FontStack stack, float size)
        {
            text.SetFontStack(stack, size);
            return text;
        }

        /// <summary>Sets <see cref="TextCore.FontWeight"/>.</summary>
        public TText FontWeight(FontWeight weight)
        {
            text.FontWeight = weight;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.FontStyle"/>.</summary>
        public TText FontStyle(FontStyle style)
        {
            text.FontStyle = style;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.FontSize"/>.</summary>
        public TText FontSize(float size)
        {
            text.FontSize = size;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.LineHeight"/>.</summary>
        public TText LineHeight(float pixels)
        {
            text.LineHeight = pixels;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.TextColor"/>.</summary>
        public TText TextColor(Color color)
        {
            text.TextColor = color;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.TextAlign"/>.</summary>
        public TText TextAlign(TextAlign align)
        {
            text.TextAlign = align;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.VerticalAlign"/>.</summary>
        public TText VerticalAlign(TextVerticalAlign align)
        {
            text.VerticalAlign = align;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.MaxLines"/>.</summary>
        public TText MaxLines(int lines)
        {
            text.MaxLines = lines;
            return text;
        }

        /// <summary>
        /// Limits the text to <paramref name="maxChars"/> characters and <paramref name="maxLines"/> lines (0 for no
        /// line limit).
        /// </summary>
        public TText TextLimits(int maxChars, int maxLines)
        {
            text.SetTextLimits(maxChars, maxLines);
            return text;
        }

        /// <summary>Sets <see cref="TextCore.Wrapping"/>.</summary>
        public TText Wrapping(bool wrap = true)
        {
            text.Wrapping = wrap;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.Overflow"/>.</summary>
        public TText Overflow(TextOverflow overflow)
        {
            text.Overflow = overflow;
            return text;
        }
    }

    extension<TText>(TText text) where TText : TextCore
    {
        /// <summary>Sets <see cref="TextCore.Obscured"/>.</summary>
        public TText Obscured(bool obscured = true)
        {
            text.Obscured = obscured;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.Editable"/>.</summary>
        public TText Editable(bool editable = true)
        {
            text.Editable = editable;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.CaretColor"/>.</summary>
        public TText CaretColor(Color color)
        {
            text.CaretColor = color;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.Selectable"/>.</summary>
        public TText Selectable(bool selectable = true)
        {
            text.Selectable = selectable;
            return text;
        }

        /// <summary>Sets <see cref="TextCore.SelectionColor"/>.</summary>
        public TText SelectionColor(Color color)
        {
            text.SelectionColor = color;
            return text;
        }

        /// <summary>Adds a handler to <see cref="TextCore.TextChanged"/>.</summary>
        public TText OnTextChanged(Action<TextChangedEventArgs> handler)
        {
            text.TextChanged += handler;
            return text;
        }

        /// <summary>Adds a handler to <see cref="TextCore.SelectionChanged"/>.</summary>
        public TText OnSelectionChanged(Action<SelectionChangedEventArgs> handler)
        {
            text.SelectionChanged += handler;
            return text;
    
    }
    }

    extension<TText>(TText text) where TText : Text
    {
        /// <summary>Fades text that overflows its box horizontally and/or vertically.</summary>
        public TText OverflowFade(bool horizontal = true, bool vertical = false)
        {
            text.SetOverflowFade(horizontal, vertical);
            return text;
        }
    }

    extension<TLayout>(TLayout layout) where TLayout : TextLayout
    {
        /// <summary>Runs <paramref name="callback"/> once the page has loaded and the layout is prepared with its fonts.</summary>
        public TLayout OnReady(Action<TextLayoutReadyEventArgs> callback)
        {
            layout.WhenReady(callback);
            return layout;
        }
    }

    extension<TLayout>(TLayout layout) where TLayout : DynamicTextLayout
    {
        /// <summary>Sets how the layout handles characters its prepared charset lacks.</summary>
        public TLayout CharsetOverflow(DynamicTextOverflow overflow)
        {
            layout.CharsetOverflow = overflow;
            return layout;
        }

        /// <summary>Sets how many decimal places numbers show.</summary>
        public TLayout Precision(int digits)
        {
            layout.Precision = digits;
            return layout;
        }

        /// <summary>Sets text shown before the value.</summary>
        public TLayout Prefix(string prefix)
        {
            layout.Prefix = prefix;
            return layout;
        }

        /// <summary>Sets text shown after the value.</summary>
        public TLayout Suffix(string suffix)
        {
            layout.Suffix = suffix;
            return layout;
        }
    }

    extension<TRich>(TRich richText) where TRich : RichText
    {
        /// <summary>Replaces the rich text's spans.</summary>
        public TRich Spans(params ReadOnlySpan<Span> spans)
        {
            richText.SetSpans(spans);
            return richText;
        }

        /// <summary>Appends a span.</summary>
        public TRich Append(Span span)
        {
            richText.AddSpan(span);
            return richText;
        }
    }

    extension(Span span)
    {
        /// <summary>Colors the span's text.</summary>
        public Span Color(Color color)
        {
            span.Color = color;
            return span;
        }

        /// <summary>Fills behind the span's text.</summary>
        public Span Background(Color color)
        {
            span.Background = color;
            return span;
        }

        /// <summary>Underlines the span.</summary>
        public Span Underline()
        {
            span.Underline = true;
            return span;
        }

        /// <summary>Strikes through the span.</summary>
        public Span Strikethrough()
        {
            span.Strikethrough = true;
            return span;
        }

        /// <summary>Sets the span's font family.</summary>
        public Span FontFamily(FontFamily family)
        {
            span.FontFamily = family;
            return span;
        }

        /// <summary>Sets the span's font size in pixels.</summary>
        public Span FontSize(float size)
        {
            span.FontSize = size;
            return span;
        }

        /// <summary>Sets the span's font weight.</summary>
        public Span FontWeight(FontWeight weight)
        {
            span.FontWeight = weight;
            return span;
        }

        /// <summary>Sets the span's font style.</summary>
        public Span FontStyle(FontStyle style)
        {
            span.FontStyle = style;
            return span;
        }

        /// <summary>Makes the span bold.</summary>
        public Span Bold() => span.FontWeight(FuiCs.FontWeight.Bold);

        /// <summary>Makes the span italic.</summary>
        public Span Italic() => span.FontStyle(FuiCs.FontStyle.Italic);
    }
}
