namespace FuiCs;

/// <summary>The default radio indicator: a ring with a centered dot when checked.</summary>
internal sealed class RadioIndicator : IndicatorPresenter
{
    private readonly float size;
    private readonly float borderWidth;
    private readonly FlexBox dot;

    private RadioIndicator(float size)
        : base(new FlexBox().Width(size).Height(size).AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center))
    {
        this.size = size;
        borderWidth = size >= 24.0f ? 2.0f : 1.0f;
        var dotSize = size * 0.4f;
        var inset = Math.Max(0.0f, ((size - dotSize) * 0.5f) - borderWidth);
        dot = new FlexBox().PositionAbsolute().Position(inset, inset).Width(dotSize).Height(dotSize).CornerRadius(dotSize * 0.5f);
        Root.Child(dot);
    }

    public static IndicatorPresenter Create(LabeledControlSizing? sizing) => new RadioIndicator(sizing?.IndicatorSize ?? 20.0f);

    public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
    {
        var palette = theme.Colors;
        var accent = colors?.Accent ?? (state.IsPressed ? palette.AccentPressed : state.IsHovered ? palette.AccentHovered : palette.Accent);
        var ring = state.IsChecked == true ? accent : colors?.Border ?? palette.Border;
        Root.CornerRadius = new CornerRadius(size * 0.5f);
        Root.Border = new Border(borderWidth, ring);
        Root.Background = colors?.Background ?? palette.Surface;
        dot.Background = ring;
        dot.Opacity = state.IsChecked == true ? 1.0f : 0.0f;
    }
}
