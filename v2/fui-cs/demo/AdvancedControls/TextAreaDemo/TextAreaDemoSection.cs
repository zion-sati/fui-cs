using System;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls.TextAreaDemo;

/// <summary>
/// A text area reconfigured live: quick toggles for read-only, wrapping, tabs and the scroll bars, radio groups for the
/// exact scroll bar policies, line height and font, a visibility dropdown, and status lines for its focus, selection
/// and settings. The toggles and the policy groups mirror each other.
/// </summary>
internal sealed class TextAreaDemoSection : IDisposable
{
    private const float FixedLineHeight = 28.0f;

    private readonly DemoTextArea textArea = new DemoTextArea("Line one\nLine two\nLine three\nLonger content so scrollbar policy is easy to spot.")
        .Placeholder("Type notes here or paste sample content. Use the controls below to reconfigure the TextArea live.")
        .FillWidth()
        .Height(220.0f)
        .NodeId("demo-advanced-controls:text-area");

    private readonly DemoCheckbox readOnlyToggle = new DemoCheckbox("Read-only", true).NodeId("demo-advanced-controls:read-only-toggle");
    private readonly DemoCheckbox wrappingToggle = new DemoCheckbox("Wrapping", true).IsChecked(true).NodeId("demo-advanced-controls:wrapping-toggle");
    private readonly DemoCheckbox acceptsTabToggle = new DemoCheckbox("Accepts Tab", true).IsChecked(true).NodeId("demo-advanced-controls:accepts-tab-toggle");
    private readonly DemoCheckbox alwaysVerticalToggle = new DemoCheckbox("Always show vertical scrollbar", true).NodeId("demo-advanced-controls:always-vertical-toggle");
    private readonly DemoCheckbox neverVerticalToggle = new DemoCheckbox("Hide vertical scrollbar", true).NodeId("demo-advanced-controls:never-vertical-toggle");
    private readonly DemoCheckbox alwaysHorizontalToggle = new DemoCheckbox("Always show horizontal scrollbar", true).NodeId("demo-advanced-controls:always-horizontal-toggle");
    private readonly DemoCheckbox neverHorizontalToggle = new DemoCheckbox("Hide horizontal scrollbar", true).NodeId("demo-advanced-controls:never-horizontal-toggle");
    private readonly DemoRadioGroup verticalPolicyGroup = PolicyGroup("Vertical").NodeId("demo-advanced-controls:vertical-policy-group");
    private readonly DemoRadioGroup horizontalPolicyGroup = PolicyGroup("Horizontal").NodeId("demo-advanced-controls:horizontal-policy-group");
    private readonly DemoRadioGroup lineHeightGroup = new DemoRadioGroup(true)
        .Radio(new DemoRadioButton("normal", "Line height: Normal", true), new DemoRadioButton("fixed-28", "Line height: Fixed 28 px", true))
        .SelectedIndex(0)
        .NodeId("demo-advanced-controls:line-height-group");
    private readonly DemoRadioGroup fontModeGroup = new DemoRadioGroup(true)
        .Radio(new DemoRadioButton("variable", "Text font: Variable width", true), new DemoRadioButton("mono", "Text font: Monospace", true))
        .SelectedIndex(0)
        .NodeId("demo-advanced-controls:font-mode-group");
    private readonly DemoDropdown visibilityDropdown = new DemoDropdown()
        .Items(
            new DropdownItem("normal", "Visibility: Normal - keep layout reserved and content rendered"),
            new DropdownItem("hidden", "Visibility: Hidden - keep layout reserved but stop painting content"),
            new DropdownItem("collapsed", "Visibility: Collapsed - remove layout space and hide the content"))
        .SelectedIndex(0)
        .FillWidth()
        .NodeId("demo-advanced-controls:visibility-dropdown");
    private readonly Text focusStatusText = StatusText(DemoTextRecipe.StatusValue).NodeId("text-area-focus");
    private readonly Text selectionStatusText = StatusText(DemoTextRecipe.StatusValue).NodeId("text-area-selection");
    private readonly Text settingsStatusText = StatusText(DemoTextRecipe.StatusSupporting).NodeId("text-area-settings");
    private readonly IDisposable themeSubscription;
    private bool syncingVerticalPolicy;
    private bool syncingHorizontalPolicy;

