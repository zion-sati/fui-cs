using System.Collections.Generic;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.TemplatedControls;

/// <summary>
/// The route's sections: controls dressed by the house templates, the checkbox with its local override, the same
/// controls tinted by per-instance colors, and a live summary of their state. The dropdowns, text input and text area
/// of the reference route join in Stage 5, with those controls.
/// </summary>
internal sealed class TemplatedControlsView
{
    public DemoCheckbox HouseCheckbox { get; } = new DemoCheckbox("House style notifications", true)
        .Template(TemplatedControlsTemplates.Checkbox())
        .NodeId("templated-controls:house-checkbox");

    public DemoCheckbox OverrideCheckbox { get; } = new DemoCheckbox("Local override emphasis", true)
        .Template(TemplatedControlsTemplates.LocalCheckbox())
        .IsChecked(true)
        .NodeId("templated-controls:override-checkbox");

    public DemoSwitch HouseSwitch { get; } = new DemoSwitch("House style quick actions", true)
        .Template(TemplatedControlsTemplates.Switch())
        .IsChecked(true)
        .NodeId("templated-controls:house-switch");

    public DemoRadioGroup ModeGroup { get; } = new DemoRadioGroup(true)
        .Radio(
            new DemoRadioButton("balanced", "Balanced chrome", true).Template(TemplatedControlsTemplates.Radio()),
            new DemoRadioButton("compact", "Compact chrome", true).Template(TemplatedControlsTemplates.Radio()),
            new DemoRadioButton("bold", "Bold chrome", true).Template(TemplatedControlsTemplates.Radio()))
        .SelectedIndex(1)
        .SemanticLabel("House style mode")
        .NodeId("templated-controls:mode-group");

    public DemoSlider IntensitySlider { get; } = new DemoSlider(40.0f)
        .Template(TemplatedControlsTemplates.Slider())
        .Minimum(0.0f)
        .Maximum(100.0f)
        .Step(5.0f)
        .Length(240.0f)
        .SemanticLabel("House style intensity")
        .NodeId("templated-controls:intensity-slider");

    public Button HouseButton { get; } = new Button("Run house action").Template(TemplatedControlsTemplates.Button());

    public Button ColorButton { get; } = new Button("Run tinted action")
        .Template(TemplatedControlsTemplates.Button())
        .NodeId("templated-controls:color-button");

    public DemoCheckbox ColorCheckbox { get; } = new DemoCheckbox("Tinted checkbox chrome", true)
        .Template(TemplatedControlsTemplates.Checkbox())
        .IsChecked(true)
        .NodeId("templated-controls:color-checkbox");

    public DemoSwitch ColorSwitch { get; } = new DemoSwitch("Tinted switch chrome", true)
        .Template(TemplatedControlsTemplates.Switch())
        .IsChecked(true)
        .NodeId("templated-controls:color-switch");

    public DemoRadioButton CoolRadio { get; } = new DemoRadioButton("cool", "Cool accent radio", true)
        .Template(TemplatedControlsTemplates.Radio())
        .NodeId("templated-controls:color-radio-cool");

    public DemoRadioButton WarmRadio { get; } = new DemoRadioButton("warm", "Warm accent radio", true)
        .Template(TemplatedControlsTemplates.Radio())
        .NodeId("templated-controls:color-radio-warm");

    public DemoRadioGroup ColorRadioGroup { get; }

    public DemoSlider ColorSlider { get; } = new DemoSlider(72.0f)
        .Template(TemplatedControlsTemplates.Slider())
        .Minimum(0.0f)
        .Maximum(100.0f)
        .Step(2.0f)
        .Length(240.0f)
        .SemanticLabel("Tinted slider")
        .NodeId("templated-controls:color-slider");

    public Text DefaultsStatusText { get; } = new DemoText("", DemoTextRecipe.StatusValue).FontSize(15.0f);

    public Text OverrideStatusText { get; } = new DemoText("", DemoTextRecipe.StatusSupporting).FontSize(15.0f);

    private readonly Text defaultsHintText = new DemoText(
            "The demo design system applies explicit templates to Button, Checkbox, Switch, RadioButton, Slider, Dropdown, TextInput, and TextArea.",
            DemoTextRecipe.Hint)
        .FontSize(15.0f)
        .MaxLines(3);

