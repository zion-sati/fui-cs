using FuiCs;

namespace FuiCs.Browser;

/// <summary>Worker jobs in the browser test app's worker: progress and results, failures, and cancellation.</summary>
internal static class WorkerPage
{
    private const string WorkerPath = "browser/workers/FuiCs.TestWorker";

    public static Node Build()
    {
        var rows = FlexBox.Column().AlignItems(AlignItems.Start);
        foreach (var (entry, input) in new[] { ("count", "5"), ("fail", "bad input"), ("silent", ""), ("throws", "boom"), ("missing", ""), ("services", "hello") })
        {
            rows.Child(Row(entry, input, cancellable: false));
        }
        rows.Child(Row("slow", "", cancellable: true));
        rows.Child(Row("count", "1", cancellable: false, path: "browser/workers/NoSuchWorker", id: "absent"));
        // From the worker's script, three folders up is the app's root (browser/workers/FuiCs.TestWorker/).
        rows.Child(Row("fetch", "../../../assets/halves.png", cancellable: false, id: "fetch-found"));
        rows.Child(Row("fetch", "../../../assets/missing.json", cancellable: false, id: "fetch-missing"));
        rows.Child(Row("fetch", "http://127.0.0.1:9/unreachable", cancellable: false, id: "fetch-unreachable"));
        return rows.Padding(20.0f);
    }

    private static FlexBox Row(string entry, string input, bool cancellable, string path = WorkerPath, string? id = null)
    {
        id ??= entry;
        var log = new Text("idle").FontSize(14.0f).NodeId(id + "-log");
        var progress = new Text("").FontSize(14.0f).NodeId(id + "-progress");
        Worker? running = null;
        var start = Button(id + "-start");
        start.OnPointerClick(_ =>
        {
            log.Text = "running";
            running = new Worker(path, entry)
                .OnProgress(e => progress.Text = e.Message)
                .OnCompleted(e => log.Text = "completed " + e.Result)
                .OnError(e => log.Text = "error " + e.Message)
                .Start(input);
        });
        var row = FlexBox.Row(start, new FlexBox().Width(8.0f), log, new FlexBox().Width(8.0f), progress)
            .AlignItems(AlignItems.Center)
            .Height(30.0f);
        if (cancellable)
        {
            var cancel = Button(id + "-cancel");
            cancel.OnPointerClick(_ => running?.Cancel());
            row.Child(new FlexBox().Width(8.0f)).Child(cancel);
        }
        return row;
    }

    private static FlexBox Button(string id) =>
        new FlexBox().NodeId(id).Width(60.0f).Height(24.0f).Background(LayoutPage.Grey).Cursor(CursorStyle.Pointer);
}
