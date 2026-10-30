# TextLayout

class in `FuiCs`: [`ITextFormat`](ITextFormat.md), [`ISizable`](ISizable.md), `IDisposable`

Formatted text for drawing and measuring outside the page tree. The host shapes it like a [`Text`](Text.md) node, with the same fonts, fallbacks and rich-text runs. It can be measured once [`TextLayout.IsReady`](TextLayout.md), which the fluent `OnReady` callback reports; any change makes it unready until it is prepared again.

## Properties

- `FillHeight`: Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.
- `FillHeightPercent`: Takes this percentage of the remaining height; null when not set.
- `FillWidth`: Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.
- `FillWidthPercent`: Takes this percentage of the remaining width; null when not set.
- `FontFamily`: The family whose closest face to [`ITextFormat.FontWeight`](ITextFormat.md) and [`ITextFormat.FontStyle`](ITextFormat.md) is used; null for the theme's body family.
- `FontSize`: The font size in pixels.
- `FontStyle`: The style of the face to use from the family.
- `FontWeight`: The weight of the face to use from the family.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `IsReady`: Whether the layout's fonts have loaded, so it can be measured.
- `LineHeight`: The line height in pixels; null for the font's own.
- `MaxHeight`: The largest height layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MaxLines`: The most lines shown; 0 for no limit.
- `MaxWidth`: The largest width layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MeasuredHeight`: The height the text needs; throws until [`TextLayout.IsReady`](TextLayout.md).
- `MeasuredWidth`: The width the text needs; throws until [`TextLayout.IsReady`](TextLayout.md).
- `MinHeight`: The smallest height layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `MinWidth`: The smallest width layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `Overflow`: How text past the box or line limit ends.
- `Text`: The text.
- `TextAlign`: How lines align horizontally.
- `TextColor`: The text color; null follows the theme's primary text color.
- `VerticalAlign`: How the text aligns vertically in its box.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `Wrapping`: Wraps lines at the box's width.

## Methods

- `Dispose()`: Releases the native layout.
- `DrawNode()`: The detached node the host shapes and draws.
- `Measure()`: The shaped size, baseline and line count.
- `Prepare()`: Shapes the text now and caches its metrics.
- `Rich(ReadOnlySpan<Span>)`: A layout of styled spans.
- `SetFontStack(FontStack, float)`: Measures with `stack` at `fontSize` pixels instead of a family.
- `SetTextLimits(int, int)`: Limits the text to `maxChars` characters and `maxLines` lines (0 for no line limit).
- `WhenReady(Action<TextLayoutReadyEventArgs>)`: Runs `callback` once the page has loaded and the layout's fonts are available, after preparing it. The fluent `OnReady` calls this.
