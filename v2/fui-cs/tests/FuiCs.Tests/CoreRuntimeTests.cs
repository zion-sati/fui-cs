using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class CoreRuntimeTests
{
    [Test]
    public async Task SignalRaisesChangedForNewValuesOnlyAndSchedulesACommit()
    {
        var abi = TestRuntime.Start();
        var signal = new Signal<int>(1);
        var seen = new List<int>();
        signal.Changed += value => seen.Add(value);
        signal.Changed += value => seen.Add(value * 10);

        var changed = signal.Set(2);
        var unchanged = signal.Set(2);

        await Assert.That(changed).IsTrue();
        await Assert.That(unchanged).IsFalse();
        await Assert.That(string.Join(",", seen)).IsEqualTo("2,20");
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(1);
    }

    [Test]
    public async Task SignalWithoutListenersDoesNotScheduleACommit()
    {
        var abi = TestRuntime.Start();
        var signal = new Signal<string>("a") { Value = "b" };

        await Assert.That(signal.Value).IsEqualTo("b");
        await Assert.That(signal.HasListeners).IsFalse();
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(0);
    }

    [Test]
    public async Task DisposingASubscriptionUnsubscribesOnce()
    {
        TestRuntime.Start();
        var signal = new Signal<int>(0);
        var seen = new List<int>();
        var subscription = signal.Subscribe(value => seen.Add(value));

        signal.Value = 1;
        subscription.Dispose();
        subscription.Dispose();
        signal.Value = 2;

        await Assert.That(string.Join(",", seen)).IsEqualTo("1");
        await Assert.That(signal.HasListeners).IsFalse();
    }

    [Test]
    public async Task LoadedHandlersRunOnceAfterTheFirstCommitThenImmediately()
    {
        TestRuntime.Start();
        var log = new List<string>();
        FrameScheduler.OnLoaded(_ => log.Add("early"));

        var beforeFirstCommit = log.Count;
        FrameScheduler.FireLoadedCallbacks();
        FrameScheduler.FireLoadedCallbacks();
        FrameScheduler.OnLoaded(_ => log.Add("late"));

        await Assert.That(beforeFirstCommit).IsEqualTo(0);
        await Assert.That(string.Join(",", log)).IsEqualTo("early,late");
    }

    [Test]
    public async Task AfterNextCommitRunsOnceAfterTheCommit()
    {
        var abi = TestRuntime.Start();
        var log = new List<string>();
        FrameScheduler.MarkNeedsCommit();
        FrameScheduler.AfterNextCommit(() =>
        {
            log.Add(abi.CountOf("ui_commit_frame").ToString(System.Globalization.CultureInfo.InvariantCulture));
            FrameScheduler.AfterNextCommit(() => log.Add("next"));
        });

        FrameScheduler.FlushCommit();
        var afterFirst = string.Join(",", log);
        FrameScheduler.MarkNeedsCommit();
        FrameScheduler.FlushCommit();

        await Assert.That(afterFirst).IsEqualTo("1");
        await Assert.That(string.Join(",", log)).IsEqualTo("1,next");
    }

    [Test]
    public async Task RetainedViewDisposesResourcesInReverseThenItsRoot()
    {
        TestRuntime.Start();
        var log = new List<string>();
        var root = new TestNode();
        var view = new RetainedView(root)
            .Retain(new DisposeProbe(log, "first"))
            .Retain(new DisposeProbe(log, "second"))
            .OnActivate(_ => log.Add("activate"))
            .OnDeactivate(_ => log.Add("deactivate"))
            .OnDispose(_ => log.Add("dispose"));

        view.Activate();
        view.Dispose();
        view.Retain(new DisposeProbe(log, "late"));

        await Assert.That(string.Join(",", log)).IsEqualTo("activate,deactivate,dispose,second,first,late");
        await Assert.That(root.IsDisposed).IsTrue();
        await Assert.That(view.IsActive).IsFalse();
    }
}
