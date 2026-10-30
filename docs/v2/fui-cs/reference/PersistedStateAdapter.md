# PersistedStateAdapter

class in `FuiCs`

Saves part of a node's state into the host's per-history-entry UI state and restores it when the user returns, for nodes with a [`Node.NodeId`](Node.md). [`PersistedStateAdapter.Kind`](PersistedStateAdapter.md) names the state; [`PersistedStateAdapter.Version`](PersistedStateAdapter.md) lets [`PersistedStateAdapter.Restore`](PersistedStateAdapter.md) read payloads an older version saved.

## Properties

- `Kind`: The state's kind, which keys it with the node's id.
- `Version`: The format version, passed back on restore.

## Methods

- `Capture(Node)`: The payload to save for `node`, or null to save nothing.
- `Restore(Node, string, uint)`: Restores `payload`, saved at `version`, into `node`.
