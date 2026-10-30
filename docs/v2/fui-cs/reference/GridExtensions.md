# GridExtensions

class in `FuiCs`

Fluent configuration for [`Grid`](Grid.md) and for nodes placed in a grid.

## Methods

- `ClearColumnSharedSizeGroup(int)`: Takes column `index` out of its shared size group.
- `ClearRowSharedSizeGroup(int)`: Takes row `index` out of its shared size group.
- `Columns(GridLength[])`: Sets the column widths.
- `ColumnSharedSizeGroup(int, string)`: Sizes column `index` with the others in `group` within the shared size scope.
- `GridCell(int, int, int, int)`: Puts the node in a cell of its parent grid.
- `GridColumn(int)`: Places the node in a grid column.
- `GridColumnSpan(int)`: Spans the node over grid columns.
- `GridRow(int)`: Places the node in a grid row.
- `GridRowSpan(int)`: Spans the node over grid rows.
- `GridSharedSizeScope(bool)`: Makes the node the scope within which grid shared-size groups line up.
- `PlaceChild(Node, int, int, int, int)`: Adds `child` at a cell, spanning rows and columns.
- `Rows(GridLength[])`: Sets the row heights.
- `RowSharedSizeGroup(int, string)`: Sizes row `index` with the others in `group` within the shared size scope.
