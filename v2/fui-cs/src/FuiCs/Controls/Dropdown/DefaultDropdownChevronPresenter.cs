namespace FuiCs;

/// <summary>The default dropdown chevron: a down arrow that turns up while the list is open, muted until hovered.</summary>
internal sealed class DefaultDropdownChevronPresenter : DropdownChevronPresenter
{
    private const string Collapsed =
        "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'>"
        + "<path d='M3 4.5 6 7.5 9 4.5' fill='none' stroke='%23000000' stroke-width='1.6' stroke-linecap='round' stroke-linejoin='round'/></svg>";

    private const string Expanded =
        "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'>"
        + "<path d='M3 7.5 6 4.5 9 7.5' fill='none' stroke='%23000000' stroke-width='1.6' stroke-linecap='round' stroke-linejoin='round'/></svg>";

    private const float DefaultIconSize = 12.0f;

    private readonly float iconSize;
    private readonly Svg icon;

    private DefaultDropdownChevronPresenter(float iconSize, Svg icon)
        : base(new FlexBox().FillSize().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center).Child(icon))
    {
        this.iconSize = iconSize;
        this.icon = icon;
    }

    /// <summary>The icon's size: the sizing's chevron icon size, or 12 px.</summary>
    internal float IconSize => iconSize;

    public static DropdownChevronPresenter Create(DropdownSizing? sizing)
    {
        var size = sizing?.ChevronIconSize ?? DefaultIconSize;
        return new DefaultDropdownChevronPresenter(size, new Svg().Width(size).Height(size));
    }

    public override void Apply(Theme theme, DropdownChevronVisualState state)
    {
        var palette = theme.Colors;
        Root.FillSize().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center);
        icon.Width(iconSize).Height(iconSize);
        var source = state.IsOpen ? Expanded : Collapsed;
        if (icon.Source != source)
        {
            icon.Source = source;
        }
        icon.Tint = state.IsEnabled && state.IsHovered ? palette.TextPrimary : palette.TextMuted;
    }
}
