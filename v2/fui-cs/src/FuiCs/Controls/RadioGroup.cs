namespace FuiCs;

/// <summary>
/// A column of radio buttons of which at most one is selected. Its selection persists across history navigation.
/// </summary>
public class RadioGroup : FlexBox
{
    private static readonly PersistedValue<RadioGroup, int> PersistedSelection = new(
        "radio-group-selected-index",
        group => group.selectedIndex,
        (group, index) =>
        {
            if (index >= 0 && index < group.radios.Count)
            {
                group.Select(index, focus: false, announce: false);
            }
        },
        PersistedCodecs.Int32)
    {
        ShouldCapture = group => group.selectedIndex >= 0,
    };

    private readonly List<RadioButton> radios = [];
    private int selectedIndex = -1;

    /// <summary>An empty group; add radios to it.</summary>
    public RadioGroup()
    {
        SemanticRole = FuiCs.SemanticRole.RadioGroup;
        FlexDirection = FuiCs.FlexDirection.Column;
        RegisterPersistedState(PersistedSelection);
    }

    /// <summary>Raised when the selection changes, by the user or from code.</summary>
    public event Action<RadioGroupChangedEventArgs>? Changed;

    /// <summary>The group's radios in order.</summary>
    public IReadOnlyList<RadioButton> Radios => radios;

    /// <summary>The selected radio's index, or -1 for none. Selecting a disabled radio leaves the selection as it is.</summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, -1);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, radios.Count);
            Select(value, focus: false, announce: false);
        }
    }

    /// <summary>The selected radio, if any.</summary>
    public RadioButton? SelectedRadio => selectedIndex >= 0 ? radios[selectedIndex] : null;

    /// <summary>The selected radio's value, if any.</summary>
    public string? SelectedValue => SelectedRadio?.Value;

    /// <summary>Adds <paramref name="radio"/> as the group's last radio; a checked radio becomes the selection if there is none.</summary>
    public void AddRadio(RadioButton radio)
    {
        ArgumentNullException.ThrowIfNull(radio);
        if (radio.Group is not null)
        {
            throw new InvalidOperationException("The radio button already belongs to a group.");
        }
        radio.Group = this;
        radios.Add(radio);
        AddChild(radio);
        if (!radio.IsChecked)
        {
            return;
        }
        if (selectedIndex < 0)
        {
            selectedIndex = radios.Count - 1;
        }
        else
        {
            radio.SetChecked(false, announce: false);
        }
    }

    /// <summary>Adds a radio for <paramref name="value"/>, labeled with <paramref name="label"/> or the value, and returns it.</summary>
    public RadioButton AddOption(string value, string? label = null)
    {
        var radio = new RadioButton(value, label);
        AddRadio(radio);
        return radio;
    }

    internal void Select(RadioButton radio, bool focus, bool announce) => Select(radios.IndexOf(radio), focus, announce);

    internal void SetChecked(RadioButton radio, bool value)
    {
        var index = radios.IndexOf(radio);
        if (value)
        {
            Select(index, focus: false, announce: false);
        }
        else if (index == selectedIndex)
        {
            Select(-1, focus: false, announce: false);
        }
    }

    /// <summary>Selects the next enabled radio after <paramref name="from"/> in <paramref name="step"/>'s direction, wrapping around.</summary>
    internal void MoveSelection(RadioButton from, int step)
    {
        var index = radios.IndexOf(from);
        for (var tried = 0; tried < radios.Count; ++tried)
        {
            index = (index + step + radios.Count) % radios.Count;
            if (radios[index].IsEnabled)
            {
                Select(index, focus: true, announce: true);
                return;
            }
        }
    }

    internal void SelectBoundary(bool first)
    {
        for (var tried = 0; tried < radios.Count; ++tried)
        {
            var index = first ? tried : radios.Count - 1 - tried;
            if (radios[index].IsEnabled)
            {
                Select(index, focus: true, announce: true);
                return;
            }
        }
    }

    /// <summary>Selects the radio at <paramref name="index"/>, or none for -1; a disabled radio is not selected.</summary>
    private void Select(int index, bool focus, bool announce)
    {
        if (index >= 0 && !radios[index].IsEnabled)
        {
            return;
        }
        if (index == selectedIndex)
        {
            if (focus && index >= 0)
            {
                radios[index].Focus();
            }
            return;
        }
        var previous = selectedIndex;
        selectedIndex = index;
        if (previous >= 0)
        {
            radios[previous].SetChecked(false, announce: false);
        }
        if (index >= 0)
        {
            radios[index].SetChecked(true, announce);
            if (focus)
            {
                radios[index].Focus();
            }
        }
        Changed?.Invoke(new RadioGroupChangedEventArgs(index, SelectedValue));
    }
}
