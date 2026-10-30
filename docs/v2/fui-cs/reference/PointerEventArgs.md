# PointerEventArgs

class in `FuiCs`: `EventArgs`

A pointer moving, pressing, releasing, entering, leaving or being cancelled over a node.

## Properties

- `Button`: The button that changed.
- `Buttons`: The buttons held.
- `ClickCount`: The click count of the press: 2 for a double click.
- `EventType`: What the pointer did.
- `Handled`: Set to stop the event reaching ancestors.
- `Height`: The contact's height in pixels.
- `IsPrimary`: Whether this is the primary pointer of its type.
- `IsPrimaryActivation`: A primary-button press, or any touch or pen contact.
- `Modifiers`: The modifier keys held.
- `PointerId`: The pointer's identifier, stable for the life of a contact.
- `PointerType`: The device behind the pointer.
- `Pressure`: The contact pressure, from 0 to 1.
- `SceneX`: The pointer in page coordinates.
- `SceneY`: The pointer in page coordinates.
- `TangentialPressure`: A pen's barrel pressure, from -1 to 1.
- `TiltX`: A pen's tilt across, in degrees.
- `TiltY`: A pen's tilt along, in degrees.
- `Twist`: A pen's rotation, in degrees.
- `Width`: The contact's width in pixels.
- `X`: The pointer relative to the node handling the event.
- `Y`: The pointer relative to the node handling the event.
