using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class FetchTests
{
    [Test]
    public async Task ARequestIsSentToTheHostAndCompletesWithItsResponse()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var response = new FetchRequest("https://example.com/api").Method("POST").Header("Accept", "text/plain").Header("X-Ünïcode", "✓")
            .Body("{\"a\":1}").SendAsync();

        var start = abi.Named("fui_fetch_start")[0].Arguments;
        HostEvents.OnFetchComplete(1, true, 201, TextParts.Encode(["Created", "https://example.com/api/1"]));
        var result = await response;

        await Assert.That(start[0]).IsEqualTo((object)1u);
        await Assert.That(start[1]).IsEqualTo((object)"POST");
        await Assert.That(start[2]).IsEqualTo((object)"https://example.com/api");
        await Assert.That(string.Join("|", TextParts.Decode((byte[])start[3]!))).IsEqualTo("Accept|text/plain|X-Ünïcode|✓");
        await Assert.That(Encoding.UTF8.GetString((byte[])start[4]!)).IsEqualTo("{\"a\":1}");
        await Assert.That(result).IsEqualTo(new FetchResponse(true, 201, "Created", "https://example.com/api/1"));
    }

    [Test]
    public async Task ARequestWithoutABodySendsAnEmptyBody()
    {
        var abi = TestRuntime.Start(new LayoutAbi());

        _ = new FetchRequest("/ping").SendAsync();

        await Assert.That(((byte[])abi.Named("fui_fetch_start")[0].Arguments[4]!).Length).IsEqualTo(0);
        await Assert.That(abi.Named("fui_fetch_start")[0].Arguments[1]).IsEqualTo((object)"GET");
    }

    [Test]
    public async Task MountingANewRootCancelsRequestsInFlight()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        Application.Mount(new FlexBox());
        var response = new FetchRequest("/slow").SendAsync();

        Application.Mount(new FlexBox());
        HostEvents.OnFetchComplete(1, true, 200, TextParts.Encode(["late", "/slow"]));

        await Assert.That(response.IsCanceled).IsTrue();
        await Assert.That(abi.HasCall("fui_fetch_cancel", 1u)).IsTrue();
    }

    [Test]
    public async Task AHostFailureFaultsTheTask()
    {
        TestRuntime.Start(new LayoutAbi());
        var response = new FetchRequest("/missing").SendAsync();

        HostEvents.OnFetchError(1, "blocked by CORS");
        var message = "";
        try
        {
            await response;
        }
        catch (FetchException exception)
        {
            message = exception.Message;
        }

        await Assert.That(message).IsEqualTo("blocked by CORS");
    }

    [Test]
    public async Task CancellingCancelsTheHostRequestAndIgnoresALateReply()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        using var cancellation = new CancellationTokenSource();
        var response = new FetchRequest("/slow").SendAsync(cancellation.Token);

        cancellation.Cancel();
        HostEvents.OnFetchComplete(1, true, 200, TextParts.Encode(["OK", "/slow"]));
        var cancelled = false;
        try
        {
            await response;
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }

        await Assert.That(cancelled).IsTrue();
        await Assert.That(abi.HasCall("fui_fetch_cancel", 1u)).IsTrue();
    }

    [Test]
    public async Task RequestsGetDistinctIdsAndAnEmptyUrlIsRejected()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        _ = new FetchRequest("/a").SendAsync();
        _ = new FetchRequest("/b").SendAsync();
        var thrown = false;
        try
        {
            _ = new FetchRequest("").SendAsync();
        }
        catch (InvalidOperationException)
        {
            thrown = true;
        }

        await Assert.That(abi.Arguments("fui_fetch_start", 0)).IsEqualTo("1,2");
        await Assert.That(thrown).IsTrue();
    }

    [Test]
    public async Task ARequestIsSentOnce()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var request = new FetchRequest("/once");
        _ = request.SendAsync();
        var thrown = false;
        try
        {
            _ = request.SendAsync();
        }
        catch (InvalidOperationException)
        {
            thrown = true;
        }

        await Assert.That(request.IsSent).IsTrue();
        await Assert.That(thrown).IsTrue();
        await Assert.That(abi.CountOf("fui_fetch_start")).IsEqualTo(1);
    }

    [Test]
    public async Task APostCarriesItsMethodHeadersAndBody()
    {
        var abi = TestRuntime.Start(new LayoutAbi());

        _ = new FetchRequest("https://example.test/posts")
            .Method("POST")
            .Header("Content-Type", "application/json; charset=UTF-8")
            .Body("{\"title\":\"x\"}")
            .SendAsync();

        var start = abi.Named("fui_fetch_start")[0].Arguments;
        await Assert.That(start[1]).IsEqualTo((object)"POST");
        await Assert.That(start[2]).IsEqualTo((object)"https://example.test/posts");
        await Assert.That(string.Join("|", TextParts.Decode((byte[])start[3]!))).IsEqualTo("Content-Type|application/json; charset=UTF-8");
        await Assert.That(System.Text.Encoding.UTF8.GetString((byte[])start[4]!)).IsEqualTo("{\"title\":\"x\"}");
    }
}
