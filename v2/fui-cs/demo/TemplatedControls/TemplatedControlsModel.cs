using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.TemplatedControls;

internal static class TemplatedControlsModel
{
    public static RoutePageModel Create() => new(
        "Templated controls",
        "Inspect explicit design-system templates, local overrides, and presenter-aware color overrides on a dedicated demo route.",
        "Template showcase",
        Color.FromRgb(244, 114, 182),
        "Record template ping",
        "Template actions",
        [
            new RoutePageNavItem(DemoRoutes.Home, "Dashboard"),
            new RoutePageNavItem(DemoRoutes.AdvancedControls, "Advanced controls"),
            new RoutePageNavItem(DemoRoutes.TemplatedControls, "Templated controls"),
        ],
        [
            "This route applies explicit house templates through its design-system controls, without mutable application-wide template state.",
            "One local override stays distinct to show that per-control templates remain independent of the shared design-system constructors.",
            "Per-instance color objects tint the same presenter-owned chrome, so apps can restyle built-ins without forking the shipped control behavior.",
            "Every stateful control owns a stable nodeId so browser-history restores still work across remounts.",
            "The page lives in the same routed demo shell as the dashboard and advanced-controls pages, so template coverage exercises real route swaps, not just inline snippets.",
        ]);
}
