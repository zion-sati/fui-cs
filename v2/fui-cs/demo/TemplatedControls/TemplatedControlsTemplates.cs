using FuiCs;

namespace FuiCs.Demo.TemplatedControls;

/// <summary>
/// The route's house templates: rounded indicators, a pill switch, a thick-tracked slider and an outlined button, plus
/// the square checkbox the local override uses. Each presenter takes per-instance colors before the theme's.
/// </summary>
internal static class TemplatedControlsTemplates
{
    public static ButtonTemplate Button() => new(static () => new HouseButtonPresenter());

    public static IndicatorTemplate Checkbox() => new(static _ => new HouseCheckboxIndicatorPresenter());

    public static IndicatorTemplate LocalCheckbox() => new(static _ => new LocalOverrideCheckboxIndicatorPresenter());

    public static IndicatorTemplate Radio() => new(static _ => new HouseRadioIndicatorPresenter());

    public static IndicatorTemplate Switch() => new(static _ => new HouseSwitchIndicatorPresenter());

    public static SliderTemplate Slider() => new(static _ => new HouseSliderPresenter());

    private static Color ResolveAccent(Theme theme, bool hovered, bool pressed) =>
        pressed ? theme.Colors.AccentPressed : hovered ? theme.Colors.AccentHovered : theme.Colors.Accent;

    private static FlexBox IndicatorRoot(float width, float height) => new FlexBox()
        .Width(width)
        .Height(height)
        .AlignItems(AlignItems.Center)
        .JustifyContent(JustifyContent.Center);

    private sealed class HouseCheckboxIndicatorPresenter : IndicatorPresenter
    {
        private readonly FlexBox fill = new FlexBox().Width(12.0f).Height(12.0f);

        public HouseCheckboxIndicatorPresenter()
            : base(IndicatorRoot(24.0f, 24.0f))
        {
            Root.Child(fill);
        }

        public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
        {
            var accent = colors?.Accent ?? ResolveAccent(theme, state.IsHovered, state.IsPressed);
            var mixed = state.IsChecked is null;
            var isChecked = state.IsChecked != false;
            Root.CornerRadius(8.0f)
                .Border(2.0f, isChecked ? accent : colors?.Border ?? theme.Colors.Border)
                .Background(isChecked ? accent : colors?.Background ?? theme.Colors.Surface);
            fill.CornerRadius(mixed ? 3.0f : 6.0f)
                .Width(mixed ? 14.0f : 12.0f)
                .Height(mixed ? 6.0f : 12.0f)
                .Background(colors?.Background ?? theme.Colors.Surface)
                .Opacity(isChecked ? 1.0f : 0.0f);
        }
    }

    private sealed class LocalOverrideCheckboxIndicatorPresenter : IndicatorPresenter
    {
        private readonly FlexBox stripe = new FlexBox().Width(14.0f).Height(10.0f);

        public LocalOverrideCheckboxIndicatorPresenter()
            : base(IndicatorRoot(24.0f, 24.0f))
        {
            Root.Child(stripe);
        }

        public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
        {
            var accent = colors?.Accent ?? ResolveAccent(theme, state.IsHovered, state.IsPressed);
            var mixed = state.IsChecked is null;
            var isChecked = state.IsChecked != false;
            Root.CornerRadius(4.0f)
                .Border(2.0f, accent)
                .Background(isChecked ? colors?.Background ?? theme.Colors.Background : colors?.Background ?? theme.Colors.Surface);
            stripe.CornerRadius(mixed ? 2.0f : 5.0f)
                .Width(mixed ? 14.0f : 10.0f)
                .Height(mixed ? 6.0f : 14.0f)
                .Background(accent)
                .Opacity(isChecked ? 1.0f : 0.0f);
        }
    }

    private sealed class HouseRadioIndicatorPresenter : IndicatorPresenter
    {
        private readonly FlexBox dot = new FlexBox().Width(10.0f).Height(10.0f);

        public HouseRadioIndicatorPresenter()
            : base(IndicatorRoot(24.0f, 24.0f))
        {
            Root.Child(dot);
        }

        public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
        {
            var accent = colors?.Accent ?? ResolveAccent(theme, state.IsHovered, state.IsPressed);
            var isChecked = state.IsChecked == true;
            Root.CornerRadius(12.0f)
                .Border(2.0f, isChecked ? accent : colors?.Border ?? theme.Colors.Border)
                .Background(colors?.Background ?? theme.Colors.Surface);
            dot.CornerRadius(5.0f)
                .Background(accent)
                .Opacity(isChecked ? 1.0f : 0.0f);
        }
    }

    private sealed class HouseSwitchIndicatorPresenter : IndicatorPresenter
    {
        private readonly FlexBox thumb = new FlexBox().PositionAbsolute().Position(4.0f, 4.0f).Width(22.0f).Height(22.0f);

        public HouseSwitchIndicatorPresenter()
            : base(new FlexBox().Width(52.0f).Height(30.0f).ClipToBounds(true))
        {
            Root.Child(thumb);
        }

