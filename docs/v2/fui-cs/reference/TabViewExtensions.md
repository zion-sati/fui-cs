# TabViewExtensions

class in `FuiCs`

Fluent configuration for [`TabView`](TabView.md) and [`TabItem`](TabItem.md).

## Methods

- `Description(string)`: Sets [`TabItem.Description`](TabItem.md).
- `IsEnabled(bool)`: Sets [`TabItem.IsEnabled`](TabItem.md).
- `Items(ReadOnlySpan<TabItem>)`: Replaces the tabs with `items`.
- `OnSelectionChanged(Action<TabSelectionChangedEventArgs>)`: Adds a handler to [`TabView.SelectionChanged`](TabView.md).
- `SelectedIndex(int)`: Sets [`TabView.SelectedIndex`](TabView.md).
