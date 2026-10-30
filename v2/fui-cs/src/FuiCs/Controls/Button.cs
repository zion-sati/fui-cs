namespace FuiCs;

/// <summary>
/// A push button. A primary click, or Enter or Space while it has focus, raises <see cref="Click"/>; a press
/// released outside the button does not. Its chrome follows the theme and <see cref="Colors"/>; setting a chrome property overrides
/// that property only.
/// </summary>
public class Button : FlexBox
{
    private const float DisabledOpacity = 0.38f;

    private ButtonPresenter presenter;
    private string label;
    private ButtonColors? colors;
    private ButtonTemplate? template;
    private bool hovered;
    private bool pressed;
    private bool pointerDown;
    private bool focused;
    private string? keyboardKey;
    private float enabledOpacity = 1.0f;
    private Color? background;
    private Color? hoverBackground;
    private Color? pressedBackground;
    private CornerRadius? cornerRadius;
    private Border? border;
    private DropShadow? dropShadow;
    private Thickness? padding;
    private FontFamily? fontFamily;
    private FontWeight? fontWeight;
    private FontStyle? fontStyle;
    private float? fontSize;
    private Color? textColor;

    /// <summary>A button showing <paramref name="label"/>.</summary>
    public Button(string label = "")
    {
        ArgumentNullException.ThrowIfNull(label);
        this.label = label;
        presenter = ButtonTemplate.Default.CreatePresenter();
        presenter.Label.Text = label;
        AddChild(presenter.ContentRoot);
        PointerClick += _ => Click?.Invoke(ClickEventArgs.Empty);
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => SyncFocusRing()));
        SemanticRole = FuiCs.SemanticRole.Button;
        SemanticLabel = label;
        Cursor = CursorStyle.Pointer;
        SetFocusable(true, 0);
        RequireInteractive();
        FlexDirection = FuiCs.FlexDirection.Row;
        JustifyContent = FuiCs.JustifyContent.Center;
        AlignItems = FuiCs.AlignItems.Center;
        Refresh();
    }

    /// <summary>Raised when the button is activated by pointer or keyboard.</summary>
    public event Action<ClickEventArgs>? Click;

    /// <summary>Raised when the pointer enters or leaves the button.</summary>
    public event Action<HoverChangedEventArgs>? HoverChanged;

    /// <summary>The label's text.</summary>
    public string Label
    {
        get => label;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            label = value;
            SemanticLabel = value;
            presenter.Label.Text = value;
        }
    }

    /// <summary>Colors over the theme's for each state; null follows the theme.</summary>
    public ButtonColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            Refresh();
        }
    }

    /// <summary>Builds the button's content; null uses <see cref="ButtonTemplate.Default"/>.</summary>
    public ButtonTemplate? Template
    {
        get => template;
        set
        {
            template = value;
            ReplacePresenter((value ?? ButtonTemplate.Default).CreatePresenter());
        }
    }

    /// <summary>Whether the pointer is over the button.</summary>
    public bool IsHovered => hovered;

    /// <summary>Whether the button is pressed.</summary>
    public bool IsPressed => pressed;

    /// <summary>The fill at rest; until set, the colors' background or the theme's accent.</summary>
    public override Color Background
    {
        get => base.Background;
        set
        {
            background = value;
            Refresh();
        }
    }

    /// <summary>The fill under the pointer; when null, the colors' hover fill or the theme's hovered accent.</summary>
    public Color? HoverBackground
    {
        get => hoverBackground;
        set
        {
            hoverBackground = value;
            Refresh();
        }
    }

    /// <summary>The fill while pressed; when null, the colors' pressed fill or the theme's pressed accent.</summary>
    public Color? PressedBackground
    {
        get => pressedBackground;
        set
        {
            pressedBackground = value;
            Refresh();
        }
    }

    /// <inheritdoc/>
    public override CornerRadius CornerRadius
    {
        get => base.CornerRadius;
        set
        {
            cornerRadius = value;
            Refresh();
        }
    }

    /// <inheritdoc/>
    public override Border Border
    {
        get => base.Border;
        set
        {
            border = value;
            Refresh();
        }
    }

    /// <inheritdoc/>
    public override DropShadow DropShadow
    {
        get => base.DropShadow;
        set
        {
            dropShadow = value;
            Refresh();
        }
    }

    /// <inheritdoc/>
    public override Thickness Padding
    {
        get => base.Padding;
        set
        {
            padding = value;
            Refresh();
        }
    }

    /// <summary>The label's font family; null follows the theme's body family.</summary>
    public FontFamily? FontFamily
    {
        get => fontFamily;
        set
        {
            fontFamily = value;
            Refresh();
        }
    }

    /// <summary>The label's weight; the theme's when null.</summary>
    public FontWeight? FontWeight
    {
        get => fontWeight;
        set
        {
            fontWeight = value;
            Refresh();
        }
    }

    /// <summary>The label's style; upright when null.</summary>
    public FontStyle? FontStyle
    {
        get => fontStyle;
        set
        {
            fontStyle = value;
            Refresh();
        }
    }

    /// <summary>The label's size; the theme's body size when null.</summary>
    public float? FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            Refresh();
        }
    }

    /// <summary>The label color; null follows <see cref="Colors"/> and the theme.</summary>
    public Color? TextColor
    {
        get => textColor;
        set
        {
            textColor = value;
            Refresh();
        }
    }

    /// <summary>Raises <see cref="Click"/> as if the user activated the button.</summary>
    public void PerformClick() => Click?.Invoke(ClickEventArgs.Empty);

    /// <summary>Shows the button pressed for a key another control handles, such as a form's Enter.</summary>
    internal void BeginPress() => SetPressed(true);

    /// <summary>Releases a <see cref="BeginPress"/>, raising <see cref="Click"/> when <paramref name="fire"/> is set.</summary>
    internal void EndPress(bool fire)
    {
        if (!pressed)
        {
            return;
        }
        SetPressed(false);
        if (fire)
        {
            Click?.Invoke(ClickEventArgs.Empty);
        }
    }

    /// <summary>
    /// Tracks hover and the pressed look. The host keeps routing a pressed pointer here after it leaves, so the button
    /// looks pressed only while that pointer is over it; the click itself comes from the node's click recognition.
    /// </summary>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }
        base.HandlePointerEvent(e);
        switch (e.EventType)
        {
            case PointerEventType.Enter:
                SetHovered(true);
                break;
            case PointerEventType.Leave:
                SetHovered(false);
                EndPointerPress();
                break;
            case PointerEventType.Down when e.IsPrimaryActivation:
                pointerDown = true;
                SetHovered(true);
                SetPressed(true);
                break;
            case PointerEventType.Move when pointerDown && keyboardKey is null:
                SetPressed(GetBounds().Contains(e.SceneX, e.SceneY));
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
        if (!IsEnabled || modifiers != KeyModifier.None || !IsActivationKey(key))
        {
            return handled;
        }
        if (eventType == KeyEventType.Down)
        {
            if (keyboardKey is null)
            {
                keyboardKey = key;
                SetPressed(true);
            }
            return true;
        }
        if (eventType == KeyEventType.Up && keyboardKey == key)
        {
            keyboardKey = null;
            SetPressed(false);
            Click?.Invoke(ClickEventArgs.Empty);
            return true;
        }
        return handled;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        if (!focused && keyboardKey is not null)
        {
            keyboardKey = null;
            SetPressed(false);
        }
        var next = focused && IsEnabled;
        if (this.focused != next)
        {
            this.focused = next;
            Refresh();
        }
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        if (!isEnabled)
        {
            enabledOpacity = Opacity;
            keyboardKey = null;
            pointerDown = false;
            pressed = false;
            focused = false;
            SetHovered(false);
            base.Opacity = DisabledOpacity;
        }
        else
        {
            base.Opacity = enabledOpacity;
        }
        Cursor = isEnabled ? CursorStyle.Pointer : CursorStyle.Default;
        Refresh();
    }

    private static bool IsActivationKey(string key) => key is "Enter" or " " or "Space" or "Spacebar";

    private void SetHovered(bool next)
    {
        if (hovered == next)
        {
            return;
        }
        hovered = next;
        Refresh();
        HoverChanged?.Invoke(new HoverChangedEventArgs(next));
    }

    private void SetPressed(bool next)
    {
        if (pressed != next)
        {
            pressed = next;
            Refresh();
        }
    }

    private void EndPointerPress()
    {
        pointerDown = false;
        if (keyboardKey is null)
        {
            SetPressed(false);
        }
    }

    private void ReplacePresenter(ButtonPresenter next)
    {
        next.Label.Text = label;
        var previous = presenter;
        presenter = next;
        if (ReferenceEquals(previous, next))
        {
            return;
        }
        AddChild(next.ContentRoot);
        RemoveChild(previous.ContentRoot);
        previous.ContentRoot.Dispose();
        Refresh();
    }

    /// <summary>Applies the template, then the chrome and label style from the overrides, colors, theme and state.</summary>
    private void Refresh()
    {
        var theme = Theme.Active.Value;
        var enabled = IsEnabled;
        presenter.Apply(theme, new ButtonVisualState(hovered, pressed, focused, enabled), colors);
        base.Background = CurrentBackground(theme, enabled);
        base.CornerRadius = cornerRadius ?? new CornerRadius(theme.Spacing.Sm);
        base.Border = border ?? new Border(1.0f, colors?.Border ?? theme.Colors.Border);
        base.DropShadow = dropShadow ?? default;
        base.Padding = padding ?? new Thickness(theme.Spacing.Md, theme.Spacing.Sm, theme.Spacing.Md, theme.Spacing.Sm);
        var text = presenter.Label;
        text.FontFamily = fontFamily ?? theme.Fonts.BodyFamily;
        text.FontWeight = fontWeight ?? FuiCs.FontWeight.Regular;
        text.FontStyle = fontStyle ?? FuiCs.FontStyle.Normal;
        text.FontSize = fontSize ?? theme.Fonts.SizeBody;
        text.TextColor = textColor ?? (!enabled && colors?.TextMuted is { } muted ? muted : colors?.TextPrimary ?? theme.Colors.TextOnAccent);
        SyncFocusRing();
    }

    /// <summary>Shows the focus ring while the button has focus, is enabled and focus is visible.</summary>
    private void SyncFocusRing()
    {
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, base.CornerRadius);
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }

    private Color CurrentBackground(Theme theme, bool enabled)
    {
        var normal = background ?? colors?.Background ?? theme.Colors.Accent;
        if (!enabled)
        {
            return normal;
        }
        if (pressed)
        {
            return pressedBackground ?? colors?.BackgroundPressed ?? colors?.BackgroundHover ?? colors?.Background ?? theme.Colors.AccentPressed;
        }
        if (hovered)
        {
            return hoverBackground ?? colors?.BackgroundHover ?? colors?.Background ?? theme.Colors.AccentHovered;
        }
        return normal;
    }
}
