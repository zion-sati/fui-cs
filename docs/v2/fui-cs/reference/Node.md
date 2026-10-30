# Node

class in `FuiCs`: `IDisposable`

A retained UI node. Constructing a node does no native work; [`Node.Build`](Node.md) creates the native node and its subtree. After that, changes update the native node directly and schedule a commit.

Fluent members: [`NodeExtensions`](NodeExtensions.md).

## Properties

- `AllowDrop`: Whether drags can drop on this node. Subscribing to a drop event turns it on.
- `Children`: The node's children in order.
- `Cursor`: The pointer's cursor over the node.
- `DebugIsAbsolutelyPositioned`: For layout diagnostics: whether the node is taken out of its parent's flow.
- `DefaultSemanticLabel`: The label used when none is set explicitly; null means no default.
- `DefaultSemanticRole`: The role used when none is set explicitly; `SemanticRole.None` means no default.
- `DragAllowedEffects`: The effects a drag from this node allows. Copy by default.
- `DragDataProvider`: Creates the data for a drag that starts on this node; returning null declines the drag. Setting it makes the node a drag source.
- `Focusable`: Whether the node takes keyboard focus while it is enabled and visible.
- `Handle`: The native node; [`HandleValue.Invalid`](HandleValue.md) before [`Node.Build`](Node.md) and after disposal.
- `HasExplicitSemanticLabel`: Whether a label was set explicitly rather than by the node's type.
- `HasExplicitSemanticRole`: Whether a role was set explicitly rather than by the node's type.
- `InnerEditor`: The editable text inside a control whose shell takes its pointer input, such as a text input.
- `IsBuilt`: Whether the native node exists.
- `IsContextMenuEnabled`: Whether a context menu may open over this node and its descendants; when false, none does.
- `IsDisposed`: Whether the node has been disposed.
- `IsEditableText`: Whether the node is text the user can edit.
- `IsEnabled`: True when this node and all its ancestors are enabled. Setting it changes only this node.
- `IsFocused`: Whether this node has keyboard focus.
- `IsPortal`: Marks the node as a portal, rendered above the normal tree. Applied when the node is built.
- `IsSelectableText`: Whether the node is text the user can select.
- `IsSelectionBarrier`: Whether text selection does not extend across the node, as for controls with their own text.
- `IsVisible`: True when neither this node nor any ancestor is hidden or collapsed.
- `LongPressRecognizer`: A configured long-press recognizer; its duration and tolerance apply to [`Node.LongPress`](Node.md) too.
- `NodeId`: The node's stable id, which the host's debug tree, find-on-page and persisted UI state key on.
- `Parent`: The node's parent, or null for a root or a detached node.
- `PreserveSelectionOnPointerDown`: Keeps the page's text selection when a press lands on the node, as toolbars and handles need.
- `RetainedMutationCount`: Counts the changes this node has sent to the host, so a bitmap render can tell its prepared text is stale.
- `SemanticChecked`: The checked state assistive technology reports.
- `SemanticDisabled`: The accessibility disabled state; null leaves it unset. Setting it stops tracking [`Node.IsEnabled`](Node.md).
- `SemanticExpanded`: The accessibility expanded state; null leaves it unset.
- `SemanticLabel`: The accessibility label: the label set here, otherwise the node's default label.
- `SemanticOrientation`: The orientation assistive technology reports, as for a slider or scroll bar.
- `SemanticRole`: The accessibility role: the role set here, otherwise the node's default role.
- `SemanticSelected`: The accessibility selected state; null leaves it unset.
- `SemanticValueRange`: The accessibility value range; null leaves it unset.
- `TabIndex`: The node's place in the tab order when focusable; 0 follows document order.
- `TextPreparedAtCommit`: The frame scheduler's commit count when [`Bitmap.PrepareText`](Bitmap.md) queued this node's shaping.
- `TextPreparedAtMutation`: The [`Node.RetainedMutationCount`](Node.md) when [`Bitmap.PrepareText`](Bitmap.md) last prepared this node's text.
- `ToolTip`: The tooltip shown while the pointer rests on this node or it has visible keyboard focus.
- `Visibility`: This node's own visibility. [`Node.IsVisible`](Node.md) also accounts for ancestors.

## Methods

