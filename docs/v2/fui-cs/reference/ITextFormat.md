# ITextFormat

interface in `FuiCs`

The text and its formatting, shared by text nodes and [`TextLayout`](TextLayout.md); one set of fluent members serves both.

## Properties

- `FontFamily`: The family whose closest face to [`ITextFormat.FontWeight`](ITextFormat.md) and [`ITextFormat.FontStyle`](ITextFormat.md) is used; null for the theme's body family.
- `FontSize`: The font size in pixels.
- `FontStyle`: The style of the face to use from the family.
- `FontWeight`: The weight of the face to use from the family.
- `LineHeight`: The line height in pixels; null for the font's own.
- `MaxLines`: The most lines shown; 0 for no limit.
- `Overflow`: How text past the box or line limit ends.
- `Text`: The text.
- `TextAlign`: How lines align horizontally.
- `TextColor`: The text color; null follows the theme's primary text color.
- `VerticalAlign`: How the text aligns vertically in its box.
- `Wrapping`: Wraps lines at the box's width.

## Methods

- `SetFontStack(FontStack, float)`: Uses `stack`, with its fallbacks, at `fontSize`.
- `SetTextLimits(int, int)`: Limits the text to `maxChars` characters and `maxLines` lines (0 for no line limit).
