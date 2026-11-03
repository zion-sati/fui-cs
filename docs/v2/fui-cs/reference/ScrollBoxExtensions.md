# ScrollBoxExtensions

class in `FuiCs`

Fluent configuration for [`ScrollBox`](ScrollBox.md).

## Methods

- `HorizontalScrollBarVisibility(ScrollBarVisibility)`: Sets [`ScrollBox.HorizontalScrollBarVisibility`](ScrollBox.md).
- `HorizontalScrollEnabled(bool)`: Sets [`ScrollBox.IsHorizontalScrollEnabled`](ScrollBox.md).
- `OnScrollChanged(Action<ScrollChangedEventArgs>)`: Adds a handler to [`ScrollBox.ScrollChanged`](ScrollBox.md).
- `PersistScroll(bool)`: Sets [`ScrollBox.PersistScroll`](ScrollBox.md).
- `ScrollBarGutter(float)`: Sets [`ScrollBox.ScrollBarGutter`](ScrollBox.md).
- `ScrollContentSize(float?, float?)`: Overrides the content's size for scrolling; null keeps the measured size.
- `ScrollOffset(float, float)`: Sets the offset, through its transition if it has one; before the box is built, it is the initial offset.
- `SmoothScrolling(bool)`: Sets [`ScrollBox.SmoothScrolling`](ScrollBox.md).
- `VerticalScrollBarVisibility(ScrollBarVisibility)`: Sets [`ScrollBox.VerticalScrollBarVisibility`](ScrollBox.md).
- `VerticalScrollEnabled(bool)`: Sets [`ScrollBox.IsVerticalScrollEnabled`](ScrollBox.md).
- `WheelLatching(bool)`: Sets [`ScrollBox.WheelLatching`](ScrollBox.md).
