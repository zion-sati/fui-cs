using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// The built-in context menu over the page and over selectable text, an app menu shown from a node's
/// ContextMenuOpening, and a region without menus. Status lines report what the app menu ran.
/// </summary>
internal static class ContextMenuPage
{
    public static Node Build()
    {
        var log = Status("menu-log", "none");
        var visibility = Status("menu-visible", "hidden");
        var appMenu = new ContextMenu()
            .Items(
                new MenuItem("Rename", () => log.Text = "renamed") { ShortcutLabel = "F2" },
                new MenuItem("Duplicate", () => log.Text = "duplicated"),
                MenuItem.Separator,
                new MenuItem("Delete", () => log.Text = "deleted") { IsEnabled = false })
            .OnVisibilityChanged(e => visibility.Text = e.Visible ? "shown" : "hidden");

        var file = new FlexBox()
            .NodeId("file")
            .Width(160.0f)
            .Height(40.0f)
            .Background(LayoutPage.Grey)
            .Child(new Text("report.pdf").FontSize(14.0f).Selectable(false))
            .OnContextMenuOpening(e => appMenu.Show(null, e.X, e.Y));

        var under = new Button("Under").NodeId("under").OnClick(_ => log.Text = "under clicked");

        var linkLog = Status("link-log", "none");
        var link = new NavLink("?page=context-menu#docs").NodeId("docs-link")
            .OnNavigate(e => linkLog.Text = "navigated " + e.Path)
            .Child(new Text("Read the docs").FontSize(16.0f).TextColor(LayoutPage.Blue));
        var image = Image.Load(ImagePage.Halves).NodeId("menu-image").Width(80.0f);
        var vector = Svg.Load(ImagePage.Block, LayoutPage.Green).NodeId("menu-svg").Height(30.0f);

        return FlexBox.Column(
                new Text("Selectable words for the menu").FontSize(16.0f).Selectable(true).NodeId("words"),
                Gap(),
                FlexBox.Row(file, Gap(), under).AlignItems(AlignItems.Center),
                Gap(),
                new FlexBox().NodeId("no-menu").Width(160.0f).Height(40.0f).Background(LayoutPage.Green).IsContextMenuEnabled(false),
                Gap(),
                FlexBox.Row(link, Gap(), image, Gap(), vector).AlignItems(AlignItems.Center),
                new FlexBox().NodeId("blank").Width(300.0f).Height(80.0f),
                log,
                linkLog,
                visibility,
                appMenu)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Gap() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id, string text) => new Text(text).FontSize(14.0f).NodeId(id);
}
