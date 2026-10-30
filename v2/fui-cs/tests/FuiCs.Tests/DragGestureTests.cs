using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DragGestureTests
{
    private static (DragGesture Gesture, List<string> Log) Create()
    {
        TestRuntime.Start();
        var host = new TestNode();
        host.Build();
        var gesture = new DragGesture(host);
        var log = new List<string>();
        gesture.Started += e => log.Add("started " + e.X);
        gesture.Delta += e => log.Add("delta " + e.DeltaX + " total " + e.TotalDeltaX);
        gesture.Completed += e => log.Add((e.Cancelled ? "cancelled " : "completed ") + e.TotalDeltaX);
        gesture.DraggingChanged += dragging => log.Add("dragging " + dragging);
        return (gesture, log);
    }

    [Test]
    public async Task ADragStartsPastTheThresholdAndReportsDeltasUntilRelease()
    {
        var (gesture, log) = Create();

        gesture.HandlePointerDown(0.0f, 0.0f);
        gesture.HandlePointerMove(3.0f, 0.0f);
        gesture.HandlePointerMove(5.0f, 0.0f);
        gesture.HandlePointerMove(8.0f, 0.0f);
        gesture.HandlePointerUp(8.0f, 0.0f);

        await Assert.That(string.Join("|", log))
            .IsEqualTo("dragging True|started 5|delta 5 total 5|delta 3 total 8|dragging False|completed 8");
        await Assert.That(gesture.IsDragging).IsFalse();
    }

    [Test]
    public async Task CancellingAnActiveDragCompletesItAsCancelled()
    {
        var (gesture, log) = Create();

        gesture.HandlePointerDown(0.0f, 0.0f);
        gesture.HandlePointerMove(6.0f, 0.0f);
        gesture.Cancel();

        await Assert.That(string.Join("|", log)).IsEqualTo("dragging True|started 6|delta 6 total 6|dragging False|cancelled 6");
    }

    [Test]
    public async Task ATouchDragWaitsForTheLongPress()
    {
        var (gesture, log) = Create();

        gesture.HandlePointerDown(0.0f, 0.0f, waitForLongPress: true);
        gesture.HandlePointerMove(9.0f, 0.0f);
        var beforeLongPress = log.Count;
        gesture.HandleLongPress(9.0f, 0.0f);

        await Assert.That(beforeLongPress).IsEqualTo(0);
        await Assert.That(string.Join("|", log)).IsEqualTo("dragging True|started 9");
    }

    [Test]
    public async Task AZeroThresholdStartsOnPress()
    {
        var (gesture, log) = Create();
        gesture.Threshold(0.0f);

        gesture.HandlePointerDown(2.0f, 0.0f);

        await Assert.That(string.Join("|", log)).IsEqualTo("dragging True|started 2");
    }
}