    public TextAreaDemoSection()
    {
        textArea.SemanticLabel = "Advanced controls demo text area";
        readOnlyToggle.Changed += e =>
        {
            textArea.IsReadOnly = e.IsChecked != false;
            SyncStatus();
        };
        wrappingToggle.Changed += e =>
        {
            var enabled = e.IsChecked != false;
            textArea.Wrapping = enabled;
            if (enabled)
            {
                horizontalPolicyGroup.SelectedIndex = 0;
            }
            SyncHorizontalPolicy();
            SyncStatus();
        };
        acceptsTabToggle.Changed += e =>
        {
            textArea.AcceptsTab = e.IsChecked != false;
            SyncStatus();
        };
        alwaysVerticalToggle.Changed += e => OnPolicyToggle(e, neverVerticalToggle, vertical: true);
        neverVerticalToggle.Changed += e => OnPolicyToggle(e, alwaysVerticalToggle, vertical: true);
        alwaysHorizontalToggle.Changed += e => OnPolicyToggle(e, neverHorizontalToggle, vertical: false);
        neverHorizontalToggle.Changed += e => OnPolicyToggle(e, alwaysHorizontalToggle, vertical: false);
        textArea.FocusChanged += _ => SyncStatus();
        textArea.SelectionChanged += _ => SyncStatus();
        textArea.TextChanged += _ => SyncStatus();
        verticalPolicyGroup.Changed += _ => SyncVerticalPolicy();
        horizontalPolicyGroup.Changed += _ => SyncHorizontalPolicy();
        lineHeightGroup.Changed += _ => SyncLineHeight();
        fontModeGroup.Changed += _ => SyncFontMode();
        visibilityDropdown.Changed += _ => SyncVisibility();
        themeSubscription = Theme.Active.Subscribe(_ => SyncFontMode());
        SyncFontMode();
        textArea.AcceptsTab = acceptsTabToggle.IsChecked == true;
        SyncVerticalPolicy();
        SyncHorizontalPolicy();
        SyncLineHeight();
        SyncVisibility();
        SyncStatus();
    }

    public RoutePageSection BuildSection()
    {
        var checkboxColumn = FlexBox.Column(
            readOnlyToggle,
            Spacer(8.0f),
            wrappingToggle,
            Spacer(8.0f),
            acceptsTabToggle,
            Spacer(8.0f),
            alwaysVerticalToggle,
            Spacer(8.0f),
            neverVerticalToggle,
            Spacer(8.0f),
            alwaysHorizontalToggle,
            Spacer(8.0f),
            neverHorizontalToggle);
        var policyColumn = FlexBox.Column(
            verticalPolicyGroup,
            Spacer(12.0f),
            horizontalPolicyGroup,
            Spacer(12.0f),
            lineHeightGroup,
            Spacer(12.0f),
            fontModeGroup,
            Spacer(12.0f),
            visibilityDropdown);
        var hint = new DemoText(
                "Use the quick toggles for common changes, or the radio groups when you want an exact scrollbar or line-height setting.",
                DemoTextRecipe.Hint)
            .FontSize(15.0f)
            .MaxLines(3);
        var body = FlexBox.Column(
                textArea,
                Spacer(14.0f),
                hint,
                Spacer(14.0f),
                FlexBox.Row(checkboxColumn, new FlexBox().Width(18.0f).Height(1.0f), policyColumn).FillWidth(),
                Spacer(14.0f),
                focusStatusText,
                Spacer(6.0f),
                selectionStatusText,
                Spacer(6.0f),
                settingsStatusText)
            .FillWidth();
        return RoutePageSection.Create(
            "Advanced controls",
            "Configure a TextArea live to explore wrapping, read-only mode, scrollbar policy, line height, font family, and visibility.",
            body);
    }

    public void Dispose() => themeSubscription.Dispose();

    private static DemoRadioGroup PolicyGroup(string axis) => new DemoRadioGroup(true)
        .Radio(
            new DemoRadioButton("auto", axis + " scrollbar: Auto", true),
            new DemoRadioButton("always", axis + " scrollbar: Always", true),
            new DemoRadioButton("never", axis + " scrollbar: Never", true))
        .SelectedIndex(0);

    private static Text StatusText(DemoTextRecipe recipe) => new DemoText(string.Empty, recipe).FontSize(15.0f);

    private static FlexBox Spacer(float height) => new FlexBox().Height(height);

