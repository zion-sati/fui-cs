# FUI-CS Accessibility and Semantics

The EffinDom runtime keeps a semantic tree beside the retained tree, which the
browser bridge exposes to assistive technology, the browser's find on page and
its inspection tools. Every node can carry semantics; controls set theirs by
default.

## Node semantics

| Member | Meaning |
|---|---|
| `SemanticRole` | The node's role (`Button`, `Checkbox`, `Link`, `Textbox`, `Slider`, `List`, `ListItem`, `Dialog`, `Heading`, ...) |
| `SemanticLabel` | The accessible name |
| `SemanticChecked` | Checked, unchecked or mixed |
| `SemanticExpanded` | Whether a collapsible control is open; null when it does not apply |
| `SemanticSelected` | Whether an item is selected |
| `SemanticDisabled` | Whether the node is disabled; controls keep it in step with `IsEnabled` |
| `SemanticValueRange` | A value between a minimum and maximum, as for a slider |
| `SemanticOrientation` | Horizontal or vertical |
| `RequestSemanticAnnouncement()` | Asks assistive technology to announce the node's current state |

An explicit role or label replaces the control's default; leave them unset to
keep the defaults below.

## Defaults

| Node | Role | Label |
|---|---|---|
| `Text` | Static text, unless it is empty or an ancestor has a role | Its text |
| `Button` | Button | Its label |
| `Checkbox`, `Switch`, `RadioButton` | Checkbox, switch, radio | Their label, with the checked state |
| `RadioGroup` | Radio group | |
| `Slider` | Slider | "Slider" or "Vertical slider", with the value range and orientation |
| `Dropdown` | Combo box, with expanded state | The selected item's label, or "Dropdown" |
| `ComboBox` | Combo box, with expanded state | Its text, or "Combo box" |
| Dropdown and combo box lists | List of list items | Each item's label, with its selected state |
| `TextInput`, `TextArea` editors | Text box | See the [Text input reference](./TEXT_INPUT_REFERENCE.md#semantics) |
| `Image`, `Svg` | Image | `AltText` |
| `NavLink` | Link | Set `SemanticLabel` to name the link |
| `Dialog` | Dialog, marked modal; while open, the semantic tree is scoped to it | Its title |
| `Form` | Form | |
| `TabView` panels | Tab panel | |

A `Text` inside a node that has a role takes no role or label of its own, so it
is not read beside the control. Controls with a text label (buttons, toggles,
dropdowns) label themselves; give any other control with a role, such as a
`NavLink` around custom content, a `SemanticLabel`.

## Announcements

Controls announce the user's changes: a dropdown or combo box opening, closing
or choosing; a checkbox, switch or radio button toggled; a slider moved. Call
`RequestSemanticAnnouncement()` after changing state the user should hear about.

## Focus and keyboard

Every control is reachable by keyboard; see the
[Keyboard policy](./KEYBOARD_POLICY.md). The focus ring shows after keyboard
input, so keyboard users always see where focus is. A `Dialog` focuses its OK
button when shown, scopes the semantic tree to itself while open, and returns
focus to where it was when it closes.

## Text

Text is selectable by default, and the browser's find on page finds it. A
`SelectionArea` lets one drag select across many nodes; `AntiSelectionArea`
keeps a part out. Editable text keeps its own selection, which the page's
selection does not cross.

## Projected forms

Text inputs with an autofill hint inside a `Form` are projected as real form
fields, which stand in for the editors in the accessibility tree (see
[Forms and autofill](./FORMS_AND_AUTOFILL.md)).
