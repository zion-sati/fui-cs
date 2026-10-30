# FUI-CS Controls and Nodes

The retained building blocks an application composes. Each type's members are
in the [per-type reference](./reference/README.md); this page says what each is
for and the members you reach for first.

## Controls

| Control | Purpose | Key members |
|---|---|---|
| `Button` | An action | `Label`, `Click`, `Colors`, `Template`, `Background`/`HoverBackground`/`PressedBackground` |
| `Checkbox` | On, off, or indeterminate | `IsChecked` (`bool?`), `IsThreeState`, `Changed`, `Click` |
| `Switch` | On or off | `IsChecked`, `Changed` |
| `RadioButton` / `RadioGroup` | One choice of several | group `Radio(...)`/`Option(value, label)`, `SelectedIndex`, `SelectedValue`, `Changed` |
| `Slider` | A value in a range | `Minimum`, `Maximum`, `Step`, `Value`, `Orientation`, `Changed` |
| `ProgressBar` | Progress | `Minimum`, `Maximum`, `Value`, `Length`, `Thickness`, `Orientation` |
| `Dropdown` | One of a fixed list | `Items` (`DropdownItem`), `SelectedIndex`, `Changed`, `MaxVisibleItems`, templates |
| `ComboBox` | Text with a filtered list | `Items`, `Text`, `FilterMode`, `CommitMode`, `AutoComplete`, `Changed`, `TextChanged` |
| `TextInput` | One line of text | `Text`, `Placeholder`, `IsPassword`, `IsReadOnly`, `HostAutofill`, `TextChanged` |
| `TextArea` | Several lines of text | `Text`, `Wrapping`, scroll bar visibility, `AcceptsTab`, `TextChanged` |
| `Form` | Enter and Escape for a page, and autofill grouping | `DefaultButton`, `CancelButton`, `Activate()` |
| `Dialog` | A modal question | `Title`, `Body`, `BodyContent`, `Show()`/`ShowAsync()`, `Accepted`, `Cancelled` |
| `ContextMenu` / `MenuItem` | A menu at a point | `Items`, `Show(target, x, y)`, `Hide()`, styling |
| `Popup` | Any content over the page | `ShowAnchored`, `ShowAtPoint`, `Hide()`, `Placement`, backdrop, `Opened`/`Closed` |
| `ToolTip` | A hint on hover or focus | `node.ToolTip("...")` or `new ToolTip(text) { ... }` |
| `NavLink` | Any content as a link | `Href`, `OpenInNewTab`, `Navigate`, `InteractionStateChanged` |
| `TabView` / `TabItem` | One tab's content at a time | `Items`, `SelectedIndex`, `SelectionChanged`; no tab strip of its own |
| `SelectionArea` / `AntiSelectionArea` | Text selection across nodes | `SelectedText`, `SelectedTextChanged`, static `ClearCurrentSelection()` |

`Button`, `Checkbox`, `RadioButton` and `Switch` take `FontFamily`,
`FontWeight`, `FontStyle`, `FontSize` and a text color for their label, which
survive theme changes. `NavLink` is the click target and its content only a
label: style that content from `InteractionStateChanged`.

## Nodes

| Node | Purpose | Key members |
|---|---|---|
| `FlexBox` | The layout box | `FlexDirection`, `JustifyContent`, `AlignItems`, `Padding`, `Margin`, `Background`, `Border`, `CornerRadius`, `DropShadow`, `ClipToBounds` |
| `Grid` | Rows and columns, as WPF's | `Columns`/`Rows` of `GridLength` (`Pixels`, `Star`, `Auto`), `Place`, shared size groups |
| `Text` | Selectable text | `Text`, font members, `TextColor`, `MaxLines`, `Wrapping`, `Overflow` |
| `TextCore` | Text without default selection | the same members; editors and labels build on it |
| `RichText` / `Span` | Styled runs in one text | `new RichText(spans)`, `SetSpans`, `AddSpan`; `new Span("text").Bold().Color(...)` |
| `Image` / `Svg` | Loaded images | `Image.Load(url)`, `Svg.Load(url, tint)`, `ObjectFit`, `Sampling`, `AssetStateChanged` |
| `ScrollView` | The native scroll viewport | `ScrollTo`, `ScrollToAnimated`, `ScrollChanged`, offsets and extent |
| `ScrollBox` | A scroll view with scroll bars | visibility per axis, `ScrollBarGutter`, `VerticalScrollBar` styling |
| `VirtualList` | A long list of pooled rows | `new VirtualList(count, itemHeight).OnBindItem(...)`, `UpdateItemCount` |
| `Portal` | A subtree drawn above the page | holds popups and menus |
| `CustomDrawable` | Immediate drawing | override `OnRender(DrawContext)`, `InvalidateVisual()` |
| `TextLayout` / `DynamicTextLayout` | Text outside the tree | measuring, drawing; `OnReady` |
| `Bitmap` | Pixels you write or draw | `Pixels`, `Canvas`, `Commit()`, `Render(node)` |

## Core layout concept

A `FlexBox` lays out its children in a column by default
(`FlexDirection.Column`); `FlexBox.Row(...)` and `FlexBox.Column(...)` build a
box with children in one call.

Size a node in one of two ways:

- **Fixed**: `Width(200.0f)`, `Height(50.0f, Unit.Percent)`. A percentage is of
  the parent's size; `Length.Auto` sizes to content.
- **Fill**: `FillWidth()`, `FillHeight()`, `FillSize()` take the space the parent
  offers along the main axis, or the full cross size. `FillWidthPercent(50)`
  takes a share of the remaining space instead.

Setting one replaces the other. `MinWidth`, `MaxWidth`, `MinHeight` and
`MaxHeight` bound either. In a row, prefer `FillWidth()` for "take the rest"
and fixed widths for the siblings; `Width(100, Unit.Percent)` in a row makes a
child as wide as the whole row, leaving its siblings no room.

A `FlexBox` does not clip its children, as CSS's `overflow: visible`: content
and shadows may extend past it. Set `ClipToBounds` where overflow must be hidden.
Scroll views always clip.

`PositionAbsolute()` takes a box out of the flow; `Position(left, top)` places
it relative to its parent.

## Composition helpers

- `FlexBox.Row(...)`, `FlexBox.Column(...)`
- `.Child(node)`, `.Children(...)` on any `IContentContainer` (`FlexBox`,
  `ScrollView`, `ScrollBox`)
- `Grid` attached placement: `node.GridRow(1).GridColumn(2)` or
  `node.GridCell(row, column, rowSpan, columnSpan)`, or
  `grid.PlaceChild(node, row, column, rowSpan, columnSpan)`
- `RetainedView` for a subtree created once and activated, deactivated and
  reused, as `TabView`'s content is
