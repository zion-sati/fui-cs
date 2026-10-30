# TextExtensions

class in `FuiCs`

Fluent configuration for text nodes, text layouts and rich-text spans.

## Methods

- `Append(Span)`: Appends a span.
- `Background(Color)`: Fills behind the span's text.
- `Bold()`: Makes the span bold.
- `CaretColor(Color)`: Sets [`TextCore.CaretColor`](TextCore.md).
- `CharsetOverflow(DynamicTextOverflow)`: Sets how the layout handles characters its prepared charset lacks.
- `Color(Color)`: Colors the span's text.
- `Editable(bool)`: Sets [`TextCore.Editable`](TextCore.md).
- `FontFamily(FontFamily)`: Sets [`TextCore.FontFamily`](TextCore.md).
- `FontFamily(FontFamily)`: Sets the span's font family.
- `FontSize(float)`: Sets [`TextCore.FontSize`](TextCore.md).
- `FontSize(float)`: Sets the span's font size in pixels.
- `FontStack(FontStack, float)`: Renders with `stack` at `size` pixels instead of a family.
- `FontStyle(FontStyle)`: Sets [`TextCore.FontStyle`](TextCore.md).
- `FontStyle(FontStyle)`: Sets the span's font style.
- `FontWeight(FontWeight)`: Sets [`TextCore.FontWeight`](TextCore.md).
- `FontWeight(FontWeight)`: Sets the span's font weight.
- `Italic()`: Makes the span italic.
- `LineHeight(float)`: Sets [`TextCore.LineHeight`](TextCore.md).
- `MaxLines(int)`: Sets [`TextCore.MaxLines`](TextCore.md).
- `Obscured(bool)`: Sets [`TextCore.Obscured`](TextCore.md).
- `OnReady(Action<TextLayoutReadyEventArgs>)`: Runs `callback` once the page has loaded and the layout is prepared with its fonts.
- `OnSelectionChanged(Action<SelectionChangedEventArgs>)`: Adds a handler to [`TextCore.SelectionChanged`](TextCore.md).
- `OnTextChanged(Action<TextChangedEventArgs>)`: Adds a handler to [`TextCore.TextChanged`](TextCore.md).
- `Overflow(TextOverflow)`: Sets [`TextCore.Overflow`](TextCore.md).
- `OverflowFade(bool, bool)`: Fades text that overflows its box horizontally and/or vertically.
- `Precision(int)`: Sets how many decimal places numbers show.
- `Prefix(string)`: Sets text shown before the value.
- `Selectable(bool)`: Sets [`TextCore.Selectable`](TextCore.md).
- `SelectionColor(Color)`: Sets [`TextCore.SelectionColor`](TextCore.md).
- `Spans(ReadOnlySpan<Span>)`: Replaces the rich text's spans.
- `Strikethrough()`: Strikes through the span.
- `Suffix(string)`: Sets text shown after the value.
- `Text(string)`: Sets [`TextCore.Text`](TextCore.md).
- `TextAlign(TextAlign)`: Sets [`TextCore.TextAlign`](TextCore.md).
- `TextColor(Color)`: Sets [`TextCore.TextColor`](TextCore.md).
- `TextLimits(int, int)`: Limits the text to `maxChars` characters and `maxLines` lines (0 for no line limit).
- `Underline()`: Underlines the span.
- `VerticalAlign(TextVerticalAlign)`: Sets [`TextCore.VerticalAlign`](TextCore.md).
- `Wrapping(bool)`: Sets [`TextCore.Wrapping`](TextCore.md).
