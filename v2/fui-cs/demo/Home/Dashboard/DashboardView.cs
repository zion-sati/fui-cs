using System;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.Home.Dashboard;

/// <summary>
/// The dashboard's layout: a sidebar with a pooled list beside a main column that scrolls, holding the header, the
/// preview panel with its sample cards, and the inspector with the input event inspector. The whole row scrolls
/// sideways when the window is narrower than it. The dialog's credential fields and the common controls' dropdown,
/// combo box and text inputs join in Stage 5 with those controls.
/// </summary>
internal sealed partial class DashboardView : IDisposable
{
    private const float MainContentScrollBarGutter = 12.0f;
    private const float MainContentScrollBarRailReserve = DemoTokens.ScrollBarTrackWidth + MainContentScrollBarGutter + 4.0f;

    public DashboardView()
    {
        SidebarList = new VirtualList(DemoTokens.SidebarListTotalItems, DemoTokens.SidebarListItemHeight)
            .OnBindItem(BindListItem)
            .NodeId("demo-dashboard:sidebar-list")
            .Width(DemoTokens.FullSize, Unit.Percent)
            .FillHeight();
        DemoScrollBars.Configure(SidebarList.ScrollBox.VerticalScrollBar);
        // The panel stretches to the row's height inside its margins, and the list takes what its column leaves.
        sidebarShell = new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Width(SidebarWidth())
            .AlignSelf(AlignSelf.Stretch)
            .Padding(16.0f, 16.0f, 16.0f, 16.0f)
            .Child(FlexBox.Column(
                    new DashboardText("Scrollable list", 22.0f, DemoTextRecipe.SectionTitle),
                    VerticalSpacer(8.0f),
                    new DashboardText("This pooled retained list rebinds rows in place so scrolling stays stable while item content changes.", 14.0f, DemoTextRecipe.Hint),
                    VerticalSpacer(DemoTokens.SpacingMedium),
                    SidebarList,
                    VerticalSpacer(DemoTokens.SidebarListBottomPadding))
                .Width(DemoTokens.FullSize, Unit.Percent)
                .Height(DemoTokens.FullSize, Unit.Percent));

        MainContentScrollState = new ScrollState();
        MainContentScrollState.ViewportHeight.Value = MainContentViewportHeight();
        MainContentScrollState.ContentHeight.Value = DemoTokens.MainContentStartupHeight;
        MainContentScrollBox = new DemoScrollBox(MainContentScrollState)
            .HorizontalScrollEnabled(true)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .NodeId("demo-dashboard-main-scroll")
            .ScrollBarGutter(MainContentScrollBarGutter);
        MainContentColumn = FlexBox.Column(
                mainHeaderPanel.Child(BuildMainHeaderContent()),
                VerticalSpacer(DemoTokens.SpacingLarge),
                BuildPreviewPanel(),
                VerticalSpacer(DemoTokens.SpacingLarge),
                BuildInspectorPanel())
            .Width(DemoTokens.FullSize, Unit.Percent)
            .Padding(0.0f, 0.0f, 0.0f, DemoTokens.SpacingMedium);
        MainContentScrollBox.Width(MainContentScrollBoxWidth()).Height(DemoTokens.FullSize, Unit.Percent).Child(MainContentColumn);
        mainPanel.Child(MainContentScrollBox).Width(MainColumnWidth());

        var columnGap = ColumnGap();
        dashboardGapSpacer.Width(columnGap);
        // The row sizes to its children, the panels' margins included, so the shell's scroll extent covers them.
        dashboardContentRow
            .Child(sidebarShell)
            .Child(dashboardGapSpacer)
            .Child(mainPanel);
        DashboardScrollBox = new DemoScrollBox()
            .HorizontalScrollEnabled(true)
            .VerticalScrollEnabled(false)
            .NodeId("demo-dashboard-shell-scroll")
            .Width(DemoTokens.FullSize, Unit.Percent)
            .Height(DemoTokens.FullSize, Unit.Percent)
            .Child(dashboardContentRow);
        DialogUsernameInput.SemanticLabel = "Username";
        DialogPasswordInput.SemanticLabel = "Password";
        Dialog.BodyContent = new Form().Child(FlexBox.Column(
                new DashboardText("Username", 14.0f, DemoTextRecipe.Hint),
                new FlexBox().Height(6.0f),
                DialogUsernameInput,
                new FlexBox().Height(12.0f),
                new DashboardText("Password", 14.0f, DemoTextRecipe.Hint),
                new FlexBox().Height(6.0f),
                DialogPasswordInput)
            .Width(DemoTokens.FullSize, Unit.Percent));
        Dialog.Shown += () => DialogUsernameInput.Focus();
        Dialog.BackgroundBlur = 10.0f;
        Root = new SelectionArea()
            .Width(Application.ViewportWidth.Value)
            .Height(Application.ViewportHeight.Value)
            .Padding(DemoTokens.DemoRootPadding, DemoTokens.DemoRootPadding, DemoTokens.DemoRootPadding, DemoTokens.DemoRootPadding)
            .Child(DashboardScrollBox)
            .Child(Dialog)
            .Child(ContextMenu);
    }

