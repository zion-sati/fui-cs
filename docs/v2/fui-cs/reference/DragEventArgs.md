# DragEventArgs

class in `FuiCs`: `EventArgs`

A drag entering, moving over, leaving or dropping on a node.

## Properties

- `Modifiers`: The modifier keys held.
- `Proposal`: Set by drag-enter and drag-over handlers to accept the drop. Starts as [`DropProposal.None`](DropProposal.md) for each.
- `Session`: The drag.
- `X`: The pointer, relative to the node.
- `Y`: The pointer, relative to the node.
