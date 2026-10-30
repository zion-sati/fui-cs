using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A modal dialog: a card with a title, body text, optional content and OK and Cancel buttons, centered over a
/// blurred backdrop that covers the viewport wherever the dialog is mounted. Enter accepts and Escape or a backdrop
/// click cancels; either hides the dialog and returns focus to where it was.
/// </summary>
public class Dialog : Portal
{
    private const float CardWidth = 420.0f;
    private const float CardPadding = 24.0f;

    private static Dialog? active;

    private readonly TextCore title;
    private readonly TextCore body;
    private readonly FlexBox contentHost;
    private readonly Form form;
    private readonly FlexBox card;
    private readonly FlexBox overlay;
    private Node? bodyContent;
    private Node? focusBeforeShow;
    private TaskCompletionSource<bool>? pendingResult;
    private uint semanticScope;
    private Color? backdropColor;
    private float backgroundBlur = 16.0f;
    private Color? cardColor;
    private Border? cardBorder;
    private CornerRadius? cardCornerRadius;
    private DropShadow? cardShadow;

    /// <summary>A dialog with <paramref name="title"/> and <paramref name="body"/>; mount it, then show it.</summary>
    public Dialog(string title = "", string body = "")
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(body);
        this.title = new TextCore(title).Selectable(false).SemanticRole(FuiCs.SemanticRole.Heading);
        this.body = new TextCore(body).Selectable(false).SemanticRole(FuiCs.SemanticRole.StaticText);
        contentHost = FlexBox.Column().FillWidth();
        AcceptButton = new Button("OK").Width(108.0f);
        CancelButton = new Button("Cancel").Width(108.0f);
        AcceptButton.Click += _ => Accept();
        CancelButton.Click += _ => Cancel();
        form = new Form { DefaultButton = AcceptButton, CancelButton = CancelButton }
            .FillWidth()
            .Child(FlexBox.Row(AcceptButton, new FlexBox().Width(12.0f).Height(1.0f), CancelButton)
                .JustifyContent(FuiCs.JustifyContent.End)
                .AlignItems(FuiCs.AlignItems.Center)
                .FillWidth());
        card = FlexBox.Column(
                this.title,
                Spacer(12.0f),
                this.body,
                Spacer(16.0f),
                contentHost,
                Spacer(24.0f),
                form)
            .Width(CardWidth)
            .SemanticRole(FuiCs.SemanticRole.Dialog)
            .SemanticLabel(title);
        overlay = new FlexBox()
            .PositionAbsolute()
            .JustifyContent(FuiCs.JustifyContent.Center)
            .AlignItems(FuiCs.AlignItems.Center)
            .Child(card);
        // Clicks on the card's own background reach the overlay too, since the card is not a hit target.
        overlay.PointerClick += e =>
        {
            if (!card.GetBounds().Contains(e.SceneX, e.SceneY))
            {
                Cancel();
            }
        };
        PositionType = FuiCs.PositionType.Absolute;
        Position = new Point(0.0f, 0.0f);
        Width = Length.Percent(100.0f);
        Height = Length.Percent(100.0f);
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Refresh();
    }

    /// <summary>Raised after the user or code accepts the dialog, once it is hidden.</summary>
    public event Action? Accepted;

    /// <summary>Raised after the user or code cancels the dialog, once it is hidden.</summary>
    public event Action? Cancelled;

    /// <summary>Raised after the frame that first draws the shown dialog.</summary>
    public event Action? Shown;

    /// <summary>The open dialog shown most recently, if any.</summary>
    public static Dialog? Active => active;

    /// <summary>Whether the dialog is showing.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>The title's text.</summary>
    public string Title
    {
        get => title.Text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            title.Text = value;
            card.SemanticLabel = value;
        }
    }

    /// <summary>The body's text.</summary>
    public string Body
    {
        get => body.Text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            body.Text = value;
        }
    }

    /// <summary>The title's text node.</summary>
    public TextCore TitleText => title;

    /// <summary>The body's text node.</summary>
    public TextCore BodyText => body;

    /// <summary>The OK button.</summary>
    public Button AcceptButton { get; }

    /// <summary>The Cancel button.</summary>
    public Button CancelButton { get; }

    /// <summary>Content shown between the body text and the buttons, such as fields; null shows none.</summary>
    public Node? BodyContent
    {
        get => bodyContent;
        set
        {
            bodyContent = value;
            contentHost.SetContent(value is null ? [] : [value]);
        }
    }

    /// <summary>The backdrop over the page; null uses the theme's dialog backdrop.</summary>
    public Color? BackdropColor
    {
        get => backdropColor;
        set
        {
            backdropColor = value;
            Refresh();
        }
    }

    /// <summary>The blur of the page behind the backdrop.</summary>
    public override float BackgroundBlur
    {
        get => backgroundBlur;
        set
        {
            backgroundBlur = value;
            Refresh();
        }
    }

    /// <summary>The card's fill; null follows the theme.</summary>
    public Color? CardColor
    {
        get => cardColor;
        set
        {
            cardColor = value;
            Refresh();
        }
    }

    /// <summary>The card's border; null follows the theme.</summary>
    public Border? CardBorder
    {
        get => cardBorder;
        set
        {
            cardBorder = value;
            Refresh();
        }
    }

    /// <summary>The card's corner radius; null follows the theme.</summary>
    public CornerRadius? CardCornerRadius
    {
        get => cardCornerRadius;
        set
        {
            cardCornerRadius = value;
            Refresh();
        }
    }

    /// <summary>The card's shadow; null follows the theme.</summary>
    public DropShadow? CardShadow
    {
        get => cardShadow;
        set
        {
            cardShadow = value;
            Refresh();
        }
    }

    /// <summary>Accepts the open dialog shown most recently.</summary>
    public static void AcceptActive() => active?.Accept();

    /// <summary>Cancels the open dialog shown most recently.</summary>
    public static void CancelActive() => active?.Cancel();

    /// <summary>
    /// Shows the dialog, moves focus to <see cref="AcceptButton"/> once it is drawn, and makes Enter and Escape accept
    /// and cancel it.
    /// </summary>
    public void Show()
    {
        if (IsOpen)
        {
            return;
        }
        focusBeforeShow = EventRouter.FocusedNode;
        Refresh();
        CoverViewport();
        AddChild(overlay);
        if (semanticScope == 0 && overlay.IsBuilt)
        {
            semanticScope = Ui.PushSemanticScope(overlay.Handle);
        }
        form.Activate();
        IsOpen = true;
        active = this;
        FrameScheduler.AfterNextCommit(() =>
        {
            if (IsOpen)
            {
                AcceptButton.Focus();
                Shown?.Invoke();
            }
        });
    }

    /// <summary>Shows the dialog and completes with true when it is accepted or false when it is cancelled.</summary>
    public Task<bool> ShowAsync()
    {
        var result = pendingResult ??= new TaskCompletionSource<bool>();
        Show();
        return result.Task;
    }

    /// <summary>Hides the dialog without accepting or cancelling it, returning focus to where it was.</summary>
    public void Hide()
    {
        if (!IsOpen)
        {
            return;
        }
        form.Deactivate();
        if (semanticScope != 0)
        {
            Ui.RemoveSemanticScope(semanticScope);
            semanticScope = 0;
        }
        RemoveChild(overlay);
        IsOpen = false;
        if (ReferenceEquals(active, this))
        {
            active = null;
        }
        var restore = focusBeforeShow;
        focusBeforeShow = null;
        if (restore is { IsBuilt: true })
        {
            restore.Focus();
        }
    }

    /// <summary>Hides the open dialog and raises <see cref="Accepted"/>.</summary>
    public void Accept() => Close(accepted: true);

    /// <summary>Hides the open dialog and raises <see cref="Cancelled"/>.</summary>
    public void Cancel() => Close(accepted: false);

    /// <inheritdoc/>
    public override void Dispose()
    {
        Hide();
        if (!overlay.IsBuilt)
        {
            overlay.Dispose();
        }
        pendingResult?.TrySetCanceled();
        pendingResult = null;
        base.Dispose();
    }

    private static FlexBox Spacer(float height) => new FlexBox().FillWidth().Height(height);

    /// <summary>Places the overlay over the whole viewport, offsetting the dialog's own position.</summary>
    private void CoverViewport()
    {
        var bounds = IsBuilt && Ui.TryGetBounds(Handle, out var built) ? built : default;
        overlay.Position = new Point(-bounds.X, -bounds.Y);
        overlay.Width = Ui.GetViewportWidth();
        overlay.Height = Ui.GetViewportHeight();
    }

    private void Close(bool accepted)
    {
        if (!IsOpen)
        {
            return;
        }
        Hide();
        var result = pendingResult;
        pendingResult = null;
        result?.TrySetResult(accepted);
        if (accepted)
        {
            Accepted?.Invoke();
        }
        else
        {
            Cancelled?.Invoke();
        }
    }

    private void Refresh()
    {
        var theme = Theme.Active.Value;
        overlay.Background = backdropColor ?? theme.Colors.DialogBackdrop;
        overlay.BackgroundBlur = backgroundBlur;
        card.Background = cardColor ?? theme.Colors.Surface;
        card.CornerRadius = cardCornerRadius ?? new CornerRadius(theme.Spacing.Md);
        card.Border = cardBorder ?? new Border(1.0f, theme.Colors.Border);
        card.DropShadow = cardShadow ?? new DropShadow(theme.Colors.DialogShadow, 0.0f, 8.0f, 10.0f);
        card.Padding = new Thickness(CardPadding);
        title.FontFamily = theme.Fonts.HeadingFamily;
        title.FontSize = theme.Fonts.SizeHeading;
        title.TextColor = theme.Colors.TextPrimary;
        body.FontFamily = theme.Fonts.BodyFamily;
        body.FontSize = theme.Fonts.SizeBody;
        body.TextColor = theme.Colors.TextMuted;
    }
}
