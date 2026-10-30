namespace FuiCs;

/// <summary>A value that raises <see cref="Changed"/> whenever it is set to a different value.</summary>
public sealed class Signal<T>(T initial)
{
    private T current = initial;

    /// <summary>Raised with the new value. Raising it schedules a commit.</summary>
    public event Action<T>? Changed;

    /// <summary>The current value; setting a different one notifies the subscribers.</summary>
    public T Value
    {
        get => current;
        set => Set(value);
    }

    /// <summary>Whether anything subscribes.</summary>
    public bool HasListeners => Changed is not null;

    /// <summary>Sets the value; returns false when it is unchanged.</summary>
    public bool Set(T next)
    {
        if (EqualityComparer<T>.Default.Equals(current, next))
        {
            return false;
        }
        current = next;
        var changed = Changed;
        if (changed is not null)
        {
            changed(next);
            FrameScheduler.MarkNeedsCommit();
        }
        return true;
    }

    /// <summary>Restores <paramref name="value"/> without raising <see cref="Changed"/> and drops every subscriber.</summary>
    internal void Reset(T value)
    {
        current = value;
        Changed = null;
    }

    /// <summary>Adds <paramref name="handler"/> to <see cref="Changed"/>; disposing the result removes it.</summary>
    public IDisposable Subscribe(Action<T> handler)
    {
        Changed += handler;
        return new Subscription(this, handler);
    }

    private sealed class Subscription(Signal<T> signal, Action<T> handler) : IDisposable
    {
        private Signal<T>? owner = signal;

        public void Dispose()
        {
            if (owner is null)
            {
                return;
            }
            owner.Changed -= handler;
            owner = null;
        }
    }
}
