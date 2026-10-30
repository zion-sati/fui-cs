using System;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>The dashboard's cards: the header with the routes card, the preview panel's samples, and the inspector.</summary>
internal sealed partial class DashboardView
{
    private const string DemoTextureUrl = "./demo-texture.png";
    private const string DemoSecondaryTextureUrl = "./demo-secondary-texture.png";
    private const string DemoSvgUrl = "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='128' height='128' viewBox='0 0 128 128'><path d='M64 14 24 34v28c0 25 17 43 40 52 23-9 40-27 40-52V34L64 14Zm0 16 24 12v19c0 17-9 29-24 37-15-8-24-20-24-37V42l24-12Zm-8 22h16v16h16v12H72v16H56V80H40V68h16V52Z' fill='%23000000'/></svg>";

    private readonly FlexBox accentSwatch = new FlexBox().Width(DemoTokens.FullSize, Unit.Percent).Height(DemoTokens.AccentSwatchHeight);
    private readonly FlexBox inspectorDivider = new FlexBox().Width(DemoTokens.FullSize, Unit.Percent).Height(2.0f);
    private readonly FlexBox demoScopeBadge = new FlexBox()
        .Child(new DashboardText("Dashboard sample", 13.0f, DemoTextRecipe.InverseLabel))
        .Padding(12.0f, 6.0f, 12.0f, 6.0f)
        .CornerRadius(999.0f);
    private readonly Bitmap mediaBitmap = DashboardBitmap.Create();
    private readonly DemoScrollBox nestedScrollBox = new DemoScrollBox()
        .HorizontalScrollEnabled(true)
        .VerticalScrollEnabled(true)
        .HorizontalScrollBarVisibility(ScrollBarVisibility.Visible)
        .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
        .ScrollBarGutter(8.0f)
        .Width(DemoTokens.NestedScrollViewportWidth)
        .Height(DemoTokens.NestedScrollViewportHeight);

    public DemoButton CounterButton { get; } = new DemoButton("Click me", DemoButtonTone.Primary).Width(196.0f);

    public DemoButton DialogButton { get; } = new DemoButton("Open dialog").Width(196.0f);

    public DemoButton ToolTipSampleButton { get; } = new DemoButton("Tooltip sample").Width(196.0f).ToolTip(
        new ToolTip("Shared retained tooltip host.\nHover or focus here to test delayed open.\nPress Escape to leave keyboard tab mode.")
        {
            InitialShowDelay = TimeSpan.FromMilliseconds(250.0),
            BetweenShowDelay = TimeSpan.FromMilliseconds(75.0),
            ShowDuration = TimeSpan.Zero,
            Placement = PopupPlacement.Bottom,
        });

    public DemoButton FoundationsToggleButton { get; } = new DemoButton("Disable scoped child").Width(220.0f);

    public DemoButton FoundationsScopedButton { get; } = new DemoButton("Scoped child action", DemoButtonTone.Primary).Width(220.0f);

