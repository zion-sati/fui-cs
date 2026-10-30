namespace FuiCs.Demo.DesignSystem;

/// <summary>The demo's colors, each derived from a theme: surfaces tinted toward the accent or the page, and text.</summary>
public static class DemoColors
{
    public static Color SectionBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Background, theme, 0.22f, 0.08f);

    public static Color HeaderBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.22f, 0.08f);

    public static Color PanelBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Background, theme, 0.12f, 0.03f);

    /// <summary>A translucent panel for popups, which blur what lies behind them.</summary>
    public static Color PopupPanelBackground(Theme theme) =>
        WithAlpha(Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.16f, 0.05f), 0x99);

    public static Color CardBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.10f, 0.03f);

    public static Color CardBackgroundAlt(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.18f, 0.08f);

    public static Color PrimaryText(Theme theme) => theme.Colors.TextPrimary;

    public static Color MutedText(Theme theme) => theme.Colors.TextMuted;

    public static Color SubtleText(Theme theme) => Mix(theme.Colors.TextMuted, theme.Colors.Surface, theme, 0.0f, 0.10f);

    public static Color DividerColor(Theme theme) => theme.Colors.Border;

    public static Color SurfaceBorder(Theme theme) => Mix(theme.Colors.Border, theme.Colors.Surface, theme, 0.0f, 0.18f);

    public static Color SurfaceShadow(Theme theme) =>
        IsDark(theme) ? Color.FromRgba(0x00, 0x00, 0x00, 0x58) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x18);

    public static Color StrongSurfaceShadow(Theme theme) =>
        IsDark(theme) ? Color.FromRgba(0x00, 0x00, 0x00, 0x68) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x20);

    public static Color KeyTargetIdle(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Background, theme, 0.18f, 0.06f);

    public static Color KeyTargetFocused(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.40f, 0.16f);

    public static Color ButtonPrimaryBackground(Theme theme) => Mix(theme.Colors.Accent, theme.Colors.Surface, theme, 0.04f, 0.10f);

    public static Color ButtonPrimaryHover(Theme theme) => Mix(theme.Colors.AccentHovered, theme.Colors.Surface, theme, 0.02f, 0.08f);

    public static Color ButtonPrimaryPressed(Theme theme) => Mix(theme.Colors.AccentPressed, theme.Colors.Surface, theme, 0.0f, 0.06f);

    public static Color ButtonSecondaryBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Background, theme, 0.26f, 0.06f);

    public static Color ButtonSecondaryHover(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.16f, 0.08f);

    public static Color ButtonSecondaryPressed(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Accent, theme, 0.24f, 0.12f);

    public static Color ScrollBarTrackColor(Theme theme) => theme.Colors.Background;

    public static Color ScrollBarThumbColor(Theme theme) => ButtonPrimaryBackground(theme);

    public static Color FieldBackground(Theme theme) => Mix(theme.Colors.Surface, theme.Colors.Background, theme, 0.28f, 0.05f);

    public static Color FieldBorder(Theme theme) => Mix(theme.Colors.Border, theme.Colors.Accent, theme, 0.16f, 0.08f);

    public static Color PrimaryButtonBorder(Theme theme) => Mix(theme.Colors.Accent, theme.Colors.Surface, theme, 0.28f, 0.46f);

    public static Color ProgressTrack(Theme theme) => SectionBackground(theme);

    public static Color ProgressFill(Theme theme) => ButtonPrimaryBackground(theme);

    /// <summary>Whether the theme's page is dark: its background's luminance is below half.</summary>
    public static bool IsDark(Theme theme)
    {
        var background = theme.Colors.Background;
        return (background.R * 0.2126f) + (background.G * 0.7152f) + (background.B * 0.0722f) < 128.0f;
    }

    internal static Color WithAlpha(Color color, byte alpha) => Color.FromRgba(color.R, color.G, color.B, alpha);

    private static Color Mix(Color from, Color to, Theme theme, float darkAmount, float lightAmount) =>
        from.Mix(to, IsDark(theme) ? darkAmount : lightAmount);
}
