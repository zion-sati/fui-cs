# AnimationManager

class in `FuiCs`

Runs animations on the host's frames. Starting an animation requests frames until no animation is running; the shared manager is the one [`Animation.AnimateFloat`](Animation.md) and [`Animation.AnimateColor`](Animation.md) use.

## Properties

- `HasActiveAnimations`: Whether any animation is running.
- `Shared`: The manager that runs every animation on the page's frames.

## Methods

- `Cancel(Animation)`: Stops `animation` where it is.
- `Finish(Animation)`: Jumps `animation` to its end.
- `Reset()`: Cancels every running animation and forgets the last frame time.
- `Start(Animation)`: Starts `animation`, restarting it if it was already running here, and returns it.
- `Tick(double)`: Advances every running animation to the frame at `timestampMs`.
