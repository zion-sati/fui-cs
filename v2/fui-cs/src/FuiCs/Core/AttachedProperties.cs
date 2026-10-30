using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A child's cell in its parent grid; a new cell is the first row and column, spanning one of each.</summary>
public readonly record struct GridCell(int Row, int Column, int RowSpan = 1, int ColumnSpan = 1)
{
    /// <summary>The first cell, spanning one row and one column.</summary>
    public GridCell()
        : this(0, 0)
    {
    }
}

/// <summary>Per-node attached properties, applied when the node is built.</summary>
internal static class AttachedProperties
{
    private static uint nextAttachedPropertyKey = 1;
    private static readonly HashSet<uint> gridSharedSizeScopeKeys = [];
    private static readonly Dictionary<uint, GridCell> gridCells = [];

    public static uint AllocateKey()
    {
        var key = nextAttachedPropertyKey;
        nextAttachedPropertyKey += 1;
        return key;
    }

    /// <summary>Returns whether the value changed.</summary>
    public static bool SetGridSharedSizeScope(uint key, bool enabled) =>
        enabled ? gridSharedSizeScopeKeys.Add(key) : gridSharedSizeScopeKeys.Remove(key);

    public static bool IsGridSharedSizeScope(uint key) => gridSharedSizeScopeKeys.Contains(key);

    public static GridCell GetGridCell(uint key) => gridCells.TryGetValue(key, out var cell) ? cell : new GridCell();

    public static void SetGridCell(uint key, GridCell cell) => gridCells[key] = cell;

    public static void Apply(uint key, ulong handle)
    {
        if (gridSharedSizeScopeKeys.Contains(key))
        {
            Ui.SetIsSharedSizeScope(handle, true);
        }
        if (gridCells.TryGetValue(key, out var cell))
        {
            SendGridCell(handle, cell);
        }
    }

    public static void SendGridCell(ulong handle, GridCell cell) =>
        Ui.SetGridPlacement(handle, (uint)cell.Row, (uint)cell.Column, (uint)cell.RowSpan, (uint)cell.ColumnSpan);
}
