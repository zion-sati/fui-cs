namespace FuiCs;

/// <summary>
/// A retained subtree that can be activated, deactivated and reused, such as the content of a tab. It owns
/// <see cref="Root"/> and every resource passed to <see cref="Retain"/>, and disposes them together.
/// </summary>
public sealed class RetainedView(Node root) : IDisposable
{
    private readonly List<IDisposable> retainedResources = [];

    /// <summary>The view's root node.</summary>
    public Node Root { get; } = root;

    /// <summary>Whether the view is active: mounted and receiving updates.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Whether the view has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>Raised when the view activates.</summary>
    public event Action<RetainedView>? Activated;

    /// <summary>Raised when the view deactivates.</summary>
    public event Action<RetainedView>? Deactivated;

    /// <summary>Raised when the view is disposed.</summary>
    public event Action<RetainedView>? Disposed;

    /// <summary>Disposes <paramref name="resource"/> with the view, or immediately if the view is already disposed.</summary>
    public RetainedView Retain(IDisposable resource)
    {
        if (IsDisposed)
        {
            resource.Dispose();
            return this;
        }
        retainedResources.Add(resource);
        return this;
    }

    /// <summary>Adds a handler for activation.</summary>
    public RetainedView OnActivate(Action<RetainedView> handler)
    {
        Activated += handler;
        return this;
    }

    /// <summary>Adds a handler for deactivation.</summary>
    public RetainedView OnDeactivate(Action<RetainedView> handler)
    {
        Deactivated += handler;
        return this;
    }

    /// <summary>Adds a handler for disposal.</summary>
    public RetainedView OnDispose(Action<RetainedView> handler)
    {
        Disposed += handler;
        return this;
    }

    /// <summary>Activates the view.</summary>
    public void Activate()
    {
        if (IsDisposed || IsActive)
        {
            return;
        }
        IsActive = true;
        Activated?.Invoke(this);
    }

    /// <summary>Deactivates the view, keeping its nodes for the next activation.</summary>
    public void Deactivate()
    {
        if (IsDisposed || !IsActive)
        {
            return;
        }
        EventRouter.DeactivateSubtree(Root);
        IsActive = false;
        Deactivated?.Invoke(this);
    }

    /// <summary>Disposes the view's root and raises Disposed.</summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        Deactivate();
        IsDisposed = true;
        Disposed?.Invoke(this);
        for (var index = retainedResources.Count - 1; index >= 0; --index)
        {
            retainedResources[index].Dispose();
        }
        retainedResources.Clear();
        Root.Dispose();
        Activated = null;
        Deactivated = null;
        Disposed = null;
    }
}
