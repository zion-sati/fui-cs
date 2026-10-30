# Grid

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A grid container. Its children sit in cells of [`Grid.Columns`](Grid.md) and [`Grid.Rows`](Grid.md), chosen per child with the attached [`Grid.SetRow`](Grid.md), [`Grid.SetColumn`](Grid.md), [`Grid.SetRowSpan`](Grid.md) and [`Grid.SetColumnSpan`](Grid.md); a child without a cell takes the first one. Tracks, groups and cells apply immediately, before or after build.

Fluent members: [`GridExtensions`](GridExtensions.md).

## Properties

- `Columns`: The column widths.
- `Rows`: The row heights.
- `UsesFlexChildLayoutDiagnostics`: Whether this container checks its children's percent sizes for likely layout mistakes.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `GetCell(Node)`: Where `node` is placed.
- `GetColumn(Node)`: The column `node` is placed in.
- `GetColumnSpan(Node)`: How many columns `node` spans.
- `GetIsSharedSizeScope(Node)`: Whether `node` scopes shared size groups for the grids inside it.
- `GetRow(Node)`: The row `node` is placed in.
- `GetRowSpan(Node)`: How many rows `node` spans.
- `Place(Node, int, int, int, int)`: Adds `child` as content in the given cell.
- `SetCell(Node, GridCell)`: Puts `node` in `cell` of whichever grid it is a child of.
- `SetColumn(Node, int)`: Places `node` in `column`.
- `SetColumnSharedSizeGroup(int, string)`: Puts column `index` in a shared-size group; an empty name removes it from its group.
- `SetColumnSpan(Node, int)`: Spans `node` over `columnSpan` columns.
- `SetIsSharedSizeScope(Node, bool)`: Makes `node` the scope within which shared-size groups line up.
- `SetRow(Node, int)`: Places `node` in `row`.
- `SetRowSharedSizeGroup(int, string)`: Puts row `index` in a shared-size group; an empty name removes it from its group.
- `SetRowSpan(Node, int)`: Spans `node` over `rowSpan` rows.
