# TextInputPresenter

class in `FuiCs`

Styles a text input's shell for each state. The control binds it to the shell, the node that hosts the editor (the editor itself, or a text area's scroll box) and the placeholder's host before the first [`TextInputPresenter.Apply`](TextInputPresenter.md).

## Properties

- `EditorHost`: The node holding the editor: the editor itself, or a text area's scroll box.
- `Host`: The control itself, which holds the editor and the placeholder.
- `PlaceholderHost`: The placeholder's host, shown over the editor while the text is empty; [`TextInputPresenter.Apply`](TextInputPresenter.md) positions and sizes it so its content lines up with the editor.

## Methods

- `Apply(Theme, TextInputVisualState, TextInputColors)`: Styles the shell for `state`, with `colors` over the theme's.
