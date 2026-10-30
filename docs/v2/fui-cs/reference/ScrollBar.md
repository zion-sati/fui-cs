# ScrollBar

class in `FuiCs`: `IDisposable`

A scroll bar for one axis of a [`ScrollState`](ScrollState.md): a track holding a thumb sized to the visible part of the content. Dragging the thumb, or pressing the track, scrolls the bound view. It is not a node itself: [`ScrollBar.Root`](ScrollBar.md) is the track to place in a layout. A [`ScrollBox`](ScrollBox.md) creates and binds its own.

Fluent members: [`ScrollBarExtensions`](ScrollBarExtensions.md).

## Properties

- `IsChromeVisible`: Whether the bar shows; a hidden bar takes no room and cancels a thumb drag.
- `IsDragging`: Whether the thumb is being dragged.
- `MinThumbLength`: The thumb's shortest length along the axis, however long the content.
- `Orientation`: Whether the bar scrolls vertically or sideways.
- `Root`: The track: the node to place in a layout.
- `Thickness`: The room the bar takes across its axis: [`ScrollBar.TrackThickness`](ScrollBar.md).
- `ThumbColor`: The thumb's color; null follows the theme.
- `ThumbCornerRadius`: The thumb's corner radius.
- `ThumbThickness`: The thumb's thickness in pixels.
- `TrackColor`: The track's color; null follows the theme.
- `TrackCornerRadius`: The track's corner radius.
- `TrackThickness`: The track's thickness in pixels.

## Methods

- `BindTarget(ScrollView)`: Makes the bar scroll `view`, and makes wheel input over the bar scroll it too.
- `Dispose()`: Releases the bar's subscriptions and nodes.
- `HandleTrackPress(float, float)`: A press on the track centers the thumb on the press, as far as it can go.
- `SyncThumb()`: Sizes the track to the viewport and places the thumb between its two spacers.

## Events

- `ChromeMetricsChanged`: Raised when the bar's thickness changes, so the layout around it can make room.
