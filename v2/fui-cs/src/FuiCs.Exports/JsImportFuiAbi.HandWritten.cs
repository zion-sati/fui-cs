// Compiled into every FuiCs application by FuiCs.Exports.targets.
using System.Runtime.InteropServices.JavaScript;

namespace FuiCs.Hosting;

internal sealed partial class JsImportFuiAbi
{
    public bool HostTryCopyPersistedState(string nodeId, string kind, out uint version, out string payload)
    {
        var text = HostBufferImports.CopyPersistedStatePayload(nodeId, kind);
        if (text is null)
        {
            version = 0;
            payload = "";
            return false;
        }
        version = (uint)HostBufferImports.CopyPersistedStateVersion(nodeId, kind);
        payload = text;
        return true;
    }


    public float[] UiReadTextRangeRects(ulong handle, uint startOffset, uint endOffset) => ToSingles(HostBufferImports.CopyTextRangeRects(handle, startOffset, endOffset));

    public float[] UiReadCrossSelectionEndpointRects(ulong areaHandle) => ToSingles(HostBufferImports.CopyCrossSelectionEndpointRects(areaHandle));

    public uint HostGetOffscreenCanvasToken(uint offscreenId) => (uint)HostBufferImports.GetOffscreenCanvasToken(offscreenId);

    public byte[] HostReadOffscreenPixels(uint offscreenId, uint width, uint height) =>
        HostBufferImports.ReadOffscreenPixels(offscreenId, width, height);

    public byte[] UiRenderNodeToRgba(ulong handle, uint width, uint height, float scale, float x, float y) =>
        HostBufferImports.RenderNodeToRgba(handle, width, height, scale, x, y);

    private static float[] ToSingles(double[] values)
    {
        var singles = new float[values.Length];
        for (var index = 0; index < values.Length; ++index)
        {
            singles[index] = (float)values[index];
        }
        return singles;
    }
}

internal static class HostBufferImports
{
    [JSImport("fui_copy_text_range_rects", "fui_host_buffers")]
    internal static extern double[] CopyTextRangeRects(ulong handle, uint start, uint end);

    [JSImport("fui_copy_cross_selection_endpoint_rects", "fui_host_buffers")]
    internal static extern double[] CopyCrossSelectionEndpointRects(ulong areaHandle);

    [JSImport("fui_get_offscreen_canvas_token", "fui_host_buffers")]
    internal static extern double GetOffscreenCanvasToken(uint offscreenId);

    [JSImport("fui_read_offscreen_pixels", "fui_host_buffers")]
    internal static extern byte[] ReadOffscreenPixels(uint offscreenId, uint width, uint height);

    [JSImport("fui_render_node_to_rgba", "fui_host_buffers")]
    internal static extern byte[] RenderNodeToRgba(ulong handle, uint width, uint height, float scale, float x, float y);

    [JSImport("fui_copy_persisted_state_payload", "fui_host_buffers")]
    internal static extern string? CopyPersistedStatePayload(string nodeId, string kind);

    [JSImport("fui_copy_persisted_state_version", "fui_host_buffers")]
    internal static extern double CopyPersistedStateVersion(string nodeId, string kind);
}
