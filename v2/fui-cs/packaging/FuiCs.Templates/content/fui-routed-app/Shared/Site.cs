using FuiCs;

namespace FuiRoutedApp.Shared;

/// <summary>The site's routes, and the page layout every route shares: a title, its content and a link bar.</summary>
public static class Site
{
    /// <summary>The routes' addresses, as listed in FuiRoutedApp.csproj.</summary>
    public const string HomePath = "/";

    public const string SettingsPath = "/settings/";

    public static Node Page(string title, params Node[] content)
    {
        var page = FlexBox.Column(
                FlexBox.Row(Link("Home", HomePath), Link("Settings", SettingsPath)),
                FlexBox.Column(new Text(title).FontSize(28.0f).FontWeight(FontWeight.Bold)).Margin(0.0f, 24.0f, 0.0f, 16.0f))
            .Padding(32.0f)
            .AlignItems(AlignItems.Start);
        foreach (var node in content)
        {
            page.Child(node);
        }
        return page;
    }

    private static NavLink Link(string label, string path) =>
        new NavLink(path)
            .Child(new Text(label).FontSize(16.0f).Selectable(false))
            .Margin(0.0f, 0.0f, 16.0f, 0.0f);
}
