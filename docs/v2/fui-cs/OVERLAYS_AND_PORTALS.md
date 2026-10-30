# FUI-CS Overlays and Portals

Content drawn above the page: popups, dialogs, menus, dropdown lists and
tooltips.

## Portals

A `Portal` is a node whose subtree the runtime draws above the normal tree, so
an overlay escapes its parent's clipping and stacking. Every overlay control
below is, or holds, a portal. Mount the control somewhere in the page before
showing it; where you mount it does not decide where it appears.

## Popup

`Popup` shows any content over the page:

- `ShowAnchored(x, y, width, height, popupWidth, popupHeight)` places it next to
  an anchor rectangle by `Placement` (`Auto` below, or above when only that
  fits; `Top`, `Bottom`, `Overlap`), `AnchorGap` from it and `EdgePadding`
  inside the viewport;
- `ShowAtPoint(x, y, width, height)` places it at a point, kept inside the
  viewport.

A `BackdropColor` veils the page while it is open, and a click on the
backdrop closes it unless `DismissOnBackdropClick` is off. `Opened` and
`Closed` report when it shows and hides. `PanelColor`, `PanelBackgroundBlur`,
`PanelCornerRadius`, `PanelBorder` and `PanelShadow` style its panel, which is
bare until set.

## Dialog

`Dialog` is modal: a card with a title, body text, optional `BodyContent` and
OK and Cancel buttons, centered over a blurred backdrop that covers the
viewport. `Show()` opens it, or `await dialog.ShowAsync()` returns whether it
was accepted. Enter accepts, Escape or a backdrop click cancels, and focus
returns to where it was. `Shown` runs once the dialog is on screen, which is
the time to focus a field inside it. The card's color, border, corners and
shadow, and the backdrop, follow the theme until set. `Dialog.Active` is the
open dialog; `AcceptActive()` and `CancelActive()` close it.

## Context menus

Right-clicking shows the built-in context menu (see the
[Quickstart](./QUICKSTART.md#application-model) for its items). A node's
`ContextMenuOpening` event replaces it for the node and its descendants
without a handler of their own: show your own `ContextMenu` from the handler
with `menu.Show(e.Target, e.X, e.Y)`. A `ContextMenu` lists `MenuItem`s,
each a label, an optional `ShortcutLabel` (`KeyboardShortcuts.FormatPrimaryLabel`
writes one), and either a built-in `ContextMenuAction` or your click handler
(`new MenuItem("Rename", Rename)`); `MenuItem.Separator` divides groups. `ContextMenuManager.SetMenu(menu)` shows the built-in items in your own
styled menu. `IsContextMenuEnabled = false` turns menus off for a subtree.

Escape, a press outside, or choosing an item closes the menu; one menu is open
at a time (`ContextMenu.Active`, `ContextMenu.HideActive()`).

## Dropdown and combo box lists

A dropdown's or combo box's list is a portal anchored under its field, or
above it when only that fits, and kept inside the viewport. `MaxVisibleItems`
limits its rows before it scrolls, and `PopupWidth` widens it past the field.
It closes on Escape, on a press outside, when its field scrolls out of view,
and when another list opens: one dropdown and one combo box list are open at a
time (`Dropdown.Active`, `ComboBox.Active`, and `HideActive()` on each).

## Tooltips

`node.ToolTip("Save the file")` sets a tooltip; `new ToolTip(text) { ... }`
sets its timing (`InitialShowDelay`, `BetweenShowDelay`, `ShowDuration`), its
`Placement` and offsets. A tooltip shows at the pointer after the pointer
rests on the node, or next to the node while it has visible keyboard focus, and
hides when the pointer leaves or presses, focus moves, the page scrolls under
it, or its duration runs out.

## Touch selection chrome

Under touch, selected text shows handles at its ends and a toolbar of actions
above it; both live in the application shell's overlay layer. See the
[Text input reference](./TEXT_INPUT_REFERENCE.md#touch).
