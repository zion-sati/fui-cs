using System;
using System.Globalization;
using System.Collections.Generic;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.TemplatedControls;

/// <summary>Keeps the live summary in step with the house controls and recolors the tinted ones with the theme.</summary>
internal sealed class TemplatedControlsController : IRoutePageLifecycleOwner
{
    private int houseButtonClicks;

    public TemplatedControlsController()
    {
        View.HouseButton.Click += _ =>
        {
            houseButtonClicks += 1;
            SyncSummary();
        };
        View.HouseCheckbox.Changed += _ => SyncSummary();
        View.OverrideCheckbox.Changed += _ => SyncSummary();
        View.HouseSwitch.Changed += _ => SyncSummary();
        View.ModeGroup.Changed += _ => SyncSummary();
        View.IntensitySlider.Changed += _ => SyncSummary();
        SyncSummary();
    }

    public TemplatedControlsView View { get; } = new();

    public IReadOnlyList<RoutePageSection> BuildSections() => View.BuildSections();

    public void ApplyTheme(Theme theme) => View.ApplyTheme(theme);

    public void Dispose()
    {
    }

    private static string CheckedLabel(Checkbox control) => control.IsChecked switch
    {
        null => "mixed",
        true => "on",
        false => "off",
    };

    private void SyncSummary()
    {
        View.DefaultsStatusText.Text = string.Create(
            CultureInfo.InvariantCulture,
            $"Defaults: button taps {houseButtonClicks}, checkbox {CheckedLabel(View.HouseCheckbox)}, switch {(View.HouseSwitch.IsChecked ? "on" : "off")}, mode {View.ModeGroup.SelectedValue}, intensity {Math.Round(View.IntensitySlider.Value)}.");
        View.OverrideStatusText.Text = "Override checkbox stays " + CheckedLabel(View.OverrideCheckbox) + " with its distinct per-control template.";
    }
}
