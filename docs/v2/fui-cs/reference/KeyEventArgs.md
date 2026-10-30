# KeyEventArgs

class in `FuiCs`: `EventArgs`

A key pressed or released while a node has focus.

## Properties

- `EventType`: Whether the key went down or up.
- `Handled`: Set to keep the key from its default handling.
- `Key`: The key's value, as the browser's KeyboardEvent.key gives it ("a", "Enter", "ArrowLeft").
- `Modifiers`: The modifier keys held.
