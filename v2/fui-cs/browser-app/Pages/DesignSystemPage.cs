using System;
using System.Collections.Generic;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Browser;

/// <summary>
/// A route page from the demo design system: navigation links, the badge, title and status, the action button, the
/// notes card and a section with demo controls, in the demo theme from a fixed shell.
/// </summary>
internal static class DesignSystemPage
{
    public static Node Build()
    {
        var model = new RoutePageModel(
            "Design system",
            "The demo's route page, surfaces, text, buttons, links and scroll bars, as every demo route builds on them.",
            "Design showcase",
            Color.FromRgb(0x25, 0x63, 0xeb),
            "Record ping",
            "Pings",
            [new RoutePageNavItem("/v2/fui-cs/", "Home"), new RoutePageNavItem("/v2/fui-cs/index.html", "Test page")],
            ["Surfaces, text and controls follow the shell's dark mode and accent.", "The page scrolls inside its own scroll box."]);
        var controls = FlexBox.Column(
                FlexBox.Row(
                        new DemoButton("Primary", DemoButtonTone.Primary).NodeId("ds-primary"),
                        new FlexBox().Width(12.0f),
                        new DemoButton("Secondary").NodeId("ds-secondary"))
                    .AlignItems(AlignItems.Center),
                new FlexBox().Height(14.0f),
                new DemoProgressBar(60.0f).Length(320.0f).NodeId("ds-progress"),
                new FlexBox().Height(14.0f),
                new DemoSurface(DemoSurfaceRecipe.ListRow).NodeId("ds-row").Height(56.0f).FillWidth(),
                new FlexBox().Height(8.0f),
                new DemoSurface(DemoSurfaceRecipe.ListRowAlt).NodeId("ds-row-alt").Height(56.0f).FillWidth())
            .FillWidth();
        var section = RoutePageSection.Create("Controls", "Demo buttons, a progress bar and list rows.", controls);
        var filler = RoutePageSection.Create("More", "A tall section, so the page scrolls.", new FlexBox().Height(900.0f).FillWidth());
        return new RoutePageController(model, [section, filler], new FixedShell()).GetRoot();
    }

    /// <summary>A light shell with a green accent that never changes.</summary>
    private sealed class FixedShell : IDemoShell
    {
        public event Action<uint>? AccentColorChanged
        {
            add { }
            remove { }
        }

        public event Action<bool>? DarkModeChanged
        {
            add { }
            remove { }
        }

        public string AccentColorHex() => "#16a34a";

        public bool IsDarkMode() => false;
    }
}
