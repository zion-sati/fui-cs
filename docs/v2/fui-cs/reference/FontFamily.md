# FontFamily

class in `FuiCs`

The stacks of one family by weight and style; [`FontFamily.Resolve`](FontFamily.md) picks the closest face.

## Properties

- `Bold`: The bold stack, if any.
- `BoldItalic`: The bold italic stack, if any.
- `Italic`: The italic stack, if any.
- `Medium`: The medium stack, if any.
- `MediumItalic`: The medium italic stack, if any.
- `Regular`: The regular weight's stack, which every other style falls back to.
- `Semibold`: The semibold stack, if any.
- `SemiboldItalic`: The semibold italic stack, if any.

## Methods

- `RegularBold(FontStack, FontStack)`: A family of a regular and a bold stack.
- `RequiredFontIds()`: The ids of every font in the family's stacks, to wait on before measuring text in it.
- `Resolve(FontWeight, FontStyle)`: The font id of the face closest to `weight`, preferring a matching `style`.
- `WithRegular(FontFace)`: A family of one regular font.
- `WithRegular(FontStack)`: A family of one regular stack.
