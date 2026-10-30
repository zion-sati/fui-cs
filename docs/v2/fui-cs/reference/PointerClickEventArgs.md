# PointerClickEventArgs

class in `FuiCs`: `EventArgs`

A click: a primary press and release on the same node.

## Properties

- `Button`: The button that clicked.
- `Buttons`: The buttons held.
- `ClickCount`: 1 for a click, 2 for a double click, 3 for a triple click.
- `Handled`: Set to stop the click reaching ancestors.
- `Modifiers`: The modifier keys held.
- `PointerType`: The device that clicked.
- `SceneX`: The release point in page coordinates.
- `SceneY`: The release point in page coordinates.
- `X`: The release point relative to the node handling the event.
- `Y`: The release point relative to the node handling the event.
