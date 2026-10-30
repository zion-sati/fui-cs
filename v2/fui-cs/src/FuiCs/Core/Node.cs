using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A retained UI node. Constructing a node does no native work; <see cref="Build"/> creates the native node and
/// its subtree. After that, changes update the native node directly and schedule a commit.
/// </summary>
public abstract partial class Node : IDisposable
{
    private const ulong InvalidHandle = (ulong)HandleValue.Invalid;

    private readonly List<Node> children = [];
    private readonly uint attachedPropertyKey = AttachedProperties.AllocateKey();
    private List<IDisposable>? trackedResources;
    private string? nodeId;
    private ulong scrollProxyTargetHandle = InvalidHandle;
    private bool interactive;
    private bool focusable;
    private int tabIndex;
    private bool preserveSelectionOnPointerDown;
    private CursorStyle cursor = CursorStyle.Default;
    private bool ownEnabled = true;
    private bool inheritedEnabled = true;
    private bool lastEffectiveEnabled = true;
    private Visibility ownVisibility = Visibility.Normal;
    private Visibility inheritedVisibility = Visibility.Normal;
    private Visibility lastEffectiveVisibility = Visibility.Normal;

    /// <summary>The native node; <see cref="HandleValue.Invalid"/> before <see cref="Build"/> and after disposal.</summary>
    public ulong Handle { get; protected internal set; } = InvalidHandle;

    /// <summary>Whether the native node exists.</summary>
    public bool IsBuilt => Handle != InvalidHandle;

    /// <summary>The node's parent, or null for a root or a detached node.</summary>
    public Node? Parent { get; private set; }

    /// <summary>The node's children in order.</summary>
    public IReadOnlyList<Node> Children => children;

