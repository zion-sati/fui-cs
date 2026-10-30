using System;
using System.Collections.Generic;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls;

/// <summary>
/// The route's own sections: transitions and smooth scrolling, the worker with its progress bar, static rich text,
/// app-authored fonts and auto-sized boxes. The TextArea, reorder and fetch sections are their own classes.
/// </summary>
internal sealed class AdvancedControlsView
{
    public const float AnimationScrollRowHeight = 80.0f;
    public const float AnimationScrollViewportHeight = 280.0f;
    private const int AnimationScrollRowCount = 18;
    private const float AnimationScrollLogicalTail = 240.0f;
    public const float AnimationScrollLogicalContentHeight = (AnimationScrollRowCount * AnimationScrollRowHeight) + AnimationScrollLogicalTail;
    private const float ActionRowButtonHeight = 60.0f;

    private readonly List<FlexBox> animationRowCards = [];
    private readonly List<Text> animationRowTitleTexts = [];
    private readonly List<Text> animationRowDetailTexts = [];
    private readonly FontFace customEmojiFace;
    private readonly FontFamily proofMonoFamily;

    public AdvancedControlsView()
    {
        customEmojiFace = FontFace.Load(DemoRoutes.SharedFontUrl("NotoColorEmoji.ttf"));
        var customBodyStack = FontStack.Load(DemoRoutes.SharedFontUrl("DejaVuSans.ttf")).Fallback(customEmojiFace);
        var customHeadingStack = FontStack.Load(DemoRoutes.SharedFontUrl("DejaVuSans-Bold.ttf")).Fallback(customEmojiFace);
        var proofMonoStack = FontStack.Load(DemoRoutes.SharedFontUrl("NotoSansMono-Regular.ttf")).Fallback(customEmojiFace);
        var proofMonoBoldStack = FontStack.Load(DemoRoutes.SharedFontUrl("NotoSansMono-Bold.ttf")).Fallback(customEmojiFace);
        var customFamily = FontFamily.RegularBold(customBodyStack, customHeadingStack);
        proofMonoFamily = FontFamily.RegularBold(proofMonoStack, proofMonoBoldStack);

        CustomFontHeadingText = new DemoText("Custom DejaVu FontStack sample 🌍", DemoTextRecipe.Accent)
            .FontFamily(customFamily)
            .FontWeight(FontWeight.Bold)
            .FontSize(22.0f);
        CustomFontBodyText = new DemoText(
                "Load DejaVu Sans through FontStack.Load(...), use DejaVu Bold for heavier text, and keep color emoji fallback without dropping to bridge-specific APIs.",
                DemoTextRecipe.Supporting)
            .FontFamily(customFamily)
            .FontSize(16.0f)
            .MaxLines(4);
        CustomFontDirectStackText = new DemoText("Apply a stack directly: Text.FontStack(customBodyStack, 17) ✨", DemoTextRecipe.Supporting)
            .FontStack(customBodyStack, 17.0f);
        CustomFontComparisonText = new DemoText("Bold family resolution stays intact: DejaVu Bold + emoji fallback 😄", DemoTextRecipe.Body)
            .FontFamily(customFamily)
            .FontWeight(FontWeight.Bold)
            .FontSize(18.0f);
        RichTextContainerText = new RichText()
            .FontFamily(customFamily)
            .FontWeight(FontWeight.Bold)
            .FontSize(20.0f)
            .LineHeight(28.0f)
            .MaxLines(1)
            .FillWidth();
        RichTextHelperText = new RichText()
            .FontFamily(customFamily)
            .FontWeight(FontWeight.Bold)
            .FontSize(18.0f)
            .LineHeight(26.0f)
            .MaxLines(1)
            .FillWidth();

        AnimationPreviewTitleText = new DemoText("Calm transition target", DemoTextRecipe.SectionTitle).FontSize(18.0f);
        AnimationPreviewBodyText = new DemoText(
                "Opacity and background transitions stay on the same retained node while the control layer keeps behavior ownership elsewhere.",
                DemoTextRecipe.Supporting)
            .FontSize(15.0f)
            .MaxLines(3);
        AnimationPreviewCard = new FlexBox()
            .FillWidth()
            .Height(144.0f)
            .Padding(20.0f, 18.0f, 20.0f, 18.0f)
            .CornerRadius(20.0f)
            .Child(FlexBox.Column(AnimationPreviewTitleText, VerticalSpacer(8.0f), AnimationPreviewBodyText).FillWidth())
            .Transitions(new NodeTransitions
            {
                Background = new AnimationTiming(TimeSpan.FromMilliseconds(440.0), Easings.CubicOut),
                Opacity = new AnimationTiming(TimeSpan.FromMilliseconds(360.0), Easings.CubicOut),
            });
        var animationScrollContent = new FlexBox().FillWidth();
        for (var index = 0; index < AnimationScrollRowCount; index += 1)
        {
            var title = new DemoText("Animation sample row " + (index + 1).ToString(CultureInfo.InvariantCulture), DemoTextRecipe.Body)
                .FontSize(16.0f);
            var detail = new DemoText(
                    index == AnimationScrollRowCount - 1
                        ? "The final target proves retained smooth scrolling can drive to the far end of the viewport."
                        : "Retained content stays pooled and composable while the viewport animates independently.",
                    DemoTextRecipe.Hint)
                .FontSize(14.0f)
                .MaxLines(2);
            var rowCard = new FlexBox()
                .FillWidth()
                .Height(AnimationScrollRowHeight)
                .Padding(16.0f, 12.0f, 16.0f, 12.0f)
                .CornerRadius(14.0f)
                .Child(FlexBox.Column(title, VerticalSpacer(4.0f), detail).FillWidth());
            animationScrollContent.Child(rowCard);
            animationRowCards.Add(rowCard);
            animationRowTitleTexts.Add(title);
            animationRowDetailTexts.Add(detail);
        }
        AnimationScrollBox = new DemoScrollBox()
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .ScrollContentSize(null, AnimationScrollLogicalContentHeight)
            .FillWidth()
            .Height(AnimationScrollViewportHeight)
            .NodeId("advanced-controls:animation-scroll")
            .Child(animationScrollContent);
        WorkerDetailText.Text = "Press Start prime worker to compute primes for 5 seconds in the background.";
        SetAnimationPreviewState(false, Theme.Active.Value);
        ApplyTheme(Theme.Active.Value);
    }

