namespace FuiCs;

/// <summary>Colors a checkbox, radio button or switch uses instead of the theme's; unset colors follow the theme.</summary>
public sealed record LabeledControlColors
{
    /// <summary>The indicator's unchecked fill, and a switch's thumb.</summary>
    public Color? Background { get; init; }

    /// <summary>The indicator's border.</summary>
    public Color? Border { get; init; }

    /// <summary>The checked fill, used for every state instead of the theme's accent, hover and pressed colors.</summary>
    public Color? Accent { get; init; }

    /// <summary>The label's color.</summary>
    public Color? TextPrimary { get; init; }

    /// <summary>The label color while disabled.</summary>
    public Color? TextMuted { get; init; }
}
