namespace FuiCs.Demo.DesignSystem;

/// <summary>
/// The demo's route paths. The demo is served at its source paths under <c>/v2/fui-cs/demo/</c> or published at a
/// site's root; each path follows the current route, so links stay inside whichever layout the page came from.
/// </summary>
public static class DemoRoutes
{
    private const string SourceBase = "/v2/fui-cs/demo";

    public static string Home => Route("/");

    public static string Workbench => Route("/workbench/");

    public static string ImmediateDrawing => Route("/immediate-drawing/");

    public static string AdvancedControls => Route("/advanced-controls/");

    public static string TemplatedControls => Route("/templated-controls/");

    public static string ScrollbarGutter => Route("/scrollbar-gutter/");

    /// <summary>
    /// A font the demo's routes share, from the demo site's <c>fonts/</c> folder: the runtime's fonts, and DejaVu Sans,
    /// which the demo ships in <c>assets/fonts/</c> with its license.
    /// </summary>
    public static string SharedFontUrl(string assetFile)
    {
        ArgumentNullException.ThrowIfNull(assetFile);
        return Route("/fonts/" + assetFile);
    }

    /// <summary>The route at <paramref name="publishedPath"/> (from the site root) in the current layout.</summary>
    public static string Route(string publishedPath)
    {
        ArgumentNullException.ThrowIfNull(publishedPath);
        return IsSourceLayout() ? SourceBase + publishedPath : publishedPath;
    }

    private static bool IsSourceLayout()
    {
        var route = Navigation.CurrentRoute.Value;
        return route.Length == 0 || route.StartsWith(SourceBase, StringComparison.Ordinal);
    }
}
