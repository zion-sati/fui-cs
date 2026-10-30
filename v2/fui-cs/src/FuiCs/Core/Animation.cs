namespace FuiCs;

/// <summary>Maps linear progress from 0 to 1 to eased progress.</summary>
public abstract class Easing
{
    /// <summary>The eased progress for <paramref name="progress"/>, both from 0 to 1.</summary>
    public abstract float Sample(float progress);

    private protected static float ClampUnit(float value) => Math.Clamp(value, 0.0f, 1.0f);
}

/// <summary>No easing: progress is linear.</summary>
public sealed class LinearEasing : Easing
{
    /// <inheritdoc/>
    public override float Sample(float progress) => ClampUnit(progress);
}

/// <summary>Starts slow and speeds up, as a cubic.</summary>
public sealed class CubicInEasing : Easing
{
    /// <inheritdoc/>
    public override float Sample(float progress)
    {
        var t = ClampUnit(progress);
        return t * t * t;
    }
}

/// <summary>Starts fast and slows down, as a cubic.</summary>
public sealed class CubicOutEasing : Easing
{
    /// <inheritdoc/>
    public override float Sample(float progress)
    {
        var t = ClampUnit(progress) - 1.0f;
        return (t * t * t) + 1.0f;
    }
}

/// <summary>Slow at both ends, as a cubic.</summary>
public sealed class CubicInOutEasing : Easing
{
    /// <inheritdoc/>
    public override float Sample(float progress)
    {
        var t = ClampUnit(progress);
        if (t < 0.5f)
        {
            return 4.0f * t * t * t;
        }
        var offset = (-2.0f * t) + 2.0f;
        return 1.0f - (offset * offset * offset * 0.5f);
    }
}

/// <summary>Starts fast and slows down, as a quadratic.</summary>
public sealed class QuadOutEasing : Easing
{
    /// <inheritdoc/>
    public override float Sample(float progress)
    {
        var t = 1.0f - ClampUnit(progress);
        return 1.0f - (t * t);
    }
}

/// <summary>The built-in easings.</summary>
public static class Easings
{
    /// <summary>No easing.</summary>
    public static Easing Linear { get; } = new LinearEasing();

    /// <summary>Cubic ease-in.</summary>
    public static Easing CubicIn { get; } = new CubicInEasing();

    /// <summary>Cubic ease-out.</summary>
    public static Easing CubicOut { get; } = new CubicOutEasing();

    /// <summary>Cubic ease-in-out.</summary>
    public static Easing CubicInOut { get; } = new CubicInOutEasing();

    /// <summary>Quadratic ease-out.</summary>
    public static Easing QuadOut { get; } = new QuadOutEasing();
}

/// <summary>How long an animation runs and how its progress eases; a negative duration is zero.</summary>
public sealed class AnimationTiming(TimeSpan duration, Easing? easing = null)
{
    /// <summary>How long the animation runs; negative durations become zero.</summary>
    public TimeSpan Duration { get; } = duration > TimeSpan.Zero ? duration : TimeSpan.Zero;

    /// <summary>The easing applied to the progress; linear when null.</summary>
    public Easing Easing { get; } = easing ?? Easings.Linear;
}

/// <summary>
/// A timed animation driven by the host's frames. It samples its eased progress each frame until its duration has
/// passed; a frame gap longer than 100 ms counts as 100 ms, so a backgrounded page does not jump to the end.
/// </summary>
public abstract class Animation : IDisposable
{
    private const double MaxFrameDeltaMs = 100.0;

    private AnimationManager? manager;
    private bool started;
    private double lastTimestampMs;
    private double elapsedMs;

