namespace FuiCs;

/// <summary>
/// The base of checkboxes, radio buttons and switches: an indicator and a label in a row. A primary click on it, or
/// Space while it has focus, activates it and raises <see cref="Click"/>.
/// </summary>
public abstract class PressableLabeledControl : FlexBox
{
    private const float DisabledOpacity = 0.6f;

    private readonly TextCore label;
    private readonly FlexBox gap;
    private readonly FlexBox labelHost;
    private readonly IndicatorTemplate defaultTemplate;
    private IndicatorPresenter indicator;
    private IndicatorTemplate? template;
    private LabeledControlSizing? sizing;
    private LabeledControlColors? colors;
    private bool hovered;
    private bool pressed;
    private bool focused;
    private bool pointerDown;
    private bool keyDown;

    /// <summary>
    /// A control with the <paramref name="role"/> semantics, labelled <paramref name="label"/>, drawing its indicator
    /// with <paramref name="defaultTemplate"/> until another is set.
    /// </summary>
    protected PressableLabeledControl(SemanticRole role, string label, IndicatorTemplate defaultTemplate)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(defaultTemplate);
        this.defaultTemplate = defaultTemplate;
        this.label = new TextCore(label);
        gap = new FlexBox().Height(1.0f);
        labelHost = new FlexBox().Child(this.label);
        indicator = CreateIndicator();
        SemanticRole = role;
        SemanticLabel = label;
        SetFocusable(true, 0);
        RequireInteractive();
        ReflectSemanticDisabledFromEnabled();
        FlexDirection = FuiCs.FlexDirection.Row;
        AlignItems = FuiCs.AlignItems.Center;
        AddChild(indicator.Root);
        AddChild(gap);
        AddChild(labelHost);
        PointerClick += _ => Activate();
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => Refresh()));
        Refresh();
    }

    /// <summary>Raised after the user activates the control by pointer or keyboard.</summary>
    public event Action<ClickEventArgs>? Click;

    /// <summary>The label's text.</summary>
    public string Label
    {
        get => label.Text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            label.Text = value;
            SemanticLabel = value;
        }
    }

    /// <summary>Colors over the theme's for the indicator and label.</summary>
    public LabeledControlColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            Refresh();
        }
    }

    /// <summary>The indicator and label sizes; the defaults when null.</summary>
    public LabeledControlSizing? Sizing
    {
        get => sizing;
        set
        {
            sizing = value;
            ReplaceIndicator();
        }
    }

    /// <summary>Builds the indicator; null uses the control's default template.</summary>
    public IndicatorTemplate? Template
    {
        get => template;
        set
        {
            template = value;
            ReplaceIndicator();
        }
    }

    /// <summary>Whether the pointer is over the control.</summary>
    public bool IsHovered => hovered;

    /// <summary>Whether the control is pressed.</summary>
    public bool IsPressed => pressed;

    /// <summary>What the indicator shows as checked; null for indeterminate.</summary>
    protected abstract bool? IndicatorChecked { get; }

    /// <summary>Changes the control's value for a user activation, before <see cref="Click"/> is raised.</summary>
    protected abstract void OnActivated();

    /// <summary>Restyles the control and its indicator for the current theme, colors and state.</summary>
    protected void Refresh()
    {
        var theme = Theme.Active.Value;
        var enabled = IsEnabled;
        Cursor = enabled ? CursorStyle.Pointer : CursorStyle.Default;
        CornerRadius = new CornerRadius(theme.Spacing.Sm);
        Border = new Border(2.0f, Color.Transparent);
        Padding = new Thickness(theme.Spacing.Xs);
        Opacity = enabled ? 1.0f : DisabledOpacity;
        gap.Width = theme.Spacing.Sm;
        label.FontFamily = theme.Fonts.BodyFamily;
        label.FontSize = sizing?.LabelFontSize ?? theme.Fonts.SizeBody;
        label.TextColor = enabled ? colors?.TextPrimary ?? theme.Colors.TextPrimary : colors?.TextMuted ?? theme.Colors.TextMuted;
        indicator.Apply(theme, new IndicatorVisualState(IndicatorChecked, hovered, pressed, focused, enabled), colors);
        if (focused && enabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(theme.Spacing.Sm));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }

    /// <summary>Tracks hover and the pressed look; the host keeps routing a pressed pointer here after it leaves.</summary>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        if (!IsEnabled)
        {
            return;
        }
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                SetState(ref hovered, true);
                break;
            case PointerEventType.Leave:
                SetState(ref hovered, false);
                EndPointerPress();
                break;
            case PointerEventType.Down when e.IsPrimaryActivation:
                pointerDown = true;
                SetState(ref pressed, true);
                break;
            case PointerEventType.Move when pointerDown && !keyDown:
                SetState(ref pressed, GetBounds().Contains(e.SceneX, e.SceneY));
                break;
            case PointerEventType.Up:
            case PointerEventType.Cancel:
                EndPointerPress();
                break;
        }
    }

    /// <inheritdoc/>
    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (!IsEnabled || modifiers != KeyModifier.None || key is not (" " or "Space" or "Spacebar"))
        {
            return handled;
        }
        if (eventType == KeyEventType.Down)
        {
            keyDown = true;
            SetState(ref pressed, true);
            return true;
        }
        if (eventType == KeyEventType.Up && keyDown)
        {
            keyDown = false;
            SetState(ref pressed, false);
            Activate();
            return true;
        }
        return handled;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        if (this.focused == focused)
        {
            return;
        }
        this.focused = focused;
        if (!focused)
        {
            keyDown = false;
            pressed = false;
        }
        Refresh();
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        pointerDown = false;
        keyDown = false;
        pressed = false;
        if (!isEnabled)
        {
            hovered = false;
        }
        Refresh();
    }

    private IndicatorPresenter CreateIndicator() => (template ?? defaultTemplate).CreatePresenter(sizing);

    private void ReplaceIndicator()
    {
        var previous = indicator;
        indicator = CreateIndicator();
        ReplaceChildren([indicator.Root, gap, labelHost]);
        previous.Root.Dispose();
        Refresh();
    }

    private void Activate()
    {
        OnActivated();
        Click?.Invoke(ClickEventArgs.Empty);
    }

    private void EndPointerPress()
    {
        pointerDown = false;
        if (!keyDown)
        {
            SetState(ref pressed, false);
        }
    }

    private void SetState(ref bool state, bool value)
    {
        if (state != value)
        {
            state = value;
            Refresh();
        }
    }
}
