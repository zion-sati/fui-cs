namespace FuiCs.Workers;

/// <summary>
/// The named entries a worker project offers; the page starts one with <c>new Worker(path, "name")</c>. The worker
/// project registers them in <c>FuiWorker.Register(WorkerEntries entries)</c>.
/// </summary>
public sealed class WorkerEntries
{
    private readonly Dictionary<string, Func<string, Task>> entries = new(StringComparer.Ordinal);

    /// <summary>Adds the entry <paramref name="name"/>, an async job given the page's input; a later entry of the same name replaces it.</summary>
    public WorkerEntries Add(string name, Func<string, Task> entry)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(entry);
        entries[name] = entry;
        return this;
    }

    /// <summary>Adds an entry that does all its work at once, without yielding.</summary>
    public WorkerEntries Add(string name, Action<string> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return Add(name, input =>
        {
            entry(input);
            return Task.CompletedTask;
        });
    }

    /// <summary>Finds the entry <paramref name="name"/>, as the worker does when the page starts it.</summary>
    public bool TryGet(string name, out Func<string, Task> entry) =>
        entries.TryGetValue(name, out entry!);
}
