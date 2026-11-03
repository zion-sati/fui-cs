namespace FuiCs.Demo.DesignSystem;

/// <summary>
/// A route page: a selection area holding a scrolling page shell with the navigation bar, the accent badge, the title
/// and description, the route and action status, the action button, the notes card, then the route's sections.
/// </summary>
public sealed class RoutePageView : IDisposable
{
    private const float NarrowWidth = 640.0f;
    private const float ScrollBarGutter = 12.0f;
    private const float ScrollBarRailReserve = DemoTokens.ScrollBarTrackWidth + ScrollBarGutter;

    private readonly RoutePageModel model;
    private readonly ScrollBox contentScrollBox;
    private readonly SelectionArea root;
    private readonly FlexBox contentColumn;
    private readonly FlexBox accentBadge;
    private readonly Text routeStatusText;
    private readonly Text counterStatusText;
    private readonly List<(string RoutePath, DemoNavLink Link)> navLinks = [];
    private string activeRoute = "";

    public RoutePageView(RoutePageModel model, IReadOnlyList<RoutePageSection> sections)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(sections);
        this.model = model;
        contentScrollBox = new DemoScrollBox()
            .HorizontalScrollEnabled(true)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .ScrollBarGutter(ScrollBarGutter)
            .Width(ScrollBoxWidth())
            .FillHeight()
            .NodeId("route-page-scroll:" + model.Title.ToLowerInvariant());
        var surface = new DemoSurface(DemoSurfaceRecipe.PageShell).FillWidth();
        var surfaceFrame = new FlexBox().FillWidth().Padding(DemoTokens.PageShellFramePadding).Child(surface);
        root = new SelectionArea().FillSize().Padding(RootPadding()).Child(contentScrollBox);

        var titleText = new DemoText(model.Title, DemoTextRecipe.PageTitle).FontSize(34.0f).SemanticRole(SemanticRole.Heading);
        var descriptionText = new DemoText(model.Description, DemoTextRecipe.Supporting).FontSize(17.0f).MaxLines(3);
        routeStatusText = new DemoText("", DemoTextRecipe.StatusValue).FontSize(16.0f);
        counterStatusText = new DemoText("", DemoTextRecipe.StatusSupporting).FontSize(16.0f);
        accentBadge = new FlexBox()
            .Padding(14.0f, 8.0f, 14.0f, 8.0f)
            .CornerRadius(999.0f)
            .Child(new DemoText(model.Badge, DemoTextRecipe.InverseLabel).FontSize(14.0f));
        ActionButton = new DemoButton(model.ButtonLabel, DemoButtonTone.Primary).Width(180.0f);

        var highlightColumn = FlexBox.Column();
        for (var index = 0; index < model.Highlights.Count; ++index)
        {
            if (index > 0)
            {
                highlightColumn.Child(RoutePageSection.Spacer(10.0f));
            }
            highlightColumn.Child(new DemoText(model.Highlights[index], DemoTextRecipe.Supporting).FontSize(16.0f));
        }
        var highlightCard = new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(18.0f)
            .Child(FlexBox.Column(
                new DemoText("Hot-swap notes", DemoTextRecipe.SectionTitle).FontSize(18.0f),
                RoutePageSection.Spacer(12.0f),
                highlightColumn));

        // The links wrap rather than run past the page in a narrow window.
        var navBar = FlexBox.Row().FlexWrap(FlexWrap.Wrap).FillWidth();
        foreach (var item in model.NavItems)
        {
            var link = new DemoNavLink(item.RoutePath, item.Label).Margin(0.0f, 0.0f, 10.0f, 8.0f);
            navLinks.Add((item.RoutePath, link));
            navBar.Child(link);
        }

        contentColumn = FlexBox.Column(
                navBar,
                RoutePageSection.Spacer(16.0f),
                accentBadge,
                RoutePageSection.Spacer(16.0f),
                titleText,
                RoutePageSection.Spacer(10.0f),
                descriptionText,
                RoutePageSection.Spacer(18.0f),
                routeStatusText,
                RoutePageSection.Spacer(10.0f),
                counterStatusText,
                RoutePageSection.Spacer(18.0f),
                ActionButton,
                RoutePageSection.Spacer(22.0f),
                highlightCard)
            .FillWidth()
            .Padding(ContentPadding());
        foreach (var section in sections)
        {
            contentColumn.Child(RoutePageSection.Spacer(22.0f)).Child(section.Card);
        }
        surface.Child(contentColumn);
        contentScrollBox.Child(surfaceFrame);
    }

    public DemoButton ActionButton { get; }

    public Node Root => root;

    public void Dispose() => contentScrollBox.Dispose();

    public void ApplyTheme(Theme theme)
    {
        DemoScrollBars.ApplyTheme(contentScrollBox, theme);
        accentBadge.Background = theme.Colors.Accent;
        SyncNavLinks();
    }

    /// <summary>Narrow viewports get tighter padding; the scroll box leaves room for its bar's rail.</summary>
    public void SyncViewportLayout()
    {
        root.Padding = new Thickness(RootPadding());
        contentScrollBox.Width = new Length(ScrollBoxWidth());
        contentColumn.Padding = new Thickness(ContentPadding());
    }

    public void SetCurrentRoute(string route)
    {
        activeRoute = route;
        routeStatusText.Text = "Current route: " + (route.Length > 0 ? route : "(waiting for host route)");
        SyncNavLinks();
    }

    public void SetActionCount(int count) =>
        counterStatusText.Text = model.CounterLabel + ": " + count.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static float RootPadding() =>
        Application.ViewportWidth.Value < NarrowWidth ? DemoTokens.DemoRootPadding : DemoTokens.RoutePageRootPadding;

    private static float ContentPadding() => Application.ViewportWidth.Value < NarrowWidth ? 24.0f : 28.0f;

    private static float ScrollBoxWidth()
    {
        var available = Math.Max(Application.ViewportWidth.Value - (RootPadding() * 2.0f), 0.0f);
        var withoutRail = available - ScrollBarRailReserve;
        return withoutRail > 0.0f ? withoutRail : available;
    }

    private void SyncNavLinks()
    {
        foreach (var (routePath, link) in navLinks)
        {
            link.IsActive = activeRoute == routePath;
        }
    }
}
