namespace FuiCs;

/// <summary>An on/off switch. Activating it toggles <see cref="IsChecked"/>; its state persists across history navigation.</summary>
public class Switch : PressableLabeledControl
{
    private static readonly PersistedValue<Switch, bool> PersistedChecked = new(
        "switch-checked",
        control => control.isChecked,
        (control, value) => control.SetChecked(value, announce: false),
        PersistedCodecs.Boolean);

    private bool isChecked;

    /// <summary>A switch labelled <paramref name="label"/>, off.</summary>
    public Switch(string label = "")
        : base(SemanticRole.Switch, label, DefaultTemplate)
    {
        SemanticChecked = SemanticCheckedState.False;
        RegisterPersistedState(PersistedChecked);
    }

    /// <summary>A pill track with a sliding thumb.</summary>
    public static IndicatorTemplate DefaultTemplate { get; } = new(SwitchIndicator.Create);

    /// <summary>Raised when <see cref="IsChecked"/> changes, by the user or from code.</summary>
    public event Action<SwitchChangedEventArgs>? Changed;

    /// <summary>Whether the switch is on.</summary>
    public bool IsChecked
    {
        get => isChecked;
        set => SetChecked(value, announce: false);
    }

    /// <inheritdoc/>
    protected override bool? IndicatorChecked => isChecked;

    /// <inheritdoc/>
    protected override void OnActivated() => SetChecked(!isChecked, announce: true);

    private void SetChecked(bool value, bool announce)
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
        Changed?.Invoke(new SwitchChangedEventArgs(value));
    }
}
