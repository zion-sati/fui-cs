# Svg

class in `FuiCs`: [`FlexBox`](FlexBox.md), [`IImageContent`](IImageContent.md)

An SVG loaded by the host from a URL, or shown by the id of an SVG loaded elsewhere, optionally tinted. An auto width or height takes the SVG's intrinsic size once it loads, keeping its aspect ratio against a fixed other side.

## Properties

- `AltText`: The SVG's text alternative for assistive technology.
- `AssetError`: Why the asset failed to load; empty unless it failed.
- `AssetHeight`: The asset's natural height once loaded.
- `AssetState`: The asset's loading state.
- `AssetWidth`: The asset's natural width once loaded.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `Sampling`: How the SVG is sampled when scaled.
- `Source`: The SVG's URL; setting it loads it.
- `SvgId`: The SVG shown; setting it shows an SVG loaded elsewhere and clears [`Svg.Source`](Svg.md).
- `Tint`: Recolors the SVG's shapes; null draws its own colors.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `Load(string, Color?)`: An SVG loading `url`.

## Events

- `AssetStateChanged`: Raised as the SVG's asset loads or fails.
