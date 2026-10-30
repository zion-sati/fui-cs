# FUI-CS Text Input Reference

`TextInput` (one line), `TextArea` (several lines) and the field of a
`ComboBox` share one editing core, `TextInputCore`. Editing itself (typing,
IME composition, the browser's editing keys, clipboard, undo) runs in the
EffinDom runtime and the browser bridge; the control holds the value, the
selection and the presentation, and reports changes.

## Structure

A text input is a shell (`TextInputCore`, a `FlexBox`) around an editor text
node (`Editor`, a `TextCore`) and, while the text is empty, a placeholder. A
text area puts its editor in a scroll box. The shell's look comes from its
`TextInputTemplate`'s presenter; `TextInputColors` override the shell's fill
and border and the text, disabled text, placeholder and caret colors.

## Value and events

| Member | Meaning |
|---|---|
| `Text` | The value. Setting it moves the caret to the end and raises nothing |
| `TextChanged` | Raised for each edit by the user, and for a restored value that differs |
| `Placeholder` | Shown while the text is empty; also the default accessible label |
| `MaxChars` | The character limit; null for none. The runtime does not yet apply it to typed text |
| `IsReadOnly` | Selectable, not editable |
| `IsPassword` | Obscured; marked as a password for the browser and password managers; never persisted |
| `AcceptsTab` | Tab inserts a tab instead of moving focus |
| `HostAutofill` | The browser autofill hint, such as `username` (see [Forms and autofill](./FORMS_AND_AUTOFILL.md)) |
| `FontFamily`, `FontSize`, `LineHeight` | The theme's body text until set |

The text persists across history navigation under the control's `NodeId`,
unless it is a password.

## Positions

Selection positions on a text input count characters (Unicode scalar values),
so an emoji or an accented letter is one position whatever its encoding:

| Member | Meaning |
|---|---|
| `SelectionStart`, `SelectionEnd` | The selection in characters; equal for a caret. The end may be before the start for a backward selection |
| `SelectionStartByteOffset`, `SelectionEndByteOffset` | The same positions in UTF-8 bytes, as the runtime counts |
| `SetSelection(start, end)`, `SetCaret(position)`, `CaretToEnd()` | Move the selection from code; positions past the end clamp to it, negative positions throw |
| `SelectionChanged` | Raised when the user moves the caret or selection, with character positions |

A plain `TextCore` node with `Editable` set reports UTF-8 offsets instead, as
the runtime does.

## Focus

The editor holds keyboard focus. The control reports it as its own:
`IsFocused` and `FocusChanged` follow the editor, `Focus()` focuses the editor
and restores the selection the control holds, and a press anywhere on the
shell focuses the editor.

## Keys

Typing, Backspace, Delete, the editing shortcuts (copy, cut, paste, select
all, undo, redo) and IME composition edit the text. The arrows, Home, End, Page
Up and Page Down move the caret and do not scroll the page. Enter adds a line
in a text area and does nothing in a single-line input. Tab moves focus unless
`AcceptsTab`. Other shortcuts and function keys pass to the browser.

## Text area

`TextArea` wraps by default (`Wrapping`) and scrolls vertically; with wrapping
off it scrolls sideways too. `VerticalScrollBarVisibility` and
`HorizontalScrollBarVisibility` choose when its bars show (the horizontal bar
stays hidden while wrapping).

## Combo box field

A `ComboBox`'s field is a `TextInput` inside it. The combo box takes Up, Down,
Enter and Escape for its list and leaves the caret keys to the field; Tab moves
on. Its `Text` and `TextChanged` are the field's.

## Context menu

Right-clicking an editor shows Undo, Redo, Cut, Copy, Paste and Select All,
and returns focus to the editor after each. Undo and Redo are enabled while
there is an edit to undo or redo, and Cut and Copy while there is a selection;
Cut and Copy appear where the host can write the clipboard, and Paste where it
can read it.

## Touch

Under touch, a long press on text selects a word and shows handles at the
selection's ends and a toolbar above it. In an editor the toolbar offers Cut,
Copy, Paste and Select all (the fourth behind More); dragging a handle extends
the selection, and a tap elsewhere closes the toolbar.
`SelectionHandles.Mode` chooses when handles show: for touch selections
(`Auto`), for every selection (`Always`), or never (`Disabled`).

## Semantics

The editor has the text box role. Its label is, in order: an explicit
`SemanticLabel`, the placeholder, a read-only input's text, then "Password
input" or "Text input" (or "Text area"). Inside a `Form` with an autofill hint,
the projected form field stands in for the editor in the accessibility tree.
