# Dropdown

class in `FuiCs`: [`FlexBox`](FlexBox.md), `ISelectablePopupListOwner`

Picks one of [`Dropdown.Items`](Dropdown.md) from a list that opens under its field. A press, Enter, Space or the down arrow opens the list; while it is open the arrows, Home and End move the highlight, Enter or a release on a row selects, and Escape, a press outside the list, or scrolling the field out of view closes it. Focus stays on the field. Its selection persists across history navigation.

Fluent members: [`DropdownExtensions`](DropdownExtensions.md).

## Properties

- `Active`: The open dropdown, if any.
- `ChevronTemplate`: Builds the chevron; null uses [`DropdownChevronTemplate.Default`](DropdownChevronTemplate.md).
- `Colors`: Colors over the theme's, carried into the field and list.
- `FieldTemplate`: Builds the field; null uses [`DropdownFieldTemplate.Default`](DropdownFieldTemplate.md).
- `HighlightedIndex`: The item the keyboard or pointer highlights in the open list, or -1.
- `IsOpen`: Whether the list is open.
- `Items`: The options. Assigning them selects the first, or nothing when there are none.
- `MaxVisibleItems`: The most rows the list shows before it scrolls; null shows every row.
- `OptionRowTemplate`: Builds the list's rows; null uses [`DropdownOptionRowTemplate.Default`](DropdownOptionRowTemplate.md).
- `PopupPanelBackgroundBlur`: The blur of the page behind the list panel; null is the default blur.
- `PopupPanelColor`: The list panel's fill; null follows the theme's context menu panel.
- `PopupWidth`: The list's width; null matches the field.
- `SelectedIndex`: The selected item's index, or -1 for none. Setting it does not raise [`Dropdown.Changed`](Dropdown.md).
- `SelectedItem`: The selected item, if any.
- `Sizing`: The field's and rows' sizes; the default templates' when null.

## Methods

- `DismissActiveIfTriggerScrolledAway()`: Closes the open dropdown once its field leaves the scroll view around it, or the viewport.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandleOpenKey(KeyEventType, string, KeyModifier)`: The keys of the open list, ahead of the focused node: Escape, Enter, Home, End and the arrows.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `HideActive()`: Closes the open dropdown, if any.
- `HighlightIndex(int)`: Highlights an item of the open list.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `SelectHighlighted()`: Selects the highlighted item, raising [`Dropdown.Changed`](Dropdown.md) when it differs, and closes the list.

## Events

- `Changed`: Raised when the user selects another item, or a restored selection differs; not when code sets it.
