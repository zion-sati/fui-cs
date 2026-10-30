namespace FuiCs.Demo.DesignSystem;

/// <summary>A combo box with the demo's popup panel, showing at most five rows before its list scrolls.</summary>
public class DemoComboBox : ComboBox
{
    public DemoComboBox(string text = "")
        : base(text)
    {
        MaxVisibleItems = 5;
        Track(Theme.Bind(theme => DemoRecipes.Apply(this, theme)));
    }
}
