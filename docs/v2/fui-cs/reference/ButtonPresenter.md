# ButtonPresenter

class in `FuiCs`

The content a [`ButtonTemplate`](ButtonTemplate.md) builds inside a button: a root and the label text. The button styles its own chrome and the label's font and color; [`ButtonPresenter.Apply`](ButtonPresenter.md) styles anything else for the current state.

## Properties

- `ContentRoot`: The node holding the label.
- `Label`: The label's text node.

## Methods

- `Apply(Theme, ButtonVisualState, ButtonColors)`: Restyles the button for `state`, with `colors` over the theme's.
