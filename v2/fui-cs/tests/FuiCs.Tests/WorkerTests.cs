using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class WorkerTests
{
    private static uint StartedId(LayoutAbi abi) => (uint)abi.Named("fui_worker_start_string")[^1].Arguments[0]!;

    [Test]
    public async Task AWorkerStartsInTheHostAndRaisesProgressThenItsResult()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var events = new List<string>();
        var worker = new Worker("workers/Jobs", "count")
            .OnProgress(e => events.Add("progress " + e.Message))
            .OnCompleted(e => events.Add("completed " + e.Result))
            .Start("3");
        var id = StartedId(abi);

        HostEvents.OnWorkerMessage(id, 0, "1");
        HostEvents.OnWorkerMessage(id, 1, "done");
        HostEvents.OnWorkerMessage(id, 0, "late");

        await Assert.That(abi.HasCall("fui_worker_start_string", id, "workers/Jobs", "count", "3")).IsTrue();
        await Assert.That(string.Join("|", events)).IsEqualTo("progress 1|completed done");
    }

    [Test]
    public async Task CancellingStopsProgressAndAFailureEndsTheWorker()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var events = new List<string>();
        var worker = new Worker("workers/Jobs", "slow")
            .OnProgress(e => events.Add("progress " + e.Message))
            .OnError(e => events.Add("error " + e.Message))
            .Start("");
        var id = StartedId(abi);

        worker.Cancel();
        worker.Cancel();
        HostEvents.OnWorkerMessage(id, 0, "4");
        HostEvents.OnWorkerMessage(id, 2, "cancelled at 4");

        await Assert.That(abi.CountOf("fui_worker_cancel")).IsEqualTo(1);
        await Assert.That(string.Join("|", events)).IsEqualTo("error cancelled at 4");
    }

    [Test]
    public async Task AWorkerRunsOneInputAndAnOversizedInputFails()
    {
        TestRuntime.Start(new LayoutAbi());
        var worker = new Worker("workers/Jobs", "count").Start("1");
        var errors = new List<string>();
        var oversized = new Worker("workers/Jobs", "count").OnError(e => errors.Add(e.Message));

        var startedTwice = Rejects(() => worker.Start("2"));
        oversized.Start(new string('x', (1024 * 1024) + 1));

        await Assert.That(startedTwice).IsTrue();
        await Assert.That(errors.Count).IsEqualTo(1);
    }

    [Test]
    public async Task DisposingOrMountingANewRootCancelsRunningWorkersAndSilencesThem()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        Application.Mount(new FlexBox());
        var events = new List<string>();
        var disposed = new Worker("workers/Jobs", "slow").OnCompleted(e => events.Add("disposed " + e.Result)).Start("");
        var disposedId = StartedId(abi);
        var remounted = new Worker("workers/Jobs", "slow").OnCompleted(e => events.Add("remounted " + e.Result)).Start("");
        var remountedId = StartedId(abi);

        disposed.Dispose();
        Application.Mount(new FlexBox());
        HostEvents.OnWorkerMessage(disposedId, 1, "late");
        HostEvents.OnWorkerMessage(remountedId, 1, "late");

        await Assert.That(abi.HasCall("fui_worker_cancel", disposedId)).IsTrue();
        await Assert.That(abi.HasCall("fui_worker_cancel", remountedId)).IsTrue();
        await Assert.That(events.Count).IsEqualTo(0);
    }

    private static bool Rejects(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
