namespace FuiCs.Workers.Hosting;

/// <summary>Runs the worker's entries for its generated exports.</summary>
public static class WorkerHosting
{
    /// <summary>
    /// Runs <paramref name="entryName"/> with <paramref name="input"/> and returns the job's result; a failed job, and
    /// an unknown entry, throw <see cref="WorkerJobException"/>. Each run is the only job of its Web Worker, so the
    /// entries are registered for it.
    /// </summary>
    public static Task<string> Run(IWorkerHost host, Func<WorkerEntries> register, string entryName, string input)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(register);
        if (!register().TryGet(entryName, out var entry))
        {
            return Task.FromException<string>(new WorkerJobException("The worker has no entry named " + entryName + "."));
        }
        return WorkerRuntime.RunAsync(host, entry, input);
    }

    /// <summary>Cancels the running job, for the page's cancel call; the job sees it when it next yields.</summary>
    public static void Cancel() => WorkerRuntime.Cancel();
}
