using FuiCs.Bindings;

namespace FuiCs;

/// <summary>The size of a grid row or column: pixels, a share of the remaining space (star), or sized to content.</summary>
public readonly record struct GridLength(float Value, GridUnit Unit = GridUnit.Pixel)
{
    /// <summary>Sized by the cells' content.</summary>
    public static GridLength Auto => new(0.0f, GridUnit.Auto);

    /// <summary>A fixed size in pixels.</summary>
    public static GridLength Pixels(float value) => new(value, GridUnit.Pixel);

    /// <summary>A share of the remaining space, weighted against the other star tracks.</summary>
    public static GridLength Star(float weight = 1.0f) => new(weight, GridUnit.Star);

    /// <summary>A fixed size in pixels.</summary>
    public static implicit operator GridLength(float pixels) => new(pixels);
}

/// <summary>
/// A grid container. Its children sit in cells of <see cref="Columns"/> and <see cref="Rows"/>, chosen per child with
/// the attached <see cref="SetRow"/>, <see cref="SetColumn"/>, <see cref="SetRowSpan"/> and <see cref="SetColumnSpan"/>;
/// a child without a cell takes the first one. Tracks, groups and cells apply immediately, before or after build.
/// </summary>
public class Grid : FlexBox
{
    private readonly List<string> columnSharedSizeGroups = [];
    private readonly List<string> rowSharedSizeGroups = [];
    private GridLength[]? columns;
    private GridLength[]? rows;

    /// <summary>The column widths.</summary>
    public IReadOnlyList<GridLength> Columns
    {
        get => columns ?? [];
        set
        {
            columns = [.. value];
            if (IsBuilt)
            {
                ApplyTracks(columns, isColumns: true);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <summary>The row heights.</summary>
    public IReadOnlyList<GridLength> Rows
    {
        get => rows ?? [];
        set
        {
            rows = [.. value];
            if (IsBuilt)
            {
                ApplyTracks(rows, isColumns: false);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <inheritdoc/>
    protected override bool UsesFlexChildLayoutDiagnostics => false;

    /// <summary>The row <paramref name="node"/> is placed in.</summary>
    public static int GetRow(Node node) => GetCell(node).Row;

    /// <summary>The column <paramref name="node"/> is placed in.</summary>
    public static int GetColumn(Node node) => GetCell(node).Column;

    /// <summary>How many rows <paramref name="node"/> spans.</summary>
    public static int GetRowSpan(Node node) => GetCell(node).RowSpan;

    /// <summary>How many columns <paramref name="node"/> spans.</summary>
    public static int GetColumnSpan(Node node) => GetCell(node).ColumnSpan;

    /// <summary>Where <paramref name="node"/> is placed.</summary>
    public static GridCell GetCell(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return AttachedProperties.GetGridCell(node.AttachedPropertyKey);
    }

    /// <summary>Places <paramref name="node"/> in <paramref name="row"/>.</summary>
    public static void SetRow(Node node, int row) => SetCell(node, GetCell(node) with { Row = row });

    /// <summary>Places <paramref name="node"/> in <paramref name="column"/>.</summary>
    public static void SetColumn(Node node, int column) => SetCell(node, GetCell(node) with { Column = column });

    /// <summary>Spans <paramref name="node"/> over <paramref name="rowSpan"/> rows.</summary>
    public static void SetRowSpan(Node node, int rowSpan) => SetCell(node, GetCell(node) with { RowSpan = rowSpan });

    /// <summary>Spans <paramref name="node"/> over <paramref name="columnSpan"/> columns.</summary>
    public static void SetColumnSpan(Node node, int columnSpan) => SetCell(node, GetCell(node) with { ColumnSpan = columnSpan });

    /// <summary>Puts <paramref name="node"/> in <paramref name="cell"/> of whichever grid it is a child of.</summary>
    public static void SetCell(Node node, GridCell cell)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentOutOfRangeException.ThrowIfNegative(cell.Row);
        ArgumentOutOfRangeException.ThrowIfNegative(cell.Column);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cell.RowSpan);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cell.ColumnSpan);
        AttachedProperties.SetGridCell(node.AttachedPropertyKey, cell);
        if (node.IsBuilt)
        {
            AttachedProperties.SendGridCell(node.Handle, cell);
            node.NotifyRetainedLayoutMutation();
        }
    }

    /// <summary>Whether <paramref name="node"/> scopes shared size groups for the grids inside it.</summary>
    public static bool GetIsSharedSizeScope(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return AttachedProperties.IsGridSharedSizeScope(node.AttachedPropertyKey);
    }

    /// <summary>Makes <paramref name="node"/> the scope within which shared-size groups line up.</summary>
    public static void SetIsSharedSizeScope(Node node, bool isScope)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!AttachedProperties.SetGridSharedSizeScope(node.AttachedPropertyKey, isScope) || !node.IsBuilt)
        {
            return;
        }
        Ui.SetIsSharedSizeScope(node.Handle, isScope);
        node.NotifyRetainedLayoutMutation();
    }

    /// <summary>Adds <paramref name="child"/> as content in the given cell.</summary>
    public void Place(Node child, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        SetCell(child, new GridCell(row, column, rowSpan, columnSpan));
        AddContent(child);
    }

    /// <summary>Puts column <paramref name="index"/> in a shared-size group; an empty name removes it from its group.</summary>
    public void SetColumnSharedSizeGroup(int index, string group) => SetSharedSizeGroup(columnSharedSizeGroups, index, group, isColumns: true);

    /// <summary>Puts row <paramref name="index"/> in a shared-size group; an empty name removes it from its group.</summary>
    public void SetRowSharedSizeGroup(int index, string group) => SetSharedSizeGroup(rowSharedSizeGroups, index, group, isColumns: false);

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        BuildStyledNode(NodeType.Grid, includeChildren: false);
        if (columns is not null)
        {
            ApplyTracks(columns, isColumns: true);
        }
        if (rows is not null)
        {
            ApplyTracks(rows, isColumns: false);
        }
        ApplySharedSizeGroups(columnSharedSizeGroups, isColumns: true);
        ApplySharedSizeGroups(rowSharedSizeGroups, isColumns: false);
        BuildChildren();
        return Handle;
    }