        public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
        {
            var accent = colors?.Accent ?? ResolveAccent(theme, state.IsHovered, state.IsPressed);
            var background = colors?.Background ?? theme.Colors.Surface;
            var border = colors?.Border ?? theme.Colors.Border;
            var isChecked = state.IsChecked == true;
            Root.CornerRadius(15.0f)
                .Border(2.0f, isChecked ? accent : border)
                .Background(isChecked ? accent : background);
            thumb.Position(isChecked ? 26.0f : 4.0f, 4.0f)
                .CornerRadius(11.0f)
                .Background(isChecked ? background : theme.Colors.Background)
                .Border(1.0f, isChecked ? accent : border);
        }
    }

    /// <summary>
    /// A 22 px thumb over an 8 px track, with 6 px of room across the slider, laid out relative to the root the slider
    /// sizes: a rail between the thumb's centre at each end holds the track, the fill and a spacer as long as the
    /// value's share, which the thumb follows.
    /// </summary>
    private sealed class HouseSliderPresenter : SliderPresenter
    {
        private const float TrackThickness = 8.0f;
        private readonly FlexBox rail = new FlexBox().AlignItems(AlignItems.Center);
        private readonly FlexBox track = new FlexBox().PositionAbsolute();
        private readonly FlexBox fillBox = new FlexBox().PositionAbsolute();
        private readonly FlexBox fill = new();
        private readonly FlexBox spacer = new();
        private readonly FlexBox thumb = new FlexBox().Width(22.0f).Height(22.0f);

        public HouseSliderPresenter()
            : base(new FlexBox(), 22.0f, 6.0f)
        {
            fillBox.Child(fill);
            rail.Child(track).Child(fillBox).Child(spacer).Child(thumb);
            Root.Child(rail);
        }

        public override void Layout(SliderVisualState state)
        {
            var percent = state.NormalizedValue * 100.0f;
            var crossAxisInset = CrossAxisExtra * 0.5f;
            var trackOffset = (ThumbSize - TrackThickness) * 0.5f;
            var halfThumb = ThumbSize * 0.5f;
            if (state.Orientation == Orientation.Vertical)
            {
                Root.FlexDirection(FlexDirection.Row);
                rail.FlexDirection(FlexDirection.Column).Width(ThumbSize).FillHeight().Margin(crossAxisInset, halfThumb, crossAxisInset, halfThumb);
                track.Width(TrackThickness).Height(Length.Percent(100.0f)).Position(trackOffset, 0.0f);
                fillBox.FlexDirection(FlexDirection.Column).JustifyContent(JustifyContent.End)
                    .Width(TrackThickness).Height(Length.Percent(100.0f)).Position(trackOffset, 0.0f);
                fill.Width(Length.Percent(100.0f)).Height(Length.Percent(percent));
                spacer.Width(1.0f).Height(Length.Percent(100.0f - percent));
                thumb.Position(0.0f, -halfThumb);
                return;
            }
            Root.FlexDirection(FlexDirection.Column);
            rail.FlexDirection(FlexDirection.Row).Height(ThumbSize).FillWidth().Margin(halfThumb, crossAxisInset, halfThumb, crossAxisInset);
            track.Width(Length.Percent(100.0f)).Height(TrackThickness).Position(0.0f, trackOffset);
            fillBox.FlexDirection(FlexDirection.Row).JustifyContent(JustifyContent.Start)
                .Width(Length.Percent(100.0f)).Height(TrackThickness).Position(0.0f, trackOffset);
            fill.Width(Length.Percent(percent)).Height(Length.Percent(100.0f));
            spacer.Width(Length.Percent(percent)).Height(1.0f);
            thumb.Position(-halfThumb, 0.0f);
        }

        public override void Apply(Theme theme, SliderVisualState state, SliderColors? colors)
        {
            var accent = colors?.Fill ?? (state.IsDragging ? theme.Colors.AccentPressed : ResolveAccent(theme, state.IsHovered, false));
            track.CornerRadius(4.0f).Background(colors?.Track ?? theme.Colors.Background).Border(1.0f, theme.Colors.Border);
            fill.CornerRadius(4.0f).Background(accent);
            thumb.CornerRadius(11.0f).Background(colors?.Thumb ?? theme.Colors.Surface).Border(2.0f, accent);
        }
    }

    /// <summary>A centered label in the body font, in the accent while enabled; the button keeps its own chrome.</summary>
    private sealed class HouseButtonPresenter : ButtonPresenter
    {
        public HouseButtonPresenter()
            : this(new TextCore().Selectable(false).MaxLines(1).Wrapping(false))
        {
        }

        private HouseButtonPresenter(TextCore label)
            : base(new FlexBox().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center).Child(label), label)
        {
        }

        public override void Apply(Theme theme, ButtonVisualState state, ButtonColors? colors)
        {
            var accent = ResolveAccent(theme, state.IsHovered, state.IsPressed);
            var textColor = !state.IsEnabled && colors?.TextMuted is { } muted
                ? muted
                : colors?.TextPrimary ?? (state.IsEnabled ? accent : theme.Colors.TextMuted);
            ContentRoot.AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center);
            Label.FontFamily(theme.Fonts.BodyFamily).FontSize(theme.Fonts.SizeBody).TextColor(textColor);
        }
    }
}
