# Checkbox

class in `FuiCs`: [`PressableLabeledControl`](PressableLabeledControl.md)

A checkbox. Activating it toggles [`Checkbox.IsChecked`](Checkbox.md); with [`Checkbox.IsThreeState`](Checkbox.md) it cycles through checked, indeterminate (null) and unchecked, as in WPF. Its state persists across history navigation.

Fluent members: [`CheckboxExtensions`](CheckboxExtensions.md).

## Properties

- `DefaultTemplate`: A rounded box with a check mark.
- `IndicatorChecked`: What the indicator shows as checked; null for indeterminate.
- `IsChecked`: Whether the box is checked; null is indeterminate, which only a three-state box keeps (null unchecks any other).
- `IsThreeState`: Whether activating the checkbox passes through the indeterminate state. Turning it off unchecks an indeterminate box.

## Methods

- `OnActivated()`: Changes the control's value for a user activation, before [`PressableLabeledControl.Click`](PressableLabeledControl.md) is raised.

## Events

- `Changed`: Raised when [`Checkbox.IsChecked`](Checkbox.md) changes, by the user or from code.
