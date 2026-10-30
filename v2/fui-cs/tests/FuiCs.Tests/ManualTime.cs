using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FuiCs.Tests;

/// <summary>A clock that moves only when a test advances it, firing the timers that come due on the way.</summary>
internal sealed class ManualTime : TimeProvider
{
    private readonly List<ManualTimer> timers = [];
    private long nowMs;

    public override long TimestampFrequency => 1000;

    public override long GetTimestamp() => nowMs;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(this, callback, state, nowMs + (long)dueTime.TotalMilliseconds);
        timers.Add(timer);
        return timer;
    }

    public void Advance(int milliseconds)
    {
        var target = nowMs + milliseconds;
        while (Next(target) is { } timer)
        {
            nowMs = timer.DueMs;
            timers.Remove(timer);
            timer.Fire();
        }
        nowMs = target;
    }

    private ManualTimer? Next(long target)
    {
        ManualTimer? next = null;
        foreach (var timer in timers)
        {
            if (timer.DueMs <= target && (next is null || timer.DueMs < next.DueMs))
            {
                next = timer;
            }
        }
        return next;
    }

    private sealed class ManualTimer(ManualTime time, TimerCallback callback, object? state, long dueMs) : ITimer
    {
        public long DueMs { get; private set; } = dueMs;

        public void Fire() => callback(state);

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            DueMs = time.nowMs + (long)dueTime.TotalMilliseconds;
            return true;
        }

        public void Dispose() => time.timers.Remove(this);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
