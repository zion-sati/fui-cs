# ImageContentExtensions

class in `FuiCs`

Fluent configuration for [`Image`](Image.md) and [`Svg`](Svg.md).

## Methods

- `AltText(string)`: Sets [`IImageContent.AltText`](IImageContent.md).
- `NinePatch(float, float, float, float)`: Stretches the image as a nine-patch, keeping each edge's inset unscaled.
- `NinePatch(Thickness)`: Stretches the image as a nine-patch, keeping `insets` unscaled.
- `ObjectFit(ObjectFit)`: Sets [`Image.ObjectFit`](Image.md).
- `OnAssetStateChanged(Action<AssetLoadState>)`: Adds a handler for the asset loading or failing.
- `Sampling(ImageSampling)`: Sets [`IImageContent.Sampling`](IImageContent.md).
- `Source(string)`: Loads the content from `url`.
- `SvgId(uint)`: Shows the loaded SVG `svgId`.
- `TextureId(uint)`: Shows the loaded texture `textureId`.
- `Tint(Color)`: Tints the SVG's drawing.
