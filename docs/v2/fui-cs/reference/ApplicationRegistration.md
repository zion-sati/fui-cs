# ApplicationRegistration

class in `FuiCs`

Collects an application's settings for [`Application.Register`](Application.md).

## Methods

- `AccentColor(Color)`: Generates the theme from the current light/dark mode and `accent`.
- `Page(Func<Node>)`: Builds the page with `build`.
- `UseSystemTheme()`: Follows the host's dark mode and accent color.
- `UseTheme(Theme)`: Uses `theme` instead of the system's.
