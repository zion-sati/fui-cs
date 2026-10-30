# IImageContent

interface in `FuiCs`

A node that shows a host-loaded asset: [`Image`](Image.md) or [`Svg`](Svg.md).

## Properties

- `AltText`: Announces the node as an image with this label.
- `AssetError`: Why the asset failed to load; empty otherwise.
- `AssetHeight`: The asset's natural height once loaded.
- `AssetState`: The asset's loading state.
- `AssetWidth`: The asset's intrinsic width once loaded; 0 before.
- `Sampling`: How the content is sampled when scaled.
- `Source`: The asset URL. Null or empty clears the content.

## Events

- `AssetStateChanged`: Raised when the shown asset's load state changes, including when another node loaded the same URL.
