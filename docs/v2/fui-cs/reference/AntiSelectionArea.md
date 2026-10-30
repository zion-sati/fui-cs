# AntiSelectionArea

class in `FuiCs`: [`FlexBox`](FlexBox.md)

Keeps an enclosing [`SelectionArea`](SelectionArea.md) from selecting the text inside it. A [`SelectionArea`](SelectionArea.md) nested inside starts its own independent selection.

## Properties

- `IsSelectionBarrier`: Whether text selection does not extend across the node, as for controls with their own text.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
