namespace FuiCs;

/// <summary>The data a drag source offers, keyed by format. Plain text uses <see cref="TextFormat"/>.</summary>
public sealed class DragDataObject
{
    /// <summary>The format of plain text.</summary>
    public const string TextFormat = "text/plain";

    private readonly Dictionary<string, string> formats = [];

    /// <summary>Stores plain text.</summary>
    public DragDataObject SetText(string value) => SetFormat(TextFormat, value);

    /// <summary>Stores <paramref name="value"/> under <paramref name="format"/>.</summary>
    public DragDataObject SetFormat(string format, string value)
    {
        formats[format] = value;
        return this;
    }

    /// <summary>Whether data is stored under <paramref name="format"/>.</summary>
    public bool HasFormat(string format) => formats.ContainsKey(format);

    /// <summary>The plain text, if any.</summary>
    public string? GetText() => GetFormat(TextFormat);

    /// <summary>The data stored under <paramref name="format"/>, if any.</summary>
    public string? GetFormat(string format) => formats.TryGetValue(format, out var value) ? value : null;
}

/// <summary>A drop target's answer to a drag: the effect it would apply, and whether to show an insertion marker.</summary>
public readonly record struct DropProposal(DragDropEffects Effect = DragDropEffects.None, bool ShowInsertionMarker = false)
{
    /// <summary>Refuses the drop.</summary>
    public static DropProposal None => default;
}

/// <summary>A drag in progress from <see cref="Source"/>.</summary>
public sealed class DragSession(Node source, DragDataObject data, DragDropEffects allowedEffects)
{
    /// <summary>The node the drag started from.</summary>
    public Node Source { get; } = source;

    /// <summary>The dragged data.</summary>
    public DragDataObject Data { get; } = data;

    /// <summary>The effects the source allows.</summary>
    public DragDropEffects AllowedEffects { get; } = allowedEffects;

    /// <summary>The effect the target under the pointer accepts.</summary>
    public DragDropEffects CurrentEffect { get; private set; }

    /// <summary>Whether the drag is still in progress.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Raised when the drag ends.</summary>
    public event Action<DragCompletedEventArgs>? Completed;

    /// <summary>Adds a handler for the drag ending.</summary>
    public DragSession OnCompleted(Action<DragCompletedEventArgs> handler)
    {
        Completed += handler;
        return this;
    }

    /// <summary>Cancels the drag.</summary>
    public void Cancel()
    {
        if (!IsActive)
        {
            return;
        }
        EventRouter.CancelDragSession(this);
    }

    internal void SetCurrentEffect(DragDropEffects effect) => CurrentEffect = effect;

    internal void Complete(DragDropEffects effect)
    {
        if (!IsActive)
        {
            return;
        }
        IsActive = false;
        CurrentEffect = effect;
        Completed?.Invoke(new DragCompletedEventArgs(effect));
    }
}

/// <summary>A drag entering, moving over, leaving or dropping on a node.</summary>
public sealed class DragEventArgs(DragSession session, float x, float y, KeyModifier modifiers) : EventArgs
{
    /// <summary>The drag.</summary>
    public DragSession Session { get; } = session;

    /// <summary>The pointer, relative to the node.</summary>
    public float X { get; } = x;

    /// <summary>The pointer, relative to the node.</summary>
    public float Y { get; } = y;

    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;

    /// <summary>Set by drag-enter and drag-over handlers to accept the drop. Starts as <see cref="DropProposal.None"/> for each.</summary>
    public DropProposal Proposal { get; set; }
}
