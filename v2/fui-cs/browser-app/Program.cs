using System.Runtime.InteropServices.JavaScript;
using FuiCs;

namespace FuiCs.Browser;

public static class Program
{
    public static void Main()
    {
    }
}

public static partial class BrowserExports
{
    [JSExport]
    internal static void RunSmokeApp()
    {
        Runtime.Init(new WasmBackend());
        Application.Mount(Smoke.CreateSmokeTree());
    }
}

internal sealed partial class WasmBackend : IFuiBackend
{
    public void Reset() => UiReset();
    public ulong CreateNode(uint nodeType) => unchecked((ulong)UiCreateNode(unchecked((int)nodeType)));
    public void DeleteNode(ulong handle) => UiDeleteNode(unchecked((long)handle));
    public void AddChild(ulong parent, ulong child) => UiNodeAddChild(unchecked((long)parent), unchecked((long)child));
    public void RemoveChild(ulong parent, ulong child) => UiNodeRemoveChild(unchecked((long)parent), unchecked((long)child));
    public void SetRoot(ulong handle) => UiSetRoot(unchecked((long)handle));
    public void SetWidth(ulong handle, float value, uint unit) => UiSetWidth(unchecked((long)handle), value, unchecked((int)unit));
    public void SetHeight(ulong handle, float value, uint unit) => UiSetHeight(unchecked((long)handle), value, unchecked((int)unit));
    public void SetBgColor(ulong handle, uint color) => UiSetBgColor(unchecked((long)handle), color);
    public void SetText(ulong handle, string text) => UiSetText(unchecked((long)handle), text);
    public void SetFont(ulong handle, uint fontId, float size) => UiSetFont(unchecked((long)handle), unchecked((int)fontId), size);
    public void SetTextColor(ulong handle, uint color) => UiSetTextColor(unchecked((long)handle), color);
    public void SetPadding(ulong handle, float top, float right, float bottom, float left) => UiSetPadding(unchecked((long)handle), top, right, bottom, left);
    public void SetFlexDirection(ulong handle, uint direction) => UiSetFlexDirection(unchecked((long)handle), unchecked((int)direction));
    public void CommitFrame() => UiCommitFrame();
    public void ResizeWindow(float width, float height) => UiResizeWindow(width, height);
    public void RequestRender() => HostRequestRender();
    public float GetViewportWidth() => HostGetViewportWidth();
    public float GetViewportHeight() => HostGetViewportHeight();

    [JSImport("effindom_v2_ui.ui_reset", "main.js")]
    private static partial void UiReset();

    [JSImport("effindom_v2_ui.ui_create_node", "main.js")]
    [return: JSMarshalAs<JSType.BigInt>]
    private static partial long UiCreateNode(int nodeType);

    [JSImport("effindom_v2_ui.ui_delete_node", "main.js")]
    private static partial void UiDeleteNode([JSMarshalAs<JSType.BigInt>] long handle);

    [JSImport("effindom_v2_ui.ui_node_add_child", "main.js")]
    private static partial void UiNodeAddChild(
        [JSMarshalAs<JSType.BigInt>] long parent,
        [JSMarshalAs<JSType.BigInt>] long child);

    [JSImport("effindom_v2_ui.ui_node_remove_child", "main.js")]
    private static partial void UiNodeRemoveChild(
        [JSMarshalAs<JSType.BigInt>] long parent,
        [JSMarshalAs<JSType.BigInt>] long child);

    [JSImport("effindom_v2_ui.ui_set_root", "main.js")]
    private static partial void UiSetRoot([JSMarshalAs<JSType.BigInt>] long handle);

    [JSImport("effindom_v2_ui.ui_set_width", "main.js")]
    private static partial void UiSetWidth([JSMarshalAs<JSType.BigInt>] long handle, float value, int unit);

    [JSImport("effindom_v2_ui.ui_set_height", "main.js")]
    private static partial void UiSetHeight([JSMarshalAs<JSType.BigInt>] long handle, float value, int unit);

    [JSImport("effindom_v2_ui.ui_set_bg_color", "main.js")]
    private static partial void UiSetBgColor([JSMarshalAs<JSType.BigInt>] long handle, double color);

    [JSImport("effindom_v2_ui.ui_set_text", "main.js")]
    private static partial void UiSetText([JSMarshalAs<JSType.BigInt>] long handle, string text);

    [JSImport("effindom_v2_ui.ui_set_font", "main.js")]
    private static partial void UiSetFont([JSMarshalAs<JSType.BigInt>] long handle, int fontId, float size);

    [JSImport("effindom_v2_ui.ui_set_text_color", "main.js")]
    private static partial void UiSetTextColor([JSMarshalAs<JSType.BigInt>] long handle, double color);

    [JSImport("effindom_v2_ui.ui_set_padding", "main.js")]
    private static partial void UiSetPadding([JSMarshalAs<JSType.BigInt>] long handle, float top, float right, float bottom, float left);

    [JSImport("effindom_v2_ui.ui_set_flex_direction", "main.js")]
    private static partial void UiSetFlexDirection([JSMarshalAs<JSType.BigInt>] long handle, int direction);

    [JSImport("effindom_v2_ui.ui_commit_frame", "main.js")]
    private static partial void UiCommitFrame();

    [JSImport("effindom_v2_ui.ui_resize_window", "main.js")]
    private static partial void UiResizeWindow(float width, float height);

    [JSImport("fui_host.request_render", "main.js")]
    private static partial void HostRequestRender();

    [JSImport("fui_host.get_viewport_width", "main.js")]
    private static partial float HostGetViewportWidth();

    [JSImport("fui_host.get_viewport_height", "main.js")]
    private static partial float HostGetViewportHeight();
}
