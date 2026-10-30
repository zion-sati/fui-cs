namespace FuiCs;

/// <summary>The default checkbox indicator: a rounded box filled with the accent and a check mark when checked.</summary>
internal sealed class CheckboxIndicator : IndicatorPresenter
{
    private const string CheckMark =
        "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 14 14'>"
        + "<path d='M2.25 7.15 5.35 10.25 11.75 3.85' fill='none' stroke='%23000000' stroke-width='2.4' "
        + "stroke-linecap='round' stroke-linejoin='round'/></svg>";

    private readonly float cornerRadius;
    private readonly Svg mark;

    private CheckboxIndicator(float size)
        : base(new FlexBox().Width(size).Height(size).AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center))
    {
        cornerRadius = size * 0.2f;
        var markSize = size * 0.8f;
        mark = new Svg().Width(markSize).Height(markSize);
        Root.Child(new FlexBox().FillSize().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center).Child(mark));
    }

    public static IndicatorPresenter Create(LabeledControlSizing? sizing) => new CheckboxIndicator(sizing?.IndicatorSize ?? 20.0f);

    public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
    {
        var palette = theme.Colors;
        var background = colors?.Background ?? palette.Surface;
        var border = colors?.Border ?? palette.Border;
        var accent = colors?.Accent ?? (state.IsPressed ? palette.AccentPressed : state.IsHovered ? palette.AccentHovered : palette.Accent);
        var markVisible = false;
        if (state.IsChecked != false)
        {
            background = accent;
            border = accent;
            markVisible = state.IsChecked == true;
        }
        else if (state.IsHovered && colors?.Background is null)
        {
            background = palette.Background;
        }
        Root.CornerRadius = new CornerRadius(cornerRadius);
        Root.Border = new Border(1.0f, border);
        Root.Background = background;
        mark.Source = markVisible ? CheckMark : null;
        mark.Opacity = markVisible ? 1.0f : 0.0f;
        mark.Tint = palette.TextOnAccent;
    }
}
