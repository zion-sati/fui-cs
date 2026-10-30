using System;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class GridTests
{
    [Test]
    public async Task BuildSendsTracksGroupsAndPlacements()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var cell = new TestNode();
        var grid = new Grid()
            .Columns(GridLength.Pixels(100.0f), GridLength.Star(), GridLength.Auto)
            .Rows(40.0f)
            .ColumnSharedSizeGroup(1, "labels")
            .PlaceChild(cell, 0, 1, 1, 2);

        grid.Build();

        await Assert.That(abi.Created[0].Type).IsEqualTo(NodeType.Grid);
        var columns = abi.Named("ui_grid_set_columns")[0].Arguments;
        await Assert.That(string.Join(",", (float[])columns[2]!)).IsEqualTo("100,1,0");
        await Assert.That(string.Join(",", (byte[])columns[3]!)).IsEqualTo("0,2,1");
        await Assert.That(abi.CountOf("ui_grid_set_rows")).IsEqualTo(1);
        await Assert.That(abi.HasCall("ui_grid_set_column_shared_size_group", grid.Handle, 1u, "labels")).IsTrue();
        await Assert.That(abi.HasCall("ui_node_add_child", grid.Handle, cell.Handle)).IsTrue();
        await Assert.That(abi.HasCall("ui_node_set_grid_placement", cell.Handle, 0u, 1u, 1u, 2u)).IsTrue();
    }

    [Test]
    public async Task AnUnplacedNodeSpansOneCell()
    {
        TestRuntime.Start(new LayoutAbi());
        var node = new TestNode();

        Grid.SetRow(node, 2);

        await Assert.That(Grid.GetCell(node)).IsEqualTo(new GridCell(2, 0, 1, 1));
        await Assert.That(new GridCell().ColumnSpan).IsEqualTo(1);
    }

    [Test]
    public async Task TracksAndPlacementsUpdateAfterBuild()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var grid = new Grid().Columns(GridLength.Star());
        grid.Build();
        abi.Calls.Clear();

        grid.Columns(GridLength.Star(), GridLength.Star(2.0f));
        var late = new TestNode();
        grid.PlaceChild(late, 1, 0);

        await Assert.That(abi.CountOf("ui_grid_set_columns")).IsEqualTo(1);
        await Assert.That(abi.HasCall("ui_node_add_child", grid.Handle, late.Handle)).IsTrue();
        await Assert.That(abi.HasCall("ui_node_set_grid_placement", late.Handle, 1u, 0u, 1u, 1u)).IsTrue();
        await Assert.That(abi.CountOf("ui_create_node")).IsEqualTo(1);
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(1);
    }

    [Test]
    public async Task ClearingASharedSizeGroupSendsAnEmptyName()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var grid = new Grid().RowSharedSizeGroup(0, "header");
        grid.Build();
        abi.Calls.Clear();

        grid.ClearRowSharedSizeGroup(0).ClearRowSharedSizeGroup(5);

        await Assert.That(abi.Arguments("ui_grid_set_row_shared_size_group", 2)).IsEqualTo("");
        await Assert.That(abi.CountOf("ui_grid_set_row_shared_size_group")).IsEqualTo(1);
    }

    [Test]
    public async Task TheSharedSizeScopeIsAnAttachedProperty()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scope = new FlexBox();
        Grid.SetIsSharedSizeScope(scope, true);

        scope.Build();
        var atBuild = abi.HasCall("ui_set_is_shared_size_scope", scope.Handle, true);
        abi.Calls.Clear();
        Grid.SetIsSharedSizeScope(scope, true);
        var unchangedCalls = abi.CountOf("ui_set_is_shared_size_scope");
        Grid.SetIsSharedSizeScope(scope, false);

        await Assert.That(atBuild).IsTrue();
        await Assert.That(unchangedCalls).IsEqualTo(0);
        await Assert.That(abi.HasCall("ui_set_is_shared_size_scope", scope.Handle, false)).IsTrue();
        await Assert.That(Grid.GetIsSharedSizeScope(scope)).IsFalse();
    }

    [Test]
    public async Task ChildrenWithoutACellAreBuiltIntoTheFirstCell()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var plain = new TestNode();
        var grid = new Grid().Columns(GridLength.Star()).Child(plain);

        grid.Build();

        await Assert.That(abi.HasCall("ui_node_add_child", grid.Handle, plain.Handle)).IsTrue();
        await Assert.That(abi.CountOf("ui_node_set_grid_placement")).IsEqualTo(0);
        await Assert.That(Grid.GetCell(plain) == new GridCell()).IsTrue();
    }

    [Test]
    public async Task CellsAreAttachedPropertiesThatApplyLive()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var cell = new TestNode().GridCell(0, 1, 1, 2);
        var grid = new Grid().Child(cell);
        grid.Build();
        abi.Calls.Clear();

        Grid.SetRow(cell, 2);

        await Assert.That(Grid.GetRow(cell)).IsEqualTo(2);
        await Assert.That(Grid.GetColumnSpan(cell)).IsEqualTo(2);
        await Assert.That(abi.HasCall("ui_node_set_grid_placement", cell.Handle, 2u, 1u, 1u, 2u)).IsTrue();
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(1);
    }

    [Test]
    public async Task CellsRejectNegativePositionsAndEmptySpans()
    {
        TestRuntime.Start(new LayoutAbi());
        var node = new TestNode();

        var rejected = 0;
        try
        {
            Grid.SetRow(node, -1);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected++;
        }
        try
        {
            Grid.SetColumnSpan(node, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected++;
        }

        await Assert.That(rejected).IsEqualTo(2);
        await Assert.That(Grid.GetCell(node) == new GridCell()).IsTrue();
    }

    [Test]
    public async Task GridsDoNotRunFlexLayoutWarnings()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var grid = new Grid()
            .PlaceChild(new FlexBox().Height(100.0f, Unit.Percent), 0, 0)
            .PlaceChild(new FlexBox().Height(100.0f, Unit.Percent), 1, 0);

        grid.Build();

        await Assert.That(abi.CountOf("fui_log")).IsEqualTo(0);
    }
}
