namespace FuiCs;

/// <summary>
/// A checkbox. Activating it toggles <see cref="IsChecked"/>; with <see cref="IsThreeState"/> it cycles through
/// checked, indeterminate (null) and unchecked, as in WPF. Its state persists across history navigation.
/// </summary>
public class Checkbox : PressableLabeledControl
{
    private static readonly PersistedValue<Checkbox, int> PersistedChecked = new(
        "checkbox-checked-state",
        checkbox => (int)ToSemantic(checkbox.isChecked),
        (checkbox, state) => checkbox.SetChecked(
            state switch { (int)SemanticCheckedState.True => true, (int)SemanticCheckedState.Mixed => null, _ => false },
            announce: false),
        PersistedCodecs.Int32);

    private bool? isChecked = false;

    /// <summary>A checkbox labelled <paramref name="label"/>, unchecked.</summary>
    public Checkbox(string label = "")
        : base(SemanticRole.Checkbox, label, DefaultTemplate)
    {
        SemanticChecked = SemanticCheckedState.False;
        RegisterPersistedState(PersistedChecked);
    }

    /// <summary>A rounded box with a check mark.</summary>
    public static IndicatorTemplate DefaultTemplate { get; } = new(CheckboxIndicator.Create);

    /// <summary>Raised when <see cref="IsChecked"/> changes, by the user or from code.</summary>
    public event Action<CheckboxChangedEventArgs>? Changed;

    /// <summary>Whether the box is checked; null is indeterminate, which only a three-state box keeps (null unchecks any other).</summary>
    public bool? IsChecked
    {
        get => isChecked;
        set => SetChecked(value, announce: false);
    }

    /// <summary>Whether activating the checkbox passes through the indeterminate state. Turning it off unchecks an indeterminate box.</summary>
    public bool IsThreeState
    {
        get;
        set
        {
            field = value;
            if (!value && isChecked is null)
            {
                SetChecked(false, announce: false);
            }
        }
    }

    /// <inheritdoc/>
    protected override bool? IndicatorChecked => isChecked;

    /// <inheritdoc/>
    protected override void OnActivated() =>
        SetChecked(isChecked switch { false => true, true when IsThreeState => null, _ => false }, announce: true);

    private static SemanticCheckedState ToSemantic(bool? value) => value switch
    {
        true => SemanticCheckedState.True,
        false => SemanticCheckedState.False,
        null => SemanticCheckedState.Mixed,
    };

    private void SetChecked(bool? value, bool announce)
    {
        if (value is null && !IsThreeState)
        {
            value = false;
        }
        if (isChecked == value)
        {
            return;
        }
        isChecked = value;
        SemanticChecked = ToSemantic(value);
        Refresh();
        if (announce)
        {
            RequestSemanticAnnouncement();
        }
        Changed?.Invoke(new CheckboxChangedEventArgs(value));
    }
}
