# ImageSampling

record struct in `FuiCs`

How a texture or SVG is sampled when it is scaled.

## Properties

- `CubicCatmullRom`: Catmull-Rom cubic filtering, sharper than Mitchell.
- `CubicMitchell`: Mitchell cubic filtering.
- `Linear`: Bilinear filtering.
- `LinearMipmapLinear`: Trilinear filtering across mipmap levels.
- `LinearMipmapNearest`: Bilinear filtering between the nearest mipmap level's pixels.
- `Nearest`: Nearest-pixel sampling, for pixel art.

## Methods

- `Anisotropic(uint)`: Anisotropic filtering up to `maxAnisotropy` samples.
