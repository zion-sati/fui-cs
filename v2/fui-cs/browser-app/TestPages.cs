using System.Runtime.InteropServices.JavaScript;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>The browser test pages. The harness selects one from the URL's <c>?page=</c> before it starts the app.</summary>
public static partial class TestPages
{
    private static string selected = "smoke";

    [JSExport("__selectTestPage")]
    public static void Select(string name) => selected = name;

    public static Node Build() => selected switch
    {
#if !SMOKE_ONLY
        "layout" => LayoutPage.Build(),
        "grid" => GridPage.Build(),
        "input" => InputPage.Build(),
        "popup" => PopupPage.Build(),
        "text" => TextPage.Build(),
        "scroll" => ScrollPage.Build(),
        "image" => ImagePage.Build(),
        "drawing" => DrawingPage.Build(),
        "platform" => PlatformPage.Build(),
        "fetch" => FetchPage.Build(),
        "persistence" => PersistencePage.Build(),
        "showcase" => ShowcasePage.Build(),
        "controls" => ControlsPage.Build(),
        "range" => RangePage.Build(),
        "dialog" => DialogPage.Build(),
        "context-menu" => ContextMenuPage.Build(),
        "gallery" => GalleryPage.Build(),
        "scrollbox" => ScrollBoxPage.Build(),
        "selection" => SelectionPage.Build(),
        "host" => HostPage.Build(),
        "tooltip" => ToolTipPage.Build(),
        "animation" => AnimationPage.Build(),
        "worker" => WorkerPage.Build(),
        "design-system" => DesignSystemPage.Build(),
        "virtual-list" => VirtualListPage.Build(),
#endif
        _ => SmokePage.Build(),
    };
}
