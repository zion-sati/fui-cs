using System.Buffers.Binary;
using System.Text;

using FuiCs.Hosting;

namespace FuiCs;

/// <summary>The host's answer to a <see cref="FetchRequest"/>: its status and final URL. The host does not return the body.</summary>
public sealed record FetchResponse(bool IsSuccessStatusCode, int StatusCode, string StatusText, string Url);

/// <summary>The host could not complete a <see cref="FetchRequest"/> (a network failure or a blocked request).</summary>
public sealed class FetchException(string message) : Exception(message);

/// <summary>
/// An HTTP request sent through the host. It reports the response status, not the body; use an HTTP client for
/// responses you need to read.
/// </summary>
public sealed class FetchRequest(string url)
{
    private readonly List<KeyValuePair<string, string>> headers = [];

    /// <summary>The request's URL.</summary>
    public string Url { get; } = url ?? throw new ArgumentNullException(nameof(url));

    /// <summary>The HTTP method; GET until set.</summary>
    public string Method
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = "GET";

    /// <summary>The headers added so far, in order.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Headers => headers;

    /// <summary>The request body; null sends none.</summary>
    public byte[]? Body { get; set; }

    /// <summary>Adds a header.</summary>
    public FetchRequest AddHeader(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        headers.Add(new KeyValuePair<string, string>(name, value));
        return this;
    }

    /// <summary>Whether the request has been sent; a request is sent once.</summary>
    public bool IsSent { get; private set; }

    /// <summary>Sends the request; cancelling the token cancels it in the host.</summary>
    /// <exception cref="FetchException">The host could not complete the request.</exception>
    /// <exception cref="OperationCanceledException">The token was cancelled first.</exception>
    /// <exception cref="InvalidOperationException">The URL is empty, or the request was already sent.</exception>
    public Task<FetchResponse> SendAsync(CancellationToken cancellationToken = default)
    {
        if (Url.Length == 0)
        {
            throw new InvalidOperationException("A FetchRequest needs a URL.");
        }
        if (IsSent)
        {
            throw new InvalidOperationException("A FetchRequest is sent once; create another to send again.");
        }
        IsSent = true;
        return FetchRegistry.Send(this, cancellationToken);
    }

    internal byte[] EncodeHeaders()
    {
        var parts = new string[headers.Count * 2];
        for (var index = 0; index < headers.Count; ++index)
        {
            parts[index * 2] = headers[index].Key;
            parts[(index * 2) + 1] = headers[index].Value;
        }
        return TextParts.Encode(parts);
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

        /// <summary>Adds a header.</summary>
        public FetchRequest Header(string name, string value) => request.AddHeader(name, value);

        /// <summary>Sets the request's body to raw bytes.</summary>
        public FetchRequest Body(byte[] body)
        {
            ArgumentNullException.ThrowIfNull(body);
            request.Body = body;
            return request;
        }

        /// <summary>Sets the request's body.</summary>
        public FetchRequest Body(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            request.Body = Encoding.UTF8.GetBytes(text);
            return request;
        }
    }
}

/// <summary>The host's text-parts block: a little-endian count, then each part as a length and its UTF-8 bytes.</summary>
internal static class TextParts
{
    public static byte[] Encode(ReadOnlySpan<string> parts)
    {
        var size = 4;
        foreach (var part in parts)
        {
            size += 4 + Encoding.UTF8.GetByteCount(part);
        }
        var bytes = new byte[size];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, (uint)parts.Length);
        var cursor = 4;
        foreach (var part in parts)
        {
            var length = Encoding.UTF8.GetBytes(part, bytes.AsSpan(cursor + 4));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(cursor), (uint)length);
            cursor += 4 + length;
        }
        return bytes;
    }

    public static string[] Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 4)
        {
            return [];
        }
        var count = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        var parts = new List<string>();
        var cursor = 4;
        for (var index = 0u; index < count && cursor + 4 <= bytes.Length; ++index)
        {
            var length = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes[cursor..]);
            cursor += 4;
            if (length < 0 || cursor + length > bytes.Length)
            {
                break;
            }
            parts.Add(Encoding.UTF8.GetString(bytes.Slice(cursor, length)));
            cursor += length;
        }
        return [.. parts];
    }
}

/// <summary>Requests the host is running, completed by its fetch callbacks.</summary>
internal static class FetchRegistry
{
    private sealed class Pending
    {
        public TaskCompletionSource<FetchResponse> Completion { get; } = new();

        public CancellationTokenRegistration Cancellation { get; set; }
    }

    private static readonly Dictionary<uint, Pending> pending = [];
    private static uint nextRequestId = 1;

    static FetchRegistry()
    {
        HostEvents.FetchCompleted = Complete;
        HostEvents.FetchFailed = Fail;
        Application.Unmounting += CancelAll;
    }

    public static Task<FetchResponse> Send(FetchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestId = nextRequestId++;
        var entry = new Pending();
        pending[requestId] = entry;
        Runtime.Abi.FetchStart(requestId, request.Method, request.Url, request.EncodeHeaders(), request.Body ?? []);
        if (cancellationToken.CanBeCanceled)
        {
            entry.Cancellation = cancellationToken.Register(() =>
            {
                if (pending.Remove(requestId))
                {
                    Runtime.Abi.FetchCancel(requestId);
                    entry.Completion.TrySetCanceled(cancellationToken);
                }
            });
        }
        return entry.Completion.Task;
    }

    public static void Reset()
    {
        pending.Clear();
        nextRequestId = 1;
    }

    /// <summary>Cancels every request in flight on the host; each awaiting task completes as cancelled.</summary>
    public static void CancelAll()
    {
        KeyValuePair<uint, Pending>[] inFlight = [.. pending];
        pending.Clear();
        foreach (var (requestId, entry) in inFlight)
        {
            Runtime.Abi.FetchCancel(requestId);
            entry.Cancellation.Dispose();
            entry.Completion.TrySetCanceled();
        }
    }

    private static void Complete(uint requestId, bool ok, int status, byte[] payload)
    {
        if (!pending.Remove(requestId, out var entry))
        {
            return;
        }
        entry.Cancellation.Dispose();
        var parts = TextParts.Decode(payload);
        entry.Completion.TrySetResult(new FetchResponse(ok, status, parts.Length > 0 ? parts[0] : "", parts.Length > 1 ? parts[1] : ""));
    }

    private static void Fail(uint requestId, string message)
    {
        if (!pending.Remove(requestId, out var entry))
        {
            return;
        }
        entry.Cancellation.Dispose();
        entry.Completion.TrySetException(new FetchException(message.Length > 0 ? message : "The fetch request failed."));
    }
}