    /// <summary>A scroll bar toggle unchecks its opposite and picks the matching policy radio.</summary>
    private void OnPolicyToggle(CheckboxChangedEventArgs e, DemoCheckbox opposite, bool vertical)
    {
        if (vertical ? syncingVerticalPolicy : syncingHorizontalPolicy)
        {
            return;
        }
        if (e.IsChecked != false)
        {
            opposite.IsChecked = false;
        }
        var (always, never, group) = vertical
            ? (alwaysVerticalToggle, neverVerticalToggle, verticalPolicyGroup)
            : (alwaysHorizontalToggle, neverHorizontalToggle, horizontalPolicyGroup);
        SetSyncing(vertical, true);
        group.SelectedIndex = never.IsChecked == true ? 2 : always.IsChecked == true ? 1 : 0;
        SetSyncing(vertical, false);
        if (vertical)
        {
            SyncVerticalPolicy();
        }
        else
        {
            SyncHorizontalPolicy();
        }
        SyncStatus();
    }

    private void SetSyncing(bool vertical, bool syncing)
    {
        if (vertical)
        {
            syncingVerticalPolicy = syncing;
        }
        else
        {
            syncingHorizontalPolicy = syncing;
        }
    }

    private void SyncVerticalPolicy()
    {
        syncingVerticalPolicy = true;
        var value = verticalPolicyGroup.SelectedValue;
        textArea.VerticalScrollBarVisibility = Visibility(value);
        alwaysVerticalToggle.IsChecked = value == "always";
        neverVerticalToggle.IsChecked = value == "never";
        syncingVerticalPolicy = false;
    }

    private void SyncHorizontalPolicy()
    {
        syncingHorizontalPolicy = true;
        var value = horizontalPolicyGroup.SelectedValue;
        textArea.HorizontalScrollBarVisibility = Visibility(value);
        alwaysHorizontalToggle.IsChecked = value == "always";
        neverHorizontalToggle.IsChecked = value == "never";
        syncingHorizontalPolicy = false;
    }

    private static ScrollBarVisibility Visibility(string? policy) => policy switch
    {
        "always" => ScrollBarVisibility.Visible,
        "never" => ScrollBarVisibility.Hidden,
        _ => ScrollBarVisibility.Auto,
    };

    private void SyncLineHeight()
    {
        if (lineHeightGroup.SelectedValue == "fixed-28")
        {
            textArea.LineHeight = FixedLineHeight;
        }
        else
        {
            textArea.LineHeight = null;
            lineHeightGroup.SelectedIndex = 0;
        }
        SyncStatus();
    }

    private void SyncFontMode()
    {
        var fonts = Theme.Active.Value.Fonts;
        if (fontModeGroup.SelectedValue == "mono")
        {
            textArea.FontFamily = fonts.MonoFamily;
            textArea.FontSize = fonts.SizeMono;
        }
        else
        {
            textArea.FontFamily = fonts.BodyFamily;
            textArea.FontSize = fonts.SizeBody;
            fontModeGroup.SelectedIndex = 0;
        }
        SyncStatus();
    }

    private void SyncVisibility()
    {
        textArea.Visibility = visibilityDropdown.SelectedIndex switch
        {
            1 => FuiCs.Visibility.Hidden,
            2 => FuiCs.Visibility.Collapsed,
            _ => FuiCs.Visibility.Normal,
        };
        SyncStatus();
    }

    private void SyncStatus()
    {
        var length = textArea.Text.Length.ToString(CultureInfo.InvariantCulture);
        focusStatusText.Text = "Focus: " + (textArea.IsFocused ? "focused" : "blurred") + " • Text length: " + length;
        selectionStatusText.Text = "Selection: " + textArea.SelectionStart.ToString(CultureInfo.InvariantCulture) + "-"
            + textArea.SelectionEnd.ToString(CultureInfo.InvariantCulture);
        settingsStatusText.Text = "Read-only: " + (textArea.IsReadOnly ? "on" : "off")
            + " • Wrapping: " + (wrappingToggle.IsChecked == true ? "on" : "off")
            + " • Tabs: " + (acceptsTabToggle.IsChecked == true ? "insert" : "traverse")
            + " • Visibility: " + (visibilityDropdown.SelectedIndex switch { 1 => "hidden", 2 => "collapsed", _ => "normal" })
            + " • Vertical: " + verticalPolicyGroup.SelectedValue
            + " • Horizontal: " + horizontalPolicyGroup.SelectedValue
            + " • Line height: " + (lineHeightGroup.SelectedValue == "fixed-28" ? "fixed 28px" : "normal")
            + " • Font: " + (fontModeGroup.SelectedValue == "mono" ? "monospace" : "variable");
    }
}
