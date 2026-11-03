# Slider

class in `FuiCs`: [`FlexBox`](FlexBox.md)

Picks a value between [`Slider.Minimum`](Slider.md) and [`Slider.Maximum`](Slider.md), snapped to [`Slider.Step`](Slider.md), by pressing or dragging along the track, or with the arrow keys, Home and End while focused. A vertical slider increases upwards. Its value persists across history navigation.

Fluent members: [`SliderExtensions`](SliderExtensions.md).

## Properties

- `Colors`: Colors over the theme's for each part.
- `DefaultTemplate`: A rounded track with an accent fill and a round thumb.
- `FillsLength`: Whether the slider fills its container along its orientation, as `FillWidth` or `FillHeight` does, instead of a fixed [`Slider.Length`](Slider.md). Setting [`Slider.Length`](Slider.md) returns to a fixed length.
- `IsDragging`: Whether the thumb is being dragged.
- `Length`: The length of the parts along the slider, in pixels; at least one pixel more than the thumb.
- `Maximum`: The highest value; lowering it below [`Slider.Minimum`](Slider.md) lowers the minimum too.
- `Minimum`: The lowest value; raising it above [`Slider.Maximum`](Slider.md) raises the maximum too.
- `Orientation`: Whether the slider runs across or upwards.
- `Sizing`: The track and thumb sizes; the default template's when null.
- `Step`: The spacing of the values the slider snaps to from [`Slider.Minimum`](Slider.md), and the arrow keys' change.
- `Template`: Builds the track, fill and thumb; null uses [`Slider.DefaultTemplate`](Slider.md).
- `Value`: The value, clamped to the range and snapped to [`Slider.Step`](Slider.md).

## Methods

- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `UpdateFromPointer(float, float)`: Sets the value under a node-local pointer position, measured from the thumb's center at either end.

## Events

- `Changed`: Raised when [`Slider.Value`](Slider.md) changes, by the user or from code.

## Fields

- `OuterInset`: From the slider's outer edge to its parts: the focus border, padding and content inset.
