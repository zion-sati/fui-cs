# VirtualList

class in `FuiCs`: [`FlexBox`](FlexBox.md)

A vertically scrolling list of fixed-height items that builds only the rows in view. A pool of rows, each a selection area holding an item container, is rebound to the visible items as the list scrolls: [`VirtualList.BindItem`](VirtualList.md) fills a row's container for an item index, reusing what it built there before. Spacers above and below the pool keep the content height that of every item. The list is a selection barrier, and a text selection follows its item when the item moves to another pooled row.

Fluent members: [`VirtualListExtensions`](VirtualListExtensions.md).

## Properties

- `BindItem`: Fills a row's container for an item index; the container keeps what earlier calls built in it, so a binder creates its nodes once and updates them. Setting it rebinds the rows in view.
- `FirstVisibleIndex`: The first item in view.
- `Height`: A pixel height also seeds the viewport height, before the host reports it.
- `ItemHeight`: Every item's height in pixels.
- `NodeId`: The list's id, which its scroll box carries so the scroll offset persists under it.
- `PersistScroll`: Whether the scroll offset is saved with the page's UI state under the list's id.
- `RenderedItemCount`: How many items have rows bound to them.
- `ScrollBox`: The scroll box the rows scroll in; it carries the list's node id and persisted offset.
- `ScrollState`: The list's offsets and sizes.
- `TotalContentHeight`: The height of every item together.
- `TotalItems`: How many items the list has.
- `Width`: A pixel width also seeds the viewport width, before the host reports it.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `ClearItemNode(Node)`: Empties a hidden row's text and labels, so a recycled row shows nothing stale.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `UpdateItemCount(int)`: Changes the item count, keeping the offset within the new content.
