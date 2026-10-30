using System;
using System.Threading;
using System.Threading.Tasks;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>Fetches through the host: an existing asset, a missing one, an unreachable server and a cancelled request.</summary>
internal static class FetchPage
{
    public static Node Build()
    {
        var found = Status("found-log");
        var missing = Status("missing-log");
        var unreachable = Status("unreachable-log");
        var cancelled = Status("cancelled-log");
        // Mounting cancels requests already in flight, so the page sends them once it has loaded.
        FrameScheduler.OnLoaded(loaded =>
        {
            _ = Report(found, new FetchRequest(ImagePage.Halves).SendAsync());
            _ = Report(missing, new FetchRequest("assets/missing.json").SendAsync());
            _ = Report(unreachable, new FetchRequest("http://127.0.0.1:9/unreachable").SendAsync());
            using var cancellation = new CancellationTokenSource();
            var pending = new FetchRequest(ImagePage.Block).SendAsync(cancellation.Token);
            cancellation.Cancel();
            _ = Report(cancelled, pending);
        });
        return FlexBox.Column(found, missing, unreachable, cancelled).Padding(20.0f).AlignItems(AlignItems.Start);
    }

    private static async Task Report(Text log, Task<FetchResponse> request)
    {
        try
        {
            var response = await request;
            log.Text = (response.IsSuccessStatusCode ? "ok " : "not ok ") + response.StatusCode.ToString();
        }
        catch (FetchException)
        {
            log.Text = "failed";
        }
        catch (OperationCanceledException)
        {
            log.Text = "cancelled";
        }
    }

    private static Text Status(string id) => new Text("pending").FontSize(14.0f).NodeId(id);
}
