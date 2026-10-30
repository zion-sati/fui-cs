# Length

record struct in `FuiCs`

A length in pixels or percent of the parent, or [`Length.Auto`](Length.md). A `Single` converts implicitly as pixels.

## Properties

- `Auto`: Sized by its content or the layout.

## Methods

- `op_Implicit(float)~Lengt)`: A length in pixels.
- `Percent(float)`: A percentage of the parent's size.
- `Pixels(float)`: A length in pixels.
