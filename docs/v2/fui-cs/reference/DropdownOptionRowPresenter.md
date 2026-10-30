# DropdownOptionRowPresenter

class in `FuiCs`

One row of a dropdown's list, which a [`DropdownOptionRowTemplate`](DropdownOptionRowTemplate.md) builds: a root holding the option's label. [`DropdownOptionRowPresenter.Apply`](DropdownOptionRowPresenter.md) restyles it for each state.

## Properties

- `Label`: The text the dropdown sets to the option's label.
- `Metrics`: The row's size and padding.
- `Root`: The row's root.

## Methods

- `Apply(Theme, DropdownOptionRowVisualState, DropdownColors)`: Restyles the row for `state`, with `colors` over the theme's.
