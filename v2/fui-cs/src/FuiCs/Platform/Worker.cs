using System.Text;

using FuiCs.Hosting;

namespace FuiCs;

/// <summary>A progress message from a worker.</summary>
public sealed class WorkerProgressEventArgs(string message) : EventArgs
{
    /// <summary>The message.</summary>
    public string Message { get; } = message;
}

/// <summary>A worker finishing.</summary>
public sealed class WorkerCompletedEventArgs(string result) : EventArgs
{
    /// <summary>The worker's result.</summary>
    public string Result { get; } = result;
}

/// <summary>A worker failing.</summary>
public sealed class WorkerErrorEventArgs(string message) : EventArgs
{
    /// <summary>The error.</summary>
    public string Message { get; } = message;
}

/// <summary>
/// A one-shot background job in a Web Worker. <see cref="Start"/> runs the entry <see cref="EntryName"/> of a
/// published fui-cs worker project with a string input; the worker reports progress, then completes with a result or
/// fails. <see cref="Path"/> is the worker's published folder relative to the page (<c>browser/workers/Name</c> for a
/// worker project the application references with <c>NetWasmWorker="true"</c>), or its client module.
/// </summary>
public sealed class Worker : IDisposable
{
    private const int MaxStartInputBytes = 1024 * 1024;

    private static readonly Dictionary<uint, Worker> Active = [];
    private static uint nextWorkerId = 1;
    private static bool installed;

    private readonly uint workerId;
    private bool started;
    private bool finished;
    private bool cancelRequested;

    /// <summary>
    /// A worker running <paramref name="entryName"/> from the worker app published at <paramref name="path"/>.
    /// </summary>
    public Worker(string path, string entryName)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(entryName);
        Path = path;
        EntryName = entryName;
        workerId = nextWorkerId++;
        Install();
        Active[workerId] = this;
    }

    /// <summary>The worker app's path.</summary>
    public string Path { get; }

    /// <summary>The entry the worker runs.</summary>
    public string EntryName { get; }

    /// <summary>Raised for each progress report, until the worker finishes or is cancelled.</summary>
    public event Action<WorkerProgressEventArgs>? Progress;

    /// <summary>Raised when the worker finishes.</summary>
    public event Action<WorkerCompletedEventArgs>? Completed;

    /// <summary>Raised when the worker fails, including a cancelled worker that reports how it stopped.</summary>
    public event Action<WorkerErrorEventArgs>? Error;

    /// <summary>Starts the worker with <paramref name="input"/>; an input over 1 MiB of UTF-8 raises <see cref="Error"/>.</summary>
    /// <exception cref="InvalidOperationException">The worker was already started: a worker runs one input.</exception>
    /// <exception cref="ObjectDisposedException">The worker was disposed.</exception>
    public Worker Start(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (started)
        {
            throw new InvalidOperationException("A Worker runs one input; create another Worker to run again.");
        }
        ObjectDisposedException.ThrowIf(finished, this);
        started = true;
        if (Encoding.UTF8.GetByteCount(input) > MaxStartInputBytes)
        {
            DispatchError("Worker.Start input exceeds the maximum UTF-8 payload size.");
            return this;
        }
        Runtime.Abi.HostWorkerStartString(workerId, Path, EntryName, input);
        return this;
    }

    /// <summary>Asks the running worker to stop; its progress reports are no longer raised.</summary>
    public void Cancel()
    {
        if (!started || finished || cancelRequested)
        {
            return;
        }
        cancelRequested = true;
        Runtime.Abi.HostWorkerCancel(workerId);
    }

    /// <summary>Cancels a running worker and raises no further events.</summary>
    public void Dispose()
    {
        if (started && !finished)
        {
            Runtime.Abi.HostWorkerCancel(workerId);
        }
        Finish();
    }

    internal static void DisposeAll()
    {
        Worker[] running = [.. Active.Values];
        foreach (var worker in running)
        {
            worker.Dispose();
        }
    }

    internal static void Reset()
    {
        Active.Clear();
        nextWorkerId = 1;
    }

    // Workers clear themselves when the root is replaced, and route the host's messages, once the first is created.
    private static void Install()
    {
        if (installed)
        {
            return;
        }
        installed = true;
        HostEvents.WorkerMessage = HandleMessage;
        Application.Unmounting += DisposeAll;
    }

    private static void HandleMessage(uint workerId, uint kind, string text)
    {
        if (!Active.TryGetValue(workerId, out var worker))
        {
            return;
        }
        switch ((WorkerMessageKind)kind)
        {
            case WorkerMessageKind.Progress:
                worker.DispatchProgress(text);
                break;
            case WorkerMessageKind.Complete:
                worker.DispatchComplete(text);
                break;
            default:
                worker.DispatchError(text);
                break;
        }
    }

    private void DispatchProgress(string message)
    {
        if (!finished && !cancelRequested)
        {
            Progress?.Invoke(new WorkerProgressEventArgs(message));
        }
    }

    private void DispatchComplete(string result)
    {
        if (finished)
        {
            return;
        }
        var handler = Completed;
        Finish();
        handler?.Invoke(new WorkerCompletedEventArgs(result));
    }

    private void DispatchError(string message)
    {
        if (finished)
        {
            return;
        }
        var handler = Error;
        Finish();
        handler?.Invoke(new WorkerErrorEventArgs(message));
    }

    private void Finish()
    {
        if (finished)
        {
            return;
        }
        finished = true;
        Progress = null;
        Completed = null;
        Error = null;
        Active.Remove(workerId);
    }
}

/// <summary>The kinds of message a worker sends the page, as <c>__fui_on_worker_message</c> passes them.</summary>
internal enum WorkerMessageKind : uint
{
    Progress = 0,
    Complete = 1,
    Error = 2,
}

/// <summary>Fluent configuration for <see cref="Worker"/>.</summary>
public static class WorkerExtensions
{
    extension(Worker worker)
    {
        /// <summary>Adds a handler for progress messages.</summary>
        public Worker OnProgress(Action<WorkerProgressEventArgs> handler)
        {
            worker.Progress += handler;
            return worker;
        }

        /// <summary>Adds a handler for the result.</summary>
        public Worker OnCompleted(Action<WorkerCompletedEventArgs> handler)
        {
            worker.Completed += handler;
            return worker;
        }

        /// <summary>Adds a handler for errors.</summary>
        public Worker OnError(Action<WorkerErrorEventArgs> handler)
        {
            worker.Error += handler;
            return worker;
        }
    }
}
