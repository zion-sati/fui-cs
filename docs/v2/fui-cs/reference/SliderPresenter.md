# SliderPresenter

class in `FuiCs`

The track, fill and thumb a [`SliderTemplate`](SliderTemplate.md) builds inside a slider. The slider positions [`SliderPresenter.Root`](SliderPresenter.md) and maps pointer positions with [`SliderPresenter.ThumbSize`](SliderPresenter.md); [`SliderPresenter.Layout`](SliderPresenter.md) places the parts along the slider's length and [`SliderPresenter.Apply`](SliderPresenter.md) styles them.

## Properties

- `CrossAxisExtra`: Extra room across the slider beyond the thumb.
- `Root`: The parts' root, which the slider positions.
- `ThumbSize`: The thumb's extent along the slider, which the value range spans less of.

## Methods

- `Apply(Theme, SliderVisualState, SliderColors)`: Styles the parts for `state`, with `colors` over the theme's.
- `Layout(SliderVisualState, float)`: Places the parts for `state` along `length` pixels.
