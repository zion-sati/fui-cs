using FuiCs;

namespace FuiCs.Demo.ScrollbarGutter;

/// <summary>
/// The scrollbar gutter route: a repro page for scroll boxes whose automatic scroll bars once oscillated, because the
/// gutter a shown bar reserves was counted twice when deciding whether the bar is needed.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication()
    {
        Application.SetCaption("EffinDOM FUI-CS Demo • Scrollbar Gutter");
        return Application.CreateManaged(static () => new ScrollbarGutterBugController());
    }
}
