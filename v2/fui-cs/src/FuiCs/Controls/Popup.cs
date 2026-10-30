namespace FuiCs;

/// <summary>
/// A popup surface shown over the page at a point or next to an anchor, over an optional backdrop. Content added
/// with <c>Child</c> goes on the surface; a backdrop click hides the popup unless <see cref="DismissOnBackdropClick"/> is off.
/// </summary>
public class Popup : Portal
{
    private readonly PopupPresenter presenter;

    /// <summary>An empty popup; add its content, mount it, then open it.</summary>
    public Popup()
    {
        Surface = new FlexBox().PositionAbsolute().FlexDirection(FlexDirection.Column);
        presenter = new PopupPresenter(this, Surface);
        PositionType = PositionType.Absolute;
        Position = new Point(0.0f, 0.0f);
        Width = Length.Percent(100.0f);
        Height = Length.Percent(100.0f);
        // Clicks on the panel's own background reach the overlay too, since the panel is not a hit target.
        presenter.Overlay.PointerClick += e =>
        {
            if (DismissOnBackdropClick && !Surface.GetBounds().Contains(e.SceneX, e.SceneY))
            {
                Hide();
            }
        };
    }

    /// <summary>Whether the popup is showing.</summary>
    public bool IsOpen => presenter.IsOpen;

    /// <summary>Raised when the popup shows after being hidden.</summary>
    public event Action? Opened;

    /// <summary>Raised when the popup hides, by <see cref="Hide"/> or a backdrop click.</summary>
    public event Action? Closed;

    /// <summary>The panel that holds the popup's content.</summary>
    public FlexBox Surface { get; }

    /// <summary>Where the popup opens relative to its anchor.</summary>
    public PopupPlacement Placement
    {
        get => presenter.Placement;
        set => presenter.Placement = value;
    }

    /// <summary>The least distance between the popup and the viewport's edges.</summary>
    public float EdgePadding
    {
        get => presenter.EdgePadding;
        set => presenter.EdgePadding = value;
    }

    /// <summary>The space between an anchor and the popup.</summary>
    public float AnchorGap
    {
        get => presenter.AnchorGap;
        set => presenter.AnchorGap = value;
    }

    /// <summary>Closes the popup when the backdrop is clicked.</summary>
    public bool DismissOnBackdropClick { get; set; } = true;

    /// <summary>The veil over the page while the popup is open.</summary>
    public Color BackdropColor
    {
        get => presenter.BackdropColor;
        set => presenter.BackdropColor = value;
    }

    /// <summary>Blur behind the backdrop; <see cref="Surface"/> has its own <c>BackgroundBlur</c> for the panel.</summary>
    public override float BackgroundBlur
    {
        get => presenter.BackgroundBlur;
        set => presenter.BackgroundBlur = value;
    }

    /// <inheritdoc/>
    public override void AddContent(Node node) => Surface.AddContent(node);

    /// <inheritdoc/>
    public override void SetContent(params ReadOnlySpan<Node> nodes) => Surface.SetContent(nodes);

    /// <summary>Sizes the surface and shows it next to an anchor rectangle.</summary>
    public void ShowAnchored(float anchorX, float anchorY, float anchorWidth, float anchorHeight, float width, float height)
    {
        var wasOpen = IsOpen;
        SizeSurface(width, height);
        presenter.ShowAnchored(anchorX, anchorY, anchorWidth, anchorHeight, width, height);
        RaiseOpened(wasOpen);
    }

    /// <summary>Sizes the surface and shows it at a point.</summary>
    public void ShowAtPoint(float x, float y, float width, float height)
    {
        var wasOpen = IsOpen;
        SizeSurface(width, height);
        presenter.ShowAtPoint(x, y, width, height);
        RaiseOpened(wasOpen);
    }

    /// <summary>Closes the popup.</summary>
    public void Hide()
    {
        if (!IsOpen)
        {
            return;
        }
        presenter.Hide();
        Closed?.Invoke();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        presenter.Dispose();
        base.Dispose();
    }

    private void RaiseOpened(bool wasOpen)
    {
        if (!wasOpen && IsOpen)
        {
            Opened?.Invoke();
        }
    }

    private void SizeSurface(float width, float height)
    {
        Surface.Width = width;
        Surface.Height = height;
    }
}

/// <summary>Fluent configuration for <see cref="Popup"/> and its subclasses.</summary>
public static class PopupExtensions
{
    extension<TPopup>(TPopup popup) where TPopup : Popup
    {
        /// <summary>Adds a handler for the popup opening.</summary>
        public TPopup OnOpened(Action handler)
        {
            popup.Opened += handler;
            return popup;
        }

        /// <summary>Adds a handler for the popup closing.</summary>
        public TPopup OnClosed(Action handler)
        {
            popup.Closed += handler;
            return popup;
        }

        /// <summary>Sets <see cref="Popup.Placement"/>.</summary>
        public TPopup Placement(PopupPlacement placement)
        {
            popup.Placement = placement;
            return popup;
        }

        /// <summary>Sets <see cref="Popup.EdgePadding"/>.</summary>
        public TPopup EdgePadding(float padding)
        {
            popup.EdgePadding = padding;
            return popup;
        }

        /// <summary>Sets <see cref="Popup.AnchorGap"/>.</summary>
        public TPopup AnchorGap(float gap)
        {
            popup.AnchorGap = gap;
            return popup;
        }

        /// <summary>Sets <see cref="Popup.DismissOnBackdropClick"/>.</summary>
        public TPopup DismissOnBackdropClick(bool dismiss = true)
        {
            popup.DismissOnBackdropClick = dismiss;
            return popup;
        }

        /// <summary>Sets <see cref="Popup.BackdropColor"/>.</summary>
        public TPopup BackdropColor(Color color)
        {
            popup.BackdropColor = color;
            return popup;
        }

        /// <summary>Fills the panel.</summary>
        public TPopup PanelColor(Color color)
        {
            popup.Surface.Background = color;
            return popup;
        }

        /// <summary>Blurs the page behind the panel.</summary>
        public TPopup PanelBackgroundBlur(float sigma)
        {
            popup.Surface.BackgroundBlur = sigma;
            return popup;
        }

        /// <summary>Rounds the panel's corners.</summary>
        public TPopup PanelCornerRadius(float radius)
        {
            popup.Surface.CornerRadius = new CornerRadius(radius);
            return popup;
        }

        /// <summary>Draws a solid border around the panel.</summary>
        public TPopup PanelBorder(float width, Color color)
        {
            popup.Surface.Border = FuiCs.Border.Solid(width, color);
            return popup;
        }

        /// <summary>Draws a border around the panel.</summary>
        public TPopup PanelBorder(Border border)
        {
            popup.Surface.Border = border;
            return popup;
        }

        /// <summary>Draws a shadow behind the panel.</summary>
        public TPopup PanelShadow(Color color, float offsetX, float offsetY, float blurSigma, float spread = 0.0f)
        {
            popup.Surface.DropShadow = new DropShadow(color, offsetX, offsetY, blurSigma, spread);
            return popup;
        }
    }
}
