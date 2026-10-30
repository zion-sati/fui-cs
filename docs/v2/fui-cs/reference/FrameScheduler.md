# FrameScheduler

class in `FuiCs`

Coalesces retained mutations into one commit per frame, and runs loaded and after-commit callbacks.

## Properties

- `CommitCount`: The number of frames committed, so work queued for the next commit can tell when it has run.

## Methods

- `AfterNextCommit(Action)`: Runs `callback` once, after the next frame is committed.
- `FireLoadedCallbacks()`: Runs the loaded handlers registered before the first commit.
- `FlushCommit()`: Commits pending changes now if any are waiting; returns whether it did.
- `MarkNeedsCommit()`: Asks for a commit at the end of the current event or frame.
- `OnLoaded(Action<LoadedEventArgs>)`: Runs `handler` once after the page has been mounted and the first frame has been committed, or immediately if that has already happened. Safe to call while building the page; timers started from the handler survive the mount's timer reset.
- `Reset()`: Clears all scheduler state, including pending loaded handlers.
- `ResetCommitState()`: Starts a new mount: loaded handlers registered while building the page are kept and fire on the next first commit.
