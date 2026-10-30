namespace FuiCs;

/// <summary>The default dropdown field: a bordered, rounded row with the value fading at its right edge, then the chevron's box.</summary>
internal sealed class DefaultDropdownFieldPresenter : DropdownFieldPresenter
{
    private const float DefaultChevronBoxSize = 16.0f;
    private const float DefaultPaddingX = 16.0f;
    private const float DefaultFontSize = 16.0f;
    private const float DefaultHeight = 32.0f;

    private static readonly DropdownFieldMetrics DefaultMetrics =
        new(DefaultHeight, DefaultFontSize, DefaultChevronBoxSize, DefaultPaddingX, 0.0f, DefaultPaddingX, 0.0f);

    private DefaultDropdownFieldPresenter(DropdownFieldMetrics metrics, Text valueNode, FlexBox valueHost, FlexBox chevronHost)
        : base(FlexBox.Row(valueHost, chevronHost).AlignItems(AlignItems.Center), valueHost, valueNode, chevronHost, metrics)
    {
    }

    public static DropdownFieldPresenter Create(DropdownSizing? sizing)
    {
        var metrics = ResolveMetrics(sizing);
        var valueNode = new Text()
            .Selectable(false)
            .FillSize()
            .MaxLines(1)
            .Wrapping(false)
            .OverflowFade(horizontal: true, vertical: false)
            .VerticalAlign(TextVerticalAlign.Center);
        // The value fades at the host's edge, which clips the rest of a long label.
        var valueHost = new FlexBox().FillSize().ClipToBounds(true).Child(valueNode);
        var chevronHost = new FlexBox()
            .Width(metrics.ChevronBoxSize)
            .Height(metrics.ChevronBoxSize)
            .AlignItems(AlignItems.Center)
            .JustifyContent(JustifyContent.Center);
        return new DefaultDropdownFieldPresenter(metrics, valueNode, valueHost, chevronHost);
    }

    /// <summary>The default metrics, with the sizing's values; an unset height grows to fit the font and chevron.</summary>
    internal static DropdownFieldMetrics ResolveMetrics(DropdownSizing? sizing)
    {
        if (sizing is null || (sizing.FieldHeight is null && sizing.FieldFontSize is null && sizing.ChevronBoxSize is null))
        {
            return DefaultMetrics;
        }
        var fontSize = sizing.FieldFontSize ?? DefaultFontSize;
        var chevronBoxSize = sizing.ChevronBoxSize ?? DefaultChevronBoxSize;
        var height = sizing.FieldHeight ?? Math.Max(DefaultHeight, Math.Max(fontSize, chevronBoxSize));
        return DefaultMetrics with { Height = height, FontSize = fontSize, ChevronBoxSize = chevronBoxSize };
    }

    public override void Apply(Theme theme, DropdownFieldVisualState state, DropdownColors? colors)
    {
        var palette = theme.Colors;
        var metrics = Metrics;
        var contentHeight = Math.Max(metrics.FontSize, metrics.Height - metrics.PaddingTop - metrics.PaddingBottom);
        var background = colors?.Background ?? (state.IsPressed && state.IsEnabled ? palette.Background : palette.Surface);
        Root.FlexDirection(FlexDirection.Row)
            .AlignItems(AlignItems.Center)
            .Height(metrics.Height)
            .CornerRadius(theme.Spacing.Sm)
            .Border(2.0f, colors?.Border ?? palette.Border)
            .Padding(metrics.PaddingLeft, metrics.PaddingTop, metrics.PaddingRight, metrics.PaddingBottom)
            .Background(background);
        ValueHost.FillSize();
        ValueNode.FontFamily(theme.Fonts.BodyFamily)
            .FontSize(metrics.FontSize)
            .LineHeight(contentHeight)
            .TextColor(!state.IsEnabled ? palette.TextMuted : colors?.TextPrimary ?? palette.TextPrimary);
        ChevronHost.Width(metrics.ChevronBoxSize)
            .Height(metrics.ChevronBoxSize)
            .AlignItems(AlignItems.Center)
            .JustifyContent(JustifyContent.Center);
    }
}
