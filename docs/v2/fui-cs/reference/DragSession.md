# DragSession

class in `FuiCs`

A drag in progress from [`DragSession.Source`](DragSession.md).

## Properties

- `AllowedEffects`: The effects the source allows.
- `CurrentEffect`: The effect the target under the pointer accepts.
- `Data`: The dragged data.
- `IsActive`: Whether the drag is still in progress.
- `Source`: The node the drag started from.

## Methods

- `Cancel()`: Cancels the drag.
- `OnCompleted(Action<DragCompletedEventArgs>)`: Adds a handler for the drag ending.

## Events

- `Completed`: Raised when the drag ends.
