using System;
using System.Collections.Generic;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>
/// Runs the dashboard: follows the shell's clock tick, accent and dark mode with the demo theme, counts the samples'
/// interactions, keeps the status lines in step, and makes the dashboard's context menu the application's.
/// </summary>
internal sealed class DashboardController : ManagedApplicationController
{
    private readonly List<IDisposable> subscriptions = [];
    private Color accentColor;

    public DashboardController()
    {
        Model = new DashboardModel(Theme.IsDarkMode);
        View = new DashboardView();
        accentColor = RoutePageController.ParseAccentColor(DemoShell.AccentColorHex(), Theme.Active.Value.Colors.Accent);
        DemoShell.ClockTickChanged += SetClockTick;
        DemoShell.AccentColorChanged += SetAccentColor;
        DemoShell.DarkModeChanged += SetDarkMode;
        WireViewEvents();
        ContextMenuManager.SetMenu(View.ContextMenu);
        AttachListeners();
        SyncTheme();
        RefreshStaticState();
    }

    public DashboardModel Model { get; }

    public DashboardView View { get; }

    public override Node GetRoot() => View.Root;

    public override void Mount()
    {
        Application.SetCaption("EffinDOM FUI-CS Demo • Dashboard");
        base.Mount();
        SyncStartupScrollMetrics();
    }

