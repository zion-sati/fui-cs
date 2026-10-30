namespace FuiCs.Demo.DesignSystem;

/// <summary>A dropdown with the demo's popup panel, showing at most six rows before its list scrolls.</summary>
public class DemoDropdown : Dropdown
{
    public DemoDropdown()
    {
        MaxVisibleItems = 6;
        Track(Theme.Bind(theme => DemoRecipes.Apply(this, theme)));
    }
}
