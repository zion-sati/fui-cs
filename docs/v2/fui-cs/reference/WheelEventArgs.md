# WheelEventArgs

class in `FuiCs`: `EventArgs`

A mouse wheel or trackpad scroll over a node.

## Properties

- `DeltaMode`: The unit of the deltas.
- `DeltaX`: The horizontal scroll amount, in DeltaMode units.
- `DeltaY`: The vertical scroll amount, in DeltaMode units.
- `Handled`: Set to keep the wheel from scrolling ancestors.
- `Modifiers`: The modifier keys held; Ctrl with a wheel is a trackpad pinch.
- `SceneX`: The pointer in page coordinates.
- `SceneY`: The pointer in page coordinates.
- `X`: The pointer relative to the node handling the event.
- `Y`: The pointer relative to the node handling the event.
