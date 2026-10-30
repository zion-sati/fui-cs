# KeyboardShortcuts

class in `FuiCs`

The platform's keyboard conventions: which modifier drives shortcuts and text navigation, undo and redo detection, and shortcut labels. Every member defaults to the current [`Platform.Family`](Platform.md).

## Methods

- `DocumentBoundaryModifier(PlatformFamily?)`: The modifier that moves to the document's start or end.
- `FormatLabel(string, KeyModifier, PlatformFamily?)`: A shortcut's label: modifier symbols run together on Apple platforms ("⇧⌘Z"), names joined by + elsewhere ("Ctrl+Shift+Z").
- `FormatPrimaryLabel(string, PlatformFamily?)`: The label of the primary modifier with `key`, such as Ctrl+C or ⌘C.
- `HasDocumentBoundaryModifier(KeyModifier, PlatformFamily?)`: Whether `modifiers` hold the platform's document-boundary modifier.
- `HasLineBoundaryModifier(KeyModifier, PlatformFamily?)`: Whether `modifiers` hold the platform's line-boundary modifier.
- `HasPrimaryModifier(KeyModifier, PlatformFamily?)`: Whether `modifiers` hold the platform's primary shortcut modifier (Cmd on Apple, Ctrl elsewhere).
- `HasWordNavigationModifier(KeyModifier, PlatformFamily?)`: Whether `modifiers` hold the platform's word-navigation modifier (Alt on Apple, Ctrl elsewhere).
- `IsRedo(string, KeyModifier, PlatformFamily?)`: Whether the key is the platform's Redo shortcut.
- `IsUndo(string, KeyModifier, PlatformFamily?)`: Whether the key is the platform's Undo shortcut.
- `LineBoundaryModifier(PlatformFamily?)`: The modifier that moves to a line's start or end: Command on Apple platforms, none elsewhere.
- `PrimaryModifier(PlatformFamily?)`: Command on Apple platforms, Control elsewhere.
- `RedoLabel(PlatformFamily?)`: The label of the platform's Redo shortcut.
- `UndoLabel(PlatformFamily?)`: The label of the platform's Undo shortcut.
- `WordNavigationModifier(PlatformFamily?)`: The modifier that moves by word: Option on Apple platforms, Control elsewhere.
