namespace FuiCs;

/// <summary>Categorized logging through the host.</summary>
public static class Logger
{
    /// <summary>Whether the host records <see cref="Log"/> messages. Warnings and errors are always recorded.</summary>
    public static bool IsEnabled => Runtime.Abi.HostLogsEnabled();

    /// <summary>Logs <paramref name="message"/> under <paramref name="category"/> to the host's console.</summary>
    public static void Log(string category, string message)
    {
        if (!IsEnabled)
        {
            return;
        }
        Runtime.Abi.HostLog(category, message);
    }

    /// <summary>Logs a warning under <paramref name="category"/>.</summary>
    public static void Warn(string category, string message) => Runtime.Abi.HostLog("Warning/" + category, message);

    /// <summary>Logs an error under <paramref name="category"/>.</summary>
    public static void Error(string category, string message) => Runtime.Abi.HostLog("Error/" + category, message);
}
