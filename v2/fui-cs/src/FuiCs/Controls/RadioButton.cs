namespace FuiCs;

/// <summary>
/// A radio button with a <see cref="Value"/>. In a <see cref="RadioGroup"/>, activating it selects it and the arrow
/// keys, Home and End move the selection between the group's enabled radios; on its own, activating it checks it.
/// </summary>
public class RadioButton : PressableLabeledControl
{
    private bool isChecked;

    /// <summary>A radio button for <paramref name="value"/>, labelled <paramref name="label"/> or the value.</summary>
    public RadioButton(string value, string? label = null)
        : base(SemanticRole.Radio, label ?? value ?? throw new ArgumentNullException(nameof(value)), DefaultTemplate)
    {
        Value = value;
        SemanticChecked = SemanticCheckedState.False;
    }

    /// <summary>A ring with a centered dot.</summary>
    public static IndicatorTemplate DefaultTemplate { get; } = new(RadioIndicator.Create);

    /// <summary>Raised when <see cref="IsChecked"/> changes, by the user, the group or code.</summary>
    public event Action<RadioButtonChangedEventArgs>? Changed;

    /// <summary>The value the radio stands for.</summary>
    public string Value { get; }

    /// <summary>The group the radio was added to, if any.</summary>
    public RadioGroup? Group { get; internal set; }

    /// <summary>Whether the radio is checked; in a group, setting it selects the radio or clears the selection.</summary>
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (Group is { } group)
            {
                group.SetChecked(this, value);
            }
            else
            {
                SetChecked(value, announce: false);
            }
        }
    }

    /// <inheritdoc/>
    protected override bool? IndicatorChecked => isChecked;

    /// <inheritdoc/>
    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (!IsEnabled || eventType != KeyEventType.Down || modifiers != KeyModifier.None || Group is not { } group)
        {
            return handled;
        }
        switch (key)
        {
            case "ArrowLeft" or "ArrowUp":
                group.MoveSelection(this, -1);
                return true;
            case "ArrowRight" or "ArrowDown":
                group.MoveSelection(this, 1);
                return true;
            case "Home":
                group.SelectBoundary(first: true);
                return true;
            case "End":
                group.SelectBoundary(first: false);
                return true;
            default:
                return handled;
        }
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (Group is { } group)
        {
            group.Select(this, focus: true, announce: true);
        }
        else
        {
            SetChecked(true, announce: true);
        }
    }

    internal void SetChecked(bool value, bool announce)
    {
        if (isChecked == value)
        {
            return;
        }
        isChecked = value;
        SemanticChecked = value ? SemanticCheckedState.True : SemanticCheckedState.False;
        Refresh();
        if (announce)
        {
            RequestSemanticAnnouncement();
        }
        Changed?.Invoke(new RadioButtonChangedEventArgs(value));
    }
}
