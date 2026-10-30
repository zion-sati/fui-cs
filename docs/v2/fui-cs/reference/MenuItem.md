# MenuItem

class in `FuiCs`

An entry of a [`ContextMenu`](ContextMenu.md): a label, an optional shortcut hint, and the action it runs when chosen. [`MenuItem.Separator`](MenuItem.md) draws a divider between groups.

## Properties

- `Action`: The built-in action the item runs, or None.
- `FocusTargetAfterAction`: Returns keyboard focus to the target editor after the action, as an editor's own menu items do.
- `IsEnabled`: Whether the item can be chosen; a disabled item shows muted.
- `IsSeparator`: Whether the item is a divider.
- `Label`: The item's text.
- `Payload`: The text or URL a built-in action uses.
- `SelectionRange`: The target's selection, in UTF-8 offsets, when the menu opened; Cut deletes it if the snapshot is gone.
- `Separator`: A divider between groups of items; leading, trailing and repeated dividers are not drawn.
- `ShortcutLabel`: A shortcut hint drawn after the label, such as "Ctrl+C".
- `TargetHandle`: The text node a text action applies to.

## Events

- `Click`: Raised when the user chooses the item, after its built-in action runs and the menu hides.
