# Color

record struct in `FuiCs`

An RGBA color packed as 0xRRGGBBAA, the layout the host expects. A `UInt32` converts implicitly.

## Properties

- `A`: The alpha channel; 255 is opaque.
- `B`: The blue channel.
- `G`: The green channel.
- `R`: The red channel.
- `Transparent`: Fully transparent black.

## Methods

- `FromHsl(float, float, float)`: An opaque color from hue in degrees and saturation and lightness from 0 to 1.
- `FromRgb(byte, byte, byte)`: An opaque color from its channels.
- `FromRgba(byte, byte, byte, byte)`: A color from its channels.
- `Mix(Color, float)`: Blends each channel toward `to` by `amount`, from 0 (this color) to 1.
- `op_Implicit(uint)~Colo)`: A color from a 0xRRGGBBAA value.
