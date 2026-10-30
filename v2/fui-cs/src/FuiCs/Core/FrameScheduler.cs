using FuiCs.Bindings;

namespace FuiCs;

/// <summary>The page's first frame being committed.</summary>
public sealed class LoadedEventArgs : EventArgs
{
    /// <summary>The shared instance.</summary>
    public static new readonly LoadedEventArgs Empty = new();
}

/// <summary>
/// Coalesces retained mutations into one commit per frame, and runs loaded and after-commit callbacks.
/// </summary>
public static class FrameScheduler
{
    private static bool needsCommit;
    private static bool flushScheduled;
    private static bool didFirstCommit;
    private static Action? nextCommit;
    private static Action<LoadedEventArgs>? loaded;

    /// <summary>Asks for a commit at the end of the current event or frame.</summary>
    public static void MarkNeedsCommit()
    {
        needsCommit = true;
        if (flushScheduled)
        {
            return;
        }
        flushScheduled = true;
        Ui.RequestRender();
    }

    /// <summary>
    /// Runs <paramref name="handler"/> once after the page has been mounted and the first frame has been
    /// committed, or immediately if that has already happened. Safe to call while building the page; timers
    /// started from the handler survive the mount's timer reset.
    /// </summary>
    public static void OnLoaded(Action<LoadedEventArgs> handler)
    {
        if (didFirstCommit)
        {
            handler(LoadedEventArgs.Empty);
            return;
        }
        loaded += handler;
    }

    /// <summary>Runs the loaded handlers registered before the first commit.</summary>
    public static void FireLoadedCallbacks()
    {
        if (didFirstCommit)
        {
            return;
        }
        didFirstCommit = true;
        var handlers = loaded;
        loaded = null;
        handlers?.Invoke(LoadedEventArgs.Empty);
    }

    /// <summary>The number of frames committed, so work queued for the next commit can tell when it has run.</summary>
    internal static int CommitCount { get; private set; }

    /// <summary>Runs <paramref name="callback"/> once, after the next frame is committed.</summary>
    public static void AfterNextCommit(Action callback) => nextCommit += callback;

    /// <summary>Commits pending changes now if any are waiting; returns whether it did.</summary>
    public static bool FlushCommit()
    {
        flushScheduled = false;
        if (!needsCommit)
        {
            return false;
        }
        needsCommit = false;
        Ui.CommitFrame();
        CommitCount += 1;
        var callbacks = nextCommit;
        nextCommit = null;
        callbacks?.Invoke();
        return true;
    }

    /// <summary>Starts a new mount: loaded handlers registered while building the page are kept and fire on the next first commit.</summary>
    public static void ResetCommitState()
    {
        needsCommit = false;
        flushScheduled = false;
        didFirstCommit = false;
        nextCommit = null;
    }

    /// <summary>Clears all scheduler state, including pending loaded handlers.</summary>
    internal static void Reset()
    {
        ResetCommitState();
        loaded = null;
    }
}
