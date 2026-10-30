# FUI-CS Keyboard Policy

Which keys each part of the SDK handles, and what happens to the rest.

## Focus

Tab and Shift+Tab move keyboard focus through focusable nodes as HTML's
`tabindex` does: positive tab indexes first, in ascending order, then the rest
in document order; a negative index leaves a node out of the tab order.
`Focusable(true, tabIndex)` makes any node a tab stop; controls are focusable by
default and leave the tab order while disabled. A text input's or combo box's tab stop is its editor, and the control
reports the editor's focus as its own (`IsFocused`, `FocusChanged`).

The focus ring shows after keyboard input and hides after pointer input, as
browsers' `:focus-visible` does: `FocusVisibility.KeyboardFocusVisible` follows
the last input. Escape and caret-moving keys do not turn it on.

## Keys by control

| Control | Keys |
|---|---|
| `Button` | Enter or Space presses; releasing it clicks |
| `Checkbox`, `Switch`, `RadioButton` | Space toggles |
| `RadioGroup` (a radio focused) | Arrows move the selection, Home and End select the first or last |
| `Slider` | Left and Right (Up and Down when vertical) step; Home and End go to the ends |
| `Dropdown` (closed) | Enter, Space or Down opens; Up opens with the item above the selection highlighted |
| `Dropdown` (open) | Up and Down move the highlight (wrapping), Home and End go to the ends, Enter selects, Escape closes; Tab moves focus on and leaves the list open until a press outside |
| `ComboBox` | Down or Up opens the list; while it is open, Up and Down move the highlight (wrapping), Home and End go to the ends, Enter selects and Escape closes; while it is closed, the caret keys stay with the field; Tab moves focus on and closes the list |
| `TextInput`, `TextArea` | Typing and the browser's editing keys edit; the arrows, Home, End, Page Up and Page Down move the caret and do not scroll the page; Tab moves focus unless `AcceptsTab`; Enter adds a line in a text area only |
| `Dialog` | Enter accepts (a focused button takes Enter itself), Escape cancels |
| `Form` (active) | Enter presses `DefaultButton`, Escape presses `CancelButton` |
| `ContextMenu` | Escape closes |
| `NavLink` | Enter navigates; Ctrl+Enter or ⌘+Enter opens a new tab |
| `TabView` | None: it has no tab strip; the selector you build handles keys |

## Unhandled keys

A key no focused node handles falls through:

- The arrows, Page Up, Page Down, Home and End scroll the nearest scroll view
  around the focused node, or the one last clicked. Text keeps Home and End,
  and editable text keeps the arrows.
- Tab moves focus.
- Browser shortcuts (Ctrl or ⌘ with a key, function keys) go to the browser,
  except the editing shortcuts a focused editor handles: copy, cut, paste,
  select all, undo and redo.

A handler that sets `Handled` on `KeyDown` keeps a key from this fall-through.

## Open popups

An open dropdown, combo box list or context menu takes its keys before the
focused node, so the arrows move its highlight even while focus stays on the
field. The keys it does not use go on to the focused node.

## Platform shortcuts

`KeyboardShortcuts` answers platform questions: `PrimaryModifier()` is ⌘ on
Apple platforms and Ctrl elsewhere, `IsUndo` and `IsRedo` recognize the
platform's undo and redo, and `FormatPrimaryLabel("c")` writes a shortcut hint
such as `Ctrl+C` or `⌘C` for menus.
