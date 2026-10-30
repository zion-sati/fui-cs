# Theme

record in `FuiCs`

Colors, spacing, fonts and component styles. [`Theme.Active`](Theme.md) is the theme in use; controls follow it as it changes. Derive variants with `with`, or generate one from a light/dark choice and an accent color. Theme records compare by reference: assigning a new theme with the same values still counts as a change, and skipping member-wise equality keeps about 30 KB of comparer code out of every application.

## Properties

- `Active`: The theme in use. Setting it directly is the same as [`Theme.UseCustom`](Theme.md).
- `Colors`: The palette.
- `ContextMenu`: The context menu's style.
- `DefaultDark`: The default dark theme with the default accent.
- `DefaultLight`: The default light theme with the default accent.
- `Fonts`: The fonts and sizes.
- `IsDarkMode`: Whether the active theme is dark: the system's choice while following it, otherwise the custom theme's.
- `IsUsingSystemTheme`: Whether the active theme follows the system's light or dark mode and accent color.
- `Spacing`: The spacing steps.
- `ToolTip`: The tooltip's style.

## Methods

- `Bind(Action<Theme>)`: Runs `handler` with the active theme now and whenever it changes, until disposed.
- `Equals(Theme)`: Compares by reference, as every theme record does (see [`Theme`](Theme.md)).
- `Generate(bool, Color?)`: A theme for light or dark mode around `accentColor` (the default blue when null).
- `GetHashCode()`: The reference's hash, matching the reference equality.
- `NormalizeAccent(Color)`: A zero accent means the default; a fully transparent one is made opaque.
- `SetAccentColor(Color)`: Uses a theme generated from the current light/dark mode and `accent`.
- `UseCustom(Theme)`: Makes `theme` the active theme and stops following the system; returns it.
- `UseSystem()`: Follows the host's dark mode and accent color.
