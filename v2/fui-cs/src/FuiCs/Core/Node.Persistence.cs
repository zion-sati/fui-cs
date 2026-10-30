namespace FuiCs;

public abstract partial class Node
{
    private List<PersistedStateAdapter>? persistedStateAdapters;

    /// <summary>Persists state with <paramref name="adapter"/>, replacing an adapter of the same kind.</summary>
    public void RegisterPersistedState(PersistedStateAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        var adapters = persistedStateAdapters ??= [];
        var index = adapters.FindIndex(existing => existing.Kind == adapter.Kind);
        if (index >= 0)
        {
            adapters[index] = adapter;
        }
        else
        {
            adapters.Add(adapter);
        }
    }

    /// <summary>Saves this node's persisted state. Overrides that persist more should call the base method.</summary>
    protected virtual void CapturePersistedState()
    {
        if (persistedStateAdapters is not { Count: > 0 } adapters || string.IsNullOrEmpty(NodeId))
        {
            return;
        }
        foreach (var adapter in adapters)
        {
            if (adapter.Capture(this) is { } payload)
            {
                PersistedUiState.StoreText(NodeId, adapter.Kind, adapter.Version, payload);
            }
        }
    }

    /// <summary>Restores this node's persisted state. Overrides that persist more should call the base method.</summary>
    protected virtual void RestorePersistedState()
    {
        if (persistedStateAdapters is not { Count: > 0 } adapters || string.IsNullOrEmpty(NodeId))
        {
            return;
        }
        foreach (var adapter in adapters)
        {
            if (PersistedUiState.TryLoadText(NodeId, adapter.Kind, out var version, out var payload))
            {
                adapter.Restore(this, payload, version);
            }
        }
    }

    internal void CapturePersistedStateTree()
    {
        CapturePersistedState();
        foreach (var child in children)
        {
            child.CapturePersistedStateTree();
        }
    }

    /// <summary>Restores children first, so a container restoring its own state sees its content restored.</summary>
    internal void RestorePersistedStateTree()
    {
        foreach (var child in children)
        {
            child.RestorePersistedStateTree();
        }
        RestorePersistedState();
    }
}
