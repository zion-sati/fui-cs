namespace FuiCs.Demo.DesignSystem;

/// <summary>A link in a route page's navigation bar.</summary>
public sealed record RoutePageNavItem(string RoutePath, string Label);

/// <summary>A route page's content: its heading text, badge, action button, navigation and notes, and the action count.</summary>
public sealed class RoutePageModel(
    string title,
    string description,
    string badge,
    Color accentColor,
    string buttonLabel,
    string counterLabel,
    IReadOnlyList<RoutePageNavItem> navItems,
    IReadOnlyList<string> highlights)
{
    public string Title { get; } = title;

    public string Description { get; } = description;

    public string Badge { get; } = badge;

    /// <summary>The accent until the shell reports its own.</summary>
    public Color AccentColor { get; } = accentColor;

    public string ButtonLabel { get; } = buttonLabel;

    public string CounterLabel { get; } = counterLabel;

    public IReadOnlyList<RoutePageNavItem> NavItems { get; } = navItems;

    public IReadOnlyList<string> Highlights { get; } = highlights;

    /// <summary>How many times the page's action button was pressed.</summary>
    public Signal<int> ActionCount { get; } = new(0);
}
