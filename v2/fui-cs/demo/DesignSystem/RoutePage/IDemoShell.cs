namespace FuiCs.Demo.DesignSystem;

/// <summary>
/// The demo shell's host services and events a route page follows. Each route generates its own <c>DemoShell</c>
/// bindings, which this library cannot reference, and passes an adapter over them.
/// </summary>
public interface IDemoShell
{
    /// <summary>The shell's accent as <c>#rrggbb</c>.</summary>
    string AccentColorHex();

    bool IsDarkMode();

    event Action<uint>? AccentColorChanged;

    event Action<bool>? DarkModeChanged;
}