    /// <summary>An animation that runs for <paramref name="timing"/>'s duration with its easing.</summary>
    protected Animation(AnimationTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);
        Timing = timing;
    }

    /// <summary>The duration and easing.</summary>
    public AnimationTiming Timing { get; }

    /// <summary>Whether the animation is running.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Animates from <paramref name="from"/> to <paramref name="to"/>, passing each sampled value to <paramref name="handler"/>.</summary>
    public static Animation AnimateFloat(float from, float to, AnimationTiming timing, Action<float> handler) =>
        AnimationManager.Shared.Start(new FloatAnimation(from, to, timing, handler));

    /// <summary>Animates each channel from <paramref name="from"/> to <paramref name="to"/>, passing each sampled color to <paramref name="handler"/>.</summary>
    public static Animation AnimateColor(Color from, Color to, AnimationTiming timing, Action<Color> handler) =>
        AnimationManager.Shared.Start(new ColorAnimation(from, to, timing, handler));

    /// <summary>Stops the animation where it is, without a final sample.</summary>
    public void Cancel()
    {
        if (manager is { } owner)
        {
            owner.Cancel(this);
            return;
        }
        CancelCore();
    }

    /// <summary>Jumps to the end: samples the final value and stops.</summary>
    public void Finish()
    {
        if (manager is { } owner)
        {
            owner.Finish(this);
            return;
        }
        FinishCore();
    }

    /// <summary>Cancels the animation.</summary>
    public void Dispose() => Cancel();

    /// <summary>Called on the first frame, with its timestamp.</summary>
    protected virtual void OnStart(double timestampMs)
    {
    }

    /// <summary>Called each frame with the eased and the linear progress, from 0 to 1.</summary>
    protected abstract void OnSample(float easedProgress, float linearProgress);

    /// <summary>Called when the animation stops, with whether it ran to the end.</summary>
    protected virtual void OnStop(bool finished)
    {
    }

    internal void Attach(AnimationManager owner, double knownTimestampMs, bool hasKnownTimestamp)
    {
        manager = owner;
        IsRunning = true;
        started = false;
        lastTimestampMs = 0.0;
        elapsedMs = 0.0;
        if (hasKnownTimestamp)
        {
            Start(knownTimestampMs);
        }
    }

    internal void Tick(double timestampMs)
    {
        if (!IsRunning)
        {
            return;
        }
        if (!started)
        {
            Start(timestampMs);
            return;
        }
        var durationMs = Timing.Duration.TotalMilliseconds;
        elapsedMs += Math.Clamp(timestampMs - lastTimestampMs, 0.0, MaxFrameDeltaMs);
        lastTimestampMs = timestampMs;
        var linearProgress = durationMs <= 0.0 ? 1.0f : Math.Clamp((float)(elapsedMs / durationMs), 0.0f, 1.0f);
        OnSample(Timing.Easing.Sample(linearProgress), linearProgress);
        if (linearProgress >= 1.0f)
        {
            Stop(finished: true);
        }
    }

    internal void CancelCore()
    {
        if (IsRunning)
        {
            Stop(finished: false);
        }
    }

    internal void FinishCore()
    {
        if (!IsRunning)
        {
            return;
        }
        if (!started)
        {
            started = true;
            lastTimestampMs = 0.0;
            elapsedMs = Timing.Duration.TotalMilliseconds;
            OnStart(0.0);
        }
        OnSample(1.0f, 1.0f);
        Stop(finished: true);
    }

    private void Start(double timestampMs)
    {
        started = true;
        lastTimestampMs = timestampMs;
        elapsedMs = 0.0;
        OnStart(timestampMs);
        if (Timing.Duration <= TimeSpan.Zero)
        {
            OnSample(1.0f, 1.0f);
            Stop(finished: true);
            return;
        }
        OnSample(Timing.Easing.Sample(0.0f), 0.0f);
    }

    private void Stop(bool finished)
    {
        IsRunning = false;
        manager = null;
        OnStop(finished);
    }

    private sealed class FloatAnimation(float from, float to, AnimationTiming timing, Action<float> handler) : Animation(timing)
    {
        protected override void OnSample(float easedProgress, float linearProgress) =>
            handler(from + ((to - from) * Math.Clamp(easedProgress, 0.0f, 1.0f)));
    }

    private sealed class ColorAnimation(Color from, Color to, AnimationTiming timing, Action<Color> handler) : Animation(timing)
    {
        protected override void OnSample(float easedProgress, float linearProgress) => handler(from.Mix(to, easedProgress));
    }
}

/// <summary>
/// Runs animations on the host's frames. Starting an animation requests frames until no animation is running; the
/// shared manager is the one <see cref="Animation.AnimateFloat"/> and <see cref="Animation.AnimateColor"/> use.
/// </summary>
public sealed class AnimationManager
{
    private readonly List<Animation> active = [];
    private double lastTimestampMs;
    private bool hasLastTimestamp;

    /// <summary>The manager that runs every animation on the page's frames.</summary>
    public static AnimationManager Shared { get; } = new();

    /// <summary>Whether any animation is running.</summary>
    public bool HasActiveAnimations => active.Count > 0;

    /// <summary>Starts <paramref name="animation"/>, restarting it if it was already running here, and returns it.</summary>
    public Animation Start(Animation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        Cancel(animation);
        animation.Attach(this, lastTimestampMs, hasLastTimestamp);
        if (animation.IsRunning)
        {
            active.Add(animation);
        }
        FrameScheduler.MarkNeedsCommit();
        return animation;
    }

    /// <summary>Stops <paramref name="animation"/> where it is.</summary>
    public void Cancel(Animation animation)
    {
        if (RemoveActive(animation))
        {
            animation.CancelCore();
        }
    }

    /// <summary>Jumps <paramref name="animation"/> to its end.</summary>
    public void Finish(Animation animation)
    {
        if (RemoveActive(animation))
        {
            animation.FinishCore();
        }
    }

    /// <summary>Advances every running animation to the frame at <paramref name="timestampMs"/>.</summary>
    public void Tick(double timestampMs)
    {
        lastTimestampMs = timestampMs;
        hasLastTimestamp = true;
        for (var index = active.Count - 1; index >= 0; --index)
        {
            var animation = active[index];
            animation.Tick(timestampMs);
            if (!animation.IsRunning)
            {
                RemoveAt(index);
            }
        }
        if (active.Count > 0)
        {
            FrameScheduler.MarkNeedsCommit();
        }
    }

    /// <summary>Cancels every running animation and forgets the last frame time.</summary>
    public void Reset()
    {
        for (var index = active.Count - 1; index >= 0; --index)
        {
            active[index].CancelCore();
        }
        active.Clear();
        lastTimestampMs = 0.0;
        hasLastTimestamp = false;
    }

    private bool RemoveActive(Animation animation)
    {
        var index = active.IndexOf(animation);
        if (index < 0)
        {
            return false;
        }
        RemoveAt(index);
        return true;
    }

    // Swap-remove, as the reference does; ticking runs backwards so a swapped-in animation is not skipped.
    private void RemoveAt(int index)
    {
        var last = active.Count - 1;
        active[index] = active[last];
        active.RemoveAt(last);
    }
}
