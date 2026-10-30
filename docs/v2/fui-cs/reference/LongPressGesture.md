# LongPressGesture

class in `FuiCs`

A configurable long-press recognizer for [`Node`](Node.md).

## Properties

- `MinimumDurationMs`: How long a pointer must stay down, in milliseconds.
- `MovementTolerancePx`: How far a pointer may move, in pixels, and still count.

## Methods

- `Create()`: A long-press recognizer with the default duration and tolerance.
- `MinimumDuration(int)`: Sets how long a pointer must stay down, in milliseconds.
- `MovementTolerance(float)`: Sets how far a pointer may move, in pixels, and still count.
- `OnRecognized(Action<LongPressEventArgs>)`: Adds a handler for recognized long presses.

## Events

- `Recognized`: Raised when a long press is recognized.
