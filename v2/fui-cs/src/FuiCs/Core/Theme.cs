namespace FuiCs;

/// <summary>The theme's palette.</summary>
public sealed record ThemeColors
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ThemeColors? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    /// <summary>The page behind everything.</summary>
    public required Color Background { get; init; }
    /// <summary>Cards, panels and fields.</summary>
    public required Color Surface { get; init; }
    /// <summary>Body text.</summary>
    public required Color TextPrimary { get; init; }
    /// <summary>Secondary and disabled text.</summary>
    public required Color TextMuted { get; init; }
    /// <summary>Text on an accent fill.</summary>
    public required Color TextOnAccent { get; init; }
    /// <summary>The accent: primary buttons, checked controls, focus and selection highlights.</summary>
    public required Color Accent { get; init; }
    /// <summary>The accent while pressed.</summary>
    public required Color AccentPressed { get; init; }
    /// <summary>The accent while hovered.</summary>
    public required Color AccentHovered { get; init; }
    /// <summary>Field and panel borders.</summary>
    public required Color Border { get; init; }
    /// <summary>The text selection highlight.</summary>
    public required Color Selection { get; init; }
    /// <summary>The scroll bar's track.</summary>
    public required Color ScrollbarTrack { get; init; }
    /// <summary>The scroll bar's thumb.</summary>
    public required Color ScrollbarThumb { get; init; }
    /// <summary>The veil over the page behind a modal dialog.</summary>
    public required Color DialogBackdrop { get; init; }
    /// <summary>A dialog card's shadow.</summary>
    public required Color DialogShadow { get; init; }
    /// <summary>A floating panel's shadow.</summary>
    public required Color PanelShadow { get; init; }
    /// <summary>The keyboard focus ring.</summary>
    public required Color FocusRing { get; init; }
}

/// <summary>The theme's spacing steps, extra small to extra large, in pixels.</summary>
public sealed record ThemeSpacing(float Xs, float Sm, float Md, float Lg, float Xl)
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ThemeSpacing? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}

/// <summary>The theme's font stacks and families; families default to regular/bold pairs of the stacks.</summary>
public sealed record ThemeFonts
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ThemeFonts? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    /// <summary>The body text's stack.</summary>
    public required FontStack BodyStack { get; init; }
    /// <summary>The headings' stack.</summary>
    public required FontStack HeadingStack { get; init; }
    /// <summary>The monospace stack.</summary>
    public required FontStack MonoStack { get; init; }
    /// <summary>The bold monospace stack.</summary>
    public required FontStack MonoBoldStack { get; init; }
    /// <summary>The body text's family.</summary>
    public required FontFamily BodyFamily { get; init; }
    /// <summary>The headings' family.</summary>
    public required FontFamily HeadingFamily { get; init; }
    /// <summary>The monospace family.</summary>
    public required FontFamily MonoFamily { get; init; }
    /// <summary>The body text's size in pixels.</summary>
    public required float SizeBody { get; init; }
    /// <summary>The headings' size in pixels.</summary>
    public required float SizeHeading { get; init; }
    /// <summary>The monospace text's size in pixels.</summary>
    public required float SizeMono { get; init; }
}

/// <summary>How a context menu draws its items.</summary>
public sealed record ContextMenuItemTheme
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ContextMenuItemTheme? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    /// <summary>An item's fill.</summary>
    public required Color Background { get; init; }
    /// <summary>An item's fill under the pointer.</summary>
    public required Color HoverBackground { get; init; }
    /// <summary>An item's text.</summary>
    public required Color TextColor { get; init; }
    /// <summary>An item's corner radius.</summary>
    public required float CornerRadius { get; init; }
    /// <summary>An item's font family.</summary>
    public required FontFamily FontFamily { get; init; }
    /// <summary>An item's font size in pixels.</summary>
    public required float FontSize { get; init; }
    /// <summary>An item's height in pixels.</summary>
    public required float Height { get; init; }
    /// <summary>An item's padding.</summary>
    public required Thickness Padding { get; init; }
}

