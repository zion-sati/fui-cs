using FuiCs;
using FuiCs.Demo.Home.Dashboard;

namespace FuiCs.Demo.Home;

/// <summary>
/// The dashboard route: a pooled list beside a scrolling column of samples (layout and theme, input, focus scopes,
/// common controls, media, nested scrolling and an input event inspector), following the shell's clock, accent and
/// dark mode.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.CreateManaged(static () => new DashboardController());
}
