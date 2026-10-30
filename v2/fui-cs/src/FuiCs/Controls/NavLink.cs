namespace FuiCs;

/// <summary>A link navigating.</summary>
public sealed class NavigateEventArgs(string path) : EventArgs
{
    /// <summary>The link's target.</summary>
    public string Path { get; } = path;
}

/// <summary>What a link's content can restyle for: hover, a press in progress, focus and enabled state.</summary>
public readonly record struct NavLinkInteractionState(bool IsHovered, bool IsPressed, bool IsFocused, bool IsEnabled);

/// <summary>
/// A link around any content. A primary click, a tap or Enter navigates to <see cref="Href"/>; a middle click, the
/// platform's primary shortcut modifier (Ctrl or ⌘) or <see cref="OpenInNewTab"/> opens it in a new tab. While the
/// link is hovered or focused, the host previews its URL, as browsers do.
/// </summary>
public class NavLink : FlexBox
{
    private static NavLink? previewOwner;

    private string href;
    private bool hovered;
    private bool focused;
    private bool pointerPressed;
    private bool pointerNewTab;
    private bool enterPressed;
    private bool enterNewTab;
    private bool previewVisible;
    private bool previewPinned;

    /// <summary>A link to <paramref name="href"/>, opened in a new tab when <paramref name="openInNewTab"/>.</summary>
    public NavLink(string href, bool openInNewTab = false)
    {
        ArgumentNullException.ThrowIfNull(href);
        this.href = href;
        OpenInNewTab = openInNewTab;
        SemanticRole = FuiCs.SemanticRole.Link;
        Cursor = CursorStyle.Pointer;
        SetFocusable(true, 0);
        RequireInteractive();
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => SyncFocusRing()));
    }

    /// <summary>Raised after the link navigates.</summary>
    public event Action<NavigateEventArgs>? Navigate;

    /// <summary>Raised when hover, press, focus or enabled state changes, to restyle the link's content.</summary>
    public event Action<NavLinkInteractionState>? InteractionStateChanged;

    /// <summary>The link's target.</summary>
    public string Href
    {
        get => href;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            href = value;
            if (previewVisible)
            {
                ShowPreview();
            }
        }
    }

    /// <summary>Whether every activation opens a new tab.</summary>
    public bool OpenInNewTab { get; set; }

    /// <summary>Whether the link is hovered, pressed, focused and enabled, for styling its content.</summary>
    public NavLinkInteractionState InteractionState => new(hovered, pointerPressed || enterPressed, focused, IsEnabled);

    /// <inheritdoc/>
    public override void Dispose()
    {
        HidePreview();
        base.Dispose();
    }

    /// <summary>Keeps the URL preview showing while a context menu for the link is open.</summary>
    internal void PinPreviewForContextMenu()
    {
        previewPinned = true;
        ShowPreview();
    }

    internal void ReleasePreviewForContextMenu()
    {
        if (!previewPinned)
        {
            return;
        }
        previewPinned = false;
        if (!hovered && !focused)
        {
            HidePreview();
        }
    }

    /// <inheritdoc/>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        HandleLinkPointer(e);
    }

    /// <summary>Hovering and pressing the link's content count as the link's, as they do for a browser's links.</summary>
    protected internal override bool HandleBubbledPointerEvent(PointerEventArgs e)
    {
        var handled = base.HandleBubbledPointerEvent(e);
        if (IsEnabled && IsVisible)
        {
            HandleLinkPointer(e);
        }
        return handled;
    }

    private void HandleLinkPointer(PointerEventArgs e)
    {
        var middle = e.PointerType == PointerType.Mouse && e.Button == PointerButton.Auxiliary;
        var activation = e.IsPrimaryActivation || middle;
        var over = GetBounds().Contains(e.SceneX, e.SceneY);
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                hovered = true;
                ShowPreview();
                break;
            case PointerEventType.Leave when !over:
                hovered = false;
                pointerPressed = false;
                if (!previewPinned && !focused)
                {
                    HidePreview();
                }
                break;
            case PointerEventType.Down:
                pointerPressed = activation;
                pointerNewTab = activation && (middle || OpensInNewTab(e.Modifiers));
                break;
            case PointerEventType.Up when pointerPressed && activation:
                pointerPressed = false;
                // The host keeps routing a pressed pointer to the pressed node after it leaves; only a release over
                // the link counts.
                if (over)
                {
                    Activate(middle || pointerNewTab || OpensInNewTab(e.Modifiers));
                }
                break;
            case PointerEventType.Cancel:
                pointerPressed = false;
                break;
            default:
                return;
        }
        Refresh();
    }

    /// <inheritdoc/>
    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (key != "Enter" || (modifiers != KeyModifier.None && !KeyboardShortcuts.HasPrimaryModifier(modifiers)))
        {
            return handled;
        }
        if (eventType == KeyEventType.Down)
        {
            enterPressed = true;
            enterNewTab = OpensInNewTab(modifiers);
            Refresh();
            return true;
        }
        if (eventType == KeyEventType.Up && enterPressed)
        {
            enterPressed = false;
            Activate(enterNewTab || OpensInNewTab(modifiers));
            Refresh();
            return true;
        }
        return handled;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        this.focused = focused;
        if (focused)
        {
            ShowPreview();
        }
        else
        {
            enterPressed = false;
            if (!hovered && !previewPinned)
            {
                HidePreview();
            }
        }
        Refresh();
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        if (!isEnabled)
        {
            hovered = false;
            focused = false;
            pointerPressed = false;
            enterPressed = false;
        }
        Refresh();
    }

    private bool OpensInNewTab(KeyModifier modifiers) => OpenInNewTab || KeyboardShortcuts.HasPrimaryModifier(modifiers);

    private void Activate(bool newTab)
    {
        Navigation.NavigateTo(href, newTab);
        Navigate?.Invoke(new NavigateEventArgs(href));
    }

    private void Refresh()
    {
        SyncFocusRing();
        InteractionStateChanged?.Invoke(InteractionState);
    }

    private void SyncFocusRing()
    {
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(Theme.Active.Value.Spacing.Xs));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }

    private void ShowPreview()
    {
        if (previewOwner is { } previous && !ReferenceEquals(previous, this))
        {
            previous.previewVisible = false;
        }
        Runtime.Abi.HostShowUrlPreview(href);
        previewVisible = true;
        previewOwner = this;
    }

    private void HidePreview()
    {
        if (!previewVisible && !ReferenceEquals(previewOwner, this))
        {
            return;
        }
        previewVisible = false;
        if (ReferenceEquals(previewOwner, this))
        {
            previewOwner = null;
            Runtime.Abi.HostHideUrlPreview();
        }
    }
}

/// <summary>Fluent configuration for <see cref="NavLink"/>.</summary>
public static class NavLinkExtensions
{
    extension<TLink>(TLink link) where TLink : NavLink
    {
        /// <summary>Sets <see cref="NavLink.Href"/>.</summary>
        public TLink Href(string href)
        {
            link.Href = href;
            return link;
        }

        /// <summary>Opens the link in a new tab.</summary>
        public TLink OpenInNewTab(bool openInNewTab = true)
        {
            link.OpenInNewTab = openInNewTab;
            return link;
        }

        /// <summary>Adds a handler to <see cref="NavLink.Navigate"/>.</summary>
        public TLink OnNavigate(Action<NavigateEventArgs> handler)
        {
            link.Navigate += handler;
            return link;
        }

        /// <summary>Adds a handler to <see cref="NavLink.InteractionStateChanged"/>.</summary>
        public TLink OnInteractionStateChanged(Action<NavLinkInteractionState> handler)
        {
            link.InteractionStateChanged += handler;
            handler(link.InteractionState);
            return link;
        }
    }
}
