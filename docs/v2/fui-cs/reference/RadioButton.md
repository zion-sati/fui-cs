# RadioButton

class in `FuiCs`: [`PressableLabeledControl`](PressableLabeledControl.md)

A radio button with a [`RadioButton.Value`](RadioButton.md). In a [`RadioGroup`](RadioGroup.md), activating it selects it and the arrow keys, Home and End move the selection between the group's enabled radios; on its own, activating it checks it.

Fluent members: [`RadioButtonExtensions`](RadioButtonExtensions.md).

## Properties

- `DefaultTemplate`: A ring with a centered dot.
- `Group`: The group the radio was added to, if any.
- `IndicatorChecked`: What the indicator shows as checked; null for indeterminate.
- `IsChecked`: Whether the radio is checked; in a group, setting it selects the radio or clears the selection.
- `Value`: The value the radio stands for.

## Methods

- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `OnActivated()`: Changes the control's value for a user activation, before [`PressableLabeledControl.Click`](PressableLabeledControl.md) is raised.

## Events

- `Changed`: Raised when [`RadioButton.IsChecked`](RadioButton.md) changes, by the user, the group or code.