    private void ApplyTracks(GridLength[] tracks, bool isColumns)
    {
        var values = new float[tracks.Length];
        var units = new byte[tracks.Length];
        for (var index = 0; index < tracks.Length; ++index)
        {
            values[index] = tracks[index].Value;
            units[index] = (byte)tracks[index].Unit;
        }
        if (isColumns)
        {
            Ui.GridSetColumns(Handle, values, units);
        }
        else
        {
            Ui.GridSetRows(Handle, values, units);
        }
    }

    private void SetSharedSizeGroup(List<string> groups, int index, string group, bool isColumns)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentNullException.ThrowIfNull(group);
        if (group.Length == 0 && index >= groups.Count)
        {
            return;
        }
        while (groups.Count <= index)
        {
            groups.Add("");
        }
        groups[index] = group;
        if (IsBuilt)
        {
            SendSharedSizeGroup((uint)index, group, isColumns);
            NotifyRetainedLayoutMutation();
        }
    }

    private void ApplySharedSizeGroups(List<string> groups, bool isColumns)
    {
        for (var index = 0; index < groups.Count; ++index)
        {
            if (groups[index].Length > 0)
            {
                SendSharedSizeGroup((uint)index, groups[index], isColumns);
            }
        }
    }

    private void SendSharedSizeGroup(uint index, string group, bool isColumns)
    {
        if (isColumns)
        {
            Ui.GridSetColumnSharedSizeGroup(Handle, index, group);
        }
        else
        {
            Ui.GridSetRowSharedSizeGroup(Handle, index, group);
        }
    }
}

/// <summary>Fluent configuration for <see cref="Grid"/> and for nodes placed in a grid.</summary>
public static class GridExtensions
{
    extension<TGrid>(TGrid grid) where TGrid : Grid
    {
        /// <summary>Sets the column widths.</summary>
        public TGrid Columns(params GridLength[] columns)
        {
            grid.Columns = columns;
            return grid;
        }

        /// <summary>Sets the row heights.</summary>
        public TGrid Rows(params GridLength[] rows)
        {
            grid.Rows = rows;
            return grid;
        }

        /// <summary>Adds <paramref name="child"/> at a cell, spanning rows and columns.</summary>
        public TGrid PlaceChild(Node child, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            grid.Place(child, row, column, rowSpan, columnSpan);
            return grid;
        }

        /// <summary>
        /// Sizes column <paramref name="index"/> with the others in <paramref name="group"/> within the shared size
        /// scope.
        /// </summary>
        public TGrid ColumnSharedSizeGroup(int index, string group)
        {
            grid.SetColumnSharedSizeGroup(index, group);
            return grid;
        }

        /// <summary>Takes column <paramref name="index"/> out of its shared size group.</summary>
        public TGrid ClearColumnSharedSizeGroup(int index)
        {
            grid.SetColumnSharedSizeGroup(index, "");
            return grid;
        }

        /// <summary>
        /// Sizes row <paramref name="index"/> with the others in <paramref name="group"/> within the shared size scope.
        /// </summary>
        public TGrid RowSharedSizeGroup(int index, string group)
        {
            grid.SetRowSharedSizeGroup(index, group);
            return grid;
        }

        /// <summary>Takes row <paramref name="index"/> out of its shared size group.</summary>
        public TGrid ClearRowSharedSizeGroup(int index)
        {
            grid.SetRowSharedSizeGroup(index, "");
            return grid;
        }
    }

    extension<TNode>(TNode node) where TNode : Node
    {
        /// <summary>Puts the node in a cell of its parent grid.</summary>
        public TNode GridCell(int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            Grid.SetCell(node, new GridCell(row, column, rowSpan, columnSpan));
            return node;
        }

        /// <summary>Places the node in a grid row.</summary>
        public TNode GridRow(int row)
        {
            Grid.SetRow(node, row);
            return node;
        }

        /// <summary>Places the node in a grid column.</summary>
        public TNode GridColumn(int column)
        {
            Grid.SetColumn(node, column);
            return node;
        }

        /// <summary>Spans the node over grid rows.</summary>
        public TNode GridRowSpan(int rowSpan)
        {
            Grid.SetRowSpan(node, rowSpan);
            return node;
        }

        /// <summary>Spans the node over grid columns.</summary>
        public TNode GridColumnSpan(int columnSpan)
        {
            Grid.SetColumnSpan(node, columnSpan);
            return node;
        }

        /// <summary>Makes the node the scope within which grid shared-size groups line up.</summary>
        public TNode GridSharedSizeScope(bool isScope = true)
        {
            Grid.SetIsSharedSizeScope(node, isScope);
            return node;
        }
    }
}
