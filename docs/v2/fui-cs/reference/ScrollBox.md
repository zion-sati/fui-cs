# ScrollBox

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A [`ScrollView`](ScrollView.md) with scroll bars, like WPF's ScrollViewer: content added to the box goes into the view, and each bar shows according to its [`ScrollBarVisibility`](ScrollBarVisibility.md). Wheel input over the bars, the gutter and the corner scrolls the view, and its offset persists under the box's node id.

Fluent members: [`ScrollBoxExtensions`](ScrollBoxExtensions.md).

## Properties

- `HorizontalOffset`: The horizontal offset, in pixels.
- `HorizontalScrollBar`: The horizontal bar.
- `HorizontalScrollBarVisibility`: When the horizontal bar shows.
- `IsHorizontalScrollEnabled`: Whether the content scrolls sideways.
- `IsVerticalScrollEnabled`: Whether the content scrolls vertically.
- `PersistScroll`: Whether the offset is saved with the page's UI state under the box's [`Node.NodeId`](Node.md).
- `ScrollBarGutter`: Room between the content and the vertical bar while the bar shows.
- `ScrollState`: The offsets and sizes the view and the bars share.
- `SmoothScrolling`: Animates wheel and keyboard scrolling toward its target instead of jumping.
- `Transitions`: The viewport's transitions: only [`NodeTransitions.ScrollOffset`](NodeTransitions.md) applies to a scroll box.
- `VerticalOffset`: The vertical offset, in pixels.
- `VerticalScrollBar`: The vertical bar.
- `VerticalScrollBarVisibility`: When the vertical bar shows.
- `Viewport`: The scroll view holding the content.

## Methods

- `AddContent(Node)`: Adds `node` to the container's content.
- `BindChrome(ulong)`: Points the bars, the box and its chrome at the view, so they scroll it.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HitTarget(FlexBox)`: Makes a chrome node a hit target, so wheel input over it reaches its scroll proxy.
- `RefreshChrome()`: Shows each bar per its visibility and the overflow on its axis, and sizes the gutter and corner.
- `ScrollTo(float, float)`: Scrolls to the offset at once.
- `ScrollToAnimated(float, float, AnimationTiming)`: Scrolls to the offset over `timing`.
- `SetContent(ReadOnlySpan<Node>)`: Replaces the content with `nodes`.
- `SetScrollContentSize(float?, float?)`: Overrides the content's size for scrolling, as for a virtual list; null keeps the measured size.
- `SetScrollOffset(float, float)`: Sets the offset, animated when the view has a scroll offset transition.

## Events

- `ScrollChanged`: Raised when the view scrolls or its content or viewport size changes.