    /// <summary>Whether the node has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>The node's stable id, which the host's debug tree, find-on-page and persisted UI state key on.</summary>
    public virtual string? NodeId
    {
        get => nodeId;
        set
        {
            nodeId = value;
            if (IsBuilt)
            {
                Ui.SetNodeId(Handle, value ?? "");
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The pointer's cursor over the node.</summary>
    public CursorStyle Cursor
    {
        get => cursor;
        set
        {
            cursor = value;
            EventRouter.HandleCursorStyleChanged(this);
        }
    }

    /// <summary>True when this node and all its ancestors are enabled. Setting it changes only this node.</summary>
    public virtual bool IsEnabled
    {
        get => ownEnabled && inheritedEnabled;
        set
        {
            if (ownEnabled == value)
            {
                return;
            }
            ownEnabled = value;
            if (!value)
            {
                CancelDragState();
            }
            ApplyEnabledChanged();
        }
    }

    /// <summary>This node's own visibility. <see cref="IsVisible"/> also accounts for ancestors.</summary>
    public Visibility Visibility
    {
        get => ownVisibility;
        set
        {
            if (ownVisibility == value)
            {
                return;
            }
            ownVisibility = value;
            if (value != Visibility.Normal)
            {
                CancelDragState();
            }
            ApplyVisibilityChanged();
        }
    }

    /// <summary>True when neither this node nor any ancestor is hidden or collapsed.</summary>
    public bool IsVisible => EffectiveVisibility == Visibility.Normal;

    /// <summary>Whether the node takes keyboard focus while it is enabled and visible.</summary>
    public bool Focusable
    {
        get => focusable;
        set => SetFocusable(value, tabIndex);
    }

    /// <summary>The node's place in the tab order when focusable; 0 follows document order.</summary>
    public int TabIndex
    {
        get => tabIndex;
        set => SetFocusable(focusable, value);
    }

    /// <summary>Keeps the page's text selection when a press lands on the node, as toolbars and handles need.</summary>
    public bool PreserveSelectionOnPointerDown
    {
        get => preserveSelectionOnPointerDown;
        set
        {
            if (preserveSelectionOnPointerDown == value)
            {
                return;
            }
            preserveSelectionOnPointerDown = value;
            if (IsBuilt)
            {
                Ui.SetPreserveSelectionOnPointerDown(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Whether text selection does not extend across the node, as for controls with their own text.</summary>
    public virtual bool IsSelectionBarrier => false;

    /// <summary>Whether the node is text the user can select.</summary>
    public virtual bool IsSelectableText => false;

    /// <summary>Whether the node is text the user can edit.</summary>
    public virtual bool IsEditableText => false;

    /// <summary>The editable text inside a control whose shell takes its pointer input, such as a text input.</summary>
    internal virtual TextCore? InnerEditor => null;

    internal uint AttachedPropertyKey => attachedPropertyKey;

    /// <summary>For layout diagnostics: whether the node is taken out of its parent's flow.</summary>
    internal virtual bool DebugIsAbsolutelyPositioned => false;

    /// <summary>Marks the node as a portal, rendered above the normal tree. Applied when the node is built.</summary>
    protected bool IsPortal { get; set; }

    private Visibility EffectiveVisibility =>
        inheritedVisibility != Visibility.Normal ? inheritedVisibility : ownVisibility;

    /// <summary>Creates the native node and its subtree if it does not exist yet, and returns its handle.</summary>
    public abstract ulong Build();

    /// <summary>Disposes the subtree, deleting each native node once.</summary>
    public virtual void Dispose() => DisposeTree();

    /// <summary>The node's bounds in scene pixels; empty before the node is built and laid out.</summary>
    public Rect GetBounds() => IsBuilt && Ui.TryGetBounds(Handle, out var bounds) ? bounds : default;

    /// <summary>Converts a page point to the node's coordinates.</summary>
    public Point AbsoluteToLocalPosition(float absoluteX, float absoluteY)
    {
        var bounds = GetBounds();
        return new Point(absoluteX - bounds.X, absoluteY - bounds.Y);
    }

    /// <summary>Converts a point in the node's coordinates to the page's.</summary>
    public Point LocalToAbsolutePosition(float localX, float localY)
    {
        var bounds = GetBounds();
        return new Point(localX + bounds.X, localY + bounds.Y);
    }

    /// <summary>Moves keyboard focus to this node once it is built.</summary>
    public virtual void Focus()
    {
        if (IsBuilt)
        {
            Ui.RequestFocus(Handle);
        }
    }

    /// <summary>Appends <paramref name="node"/>, moving it from its current parent. Adding itself or an existing child does nothing.</summary>
    public void AddChild(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (ReferenceEquals(node, this) || children.Contains(node))
        {
            return;
        }
        node.Parent?.RemoveChild(node);
        node.Parent = this;
        node.OnParentChanged();
        children.Add(node);
        node.SetInheritedEnabled(IsEnabled);
        node.SetInheritedVisibility(EffectiveVisibility);
        if (IsBuilt)
        {
            Ui.AddChild(Handle, node.Build());
            NotifyRetainedMutation();
            OnChildLayoutChanged();
        }
    }

    /// <summary>Removes <paramref name="node"/> without disposing it; its native node stays alive so it can be added again.</summary>
    public void RemoveChild(Node node)
    {
        var index = children.IndexOf(node);
        if (index < 0)
        {
            return;
        }
        if (IsBuilt && node.IsBuilt)
        {
            Ui.RemoveChild(Handle, node.Handle);
        }
        children.RemoveAt(index);
        node.Parent = null;
        node.OnParentChanged();
        node.SetInheritedEnabled(true);
        node.SetInheritedVisibility(Visibility.Normal);
        if (IsBuilt)
        {
            NotifyRetainedMutation();
            OnChildLayoutChanged();
        }
    }

    /// <summary>
    /// Replaces the node's children with <paramref name="nodes"/>, building them if the node is built.
    /// </summary>
    protected void ReplaceChildren(ReadOnlySpan<Node> nodes)
    {
        while (children.Count > 0)
        {
            RemoveChild(children[^1]);
        }
        foreach (var node in nodes)
        {
            AddChild(node);
        }
    }

    /// <summary>Sends the node id, semantics, visibility and attached properties to a newly created native node.</summary>
    protected void ApplyNodeMetadata()
    {
        if (nodeId is not null)
        {
            Ui.SetNodeId(Handle, nodeId);
        }
        ApplySemanticMetadata();
        if (IsPortal)
        {
            Ui.SetIsPortal(Handle, true);
        }
        Ui.SetVisibility(Handle, EffectiveVisibility);
        AttachedProperties.Apply(attachedPropertyKey, Handle);
        if (scrollProxyTargetHandle != InvalidHandle)
        {
            Ui.SetScrollProxyTarget(Handle, scrollProxyTargetHandle);
        }
    }

    /// <summary>Registers a newly created native node for events and applies focus and interaction state.</summary>
    protected void FinishBuild()
    {
        IsDisposed = false;
        lastEffectiveEnabled = IsEnabled;
        lastEffectiveVisibility = EffectiveVisibility;
        EventRouter.Register(Handle, this);
        if (focusable && IsEnabled && IsVisible)
        {
            Ui.SetFocusable(Handle, focusable, tabIndex);
        }
        if (interactive && IsEnabled && IsVisible)
        {
            Ui.SetInteractive(Handle, true);
        }
        if (preserveSelectionOnPointerDown)
        {
            Ui.SetPreserveSelectionOnPointerDown(Handle, true);
        }
    }

    /// <summary>Builds the children and attaches them to the native node, for a <see cref="Build"/> override.</summary>
    protected void BuildChildren()
    {
        foreach (var child in children)
        {
            Ui.AddChild(Handle, child.Build());
        }
    }

    /// <summary>Disposes the node's children and deletes its native node.</summary>
    protected void DisposeTree()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;
        DisposeTrackedResources();
        CancelDragState();
        // Before the children: a shown tooltip's host is one of them.
        EventRouter.ToolTips?.OwnerDisposing(this);
        for (var index = children.Count - 1; index >= 0; --index)
        {
            children[index].Dispose();
        }
        children.Clear();
        if (IsBuilt)
        {
            EventRouter.Unregister(Handle);
            Ui.DeleteNode(Handle);
            Handle = InvalidHandle;
            FrameScheduler.MarkNeedsCommit();
        }
    }

    /// <summary>Disposes <paramref name="resource"/>, such as a signal subscription, when this node is disposed.</summary>
    protected void Track(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        (trackedResources ??= []).Add(resource);
    }

    /// <summary>Schedules a commit when the node is built.</summary>
    protected void NotifyRetainedMutation()
    {
        RetainedMutationCount += 1;
        if (IsBuilt)
        {
            FrameScheduler.MarkNeedsCommit();
        }
    }

    /// <summary>Counts the changes this node has sent to the host, so a bitmap render can tell its prepared text is stale.</summary>
    internal int RetainedMutationCount { get; private set; }

    /// <summary>The <see cref="RetainedMutationCount"/> when <see cref="Bitmap.PrepareText"/> last prepared this node's text.</summary>
    internal int? TextPreparedAtMutation { get; set; }

    /// <summary>The frame scheduler's commit count when <see cref="Bitmap.PrepareText"/> queued this node's shaping.</summary>
    internal int TextPreparedAtCommit { get; set; }

    /// <summary>Schedules a commit and tells the parent that this node's layout changed.</summary>
    protected internal void NotifyRetainedLayoutMutation()
    {
        NotifyRetainedMutation();
        if (Parent is { IsBuilt: true } parent)
        {
            parent.OnChildLayoutChanged();
        }
    }

    /// <summary>Called when the node is attached to or detached from a parent.</summary>
    protected virtual void OnParentChanged()
    {
    }

    /// <summary>Called when a child's layout changes.</summary>
    protected virtual void OnChildLayoutChanged()
    {
    }

    /// <summary>
    /// Called when the node's effective enabled state changes, through its own setting or an ancestor's.
    /// </summary>
    protected virtual void OnEffectiveEnabledChanged(bool isEnabled)
    {
    }

    /// <summary>
    /// Called when the node's effective visibility changes, through its own setting or an ancestor's.
    /// </summary>
    protected virtual void OnEffectiveVisibilityChanged(Visibility visibility)
    {
    }

    /// <summary>Makes the node a hit-test target while it is enabled and visible.</summary>
    protected void RequireInteractive()
    {
        interactive = true;
        if (IsBuilt && IsEnabled && IsVisible)
        {
            Ui.SetInteractive(Handle, true);
            NotifyRetainedMutation();
        }
    }

    /// <summary>Sends the pointer's moves and release to this node until it releases them.</summary>
    protected internal void CapturePointer()
    {
        if (!IsBuilt)
        {
            return;
        }
        EventRouter.CapturePointer(Handle);
        Runtime.Abi.HostSetPointerCapture(Handle);
    }

    /// <summary>Releases a pointer captured with <see cref="CapturePointer"/>.</summary>
    protected internal void ReleasePointer()
    {
        if (!IsBuilt)
        {
            return;
        }
        EventRouter.ReleasePointer(Handle);
        Runtime.Abi.HostReleasePointerCapture();
    }

    internal void SetFocusable(bool flag, int index)
    {
        focusable = flag;
        tabIndex = index;
        if (flag)
        {
            RequireInteractive();
        }
        if (IsBuilt)
        {
            Ui.SetFocusable(Handle, IsEnabled && IsVisible && flag, index);
            if (interactive)
            {
                Ui.SetInteractive(Handle, IsEnabled && IsVisible);
            }
            NotifyRetainedMutation();
        }
    }

    /// <summary>Routes scrolling over this node to the scroll node <paramref name="handle"/>.</summary>
    internal void BindScrollProxyTarget(ulong handle)
    {
        scrollProxyTargetHandle = handle;
        if (IsBuilt)
        {
            Ui.SetScrollProxyTarget(Handle, handle);
            NotifyRetainedMutation();
        }
    }

    partial void CancelDragState();

    /// <summary>The font ids this node renders with, so callers can wait for them to load.</summary>
    internal virtual IReadOnlyList<uint> RequiredFontIds() => [];

    /// <summary>For layout diagnostics: the node's percent size on the parent's main axis, or -1.</summary>
    internal virtual float DebugMainAxisPercentValue(bool isHorizontal) => -1.0f;

    /// <summary>For layout diagnostics: the child indices from the root, as root/0/2.</summary>
    internal string DebugTreePath()
    {
        var path = "";
        for (var current = this; current.Parent is { } parent; current = parent)
        {
            path = "/" + parent.children.IndexOf(current) + path;
        }
        return "root" + path;
    }

    private void DisposeTrackedResources()
    {
        if (trackedResources is null)
        {
            return;
        }
        for (var index = trackedResources.Count - 1; index >= 0; --index)
        {
            trackedResources[index].Dispose();
        }
        trackedResources.Clear();
    }

    private void SetInheritedEnabled(bool flag)
    {
        if (inheritedEnabled == flag)
        {
            return;
        }
        inheritedEnabled = flag;
        ApplyEnabledChanged();
    }

    private void SetInheritedVisibility(Visibility value)
    {
        if (inheritedVisibility == value)
        {
            return;
        }
        inheritedVisibility = value;
        ApplyVisibilityChanged();
    }

    private void ApplyEnabledChanged()
    {
        var effective = IsEnabled;
        if (lastEffectiveEnabled == effective)
        {
            return;
        }
        lastEffectiveEnabled = effective;
        if (trackSemanticDisabledFromEnabled)
        {
            semanticDisabled = !effective;
        }
        if (IsBuilt)
        {
            var interactiveEffective = effective && IsVisible;
            var updated = false;
            if (interactive)
            {
                Ui.SetInteractive(Handle, interactiveEffective);
                updated = true;
            }
            if (focusable)
            {
                Ui.SetFocusable(Handle, interactiveEffective, tabIndex);
                updated = true;
            }
            if (trackSemanticDisabledFromEnabled)
            {
                Ui.SetSemanticDisabled(Handle, true, !effective);
                updated = true;
            }
            if (updated)
            {
                NotifyRetainedMutation();
            }
        }
        OnEffectiveEnabledChanged(effective);
        if (!effective)
        {
            CancelDragState();
        }
        foreach (var child in children)
        {
            child.SetInheritedEnabled(effective);
        }
    }

    private void ApplyVisibilityChanged()
    {
        var effective = EffectiveVisibility;
        if (lastEffectiveVisibility == effective)
        {
            return;
        }
        lastEffectiveVisibility = effective;
        if (IsBuilt)
        {
            Ui.SetVisibility(Handle, effective);
            var interactiveEffective = IsEnabled && IsVisible;
            if (interactive)
            {
                Ui.SetInteractive(Handle, interactiveEffective);
            }
            if (focusable)
            {
                Ui.SetFocusable(Handle, interactiveEffective, tabIndex);
            }
            NotifyRetainedMutation();
        }
        OnEffectiveVisibilityChanged(effective);
        if (effective != Visibility.Normal)
        {
            CancelDragState();
        }
        foreach (var child in children)
        {
            child.SetInheritedVisibility(effective);
        }
    }
}
