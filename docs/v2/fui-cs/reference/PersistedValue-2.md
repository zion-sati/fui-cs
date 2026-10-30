# PersistedValue<TNode, TValue>

class in `FuiCs`: [`PersistedStateAdapter`](PersistedStateAdapter.md)

Persists one value of a node, read with `capture` and written back with `restore`.

## Properties

- `ShouldCapture`: When set and false for a node, nothing is saved for it.

## Methods

- `Capture(Node)`: The payload to save for `node`, or null to save nothing.
- `Restore(Node, string, uint)`: Restores `payload`, saved at `version`, into `node`.
