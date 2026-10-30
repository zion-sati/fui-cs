# FlexBox

class in `FuiCs`: [`Node`](Node.md), [`ISizable`](ISizable.md), [`IContentContainer`](IContentContainer.md)

A flexbox container with box styling: size, flex layout, padding and margin, background, border, corners, opacity, blur, shadow and gradient. Properties only reach the native node once they are set.

Fluent members: [`FlexBoxExtensions`](FlexBoxExtensions.md).

## Properties

- `AlignItems`: How children align across the main axis.
- `AlignSelf`: How this box aligns in its parent, overriding the parent's AlignItems.
- `Background`: The fill color; on a built box with a [`NodeTransitions.Background`](NodeTransitions.md) timing, a change animates and this reads the current color.
- `BackgroundBlur`: The Gaussian blur sigma applied to what is behind the box.
- `Blur`: The Gaussian blur sigma applied to the box and its content.
- `Border`: The outline drawn around the box.
- `ClipToBounds`: Clips children to the box, as CSS's `overflow: hidden` does; off by default, so content and shadows may overflow.
- `CornerRadius`: The radius of each corner.
- `DropShadow`: The shadow drawn behind the box.
- `FillHeight`: Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.
- `FillHeightPercent`: Takes this percentage of the remaining height; null when not set.
- `FillWidth`: Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.
- `FillWidthPercent`: Takes this percentage of the remaining width; null when not set.
- `FlexBasis`: The initial main-axis size before flexing; NaN until set.
- `FlexDirection`: Whether children flow in a row or a column, and in which direction.
- `FlexWrap`: Whether children wrap onto more lines when they do not fit.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `JustifyContent`: How children are spaced along the main axis.
- `LinearGradient`: A gradient that fills the box in place of its background color; a gradient with no stops is ignored with a warning.
- `Margin`: The space around the box.
- `MaxHeight`: The largest height layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MaxWidth`: The largest width layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MinHeight`: The smallest height layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `MinWidth`: The smallest width layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `Opacity`: The opacity of the box and its content; on a built box with a [`NodeTransitions.Opacity`](NodeTransitions.md) timing, a change animates and this reads the current value.
- `Padding`: The space between the border and the children.
- `Position`: The left and top offsets of an absolutely positioned box.
- `PositionType`: Whether the box takes part in its parent's flow or is placed at Position.
- `Transitions`: Which property changes animate; null changes them at once.
- `UsesFlexChildLayoutDiagnostics`: Whether this container checks its children's percent sizes for likely layout mistakes.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.

## Methods

- `AddContent(Node)`: Adds `node` to the container's content.
- `ApplyLayoutStyle()`: Sends the box's layout properties to the native node, for a Build override.
- `ApplyVisualStyle()`: Sends the box's visual properties to the native node, for a Build override.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `BuildStyledNode(NodeType, bool)`: Creates the native node of `type` with this box's metadata, layout and style.
- `Column(ReadOnlySpan<Node>)`: A column of `children`.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `EmitDeveloperLayoutWarnings()`: Warns once when a child of several uses a 100% main-axis size, or when main-axis percentages exceed 100%: percent is parent-relative sizing, not flex sharing.
- `OnChildLayoutChanged()`: Called when a child's layout changes.
- `Row(ReadOnlySpan<Node>)`: A row of `children`.
- `SetContent(ReadOnlySpan<Node>)`: Replaces the content with `nodes`.
