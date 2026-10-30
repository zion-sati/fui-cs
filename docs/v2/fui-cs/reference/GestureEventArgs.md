# GestureEventArgs

class in `FuiCs`: `EventArgs`

Pan and pinch gesture events.

## Properties

- `DeltaX`: The pan's movement since the last event, in pixels.
- `DeltaY`: The pan's movement since the last event, in pixels.
- `Handled`: Set to stop the event reaching ancestors and the host's default handling.
- `Kind`: Whether the event reports a pan or a pinch.
- `Phase`: Where the event falls in its gesture.
- `PointerCount`: How many pointers take part in the gesture.
- `Scale`: The pinch's scale since the last event; 1 when it did not change.
- `SceneX`: The gesture's point in page coordinates.
- `SceneY`: The gesture's point in page coordinates.
- `X`: The gesture's point relative to the node handling the event.
- `Y`: The gesture's point relative to the node handling the event.
