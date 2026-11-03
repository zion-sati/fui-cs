namespace FuiCs;

/// <summary>
/// Picks a value between <see cref="Minimum"/> and <see cref="Maximum"/>, snapped to <see cref="Step"/>, by pressing or
/// dragging along the track, or with the arrow keys, Home and End while focused. A vertical slider increases upwards.
/// Its value persists across history navigation.
/// </summary>
public class Slider : FlexBox
{
    private const float FocusBorderWidth = 2.0f;
    private const float ChromePadding = 2.0f;
    private const float ContentInset = 1.0f;

    /// <summary>From the slider's outer edge to its parts: the focus border, padding and content inset.</summary>
    private const float OuterInset = FocusBorderWidth + ChromePadding + ContentInset;

    private static readonly PersistedValue<Slider, float> PersistedValue = new(
        "slider-value",
        slider => slider.value,
        (slider, value) => slider.SetValue(value, announce: false),
        PersistedCodecs.Single);

    private readonly DragGesture drag;
    private SliderPresenter parts;
    private SliderTemplate? template;
    private SliderSizing? sizing;
    private SliderColors? colors;
    private float minimum;
    private float maximum = 100.0f;
    private float step = 1.0f;
    private float value;
    private float length = 180.0f;
    private bool fillsLength;
    private Orientation orientation = FuiCs.Orientation.Horizontal;
    private bool hovered;
    private bool focused;

