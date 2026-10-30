# SelectionArea

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A region whose text the user can select across nodes with one drag, as one selection. Text inside it is selectable unless it sets its own selection behavior, sits under a node with its own cursor, or is inside an [`AntiSelectionArea`](AntiSelectionArea.md). [`SelectionArea.SelectedText`](SelectionArea.md) reports what is selected.

## Properties

- `SelectedText`: The text selected across the area; empty when nothing is.

## Methods

- `AddContent(Node)`: Adds `node` to the container's content.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `ClearCurrentSelection()`: Clears the page's text selection, whether it spans a selection area or sits in one text node.
- `HandleCrossSelectionChanged(string)`: Called when a selection area's selection across nodes changes, with its text.
- `PrepareSelectionDefaults(Node, bool)`: Makes default text selectable, except under a node that sets its own cursor or a selection barrier.
- `SetContent(ReadOnlySpan<Node>)`: Replaces the content with `nodes`.

## Events

- `SelectedTextChanged`: Raised when the selection across the area changes.
