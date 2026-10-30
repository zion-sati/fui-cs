namespace FuiCs;

/// <summary>
/// The default option row: a padded row whose highlight is the context menu's hover color, with the selected option's
/// label in the accent.
/// </summary>
internal sealed class DefaultDropdownOptionRowPresenter : DropdownOptionRowPresenter
{
    private static readonly DropdownOptionRowMetrics DefaultMetrics = new(34.0f, 10.0f, 10.0f, 16.0f);

    // The label sits in a host that clips the rest of a long label where its fade ends; the row's padding surrounds it.
    private DefaultDropdownOptionRowPresenter(DropdownOptionRowMetrics metrics, Text label)
        : base(
            new FlexBox().FillSize().AlignItems(AlignItems.Center)
                .Child(new FlexBox().FillSize().AlignItems(AlignItems.Center).ClipToBounds(true).Child(label)),
            label,
            metrics)
    {
    }

    public static DropdownOptionRowPresenter Create(DropdownSizing? sizing)
    {
        var label = new Text()
            .Selectable(false)
            .FillSize()
            .MaxLines(1)
            .Wrapping(false)
            .OverflowFade(horizontal: true, vertical: false)
            .VerticalAlign(TextVerticalAlign.Center);
        return new DefaultDropdownOptionRowPresenter(ResolveMetrics(sizing), label);
    }

    internal static DropdownOptionRowMetrics ResolveMetrics(DropdownSizing? sizing) =>
        sizing is null ? DefaultMetrics : DefaultMetrics with
        {
            Height = sizing.OptionHeight ?? DefaultMetrics.Height,
            FontSize = sizing.OptionFontSize ?? DefaultMetrics.FontSize,
        };

    public override void Apply(Theme theme, DropdownOptionRowVisualState state, DropdownColors? colors)
    {
        var palette = theme.Colors;
        Root.Padding(Metrics.PaddingLeft, 0.0f, Metrics.PaddingRight, 0.0f)
            .CornerRadius(theme.Spacing.Xs)
            .Background(state.IsHighlighted ? theme.ContextMenu.Item.HoverBackground : Color.Transparent);
        var labelColor = !state.IsEnabled
            ? palette.TextMuted
            : state.IsSelected ? colors?.Accent ?? palette.Accent : colors?.TextPrimary ?? palette.TextPrimary;
        Label.FontFamily(theme.Fonts.BodyFamily).FontSize(Metrics.FontSize).TextColor(labelColor);
    }
}
