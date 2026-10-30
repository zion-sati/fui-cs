using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class SubscriptionTests
{
    [Test]
    public async Task ADisposedOwnerStopsReceivingSignalChanges()
    {
        TestRuntime.Start();
        var signal = new Signal<int>(0);
        var seen = new List<int>();
        var owner = new TestNode();
        owner.TrackResource(signal.Subscribe(value => seen.Add(value)));

        signal.Value = 1;
        owner.Dispose();
        signal.Value = 2;

        await Assert.That(string.Join(",", seen)).IsEqualTo("1");
        await Assert.That(signal.HasListeners).IsFalse();
    }

    [Test]
    public async Task TrackedResourcesAreDisposedNewestFirstBeforeTheSubtree()
    {
        TestRuntime.Start();
        var log = new List<string>();
        var owner = new TestNode();
        var child = new TestNode();
        owner.AddChild(child);
        child.TrackResource(new DisposeProbe(log, "child"));
        owner.TrackResource(new DisposeProbe(log, "first"));
        owner.TrackResource(new DisposeProbe(log, "second"));

        owner.Dispose();
        owner.Dispose();

        await Assert.That(string.Join(",", log)).IsEqualTo("second,first,child");
    }

    [Test]
    public async Task SignalsCompareValuesNotInstances()
    {
        TestRuntime.Start();
        var signal = new Signal<string>("same");
        var changes = 0;
        signal.Changed += _ => changes++;

        signal.Value = new string(['s', 'a', 'm', 'e']);
        signal.Value = "different";

        await Assert.That(changes).IsEqualTo(1);
    }
}
