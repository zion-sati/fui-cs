# DynamicTextLayout

class in `FuiCs`: [`TextLayout`](TextLayout.md)

Short, single-style text that changes often, such as counters and clocks. The host prepares the glyphs of a fixed charset once, so new text reshapes without a font round trip; once ready, every accepted change is prepared immediately and stays measurable.

## Properties

- `Charset`: The prepared characters, including those of [`DynamicTextLayout.Prefix`](DynamicTextLayout.md) and [`DynamicTextLayout.Suffix`](DynamicTextLayout.md).
- `CharsetOverflow`: What the layout does with characters its prepared charset lacks.
- `Precision`: Digits after the decimal point; null formats the shortest text that round-trips.
- `Prefix`: Text drawn before the value.
- `Suffix`: Text drawn after the value.
- `Text`
- `Value`: The last number set with [`DynamicTextLayout.TrySetValue`](DynamicTextLayout.md); null until one is set.

## Methods

- `Numeric()`: A layout for numbers set with [`DynamicTextLayout.TrySetValue`](DynamicTextLayout.md), formatted with [`DynamicTextLayout.Precision`](DynamicTextLayout.md), [`DynamicTextLayout.Prefix`](DynamicTextLayout.md) and [`DynamicTextLayout.Suffix`](DynamicTextLayout.md).
- `TrySetText(string)`: Sets the text; returns false when it is rejected under [`DynamicTextOverflow.Reject`](DynamicTextOverflow.md).
- `TrySetValue(double)`: Shows `value` between [`DynamicTextLayout.Prefix`](DynamicTextLayout.md) and [`DynamicTextLayout.Suffix`](DynamicTextLayout.md); returns false when the text is rejected.
