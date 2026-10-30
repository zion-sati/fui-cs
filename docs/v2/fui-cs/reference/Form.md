# Form

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A column grouping a form's fields. While active, Enter presses [`Form.DefaultButton`](Form.md) and Escape presses [`Form.CancelButton`](Form.md), as WPF's IsDefault and IsCancel buttons do; Enter on a focused button presses that button instead.

## Properties

- `CancelButton`: The button Escape presses while the form is active.
- `DefaultButton`: The button Enter presses while the form is active.
- `IsActive`: Whether the form is handling Enter and Escape.

## Methods

- `Activate()`: Starts handling Enter and Escape for the whole page; the most recently activated form wins.
- `Deactivate()`: Stops handling Enter and Escape, releasing a key press in progress.
- `Dispose()`: Disposes the subtree, deleting each native node once.
