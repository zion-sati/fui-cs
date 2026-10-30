namespace FuiCs.Demo.DesignSystem;

/// <summary>The demo's theme: the generated theme for the mode and accent, with roomier spacing, larger type and demo-tinted surfaces and popups.</summary>
public static class DemoTheme
{
    public static Theme Generate(bool isDark, Color accent)
    {
        var baseTheme = Theme.Generate(isDark, accent);
        var colors = baseTheme.Colors;
        var fonts = baseTheme.Fonts with { SizeBody = 16.0f, SizeHeading = 26.0f, SizeMono = 15.0f };
        return new Theme
        {
            Colors = colors with
            {
                Background = colors.Background.Mix(colors.Accent, isDark ? 0.04f : 0.01f),
                Surface = colors.Surface.Mix(colors.Accent, isDark ? 0.05f : 0.02f),
                Border = colors.Border.Mix(colors.Accent, isDark ? 0.10f : 0.05f),
                ScrollbarTrack = DemoColors.SectionBackground(baseTheme),
                ScrollbarThumb = DemoColors.ButtonPrimaryBackground(baseTheme),
                DialogShadow = DemoColors.StrongSurfaceShadow(baseTheme),
                PanelShadow = DemoColors.SurfaceShadow(baseTheme),
            },
            Spacing = new ThemeSpacing(6.0f, 10.0f, 16.0f, 24.0f, 34.0f),
            Fonts = fonts,
            ContextMenu = new ContextMenuTheme
            {
                PanelBackground = DemoColors.PopupPanelBackground(baseTheme),
                PanelBorderColor = DemoColors.SurfaceBorder(baseTheme),
                PanelShadowColor = DemoColors.StrongSurfaceShadow(baseTheme),
                PanelCornerRadius = isDark ? 18.0f : 16.0f,
                SeparatorColor = DemoColors.DividerColor(baseTheme),
                ShadowOffsetY = 12.0f,
                ShadowBlur = 30.0f,
                ShadowSpread = 0.0f,
                Item = new ContextMenuItemTheme
                {
                    Background = Color.Transparent,
                    HoverBackground = DemoColors.SectionBackground(baseTheme),
                    TextColor = DemoColors.PrimaryText(baseTheme),
                    CornerRadius = isDark ? 12.0f : 10.0f,
                    FontFamily = fonts.BodyFamily,
                    FontSize = 14.0f,
                    Height = 36.0f,
                    Padding = new Thickness(14.0f, 8.0f),
                },
            },
            ToolTip = new ToolTipTheme
            {
                PanelBackground = DemoColors.PopupPanelBackground(baseTheme),
                PanelBorderColor = DemoColors.SurfaceBorder(baseTheme),
                PanelShadowColor = DemoColors.StrongSurfaceShadow(baseTheme),
                PanelCornerRadius = isDark ? 14.0f : 12.0f,
                TextColor = DemoColors.PrimaryText(baseTheme),
                FontFamily = fonts.BodyFamily,
                FontSize = 14.0f,
                MaxWidth = 320.0f,
                Padding = new Thickness(12.0f, 8.0f),
                ShadowOffsetY = 12.0f,
                ShadowBlur = 28.0f,
                ShadowSpread = 0.0f,
            },
        };
    }
}
