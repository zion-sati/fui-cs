using System;

using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls;

/// <summary>The route page's view of the demo shell, over this route's generated shell bindings.</summary>
internal sealed class DemoShellAdapter : IDemoShell
{
    public event Action<uint>? AccentColorChanged
    {
        add => DemoShell.AccentColorChanged += value;
        remove => DemoShell.AccentColorChanged -= value;
    }

    public event Action<bool>? DarkModeChanged
    {
        add => DemoShell.DarkModeChanged += value;
        remove => DemoShell.DarkModeChanged -= value;
    }

    public string AccentColorHex() => DemoShell.AccentColorHex();

    public bool IsDarkMode() => DemoShell.IsDarkMode();
}
