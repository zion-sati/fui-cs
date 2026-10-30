namespace FuiCs;

/// <summary>Colors a text input or text area uses instead of the theme's; unset colors follow the theme.</summary>
public sealed record TextInputColors
{
    /// <summary>The shell's fill.</summary>
    public Color? Background { get; init; }

    /// <summary>The text's color.</summary>
    public Color? TextPrimary { get; init; }

    /// <summary>The text while disabled.</summary>
    public Color? TextMuted { get; init; }

    /// <summary>The placeholder's color.</summary>
    public Color? Placeholder { get; init; }

    /// <summary>The caret's color.</summary>
    public Color? Caret { get; init; }

    /// <summary>The shell's border.</summary>
    public Color? Border { get; init; }

    /// <summary>An accent for templates to use; the default template does not.</summary>
    public Color? Accent { get; init; }
}
