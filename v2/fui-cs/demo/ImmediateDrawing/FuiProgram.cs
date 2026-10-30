using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>
/// The immediate drawing route: live plots drawn with DrawContext on every tick, an interactive noise strand, and a
/// bitmap paint canvas with a rich-text hint rendered into its pixels.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication()
    {
        Application.SetCaption("EffinDOM FUI-CS Demo • Immediate Drawing");
        return Application.CreateManaged(static () => new DrawingPage());
    }
}
