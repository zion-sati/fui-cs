# SelectionHandles

class in `FuiCs`

The draggable handles at the two ends of a text selection, for touch. They follow the selection of a text node or a selection area's cross-node selection, and dragging one moves that end of the selection.

## Properties

- `Mode`: When the selection handles show; [`SelectionHandleMode.Auto`](SelectionHandleMode.md) shows them for touch selections.

## Methods

- `ClearShell()`: Ends a handle drag, hides the handles and forgets them, as they went with the shell that hosted them.
- `HideHandles()`: Hides both hit targets, leaving their parts visible for the next show.
- `RefreshActiveGeometry()`: Moves the handles to the selection's current geometry, after a scroll or layout change.
- `RouteActiveHandleDrag(PointerEventArgs)`: Routes the pointer to the handle being dragged, wherever it is; returns whether it took the event.
- `SetVisibility(SelectionHandles.HandleNode, Visibility)`: Shows the handle's hit target, with its knob and shoulder shown or hidden.
