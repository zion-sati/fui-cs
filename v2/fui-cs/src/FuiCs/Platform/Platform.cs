namespace FuiCs;

/// <summary>The host the application runs in: its platform family, environment and capabilities.</summary>
public sealed record HostContext(PlatformFamily Family, HostEnvironment Environment, HostCapability Capabilities)
{
    /// <summary>Whether the host supports <paramref name="capability"/>.</summary>
    public bool Supports(HostCapability capability) => capability != HostCapability.None && (Capabilities & capability) == capability;
}

/// <summary>Queries about the host, read once per runtime.</summary>
public static class Platform
{
    private const HostCapability KnownCapabilities =
        HostCapability.BrowserHistory | HostCapability.Reload | HostCapability.NewBrowsingContext | HostCapability.OpenExternalUri
        | HostCapability.ClipboardRead | HostCapability.ClipboardWrite | HostCapability.FileDialogs;

    private static HostContext? current;

    /// <summary>The host's context, read once.</summary>
    public static HostContext Current => current ??= Query();

    /// <summary>The platform family: Apple, Windows, Linux or unknown.</summary>
    public static PlatformFamily Family => Current.Family;

    /// <summary>Where the application runs: a browser, a desktop host or headless.</summary>
    public static HostEnvironment Environment => Current.Environment;

    /// <summary>Whether the host supports <paramref name="capability"/>.</summary>
    public static bool Supports(HostCapability capability) => Current.Supports(capability);

    /// <summary>Whether the primary pointer is coarse (touch). The host can change this, so it is read each time.</summary>
    public static bool IsCoarsePointer => Runtime.Abi.HostIsCoarsePointer();

    internal static void Reset() => current = null;

    private static HostContext Query()
    {
        var family = Runtime.Abi.HostGetPlatformFamily();
        var environment = Runtime.Abi.HostGetHostEnvironment();
        return new HostContext(
            family > PlatformFamily.Linux ? PlatformFamily.Unknown : family,
            environment > HostEnvironment.Headless ? HostEnvironment.Unknown : environment,
            Runtime.Abi.HostGetHostCapabilities() & KnownCapabilities);
    }
}
