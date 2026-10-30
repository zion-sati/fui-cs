namespace FuiCs.Demo.DesignSystem;

/// <summary>An 18 px progress bar in the demo's colors, recolored when the theme changes.</summary>
public class DemoProgressBar : ProgressBar
{
    public DemoProgressBar(float value = 0.0f)
        : base(value)
    {
        Thickness = 18.0f;
        Track(Theme.Bind(theme => DemoRecipes.Apply(this, theme)));
    }
}
