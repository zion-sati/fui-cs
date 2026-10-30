# TextCore

class in `FuiCs`: [`Node`](Node.md), [`ISizable`](ISizable.md), [`ITextFormat`](ITextFormat.md)

A retained text node: content, font, color, alignment, wrapping, limits, selection and editing. Unset color, font family and selection color follow the active theme.

## Properties

- `CaretColor`: The caret's color while editing; null follows the theme's accent.
- `Editable`: Lets the user edit the text; editable text is selectable unless selection was set explicitly.
- `FillHeight`: Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.
- `FillHeightPercent`: Takes this percentage of the remaining height; null when not set.
- `FillWidth`: Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.
- `FillWidthPercent`: Takes this percentage of the remaining width; null when not set.
- `FontFamily`: The theme's body family when null; setting it replaces a [`TextCore.FontStack`](TextCore.md).
- `FontSize`: The font size in pixels; 0 leaves the host default until set.
- `FontStack`: A single stack to render with instead of a family; set it with its size through [`TextCore.SetFontStack`](TextCore.md).
- `FontStyle`: The style of the face to use from the family.
- `FontWeight`: The weight of the face to use from the family.
- `HasAutomaticCursor`: Whether the cursor is the I-beam that being selectable gave it, which an ancestor's own cursor overrides.
- `Height`: A fixed height; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `IsEditableText`: Whether the node is text the user can edit.
- `IsSelectableText`: Whether the node is text the user can select.
- `LineHeight`: The line height in pixels; null uses the font's own.
- `MaxChars`: The most characters the text may hold.
- `MaxHeight`: The largest height layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MaxLines`: The maximum number of lines; 0 means no limit.
- `MaxWidth`: The largest width layout gives the node; [`Length.Auto`](Length.md) for no maximum.
- `MinHeight`: The smallest height layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `MinWidth`: The smallest width layout gives the node; [`Length.Auto`](Length.md) for no minimum.
- `Obscured`: Renders the text as bullets, for passwords.
- `Overflow`: How text past the box or line limit ends.
- `Selectable`: Lets the user select the text, with the I-beam cursor.
- `SelectedText`: The selected text; the selection's UTF-8 offsets are converted to the string's.
- `SelectionColor`: The selection highlight; null follows the theme's selection color.
- `SelectionEnd`: Where the selection ends, in UTF-8 offsets.
- `SelectionStart`: Where the selection starts, in UTF-8 offsets.
- `Text`: The text. The host updates it as the user edits editable text.
- `TextAlign`: How lines align horizontally.
- `TextColor`: The text color; null follows the theme's primary text color.
- `UsesDefaultSelectionBehavior`: Whether selection was never set explicitly, so the node keeps its type's default.
- `VerticalAlign`: How the text aligns vertically in its box.
- `Width`: A fixed width; [`Length.Auto`](Length.md) sizes to content. Setting it replaces any fill.
- `Wrapping`: Wraps lines at the available width. On by default.

## Methods

- `ApplyOverflowFade(bool, bool)`: Fades text that overflows the box horizontally and/or vertically.
- `Build()`: Creates the native node and its subtree if it does not exist yet, and returns its handle.
- `HandleSelectionChanged(uint, uint)`: Called when the node's selection changes, given in UTF-8 offsets.
- `HandleTextChanged(string)`: Called when the user replaces the node's whole text.
- `HandleTextReplaced(uint, uint, string)`: Called when the user replaces a range of the node's text, given in UTF-8 offsets.
- `OnContentChanged()`: Called after the text changes, from code or by the user.
- `OnThemeChanged(Theme)`: Applies `theme` to the theme-derived color, font and selection color. Overrides should call the base method.
- `SetFontId(uint, float)`: Renders with the font `id` at `fontSize`, bypassing family resolution.
- `SetFontStack(FontStack, float)`: Renders with `stack` at `fontSize` instead of a family.
- `SetTextLimits(int, int)`: Limits the text to `maxChars` characters and `maxLines` lines (0 for no line limit).
- `SetTextStyleRuns(uint[])`: Sets the styled runs, seven words each: start and end UTF-8 offsets, font id, size bits, color, background, decorations.
- `TextBetween(uint, uint)`: The text between two UTF-8 offsets, in either order.

## Events

- `SelectionChanged`: Raised when the selection changes, with UTF-8 offsets.
- `TextChanged`: Raised when the user edits the text.
