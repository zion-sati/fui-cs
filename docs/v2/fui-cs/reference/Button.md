# Button

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A push button. A primary click, or Enter or Space while it has focus, raises [`Button.Click`](Button.md); a press released outside the button does not. Its chrome follows the theme and [`Button.Colors`](Button.md); setting a chrome property overrides that property only.

Fluent members: [`ButtonExtensions`](ButtonExtensions.md).

## Properties

- `Background`: The fill at rest; until set, the colors' background or the theme's accent.
- `Border`: The outline drawn around the box.
- `Colors`: Colors over the theme's for each state; null follows the theme.
- `CornerRadius`: The radius of each corner.
- `DropShadow`: The shadow drawn behind the box.
- `FontFamily`: The label's font family; null follows the theme's body family.
- `FontSize`: The label's size; the theme's body size when null.
- `FontStyle`: The label's style; upright when null.
- `FontWeight`: The label's weight; the theme's when null.
- `HoverBackground`: The fill under the pointer; when null, the colors' hover fill or the theme's hovered accent.
- `IsHovered`: Whether the pointer is over the button.
- `IsPressed`: Whether the button is pressed.
- `Label`: The label's text.
- `Padding`: The space between the border and the children.
- `PressedBackground`: The fill while pressed; when null, the colors' pressed fill or the theme's pressed accent.
- `Template`: Builds the button's content; null uses [`ButtonTemplate.Default`](ButtonTemplate.md).
- `TextColor`: The label color; null follows [`Button.Colors`](Button.md) and the theme.

## Methods

- `BeginPress()`: Shows the button pressed for a key another control handles, such as a form's Enter.
- `EndPress(bool)`: Releases a [`Button.BeginPress`](Button.md), raising [`Button.Click`](Button.md) when `fire` is set.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandlePointerEvent(PointerEventArgs)`: Tracks hover and the pressed look. The host keeps routing a pressed pointer here after it leaves, so the button looks pressed only while that pointer is over it; the click itself comes from the node's click recognition.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `PerformClick()`: Raises [`Button.Click`](Button.md) as if the user activated the button.
- `Refresh()`: Applies the template, then the chrome and label style from the overrides, colors, theme and state.
- `SyncFocusRing()`: Shows the focus ring while the button has focus, is enabled and focus is visible.

## Events

- `Click`: Raised when the button is activated by pointer or keyboard.
- `HoverChanged`: Raised when the pointer enters or leaves the button.
