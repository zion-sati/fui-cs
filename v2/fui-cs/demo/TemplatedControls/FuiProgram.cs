using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.TemplatedControls;

/// <summary>
/// The templated controls route: a route page of controls dressed by explicit house templates, a local override, and
/// per-instance colors, following the shell's dark mode and accent.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.CreateManaged(static () =>
    {
        var controller = new TemplatedControlsController();
        return new RoutePageController(TemplatedControlsModel.Create(), controller.BuildSections(), new DemoShellAdapter(), controller);
    });
}
