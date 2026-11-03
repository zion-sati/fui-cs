# ScrollViewExtensions

class in `FuiCs`

Fluent configuration for [`ScrollView`](ScrollView.md).

## Methods

- `FlexBasis(float)`: Sets the share of the main axis the view starts from before growing.
- `Friction(float)`: Sets how quickly momentum scrolling slows.
- `HorizontalScrollEnabled(bool)`: Sets [`ScrollView.IsHorizontalScrollEnabled`](ScrollView.md).
- `OnScrollChanged(Action<ScrollChangedEventArgs>)`: Adds a handler to [`ScrollView.ScrollChanged`](ScrollView.md).
- `PersistScroll(bool)`: Sets [`ScrollView.PersistScroll`](ScrollView.md).
- `ScrollContentSize(float?, float?)`: Overrides the content's size for scrolling; null keeps the measured size.
- `ScrollOffset(float, float)`: Sets the offset, through its transition if it has one; before the view is built, it is the initial offset.
- `SmoothScrolling(bool)`: Sets [`ScrollView.SmoothScrolling`](ScrollView.md).
- `Transitions(NodeTransitions)`: Sets which property changes animate; only the scroll offset applies.
- `VerticalScrollEnabled(bool)`: Sets [`ScrollView.IsVerticalScrollEnabled`](ScrollView.md).
- `WheelLatching(bool)`: Sets [`ScrollView.WheelLatching`](ScrollView.md).
