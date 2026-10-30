# NodeExtensions

class in `FuiCs`

Fluent configuration for every node. Each member sets the property or subscribes to the event of the same name and returns the node as its own type, so chains keep the concrete type.

## Methods

- `AllowDrop(bool)`: Sets [`Node.AllowDrop`](Node.md).
- `Cursor(CursorStyle)`: Sets [`Node.Cursor`](Node.md).
- `DragAllowedEffects(DragDropEffects)`: Makes the node a drag source allowing `effects`.
- `DragData(Func<DragDataObject>)`: Makes the node a drag source whose data `provider` creates when a drag starts.
- `Enabled(bool)`: Sets [`Node.IsEnabled`](Node.md).
- `Focusable(bool, int)`: Lets the node take keyboard focus, at `tabIndex` in the tab order (0 for document order).
- `LongPressRecognizer(LongPressGesture)`: Sets the recognizer for long presses on the node; null uses the defaults.
- `NodeId(string)`: Sets [`Node.NodeId`](Node.md).
- `OnDragCompleted(Action<DragCompletedEventArgs>)`: Adds a handler to [`Node.DragCompleted`](Node.md).
- `OnDragEnter(Action<DragEventArgs>)`: Adds a handler to [`Node.DragEnter`](Node.md).
- `OnDragLeave(Action<DragEventArgs>)`: Adds a handler to [`Node.DragLeave`](Node.md).
- `OnDragOver(Action<DragEventArgs>)`: Adds a handler to [`Node.DragOver`](Node.md).
- `OnDrop(Action<DragEventArgs>)`: Adds a handler to [`Node.Drop`](Node.md).
- `OnFocusChanged(Action<FocusChangedEventArgs>)`: Adds a handler to [`Node.FocusChanged`](Node.md).
- `OnKeyDown(Action<KeyEventArgs>)`: Adds a handler to [`Node.KeyDown`](Node.md).
- `OnKeyUp(Action<KeyEventArgs>)`: Adds a handler to [`Node.KeyUp`](Node.md).
- `OnLongPress(Action<LongPressEventArgs>)`: Adds a handler to [`Node.LongPress`](Node.md).
- `OnPanGesture(Action<GestureEventArgs>)`: Adds a handler to [`Node.PanGesture`](Node.md).
- `OnPinchGesture(Action<GestureEventArgs>)`: Adds a handler to [`Node.PinchGesture`](Node.md).
- `OnPointerCancel(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerCancel`](Node.md).
- `OnPointerClick(Action<PointerClickEventArgs>)`: Adds a handler to [`Node.PointerClick`](Node.md).
- `OnPointerDoubleClick(Action<PointerClickEventArgs>)`: Adds a handler to [`Node.PointerDoubleClick`](Node.md).
- `OnPointerDown(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerDown`](Node.md).
- `OnPointerEnter(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerEnter`](Node.md).
- `OnPointerLeave(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerLeave`](Node.md).
- `OnPointerMove(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerMove`](Node.md).
- `OnPointerTripleClick(Action<PointerClickEventArgs>)`: Adds a handler to [`Node.PointerTripleClick`](Node.md).
- `OnPointerUp(Action<PointerEventArgs>)`: Adds a handler to [`Node.PointerUp`](Node.md).
- `OnWheel(Action<WheelEventArgs>)`: Adds a handler to [`Node.Wheel`](Node.md).
- `PersistState(PersistedStateAdapter)`: Saves and restores the node's state with `adapter` across history navigation.
- `PreserveSelectionOnPointerDown(bool)`: Sets [`Node.PreserveSelectionOnPointerDown`](Node.md).
- `SemanticChecked(SemanticCheckedState)`: Sets [`Node.SemanticChecked`](Node.md).
- `SemanticDisabled(bool?)`: Sets the accessibility disabled state; null leaves it unset.
- `SemanticExpanded(bool?)`: Sets the accessibility expanded state; null leaves it unset.
- `SemanticLabel(string)`: Sets [`Node.SemanticLabel`](Node.md).
- `SemanticOrientation(Orientation)`: Sets [`Node.SemanticOrientation`](Node.md).
- `SemanticRole(SemanticRole)`: Sets [`Node.SemanticRole`](Node.md).
- `SemanticSelected(bool?)`: Sets the accessibility selected state; null leaves it unset.
- `SemanticValueRange(float, float, float)`: Reports a value of `now` between `min` and `max` to assistive technology.
- `SemanticValueRange(SemanticValueRange?)`: Sets the accessibility value range; null leaves it unset.
- `Visibility(Visibility)`: Sets [`Node.Visibility`](Node.md).
