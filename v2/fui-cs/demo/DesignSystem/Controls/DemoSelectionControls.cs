namespace FuiCs.Demo.DesignSystem;

/// <summary>A checkbox that may span its container's width.</summary>
public class DemoCheckbox : Checkbox
{
    public DemoCheckbox(string label, bool fullWidth = false)
        : base(label)
    {
        if (fullWidth)
        {
            FillWidth = true;
        }
    }
}

/// <summary>A switch that may span its container's width.</summary>
public class DemoSwitch : Switch
{
    public DemoSwitch(string label, bool fullWidth = false)
        : base(label)
    {
        if (fullWidth)
        {
            FillWidth = true;
        }
    }
}

/// <summary>A radio button that may span its container's width.</summary>
public class DemoRadioButton : RadioButton
{
    public DemoRadioButton(string value, string? label = null, bool fullWidth = false)
        : base(value, label)
    {
        if (fullWidth)
        {
            FillWidth = true;
        }
    }
}

/// <summary>A radio group that may span its container's width.</summary>
public class DemoRadioGroup : RadioGroup
{
    public DemoRadioGroup(bool fullWidth = false)
    {
        if (fullWidth)
        {
            FillWidth = true;
        }
    }
}

public class DemoSlider(float value = 0.0f) : Slider(value);
