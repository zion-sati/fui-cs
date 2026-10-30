using FuiCs;

namespace FuiCs.Browser;

/// <summary>The browser test application; the generated <c>__runApp</c> export starts it on the selected test page.</summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(TestPages.Build);
}
