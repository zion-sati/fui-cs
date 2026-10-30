# Animation

class in `FuiCs`: `IDisposable`

A timed animation driven by the host's frames. It samples its eased progress each frame until its duration has passed; a frame gap longer than 100 ms counts as 100 ms, so a backgrounded page does not jump to the end.

## Properties

- `IsRunning`: Whether the animation is running.
- `Timing`: The duration and easing.

## Methods

- `AnimateColor(Color, Color, AnimationTiming, Action<Color>)`: Animates each channel from `from` to `to`, passing each sampled color to `handler`.
- `AnimateFloat(float, float, AnimationTiming, Action<float>)`: Animates from `from` to `to`, passing each sampled value to `handler`.
- `Cancel()`: Stops the animation where it is, without a final sample.
- `Dispose()`: Cancels the animation.
- `Finish()`: Jumps to the end: samples the final value and stops.
- `OnSample(float, float)`: Called each frame with the eased and the linear progress, from 0 to 1.
- `OnStart(double)`: Called on the first frame, with its timestamp.
- `OnStop(bool)`: Called when the animation stops, with whether it ran to the end.
