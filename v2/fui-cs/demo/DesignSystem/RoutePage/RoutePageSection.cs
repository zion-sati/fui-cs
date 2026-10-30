namespace FuiCs.Demo.DesignSystem;

/// <summary>A titled section card on a route page.</summary>
public sealed record RoutePageSection(FlexBox Card, Text Heading, Text Description)
{
    /// <summary>A section panel holding a heading, a description of up to three lines, then <paramref name="body"/>.</summary>
    public static RoutePageSection Create(string title, string description, Node body)
    {
        var heading = new DemoText(title, DemoTextRecipe.SectionTitle).FontSize(20.0f).SemanticRole(SemanticRole.Heading);
        var descriptionText = new DemoText(description, DemoTextRecipe.Supporting).FontSize(15.0f).MaxLines(3);
        var card = new DemoSurface(DemoSurfaceRecipe.SectionPanel)
            .Padding(18.0f)
            .Child(FlexBox.Column(heading, Spacer(8.0f), descriptionText, Spacer(14.0f), body).FillWidth());
        return new RoutePageSection(card, heading, descriptionText);
    }

    internal static FlexBox Spacer(float height) => new FlexBox().FillWidth().Height(height);
}
