using FuiCs;

namespace FuiCs.Browser;

/// <summary>Grid tracks, cells, spans, shared size groups and live updates, laid out by the host.</summary>
internal static class GridPage
{
    public static Node Build()
    {
        var moving = Cell("g-a", LayoutPage.Red).GridCell(0, 0);
        var grid = new Grid()
            .NodeId("grid-main")
            .Width(400.0f)
            .Height(200.0f)
            .Background(LayoutPage.Grey)
            .Columns(100.0f, GridLength.Star(), GridLength.Star(2.0f))
            .Rows(50.0f, GridLength.Star(), 30.0f)
            .Children(
                moving,
                Cell("g-b", LayoutPage.Blue).GridCell(0, 2),
                Cell("g-span", LayoutPage.Green).GridCell(1, 0, columnSpan: 2),
                Cell("g-last", LayoutPage.Red).GridCell(2, 1));

        var move = Button("move-button", LayoutPage.Blue);
        move.OnPointerClick(_ => Grid.SetColumn(moving, 1));
        var tracks = Button("tracks-button", LayoutPage.Green);
        tracks.OnPointerClick(_ => grid.Columns = [200.0f, GridLength.Star(), GridLength.Star()]);

        var shared = FlexBox.Column(
                SharedRow("s1", "Name", "label"),
                SharedRow("s2", "Email address", "label"))
            .NodeId("shared-scope")
            .GridSharedSizeScope();
        var unshared = SharedRow("s3", "Name", null);

        var autoTracks = new Grid()
            .NodeId("auto-grid")
            .Columns(GridLength.Auto, 100.0f)
            .Rows(GridLength.Auto, GridLength.Auto)
            .Children(
                new Text("Auto label").FontSize(20.0f).NodeId("auto-text").GridCell(0, 0),
                new FlexBox().NodeId("auto-text-value").Background(LayoutPage.Blue).GridCell(0, 1),
                new FlexBox().NodeId("auto-sized").Width(180.0f).Height(30.0f).Background(LayoutPage.Red).GridCell(1, 0),
                new FlexBox().NodeId("auto-sized-value").Background(LayoutPage.Green).GridCell(1, 1));

        return FlexBox.Column(
                grid,
                FlexBox.Row(move, new FlexBox().Width(10.0f), tracks).Margin(0.0f, 10.0f, 0.0f, 10.0f),
                shared,
                unshared,
                autoTracks.Margin(0.0f, 10.0f, 0.0f, 0.0f))
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Cell(string id, Color color) => new FlexBox().NodeId(id).Background(color);

    private static FlexBox Button(string id, Color color) =>
        new FlexBox().NodeId(id).Width(60.0f).Height(30.0f).Background(color).Cursor(CursorStyle.Pointer);

    /// <summary>A two-column grid whose auto label column is sized with the other grids in <paramref name="group"/>.</summary>
    private static Grid SharedRow(string prefix, string label, string? group)
    {
        var grid = new Grid()
            .NodeId(prefix + "-grid")
            .Columns(GridLength.Auto, 100.0f)
            .Rows(28.0f);
        if (group is not null)
        {
            grid.SetColumnSharedSizeGroup(0, group);
        }
        return grid.Children(
                new Text(label).FontSize(18.0f).NodeId(prefix + "-label").GridCell(0, 0),
                new FlexBox().NodeId(prefix + "-value").Background(LayoutPage.Blue).GridCell(0, 1));
    }
}
