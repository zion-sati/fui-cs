namespace FuiCs;

/// <summary>The default slider parts: a rounded track, an accent fill up to the value and a round thumb.</summary>
internal sealed class SliderParts : SliderPresenter
{
    private readonly float trackThickness;
    private readonly FlexBox track;
    private readonly FlexBox fill;
    private readonly FlexBox thumb;

    private SliderParts(float thumbSize, float trackThickness)
        : base(new FlexBox(), thumbSize)
    {
        this.trackThickness = Math.Clamp(trackThickness, 1.0f, ThumbSize);
        track = new FlexBox().PositionAbsolute();
        fill = new FlexBox().PositionAbsolute();
        thumb = new FlexBox().PositionAbsolute().Width(ThumbSize).Height(ThumbSize);
        Root.Child(track).Child(fill).Child(thumb);
    }

    public static SliderPresenter Create(SliderSizing? sizing) => new SliderParts(sizing?.ThumbSize ?? 18.0f, sizing?.TrackThickness ?? 6.0f);

    public override void Layout(SliderVisualState state, float length)
    {
        var available = Math.Max(0.0f, length - ThumbSize);
        var fraction = Math.Clamp(state.NormalizedValue, 0.0f, 1.0f);
        var crossInset = CrossAxisExtra * 0.5f;
        var trackOffset = crossInset + ((ThumbSize - trackThickness) * 0.5f);
        var halfThumb = ThumbSize * 0.5f;
        if (state.Orientation == Orientation.Vertical)
        {
            var fillLength = MathF.Round(available * fraction);
            Root.Width = ThumbSize + CrossAxisExtra;
            Root.Height = length;
            track.Width(trackThickness).Height(available).Position(trackOffset, halfThumb);
            fill.Width(trackThickness).Height(fillLength).Position(trackOffset, halfThumb + available - fillLength);
            thumb.Position(crossInset, available - (available * fraction));
            return;
        }
        Root.Width = length;
        Root.Height = ThumbSize + CrossAxisExtra;
        track.Width(available).Height(trackThickness).Position(halfThumb, trackOffset);
        fill.Width(available * fraction).Height(trackThickness).Position(halfThumb, trackOffset);
        thumb.Position(available * fraction, crossInset);
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
