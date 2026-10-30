using System.Globalization;

using FuiCs.Workers;

namespace FuiCs.TestWorker;

/// <summary>The jobs the worker test page runs.</summary>
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) => entries
        .Add("count", CountAsync)
        .Add("fail", Fail)
        .Add("silent", Silent)
        .Add("throws", Throws)
        .Add("slow", input => new SlowJob().Start(input))
        .Add("services", CallServices)
        .Add("fetch", FetchAsync);

    /// <summary>Fetches the input URL from inside the worker and completes with the status.</summary>
    private static async Task FetchAsync(string input)
    {
        try
        {
            var response = await new FetchRequest(input).Header("X-Fui-Worker", "1").SendAsync();
            WorkerRuntime.Complete((response.IsSuccessStatusCode ? "ok " : "not ok ") + response.StatusCode.ToString(CultureInfo.InvariantCulture));
        }
        catch (FetchException)
        {
            WorkerRuntime.Fail("fetch failed");
        }
    }

    /// <summary>Calls each of the worker's own host services and completes with what they returned.</summary>
    private static void CallServices(string input)
    {
        var described = ProbeWorker.Describe(input, -9_007_199_254_740_993L, [1u, 2u, 3u], true);
        var scaled = ProbeWorker.Scale([1.5, -2.0], 2.0);
        var reversed = ProbeWorker.ReverseBytes([1, 2, 3]);
        WorkerRuntime.Complete(string.Join(
            "|",
            described,
            string.Join(",", scaled),
            string.Join(",", reversed),
            ProbeWorker.InWorker() ? "in worker" : "on page"));
    }

    /// <summary>Reports each step up to the input's count, yielding between them, then completes.</summary>
    private static async Task CountAsync(string input)
    {
        var total = int.Parse(input, CultureInfo.InvariantCulture);
        for (var step = 1; step <= total; ++step)
        {
            WorkerRuntime.ReportProgress(step.ToString(CultureInfo.InvariantCulture));
            await WorkerRuntime.YieldAsync();
        }
        WorkerRuntime.Complete("counted " + total.ToString(CultureInfo.InvariantCulture));
    }

    private static void Fail(string input) => WorkerRuntime.Fail("failed: " + input);

    private static void Silent(string input)
    {
    }

    private static void Throws(string input) => throw new InvalidOperationException("thrown: " + input);

    /// <summary>Runs a hundred 20 ms slices, reporting each, until done or cancelled.</summary>
    private sealed class SlowJob : WorkerJob
    {
        private const int Steps = 100;

        protected override async Task Run()
        {
            for (var step = 0; step < Steps; ++step)
            {
                if (IsCancelled)
                {
                    Fail("cancelled at " + step.ToString(CultureInfo.InvariantCulture));
                    return;
                }
                ReportProgress(step.ToString(CultureInfo.InvariantCulture));
                await YieldAsync(TimeSpan.FromMilliseconds(20));
            }
            Complete("slow done");
        }
    }
}
