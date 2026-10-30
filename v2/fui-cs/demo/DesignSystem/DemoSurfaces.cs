namespace FuiCs.Demo.DesignSystem;

/// <summary>The demo's surface roles.</summary>
public enum DemoSurfaceRecipe
{
    PageShell,
    SectionPanel,
    InsetPanel,
    FlatFrame,
    ListRow,
    ListRowAlt,
    CalloutInset,
    AccentBadge,
    AccentSwatch,
    Divider,
    KeyTargetIdle,
    KeyTargetFocused,
}

/// <summary>A surface's fill, corners, border, margin and shadow.</summary>
public sealed record DemoSurfaceStyle(Color Background, float Radius, Border Border, float Margin, DropShadow Shadow);

/// <summary>Resolves and applies the demo's surface recipes.</summary>
public static class DemoSurfaces
{
    private const float BorderlessRadius = 999.0f;

    public static DemoSurfaceStyle Resolve(Theme theme, DemoSurfaceRecipe recipe) => recipe switch
    {
        DemoSurfaceRecipe.PageShell => Style(
            theme,
            theme.Colors.Background,
            DemoTokens.SurfaceRadiusLarge,
            shadow: new DropShadow(DemoColors.SurfaceShadow(theme), 0.0f, 4.0f, 14.0f)),
        DemoSurfaceRecipe.SectionPanel => Style(
            theme,
            theme.Colors.Background,
            DemoTokens.SurfaceRadiusMedium,
            margin: DemoTokens.SurfacePanelMargin,
            shadow: new DropShadow(DemoColors.SurfaceShadow(theme), 0.0f, 2.0f, 10.0f)),
        DemoSurfaceRecipe.InsetPanel => Style(theme, theme.Colors.Background, DemoTokens.SurfaceRadiusSmall),
        DemoSurfaceRecipe.FlatFrame => Style(theme, theme.Colors.Background, 0.0f),
        DemoSurfaceRecipe.ListRow => Style(theme, DemoColors.CardBackground(theme), DemoTokens.SurfaceRadiusSmall),
        DemoSurfaceRecipe.ListRowAlt or DemoSurfaceRecipe.CalloutInset =>
            Style(theme, DemoColors.CardBackgroundAlt(theme), DemoTokens.SurfaceRadiusSmall),
        DemoSurfaceRecipe.AccentBadge => Style(theme, theme.Colors.Accent, BorderlessRadius, new Border(0.0f, Color.Transparent)),
        DemoSurfaceRecipe.AccentSwatch => Style(theme, theme.Colors.Accent),
        DemoSurfaceRecipe.Divider => Style(theme, DemoColors.SectionBackground(theme), BorderlessRadius, new Border(0.0f, Color.Transparent)),
        DemoSurfaceRecipe.KeyTargetIdle => Style(theme, DemoColors.KeyTargetIdle(theme), DemoTokens.SurfaceRadiusSmall),
        DemoSurfaceRecipe.KeyTargetFocused => Style(theme, DemoColors.KeyTargetFocused(theme), DemoTokens.SurfaceRadiusSmall),
        _ => Style(theme),
    };

    /// <summary>Whether the recipe is one of a pair a node switches between with its state.</summary>
    public static bool IsStateful(DemoSurfaceRecipe recipe) => recipe >= DemoSurfaceRecipe.KeyTargetIdle;

    public static Color ColorOf(Theme theme, DemoSurfaceRecipe recipe) => Resolve(theme, recipe).Background;

    public static void Apply(FlexBox surface, Theme theme, DemoSurfaceRecipe recipe = DemoSurfaceRecipe.SectionPanel)
    {
        ArgumentNullException.ThrowIfNull(surface);
        var style = Resolve(theme, recipe);
        surface.Background = style.Background;
        surface.CornerRadius = new CornerRadius(style.Radius);
        surface.Border = style.Border;
        surface.Margin = new Thickness(style.Margin);
        surface.DropShadow = style.Shadow;
    }

    private static DemoSurfaceStyle Style(
        Theme theme,
        Color? background = null,
        float radius = DemoTokens.SurfaceRadiusMedium,
        Border? border = null,
        float margin = 0.0f,
        DropShadow shadow = default) =>
        new(
            background ?? theme.Colors.Background,
            radius,
            border ?? new Border(DemoTokens.SurfaceBorderWidth, DemoColors.SurfaceBorder(theme)),
            margin,
            shadow);
}

/// <summary>A flex box styled by one of the demo's surface recipes, restyled when the theme changes.</summary>
public class DemoSurface : FlexBox
{
    private DemoSurfaceRecipe recipe;

    public DemoSurface(DemoSurfaceRecipe recipe = DemoSurfaceRecipe.SectionPanel)
    {
        this.recipe = recipe;
        Track(Theme.Bind(ApplyDemoTheme));
    }

    public DemoSurfaceRecipe Recipe
    {
        get => recipe;
        set
        {
            recipe = value;
            ApplyDemoTheme(Theme.Active.Value);
        }
    }

    private void ApplyDemoTheme(Theme theme) => DemoSurfaces.Apply(this, theme, recipe);
}

/// <summary>Fluent configuration for <see cref="DemoSurface"/>.</summary>
public static class DemoSurfaceExtensions
{
    extension<TSurface>(TSurface surface) where TSurface : DemoSurface
    {
        public TSurface Recipe(DemoSurfaceRecipe recipe)
        {
            surface.Recipe = recipe;
            return surface;
        }
    }
}