    public override void Dispose()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
        DemoShell.ClockTickChanged -= SetClockTick;
        DemoShell.AccentColorChanged -= SetAccentColor;
        DemoShell.DarkModeChanged -= SetDarkMode;
        View.Dispose();
        ContextMenuManager.SetMenu(null);
        Theme.UseSystem();
        base.Dispose();
    }

    private static string PadTwoDigits(int value) => value.ToString("00", CultureInfo.InvariantCulture);

    private static string Format(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Quoted(string value) => value.Length == 0 ? "<empty>" : "\"" + value + "\"";

    private static string CheckedLabel(bool? state) => state switch
    {
        true => "checked",
        null => "mixed",
        false => "unchecked",
    };

    private static void ApplyImageSize(Image image)
    {
        var ready = image.AssetState == AssetLoadState.Ready;
        image.Width(ready && image.AssetWidth > 0.0f ? image.AssetWidth : 16.0f).Height(ready && image.AssetHeight > 0.0f ? image.AssetHeight : 16.0f);
    }

    private static void ApplySvgSize(Svg svg)
    {
        var ready = svg.AssetState == AssetLoadState.Ready;
        svg.Width(ready && svg.AssetWidth > 0.0f ? svg.AssetWidth : 16.0f).Height(ready && svg.AssetHeight > 0.0f ? svg.AssetHeight : 16.0f);
    }

    private static (string Caption, DemoTextRecipe Recipe) MediaCaption(AssetLoadState state, string label) => state switch
    {
        AssetLoadState.Loading => (label + " (loading...)", DemoTextRecipe.StatusSupporting),
        AssetLoadState.Failed => (label + " failed", DemoTextRecipe.Error),
        _ => (label, DemoTextRecipe.Hint),
    };

    private void SetClockTick(int next) => Model.ClockTick.Value = next;

    private void SetAccentColor(uint next)
    {
        accentColor = new Color(next);
        SyncTheme();
    }

    private void SetDarkMode(bool flag)
    {
        if (Model.DarkMode == flag)
        {
            return;
        }
        Model.DarkMode = flag;
        SyncTheme();
    }

    /// <summary>Seeds the main column's and the nested sandbox's scroll metrics from their measured bounds.</summary>
    private void SyncStartupScrollMetrics()
    {
        var viewport = View.MainContentScrollBox.Viewport.GetBounds();
        var content = View.MainContentColumn.GetBounds();
        View.MainContentScrollState.ViewportWidth.Value = viewport.Width;
        View.MainContentScrollState.ViewportHeight.Value = viewport.Height;
        View.MainContentScrollState.ContentWidth.Value = content.Width;
        View.MainContentScrollState.ContentHeight.Value = content.Height;
        var nestedViewport = View.NestedScrollBox.Viewport.GetBounds();
        var nestedContent = View.NestedScrollContent.GetBounds();
        View.NestedScrollBox.ScrollState.ViewportWidth.Value = nestedViewport.Width;
        View.NestedScrollBox.ScrollState.ViewportHeight.Value = nestedViewport.Height;
        View.NestedScrollBox.ScrollState.ContentWidth.Value = nestedContent.Width;
        View.NestedScrollBox.ScrollState.ContentHeight.Value = nestedContent.Height;
    }

    private void WireViewEvents()
    {
        View.CounterButton.Click += _ => Model.ClickCount.Value += 1;
        View.CounterButton.HoverChanged += e => Model.CounterHovered.Value = e.Hovered;
        View.KeyTargetBox.FocusChanged += e => Model.KeyTargetFocused.Value = e.Focused;
        View.KeyTargetBox.KeyDown += e => Model.LastKey.Value = e.Key;
        View.DialogButton.Click += _ => OpenDialogDemo();
        View.Dialog.Accepted += () => View.DialogStatusText.Text = "Dialog status: accepted";
        View.Dialog.Cancelled += () => View.DialogStatusText.Text = "Dialog status: cancelled";
        View.FoundationsToggleButton.Click += _ => Model.FoundationsScopeEnabled.Value = !Model.FoundationsScopeEnabled.Value;
        View.FoundationsScopedButton.Click += _ => Model.FoundationsScopedActionCount.Value += 1;
        View.FoundationsScopedButton.FocusChanged += e => Model.FoundationsScopedFocused.Value = e.Focused;
        View.CommonCheckbox.Changed += e =>
        {
            Model.CommonCheckboxState = e.IsChecked;
            UpdateCommonControlsState();
        };
        View.CommonTriStateCheckbox.Changed += e =>
        {
            Model.CommonTriStateValue = e.IsChecked;
            UpdateCommonControlsState();
        };
        View.CommonSwitch.Changed += e =>
        {
            Model.CommonSwitchValue = e.IsChecked;
            UpdateCommonControlsState();
        };
        View.CommonSwitch.Click += _ =>
        {
            Model.CommonSwitchClickCount += 1;
            UpdateCommonControlsState();
        };
        View.CommonRadioGroup.Changed += e =>
        {
            Model.CommonRadioValue = e.SelectedValue ?? "";
            UpdateCommonControlsState();
        };
        View.CommonHorizontalSlider.Changed += e =>
        {
            Model.CommonHorizontalSliderValue = e.Value;
            UpdateCommonControlsState();
        };
        View.CommonVerticalSlider.Changed += e =>
        {
            Model.CommonVerticalSliderValue = e.Value;
            UpdateCommonControlsState();
        };
        View.CommonDropdown.Changed += e =>
        {
            Model.CommonDropdownValue = e.Item.Label;
            UpdateCommonControlsState();
        };
        View.CommonComboBox.Changed += e =>
        {
            Model.CommonComboBoxValue = e.Item.Value;
            UpdateCommonControlsState();
        };
        View.CommonComboBox.TextChanged += e =>
        {
            Model.CommonComboBoxText = e.Text;
            UpdateCommonControlsState();
        };
        View.CommonTextInput.TextChanged += e =>
        {
            Model.CommonTextInputValue = e.Text;
            UpdateCommonControlsState();
        };
        View.CommonTextInput.SelectionChanged += e =>
        {
            Model.CommonTextInputSelectionStart = (int)e.Start;
            Model.CommonTextInputSelectionEnd = (int)e.End;
            UpdateCommonControlsState();
        };
        View.CommonTextInput.FocusChanged += e =>
        {
            Model.CommonTextInputFocused = e.Focused;
            UpdateCommonControlsState();
        };
    }

    private void OpenDialogDemo()
    {
        View.DialogUsernameInput.Text = string.Empty;
        View.DialogPasswordInput.Text = string.Empty;
        View.Dialog.Title = "Sign in demo";
        View.Dialog.Body = "Use Proton Pass or another password manager to fill these fields, then press OK to submit.";
        View.DialogStatusText.Text = "Dialog status: open";
        View.Dialog.Show();
    }

    private void AttachListeners()
    {
        subscriptions.Add(Model.ClockTick.Subscribe(_ => UpdateClockState()));
        subscriptions.Add(Model.ClickCount.Subscribe(_ => UpdateClickState()));
        subscriptions.Add(Model.CounterHovered.Subscribe(_ => UpdatePointerState()));
        subscriptions.Add(Model.KeyTargetFocused.Subscribe(_ => UpdateFocusState()));
        subscriptions.Add(Model.LastKey.Subscribe(_ => UpdateKeyTargetState()));
        subscriptions.Add(Model.FoundationsScopeEnabled.Subscribe(_ => UpdateFoundationsScopeState()));
        subscriptions.Add(Model.FoundationsScopedFocused.Subscribe(_ => UpdateFoundationsScopedFocusState()));
        subscriptions.Add(Model.FoundationsScopedActionCount.Subscribe(_ => UpdateFoundationsScopedActionState()));
        View.Root.SelectedTextChanged += _ => UpdateSelectionStatusState();
        subscriptions.Add(Application.ViewportWidth.Subscribe(_ => HandleViewportChanged()));
        subscriptions.Add(Application.ViewportHeight.Subscribe(_ => HandleViewportChanged()));
        subscriptions.Add(View.SidebarList.ScrollState.OffsetY.Subscribe(_ => HandleListMetricsChanged()));
        View.MediaTextureImage.AssetStateChanged += _ => UpdateMediaState();
        View.MediaSvgImage.AssetStateChanged += _ => UpdateMediaState();
        View.MediaSecondaryTextureImage.AssetStateChanged += _ => UpdateMediaState();
    }

    private void RefreshStaticState()
    {
        UpdateClockState();
        UpdateViewportState();
        UpdateClickState();
        UpdatePointerState();
        UpdateKeyTargetState();
        UpdateFocusState();
        UpdateSelectionStatusState();
        UpdateFoundationsScopeState();
        UpdateFoundationsScopedFocusState();
        UpdateFoundationsScopedActionState();
        UpdateCommonControlsState();
        UpdateMediaState();
        HandleListMetricsChanged();
        View.DialogStatusText.Text = "Dialog status: idle";
    }

    private string ClockLabel()
    {
        var tick = Model.ClockTick.Value;
        return "Tick " + Format(tick / 60) + ":" + PadTwoDigits(tick % 60);
    }

    private string FirstVisibleLabel() => "First visible item " + Format(View.SidebarList.FirstVisibleIndex);

    private void SyncTheme()
    {
        Theme.UseCustom(DemoTheme.Generate(Model.DarkMode, accentColor));
        var theme = Theme.Active.Value;
        View.ApplyTheme(theme);
        UpdatePointerState();
        UpdateFocusState();
        UpdateFoundationsScopeState();
        UpdateFoundationsScopedFocusState();
        UpdateFoundationsScopedActionState();
        UpdateCommonControlsState();
        UpdateMediaState();
    }

    private void UpdateHeaderState() => View.HeaderStatusText.Text = ClockLabel() + "  •  " + FirstVisibleLabel();

    private void UpdateClockState()
    {
        View.ClockText.Text = ClockLabel();
        UpdateHeaderState();
    }

    private void UpdateViewportState() =>
        View.ViewportText.Text = "Viewport: " + Format((int)Application.ViewportWidth.Value) + " x " + Format((int)Application.ViewportHeight.Value);

    private void UpdateClickState() => View.ClickCountText.Text = "Clicks: " + Format(Model.ClickCount.Value);

    private void UpdatePointerState()
    {
        View.PointerStatusText.Text = Model.CounterHovered.Value ? "Pointer hover active" : "Pointer idle";
        DemoTextRecipes.Apply(View.PointerStatusText, Theme.Active.Value, Model.CounterHovered.Value ? DemoTextRecipe.StatusValue : DemoTextRecipe.Hint);
    }

    private void UpdateKeyTargetState() => View.KeyTargetText.Text = "Focus me, then press keys. Last key: " + Model.LastKey.Value;

    private void UpdateFocusState()
    {
        var theme = Theme.Active.Value;
        var focused = Model.KeyTargetFocused.Value;
        View.FocusStatusText.Text = focused ? "Keyboard target focused" : "Keyboard target unfocused";
        DemoTextRecipes.Apply(View.FocusStatusText, theme, focused ? DemoTextRecipe.StatusValue : DemoTextRecipe.Hint);
        DemoSurfaces.Apply(View.KeyTargetBox, theme, focused ? DemoSurfaceRecipe.KeyTargetFocused : DemoSurfaceRecipe.KeyTargetIdle);
    }

    private void UpdateSelectionStatusState()
    {
        var length = View.Root.SelectedText.Length;
        View.SelectionStatusText.Text = length <= 0 ? "Selection: " : "Selection: " + Format(length) + (length == 1 ? " char" : " chars");
    }

    private void UpdateFoundationsScopeState()
    {
        var enabled = Model.FoundationsScopeEnabled.Value;
        if (!enabled && Model.FoundationsScopedFocused.Value)
        {
            Model.FoundationsScopedFocused.Value = false;
        }
        View.FoundationsStatusText.Text = enabled ? "Scoped parent: enabled" : "Scoped parent: disabled via parent container";
        DemoTextRecipes.Apply(View.FoundationsStatusText, Theme.Active.Value, enabled ? DemoTextRecipe.StatusValue : DemoTextRecipe.Hint);
        View.FoundationsToggleButton.Label = enabled ? "Disable scoped child" : "Enable scoped child";
        View.FoundationsScopeBox.IsEnabled = enabled;
    }

    private void UpdateFoundationsScopedFocusState()
    {
        var focused = Model.FoundationsScopedFocused.Value;
        View.FoundationsFocusText.Text = focused ? "Scoped child focus: focused" : "Scoped child focus: unfocused";
        DemoTextRecipes.Apply(View.FoundationsFocusText, Theme.Active.Value, focused ? DemoTextRecipe.StatusValue : DemoTextRecipe.Hint);
    }

    private void UpdateFoundationsScopedActionState() =>
        View.FoundationsScopedActionText.Text = "Scoped child activations " + Format(Model.FoundationsScopedActionCount.Value);

    private void UpdateMediaState()
    {
        var theme = Theme.Active.Value;
        ApplyImageSize(View.MediaTextureImage);
        var (texture, textureRecipe) = MediaCaption(View.MediaTextureImage.AssetState, "Texture-backed image");
        View.MediaTextureCaptionText.Text = texture;
        DemoTextRecipes.Apply(View.MediaTextureCaptionText, theme, textureRecipe);
        ApplySvgSize(View.MediaSvgImage);
        var (svg, svgRecipe) = MediaCaption(View.MediaSvgImage.AssetState, "SVG-backed icon");
        View.MediaSvgCaptionText.Text = svg;
        DemoTextRecipes.Apply(View.MediaSvgCaptionText, theme, svgRecipe);
        ApplyImageSize(View.MediaSecondaryTextureImage);
        var (secondary, secondaryRecipe) = MediaCaption(View.MediaSecondaryTextureImage.AssetState, "Second texture-backed image");
        View.MediaSecondaryTextureCaptionText.Text = secondary;
        DemoTextRecipes.Apply(View.MediaSecondaryTextureCaptionText, theme, secondaryRecipe);
    }

    private void HandleViewportChanged()
    {
        View.SyncViewportLayout();
        SyncStartupScrollMetrics();
        UpdateViewportState();
    }

    private void HandleListMetricsChanged()
    {
        View.ListOffsetText.Text = "List offset " + Format((int)View.SidebarList.ScrollState.OffsetY.Value) + " px";
        View.FirstVisibleText.Text = FirstVisibleLabel();
        View.RenderedCountText.Text = "Rendered rows " + Format(View.SidebarList.RenderedItemCount);
        UpdateHeaderState();
    }

    private void UpdateCommonControlsState()
    {
        View.CommonToggleStatusText.Text = "Checkbox: " + CheckedLabel(Model.CommonCheckboxState)
            + " • Tri-state: " + CheckedLabel(Model.CommonTriStateValue)
            + " • Switch changed: " + (Model.CommonSwitchValue ? "on" : "off")
            + " • Switch semantic clicks: " + Format(Model.CommonSwitchClickCount);
        View.CommonRadioStatusText.Text = "Radio group: " + Model.CommonRadioValue;
        View.CommonSliderStatusText.Text = "Horizontal slider: " + Format((int)Model.CommonHorizontalSliderValue)
            + " • Vertical slider: " + Format((int)Model.CommonVerticalSliderValue);
        View.CommonDropdownStatusText.Text = "Dropdown: " + Model.CommonDropdownValue;
        View.CommonComboBoxStatusText.Text = "ComboBox: " + Model.CommonComboBoxValue + " • Text: " + Quoted(Model.CommonComboBoxText);
        View.CommonTextInputStatusText.Text = "TextInput: " + Quoted(Model.CommonTextInputValue)
            + " • Selection: " + Format(Model.CommonTextInputSelectionStart) + "-" + Format(Model.CommonTextInputSelectionEnd)
            + " • Focus: " + (Model.CommonTextInputFocused ? "focused" : "blurred");
    }
}
