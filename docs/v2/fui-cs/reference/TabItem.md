# TabItem

class in `FuiCs`

One tab of a [`TabView`](TabView.md): its label, description and enabled state, and its content, a retained view created the first time the tab is selected and kept until the tab is removed.

## Properties

- `Description`: A longer description of the tab, for the application's own tab strip; the view does not show it.
- `IsEnabled`: Whether the tab can be selected; disabling the selected tab selects the nearest enabled one.
- `IsMaterialized`: Whether the tab's content has been created.
- `Label`: The tab's label.

## Methods

- `Content(Func<RetainedView>)`: Sets the factory that creates the tab's content when it is first selected.
- `ContentView(RetainedView)`: Sets content created ahead of selection.
- `ResolveView()`: The tab's content, created now if it does not exist yet: an empty box without a factory.