- `AbsoluteToLocalPosition(float, float)`: Converts a page point to the node's coordinates.
- `AddChild(Node)`: Appends `node`, moving it from its current parent. Adding itself or an existing child does nothing.
- `ApplyNodeMetadata()`: Sends the node id, semantics, visibility and attached properties to a newly created native node.
- `BeginPress(PointerEventArgs, float, float)`: A press either arms a click or, on a drag source, starts the drag gesture with a click held back.
- `BindScrollProxyTarget(ulong)`: Routes scrolling over this node to the scroll node `handle`.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `BuildChildren()`: Builds the children and attaches them to the native node, for a [`Node.Build`](Node.md) override.
- `CapturePersistedState()`: Saves this node's persisted state. Overrides that persist more should call the base method.
- `CapturePointer()`: Sends the pointer's moves and release to this node until it releases them.
- `DebugMainAxisPercentValue(bool)`: For layout diagnostics: the node's percent size on the parent's main axis, or -1.
- `DebugTreePath()`: For layout diagnostics: the child indices from the root, as root/0/2.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `DisposeTree()`: Disposes the node's children and deletes its native node.
- `EndPress(PointerEventArgs, float, float, bool)`: A release over the node fires the armed click; a held-back click fires only if no drag started. The host keeps routing a pressed pointer to the node after it leaves, so a release elsewhere cancels the click, as in browsers and WPF.
- `FinishBuild()`: Registers a newly created native node for events and applies focus and interaction state.
- `Focus()`: Moves keyboard focus to this node once it is built.
- `GetBounds()`: The node's bounds in scene pixels; empty before the node is built and laid out.
- `HandleBubbledPointerEvent(PointerEventArgs)`: Handles a pointer event bubbling up from a descendant; returns whether it is handled.
- `HandleCrossSelectionChanged(string)`: Called when a selection area's selection across nodes changes, with its text.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `HandleScroll(float, float, float, float, float, float)`: Called when the host scrolls the node, with its offset and the sizes of its content and viewport.
- `HandleSelectionChanged(uint, uint)`: Called when the node's selection changes, given in UTF-8 offsets.
- `HandleTextChanged(string)`: Called when the user replaces the node's whole text.
- `HandleTextReplaced(uint, uint, string)`: Called when the user replaces a range of the node's text, given in UTF-8 offsets.
- `LocalToAbsolutePosition(float, float)`: Converts a point in the node's coordinates to the page's.
- `NotifyRetainedLayoutMutation()`: Schedules a commit and tells the parent that this node's layout changed.
- `NotifyRetainedMutation()`: Schedules a commit when the node is built.
- `OnChildLayoutChanged()`: Called when a child's layout changes.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `OnEffectiveVisibilityChanged(Visibility)`: Called when the node's effective visibility changes, through its own setting or an ancestor's.
- `OnParentChanged()`: Called when the node is attached to or detached from a parent.
- `RaiseContextMenuOpening(ContextMenuEventArgs)`: Raises [`Node.ContextMenuOpening`](Node.md); returns whether the node has a handler.
- `RaiseFocusChanged(FocusChangedEventArgs)`: Raises [`Node.FocusChanged`](Node.md), for nodes whose focus is held by an inner node.
- `ReflectSemanticDisabledFromEnabled()`: Keeps [`Node.SemanticDisabled`](Node.md) equal to the inverse of [`Node.IsEnabled`](Node.md).
- `RegisterPersistedState(PersistedStateAdapter)`: Persists state with `adapter`, replacing an adapter of the same kind.
- `ReleasePointer()`: Releases a pointer captured with [`Node.CapturePointer`](Node.md).
- `RemoveChild(Node)`: Removes `node` without disposing it; its native node stays alive so it can be added again.
- `ReplaceChildren(ReadOnlySpan<Node>)`: Replaces the node's children with `nodes`, building them if the node is built.
- `RequestSemanticAnnouncement()`: Asks assistive technology to announce this node.
- `RequiredFontIds()`: The font ids this node renders with, so callers can wait for them to load.
- `RequireInteractive()`: Makes the node a hit-test target while it is enabled and visible.
- `RestorePersistedState()`: Restores this node's persisted state. Overrides that persist more should call the base method.
- `RestorePersistedStateTree()`: Restores children first, so a container restoring its own state sees its content restored.
- `Track(IDisposable)`: Disposes `resource`, such as a signal subscription, when this node is disposed.

## Events

- `ContextMenuOpening`: Raised instead of the built-in context menu when the user asks for one over this node or a descendant that has no handler of its own; show a [`ContextMenu`](ContextMenu.md) or other UI from it.
- `DragCompleted`: Raised on the drag source when its drag ends, with the effect the target applied.
- `DragEnter`: A drag entered this drop target; handlers set [`DragEventArgs.Proposal`](DragEventArgs.md) to accept it.
- `DragLeave`: Raised when a drag leaves the node.
- `DragOver`: A drag moved over this drop target; handlers set [`DragEventArgs.Proposal`](DragEventArgs.md) to accept it.
- `Drop`: Raised when a drag drops on the node.
- `FocusChanged`: Raised when the node gains or loses keyboard focus.
- `KeyDown`: Raised when a key goes down while the node has focus.
- `KeyUp`: Raised when a key goes up while the node has focus.
- `LongPress`: A long press with the default duration and tolerance; use [`Node.LongPressRecognizer`](Node.md) to configure them.
- `PanGesture`: Pan gestures on this node. Subscribing claims pan for the node.
- `PinchGesture`: Pinch gestures on this node. Subscribing claims pinch for the node.
- `PointerCancel`: Raised when the host cancels a pointer, as when a touch becomes a scroll.
- `PointerClick`: A primary press and release on this node. Subscribing makes the node interactive.
- `PointerDoubleClick`: Raised on a double click.
- `PointerDown`: Raised when a pointer presses over the node.
- `PointerEnter`: Raised when a pointer enters the node.
- `PointerLeave`: Raised when a pointer leaves the node.
- `PointerMove`: Raised when a pointer moves over the node.
- `PointerTripleClick`: Raised on a triple click.
- `PointerUp`: Raised when a pointer releases over the node.
- `Wheel`: Raised when the wheel or trackpad scrolls over the node.
