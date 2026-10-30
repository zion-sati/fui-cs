# Dialog

class in `FuiCs`: [`Portal`](Portal.md)

A modal dialog: a card with a title, body text, optional content and OK and Cancel buttons, centered over a blurred backdrop that covers the viewport wherever the dialog is mounted. Enter accepts and Escape or a backdrop click cancels; either hides the dialog and returns focus to where it was.

Fluent members: [`DialogExtensions`](DialogExtensions.md).

## Properties

- `AcceptButton`: The OK button.
- `Active`: The open dialog shown most recently, if any.
- `BackdropColor`: The backdrop over the page; null uses the theme's dialog backdrop.
- `BackgroundBlur`: The blur of the page behind the backdrop.
- `Body`: The body's text.
- `BodyContent`: Content shown between the body text and the buttons, such as fields; null shows none.
- `BodyText`: The body's text node.
- `CancelButton`: The Cancel button.
- `CardBorder`: The card's border; null follows the theme.
- `CardColor`: The card's fill; null follows the theme.
- `CardCornerRadius`: The card's corner radius; null follows the theme.
- `CardShadow`: The card's shadow; null follows the theme.
- `IsOpen`: Whether the dialog is showing.
- `Title`: The title's text.
- `TitleText`: The title's text node.

## Methods

- `Accept()`: Hides the open dialog and raises [`Dialog.Accepted`](Dialog.md).
- `AcceptActive()`: Accepts the open dialog shown most recently.
- `Cancel()`: Hides the open dialog and raises [`Dialog.Cancelled`](Dialog.md).
- `CancelActive()`: Cancels the open dialog shown most recently.
- `CoverViewport()`: Places the overlay over the whole viewport, offsetting the dialog's own position.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `Hide()`: Hides the dialog without accepting or cancelling it, returning focus to where it was.
- `Show()`: Shows the dialog, moves focus to [`Dialog.AcceptButton`](Dialog.md) once it is drawn, and makes Enter and Escape accept and cancel it.
- `ShowAsync()`: Shows the dialog and completes with true when it is accepted or false when it is cancelled.

## Events

- `Accepted`: Raised after the user or code accepts the dialog, once it is hidden.
- `Cancelled`: Raised after the user or code cancels the dialog, once it is hidden.
- `Shown`: Raised after the frame that first draws the shown dialog.
