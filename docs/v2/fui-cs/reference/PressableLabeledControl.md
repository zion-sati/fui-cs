# PressableLabeledControl

class in `FuiCs`: [`FlexBox`](FlexBox.md)

The base of checkboxes, radio buttons and switches: an indicator and a label in a row. A primary click on it, or Space while it has focus, activates it and raises [`PressableLabeledControl.Click`](PressableLabeledControl.md).

Fluent members: [`PressableLabeledControlExtensions`](PressableLabeledControlExtensions.md).

## Properties

- `Colors`: Colors over the theme's for the indicator and label.
- `IndicatorChecked`: What the indicator shows as checked; null for indeterminate.
- `IsHovered`: Whether the pointer is over the control.
- `IsPressed`: Whether the control is pressed.
- `Label`: The label's text.
- `Sizing`: The indicator and label sizes; the defaults when null.
- `Template`: Builds the indicator; null uses the control's default template.

## Methods

- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandlePointerEvent(PointerEventArgs)`: Tracks hover and the pressed look; the host keeps routing a pressed pointer here after it leaves.
- `OnActivated()`: Changes the control's value for a user activation, before [`PressableLabeledControl.Click`](PressableLabeledControl.md) is raised.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `Refresh()`: Restyles the control and its indicator for the current theme, colors and state.

## Events

- `Click`: Raised after the user activates the control by pointer or keyboard.
