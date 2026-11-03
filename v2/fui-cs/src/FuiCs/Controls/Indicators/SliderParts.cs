namespace FuiCs;

/// <summary>
/// The default slider parts: a rounded track, an accent fill up to the value and a round thumb. A rail between the
/// thumb's centre at each end holds the track, the fill and a spacer as long as the value's share of the rail; the thumb
/// follows the spacer, shifted back half its size onto the spacer's end.
/// </summary>
internal sealed class SliderParts : SliderPresenter
{
    private readonly float trackThickness;
    private readonly FlexBox rail;
    private readonly FlexBox track;
    private readonly FlexBox fillBox;
    private readonly FlexBox fill;
    private readonly FlexBox spacer;
    private readonly FlexBox thumb;

    private SliderParts(float thumbSize, float trackThickness)
        : base(new FlexBox(), thumbSize)
    {
        this.trackThickness = Math.Clamp(trackThickness, 1.0f, ThumbSize);
        track = new FlexBox().PositionAbsolute();
        fill = new FlexBox().Width(Length.Percent(0.0f)).Height(Length.Percent(100.0f));
        fillBox = new FlexBox().PositionAbsolute().Child(fill);
        spacer = new FlexBox().Width(Length.Percent(0.0f)).Height(1.0f);
        thumb = new FlexBox().Width(ThumbSize).Height(ThumbSize);
        rail = new FlexBox().AlignItems(AlignItems.Center).Child(track).Child(fillBox).Child(spacer).Child(thumb);
        Root.Child(rail);
    }

    public static SliderPresenter Create(SliderSizing? sizing) => new SliderParts(sizing?.ThumbSize ?? 18.0f, sizing?.TrackThickness ?? 6.0f);

    public override void Layout(SliderVisualState state)
    {
        var percent = Math.Clamp(state.NormalizedValue, 0.0f, 1.0f) * 100.0f;
        var crossInset = CrossAxisExtra * 0.5f;
        var trackOffset = (ThumbSize - trackThickness) * 0.5f;
        var halfThumb = ThumbSize * 0.5f;
        if (state.Orientation == Orientation.Vertical)
        {
            Root.FlexDirection = FlexDirection.Row;
            rail.FlexDirection(FlexDirection.Column).Width(ThumbSize).FillHeight().Margin(crossInset, halfThumb, crossInset, halfThumb);
            track.Width(trackThickness).Height(Length.Percent(100.0f)).Position(trackOffset, 0.0f);
            // The fill grows up from the rail's bottom.
            fillBox.FlexDirection(FlexDirection.Column)
                .JustifyContent(JustifyContent.End)
                .Width(trackThickness)
                .Height(Length.Percent(100.0f))
                .Position(trackOffset, 0.0f);
            fill.Width(Length.Percent(100.0f)).Height(Length.Percent(percent));
            spacer.Width(1.0f).Height(Length.Percent(100.0f - percent));
            thumb.Position(0.0f, -halfThumb);
            return;
        }
        Root.FlexDirection = FlexDirection.Column;
        rail.FlexDirection(FlexDirection.Row).Height(ThumbSize).FillWidth().Margin(halfThumb, crossInset, halfThumb, crossInset);
        track.Width(Length.Percent(100.0f)).Height(trackThickness).Position(0.0f, trackOffset);
        fillBox.FlexDirection(FlexDirection.Row)
            .JustifyContent(JustifyContent.Start)
            .Width(Length.Percent(100.0f))
            .Height(trackThickness)
            .Position(0.0f, trackOffset);
        fill.Width(Length.Percent(percent)).Height(Length.Percent(100.0f));
        spacer.Width(Length.Percent(percent)).Height(1.0f);
        thumb.Position(-halfThumb, 0.0f);
    }

    public override void Apply(Theme theme, SliderVisualState state, SliderColors? colors)
    {
        var palette = theme.Colors;
        var accent = state.IsDragging ? palette.AccentPressed : state.IsHovered ? palette.AccentHovered : palette.Accent;
        var radius = new CornerRadius(trackThickness * 0.5f);
        track.CornerRadius = radius;
        track.Background = colors?.Track ?? palette.ScrollbarTrack;
        fill.CornerRadius = radius;
        fill.Background = colors?.Fill ?? accent;
        thumb.CornerRadius = new CornerRadius(ThumbSize * 0.5f);
        thumb.Background = colors?.Thumb ?? accent;
        thumb.Border = new Border(1.0f, palette.Surface);
    }
}
