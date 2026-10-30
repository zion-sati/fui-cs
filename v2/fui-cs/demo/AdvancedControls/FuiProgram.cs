using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls;

/// <summary>
/// The advanced controls route: transitions and smooth scrolling, drag-and-drop reorder, a background worker, online
/// fetch, rich text, app-authored fonts and auto sizing, on a route page that follows the shell's dark mode and accent.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.CreateManaged(static () =>
    {
        var controller = new AdvancedControlsController();
        return new RoutePageController(AdvancedControlsModel.CreateRoutePageModel(), controller.BuildSections(), new DemoShellAdapter(), controller);
    });
}
