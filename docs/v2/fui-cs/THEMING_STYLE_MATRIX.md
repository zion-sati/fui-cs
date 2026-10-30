# FUI-CS Theming and Style Matrix

Every control draws its default look from the active `Theme` and follows it as
it changes. Explicit settings on a control win over the theme, one property at
a time.

## The active theme

`Theme.Active` is a `Signal<Theme>` holding the theme in use.

| Call | Effect |
|---|---|
| `Theme.UseSystem()` | Follows the host's dark mode and accent color (the default) |
| `Theme.UseCustom(theme)` | Uses `theme` and stops following the system; setting `Theme.Active.Value` does the same |
| `Theme.SetAccentColor(color)` | Generates a theme for the current mode around `color` |
| `Theme.Generate(isDark, accent)` | Builds a light or dark theme around an accent, without applying it |
| `Theme.DefaultLight`, `Theme.DefaultDark` | The default themes with the default blue accent |
| `Theme.Bind(handler)` | Runs `handler` with the theme now and on every change, until disposed |

`Theme.IsDarkMode` and `Theme.IsUsingSystemTheme` report the current state.
`Application.Register(app => app.Page(BuildPage).UseSystemTheme())` applies
the system theme at startup.

A theme is an immutable record; derive a variant with `with`:

```csharp
var dark = Theme.Generate(isDark: true, Color.FromRgb(0x10, 0xb9, 0x81));
Theme.UseCustom(dark with { Colors = dark.Colors with { FocusRing = 0xF59E0BFF } });
```

Theme records compare by reference, so assigning a new theme always counts as
a change, even with the same values.

## Theme sections

| Section | Contents |
|---|---|
| `Colors` (`ThemeColors`) | `Background`, `Surface`, `TextPrimary`, `TextMuted`, `TextOnAccent`, `Accent`, `AccentHovered`, `AccentPressed`, `Border`, `Selection`, `ScrollbarTrack`, `ScrollbarThumb`, `DialogBackdrop`, `DialogShadow`, `PanelShadow`, `FocusRing` |
| `Spacing` (`ThemeSpacing`) | `Xs`, `Sm`, `Md`, `Lg`, `Xl` (4, 8, 16, 24 and 32 by default) |
| `Fonts` (`ThemeFonts`) | `BodyStack`, `HeadingStack`, `MonoStack`, `MonoBoldStack`; `BodyFamily`, `HeadingFamily`, `MonoFamily`; `SizeBody`, `SizeHeading`, `SizeMono` |
| `ContextMenu` (`ContextMenuTheme`) | The menu panel's fill, border, corner radius, shadow and separator, and `Item` (`ContextMenuItemTheme`): fill, hover fill, text color, font, height, padding, corners |
| `ToolTip` (`ToolTipTheme`) | The tooltip panel's fill, border, corners, shadow, text color, font, padding and `MaxWidth` |

## What each control takes from the theme

| Control | Theme values | Overrides |
|---|---|---|
| Application background | `Colors.Background` | the root's `Background` |
| `Text`, `RichText`, `TextCore` | `TextPrimary`, `Fonts.BodyFamily`, `SizeBody`; selection `Colors.Selection` | `TextColor`, `FontFamily`, `FontSize`, span styles |
| `Button` | `Accent`, `AccentHovered`, `AccentPressed`, `TextOnAccent`, `Border`; `Fonts.BodyFamily`, `SizeBody`; `Spacing.Sm`/`Md` padding | `Colors` (`ButtonColors`), then `Background`, `HoverBackground`, `PressedBackground`, `TextColor`, `Padding`, font members, `Template` |
| `Checkbox`, `Switch`, `RadioButton` | indicator: `Surface`, `Border`, `Accent`, `AccentHovered`, `AccentPressed`, `TextOnAccent`, `Background`; label: `TextPrimary` (`TextMuted` disabled), body font | `LabeledControlColors`, `LabeledControlSizing`, `IndicatorTemplate`, font members |
| `Slider` | track `ScrollbarTrack`; fill and thumb `Accent` (`AccentHovered` when hovered, `AccentPressed` while dragging); thumb border `Surface`; focus border `Background` | `SliderColors`, `SliderSizing`, `SliderTemplate` |
| `ProgressBar` | track `ScrollbarTrack`, fill `Accent`, `Border` | `TrackColor`, `FillColor` |
| `TextInput`, `TextArea` | shell `Surface`, `Border`; text `TextPrimary`, placeholder and disabled `TextMuted`, caret `Accent`; body font; `Spacing.Sm`/`Md` | `TextInputColors`, `TextInputTemplate`, `FontFamily`, `FontSize`, `LineHeight` |
| `Dropdown` | field `Surface`, `Border`, `TextPrimary`, `TextMuted`, `Background`; rows `Accent`, `ContextMenu.Item.HoverBackground`; list panel `ContextMenu` panel values | `DropdownColors`, `DropdownSizing`, the field, chevron and row templates, `PopupPanelColor`, `PopupPanelBackgroundBlur` |
| `ComboBox` | as `Dropdown`, with the field's text from `TextInput` | the same, plus the field's `TextInputColors` |
| `ContextMenu`, selection toolbar | `ContextMenu` and `ContextMenu.Item`; shortcut hints `TextMuted` | the menu's styling members, or your own `ContextMenu` through `ContextMenuManager.SetMenu` |
| `ToolTip` | `ToolTip` | the tooltip's `Background` and `TextColor` |
| `Dialog` | card `Surface`, `Border`, `DialogShadow`, `Spacing.Md` corners; backdrop `DialogBackdrop`; title `HeadingFamily`, `SizeHeading`, `TextPrimary`; body `TextMuted` | `CardColor`, `CardBorder`, `CardCornerRadius`, `CardShadow`, `BackdropColor`, `BackgroundBlur` |
| `ScrollBox` bars | `ScrollbarTrack`, `ScrollbarThumb` | each bar's `TrackColor`, `ThumbColor`, thicknesses and radii |
| Focus ring | `FocusRing` | the theme |
| `NavLink` | `Spacing.Xs` gap | its content's styling |
| `Popup` | none | `PanelColor`, `PanelBorder`, `PanelShadow`, `PanelCornerRadius`, `PanelBackgroundBlur`, `BackdropColor` |

`Colors.PanelShadow` is unused by the built-in controls; it is there for an
application's own floating panels.

## Precedence

For each visual property, the first set value wins:

1. the control's own property (`Background`, `TextColor`, `FontSize`, ...);
2. the control's colors or sizing record (`ButtonColors`, `DropdownColors`,
   `LabeledControlColors`, `SliderSizing`, ...), member by member;
3. the active theme.

A null property or record member means "not set", so assigning null returns
it to the next level. Templates receive the theme, the state and the colors
record, and decide the rest; see
[Control customization](./CONTROL_CUSTOMIZATION.md).

## Dark mode in the browser

Under `Theme.UseSystem()`, the host reports the browser's
`prefers-color-scheme` and accent color, and the theme follows them live. A
page that wants a fixed look calls `Theme.UseCustom` once at startup.
