using System.Globalization;

namespace FuiCs;

/// <summary>
/// Saves part of a node's state into the host's per-history-entry UI state and restores it when the user returns,
/// for nodes with a <see cref="Node.NodeId"/>. <see cref="Kind"/> names the state; <see cref="Version"/> lets
/// <see cref="Restore"/> read payloads an older version saved.
/// </summary>
public abstract class PersistedStateAdapter
{
    /// <summary>An adapter for state of <paramref name="kind"/>, saved at <paramref name="version"/>.</summary>
    protected PersistedStateAdapter(string kind, uint version = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(kind);
        Kind = kind;
        Version = version;
    }

    /// <summary>The state's kind, which keys it with the node's id.</summary>
    public string Kind { get; }

    /// <summary>The format version, passed back on restore.</summary>
    public uint Version { get; }

    /// <summary>The payload to save for <paramref name="node"/>, or null to save nothing.</summary>
    public abstract string? Capture(Node node);

    /// <summary>
    /// Restores <paramref name="payload"/>, saved at <paramref name="version"/>, into <paramref name="node"/>.
    /// </summary>
    public abstract void Restore(Node node, string payload, uint version);
}

/// <summary>Converts a persisted value to and from its saved text.</summary>
public sealed record PersistedCodec<TValue>(Func<TValue, string> Encode, Func<string, uint, TValue> Decode);

/// <summary>Codecs for common values, in the invariant culture.</summary>
public static class PersistedCodecs
{
    /// <summary>Saves text as it is.</summary>
    public static PersistedCodec<string> Text { get; } = new(value => value, (payload, _) => payload);

    /// <summary>Saves a Boolean as true or false.</summary>
    public static PersistedCodec<bool> Boolean { get; } = new(value => value ? "true" : "false", (payload, _) => payload == "true");

    /// <summary>Saves an integer in the invariant culture.</summary>
    public static PersistedCodec<int> Int32 { get; } = new(
        value => value.ToString(CultureInfo.InvariantCulture),
        (payload, _) => int.Parse(payload, CultureInfo.InvariantCulture));

    /// <summary>Saves a float in the invariant culture.</summary>
    public static PersistedCodec<float> Single { get; } = new(
        value => value.ToString(CultureInfo.InvariantCulture),
        (payload, _) => float.Parse(payload, CultureInfo.InvariantCulture));
}

/// <summary>Persists one value of a node, read with <paramref name="capture"/> and written back with <paramref name="restore"/>.</summary>
public sealed class PersistedValue<TNode, TValue>(
    string kind,
    Func<TNode, TValue> capture,
    Action<TNode, TValue> restore,
    PersistedCodec<TValue> codec,
    uint version = 1) : PersistedStateAdapter(kind, version)
    where TNode : Node
{
    /// <summary>When set and false for a node, nothing is saved for it.</summary>
    public Func<TNode, bool>? ShouldCapture { get; init; }

    /// <inheritdoc/>
    public override string? Capture(Node node)
    {
        var typed = (TNode)node;
        return ShouldCapture is { } shouldCapture && !shouldCapture(typed) ? null : codec.Encode(capture(typed));
    }

    /// <inheritdoc/>
    public override void Restore(Node node, string payload, uint version) => restore((TNode)node, codec.Decode(payload, version));
}

/// <summary>The host's per-history-entry UI state.</summary>
internal static class PersistedUiState
{
    public static void StoreScrollOffset(string nodeId, float x, float y) => Runtime.Abi.HostSetPersistedScrollOffset(nodeId, x, y);

    public static bool TryLoadScrollOffset(string nodeId, out float x, out float y) =>
        Runtime.Abi.HostTryGetPersistedScrollOffset(nodeId, out x, out y);

    public static void StoreText(string nodeId, string kind, uint version, string payload) =>
        Runtime.Abi.HostSetPersistedState(nodeId, kind, version, payload);

    public static bool TryLoadText(string nodeId, string kind, out uint version, out string payload) =>
        Runtime.Abi.HostTryCopyPersistedState(nodeId, kind, out version, out payload);
}
