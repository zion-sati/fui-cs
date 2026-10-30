# RadioGroup

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A column of radio buttons of which at most one is selected. Its selection persists across history navigation.

Fluent members: [`RadioGroupExtensions`](RadioGroupExtensions.md).

## Properties

- `Radios`: The group's radios in order.
- `SelectedIndex`: The selected radio's index, or -1 for none. Selecting a disabled radio leaves the selection as it is.
- `SelectedRadio`: The selected radio, if any.
- `SelectedValue`: The selected radio's value, if any.

## Methods

- `AddOption(string, string)`: Adds a radio for `value`, labeled with `label` or the value, and returns it.
- `AddRadio(RadioButton)`: Adds `radio` as the group's last radio; a checked radio becomes the selection if there is none.
- `MoveSelection(RadioButton, int)`: Selects the next enabled radio after `from` in `step`'s direction, wrapping around.
- `Select(int, bool, bool)`: Selects the radio at `index`, or none for -1; a disabled radio is not selected.

## Events

- `Changed`: Raised when the selection changes, by the user or from code.
