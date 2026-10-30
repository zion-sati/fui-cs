# Paint

record struct in `FuiCs`

How a shape is filled and stroked. A transparent color draws nothing for that part.

## Properties

- `HasFill`: Whether the paint fills.
- `HasStroke`: Whether the paint outlines.

## Methods

- `Fill(Color)`: A fill with `color`.
- `FilledStroke(Color, Color, float)`: A fill with an outline.
- `Stroke(Color, float)`: An outline `width` pixels wide.