    private readonly Text overrideHintText = new DemoText(
            "The square override checkbox supplies a different explicit indicator template.",
            DemoTextRecipe.Hint)
        .FontSize(15.0f)
        .MaxLines(3);

    private readonly Text colorHintText = new DemoText(
            "Per-instance control color objects tint the shipped presenters and the house templates on this route without changing the built-in control behavior.",
            DemoTextRecipe.Hint)
        .FontSize(15.0f)
        .MaxLines(4);

    public TemplatedControlsView()
    {
        ColorRadioGroup = new DemoRadioGroup(true)
            .Radio(CoolRadio, WarmRadio)
            .SelectedIndex(0)
            .NodeId("templated-controls:color-radio-group");
    }

    public void ApplyTheme(Theme theme)
    {
        var colors = theme.Colors;
        var fieldBackground = colors.Surface.Mix(colors.Background, 0.20f);
        var fieldBorder = colors.Border.Mix(colors.Accent, 0.28f);
        var accentSoft = colors.Accent.Mix(colors.Surface, 0.22f);
        var accentHover = colors.AccentHovered.Mix(colors.Surface, 0.14f);
        var accentPressed = colors.AccentPressed.Mix(colors.Surface, 0.08f);
        var secondaryAccent = colors.AccentHovered.Mix(colors.Selection, 0.28f);
        var secondaryBorder = colors.Border.Mix(colors.AccentHovered, 0.38f);

        ColorButton.Colors = new ButtonColors
        {
            Background = accentSoft,
            BackgroundHover = accentHover,
            BackgroundPressed = accentPressed,
            Border = fieldBorder,
            TextPrimary = colors.TextPrimary,
        };
        var labeled = new LabeledControlColors
        {
            Background = fieldBackground,
            Border = fieldBorder,
            Accent = colors.Accent,
            TextPrimary = colors.TextPrimary,
            TextMuted = colors.TextMuted,
        };
        ColorCheckbox.Colors = labeled;
        ColorSwitch.Colors = labeled;
        CoolRadio.Colors = labeled;
        WarmRadio.Colors = labeled with { Border = secondaryBorder, Accent = secondaryAccent };
        ColorSlider.Colors = new SliderColors { Track = fieldBackground, Fill = colors.Accent, Thumb = colors.Surface };
    }

    public IReadOnlyList<RoutePageSection> BuildSections() =>
    [
        RoutePageSection.Create(
            "Design-system templates",
            "These controls receive explicit templates from the route design system while retaining their built-in semantics, persistence, and interaction behavior.",
            FlexBox.Column(
                    HouseCheckbox,
                    VerticalSpacer(10.0f),
                    HouseSwitch,
                    VerticalSpacer(12.0f),
                    ModeGroup,
                    VerticalSpacer(14.0f),
                    IntensitySlider,
                    VerticalSpacer(14.0f),
                    HouseButton,
                    VerticalSpacer(10.0f),
                    defaultsHintText)
                .FillWidth()),
        RoutePageSection.Create(
            "Alternative local template",
            "This control uses a different local template, so it stays distinct from the route's house controls.",
            FlexBox.Column(OverrideCheckbox, VerticalSpacer(10.0f), overrideHintText).FillWidth()),
        RoutePageSection.Create(
            "Per-instance color overrides",
            "Color objects tint button, labeled, slider, dropdown, and text-entry presenters while leaving semantics and interaction behavior with the built-in controls.",
            FlexBox.Column(
                    ColorButton,
                    VerticalSpacer(10.0f),
                    ColorCheckbox,
                    VerticalSpacer(10.0f),
                    ColorSwitch,
                    VerticalSpacer(12.0f),
                    ColorRadioGroup,
                    VerticalSpacer(14.0f),
                    ColorSlider,
                    VerticalSpacer(10.0f),
                    colorHintText)
                .FillWidth()),
        RoutePageSection.Create(
            "Live state summary",
            "Use the controls above and confirm the summary updates without any custom behavior leaking into the presenter classes.",
            FlexBox.Column(DefaultsStatusText, VerticalSpacer(8.0f), OverrideStatusText).FillWidth()),
    ];

    private static FlexBox VerticalSpacer(float height) => new FlexBox().FillWidth().Height(height);
}
