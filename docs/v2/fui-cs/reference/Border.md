# Border

record struct in `FuiCs`

A box outline; dashed and dotted borders alternate [`Border.DashOn`](Border.md) drawn and [`Border.DashOff`](Border.md) skipped pixels.

## Methods

- `Dashed(float, Color, float, float)`: A dashed border: `dashOn` pixels drawn, `dashOff` skipped.
- `Dotted(float, Color, float, float)`: A dotted border: `dashOn` pixels drawn, `dashOff` skipped.
- `Solid(float, Color)`: A solid border.