    /// <summary>A slider at <paramref name="value"/>, from 0 to 100 in steps of 1.</summary>
    public Slider(float value = 0.0f)
    {
        drag = new DragGesture(this).Threshold(0.0f);
        drag.Started += e => UpdateFromPointer(e.X, e.Y);
        drag.Delta += e => UpdateFromPointer(e.X, e.Y);
        drag.Completed += e =>
        {
            if (!e.Cancelled)
            {
                UpdateFromPointer(e.X, e.Y);
            }
        };
        drag.DraggingChanged += _ => Refresh();
        parts = DefaultTemplate.CreatePresenter(null);
        AddChild(parts.Root);
        SemanticRole = FuiCs.SemanticRole.Slider;
        SemanticOrientation = orientation;
        DefaultSemanticLabel = "Slider";
        SetFocusable(true, 0);
        RequireInteractive();
        ReflectSemanticDisabledFromEnabled();
        RegisterPersistedState(PersistedValue);
        Track(Theme.Active.Subscribe(_ => Refresh()));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => Refresh()));
        this.value = Normalize(value);
        Refresh();
    }

    /// <summary>A rounded track with an accent fill and a round thumb.</summary>
    public static SliderTemplate DefaultTemplate { get; } = new(SliderParts.Create);

    /// <summary>Raised when <see cref="Value"/> changes, by the user or from code.</summary>
    public event Action<SliderChangedEventArgs>? Changed;

    /// <summary>The lowest value; raising it above <see cref="Maximum"/> raises the maximum too.</summary>
    public float Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            maximum = Math.Max(maximum, value);
            SetValue(this.value, announce: false);
        }
    }

    /// <summary>The highest value; lowering it below <see cref="Minimum"/> lowers the minimum too.</summary>
    public float Maximum
    {
        get => maximum;
        set
        {
            maximum = value;
            minimum = Math.Min(minimum, value);
            SetValue(this.value, announce: false);
        }
    }

    /// <summary>The spacing of the values the slider snaps to from <see cref="Minimum"/>, and the arrow keys' change.</summary>
    public float Step
    {
        get => step;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            step = value;
            SetValue(this.value, announce: false);
        }
    }

    /// <summary>The value, clamped to the range and snapped to <see cref="Step"/>.</summary>
    public float Value
    {
        get => value;
        set => SetValue(value, announce: false);
    }

    /// <summary>The length of the parts along the slider, in pixels; at least one pixel more than the thumb.</summary>
    public float Length
    {
        get => length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            length = value;
            fillsLength = false;
            Refresh();
        }
    }

    /// <summary>
    /// Whether the slider fills its container along its orientation, as <c>FillWidth</c> or <c>FillHeight</c> does,
    /// instead of a fixed <see cref="Length"/>. Setting <see cref="Length"/> returns to a fixed length.
    /// </summary>
    public bool FillsLength
    {
        get => fillsLength;
        set
        {
            fillsLength = value;
            Refresh();
        }
    }

    /// <summary>Whether the slider runs across or upwards.</summary>
    public Orientation Orientation
    {
        get => orientation;
        set
        {
            orientation = value == FuiCs.Orientation.Vertical ? FuiCs.Orientation.Vertical : FuiCs.Orientation.Horizontal;
            SemanticOrientation = orientation;
            DefaultSemanticLabel = orientation == FuiCs.Orientation.Vertical ? "Vertical slider" : "Slider";
            Refresh();
        }
    }

    /// <summary>Colors over the theme's for each part.</summary>
    public SliderColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            Refresh();
        }
    }

    /// <summary>The track and thumb sizes; the default template's when null.</summary>
    public SliderSizing? Sizing
    {
        get => sizing;
        set
        {
            sizing = value;
            ReplaceParts();
        }
    }

    /// <summary>Builds the track, fill and thumb; null uses <see cref="DefaultTemplate"/>.</summary>
    public SliderTemplate? Template
    {
        get => template;
        set
        {
            template = value;
            ReplaceParts();
        }
    }

    /// <summary>Whether the thumb is being dragged.</summary>
    public bool IsDragging => drag.IsDragging;

    /// <inheritdoc/>
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
                hovered = true;
                Refresh();
                break;
            case PointerEventType.Leave:
                hovered = false;
                Refresh();
                break;
            case PointerEventType.Down when e.IsPrimaryActivation:
                drag.HandlePointerDown(e.X, e.Y, e.Modifiers);
                break;
            case PointerEventType.Move:
                drag.HandlePointerMove(e.X, e.Y, e.Modifiers);
                break;
            case PointerEventType.Up:
                drag.HandlePointerUp(e.X, e.Y, e.Modifiers);
                break;
            case PointerEventType.Cancel:
                drag.Cancel();
                break;
        }
    }

    /// <inheritdoc/>
    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (!IsEnabled || eventType != KeyEventType.Down || modifiers != KeyModifier.None)
        {
            return handled;
        }
        var vertical = orientation == FuiCs.Orientation.Vertical;
        float? next = key switch
        {
            "Home" => minimum,
            "End" => maximum,
            "ArrowRight" when !vertical => value + step,
            "ArrowLeft" when !vertical => value - step,
            "ArrowUp" when vertical => value + step,
            "ArrowDown" when vertical => value - step,
            _ => null,
        };
        if (next is not { } target)
        {
            return handled;
        }
        SetValue(target, announce: true);
        return true;
    }

    /// <inheritdoc/>
    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        this.focused = focused;
        Refresh();
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        if (!isEnabled)
        {
            drag.Cancel();
            hovered = false;
        }
        Refresh();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        drag.Cancel();
        base.Dispose();
    }

    private float EffectiveLength => Math.Max(length, parts.ThumbSize + 1.0f);

    private float Normalize(float target)
    {
        var clamped = Math.Clamp(target, minimum, maximum);
        var snapped = MathF.Round((clamped - minimum) / step) * step;
        return Math.Clamp(minimum + snapped, minimum, maximum);
    }

    private void SetValue(float target, bool announce)
    {
        var normalized = Normalize(target);
        if (normalized == value)
        {
            Refresh();
            return;
        }
        value = normalized;
        Refresh();
        if (announce)
        {
            RequestSemanticAnnouncement();
        }
        Changed?.Invoke(new SliderChangedEventArgs(value));
    }

    /// <summary>Sets the value under a node-local pointer position, measured from the thumb's center at either end.</summary>
    private void UpdateFromPointer(float x, float y)
    {
        var vertical = orientation == FuiCs.Orientation.Vertical;
        // A filling slider's length is whatever layout gave it.
        var bounds = fillsLength ? GetBounds() : default;
        var laidOut = (vertical ? bounds.Height : bounds.Width) - (OuterInset * 2.0f);
        var trackLength = fillsLength && laidOut > 0.0f ? laidOut : EffectiveLength;
        var available = trackLength - parts.ThumbSize;
        if (available <= 0.0f)
        {
            return;
        }
        var leading = OuterInset + (parts.ThumbSize * 0.5f);
        var offset = vertical
            ? available - Math.Clamp(y - leading, 0.0f, available)
            : Math.Clamp(x - leading, 0.0f, available);
        SetValue(minimum + ((maximum - minimum) * (offset / available)), announce: true);
    }

    private void ReplaceParts()
    {
        var previous = parts;
        parts = (template ?? DefaultTemplate).CreatePresenter(sizing);
        ReplaceChildren([parts.Root]);
        previous.Root.Dispose();
        Refresh();
    }

    private void Refresh()
    {
        var theme = Theme.Active.Value;
        var range = maximum - minimum;
        var state = new SliderVisualState(
            value,
            minimum,
            maximum,
            range > 0.0f ? Math.Clamp((value - minimum) / range, 0.0f, 1.0f) : 0.0f,
            orientation,
            hovered,
            drag.IsDragging,
            focused,
            IsEnabled);
        var along = EffectiveLength + (OuterInset * 2.0f);
        var across = parts.ThumbSize + parts.CrossAxisExtra + (OuterInset * 2.0f);
        if (orientation == FuiCs.Orientation.Vertical)
        {
            Width = across;
            if (fillsLength)
            {
                FillHeight = true;
            }
            else
            {
                Height = along;
            }
        }
        else
        {
            Height = across;
            if (fillsLength)
            {
                FillWidth = true;
            }
            else
            {
                Width = along;
            }
        }
        // The parts fill the slider inside its focus chrome.
        parts.Root.FillSize().Margin(ContentInset);
        parts.Layout(state);
        SemanticValueRange = new SemanticValueRange(value, minimum, maximum);
        Cursor = IsEnabled ? CursorStyle.Pointer : CursorStyle.Default;
        CornerRadius = new CornerRadius(theme.Spacing.Sm);
        Border = new Border(FocusBorderWidth, theme.Colors.Background);
        Padding = new Thickness(ChromePadding);
        Opacity = IsEnabled ? 1.0f : 0.6f;
        parts.Apply(theme, state, colors);
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(theme.Spacing.Sm));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }
}
