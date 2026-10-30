# ContextMenuManager

class in `FuiCs`

Answers the host's context-menu requests: a node's [`Node.ContextMenuOpening`](Node.md) handler, or the application's built-in menu with the items that fit the target (copy and select all for text, opening an image, copying the page selection) and the page's history and reload items.

## Methods

- `BrowsableImageUrl(Node)`: An image's or SVG's URL, unless it is inline data the browser cannot open.
- `BuildLongPressItems(ulong, float, float)`: The built-in items for a long press over `handle`: the context menu's items without the page's history and reload items.
- `EditableTextSection(TextCore)`: An editor's items. The host keeps a snapshot of its selection, so Cut and Copy still act on it after pressing the menu moves the editor's focus; each item returns focus to the editor.
- `ImageUrl(Node, float, float)`: The URL of the image or SVG at or around the target, or one under the point among its descendants. Images are not hit targets, so with no target under the pointer the search starts from the page root.
- `SetMenu(ContextMenu)`: Shows the built-in context menu (the selection, link, image, history and reload items) in `menu`, a menu in the application's tree that the app styles, instead of the shell's own menu; null restores the shell's. Mounting a new root, which disposes the old tree, restores it too.
- `ShowForLongPress(ulong, float, float)`: Shows the long-press items over `handle` in the text selection toolbar; returns whether there were any.
- `TextSection(TextCore, string)`: Copy and Select All for selectable text, where Copy takes the page selection when the request is over it; editable text has Undo, Redo, Cut, Copy, Paste and Select All instead.
- `TextTarget(Node)`: The nearest text, where a text input stands for its editor, so a press on its padding or placeholder targets it.
- `TrackPointerEvent(PointerEventType, ulong)`: Remembers the nodes a press crossed, to tell later whether a request is over the page selection.
