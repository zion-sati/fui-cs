# Application<TPage>

class in `FuiCs`: [`IApplication`](IApplication.md)

An application whose page is a `TPage`, such as a controller that owns the root node.

## Properties

- `ActivePage`: The page built by the last run, or null before it.

## Methods

- `Dispose()`: Unmounts the page and disposes it.
- `FlushRenders()`: Commits pending changes to the screen now.
- `Run()`: Replaces any running page with a newly built one.
