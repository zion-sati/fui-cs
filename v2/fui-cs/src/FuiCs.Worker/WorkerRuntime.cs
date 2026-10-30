namespace FuiCs.Workers;

/// <summary>
/// The running worker job's link to the page: progress reports, then one result or failure. Long jobs await
/// <see cref="YieldAsync"/> between slices so the worker can see <see cref="IsCancelled"/>.
/// </summary>
public static class WorkerRuntime
{
    private static IWorkerHost? host;
    private static bool cancelled;
    private static Terminal terminal;
    private static string terminalText = "";

    private enum Terminal
    {
        None,
        Complete,
        Fail,
    }

    /// <summary>The running job's host, for the worker's fetch requests.</summary>
    internal static IWorkerHost? Host => host;

    /// <summary>Whether the page has cancelled the job; seen when the job yields.</summary>
    public static bool IsCancelled => cancelled;

    /// <summary>Reports progress; ignored once the job has completed or failed.</summary>
    public static void ReportProgress(string progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        if (terminal == Terminal.None)
        {
            host?.ReportProgress(progress);
        }
    }

    /// <summary>Completes the job with <paramref name="result"/>; later results and failures are ignored.</summary>
    public static void Complete(string result)
    {
        ArgumentNullException.ThrowIfNull(result);
        SetTerminal(Terminal.Complete, result);
    }

    /// <summary>Fails the job with <paramref name="message"/>; later results and failures are ignored.</summary>
    public static void Fail(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        SetTerminal(Terminal.Fail, message);
    }

    /// <summary>
    /// Lets the worker handle the page's calls (a cancellation) before the job continues, after
    /// <paramref name="delay"/> if one is given. Returns false once the job has completed or failed.
    /// </summary>
    public static async Task<bool> YieldAsync(TimeSpan delay = default)
    {
        if (terminal != Terminal.None)
        {
            return false;
        }
        // A zero delay still returns to the worker's event loop, where the page's calls are handled.
        await Task.Delay(delay > TimeSpan.Zero ? delay : TimeSpan.FromMilliseconds(1));
        return terminal == Terminal.None;
    }

    /// <summary>Cancels the running job, from the page's cancel call; the job sees it when it next yields.</summary>
    internal static void Cancel() => cancelled = true;

    /// <summary>
    /// Runs <paramref name="entry"/> as the job and returns its result; a failure, an exception, and returning without
    /// a result each throw <see cref="WorkerJobException"/> with the job's message.
    /// </summary>
    internal static async Task<string> RunAsync(IWorkerHost jobHost, Func<string, Task> entry, string input)
    {
        host = jobHost;
        cancelled = false;
        terminal = Terminal.None;
        terminalText = "";
        try
        {
            await entry(input);
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
        }
        if (terminal == Terminal.None)
        {
            Fail("The worker job returned without calling WorkerRuntime.Complete(...) or WorkerRuntime.Fail(...).");
        }
        return terminal == Terminal.Complete ? terminalText : throw new WorkerJobException(terminalText);
    }

    private static void SetTerminal(Terminal kind, string text)
    {
        if (terminal != Terminal.None)
        {
            return;
        }
        terminal = kind;
        terminalText = text;
    }
}

/// <summary>A worker job's failure, with the message the job gave; the page receives it as the worker's error.</summary>
public sealed class WorkerJobException(string message) : Exception(message);

/// <summary>The worker's side of the page link, which the worker project's generated exports provide.</summary>
public interface IWorkerHost
{
    /// <summary>Sends a progress report to the page.</summary>
    void ReportProgress(string progress);

    /// <summary>Sends a fetch; the result is "ok\nstatus\nstatusText\nurl" and a failure faults the task.</summary>
    Task<string> FetchAsync(uint requestId, string method, string url, string headers, byte[] body);

    /// <summary>Aborts the fetch <paramref name="requestId"/>, whose task then faults.</summary>
    void CancelFetch(uint requestId);
}
