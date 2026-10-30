using FuiCs;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>The dashboard's state: the shell's clock tick, the samples' counters and the common controls' values.</summary>
internal sealed class DashboardModel(bool initialDarkMode)
{
    public Signal<int> ClockTick { get; } = new(0);

    public Signal<int> ClickCount { get; } = new(0);

    public Signal<bool> CounterHovered { get; } = new(false);

    public Signal<bool> KeyTargetFocused { get; } = new(false);

    public Signal<string> LastKey { get; } = new("(none)");

    public Signal<bool> FoundationsScopeEnabled { get; } = new(true);

    public Signal<bool> FoundationsScopedFocused { get; } = new(false);

    public Signal<int> FoundationsScopedActionCount { get; } = new(0);

    public bool DarkMode { get; set; } = initialDarkMode;

    public bool? CommonCheckboxState { get; set; } = false;

    public bool? CommonTriStateValue { get; set; }

    public bool CommonSwitchValue { get; set; } = true;

    public int CommonSwitchClickCount { get; set; }

    public string CommonRadioValue { get; set; } = "system";

    public float CommonHorizontalSliderValue { get; set; } = 40.0f;

    public float CommonVerticalSliderValue { get; set; } = 60.0f;

    public string CommonDropdownValue { get; set; } = "Balanced";

    public string CommonComboBoxValue { get; set; } = "Melbourne";

    public string CommonComboBoxText { get; set; } = "Melbourne";

    public string CommonTextInputValue { get; set; } = "";

    public int CommonTextInputSelectionStart { get; set; }

    public int CommonTextInputSelectionEnd { get; set; }

    public bool CommonTextInputFocused { get; set; }
}
