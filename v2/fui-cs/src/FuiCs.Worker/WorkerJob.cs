namespace FuiCs.Workers;

/// <summary>
/// A worker job with state: <see cref="OnStart"/> receives the input, then <see cref="Run"/> does the work, awaiting
/// <see cref="YieldAsync"/> between slices. Register it with <c>entries.Add("name", input =&gt; new MyJob().Start(input))</c>.
/// </summary>
public abstract class WorkerJob
{
    private bool started;
    private bool finished;

    /// <summary>Starts the job with <paramref name="input"/> and runs it to its end.</summary>
    /// <exception cref="InvalidOperationException">The job was already started.</exception>
    public async Task Start(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (started)
        {
            throw new InvalidOperationException("A WorkerJob runs once.");
        }
        started = true;
        OnStart(input);
        if (!finished)
        {
            await Run();
        }
    }

    /// <summary>Receives the page's input before <see cref="Run"/>; completing or failing here skips the run.</summary>
    protected virtual void OnStart(string input)
    {
    }

    /// <summary>Does the job's work, then completes or fails it; returning without either fails the job.</summary>
    protected abstract Task Run();

    /// <summary>Whether the page has cancelled the job; seen when the job yields.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "A job reads it beside its other members.")]
    protected bool IsCancelled => WorkerRuntime.IsCancelled;

    /// <summary>Reports progress to the page; ignored once the job has completed or failed.</summary>
    protected void ReportProgress(string progress)
    {
        if (!finished)
        {
            WorkerRuntime.ReportProgress(progress);
        }
    }

    /// <summary>Completes the job with <paramref name="result"/>; later results and failures are ignored.</summary>
    protected void Complete(string result)
    {
        if (!finished)
        {
            finished = true;
            WorkerRuntime.Complete(result);
        }
    }

    /// <summary>Fails the job with <paramref name="message"/>; later results and failures are ignored.</summary>
    protected void Fail(string message)
    {
        if (!finished)
        {
            finished = true;
            WorkerRuntime.Fail(message);
        }
    }

    /// <summary>
    /// Lets the worker handle the page's calls (a cancellation) before the job continues, after
    /// <paramref name="delay"/> if one is given. Returns false once the job has completed or failed.
    /// </summary>
    protected Task<bool> YieldAsync(TimeSpan delay = default) => finished ? Task.FromResult(false) : WorkerRuntime.YieldAsync(delay);
}
