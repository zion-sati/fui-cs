# Application

class in `FuiCs`

Mounts retained roots inside the application shell and flushes their commits to the host.

## Properties

- `AfterCommit`: Runs after each flush to update chrome that follows laid-out bounds, such as the focus ring; returning true commits again so the change shows in the same frame.
- `Current`: The application the host runs and disposes through `__runApp` and `__disposeApp`.
- `DevicePixelRatio`: The display's device pixel ratio, read from the host each time.
- `FrameTime`: The host's timestamp for the frame being drawn, in milliseconds.
- `MountedRoot`: The root node passed to the last [`Application.Mount`](Application.md), until it is unmounted.
- `ShellContextMenu`: The application's built-in context menu, created in the shell on first use.
- `ShellFocusHost`: The portal the focus ring draws in, created in the shell on first use.
- `ShellSelectionHandleHost`: The full-viewport portal the selection handles draw in, created on first use.
- `ShellTextToolbarHost`: The full-viewport portal the text selection toolbar draws in, created on first use.
- `ViewportHeight`: The viewport's height, updated as the window resizes.
- `ViewportWidth`: The viewport's width, updated as the window resizes.

## Methods

- `CapturePersistedUiState()`: Saves the persisted state of the mounted page, as the host does before leaving a history entry.
- `Create(Func<Node>)`: Creates the application that builds its page with `buildPage` and makes it [`Application.Current`](Application.md).
- `CreateManaged(Func<T>, Func<T, Node>, Action<T>, Action<T>)`: Creates the application whose page is built by `buildPage`, mounted from , and makes it current.
- `CreateManaged(Func<T>)`: Creates the application whose page is a controller that mounts and disposes itself, and makes it current.
- `FlushRenders()`: Commits pending retained changes. The host calls this after input and on each frame.
- `Mount(Node)`: Unmounts the current root, resets the native tree and mounts `root` inside a new shell. The previous root's requests in flight are cancelled, and its focus ring, selection chrome and tooltip cleared.
- `OnUnmount(Action)`: Runs `handler` once when the page unmounts: when the application closes, when a routed site swaps in another route, or when [`Application.Mount`](Application.md) replaces the page. Dispose there what the page set up outside its nodes, such as a `JSSubscription` to the page's JavaScript. A handler registered while a page builds belongs to that page, and runs after the framework has cleared the page's overlays and requests.
- `Register(Action<ApplicationRegistration>)`: Creates the application from a registration callback, and makes it current.
- `RestorePersistedUiState()`: Restores the mounted page's persisted state for the current history entry.
- `SetCaption(string)`: Sets the browser tab's title.
- `SetPageZoomEnabled(bool)`: Turns the framework's page zoom (pinch and ctrl-wheel) on or off.
- `Unmount()`: Unmounts the page, disposing its tree.

## Events

- `Unmounting`: Raised when the mounted root goes away. The framework's overlays and requests in flight subscribe when they are first used and clear themselves, so an application that never uses them does not carry their code.