    public DemoProgressBar WorkerProgressBar { get; } = new DemoProgressBar(0.0f).Length(320.0f);

    public DemoButton WorkerStartButton { get; } = ActionButton("Start prime worker", 170.0f);

    public DemoButton WorkerCancelButton { get; } = ActionButton("Cancel prime worker", 170.0f);

    public Text WorkerStatusText { get; } = StatusText();

    public Text WorkerDetailText { get; } = SupportingStatusText();

    public RichText RichTextContainerText { get; }

    public RichText RichTextHelperText { get; }

    public Text CustomFontHeadingText { get; }

    public Text CustomFontBodyText { get; }

    public Text CustomFontDirectStackText { get; }

    public Text CustomFontComparisonText { get; }

    public FlexBox AnimationPreviewCard { get; }

    public Text AnimationPreviewTitleText { get; }

    public Text AnimationPreviewBodyText { get; }

    public DemoButton AnimationPreviewCalmButton { get; } = ActionButton("Set calm preview", 170.0f);

    public DemoButton AnimationPreviewEmphasisButton { get; } = ActionButton("Emphasize preview card", 190.0f, DemoButtonTone.Primary);

    public DemoButton AnimationScrollTopButton { get; } = ActionButton("Scroll to first sample", 180.0f);

    public DemoButton AnimationScrollMiddleButton { get; } = ActionButton("Scroll to 7th sample", 190.0f);

    public DemoButton AnimationScrollBottomButton { get; } = ActionButton("Scroll to 13th sample", 180.0f, DemoButtonTone.Primary);

    public DemoButton AnimationScrollTailButton { get; } = ActionButton("Scroll to logical tail", 170.0f);

    public Text AnimationPreviewStatusText { get; } = StatusText();

    public Text AnimationScrollStatusText { get; } = SupportingStatusText();

    public DemoScrollBox AnimationScrollBox { get; }

    private Text WorkerHintText { get; } = Hint(
        "This sample connects the Worker API to a retained ProgressBar. Start runs a 5-second prime search with once-per-second yields; cancel waits for the next yield and then reports cooperative cancellation.",
        4);

    private Text RichTextContainerHintText { get; } = Hint("Compare container-level typography with a monospace override applied to one span.", 2);

    private Text RichTextHelperHintText { get; } = Hint("This example combines helper-span decorations with a color emoji face.", 2);

    private Text AnimationHintText { get; } = Hint(
        "Use the preview buttons to drive typed bgColor/opacity transitions, then jump the retained ScrollBox with scrollToAnimated(...). The logical-tail button proves scrollContentSize(...) can extend the range beyond the mounted subtree.",
        4);

