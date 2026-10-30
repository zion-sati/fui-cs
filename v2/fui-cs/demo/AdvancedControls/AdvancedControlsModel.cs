using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls;

/// <summary>The route's mutable state: the running worker and its progress, and the animation showcase's targets.</summary>
internal sealed class AdvancedControlsModel
{
    public Worker? ActiveWorker { get; set; }

    public int WorkerProgressPercent { get; set; }

    public string WorkerStateLabel { get; set; } = "idle";

    public bool AnimationPreviewEmphasized { get; set; }

    public string AnimationScrollTargetLabel { get; set; } = "top";

    public static RoutePageModel CreateRoutePageModel() => new(
        "Advanced controls",
        "Explore advanced controls and interaction-heavy samples on a dedicated route.",
        "Advanced samples",
        Color.FromRgb(16, 185, 129),
        "Ping advanced controls",
        "Advanced actions",
        [
            new RoutePageNavItem(DemoRoutes.Home, "Dashboard"),
            new RoutePageNavItem(DemoRoutes.AdvancedControls, "Advanced controls"),
            new RoutePageNavItem(DemoRoutes.TemplatedControls, "Templated controls"),
        ],
        [
            "Explore TextArea behavior with live wrapping, read-only, scrollbar, line-height, and visibility controls.",
            "The animation showcase demonstrates typed bgColor/opacity transitions plus retained smooth scrolling and explicit scrollContentSize(...) on the same route.",
            "The drag-and-drop reorder sample demonstrates insertion markers, edge autoscroll inside ScrollBox, and retained row reordering on top of the phase-3 session APIs.",
            "The external file-drop sample proves metadata-first browser drag targeting ahead of the later first-class file bridge.",
            "The Fetch sample talks to the live JSONPlaceholder service through the shipped Fetch API and surfaces its completion metadata without browser-specific code.",
            "The worker sample pairs a ProgressBar with cooperative background work and cancellation.",
            "Switch the TextArea between variable and mono theme fonts to compare text behavior live.",
            "Load app-authored fonts with FontStack.Load(...) and keep emoji fallback in the same sample.",
            "Routing keeps navigation and browser history consistent while this page owns its own app instance.",
        ]);
}
