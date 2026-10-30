# RetainedView

class in `FuiCs`: `IDisposable`

A retained subtree that can be activated, deactivated and reused, such as the content of a tab. It owns [`RetainedView.Root`](RetainedView.md) and every resource passed to [`RetainedView.Retain`](RetainedView.md), and disposes them together.

## Properties

- `IsActive`: Whether the view is active: mounted and receiving updates.
- `IsDisposed`: Whether the view has been disposed.
- `Root`: The view's root node.

## Methods

- `Activate()`: Activates the view.
- `Deactivate()`: Deactivates the view, keeping its nodes for the next activation.
- `Dispose()`: Disposes the view's root and raises Disposed.
- `OnActivate(Action<RetainedView>)`: Adds a handler for activation.
- `OnDeactivate(Action<RetainedView>)`: Adds a handler for deactivation.
- `OnDispose(Action<RetainedView>)`: Adds a handler for disposal.
- `Retain(IDisposable)`: Disposes `resource` with the view, or immediately if the view is already disposed.

## Events

- `Activated`: Raised when the view activates.
- `Deactivated`: Raised when the view deactivates.
- `Disposed`: Raised when the view is disposed.