    public SelectionArea Root { get; }

    public VirtualList SidebarList { get; }

    public ScrollState MainContentScrollState { get; }

    public DemoScrollBox MainContentScrollBox { get; }

    public FlexBox MainContentColumn { get; }

    public DemoScrollBox DashboardScrollBox { get; }

    public ContextMenu ContextMenu { get; } = new ContextMenu()
        .MenuWidth(236.0f)
        .ItemHeight(40.0f);

    public Dialog Dialog { get; } = new Dialog("Confirm action", "Press Enter to accept, Escape to cancel, or click the backdrop to dismiss this dialog.")
        .BackdropColor(0x00000024)
        .CardCornerRadius(DemoTokens.SurfaceRadiusMedium)
        .CardShadow(new DropShadow(Color.FromRgba(0, 0, 0, 0x90), 0.0f, 0.0f, 15.0f, 15.0f));

    /// <summary>The sign-in dialog's fields, which the host projects for browser autofill and password managers.</summary>
    public DemoTextInput DialogUsernameInput { get; } = new DemoTextInput()
        .Placeholder("Username or email")
        .HostAutofill("username")
        .Width(DemoTokens.FullSize, Unit.Percent)
        .NodeId("username");

    public DemoTextInput DialogPasswordInput { get; } = new DemoTextInput()
        .Placeholder("Password")
        .Password()
        .HostAutofill("current-password")
        .Width(DemoTokens.FullSize, Unit.Percent)
        .NodeId("dialog-current-password");

    public Text HeaderStatusText { get; } = new DashboardText("", 15.0f, DemoTextRecipe.Hint);

    public Text SelectionStatusText { get; } = new DashboardText("", 14.0f, DemoTextRecipe.StatusSupporting).MaxLines(1).Overflow(TextOverflow.Ellipsis);

    public Text ClockText { get; } = new DashboardText("", 16.0f, DemoTextRecipe.StatusValue);

    public Text ListOffsetText { get; } = new DashboardText("", 16.0f, DemoTextRecipe.StatusValue);

    public Text FirstVisibleText { get; } = new DashboardText("", 16.0f, DemoTextRecipe.StatusValue);

    public Text RenderedCountText { get; } = new DashboardText("", 16.0f, DemoTextRecipe.StatusValue);

    private readonly FlexBox mainPanel = new FlexBox().FlexDirection(FlexDirection.Column).Height(DemoTokens.FullSize, Unit.Percent);

    private readonly FlexBox mainHeaderPanel = new DemoSurface(DemoSurfaceRecipe.SectionPanel).Padding(18.0f, 16.0f, 18.0f, 16.0f);

    private readonly FlexBox sidebarShell;

    private readonly FlexBox dashboardContentRow = new FlexBox().FlexDirection(FlexDirection.Row).Height(DemoTokens.FullSize, Unit.Percent);

    private readonly FlexBox dashboardGapSpacer = HorizontalSpacer(0.0f);

