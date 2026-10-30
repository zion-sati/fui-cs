# ISizable

interface in `FuiCs`

A node sized by a fixed length, by filling the space its parent offers, and by min/max limits.

## Properties

- `FillHeight`: Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.
- `FillHeightPercent`: Takes this percentage of the remaining height; null when not set.
- `FillWidth`: Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.
- `FillWidthPercent`: Takes this percentage of the remaining width; null when not set.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `MaxHeight`: The largest height layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MaxWidth`: The largest width layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MinHeight`: The smallest height layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `MinWidth`: The smallest width layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
