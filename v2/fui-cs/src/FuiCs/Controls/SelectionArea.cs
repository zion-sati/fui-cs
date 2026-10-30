using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A region whose text the user can select across nodes with one drag, as one selection. Text inside it is
/// selectable unless it sets its own selection behavior, sits under a node with its own cursor, or is inside an
/// <see cref="AntiSelectionArea"/>. <see cref="SelectedText"/> reports what is selected.
/// </summary>
public class SelectionArea : FlexBox
{
    /// <summary>The text selected across the area; empty when nothing is.</summary>
    public string SelectedText { get; private set; } = string.Empty;

    /// <summary>Raised when the selection across the area changes.</summary>
    public event Action<TextChangedEventArgs>? SelectedTextChanged;

    /// <summary>Clears the page's text selection, whether it spans a selection area or sits in one text node.</summary>
    public static void ClearCurrentSelection() => Bindings.Ui.ClearCurrentSelection();

    /// <inheritdoc/>
    public override void AddContent(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        PrepareSelectionDefaults(node, ancestorOwnsCursor: false);
        base.AddContent(node);
    }

    /// <inheritdoc/>
    public override void SetContent(params ReadOnlySpan<Node> nodes)
    {
        foreach (var node in nodes)
        {
            PrepareSelectionDefaults(node, ancestorOwnsCursor: false);
        }
        base.SetContent(nodes);
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        foreach (var child in Children)
        {
            PrepareSelectionDefaults(child, ancestorOwnsCursor: false);
        }
        var handle = base.Build();
        Ui.SetSelectionArea(handle, true);
        return handle;
    }

    /// <inheritdoc/>
    protected internal override void HandleCrossSelectionChanged(string text)
    {
        base.HandleCrossSelectionChanged(text);
        SelectedText = text;
        SelectedTextChanged?.Invoke(new TextChangedEventArgs(text));
    }

    /// <summary>Makes default text selectable, except under a node that sets its own cursor or a selection barrier.</summary>
    private static void PrepareSelectionDefaults(Node node, bool ancestorOwnsCursor)
    {
        if (node is Text text)
        {
            if (!ancestorOwnsCursor && text.Cursor == CursorStyle.Default && text.UsesDefaultSelectionBehavior)
            {
                text.Selectable = true;
            }
            return;
        }
        if (node.IsSelectionBarrier)
        {
            return;
        }
        var childOwnsCursor = ancestorOwnsCursor || node.Cursor != CursorStyle.Default;
        foreach (var child in node.Children)
        {
            PrepareSelectionDefaults(child, childOwnsCursor);
        }
    }
}

/// <summary>
/// Keeps an enclosing <see cref="SelectionArea"/> from selecting the text inside it. A <see cref="SelectionArea"/>
/// nested inside starts its own independent selection.
/// </summary>
public class AntiSelectionArea : FlexBox
{
    /// <inheritdoc/>
    public override bool IsSelectionBarrier => true;

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        var handle = base.Build();
        Ui.SetSelectionAreaBarrier(handle, true);
        return handle;
    }
}
