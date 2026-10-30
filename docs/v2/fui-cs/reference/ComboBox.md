# ComboBox

class in `FuiCs`: [`FlexBox`](FlexBox.md), `ISelectablePopupListOwner`

A text field with a list of [`ComboBox.Items`](ComboBox.md) that filters as the user types. The list opens from the chevron, a press on the field's edge, or the arrows; while it is open the arrows, Home and End move the highlight, Enter or a release on a row selects, and Escape, a press outside, blurring or scrolling the field out of view closes it.

Fluent members: [`ComboBoxExtensions`](ComboBoxExtensions.md).

## Properties

- `Active`: The open combo box, if any.
- `AllowCustomValue`: Text that matches no item clears the selection; when false, the last match stays selected.
- `AutoComplete`: Completes the typed text with the first item that starts with it, selecting the completed part.
- `ChevronTemplate`: Builds the chevron; null uses [`DropdownChevronTemplate.Default`](DropdownChevronTemplate.md).
- `Colors`: Colors over the theme's, carried into the field and list.
- `CommitMode`: What happens to the text when the list closes or the field blurs.
- `FilteredCount`: How many items the list shows for the current text.
- `FilterMode`: Which items the list shows for the text.
- `HighlightedIndex`: The highlighted row of the open list, or -1.
- `IsOpen`: Whether the list is open.
- `Items`: The items the list filters. Assigning them closes the list and selects the item matching the text.
- `MaxVisibleItems`: The most rows the list shows before it scrolls; null shows every row.
- `OpenOnFocus`: Opens the list when the field gains focus.
- `OptionRowTemplate`: Builds the list's rows; null uses [`DropdownOptionRowTemplate.Default`](DropdownOptionRowTemplate.md).
- `Placeholder`: Shown in the field while its text is empty.
- `PopupPanelBackgroundBlur`: The blur of the page behind the list panel; null is the default blur.
- `PopupPanelColor`: The list panel's fill; null follows the theme's context menu panel.
- `PopupWidth`: The list's width; null matches the field.
- `SelectedIndex`: The selected item's index, or -1 for none. Setting it puts the item's text in the field and raises nothing.
- `SelectedItem`: The selected item, if any.
- `Sizing`: The field's height and font size, the chevron's box, and the list's rows.
- `StaysOpenOnEdit`: Opens the list, or keeps it open, as the user types. On by default.
- `Text`: The field's text. Setting it does not raise [`ComboBox.TextChanged`](ComboBox.md).

## Methods

- `DismissActiveIfTriggerScrolledAway()`: Closes the open combo box once its field leaves the scroll view around it, or the viewport.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `HandleEditorKeyDown(string, KeyModifier)`: The editor's keys: the arrows open the list or move its highlight, and Enter and Escape act on the open list.
- `HandleFocusChanged(bool)`: Called when the node gains or loses focus; raises [`Node.FocusChanged`](Node.md).
- `HandleKeyEvent(KeyEventType, string, KeyModifier)`: Handles a key event for the focused node; returns true when a key handler ran.
- `HandleOpenKey(KeyEventType, string, KeyModifier)`: The keys of the open list, ahead of the focused node: Escape, Enter, Home, End and the arrows.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `HideActive()`: Closes the open combo box, if any.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `SyncFocusedState()`: Follows focus moving between the field and its editor. Losing both closes the list after the next commit, unless a press on a row holds it, so focus moving from one to the other does not close it in between.
- `VisibleIndexOf(int)`: The row an item shows on, or the first row when the filter hides it, or -1 when nothing shows.

## Events

- `Changed`: Raised when the user selects another item; not when code sets [`ComboBox.SelectedIndex`](ComboBox.md).
- `TextChanged`: Raised when the user edits the text, or committing the text replaces it; not when code sets [`ComboBox.Text`](ComboBox.md).
