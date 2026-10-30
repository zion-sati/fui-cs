namespace FuiCs;

public abstract partial class Node
{
    private Func<DragDataObject?>? dragDataProvider;
    private bool allowDrop;
    private Action<DragEventArgs>? dragEnter;
    private Action<DragEventArgs>? dragOver;
    private Action<DragEventArgs>? dragLeave;
    private Action<DragEventArgs>? drop;

    /// <summary>The effects a drag from this node allows. Copy by default.</summary>
    public DragDropEffects DragAllowedEffects { get; set; } = DragDropEffects.Copy;

    /// <summary>Creates the data for a drag that starts on this node; returning null declines the drag. Setting it makes the node a drag source.</summary>
    public Func<DragDataObject?>? DragDataProvider
    {
        get => dragDataProvider;
        set
        {
            dragDataProvider = value;
            if (value is not null)
            {
                RequireInteractive();
                EnsureDragGesture();
            }
        }
    }

    /// <summary>Whether drags can drop on this node. Subscribing to a drop event turns it on.</summary>
    public bool AllowDrop
    {
        get => allowDrop;
        set
        {
            allowDrop = value;
            if (value)
            {
                RequireInteractive();
            }
        }
    }

    /// <summary>Raised on the drag source when its drag ends, with the effect the target applied.</summary>
    public event Action<DragCompletedEventArgs>? DragCompleted;

    /// <summary>A drag entered this drop target; handlers set <see cref="DragEventArgs.Proposal"/> to accept it.</summary>
    public event Action<DragEventArgs>? DragEnter
    {
        add => AddDropHandler(ref dragEnter, value);
        remove => dragEnter -= value;
    }

    /// <summary>A drag moved over this drop target; handlers set <see cref="DragEventArgs.Proposal"/> to accept it.</summary>
    public event Action<DragEventArgs>? DragOver
    {
        add => AddDropHandler(ref dragOver, value);
        remove => dragOver -= value;
    }

    /// <summary>Raised when a drag leaves the node.</summary>
    public event Action<DragEventArgs>? DragLeave
    {
        add => AddDropHandler(ref dragLeave, value);
        remove => dragLeave -= value;
    }

    /// <summary>Raised when a drag drops on the node.</summary>
    public event Action<DragEventArgs>? Drop
    {
        add => AddDropHandler(ref drop, value);
        remove => drop -= value;
    }

    internal bool HasDragSource => dragDataProvider is not null;

    internal bool HasDragEnterHandler => dragEnter is not null;

    internal bool HasDragOverHandler => dragOver is not null;

    internal DragDataObject? CreateDragDataObject() => dragDataProvider?.Invoke();

    internal void NotifyDragCompleted(DragDropEffects effect) => DragCompleted?.Invoke(new DragCompletedEventArgs(effect));

    internal DropProposal RaiseDragEnter(DragEventArgs e) => RaiseForProposal(dragEnter, e);

    internal DropProposal RaiseDragOver(DragEventArgs e) => RaiseForProposal(dragOver, e);

    internal void RaiseDragLeave(DragEventArgs e) => dragLeave?.Invoke(e);

    internal void RaiseDrop(DragEventArgs e) => drop?.Invoke(e);

    private static DropProposal RaiseForProposal(Action<DragEventArgs>? handlers, DragEventArgs e)
    {
        e.Proposal = DropProposal.None;
        handlers?.Invoke(e);
        return e.Proposal;
    }

    private void AddDropHandler(ref Action<DragEventArgs>? handlers, Action<DragEventArgs>? handler)
    {
        if (handler is null)
        {
            return;
        }
        handlers += handler;
        AllowDrop = true;
    }

    private void EnsureDragGesture()
    {
        if (dragGesture is not null)
        {
            return;
        }
        var gesture = new DragGesture(this).Threshold(4.0f);
        gesture.Started += _ =>
        {
            if (!EventRouter.BeginDragSession(this))
            {
                gesture.Cancel();
            }
        };
        gesture.Completed += e =>
        {
            dragClickPending = false;
            if (e.Cancelled)
            {
                EventRouter.CancelDragSessionForSource(this);
            }
        };
        dragGesture = gesture;
    }
}
