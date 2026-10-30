# DropdownFieldPresenter

class in `FuiCs`

The field a [`DropdownFieldTemplate`](DropdownFieldTemplate.md) builds for a dropdown: a root holding the host of the selected value's text and the host the chevron goes in. [`DropdownFieldPresenter.Apply`](DropdownFieldPresenter.md) restyles them for each state.

## Properties

- `ChevronHost`: Where the dropdown places its chevron presenter's root.
- `Metrics`: The field's size and padding.
- `Root`: The field's root, which the dropdown holds.
- `ValueHost`: The node holding the selected value's text.
- `ValueNode`: The text the dropdown sets to the selected option's label.

## Methods

- `Apply(Theme, DropdownFieldVisualState, DropdownColors)`: Restyles the field for `state`, with `colors` over the theme's.
