namespace FuiCs.Demo.DesignSystem;

/// <summary>A 15 px text area on the demo's field surface, restyled when the theme changes.</summary>
public class DemoTextArea : TextArea
{
    public DemoTextArea(string text = "")
        : base(text)
    {
        FontSize = 15.0f;
        Track(Theme.Bind(theme => DemoRecipes.Apply(this, theme)));
    }
}
