# Switch

class in `FuiCs`: [`PressableLabeledControl`](PressableLabeledControl.md)

An on/off switch. Activating it toggles [`Switch.IsChecked`](Switch.md); its state persists across history navigation.

Fluent members: [`SwitchExtensions`](SwitchExtensions.md).

## Properties

- `DefaultTemplate`: A pill track with a sliding thumb.
- `IndicatorChecked`: What the indicator shows as checked; null for indeterminate.
- `IsChecked`: Whether the switch is on.

## Methods

- `OnActivated()`: Changes the control's value for a user activation, before [`PressableLabeledControl.Click`](PressableLabeledControl.md) is raised.

## Events

- `Changed`: Raised when [`Switch.IsChecked`](Switch.md) changes, by the user or from code.
