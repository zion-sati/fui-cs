# RichText

class in `FuiCs`: [`Text`](Text.md)

Text made of styled [`Span`](Span.md)s, sent to the host as one string with style runs.

## Properties

- `FontFamily`: The theme's body family when null; setting it replaces a [`TextCore.FontStack`](TextCore.md).
- `FontSize`: The font size in pixels; 0 leaves the host default until set.
- `FontStyle`: The style of the face to use from the family.
- `FontWeight`: The weight of the face to use from the family.
- `Spans`: The spans in order.
- `Text`: Replaces the spans with a single unstyled span.
- `TextColor`: The text color; null follows the theme's primary text color.

## Methods

- `AddSpan(Span)`: Appends a span.
- `Apply(RichText.Composition)`: Sends the parts of `composition` that differ from the last one applied.
- `Compose()`: The font, color, merged text and style runs the spans resolve to under the current theme.
- `FromText(string)`: Rich text of one plain span.
- `OnThemeChanged(Theme)`: Spans without their own color, size or family take them from the theme, so the runs are rebuilt when the theme changes any of them. Only what changed is sent: resending unchanged text and runs would discard the shaping a detached node was prepared with for [`Bitmap.Render`](Bitmap.md).
- `SetFontStack(FontStack, float)`: Renders with `stack` at `fontSize` instead of a family.
- `SetSpans(ReadOnlySpan<Span>)`: Replaces the spans.
