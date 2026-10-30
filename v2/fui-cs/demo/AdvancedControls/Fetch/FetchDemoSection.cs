using System;
using System.Globalization;
using System.Threading;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls.Fetch;

/// <summary>
/// Real GET and POST requests to JSONPlaceholder through the fetch API, one at a time: the buttons are disabled while a
/// request runs, and the status, request and result lines report its progress and completion metadata.
/// </summary>
internal sealed class FetchDemoSection : IDisposable
{
    private const string BaseUrl = "https://jsonplaceholder.typicode.com";
    private const string GetUrl = BaseUrl + "/posts/1";
    private const string PostUrl = BaseUrl + "/posts";
    private const string PostBody = "{\"title\":\"EffinDom advanced-controls demo\",\"body\":\"Posting through the shipped Fetch API.\",\"userId\":29}";

    private readonly DemoButton getButton;
    private readonly DemoButton postButton;
    private readonly Text statusText = new DemoText("Fetch status: idle", DemoTextRecipe.StatusValue).FontSize(15.0f).NodeId("fetch-status");
    private readonly Text requestText = new DemoText("Latest request: none", DemoTextRecipe.StatusSupporting).FontSize(15.0f).MaxLines(3).NodeId("fetch-request");
    private readonly Text resultText = new DemoText("Latest result: none yet", DemoTextRecipe.StatusSupporting).FontSize(15.0f).MaxLines(5).NodeId("fetch-result");
    private readonly FlexBox body;
    private CancellationTokenSource? activeRequest;

    public FetchDemoSection()
    {
        getButton = new DemoButton("GET /posts/1").Width(156.0f).NodeId("fetch-get").OnClick(_ => StartGet());
        postButton = new DemoButton("POST /posts", DemoButtonTone.Primary).Width(156.0f).NodeId("fetch-post").OnClick(_ => StartPost());
        var hint = new DemoText(
                "This demo uses the shipped Fetch API against the live JSONPlaceholder service. The request is real and online; the current Fetch surface reports completion metadata (ok, status, statusText, resolved url) rather than response bodies.",
                DemoTextRecipe.Supporting)
            .FontSize(15.0f)
            .MaxLines(6);
        body = FlexBox.Column(
                FlexBox.Row(getButton, new FlexBox().Width(12.0f).Height(1.0f), postButton).FillWidth(),
                Spacer(12.0f),
                statusText,
                Spacer(6.0f),
                requestText,
                Spacer(6.0f),
                resultText,
                Spacer(10.0f),
                hint)
            .FillWidth();
        SyncButtons();
    }

    public RoutePageSection BuildSection() => RoutePageSection.Create(
        "Online Fetch sample",
        "Send real GET and POST requests through the shipped Fetch API to JSONPlaceholder without dropping to browser-specific networking code.",
        body);

    public void Dispose()
    {
        var request = activeRequest;
        activeRequest = null;
        request?.Cancel();
        request?.Dispose();
    }

    private static FlexBox Spacer(float height) => new FlexBox().FillWidth().Height(height);

    private void StartGet() => Start("GET " + GetUrl, new FetchRequest(GetUrl));

    private void StartPost() => Start(
        "POST " + PostUrl,
        new FetchRequest(PostUrl).Method("POST").Header("Content-Type", "application/json; charset=UTF-8").Body(PostBody));

    private async void Start(string label, FetchRequest request)
    {
        if (activeRequest is not null)
        {
            statusText.Text = "Fetch status: wait for the current request to finish";
            return;
        }
        var cancellation = new CancellationTokenSource();
        activeRequest = cancellation;
        statusText.Text = "Fetch status: running";
        requestText.Text = "Latest request: " + label;
        resultText.Text = "Latest result: waiting for JSONPlaceholder to respond...";
        SyncButtons();
        try
        {
            var response = await request.SendAsync(cancellation.Token);
            Finish(cancellation, "Fetch status: complete", string.Create(
                CultureInfo.InvariantCulture,
                $"Latest result: {label} -> ok={(response.IsSuccessStatusCode ? "true" : "false")} • status {response.StatusCode} {response.StatusText} • resolved url {response.Url}"));
        }
        catch (FetchException exception)
        {
            Finish(cancellation, "Fetch status: error", "Latest result: " + label + " -> error • " + exception.Message);
        }
        catch (OperationCanceledException)
        {
            // Disposed with the page, or the page was replaced: nothing is left to update.
        }
    }

    private void Finish(CancellationTokenSource request, string status, string result)
    {
        if (!ReferenceEquals(activeRequest, request))
        {
            return;
        }
        activeRequest = null;
        request.Dispose();
        statusText.Text = status;
        resultText.Text = result;
        SyncButtons();
    }

    private void SyncButtons()
    {
        var idle = activeRequest is null;
        getButton.IsEnabled = idle;
        postButton.IsEnabled = idle;
    }
}
