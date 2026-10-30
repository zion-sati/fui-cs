namespace FuiCs.Demo.DesignSystem;

public enum DemoButtonRecipe
{
    Primary,
    Secondary,
}

public sealed record DemoButtonStyle(Color Background, Color HoverBackground, Color PressedBackground, Border Border, Color TextColor);

public enum DemoNavLinkRecipe
{
    Inactive,
    Active,
}

public sealed record DemoNavLinkStyle(Color Background, Border Border, Color TextColor);

public sealed record DemoFieldStyle(Color Background, float Radius, Border Border);

/// <summary>A dropdown's popup: a translucent panel over a blurred backdrop.</summary>
public sealed record DemoDropdownStyle(Color PopupPanelColor, float PopupPanelBackgroundBlur);

public sealed record DemoProgressStyle(Color TrackColor, Color FillColor);

public sealed record DemoContextMenuStyle(Color PanelColor, Border PanelBorder, Color ItemHoverColor, Color SeparatorColor);

public sealed record DemoDialogStyle(Color CardColor, Border CardBorder, DropShadow CardShadow);

/// <summary>The demo's recipes for the SDK's controls: the colors each takes from the theme.</summary>
public static class DemoRecipes
{
    private static readonly Color InverseLabelColor = Color.FromRgb(0xf8, 0xfb, 0xff);

    public static DemoButtonStyle Resolve(Theme theme, DemoButtonRecipe recipe) => recipe == DemoButtonRecipe.Primary
        ? new DemoButtonStyle(
            DemoColors.ButtonPrimaryBackground(theme),
            DemoColors.ButtonPrimaryHover(theme),
            DemoColors.ButtonPrimaryPressed(theme),
            new Border(DemoTokens.SurfaceBorderWidth, DemoColors.PrimaryButtonBorder(theme)),
            InverseLabelColor)
        : new DemoButtonStyle(
            DemoColors.ButtonSecondaryBackground(theme),
            DemoColors.ButtonSecondaryHover(theme),
            DemoColors.ButtonSecondaryPressed(theme),
            new Border(DemoTokens.SurfaceBorderWidth, DemoColors.SurfaceBorder(theme)),
            DemoColors.PrimaryText(theme));

    public static void Apply(Button button, Theme theme, DemoButtonRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(button);
        var style = Resolve(theme, recipe);
        button.Background = style.Background;
        button.HoverBackground = style.HoverBackground;
        button.PressedBackground = style.PressedBackground;
        button.Border = style.Border;
        button.TextColor = style.TextColor;
    }

    public static DemoNavLinkStyle Resolve(Theme theme, DemoNavLinkRecipe recipe) => recipe == DemoNavLinkRecipe.Active
        ? new DemoNavLinkStyle(theme.Colors.Accent, new Border(DemoTokens.SurfaceBorderWidth, theme.Colors.AccentHovered), InverseLabelColor)
        : new DemoNavLinkStyle(
            DemoColors.ButtonSecondaryBackground(theme),
            new Border(DemoTokens.SurfaceBorderWidth, DemoColors.SurfaceBorder(theme)),
            DemoColors.PrimaryText(theme));

    public static void Apply(NavLink link, Text label, Theme theme, DemoNavLinkRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(label);
        var style = Resolve(theme, recipe);
        link.Background = style.Background;
        link.Border = style.Border;
        label.TextColor = style.TextColor;
    }

    public static DemoFieldStyle ResolveField(Theme theme) =>
        new(DemoColors.FieldBackground(theme), DemoTokens.SurfaceRadiusSmall, new Border(DemoTokens.SurfaceBorderWidth, DemoColors.FieldBorder(theme)));

    public static DemoDropdownStyle ResolveDropdown(Theme theme) => new(DemoColors.PopupPanelBackground(theme), 12.0f);

    public static void Apply(TextInputCore input, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(input);
        var style = ResolveField(theme);
        input.Background = style.Background;
        input.CornerRadius = new CornerRadius(style.Radius);
        input.Border = style.Border;
    }

    public static void Apply(Dropdown dropdown, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(dropdown);
        var style = ResolveDropdown(theme);
        dropdown.PopupPanelColor(style.PopupPanelColor).PopupPanelBackgroundBlur(style.PopupPanelBackgroundBlur);
    }

    public static void Apply(ComboBox comboBox, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(comboBox);
        var style = ResolveDropdown(theme);
        comboBox.PopupPanelColor(style.PopupPanelColor).PopupPanelBackgroundBlur(style.PopupPanelBackgroundBlur);
    }

    public static DemoProgressStyle ResolveProgress(Theme theme) => new(DemoColors.ProgressTrack(theme), DemoColors.ProgressFill(theme));

    public static void Apply(ProgressBar progressBar, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(progressBar);
        var style = ResolveProgress(theme);
        progressBar.TrackColor(style.TrackColor).FillColor(style.FillColor);
    }

    public static DemoContextMenuStyle ResolveContextMenu(Theme theme)
    {
        var border = DemoColors.SurfaceBorder(theme);
        return new DemoContextMenuStyle(
            DemoColors.PopupPanelBackground(theme),
            new Border(DemoTokens.SurfaceBorderWidth, border),
            DemoColors.SectionBackground(theme),
            border);
    }

    public static void Apply(ContextMenu menu, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(menu);
        var style = ResolveContextMenu(theme);
        menu.PanelColor = style.PanelColor;
        menu.PanelBorder = style.PanelBorder;
        menu.ItemHoverColor = style.ItemHoverColor;
        menu.SeparatorColor = style.SeparatorColor;
    }

    public static DemoDialogStyle ResolveDialog(Theme theme) => new(
        DemoColors.PanelBackground(theme),
        new Border(DemoTokens.SurfaceBorderWidth, DemoColors.SurfaceBorder(theme)),
        new DropShadow(DemoColors.StrongSurfaceShadow(theme), 0.0f, DemoTokens.SurfaceShadowOffsetY, DemoTokens.HeaderShadowBlur));

    public static void Apply(Dialog dialog, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        var style = ResolveDialog(theme);
        dialog.CardColor = style.CardColor;
        dialog.CardBorder = style.CardBorder;
        dialog.CardShadow = style.CardShadow;
    }
}
