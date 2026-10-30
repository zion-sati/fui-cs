# Asset

class in `FuiCs`: `IDisposable`

A texture or SVG the host loads from a URL, for drawing or for showing by id. Everything holding the same URL shares one host asset, which the host releases once the last holder is disposed; an [`Image`](Image.md) or [`Svg`](Svg.md) showing the URL is a holder too. A relative URL resolves against the page's address.

## Properties

- `Error`: Why the asset failed to load; empty otherwise.
- `Height`: The asset's natural height once loaded.
- `Id`: The host's id for the asset, for [`DrawContext`](DrawContext.md) and nodes shown by id; 0 once disposed.
- `State`: The asset's loading state.
- `Url`: The asset's URL.
- `Width`: The intrinsic width once loaded; 0 before.

## Methods

- `Dispose()`: Drops this holder's reference to the asset.

## Events

- `StateChanged`: Raised when the asset becomes ready or fails, whoever loaded it.
