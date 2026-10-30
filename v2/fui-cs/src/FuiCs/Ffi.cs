namespace FuiCs;

/// <summary>Holds the ABI implementation fui-cs talks to: JSImport in the browser, a recording double in tests.</summary>
public static class Runtime
{
    private static IFuiAbi? abi;

    /// <summary>The ABI the SDK calls; throws until the host installs one.</summary>
    public static IFuiAbi Abi =>
        abi ?? throw new InvalidOperationException("FuiCs.Runtime.Init must be called before mounting an application.");

    /// <summary>Whether the host has installed the ABI.</summary>
    public static bool IsInitialized => abi is not null;

    /// <summary>Installs the ABI, as the host does at startup or a test does with a recording one.</summary>
    public static void Init(IFuiAbi nextAbi)
    {
        abi = nextAbi;
        FontRegistry.FlushPendingFallbacks();
    }
}
