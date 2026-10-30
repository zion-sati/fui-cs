using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Workers;
using FuiCs.Workers.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class WorkerRuntimeTests
{
    private sealed class RecordingHost : IWorkerHost
    {
        public List<string> Messages { get; } = [];

        public void ReportProgress(string progress) => Messages.Add("Progress " + progress);

        public Task<string> FetchAsync(uint requestId, string method, string url, string headers, byte[] body) =>
            Task.FromResult("1\n200\nOK\n" + url);

        public void CancelFetch(uint requestId)
        {
        }
    }

    /// <summary>The progress the host saw, then the run's result or the message of its failure.</summary>
    private static async Task<string> Run(RecordingHost host, string entryName, string input, Action<WorkerEntries> register)
    {
        try
        {
            var result = await WorkerHosting.Run(host, () =>
            {
                var entries = new WorkerEntries();
                register(entries);
                return entries;
            }, entryName, input);
            host.Messages.Add("Complete " + result);
        }
        catch (WorkerJobException exception)
        {
            host.Messages.Add("Fail " + exception.Message);
        }
        return string.Join("|", host.Messages);
    }

    [Test]
    public async Task AnEntryReportsProgressAcrossYieldsThenCompletesOnce()
    {
        var log = await Run(new RecordingHost(), "count", "2", entries => entries.Add("count", async input =>
        {
            WorkerRuntime.ReportProgress("1");
            await WorkerRuntime.YieldAsync();
            WorkerRuntime.ReportProgress("2");
            WorkerRuntime.Complete("counted " + input);
            WorkerRuntime.ReportProgress("late");
            WorkerRuntime.Fail("late");
        }));

        await Assert.That(log).IsEqualTo("Progress 1|Progress 2|Complete counted 2");
    }

    [Test]
    public async Task ThrowingReturningWithoutAResultAndAnUnknownEntryFail()
    {
        var thrown = await Run(new RecordingHost(), "throws", "x", entries => entries.Add("throws", Throw));
        var silent = await Run(new RecordingHost(), "silent", "x", entries => entries.Add("silent", (Action<string>)(_ => { })));
        var unknown = await Run(new RecordingHost(), "missing", "x", _ => { });

        await Assert.That(thrown).IsEqualTo("Fail thrown x");
        await Assert.That(silent.StartsWith("Fail The worker job returned without", StringComparison.Ordinal)).IsTrue();
        await Assert.That(unknown).IsEqualTo("Fail The worker has no entry named missing.");
    }

    [Test]
    public async Task AWorkerJobStartsWithItsInputAndSeesCancellationWhenItYields()
    {
        var host = new RecordingHost();
        var log = await Run(host, "slow", "3", entries => entries.Add("slow", input => new CountingJob().Start(input)));

        await Assert.That(log).IsEqualTo("Progress 0|Progress 1|Fail cancelled at 2 of 3");
    }

    [Test]
    public async Task AJobFetchesThroughItsHostAndAFetchOutsideAJobIsRejected()
    {
        var log = await Run(new RecordingHost(), "fetch", "data.json", entries => entries.Add("fetch", async input =>
        {
            var response = await new FuiCs.Workers.FetchRequest(input).Header("Accept", "text/plain").SendAsync();
            WorkerRuntime.Complete(response.StatusCode + " " + response.StatusText + " " + response.Url + " " + response.IsSuccessStatusCode);
        }));

        await Assert.That(log).IsEqualTo("Complete 200 OK data.json True");
        await Assert.That(Rejects(() => new FuiCs.Workers.FetchRequest("x").Header("Bad", "line\nbreak"))).IsTrue();
    }

    private static bool Rejects(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static void Throw(string input) => throw new InvalidOperationException("thrown " + input);

    private sealed class CountingJob : WorkerJob
    {
        private int total;

        protected override void OnStart(string input) => total = int.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

        protected override async Task Run()
        {
            for (var step = 0; step < total; ++step)
            {
                if (IsCancelled)
                {
                    Fail("cancelled at " + step + " of " + total);
                    return;
                }
                ReportProgress(step.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // The page's cancel call arrives while the job is between slices.
                if (step == 1)
                {
                    WorkerHosting.Cancel();
                }
                await YieldAsync();
            }
            Complete("done");
        }
    }
}
