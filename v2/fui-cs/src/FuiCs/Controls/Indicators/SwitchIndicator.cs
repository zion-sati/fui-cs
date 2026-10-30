namespace FuiCs;

/// <summary>The default switch indicator: a pill track whose thumb slides to the end and fills with the accent when on.</summary>
internal sealed class SwitchIndicator : IndicatorPresenter
{
    private const float TrackWidth = 44.0f;
    private const float TrackHeight = 26.0f;
    private const float ThumbSize = 20.0f;
    private const float ThumbInsetX = 3.0f;
    private const float ThumbInsetY = 2.0f;

    private readonly float height;
    private readonly float thumbSize;
    private readonly float thumbX;
    private readonly float thumbCheckedX;
    private readonly float thumbY;
    private readonly FlexBox thumb;

    private SwitchIndicator(float scale)
        : base(new FlexBox().Width(TrackWidth * scale).Height(TrackHeight * scale).ClipToBounds(true).AlignItems(AlignItems.Center))
    {
        height = TrackHeight * scale;
        thumbSize = ThumbSize * scale;
        thumbX = ThumbInsetX * scale;
        thumbCheckedX = (TrackWidth * scale) - thumbSize - thumbX;
        thumbY = ThumbInsetY * scale;
        thumb = new FlexBox().PositionAbsolute().Position(thumbX, thumbY).Width(thumbSize).Height(thumbSize).CornerRadius(thumbSize * 0.5f);
        Root.Child(thumb);
    }

    /// <summary>The sizing's indicator size is the track height; the track and thumb scale with it.</summary>
    public static IndicatorPresenter Create(LabeledControlSizing? sizing) => new SwitchIndicator((sizing?.IndicatorSize ?? TrackHeight) / TrackHeight);

    public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
    {
        var palette = theme.Colors;
        var on = state.IsChecked == true;
        var accent = colors?.Accent ?? (state.IsPressed ? palette.AccentPressed : state.IsHovered ? palette.AccentHovered : palette.Accent);
        var track = on ? accent : colors?.Background ?? (state.IsHovered ? palette.Background : palette.Surface);
        var border = colors?.Border ?? (on ? track : palette.Border);
        Root.CornerRadius = new CornerRadius(height * 0.5f);
        Root.Border = new Border(1.0f, border);
        Root.Background = track;
        thumb.Position(on ? thumbCheckedX : thumbX, thumbY);
        thumb.Background = colors?.Background ?? palette.Surface;
        thumb.Border = new Border(1.0f, border);
    }
}