    /// <summary>The sections in the reference's order, before the controller splices in reorder and fetch.</summary>
    public IReadOnlyList<RoutePageSection> BuildSections() =>
        [BuildAnimationSection(), BuildWorkerSection(), BuildRichTextSection(), BuildCustomFontSection(), BuildAutoSizingSection()];

    public void ApplyTheme(Theme theme)
    {
        SyncRichTextTheme(theme);
        SyncAnimationTheme(theme);
    }

    public void SetAnimationPreviewState(bool emphasized, Theme theme)
    {
        AnimationPreviewTitleText.Text = emphasized ? "Emphasized transition target" : "Calm transition target";
        AnimationPreviewBodyText.Text = emphasized
            ? "The preview card now drives both opacity and background transitions together from one typed slot set."
            : "Opacity and background transitions stay on the same retained node while the control layer keeps behavior ownership elsewhere.";
        AnimationPreviewCard
            .Background(emphasized ? theme.Colors.AccentHovered : DemoColors.CardBackground(theme))
            .Opacity(emphasized ? 1.0f : 0.7f);
    }

    private static FlexBox VerticalSpacer(float height) => new FlexBox().FillWidth().Height(height);

    private static FlexBox Gap() => new FlexBox().Width(12.0f).Height(1.0f);

    private static DemoButton ActionButton(string label, float width, DemoButtonTone tone = DemoButtonTone.Secondary) =>
        new DemoButton(label, tone).Width(width).Height(ActionRowButtonHeight);

    private static Text StatusText() => new DemoText("", DemoTextRecipe.StatusValue).FontSize(15.0f);

    private static Text SupportingStatusText() => new DemoText("", DemoTextRecipe.StatusSupporting).FontSize(15.0f);

    private static Text Hint(string text, int maxLines) => new DemoText(text, DemoTextRecipe.Hint).FontSize(15.0f).MaxLines(maxLines);

    private RoutePageSection BuildAnimationSection() => RoutePageSection.Create(
        "Transitions + scroll surfaces",
        "Drive the shipped animation APIs live: typed bgColor/opacity transitions plus smooth scroll and explicit scroll-content sizing on a retained ScrollBox.",
        FlexBox.Column(
                AnimationPreviewCard,
                VerticalSpacer(14.0f),
                FlexBox.Row(AnimationPreviewCalmButton, Gap(), AnimationPreviewEmphasisButton).FillWidth().AlignItems(AlignItems.Stretch),
                VerticalSpacer(14.0f),
                AnimationScrollBox,
                VerticalSpacer(14.0f),
                FlexBox.Row(AnimationScrollTopButton, Gap(), AnimationScrollMiddleButton, Gap(), AnimationScrollBottomButton)
                    .FillWidth()
                    .AlignItems(AlignItems.Stretch),
                VerticalSpacer(10.0f),
                FlexBox.Row(AnimationScrollTailButton).FillWidth(),
                VerticalSpacer(10.0f),
                AnimationPreviewStatusText,
                VerticalSpacer(6.0f),
                AnimationScrollStatusText,
                VerticalSpacer(10.0f),
                AnimationHintText)
            .FillWidth());

    private RoutePageSection BuildWorkerSection() => RoutePageSection.Create(
        "ProgressBar + Worker sample",
        "Use a determinate ProgressBar to visualize background work without dropping to raw Web Worker APIs.",
        FlexBox.Column(
                WorkerProgressBar,
                VerticalSpacer(12.0f),
                FlexBox.Row(WorkerStartButton, Gap(), WorkerCancelButton).FillWidth().AlignItems(AlignItems.Stretch),
                VerticalSpacer(10.0f),
                WorkerStatusText,
                VerticalSpacer(6.0f),
                WorkerDetailText,
                VerticalSpacer(10.0f),
                WorkerHintText)
            .FillWidth());

    private RoutePageSection BuildRichTextSection() => RoutePageSection.Create(
        "Static rich text",
        "Use helper spans to compose inline static styling, and use RichText container defaults when you want the same font family, weight, size, or color across the whole object.",
        FlexBox.Column(
                RichTextContainerText,
                VerticalSpacer(10.0f),
                RichTextContainerHintText,
                VerticalSpacer(16.0f),
                RichTextHelperText,
                VerticalSpacer(10.0f),
                RichTextHelperHintText)
            .FillWidth());

    private RoutePageSection BuildCustomFontSection() => RoutePageSection.Create(
        "App-authored custom fonts",
        "Use FontStack.Load(...) and FontFamily helpers to register app-authored fonts with emoji-capable fallback.",
        FlexBox.Column(
                CustomFontHeadingText,
                VerticalSpacer(10.0f),
                CustomFontBodyText,
                VerticalSpacer(10.0f),
                CustomFontDirectStackText,
                VerticalSpacer(8.0f),
                CustomFontComparisonText)
            .FillWidth());