/// <summary>How a context menu's panel looks.</summary>
public sealed record ContextMenuTheme
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ContextMenuTheme? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    /// <summary>The panel's fill.</summary>
    public required Color PanelBackground { get; init; }
    /// <summary>The panel's border.</summary>
    public required Color PanelBorderColor { get; init; }
    /// <summary>The panel's shadow color.</summary>
    public required Color PanelShadowColor { get; init; }
    /// <summary>The panel's corner radius.</summary>
    public required float PanelCornerRadius { get; init; }
    /// <summary>The divider between groups.</summary>
    public required Color SeparatorColor { get; init; }
    /// <summary>The shadow's downward offset.</summary>
    public required float ShadowOffsetY { get; init; }
    /// <summary>The shadow's blur radius.</summary>
    public required float ShadowBlur { get; init; }
    /// <summary>The shadow's spread.</summary>
    public required float ShadowSpread { get; init; }
    /// <summary>The items' style.</summary>
    public required ContextMenuItemTheme Item { get; init; }
}

/// <summary>How a tooltip looks.</summary>
public sealed record ToolTipTheme
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(ToolTipTheme? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    /// <summary>The tooltip's fill.</summary>
    public required Color PanelBackground { get; init; }
    /// <summary>The tooltip's border.</summary>
    public required Color PanelBorderColor { get; init; }
    /// <summary>The tooltip's shadow color.</summary>
    public required Color PanelShadowColor { get; init; }
    /// <summary>The tooltip's corner radius.</summary>
    public required float PanelCornerRadius { get; init; }
    /// <summary>The tooltip's text.</summary>
    public required Color TextColor { get; init; }
    /// <summary>The tooltip's font family.</summary>
    public required FontFamily FontFamily { get; init; }
    /// <summary>The tooltip's font size in pixels.</summary>
    public required float FontSize { get; init; }
    /// <summary>The widest a tooltip grows before its text wraps.</summary>
    public required float MaxWidth { get; init; }
    /// <summary>The tooltip's padding.</summary>
    public required Thickness Padding { get; init; }
    /// <summary>The shadow's downward offset.</summary>
    public required float ShadowOffsetY { get; init; }
    /// <summary>The shadow's blur radius.</summary>
    public required float ShadowBlur { get; init; }
    /// <summary>The shadow's spread.</summary>
    public required float ShadowSpread { get; init; }
}

/// <summary>
/// Colors, spacing, fonts and component styles. <see cref="Active"/> is the theme in use; controls follow it as it
/// changes. Derive variants with <c>with</c>, or generate one from a light/dark choice and an accent color.
/// Theme records compare by reference: assigning a new theme with the same values still counts as a change, and
/// skipping member-wise equality keeps about 30 KB of comparer code out of every application.
/// </summary>
public sealed record Theme
{
    /// <summary>Compares by reference, as every theme record does (see <see cref="Theme"/>).</summary>
    public bool Equals(Theme? other) => ReferenceEquals(this, other);

    /// <summary>The reference's hash, matching the reference equality.</summary>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

    private static readonly Color DefaultAccent = Color.FromRgb(0x25, 0x63, 0xeb);
    private static readonly Color White = Color.FromRgb(0xff, 0xff, 0xff);
    private static readonly Color Black = Color.FromRgb(0x00, 0x00, 0x00);
    private static readonly ThemeSpacing DefaultSpacing = new(4.0f, 8.0f, 16.0f, 24.0f, 32.0f);
    private static readonly ThemeFonts DefaultFonts = CreateDefaultFonts();

    private static bool usingSystemTheme = true;
    private static bool systemDarkMode = true;
    private static Color systemAccent = DefaultAccent;

    /// <summary>The palette.</summary>
    public required ThemeColors Colors { get; init; }
    /// <summary>The spacing steps.</summary>
    public required ThemeSpacing Spacing { get; init; }
    /// <summary>The fonts and sizes.</summary>
    public required ThemeFonts Fonts { get; init; }
    /// <summary>The context menu's style.</summary>
    public required ContextMenuTheme ContextMenu { get; init; }
    /// <summary>The tooltip's style.</summary>
    public required ToolTipTheme ToolTip { get; init; }

    /// <summary>The default light theme with the default accent.</summary>
    public static Theme DefaultLight { get; } = Generate(isDark: false);

    /// <summary>The default dark theme with the default accent.</summary>
    public static Theme DefaultDark { get; } = Generate(isDark: true);

    /// <summary>The theme in use. Setting it directly is the same as <see cref="UseCustom"/>.</summary>
    public static Signal<Theme> Active { get; } = new(DefaultDark);

