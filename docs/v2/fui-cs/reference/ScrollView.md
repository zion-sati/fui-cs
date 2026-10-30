# ScrollView

class in `FuiCs`: [`Node`](Node.md), [`ISizable`](ISizable.md), [`IContentContainer`](IContentContainer.md)

A native scroll viewport over its content. The host scrolls it with wheel, touch and momentum and reports every change; [`ScrollView.ScrollTo`](ScrollView.md) moves it from code. Offsets, extent and viewport follow WPF's ScrollViewer.

Fluent members: [`ScrollViewExtensions`](ScrollViewExtensions.md).

## Properties

- `ExtentHeight`: The content's height.
- `ExtentWidth`: The scrollable content width the host last reported, or the explicit scroll content width.
- `FillHeight`: Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.
- `FillHeightPercent`: Takes this percentage of the remaining height; null when not set.
- `FillWidth`: Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.
- `FillWidthPercent`: Takes this percentage of the remaining width; null when not set.
- `FlexBasis`: The initial main-axis size before flexing; NaN until set.
- `Friction`: Momentum friction; null keeps the host default.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `HorizontalOffset`: The horizontal offset, in pixels.
- `IsHorizontalScrollEnabled`: Whether the view scrolls sideways.
- `IsVerticalScrollEnabled`: Whether the view scrolls vertically.
- `MaxHeight`: The largest height layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MaxWidth`: The largest width layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MinHeight`: The smallest height layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `MinWidth`: The smallest width layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `PersistenceOwner`: When set, the offset persists under this node's id and setting instead of the view's own.
- `PersistScroll`: Whether the scroll offset is saved with the page's UI state and restored when the user returns to the history entry. Needs a [`Node.NodeId`](Node.md). On by default.
- `ScrollState`: The view's offsets and sizes as signals; a [`ScrollBox`](ScrollBox.md) shares it with its scroll bars.
- `SmoothScrolling`: Animates wheel and keyboard scrolling toward its target instead of jumping.
- `Transitions`: Which property changes animate; only [`NodeTransitions.ScrollOffset`](NodeTransitions.md) applies to a scroll view.
- `VerticalOffset`: The vertical offset, in pixels.
- `ViewportHeight`: The viewport's height.
- `ViewportWidth`: The visible width the host last reported, or a fixed pixel [`ScrollView.Width`](ScrollView.md) before then.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.

## Methods

- `AddContent(Node)`: Adds a node to the scrolled content.
- `BindScrollState(ScrollState)`: Makes `state` the view's scroll state and brings it up to date.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `CapturePersistedState()`: Saves this node's persisted state. Overrides that persist more should call the base method.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleScroll(float, float, float, float, float, float)`: Called when the host scrolls the node, with its offset and the sizes of its content and viewport.
- `RestorePersistedState()`: Restores this node's persisted state. Overrides that persist more should call the base method.
- `ScrollTo(float, float)`: Scrolls to the offset at once, taking over from any momentum scroll or animation in progress.
- `ScrollToAnimated(float, float, AnimationTiming)`: Animates to the offset with `timing`; before the view is built, it is the initial offset.
- `SetContent(ReadOnlySpan<Node>)`: Replaces the scrolled content.
- `SetRuntimeScrollOffset(float, float)`: Moves the offset for the framework's own scrolling (keyboard, scroll bars) without clearing momentum.
- `SetScrollContentSize(float?, float?)`: Sets the scrollable content size instead of measuring the content, for content that is laid out lazily; a null dimension is measured.
- `SetScrollOffset(float, float)`: Sets the offset, animating it on a built view with a [`NodeTransitions.ScrollOffset`](NodeTransitions.md) timing; before the view is built, it is the initial offset.
- `TryRestorePersistedOffset()`: Restores a saved offset once the host has reported a viewport and enough content to scroll to it, clamped to the content; returns whether it did.

## Events

- `ScrollChanged`: Raised when the offset, extent or viewport changes, whether the host or code scrolled.

## Fields

- `ProgrammaticScrollTolerance`: How far, in pixels, the host may settle from a programmatic offset and still acknowledge it.
