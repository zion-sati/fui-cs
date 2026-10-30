namespace FuiCs;

/// <summary>The host's current route and navigation.</summary>
public static class Navigation
{
    /// <summary>The route the host reports, updated as the user navigates.</summary>
    public static Signal<string> CurrentRoute { get; } = new("");

    /// <summary>Navigates the host to <paramref name="target"/>, a route or URL, optionally in a new tab or window.</summary>
    public static void NavigateTo(string target, bool openInNewTab = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        Runtime.Abi.HostNavigateTo(target, openInNewTab);
    }

    internal static void Reset() => CurrentRoute.Reset("");
}
