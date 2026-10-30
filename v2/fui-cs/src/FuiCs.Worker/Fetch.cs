using System.Text;

namespace FuiCs.Workers;

/// <summary>The host's answer to a worker's <see cref="FetchRequest"/>: its status and final URL, without the body.</summary>
public sealed record FetchResponse(bool IsSuccessStatusCode, int StatusCode, string StatusText, string Url);

/// <summary>The worker could not complete a <see cref="FetchRequest"/> (a network failure or a blocked request).</summary>
public sealed class FetchException(string message) : Exception(message);

/// <summary>
/// An HTTP request a worker job sends through its Web Worker, as the page's <c>FetchRequest</c> does; a relative URL
/// resolves against the worker's script.
/// </summary>
public sealed class FetchRequest(string url)
{
    private static uint nextRequestId = 1;
    private readonly List<KeyValuePair<string, string>> headers = [];

    /// <summary>The URL to request; a relative URL resolves against the worker's script.</summary>
    public string Url { get; } = url ?? throw new ArgumentNullException(nameof(url));

    /// <summary>The HTTP method. GET by default.</summary>
    public string Method
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = "GET";

    /// <summary>The headers <see cref="AddHeader"/> added, in order.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Headers => headers;

    /// <summary>The request body; null sends none.</summary>
    public byte[]? Body { get; set; }

    /// <summary>Adds a header to send; a name added twice is sent twice.</summary>
    /// <exception cref="ArgumentException">The name or value has a line break, which no HTTP header can.</exception>
    public FetchRequest AddHeader(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        if (name.Contains('\n') || value.Contains('\n'))
        {
            throw new ArgumentException("A header name or value cannot contain a line break.");
        }
        headers.Add(new KeyValuePair<string, string>(name, value));
        return this;
    }

    /// <summary>Whether the request has been sent; a request is sent once.</summary>
    public bool IsSent { get; private set; }

    /// <summary>Sends the request; cancelling the token aborts it.</summary>
    /// <exception cref="FetchException">The worker could not complete the request.</exception>
    /// <exception cref="OperationCanceledException">The token was cancelled first.</exception>
    /// <exception cref="InvalidOperationException">No worker job is running, or the request was already sent.</exception>
    public async Task<FetchResponse> SendAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var host = WorkerRuntime.Host ?? throw new InvalidOperationException("A FetchRequest is sent from a running worker job.");
        if (IsSent)
        {
            throw new InvalidOperationException("A FetchRequest is sent once; create another to send again.");
        }
        IsSent = true;
        var requestId = nextRequestId++;
        // One line per header name and value; HTTP forbids line breaks in either.
        var encodedHeaders = new StringBuilder();
        foreach (var (name, value) in headers)
        {
            encodedHeaders.Append(name).Append('\n').Append(value).Append('\n');
        }
        using var registration = cancellationToken.Register(() => host.CancelFetch(requestId));
        string encoded;
        try
        {
            encoded = await host.FetchAsync(requestId, Method, Url, encodedHeaders.ToString(), Body ?? []);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new FetchException(exception.Message);
        }
        // The host answers "ok\nstatus\nstatusText\nurl".
        var parts = encoded.Split('\n');
        return new FetchResponse(parts[0] == "1", int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), parts[2], parts[3]);
    }
}

/// <summary>Fluent configuration for <see cref="FetchRequest"/>.</summary>
public static class FetchRequestExtensions
{
    extension(FetchRequest request)
    {
        /// <summary>Sets <see cref="FetchRequest.Method"/>.</summary>
        public FetchRequest Method(string method)
        {
            request.Method = method;
            return request;
        }

        /// <summary>Adds a header, as <see cref="FetchRequest.AddHeader"/> does.</summary>
        public FetchRequest Header(string name, string value) => request.AddHeader(name, value);

        /// <summary>Sets the body to <paramref name="body"/>.</summary>
        public FetchRequest Body(byte[] body)
        {
            ArgumentNullException.ThrowIfNull(body);
            request.Body = body;
            return request;
        }

        /// <summary>Sets the body to <paramref name="text"/> in UTF-8.</summary>
        public FetchRequest Body(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            request.Body = Encoding.UTF8.GetBytes(text);
            return request;
        }
    }
}