    /// <summary>
    /// Whether the active theme is dark: the system's choice while following it, otherwise the custom theme's.
    /// </summary>
    public static bool IsDarkMode { get; private set; } = true;

    /// <summary>Whether the active theme follows the system's light or dark mode and accent color.</summary>
    public static bool IsUsingSystemTheme => usingSystemTheme;

    /// <summary>Runs <paramref name="handler"/> with the active theme now and whenever it changes, until disposed.</summary>
    public static IDisposable Bind(Action<Theme> handler)
    {
        var subscription = Active.Subscribe(handler);
        handler(Active.Value);
        return subscription;
    }

    /// <summary>Follows the host's dark mode and accent color.</summary>
    public static Theme UseSystem()
    {
        systemDarkMode = Runtime.Abi.HostIsDarkMode();
        systemAccent = NormalizeAccent(Runtime.Abi.HostGetAccentColor());
        return ApplySystem();
    }

    /// <summary>Makes <paramref name="theme"/> the active theme and stops following the system; returns it.</summary>
    public static Theme UseCustom(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        return Apply(theme, system: false, EstimateDark(theme));
    }

    /// <summary>Uses a theme generated from the current light/dark mode and <paramref name="accent"/>.</summary>
    public static Theme SetAccentColor(Color accent) => UseCustom(Generate(IsDarkMode, accent));

