# DragDataObject

class in `FuiCs`

The data a drag source offers, keyed by format. Plain text uses [`DragDataObject.TextFormat`](DragDataObject.md).

## Methods

- `GetFormat(string)`: The data stored under `format`, if any.
- `GetText()`: The plain text, if any.
- `HasFormat(string)`: Whether data is stored under `format`.
- `SetFormat(string, string)`: Stores `value` under `format`.
- `SetText(string)`: Stores plain text.

## Fields

- `TextFormat`: The format of plain text.