    public Text KeyTargetText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue, false);

    public FlexBox KeyTargetBox { get; private set; } = null!;

    public Text ViewportText { get; } = new DashboardText("", 15.0f);

    public Text ClickCountText { get; } = new DashboardText("", 17.0f);

    public Text PointerStatusText { get; } = new DashboardText("", 15.0f);

    public Text FocusStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text DialogStatusText { get; } = new DashboardText("", 15.0f, DemoTextRecipe.StatusValue);

    public Text FoundationsStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text FoundationsFocusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text FoundationsScopedActionText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public FlexBox FoundationsScopeBox { get; } = new DemoSurface(DemoSurfaceRecipe.InsetPanel).Width(DemoTokens.FullSize, Unit.Percent).Padding(14.0f, 14.0f, 14.0f, 14.0f);

    public DemoCheckbox CommonCheckbox { get; } = new DemoCheckbox("Email updates", true).NodeId("demo-dashboard:email-updates-checkbox");

    public DemoCheckbox CommonTriStateCheckbox { get; } = new DemoCheckbox("Review state", true)
        .IsThreeState()
        .IsChecked(null)
        .NodeId("demo-dashboard:review-state-checkbox");

    public DemoSwitch CommonSwitch { get; } = new DemoSwitch("Focus mode", true).IsChecked(true).NodeId("demo-dashboard:focus-mode-switch");

    public DemoRadioGroup CommonRadioGroup { get; } = new DemoRadioGroup(true)
        .Radio(new DemoRadioButton("system", "System", true), new DemoRadioButton("compact", "Compact", true), new DemoRadioButton("immersive", "Immersive", true))
        .SelectedIndex(0)
        .NodeId("demo-dashboard:mode-radio-group");

    public DemoSlider CommonHorizontalSlider { get; } = new DemoSlider(40.0f)
        .Minimum(0.0f)
        .Maximum(100.0f)
        .Step(5.0f)
        .Length(220.0f)
        .NodeId("demo-dashboard:horizontal-slider");

    public DemoSlider CommonVerticalSlider { get; } = new DemoSlider(60.0f)
        .Minimum(0.0f)
        .Maximum(100.0f)
        .Step(10.0f)
        .Orientation(Orientation.Vertical)
        .Length(140.0f)
        .NodeId("demo-dashboard:vertical-slider");

    public DemoDropdown CommonDropdown { get; } = new DemoDropdown()
        .Items(new DropdownItem("balanced", "Balanced"), new DropdownItem("quality", "Quality first"), new DropdownItem("speed", "Speed first"))
        .SelectedIndex(0)
        .Width(220.0f)
        .NodeId("demo-dashboard:render-mode-dropdown");

    public DemoComboBox CommonComboBox { get; } = new DemoComboBox()
        .Placeholder("Search city")
        .Items(
            "Melbourne", "Sydney", "Singapore", "San Francisco", "Seattle", "Tokyo", "Osaka", "London", "Paris", "Berlin",
            "Toronto", "Vancouver", "Auckland", "Wellington", "Hong Kong", "Bangkok", "Seoul", "Taipei", "New York", "Chicago")
        .SelectedIndex(0)
        .Width(220.0f)
        .NodeId("demo-dashboard:city-combobox");

    public Text CommonComboBoxStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public DemoTextInput CommonTextInput { get; } = new DemoTextInput()
        .Placeholder("Type here")
        .MaxChars(32)
        .HostAutofill("username")
        .Width(220.0f)
        .NodeId("demo-dashboard:text-input");

    public DemoTextInput CommonPasswordInput { get; } = new DemoTextInput("super-secret")
        .Password()
        .HostAutofill("current-password")
        .Width(220.0f)
        .NodeId("current-password");

    public DemoTextInput CommonReadOnlyInput { get; } = new DemoTextInput("Read-only selection sample").ReadOnly().Width(220.0f);

    public Text CommonTextInputStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text CommonToggleStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text CommonRadioStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text CommonSliderStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Text CommonDropdownStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusValue);

    public Image MediaTextureImage { get; } = Image.Load(DemoTextureUrl, ObjectFit.Cover)
        .AltText("Bridge loaded texture sample")
        .Width(16.0f)
        .Height(16.0f)
        .CornerRadius(DemoTokens.SurfaceRadiusSmall);

    public Svg MediaSvgImage { get; } = Svg.Load(DemoSvgUrl)
        .AltText("Bridge loaded SVG sample")
        .Width(16.0f)
        .Height(16.0f)
        .CornerRadius(DemoTokens.SurfaceRadiusSmall);

    public Image MediaSecondaryTextureImage { get; } = Image.Load(DemoSecondaryTextureUrl, ObjectFit.Cover)
        .AltText("Bridge loaded secondary texture sample")
        .Width(16.0f)
        .Height(16.0f)
        .CornerRadius(DemoTokens.SurfaceRadiusSmall);

    public Text MediaTextureCaptionText { get; } = new DashboardText("Texture-backed image", 14.0f, DemoTextRecipe.StatusValue);

    public Text MediaSvgCaptionText { get; } = new DashboardText("SVG-backed icon", 14.0f, DemoTextRecipe.StatusValue);

    public Text MediaSecondaryTextureCaptionText { get; } = new DashboardText("Second texture-backed image", 14.0f, DemoTextRecipe.StatusValue);

    public FlexBox NestedScrollContent { get; } = new DemoSurface(DemoSurfaceRecipe.CalloutInset)
        .Width(DemoTokens.NestedScrollContentWidth)
        .Height(DemoTokens.NestedScrollContentHeight)
        .Padding(16.0f, 16.0f, 16.0f, 16.0f);

    public DemoScrollBox NestedScrollBox => nestedScrollBox;

    private FlexBox BuildMainHeaderContent()
    {
        // The links wrap rather than run past the card in a narrow window.
        var navRow = FlexBox.Row(
                new DemoNavLink(DemoRoutes.AdvancedControls, "Advanced controls").Margin(0.0f, 0.0f, 10.0f, 8.0f),
                new DemoNavLink(DemoRoutes.TemplatedControls, "Templated controls").Margin(0.0f, 0.0f, 10.0f, 8.0f),
                new DemoNavLink(DemoRoutes.ScrollbarGutter, "Scrollbar gutter bug").Margin(0.0f, 0.0f, 10.0f, 8.0f),
                new DemoNavLink(DemoRoutes.ImmediateDrawing, "Immediate-mode drawing").Margin(0.0f, 0.0f, 0.0f, 8.0f))
            .FlexWrap(FlexWrap.Wrap)
            .Width(DemoTokens.FullSize, Unit.Percent);
        var demoScopeCard = new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(16.0f, 16.0f, 16.0f, 16.0f)
            .Child(FlexBox.Column(
                    demoScopeBadge,
                    VerticalSpacer(10.0f),
                    new DashboardText("SDK samples are routed", 22.0f, DemoTextRecipe.SectionTitle),
                    VerticalSpacer(8.0f),
                    new DashboardText("This page is the dashboard route in the sample shell, and both Advanced controls and Templated controls open on their own routed pages.", 15.0f, DemoTextRecipe.Supporting),
                    VerticalSpacer(10.0f),
                    new DashboardText("Use the NavLinks below to jump between the three demo MFEs without leaving the shared runtime shell.", 14.0f, DemoTextRecipe.Hint),
                    VerticalSpacer(12.0f),
                    navRow)
                .Width(DemoTokens.FullSize, Unit.Percent));
        return FlexBox.Column(
                new DashboardText("EffinDom FUI-CS Demo", 30.0f, DemoTextRecipe.PageTitle),
                VerticalSpacer(6.0f),
                new DashboardText("Explore layout, scrolling, state, media, and input behavior in one retained scene.", 16.0f, DemoTextRecipe.Supporting),
                VerticalSpacer(12.0f),
                demoScopeCard,
                VerticalSpacer(10.0f),
                HeaderStatusText,
                VerticalSpacer(6.0f),
                SelectionStatusText)
            .Width(DemoTokens.FullSize, Unit.Percent);
    }

    private FlexBox BuildPreviewPanel()
    {
        KeyTargetBox = new FlexBox()
            .Width(280.0f)
            .Padding(12.0f, 10.0f, 12.0f, 10.0f)
            .Child(KeyTargetText)
            .NodeId("demo-key-target")
            .Focusable(true);
        return new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(20.0f, 20.0f, 20.0f, 20.0f)
            .Child(FlexBox.Column(
                    new DashboardText("Interactive color preview", 24.0f, DemoTextRecipe.SectionTitle),
                    VerticalSpacer(12.0f),
                    new DashboardText("The hue slider drives a single accent plate so the preview reads as one deliberate color target instead of nested boxes.", 15.0f, DemoTextRecipe.Supporting),
                    VerticalSpacer(12.0f),
                    accentSwatch,
                    VerticalSpacer(4.0f),
                    ViewportText,
                    VerticalSpacer(4.0f),
                    ClickCountText,
                    VerticalSpacer(4.0f),
                    PointerStatusText,
                    VerticalSpacer(12.0f),
                    CounterButton,
                    VerticalSpacer(6.0f),
                    DialogButton,
                    VerticalSpacer(6.0f),
                    ToolTipSampleButton,
                    VerticalSpacer(6.0f),
                    new DashboardText("Hover or Tab onto the tooltip sample to inspect the shared retained tooltip host.", 14.0f, DemoTextRecipe.Hint),
                    VerticalSpacer(6.0f),
                    DialogStatusText,
                    VerticalSpacer(8.0f),
                    BuildFoundationsCard(),
                    VerticalSpacer(8.0f),
                    BuildCommonControlsCard(),
                    VerticalSpacer(8.0f),
                    BuildMediaCard(),
                    VerticalSpacer(8.0f),
                    BuildNestedScrollCard(),
                    VerticalSpacer(6.0f),
                    KeyTargetBox,
                    VerticalSpacer(6.0f),
                    FocusStatusText,
                    VerticalSpacer(6.0f),
                    new DashboardText("Resize the browser or drag the slider to explore layout, derived state, and resize handling together.", 15.0f, DemoTextRecipe.Hint))
                .Width(DemoTokens.FullSize, Unit.Percent));
    }

    private FlexBox BuildFoundationsCard() => new DemoSurface(DemoSurfaceRecipe.SectionPanel)
        .Padding(16.0f, 16.0f, 16.0f, 16.0f)
        .Child(FlexBox.Column(
                new DashboardText("Control foundations", 20.0f, DemoTextRecipe.SectionTitle),
                VerticalSpacer(8.0f),
                new DashboardText("Tab onto these buttons to see the focus ring. Use the outer toggle to disable the parent scope and watch the child button dim and stop activating.", 14.0f, DemoTextRecipe.Supporting),
                VerticalSpacer(12.0f),
                FoundationsStatusText,
                VerticalSpacer(12.0f),
                FoundationsToggleButton,
                VerticalSpacer(10.0f),
                FoundationsScopeBox.Child(FlexBox.Column(
                        new DashboardText("This child button inherits enabled/disabled from its parent scope.", 14.0f, DemoTextRecipe.Hint),
                        VerticalSpacer(10.0f),
                        FoundationsScopedButton,
                        VerticalSpacer(8.0f),
                        FoundationsFocusText,
                        VerticalSpacer(8.0f),
                        FoundationsScopedActionText)
                    .Width(DemoTokens.FullSize, Unit.Percent)))
            .Width(DemoTokens.FullSize, Unit.Percent));

    private FlexBox BuildCommonControlsCard() => new DemoSurface(DemoSurfaceRecipe.SectionPanel)
        .Padding(16.0f, 16.0f, 16.0f, 16.0f)
        .Child(FlexBox.Column(
                new DashboardText("Common controls", 20.0f, DemoTextRecipe.SectionTitle),
                VerticalSpacer(8.0f),
                new DashboardText("Checkbox, switch, radio, slider, dropdown, combobox, and text input samples all sit on the same retained semantic and focus foundation.", 14.0f, DemoTextRecipe.Supporting),
                VerticalSpacer(12.0f),
                CommonCheckbox,
                VerticalSpacer(8.0f),
                CommonTriStateCheckbox,
                VerticalSpacer(8.0f),
                CommonSwitch,
                VerticalSpacer(12.0f),
                CommonRadioGroup.Width(DemoTokens.FullSize, Unit.Percent),
                VerticalSpacer(12.0f),
                FlexBox.Row(CommonHorizontalSlider, HorizontalSpacer(18.0f), CommonVerticalSlider).AlignItems(AlignItems.Center),
                VerticalSpacer(12.0f),
                CommonDropdown,
                VerticalSpacer(12.0f),
                CommonComboBox,
                VerticalSpacer(12.0f),
                CommonTextInput,
                VerticalSpacer(8.0f),
                CommonPasswordInput,
                VerticalSpacer(8.0f),
                CommonReadOnlyInput,
                VerticalSpacer(12.0f),
                CommonTextInputStatusText,
                VerticalSpacer(6.0f),
                CommonToggleStatusText,
                VerticalSpacer(6.0f),
                CommonRadioStatusText,
                VerticalSpacer(6.0f),
                CommonSliderStatusText,
                VerticalSpacer(6.0f),
                CommonDropdownStatusText,
                VerticalSpacer(6.0f),
                CommonComboBoxStatusText)
            .Width(DemoTokens.FullSize, Unit.Percent));

    private FlexBox BuildMediaCard()
    {
        var bitmapImage = new Image(mediaBitmap.TextureId, ObjectFit.Contain)
            .AltText("App-owned premultiplied RGBA bitmap sample")
            .Width(DashboardBitmap.Size)
            .Height(DashboardBitmap.Size)
            .CornerRadius(DemoTokens.SurfaceRadiusSmall);
        return new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(16.0f, 16.0f, 16.0f, 24.0f)
            .Child(FlexBox.Column(
                    new DashboardText("Media assets", 20.0f, DemoTextRecipe.SectionTitle),
                    VerticalSpacer(8.0f),
                    new DashboardText("Compare texture-backed images with SVG-backed assets and an app-owned premultiplied RGBA bitmap upload.", 14.0f, DemoTextRecipe.Supporting),
                    VerticalSpacer(12.0f),
                    // The tiles wrap onto another line when the card is too narrow for two.
                    FlexBox.Row(
                            MediaFrame(DemoSurfaceRecipe.InsetPanel, 24.0f, MediaTextureImage, MediaTextureCaptionText).Margin(0.0f, 0.0f, 12.0f, 12.0f),
                            MediaFrame(DemoSurfaceRecipe.InsetPanel, 24.0f, MediaSvgImage, MediaSvgCaptionText).Margin(0.0f, 0.0f, 0.0f, 12.0f))
                        .FlexWrap(FlexWrap.Wrap)
                        .Width(DemoTokens.FullSize, Unit.Percent),
                    FlexBox.Row(
                            MediaFrame(DemoSurfaceRecipe.FlatFrame, 28.0f, MediaSecondaryTextureImage, MediaSecondaryTextureCaptionText).Margin(0.0f, 0.0f, 12.0f, 12.0f),
                            new DemoSurface(DemoSurfaceRecipe.FlatFrame)
                                .Width(216.0f)
                                .Height(195.0f)
                                .Margin(0.0f, 0.0f, 0.0f, 12.0f)
                                .Child(FlexBox.Column(bitmapImage, VerticalSpacer(8.0f), new DashboardText("Custom drawn bitmap", 14.0f, DemoTextRecipe.StatusValue))))
                        .FlexWrap(FlexWrap.Wrap)
                        .Width(DemoTokens.FullSize, Unit.Percent))
                .Width(DemoTokens.FullSize, Unit.Percent));
    }

    private static FlexBox MediaFrame(DemoSurfaceRecipe recipe, float bottomPadding, Node media, Text caption) => new DemoSurface(recipe)
        .Width(Length.Auto)
        .Padding(24.0f, 24.0f, 24.0f, bottomPadding)
        .Child(FlexBox.Column(media, VerticalSpacer(8.0f), caption));

    private FlexBox BuildNestedScrollCard()
    {
        NestedScrollContent.Child(FlexBox.Column(
                new DashboardText("Nested origin marker", 16.0f, DemoTextRecipe.StatusValue),
                VerticalSpacer(72.0f),
                new DashboardText("Long horizontal note: diagonal wheel and touch gestures should keep this inner viewport moving on both axes while the outer dashboard stays put.", 16.0f, DemoTextRecipe.Hint),
                VerticalSpacer(116.0f),
                new DashboardText("Nested far marker", 16.0f, DemoTextRecipe.StatusValue))
            .Width(DemoTokens.FullSize, Unit.Percent));
        nestedScrollBox.Child(NestedScrollContent);
        return new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(16.0f, 16.0f, 16.0f, 16.0f)
            .Child(FlexBox.Column(
                    new DashboardText("Nested scroll sandbox", 20.0f, DemoTextRecipe.SectionTitle),
                    VerticalSpacer(8.0f),
                    new DashboardText("A fixed inner viewport overflows in both directions so you can test nested wheel and touch scrolling without the outer page taking over.", 14.0f, DemoTextRecipe.Supporting),
                    VerticalSpacer(12.0f),
                    new DemoSurface(DemoSurfaceRecipe.InsetPanel)
                        .Width(DemoTokens.NestedScrollViewportWidth + 8.0f)
                        .Padding(4.0f, 4.0f, 4.0f, 4.0f)
                        .Child(nestedScrollBox))
                .Width(DemoTokens.FullSize, Unit.Percent));
    }

    private FlexBox BuildInspectorPanel() => new DemoSurface(DemoSurfaceRecipe.SectionPanel)
        .Padding(20.0f, 20.0f, 20.0f, 20.0f)
        .Child(FlexBox.Column(
                new DashboardText("Inspector", 24.0f, DemoTextRecipe.SectionTitle),
                VerticalSpacer(DemoTokens.SpacingMedium),
                ClockText,
                VerticalSpacer(DemoTokens.SpacingSmall),
                ListOffsetText,
                VerticalSpacer(DemoTokens.SpacingSmall),
                FirstVisibleText,
                VerticalSpacer(DemoTokens.SpacingSmall),
                RenderedCountText,
                VerticalSpacer(DemoTokens.SpacingSmall),
                new DashboardText(
                    "Open " + DemoRoutes.Home + " for the dashboard sample, " + DemoRoutes.AdvancedControls + " for Advanced controls, or " + DemoRoutes.TemplatedControls + " for the templated-controls showcase.",
                    15.0f,
                    DemoTextRecipe.Supporting),
                VerticalSpacer(DemoTokens.SpacingMedium),
                inspectorDivider,
                VerticalSpacer(DemoTokens.SpacingMedium),
                new DashboardText("This page favors predictable FlexBox, Row, and Column sizing so you can inspect layout behavior while Grid and additional ScrollView scenarios continue to expand.", 15.0f, DemoTextRecipe.Hint),
                VerticalSpacer(DemoTokens.SpacingMedium),
                new DashboardEventInspector())
            .Width(DemoTokens.FullSize, Unit.Percent));
}
