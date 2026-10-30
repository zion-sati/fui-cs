# TextInputCore

class in `FuiCs`: [`FlexBox`](FlexBox.md)

The editable text control that [`TextInput`](TextInput.md) and [`TextArea`](TextArea.md) build on: a shell styled by its [`TextInputTemplate`](TextInputTemplate.md) around an editable text node, with a placeholder while the text is empty. Selection positions count characters (Unicode scalar values); the `ByteOffset` members give the same positions in UTF-8 bytes, as the runtime counts them. Its text persists across history navigation, unless it is a password.

## Properties

- `AcceptsTab`: Tab inserts a tab character instead of moving focus.
- `Colors`: Colors over the theme's for the shell, text, placeholder and caret.
- `Editor`: The editable text node inside the shell.
- `FontFamily`: The theme's body family when null.
- `FontSize`: The theme's body size when null.
- `HorizontalScrollBarVisibility`: A text area's horizontal scroll bar, hidden while it wraps.
- `HostAutofill`: The browser autofill hint (such as `username` or `current-password`); null or empty for none.
- `IsFocused`: Whether the editor has keyboard focus.
- `IsPassword`: Shows the text as bullets and marks the field as a password for the browser and password managers.
- `IsReadOnly`: Lets the user select and copy the text but not change it.
- `IsSelectionBarrier`: A text input contains selection: page selection does not extend into or out of it.
- `KeyDownOverride`: Answers the editor's key presses in place of the input's own handling, for a control built around the input (a combo box). Keys it returns false for stay unhandled, so Tab still moves focus.
- `LineHeight`: The editor's and placeholder's line height; null follows the font.
- `MaxChars`: The most characters the user can enter; null for no limit.
- `Placeholder`: Shown in the muted color while the text is empty; it is also the default accessible label.
- `SelectionEnd`: The selection's end, in characters; it equals the start for a caret.
- `SelectionEndByteOffset`: The selection's end in UTF-8 bytes, as the runtime counts.
- `SelectionStart`: The selection's start, in characters.
- `SelectionStartByteOffset`: The selection's start in UTF-8 bytes, as the runtime counts.
- `SemanticLabel`: The editor's accessible label, which otherwise follows the placeholder, a read-only value or the control's kind.
- `Template`: Builds the shell's look; null uses [`TextInputTemplate.Default`](TextInputTemplate.md).
- `Text`: The text. Setting it moves the caret to the end and does not raise [`TextInputCore.TextChanged`](TextInputCore.md).
- `VerticalScrollBarVisibility`: A text area's vertical scroll bar.
- `Wrapping`: Wraps lines at the editor's width; a text area wraps by default and scrolls sideways when it does not.

## Methods

- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `CaretToEnd()`: Puts the caret after the last character.
- `Dispose()`: Disposes the subtree, deleting each native node once.
- `Focus()`: Focuses the editor and restores the selection the control holds.
- `HandleEditorKeyDown(string, KeyModifier)`: Consumes the editor's navigation keys, so they do not reach the page, and inserts a tab when the input accepts tabs. Other keys, such as browser shortcuts, pass through.
- `HandlePointerEvent(PointerEventArgs)`: Handles a pointer event routed to this node: raises the pointer events, then tracks clicks and drags. Overrides should call the base method.
- `OnEffectiveEnabledChanged(bool)`: Called when the node's effective enabled state changes, through its own setting or an ancestor's.
- `SetCaret(int)`: Puts the caret at a character position, clamped to the end.
- `SetSelection(int, int)`: Selects from `start` to `end` characters; positions past the end clamp to it.

## Events

- `SelectionChanged`: Raised when the user moves the caret or selection, with character positions.
- `TextChanged`: Raised when the user edits the text, or a restored value differs; not when code sets [`TextInputCore.Text`](TextInputCore.md).
