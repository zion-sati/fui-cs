# TabView

class in `FuiCs`: [`FlexBox`](FlexBox.md)

Shows the content of one of its tabs at a time, in [`TabView.ContentPresenter`](TabView.md). It has no tab strip: the app builds its own selector and sets [`TabView.SelectedIndex`](TabView.md). The first enabled tab is selected, a tab's content is created when it is first selected and deactivated rather than disposed while another tab shows, and removing or disabling the selected tab selects the nearest enabled one after it, then before it.

Fluent members: [`TabViewExtensions`](TabViewExtensions.md).

## Properties

- `ContentPresenter`: The box the selected tab's content is shown in, as the tab panel.
- `Items`: The tabs in order.
- `SelectedIndex`: The selected tab's index, or -1 when no tab is enabled. Selecting a disabled tab does nothing; a change raised from [`TabView.SelectionChanged`](TabView.md) takes effect after the current one finishes.
- `SelectedItem`: The selected tab, if any.

## Methods

- `AddItem(TabItem)`: Adds a tab; the first enabled tab added to a view without a selection is selected.
- `ClearItems()`: Removes every tab, disposing their content.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `RemoveItem(TabItem)`: Removes `item`, disposing its content.
- `RemoveItemAt(int)`: Removes the tab at `index` and disposes its content.
- `ReplaceSelection(int, TabItem)`: After the selected tab at `index` went away: the nearest enabled tab, or none.

## Events

- `SelectionChanged`: Raised when the selected tab changes, including to none.
