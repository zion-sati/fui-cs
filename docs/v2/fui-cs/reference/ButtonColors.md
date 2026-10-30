# ButtonColors

record in `FuiCs`

Colors a [`Button`](Button.md) uses instead of the theme's; unset colors follow the theme.

## Properties

- `Background`: The fill at rest.
- `BackgroundHover`: The background while hovered; falls back to [`ButtonColors.Background`](ButtonColors.md).
- `BackgroundPressed`: The background while pressed; falls back to [`ButtonColors.BackgroundHover`](ButtonColors.md), then [`ButtonColors.Background`](ButtonColors.md).
- `Border`: The border's color.
- `TextMuted`: The label color while disabled.
- `TextPrimary`: The label's color.
