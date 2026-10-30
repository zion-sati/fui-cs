# FUI-CS Events and Callbacks

Input reaches an application as multicast C# events on its nodes, in the order
the host reports it. Every event has an `On...` fluent member that subscribes
and returns the node.

## Node-level events

| Event | Raised when | Arguments |
|---|---|---|
| `PointerDown`, `PointerMove`, `PointerUp` | A pointer presses, moves or releases over the node | `PointerEventArgs`: page point (`SceneX`/`SceneY`), node point (`X`/`Y`), `Button`, `Buttons`, `PointerType`, `Modifiers`, pressure and tilt |
| `PointerEnter`, `PointerLeave` | The pointer enters or leaves | `PointerEventArgs` |
| `PointerCancel` | The host takes the pointer away, as when a touch becomes a scroll | `PointerEventArgs` |
| `PointerClick`, `PointerDoubleClick`, `PointerTripleClick` | A primary press and release on the same node, 1, 2 or 3 times | `PointerClickEventArgs` with `ClickCount` |
| `Wheel` | The wheel or trackpad scrolls over the node | `WheelEventArgs`: deltas, `DeltaMode`, `Modifiers` |
| `PanGesture`, `PinchGesture` | A touch pan or pinch on the node | `GestureEventArgs`: `Phase`, `Kind`, deltas, `Scale`, `PointerCount` |
| `LongPress` | A pointer is held still past the minimum duration | `LongPressEventArgs`: point, `DurationMs` |
| `KeyDown`, `KeyUp` | A key goes down or up while the node has focus | `KeyEventArgs`: `Key` (the browser's `KeyboardEvent.key`), `Modifiers` |
| `FocusChanged` | The node gains or loses keyboard focus | `FocusChangedEventArgs.Focused` |
| `DragEnter`, `DragOver`, `DragLeave`, `Drop` | A drag moves over or drops on the node | `DragEventArgs`: `Session`, point, `Modifiers`, `Proposal` |
| `DragCompleted` | A drag that started on the node ends | `DragCompletedEventArgs.Effect` |
| `ContextMenuOpening` | The user asks for a context menu over the node or a descendant without a handler; it replaces the built-in menu | `ContextMenuEventArgs`: `Target`, `X`, `Y` |

A release outside the node that took the press does not click, as in browsers
and WPF.

## Routing and `Handled`

Pointer events go to the deepest interactive node under the pointer, then
bubble to its ancestors' events until a handler sets `Handled`. A node that
captured the pointer gets the moves and release wherever the pointer goes.
Subscribing to a pointer event makes a node interactive, so it can be hit.

Key events go to the focused node only; they do not bubble. A key nobody
handles may scroll the view around the focused node (the arrows, Page Up, Page
Down, Home and End) or move focus (Tab). Setting `Handled` on a key event keeps
it from that default handling.

Wheel events bubble the same way as pointer events; setting `Handled` keeps the
wheel from scrolling an ancestor.

## Gestures and long presses

Subscribing to `PanGesture` or `PinchGesture` claims that gesture for the node:
a pan or pinch over it goes to the node instead of scrolling or zooming the
page. `LongPress` uses the defaults in `LongPressDefaults` (500 ms, 10 px);
`LongPressRecognizer(LongPressGesture.Create().MinimumDuration(800))` sets a
node's own; `MovementTolerance` sets how far the pointer may move.

## Drag and drop

A node becomes a drag source when its `DragDataProvider` is set: the function
returns a `DragDataObject` (text with `SetText`, or any format with
`SetFormat`), or null to decline. `DragAllowedEffects` limits what the drop may
do.

A node accepts drops once it subscribes to a drop event (or sets `AllowDrop`).
In `DragEnter` and `DragOver`, set `e.Proposal = new DropProposal(DragDropEffects.Move)`
to accept; read the data from `e.Session.Data`. `Drop` runs on the accepting
node, and `DragCompleted` reports the effect to the source.

## Control events

| Control | Event | Raised for |
|---|---|---|
| `Button` | `Click` | A primary click, or Enter or Space while focused |
| `Button` | `HoverChanged` | The pointer entering or leaving |
| `Checkbox`, `Switch`, `RadioButton` | `Changed` | Every change, from the user or code |
| `RadioGroup` | `Changed` | Every selection change |
| `Slider` | `Changed` | Every value change |
| `Dropdown` | `Changed` | The user's selection (and a restored one that differs) |
| `ComboBox` | `Changed`, `TextChanged` | The user's selection and edits |
| `TextInput`, `TextArea` | `TextChanged`, `SelectionChanged`, `FocusChanged` | The user's edits, caret and selection moves, and the editor's focus |
| `Dialog` | `Accepted`, `Cancelled`, `Shown` | The dialog's result, and its first frame on screen |
| `ContextMenu` | `VisibilityChanged` | Showing and hiding |
| `Popup` | `Opened`, `Closed` | Showing and hiding |
| `TabView` | `SelectionChanged` | Every selection change |
| `NavLink` | `Navigate`, `InteractionStateChanged` | Activation, and hover, press and focus |
| `ScrollView`, `ScrollBox` | `ScrollChanged` | Scrolling, or the content or viewport changing size |
| `SelectionArea` | `SelectedTextChanged` | The selection across the area changing |

## App-wide signals

`Signal<T>` values change outside any one node: `Theme.Active`,
`Application.ViewportWidth` and `ViewportHeight`, `Application.FrameTime`,
`Navigation.CurrentRoute`, `FocusVisibility.KeyboardFocusVisible`, and a scroll
box's `ScrollState`. `Subscribe(handler)` returns an `IDisposable`; inside a
node subclass, `Track(...)` releases it when the node is disposed.

## Timing

Handlers run on the page's single thread between frames. Changing properties
in a handler schedules one commit for the frame; nothing redraws partway.
`FrameScheduler.OnLoaded(handler)` runs once the page's first frame is
committed, which is when bounds and fonts can be read. Timers are the BCL's:
`await Task.Delay(...)`, `PeriodicTimer` and `TimeProvider` resume on the page
thread.
