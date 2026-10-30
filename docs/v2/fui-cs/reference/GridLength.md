# GridLength

record struct in `FuiCs`

The size of a grid row or column: pixels, a share of the remaining space (star), or sized to content.

## Properties

- `Auto`: Sized by the cells' content.

## Methods

- `op_Implicit(float)~GridLengt)`: A fixed size in pixels.
- `Pixels(float)`: A fixed size in pixels.
- `Star(float)`: A share of the remaining space, weighted against the other star tracks.
