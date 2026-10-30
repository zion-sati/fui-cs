namespace FuiCs;

/// <summary>Runs the single in-tree drag session: tracks the drop target under the pointer and completes the drop.</summary>
internal static class DragDropManager
{
    private static DragSession? activeSession;
    private static Node? activeTarget;

    public static CursorStyle CursorOverrideStyle()
    {
        var session = activeSession;
        if (session is null || !session.IsActive)
        {
            return CursorStyle.Default;
        }
        return session.CurrentEffect == DragDropEffects.None ? CursorStyle.Grabbing : CursorStyle.Move;
    }

    public static bool BeginSession(Node source)
    {
        if (activeSession is { } existing)
        {
            FinishSession(existing, DragDropEffects.None, 0.0f, 0.0f, KeyModifier.None, true);
        }
        if (!source.HasDragSource)
        {
            return false;
        }
        var data = source.CreateDragDataObject();
        if (data is null)
        {
            return false;
        }
        var allowed = source.DragAllowedEffects;
        if (allowed == DragDropEffects.None)
        {
            return false;
        }
        activeTarget = null;
        activeSession = new DragSession(source, data, allowed);
        return true;
    }

    public static void CancelSession(DragSession session)
    {
        if (!ReferenceEquals(activeSession, session))
        {
            return;
        }
        FinishSession(session, DragDropEffects.None, 0.0f, 0.0f, KeyModifier.None, true);
    }

    public static void CancelSessionForSource(Node source)
    {
        var session = activeSession;
        if (session is null || !ReferenceEquals(session.Source, source))
        {
            return;
        }
        FinishSession(session, DragDropEffects.None, 0.0f, 0.0f, KeyModifier.None, true);
    }

    public static void HandleNodeDestroyed(Node node)
    {
        var session = activeSession;
        if (session is not null && ReferenceEquals(session.Source, node))
        {
            FinishSession(session, DragDropEffects.None, 0.0f, 0.0f, KeyModifier.None, true);
            return;
        }
        if (ReferenceEquals(activeTarget, node))
        {
            activeTarget = null;
            session?.SetCurrentEffect(DragDropEffects.None);
        }
    }

    public static void HandlePointerEvent(Node? pointedNode, PointerEventType eventType, float x, float y, KeyModifier modifiers)
    {
        var session = activeSession;
        if (session is null || !session.IsActive)
        {
            return;
        }
        if (eventType is PointerEventType.Down or PointerEventType.Enter or PointerEventType.Move or PointerEventType.Leave)
        {
            UpdateTarget(pointedNode, session, x, y, modifiers);
            return;
        }
        if (eventType != PointerEventType.Up)
        {
            return;
        }
        var effect = UpdateTarget(pointedNode, session, x, y, modifiers);
        var target = activeTarget;
        if (target is not null && effect != DragDropEffects.None)
        {
            target.RaiseDrop(new DragEventArgs(session, x, y, modifiers));
        }
        if (ReferenceEquals(activeSession, session) && session.IsActive)
        {
            FinishSession(session, effect, x, y, modifiers, true);
        }
    }

    public static void Reset()
    {
        activeSession = null;
        activeTarget = null;
    }

    private static DragDropEffects UpdateTarget(Node? pointedNode, DragSession session, float x, float y, KeyModifier modifiers)
    {
        var target = ResolveDropTarget(pointedNode);
        var args = new DragEventArgs(session, x, y, modifiers);
        var proposal = DropProposal.None;
        if (!ReferenceEquals(target, activeTarget))
        {
            activeTarget?.RaiseDragLeave(args);
            activeTarget = target;
            if (target is not null && target.HasDragEnterHandler)
            {
                proposal = target.RaiseDragEnter(args);
            }
        }
        if (target is null)
        {
            session.SetCurrentEffect(DragDropEffects.None);
            return DragDropEffects.None;
        }
        if (target.HasDragOverHandler)
        {
            proposal = target.RaiseDragOver(args);
        }
        else if (ReferenceEquals(target, activeTarget) && IsDefaultProposal(proposal))
        {
            return session.CurrentEffect;
        }
        var effect = NormalizeEffect(proposal.Effect, session.AllowedEffects);
        session.SetCurrentEffect(effect);
        return effect;
    }

    private static Node? ResolveDropTarget(Node? pointedNode)
    {
        for (var current = pointedNode; current is not null; current = current.Parent)
        {
            if (current.AllowDrop)
            {
                return current;
            }
        }
        return null;
    }

    private static void FinishSession(DragSession session, DragDropEffects effect, float x, float y, KeyModifier modifiers, bool notifyTargetLeave)
    {
        if (!ReferenceEquals(activeSession, session))
        {
            return;
        }
        var target = activeTarget;
        activeSession = null;
        activeTarget = null;
        session.SetCurrentEffect(effect);
        if (notifyTargetLeave && target is not null)
        {
            target.RaiseDragLeave(new DragEventArgs(session, x, y, modifiers));
        }
        session.Complete(effect);
        session.Source.NotifyDragCompleted(effect);
    }

    /// <summary>The single effect to apply: Move, then Copy, then Link, among those both sides allow.</summary>
    private static DragDropEffects NormalizeEffect(DragDropEffects candidate, DragDropEffects allowed)
    {
        var masked = candidate & allowed;
        if ((masked & DragDropEffects.Move) != 0)
        {
            return DragDropEffects.Move;
        }
        if ((masked & DragDropEffects.Copy) != 0)
        {
            return DragDropEffects.Copy;
        }
        return (masked & DragDropEffects.Link) != 0 ? DragDropEffects.Link : DragDropEffects.None;
    }

    private static bool IsDefaultProposal(DropProposal proposal) =>
        proposal.Effect == DragDropEffects.None && !proposal.ShowInsertionMarker;
}
