# SliderPresenter

class in `FuiCs`

The track, fill and thumb a [`SliderTemplate`](SliderTemplate.md) builds inside a slider. The slider sizes [`SliderPresenter.Root`](SliderPresenter.md) to its track area, a fixed length or the length it fills, and maps pointer positions with [`SliderPresenter.ThumbSize`](SliderPresenter.md); [`SliderPresenter.Layout`](SliderPresenter.md) places the parts relative to the root, as percentages along the track rather than pixels, and [`SliderPresenter.Apply`](SliderPresenter.md) styles them.

## Properties

- `CrossAxisExtra`: Extra room across the slider beyond the thumb.
- `Root`: The parts' root, which the slider sizes to its track area.
- `ThumbSize`: The thumb's extent along the slider, which the value range spans less of.

## Methods

- `Apply(Theme, SliderVisualState, SliderColors)`: Styles the parts for `state`, with `colors` over the theme's.
- `Layout(SliderVisualState)`: Places the parts for `state` relative to [`SliderPresenter.Root`](SliderPresenter.md).
