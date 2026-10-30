namespace FuiCs.Demo.DesignSystem;

/// <summary>
/// A pill-shaped navigation link with a text label: the accent while it is the current route or hovered or pressed,
/// the secondary surface otherwise.
/// </summary>
public class DemoNavLink : NavLink
{
    private bool isActive;
    private bool isHovered;

    public DemoNavLink(string href, string? label = null, bool openInNewTab = false)
        : base(href, openInNewTab)
    {
        label ??= href;
        Label = new Text(label).FontSize(15.0f).Selectable(false);
        CornerRadius = new CornerRadius(DemoTokens.NavLinkRadius);
        Padding = new Thickness(DemoTokens.NavLinkPaddingX, DemoTokens.NavLinkPaddingY);
        this.Child(Label);
        SemanticLabel = label;
        InteractionStateChanged += state =>
        {
            isHovered = state.IsHovered || state.IsPressed;
            ApplyDemoTheme(Theme.Active.Value);
        };
        Track(Theme.Bind(ApplyDemoTheme));
    }

    public Text Label { get; }

    /// <summary>Whether the link is for the current route.</summary>
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            ApplyDemoTheme(Theme.Active.Value);
        }
    }

    public string LabelText
    {
        get => Label.Text;
        set
        {
            Label.Text = value;
            SemanticLabel = value;
        }
    }

    private void ApplyDemoTheme(Theme theme) =>
        DemoRecipes.Apply(this, Label, theme, isActive || isHovered ? DemoNavLinkRecipe.Active : DemoNavLinkRecipe.Inactive);
}

/// <summary>Fluent configuration for <see cref="DemoNavLink"/>.</summary>
public static class DemoNavLinkExtensions
{
    extension<TLink>(TLink link) where TLink : DemoNavLink
    {
        public TLink IsActive(bool active = true)
        {
            link.IsActive = active;
            return link;
        }
    }
}