    private static RoutePageSection BuildAutoSizingSection()
    {
        var theme = Theme.Active.Value;
        var firstBox = FlexBox.Column(
                new Text("Auto-sized box").FontSize(16.0f),
                VerticalSpacer(8.0f),
                new Text("This box is width: auto, height: auto with padding and a border. It sizes to fit its content exactly.").FontSize(14.0f))
            .Width(Length.Auto)
            .Height(Length.Auto)
            .Padding(12.0f, 10.0f, 12.0f, 10.0f)
            .Border(1.0f, DemoColors.DividerColor(theme))
            .Background(DemoColors.CardBackground(theme));
        var secondBox = FlexBox.Column(
                new Text("Another auto box").FontSize(16.0f),
                VerticalSpacer(10.0f),
                new Text("Item 1").FontSize(13.0f),
                VerticalSpacer(6.0f),
                new Text("Item 2").FontSize(13.0f),
                VerticalSpacer(6.0f),
                new Text("Item 3").FontSize(13.0f))
            .Width(Length.Auto)
            .Height(Length.Auto)
            .Padding(16.0f, 14.0f, 16.0f, 14.0f)
            .Border(2.0f, DemoColors.PrimaryText(theme))
            .Background(Color.FromRgb(220, 38, 38));
        return RoutePageSection.Create(
            "Auto sizing",
            "Use Unit.Auto for width and height to create containers that size exactly to their children, with full support for padding, borders, and margins.",
            FlexBox.Column(
                    FlexBox.Row(firstBox.Margin(0.0f, 0.0f, 20.0f, 12.0f), secondBox.Margin(0.0f, 0.0f, 0.0f, 12.0f)).FlexWrap(FlexWrap.Wrap),
                    VerticalSpacer(14.0f),
                    new DemoText(
                            "Auto-sized containers shrink to their content, ignoring flex growth. Perfect for cards, panels, and labels that need custom borders and padding without hardcoded dimensions.",
                            DemoTextRecipe.Hint)
                        .FontSize(14.0f))
                .FillWidth());
    }

    private void SyncRichTextTheme(Theme theme)
    {
        var primary = DemoTextRecipes.ColorOf(theme, DemoTextRecipe.Body);
        RichTextContainerText.TextColor = primary;
        RichTextContainerText.SetSpans(
            new Span("Base family ").Underline().Color(primary),
            new Span("with ").Color(primary),
            new Span("MONO OVERRIDE").FontFamily(proofMonoFamily).Strikethrough());
        var emojiBackground = Color.FromRgb(30, 41, 59);
        var emojiColor = Color.FromRgb(167, 243, 208);
        RichTextHelperText.TextColor = primary;
        RichTextHelperText.SetSpans(
            new Span("Rich ").Bold().Color(primary),
            new Span("text ").Italic().Color(Color.FromRgb(96, 165, 250)),
            new Span("underline ").Underline().Color(Color.FromRgb(251, 191, 36)),
            new Span("strike ").Strikethrough().Color(Color.FromRgb(248, 113, 113)),
            new Span("emoji- ").Background(emojiBackground).Color(emojiColor),
            new Span("😄").FontFamily(FontFamily.WithRegular(customEmojiFace)).Background(emojiBackground).Color(emojiColor),
            new Span(" ").Background(emojiBackground).Color(emojiColor),
            new Span("helpers").Bold().Italic().Underline().Strikethrough().Color(Color.FromRgb(203, 213, 225)));
    }

    private void SyncAnimationTheme(Theme theme)
    {
        AnimationPreviewTitleText.TextColor = DemoColors.PrimaryText(theme);
        AnimationPreviewBodyText.TextColor = DemoColors.MutedText(theme);
        AnimationPreviewCard.Border(1.0f, DemoColors.DividerColor(theme));
        DemoScrollBars.ApplyTheme(AnimationScrollBox, theme);
        for (var index = 0; index < animationRowCards.Count; index += 1)
        {
            animationRowCards[index]
                .Background((index & 1) == 0 ? DemoColors.CardBackground(theme) : DemoColors.CardBackgroundAlt(theme))
                .Border(1.0f, DemoColors.DividerColor(theme));
            animationRowTitleTexts[index].TextColor = DemoColors.PrimaryText(theme);
            animationRowDetailTexts[index].TextColor = DemoColors.MutedText(theme);
        }
    }
}