    /// <summary>
    /// A theme for light or dark mode around <paramref name="accentColor"/> (the default blue when null).
    /// </summary>
    public static Theme Generate(bool isDark, Color? accentColor = null)
    {
        var accent = NormalizeAccent(accentColor ?? DefaultAccent);
        var background = isDark ? Color.FromRgb(0x04, 0x0a, 0x14) : Color.FromRgb(0xf8, 0xfa, 0xfc);
        var surface = isDark ? Color.FromRgb(0x0f, 0x17, 0x28) : Color.FromRgb(0xff, 0xff, 0xff);
        var textPrimary = isDark ? Color.FromRgb(0xf8, 0xfa, 0xfc) : Color.FromRgb(0x0f, 0x17, 0x2a);
        var dialogShadow = isDark ? Color.FromRgba(0x00, 0x00, 0x00, 0xd8) : Color.FromRgba(0x00, 0x00, 0x00, 0x88);
        var panelShadow = WithAlpha(dialogShadow, (byte)MathF.Floor((dialogShadow.A * 0.30f) + 0.5f));
        return new Theme
        {
            Colors = new ThemeColors
            {
                Background = background,
                Surface = surface,
                TextPrimary = textPrimary,
                TextMuted = isDark ? Color.FromRgb(0x94, 0xa3, 0xb8) : Color.FromRgb(0x47, 0x55, 0x69),
                TextOnAccent = PickAccentForeground(accent),
                Accent = accent,
                AccentPressed = accent.Mix(Black, isDark ? 0.24f : 0.16f),
                AccentHovered = accent.Mix(White, isDark ? 0.14f : 0.10f),
                Border = isDark ? Color.FromRgb(0x24, 0x3b, 0x53) : Color.FromRgb(0xcb, 0xd5, 0xe1),
                Selection = WithAlpha(accent, isDark ? (byte)0x40 : (byte)0x33),
                ScrollbarTrack = isDark ? Color.FromRgb(0x12, 0x21, 0x33) : Color.FromRgb(0xe2, 0xe8, 0xf0),
                ScrollbarThumb = accent.Mix(surface, isDark ? 0.55f : 0.40f),
                DialogBackdrop = isDark ? Color.FromRgba(0x00, 0x00, 0x00, 0x24) : Color.FromRgba(0x00, 0x00, 0x00, 0x18),
                DialogShadow = dialogShadow,
                PanelShadow = panelShadow,
                FocusRing = accent,
            },
            Spacing = DefaultSpacing,
            Fonts = DefaultFonts,
            ContextMenu = new ContextMenuTheme
            {
                PanelBackground = isDark ? Color.FromRgba(0x18, 0x1d, 0x26, 0xd8) : Color.FromRgba(0xff, 0xff, 0xff, 0xdc),
                PanelBorderColor = isDark ? Color.FromRgba(0xff, 0xff, 0xff, 0x10) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x14),
                PanelShadowColor = panelShadow,
                PanelCornerRadius = isDark ? 16.0f : 14.0f,
                SeparatorColor = isDark ? Color.FromRgba(0xff, 0xff, 0xff, 0x10) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x12),
                ShadowOffsetY = 12.0f,
                ShadowBlur = 28.0f,
                ShadowSpread = 0.0f,
                Item = new ContextMenuItemTheme
                {
                    Background = Color.Transparent,
                    HoverBackground = isDark ? Color.FromRgba(0xff, 0xff, 0xff, 0x0c) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x08),
                    TextColor = textPrimary,
                    CornerRadius = isDark ? 10.0f : 9.0f,
                    FontFamily = DefaultFonts.BodyFamily,
                    FontSize = 13.0f,
                    Height = 30.0f,
                    Padding = new Thickness(12.0f, 6.0f),
                },
            },
            ToolTip = new ToolTipTheme
            {
                PanelBackground = isDark ? Color.FromRgba(0x11, 0x17, 0x20, 0xf0) : Color.FromRgba(0xff, 0xff, 0xff, 0xf8),
                PanelBorderColor = isDark ? Color.FromRgba(0xff, 0xff, 0xff, 0x12) : Color.FromRgba(0x0f, 0x17, 0x2a, 0x12),
                PanelShadowColor = panelShadow,
                PanelCornerRadius = isDark ? 12.0f : 10.0f,
                TextColor = textPrimary,
                FontFamily = DefaultFonts.BodyFamily,
                FontSize = 13.0f,
                MaxWidth = 280.0f,
                Padding = new Thickness(10.0f, 7.0f),
                ShadowOffsetY = 10.0f,
                ShadowBlur = 24.0f,
                ShadowSpread = 0.0f,
            },
        };
    }

    internal static Theme HandleSystemDarkModeChanged(bool isDark)
    {
        systemDarkMode = isDark;
        if (!usingSystemTheme)
        {
            return Active.Value;
        }
        systemAccent = NormalizeAccent(Runtime.Abi.HostGetAccentColor());
        return ApplySystem();
    }

    internal static Theme HandleSystemAccentColorChanged(Color accent)
    {
        systemAccent = NormalizeAccent(accent);
        return usingSystemTheme ? ApplySystem() : Active.Value;
    }

    internal static void Reset()
    {
        usingSystemTheme = true;
        systemDarkMode = true;
        systemAccent = DefaultAccent;
        IsDarkMode = true;
        Active.Reset(DefaultDark);
    }

    private static Theme ApplySystem() => Apply(Generate(systemDarkMode, systemAccent), system: true, systemDarkMode);

    private static Theme Apply(Theme theme, bool system, bool isDark)
    {
        usingSystemTheme = system;
        IsDarkMode = isDark;
        Active.Value = theme;
        return theme;
    }

    /// <summary>A zero accent means the default; a fully transparent one is made opaque.</summary>
    private static Color NormalizeAccent(Color accent)
    {
        if (accent.Rgba == 0)
        {
            return DefaultAccent;
        }
        return accent.A == 0 ? WithAlpha(accent, 0xff) : accent;
    }

    private static Color WithAlpha(Color color, byte alpha) => Color.FromRgba(color.R, color.G, color.B, alpha);

    private static float Luminance(Color color) => (color.R * 0.2126f) + (color.G * 0.7152f) + (color.B * 0.0722f);

    private static Color PickAccentForeground(Color accent) => Luminance(accent) < 160.0f ? White : Black;

    private static bool EstimateDark(Theme theme) => Luminance(theme.Colors.Background) < 128.0f;

    private static ThemeFonts CreateDefaultFonts()
    {
        var body = FontStack.FromId(1).AddFallback(3);
        var heading = FontStack.FromId(2).AddFallback(3);
        var mono = FontStack.FromId(7).AddFallback(4).AddFallback(3);
        var monoBold = FontStack.FromId(8).AddFallback(4).AddFallback(3);
        return new ThemeFonts
        {
            BodyStack = body,
            HeadingStack = heading,
            MonoStack = mono,
            MonoBoldStack = monoBold,
            BodyFamily = FontFamily.FromIds(1, 2, 5, 6),
            HeadingFamily = FontFamily.RegularBold(heading, heading),
            MonoFamily = FontFamily.RegularBold(mono, monoBold),
            SizeBody = 16.0f,
            SizeHeading = 24.0f,
            SizeMono = 15.0f,
        };
    }
}
