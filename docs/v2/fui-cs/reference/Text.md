# Text

class in `FuiCs`: [`TextCore`](TextCore.md)

Display text: selectable by default, and announced as static text labelled with its content unless an ancestor already has a semantic role.

Fluent members: [`TextExtensions`](TextExtensions.md).

## Properties

- `SemanticLabel`: The accessibility label: the label set here, otherwise the node's default label.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `OnContentChanged()`: Called after the text changes, from code or by the user.
- `OnParentChanged()`: Called when the node is attached to or detached from a parent.
- `SetOverflowFade(bool, bool)`: Fades text that overflows its box horizontally and/or vertically.
- `ShouldApplyDefaultSemantics()`: Non-empty text whose ancestors have no semantic role of their own.
