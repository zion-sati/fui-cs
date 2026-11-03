# ProgressBar

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A bar that fills from its start in proportion to [`ProgressBar.Value`](ProgressBar.md) between [`ProgressBar.Minimum`](ProgressBar.md) and [`ProgressBar.Maximum`](ProgressBar.md); a vertical bar fills from the bottom.

Fluent members: [`ProgressBarExtensions`](ProgressBarExtensions.md).

## Properties

- `CornerRadius`: The bar's corners; unset, the ends are fully rounded.
- `FillColor`: The filled part; null uses the theme's accent.
- `FillsLength`: Whether the bar fills its container along its orientation, as `FillWidth` or `FillHeight` does, instead of a fixed [`ProgressBar.Length`](ProgressBar.md). Setting [`ProgressBar.Length`](ProgressBar.md) returns to a fixed length.
- `Length`: The bar's extent along its orientation, in pixels.
- `Maximum`: The highest value; lowering it below [`ProgressBar.Minimum`](ProgressBar.md) lowers the minimum too.
- `Minimum`: The lowest value; raising it above [`ProgressBar.Maximum`](ProgressBar.md) raises the maximum too.
- `Orientation`: Whether the bar fills across or upwards.
- `Thickness`: The bar's extent across its orientation, in pixels.
- `TrackColor`: The unfilled track; null uses the theme's scrollbar track color.
- `Value`: The progress, clamped to the range.
