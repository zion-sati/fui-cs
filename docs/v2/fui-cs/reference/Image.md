# Image

class in `FuiCs`: [`FlexBox`](FlexBox.md), [`IImageContent`](IImageContent.md)

A texture loaded by the host from a URL, or shown by the id of a texture loaded elsewhere. An auto width or height takes the texture's intrinsic size once it loads, keeping its aspect ratio against a fixed other side.

## Properties

- `AltText`: The image's text alternative for assistive technology.
- `AssetError`: Why the asset failed to load; empty unless it failed.
- `AssetHeight`: The asset's natural height once loaded.
- `AssetState`: The asset's loading state.
- `AssetWidth`: The asset's natural width once loaded.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `NinePatch`: Stretches the texture as a nine-patch: these insets stay unscaled at the edges. Null draws it whole.
- `ObjectFit`: How the image fits its box.
- `Sampling`: How the image is sampled when scaled.
- `Source`: The image's URL; setting it loads it.
- `TextureId`: The texture shown; setting it shows a texture loaded elsewhere and clears [`Image.Source`](Image.md).
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `Load(string, ObjectFit)`: An image loading `url`.

## Events

- `AssetStateChanged`: Raised as the image's asset loads or fails.
