namespace FuiCs;

/// <summary>
/// The platform's keyboard conventions: which modifier drives shortcuts and text navigation, undo and redo
/// detection, and shortcut labels. Every member defaults to the current <see cref="Platform.Family"/>.
/// </summary>
public static class KeyboardShortcuts
{
    private sealed record Policy(
        KeyModifier Primary,
        KeyModifier WordNavigation,
        KeyModifier LineBoundary,
        KeyModifier DocumentBoundary,
        bool RedoUsesPrimaryY,
        bool RedoUsesShiftedPrimaryZ);

    private static readonly Policy Apple = new(KeyModifier.Meta, KeyModifier.Alt, KeyModifier.Meta, KeyModifier.Meta, false, true);
    private static readonly Policy Default = new(KeyModifier.Ctrl, KeyModifier.Ctrl, KeyModifier.None, KeyModifier.Ctrl, true, true);

    /// <summary>Command on Apple platforms, Control elsewhere.</summary>
    public static KeyModifier PrimaryModifier(PlatformFamily? family = null) => Resolve(family).Primary;

    /// <summary>
    /// Whether <paramref name="modifiers"/> hold the platform's primary shortcut modifier (Cmd on Apple, Ctrl
    /// elsewhere).
    /// </summary>
    public static bool HasPrimaryModifier(KeyModifier modifiers, PlatformFamily? family = null) =>
        (modifiers & PrimaryModifier(family)) != 0;

    /// <summary>The modifier that moves by word: Option on Apple platforms, Control elsewhere.</summary>
    public static KeyModifier WordNavigationModifier(PlatformFamily? family = null) => Resolve(family).WordNavigation;

    /// <summary>
    /// Whether <paramref name="modifiers"/> hold the platform's word-navigation modifier (Alt on Apple, Ctrl
    /// elsewhere).
    /// </summary>
    public static bool HasWordNavigationModifier(KeyModifier modifiers, PlatformFamily? family = null) =>
        (modifiers & WordNavigationModifier(family)) != 0;

    /// <summary>The modifier that moves to a line's start or end: Command on Apple platforms, none elsewhere.</summary>
    public static KeyModifier LineBoundaryModifier(PlatformFamily? family = null) => Resolve(family).LineBoundary;

    /// <summary>Whether <paramref name="modifiers"/> hold the platform's line-boundary modifier.</summary>
    public static bool HasLineBoundaryModifier(KeyModifier modifiers, PlatformFamily? family = null)
    {
        var modifier = LineBoundaryModifier(family);
        return modifier != KeyModifier.None && (modifiers & modifier) != 0;
    }

    /// <summary>The modifier that moves to the document's start or end.</summary>
    public static KeyModifier DocumentBoundaryModifier(PlatformFamily? family = null) => Resolve(family).DocumentBoundary;

    /// <summary>Whether <paramref name="modifiers"/> hold the platform's document-boundary modifier.</summary>
    public static bool HasDocumentBoundaryModifier(KeyModifier modifiers, PlatformFamily? family = null) =>
        (modifiers & DocumentBoundaryModifier(family)) != 0;

    /// <summary>Whether the key is the platform's Undo shortcut.</summary>
    public static bool IsUndo(string key, KeyModifier modifiers, PlatformFamily? family = null) =>
        (modifiers & KeyModifier.Shift) == 0 && HasPrimaryModifier(modifiers, family) && IsKey(key, "z");

    /// <summary>Whether the key is the platform's Redo shortcut.</summary>
    public static bool IsRedo(string key, KeyModifier modifiers, PlatformFamily? family = null)
    {
        var policy = Resolve(family);
        if ((modifiers & policy.Primary) == 0)
        {
            return false;
        }
        if (policy.RedoUsesPrimaryY && IsKey(key, "y"))
        {
            return true;
        }
        return policy.RedoUsesShiftedPrimaryZ && (modifiers & KeyModifier.Shift) != 0 && IsKey(key, "z");
    }

    /// <summary>A shortcut's label: modifier symbols run together on Apple platforms ("⇧⌘Z"), names joined by + elsewhere ("Ctrl+Shift+Z").</summary>
    public static string FormatLabel(string key, KeyModifier modifiers, PlatformFamily? family = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        var platform = family ?? Platform.Family;
        var apple = platform == PlatformFamily.Apple;
        var tokens = new List<string>();
        if (apple)
        {
            AddIf(tokens, modifiers, KeyModifier.Ctrl, "⌃");
            AddIf(tokens, modifiers, KeyModifier.Alt, "⌥");
            AddIf(tokens, modifiers, KeyModifier.Shift, "⇧");
            AddIf(tokens, modifiers, KeyModifier.Meta, "⌘");
        }
        else
        {
            AddIf(tokens, modifiers, KeyModifier.Ctrl, "Ctrl");
            AddIf(tokens, modifiers, KeyModifier.Alt, "Alt");
            AddIf(tokens, modifiers, KeyModifier.Shift, "Shift");
            AddIf(tokens, modifiers, KeyModifier.Meta, "Meta");
        }
        tokens.Add(KeyToken(key, apple));
        return string.Join(apple ? "" : "+", tokens);
    }

    /// <summary>The label of the primary modifier with <paramref name="key"/>, such as Ctrl+C or ⌘C.</summary>
    public static string FormatPrimaryLabel(string key, PlatformFamily? family = null) =>
        FormatLabel(key, PrimaryModifier(family), family);

    /// <summary>The label of the platform's Undo shortcut.</summary>
    public static string UndoLabel(PlatformFamily? family = null) => FormatPrimaryLabel("z", family);

    /// <summary>The label of the platform's Redo shortcut.</summary>
    public static string RedoLabel(PlatformFamily? family = null)
    {
        var policy = Resolve(family);
        return policy.RedoUsesPrimaryY
            ? FormatPrimaryLabel("y", family)
            : FormatLabel("z", policy.Primary | KeyModifier.Shift, family);
    }

    private static Policy Resolve(PlatformFamily? family) => (family ?? Platform.Family) == PlatformFamily.Apple ? Apple : Default;

    private static bool IsKey(string key, string expected) => string.Equals(key, expected, StringComparison.OrdinalIgnoreCase);

    private static void AddIf(List<string> tokens, KeyModifier modifiers, KeyModifier modifier, string token)
    {
        if ((modifiers & modifier) != 0)
        {
            tokens.Add(token);
        }
    }

    private static string KeyToken(string key, bool apple) => key switch
    {
        "ArrowLeft" => apple ? "←" : "Left",
        "ArrowRight" => apple ? "→" : "Right",
        "ArrowUp" => apple ? "↑" : "Up",
        "ArrowDown" => apple ? "↓" : "Down",
        "PageUp" => "PgUp",
        "PageDown" => "PgDn",
        _ when key.Length == 1 => key.ToUpperInvariant(),
        _ => key,
    };
}
