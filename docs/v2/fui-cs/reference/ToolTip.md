# ToolTip

class in `FuiCs`

A text hint shown next to a node while the pointer rests on it, or while the node has keyboard focus. The timing follows WPF's ToolTipService: a delay before the first tooltip shows, a shorter one for the next within a moment, and a duration after which it hides. A string converts to a tooltip with the default timing.

Fluent members: [`ToolTipExtensions`](ToolTipExtensions.md).

## Properties

- `Background`: The panel color; the theme's when null.
- `BetweenShowDelay`: How soon after one tooltip shows that the next shows at once.
- `HorizontalOffset`: A horizontal shift from the placed position.
- `InitialShowDelay`: How long the pointer rests on the node before the tooltip shows.
- `Placement`: Where the tooltip opens relative to the node.
- `ShowDuration`: How long the tooltip stays; zero keeps it until the pointer leaves or focus moves.
- `ShowsOnKeyboardFocus`: Whether the tooltip also shows while the node has visible keyboard focus.
- `Text`: The tooltip's text.
- `TextColor`: The text color; the theme's when null.
- `VerticalOffset`: A vertical shift from the placed position.

## Methods

- `op_Implicit(string)~ToolTi)`: A tooltip of `text` with the default timing.
