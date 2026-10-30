# DynamicTextOverflow

enum in `FuiCs`

What a [`DynamicTextLayout`](DynamicTextLayout.md) does with text that has characters outside its charset.

## Values

- `Reject`: [`DynamicTextLayout.TrySetText`](DynamicTextLayout.md) returns false and the text is unchanged.
- `FallbackShape`: The text is accepted and shaped like any other text.
