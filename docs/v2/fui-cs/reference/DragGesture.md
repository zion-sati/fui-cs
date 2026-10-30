# DragGesture

class in `FuiCs`

Recognizes a pointer drag on `host`: captures the pointer on press, starts once the pointer moves past the threshold (or on long press), and reports deltas until release or cancel. Raising any of the events schedules a commit.

## Properties

- `IsDragging`: Whether a drag is in progress.

## Methods

- `Cancel()`: Cancels the drag in progress, if any.
- `HandleLongPress(float, float, KeyModifier)`: Feeds a long press, which starts a drag waiting for one; returns whether it did.
- `HandlePointerDown(float, float, KeyModifier, bool)`: Feeds a press; with `waitForLongPress`, the drag starts only from a long press.
- `HandlePointerMove(float, float, KeyModifier)`: Feeds a pointer move.
- `HandlePointerUp(float, float, KeyModifier)`: Feeds a release, which ends the drag.
- `Threshold(float)`: Sets how far, in pixels, the pointer must move before a drag starts.

## Events

- `Completed`: Raised when the drag ends or is cancelled.
- `Delta`: Raised as the drag moves.
- `DraggingChanged`: Raised when a drag starts or ends, with whether one is in progress.
- `Started`: Raised when the drag starts.
