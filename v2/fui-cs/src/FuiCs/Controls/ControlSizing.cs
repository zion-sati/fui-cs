namespace FuiCs;

/// <summary>Sizes for a checkbox, radio button or switch; unset sizes follow the default template and the theme.</summary>
public sealed record LabeledControlSizing
{
    /// <summary>The indicator's size: a checkbox's or radio's side, or a switch's track height.</summary>
    public float? IndicatorSize
    {
        get;
        init => field = Positive(value);
    }

    /// <summary>The label's font size; the theme's body size when null.</summary>
    public float? LabelFontSize
    {
        get;
        init => field = Positive(value);
    }

    internal static float? Positive(float? value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is { } size)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size, name);
        }
        return value;
    }
}

/// <summary>Sizes for a slider; unset sizes follow the default template.</summary>
public sealed record SliderSizing
{
    /// <summary>The thumb's size; the default template's when null.</summary>
    public float? ThumbSize
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The track's thickness, at most the thumb size.</summary>
    public float? TrackThickness
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }
}

/// <summary>Sizes for a dropdown's field, option rows and chevron; unset sizes follow the default templates.</summary>
public sealed record DropdownSizing
{
    /// <summary>The field's font size; the default when null.</summary>
    public float? FieldFontSize
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The option rows' font size; the default when null.</summary>
    public float? OptionFontSize
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The field's height; grows to fit the font and chevron when null.</summary>
    public float? FieldHeight
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The option rows' height; the default when null.</summary>
    public float? OptionHeight
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The square the field keeps for the chevron.</summary>
    public float? ChevronBoxSize
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }

    /// <summary>The chevron icon's size inside its box.</summary>
    public float? ChevronIconSize
    {
        get;
        init => field = LabeledControlSizing.Positive(value);
    }
}
