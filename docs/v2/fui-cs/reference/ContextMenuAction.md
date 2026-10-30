# ContextMenuAction

enum in `FuiCs`

A built-in action a menu item runs through the host.

## Values

- `None`: The item runs only its [`MenuItem.Click`](MenuItem.md) handlers.
- `CopyCurrentSelection`: Copies [`MenuItem.Payload`](MenuItem.md), the target text's selection, or the page selection.
- `ReloadPage`: Reloads the page.
- `NavigateBack`: Goes back in the browser's history.
- `NavigateForward`: Goes forward in the browser's history.
- `OpenLink`: Navigates to the link in [`MenuItem.Payload`](MenuItem.md).
- `OpenLinkInNewTab`: Opens the link in [`MenuItem.Payload`](MenuItem.md) in a new tab.
- `OpenImage`: Navigates to the image URL in [`MenuItem.Payload`](MenuItem.md).
- `OpenImageInNewTab`: Opens the image URL in [`MenuItem.Payload`](MenuItem.md) in a new tab.
- `SelectAllText`: Selects all of the target text.
- `UndoTextEdit`: Undoes the target editable text's last edit.
- `RedoTextEdit`: Redoes the target editable text's last undone edit.
- `CutTextSelection`: Copies the target editable text's selection, then deletes it.
- `PasteText`: Pastes the clipboard into the target editable text.
