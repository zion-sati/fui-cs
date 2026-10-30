using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Demo.DesignSystem;
using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DesignSystemTests
{
    private static readonly Color Green = Color.FromRgb(0x16, 0xa3, 0x4a);

    [Test]
    public async Task TheDemoThemeTintsTheGeneratedThemeAndUsesRoomierSpacingAndType()
    {
        foreach (var isDark in new[] { false, true })
        {
            var baseTheme = Theme.Generate(isDark, Green);
            var theme = DemoTheme.Generate(isDark, Green);

            await Assert.That(theme.Colors.Background).IsEqualTo(baseTheme.Colors.Background.Mix(Green, isDark ? 0.04f : 0.01f));
            await Assert.That(theme.Colors.Surface).IsEqualTo(baseTheme.Colors.Surface.Mix(Green, isDark ? 0.05f : 0.02f));
            await Assert.That(theme.Colors.ScrollbarThumb).IsEqualTo(DemoColors.ButtonPrimaryBackground(baseTheme));
            await Assert.That(theme.Colors.TextPrimary).IsEqualTo(baseTheme.Colors.TextPrimary);
            await Assert.That((theme.Spacing.Xs, theme.Spacing.Sm, theme.Spacing.Xl)).IsEqualTo((6.0f, 10.0f, 34.0f));
            await Assert.That((theme.Fonts.SizeBody, theme.Fonts.SizeHeading, theme.Fonts.SizeMono)).IsEqualTo((16.0f, 26.0f, 15.0f));
            await Assert.That(theme.ContextMenu.PanelCornerRadius).IsEqualTo(isDark ? 18.0f : 16.0f);
            await Assert.That((theme.ContextMenu.Item.Height, theme.ContextMenu.Item.Padding)).IsEqualTo((36.0f, new Thickness(14.0f, 8.0f)));
            await Assert.That(theme.ToolTip.MaxWidth).IsEqualTo(320.0f);
            await Assert.That(theme.ContextMenu.PanelBackground.A).IsEqualTo((byte)0x99);
            await Assert.That(DemoColors.IsDark(theme)).IsEqualTo(isDark);
        }
    }

    [Test]
    public async Task SurfaceRecipesResolveTheirCornersBordersMarginsAndShadows()
    {
        var theme = DemoTheme.Generate(false, Green);

        var shell = DemoSurfaces.Resolve(theme, DemoSurfaceRecipe.PageShell);
        var panel = DemoSurfaces.Resolve(theme, DemoSurfaceRecipe.SectionPanel);
        var badge = DemoSurfaces.Resolve(theme, DemoSurfaceRecipe.AccentBadge);
        var row = DemoSurfaces.Resolve(theme, DemoSurfaceRecipe.ListRowAlt);

        await Assert.That((shell.Radius, shell.Shadow)).IsEqualTo((28.0f, new DropShadow(DemoColors.SurfaceShadow(theme), 0.0f, 4.0f, 14.0f)));
        await Assert.That((panel.Radius, panel.Margin, panel.Border)).IsEqualTo((22.0f, 12.0f, new Border(1.0f, DemoColors.SurfaceBorder(theme))));
        await Assert.That((badge.Background, badge.Radius, badge.Border.Width)).IsEqualTo((theme.Colors.Accent, 999.0f, 0.0f));
        await Assert.That((row.Background, row.Shadow)).IsEqualTo((DemoColors.CardBackgroundAlt(theme), default(DropShadow)));
        await Assert.That(DemoSurfaces.IsStateful(DemoSurfaceRecipe.KeyTargetFocused)).IsTrue();
        await Assert.That(DemoSurfaces.IsStateful(DemoSurfaceRecipe.Divider)).IsFalse();
    }

    [Test]
    public async Task DemoSurfacesAndTextFollowTheThemeAndTheirRecipes()
    {
        TestRuntime.Start();
        var light = Theme.UseCustom(DemoTheme.Generate(false, Green));
        var surface = new DemoSurface(DemoSurfaceRecipe.ListRow);
        var text = new DemoText("Notes", DemoTextRecipe.Hint);
        var lightColors = (surface.Background, text.TextColor);

        var dark = Theme.UseCustom(DemoTheme.Generate(true, Green));
        var darkColors = (surface.Background, text.TextColor);
        surface.Recipe = DemoSurfaceRecipe.AccentSwatch;
        text.Recipe(DemoTextRecipe.Accent);

        await Assert.That(lightColors).IsEqualTo((DemoColors.CardBackground(light), (Color?)DemoColors.SubtleText(light)));
        await Assert.That(darkColors).IsEqualTo((DemoColors.CardBackground(dark), (Color?)DemoColors.SubtleText(dark)));
        await Assert.That((surface.Background, text.TextColor)).IsEqualTo((dark.Colors.Accent, (Color?)dark.Colors.Accent));
        await Assert.That(DemoTextRecipes.ColorOf(dark, DemoTextRecipe.InverseLabel)).IsEqualTo(Color.FromRgb(0xf8, 0xfb, 0xff));
        await Assert.That(DemoTextRecipes.ColorOf(dark, DemoTextRecipe.StatusSupporting)).IsEqualTo(dark.Colors.TextMuted);
    }

    [Test]
    public async Task DemoButtonsTakeTheirToneAndDemoNavLinksLightUpWhenActive()
    {
        TestRuntime.Start();
        var theme = Theme.UseCustom(DemoTheme.Generate(false, Green));
        var button = new DemoButton("Go", DemoButtonTone.Primary);
        var primary = (button.Background, button.HoverBackground, button.PressedBackground, button.TextColor);
        button.Tone(DemoButtonTone.Secondary);
        var link = new DemoNavLink("/v2/fui-cs/demo/", "Dashboard");
        var inactive = (link.Background, link.Label.TextColor);
        link.IsActive(true);

        await Assert.That(primary).IsEqualTo((
            DemoColors.ButtonPrimaryBackground(theme),
            (Color?)DemoColors.ButtonPrimaryHover(theme),
            (Color?)DemoColors.ButtonPrimaryPressed(theme),
            (Color?)Color.FromRgb(0xf8, 0xfb, 0xff)));
        await Assert.That((button.Background, button.Border)).IsEqualTo((DemoColors.ButtonSecondaryBackground(theme), new Border(1.0f, DemoColors.SurfaceBorder(theme))));
        await Assert.That((button.CornerRadius, button.Padding, button.FontSize)).IsEqualTo((new CornerRadius(18.0f), new Thickness(18.0f, 12.0f), (float?)16.0f));
        await Assert.That(inactive).IsEqualTo((DemoColors.ButtonSecondaryBackground(theme), (Color?)theme.Colors.TextPrimary));
        await Assert.That((link.Background, link.Border.Color, link.SemanticLabel)).IsEqualTo((theme.Colors.Accent, theme.Colors.AccentHovered, (string?)"Dashboard"));
    }

    [Test]
    public async Task ARoutePageFollowsTheShellsThemeTheRouteAndItsActionCount()
    {
        TestRuntime.Start();
        var shell = new FakeShell { AccentHex = "#16a34a", IsDark = true };
        var model = new RoutePageModel(
            "Workbench",
            "A page.",
            "Badge",
            Color.FromRgb(0x25, 0x63, 0xeb),
            "Act",
            "Actions",
            [new RoutePageNavItem("/v2/fui-cs/demo/", "Dashboard"), new RoutePageNavItem("/v2/fui-cs/demo/workbench/", "Workbench")],
            ["One note."]);
        var section = RoutePageSection.Create("Section", "About it.", new FlexBox());
        var page = new RoutePageController(model, [section], shell);
        var darkBackground = Theme.Active.Value.Colors.Background;

        Navigation.CurrentRoute.Value = "/v2/fui-cs/demo/workbench/";
        page.View.ActionButton.PerformClick();
        page.View.ActionButton.PerformClick();
        shell.RaiseDarkMode(false);
        var lightBackground = Theme.Active.Value.Colors.Background;
        shell.RaiseAccent(0xE11D48FFu);
        var accent = Theme.Active.Value.Colors.Accent;
        var labels = Labels(page.View.Root);
        var activeLinks = ActiveLinks(page.View.Root);
        page.Dispose();

        await Assert.That(darkBackground).IsEqualTo(DemoTheme.Generate(true, Green).Colors.Background);
        await Assert.That(lightBackground).IsEqualTo(DemoTheme.Generate(false, Green).Colors.Background);
        await Assert.That(accent).IsEqualTo((Color)0xE11D48FFu);
        await Assert.That(page.ActionCount).IsEqualTo(2);
        await Assert.That(labels.Contains("Actions: 2", StringComparison.Ordinal)).IsTrue();
        await Assert.That(labels.Contains("Current route: /v2/fui-cs/demo/workbench/", StringComparison.Ordinal)).IsTrue();
        await Assert.That(labels.Contains("|Section|About it.", StringComparison.Ordinal)).IsTrue();
        await Assert.That(activeLinks).IsEqualTo("Workbench");
        await Assert.That(shell.HasHandlers).IsFalse();
    }

    [Test]
    public async Task TheShellsAccentParsesOnlyAsRgbHex()
    {
        var fallback = Color.FromRgb(1, 2, 3);

        await Assert.That(RoutePageController.ParseAccentColor("#16A34a", fallback)).IsEqualTo(Green);
        await Assert.That(RoutePageController.ParseAccentColor("#16a34", fallback)).IsEqualTo(fallback);
        await Assert.That(RoutePageController.ParseAccentColor("16a34aa", fallback)).IsEqualTo(fallback);
        await Assert.That(RoutePageController.ParseAccentColor("#16a3g4", fallback)).IsEqualTo(fallback);
        await Assert.That(RoutePageController.ParseAccentColor(null, fallback)).IsEqualTo(fallback);
    }

    [Test]
    public async Task RoutePathsFollowTheLayoutTheDemoIsServedIn()
    {
        TestRuntime.Start();
        var beforeRoute = DemoRoutes.Workbench;
        Navigation.CurrentRoute.Value = "/v2/fui-cs/demo/immediate-drawing/";
        var source = (DemoRoutes.Home, DemoRoutes.Workbench, DemoRoutes.ImmediateDrawing);
        Navigation.CurrentRoute.Value = "/immediate-drawing/";
        var published = (DemoRoutes.Home, DemoRoutes.Workbench, DemoRoutes.Route("/templated-controls/"));
        Navigation.CurrentRoute.Value = "";

        await Assert.That(beforeRoute).IsEqualTo("/v2/fui-cs/demo/workbench/");
        await Assert.That(source).IsEqualTo(("/v2/fui-cs/demo/", "/v2/fui-cs/demo/workbench/", "/v2/fui-cs/demo/immediate-drawing/"));
        await Assert.That(published).IsEqualTo(("/", "/workbench/", "/templated-controls/"));
    }

    /// <summary>The texts under <paramref name="root"/>, joined by "|".</summary>
    private static string Labels(Node root)
    {
        var labels = new List<string>();
        Visit(root, node =>
        {
            if (node is Text text)
            {
                labels.Add(text.Text);
            }
        });
        return string.Join("|", labels);
    }

    private static string ActiveLinks(Node root)
    {
        var active = new List<string>();
        Visit(root, node =>
        {
            if (node is DemoNavLink { IsActive: true } link)
            {
                active.Add(link.LabelText);
            }
        });
        return string.Join("|", active);
    }

    private static void Visit(Node node, Action<Node> visit)
    {
        visit(node);
        foreach (var child in node.Children)
        {
            Visit(child, visit);
        }
    }

    private sealed class FakeShell : IDemoShell
    {
        public string AccentHex { get; set; } = "";

        public bool IsDark { get; set; }

        public bool HasHandlers => AccentColorChanged is not null || DarkModeChanged is not null;

        public event Action<uint>? AccentColorChanged;

        public event Action<bool>? DarkModeChanged;

        public string AccentColorHex() => AccentHex;

        public bool IsDarkMode() => IsDark;

        public void RaiseDarkMode(bool isDark) => DarkModeChanged?.Invoke(isDark);

        public void RaiseAccent(uint rgba) => AccentColorChanged?.Invoke(rgba);
    }
}
