# LongPressEventArgs

class in `FuiCs`: `EventArgs`

A long press: a pointer held still past the minimum duration.

## Properties

- `DurationMs`: How long the pointer was held, in milliseconds.
- `Handled`: Set to stop the event reaching ancestors and the built-in long-press handling.
- `Modifiers`: The modifier keys held.
- `PointerId`: The pointer's identifier.
- `PointerType`: The device that pressed.
- `SceneX`: The press point in page coordinates.
- `SceneY`: The press point in page coordinates.
- `X`: The press point relative to the node handling the event.
- `Y`: The press point relative to the node handling the event.