    public void ApplyTheme(Theme theme)
    {
        var panelColor = theme.Colors.Background;
        Root.Background(panelColor);
        DemoSurfaces.Apply(accentSwatch, theme, DemoSurfaceRecipe.AccentSwatch);
        DemoSurfaces.Apply(inspectorDivider, theme, DemoSurfaceRecipe.Divider);
        DemoSurfaces.Apply(KeyTargetBox, theme, DemoSurfaceRecipe.KeyTargetIdle);
        MediaSvgImage.Tint = theme.Colors.Accent;
        DemoSurfaces.Apply(demoScopeBadge, theme, DemoSurfaceRecipe.AccentBadge);
        DemoScrollBars.ApplyTheme(SidebarList.ScrollBox.VerticalScrollBar, theme, panelColor);
        DemoScrollBars.ApplyTheme(MainContentScrollBox, theme, panelColor);
        DemoScrollBars.ApplyTheme(nestedScrollBox, theme, panelColor);
        DemoScrollBars.ApplyTheme(DashboardScrollBox, theme, panelColor);
        DemoRecipes.Apply(ContextMenu, theme);
        ContextMenu.ItemPadding = new Thickness(14.0f, 10.0f, 14.0f, 10.0f);
        ContextMenu.PanelCornerRadius = DemoTokens.SurfaceRadiusSmall;
        ContextMenu.ItemCornerRadius = 14.0f;
        DemoRecipes.Apply(Dialog, theme);
        // Rebinding every row recolors the pooled rows for the new theme.
        SidebarList.UpdateItemCount(SidebarList.TotalItems);
    }

    public void Dispose()
    {
        mediaBitmap.Dispose();
        SidebarList.Dispose();
        MainContentScrollBox.Dispose();
        DashboardScrollBox.Dispose();
        Dialog.Dispose();
    }

    /// <summary>Resizes the root, the columns and the list to the current viewport.</summary>
    public void SyncViewportLayout()
    {
        var columnGap = ColumnGap();
        Root.Width(Application.ViewportWidth.Value).Height(Application.ViewportHeight.Value);
        sidebarShell.Width(SidebarWidth());
        mainPanel.Width(MainColumnWidth());
        MainContentScrollBox.Width(MainContentScrollBoxWidth());
        dashboardGapSpacer.Width(columnGap);
        MainContentScrollState.ViewportHeight.Value = MainContentViewportHeight();
    }

    private static bool IsNarrow => Application.ViewportWidth.Value < 480.0f;

    private static float ColumnGap() => IsNarrow ? 12.0f : DemoTokens.SpacingLarge;

    private static float SidebarWidth() => IsNarrow ? 272.0f : DemoTokens.SidebarWidth;

    private static float MainColumnWidth()
    {
        // The sidebar panel's margins take room beside it too.
        var available = Application.ViewportWidth.Value - (DemoTokens.DemoRootPadding * 2.0f) - SidebarWidth()
            - (DemoTokens.SurfacePanelMargin * 2.0f) - ColumnGap();
        return available > 320.0f ? available : 320.0f;
    }

    private static float MainContentScrollBoxWidth()
    {
        var available = MainColumnWidth() - MainContentScrollBarRailReserve;
        return available > 0.0f ? available : MainColumnWidth();
    }

    private static float MainContentViewportHeight() => Math.Max(Application.ViewportHeight.Value - (DemoTokens.DemoRootPadding * 2.0f), 0.0f);

    private static FlexBox VerticalSpacer(float height) => new FlexBox().Width(DemoTokens.FullSize, Unit.Percent).Height(height);

    private static FlexBox HorizontalSpacer(float width) => new FlexBox().Width(width).Height(DemoTokens.FullSize, Unit.Percent);

    /// <summary>Fills a pooled row: a title and a detail line, on alternating row surfaces.</summary>
    private static void BindListItem(FlexBox container, int index)
    {
        if (container.Children.Count == 0)
        {
            container
                .FlexDirection(FlexDirection.Column)
                .Width(DemoTokens.FullSize, Unit.Percent)
                .Padding(12.0f, 12.0f, 12.0f, 12.0f)
                .Child(new DemoText("", DemoTextRecipe.ListTitle).FontSize(17.0f))
                .Child(new FlexBox().Width(DemoTokens.FullSize, Unit.Percent).Height(6.0f))
                .Child(new DemoText("", DemoTextRecipe.ListMeta).FontSize(13.0f));
        }
        DemoSurfaces.Apply(container, Theme.Active.Value, (index & 1) == 0 ? DemoSurfaceRecipe.ListRow : DemoSurfaceRecipe.ListRowAlt);
        ((Text)container.Children[0]).Text = "Item " + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ((Text)container.Children[2]).Text = "Fixed height row.";
    }
}
