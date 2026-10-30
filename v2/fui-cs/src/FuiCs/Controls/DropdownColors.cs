namespace FuiCs;

/// <summary>Colors a dropdown uses instead of the theme's; unset colors follow the theme.</summary>
public sealed record DropdownColors
{
    /// <summary>The field's fill.</summary>
    public Color? Background { get; init; }

    /// <summary>The selected value's text and the option labels.</summary>
    public Color? TextPrimary { get; init; }

    /// <summary>A combo box's placeholder text.</summary>
    public Color? Placeholder { get; init; }

    /// <summary>The field's border.</summary>
    public Color? Border { get; init; }

    /// <summary>The selected option's label in the list, and a combo box's caret.</summary>
    public Color? Accent { get; init; }
}
