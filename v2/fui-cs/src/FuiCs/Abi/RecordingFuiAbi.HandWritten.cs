namespace FuiCs.Testing;

public partial class RecordingFuiAbi
{
    /// <inheritdoc/>
    public virtual bool HostTryCopyPersistedState(string nodeId, string kind, out uint version, out string payload)
    {
        Calls.Add(new AbiCall("fui_copy_persisted_state", [nodeId, kind]));
        version = default;
        payload = "";
        return false;
    }

    /// <inheritdoc/>
    public virtual float[] UiReadTextRangeRects(ulong handle, uint startOffset, uint endOffset)
    {
        Calls.Add(new AbiCall("ui_copy_text_range_rects", [handle, startOffset, endOffset]));
        return [];
    }

    /// <inheritdoc/>
    public virtual float[] UiReadCrossSelectionEndpointRects(ulong areaHandle)
    {
        Calls.Add(new AbiCall("ui_copy_cross_selection_endpoint_rects", [areaHandle]));
        return [];
    }

    /// <inheritdoc/>
    public virtual uint HostGetOffscreenCanvasToken(uint offscreenId)
    {
        Calls.Add(new AbiCall("fui_canvas_get_offscreen_ptr", [offscreenId]));
        return offscreenId;
    }

    /// <inheritdoc/>
    public virtual byte[] HostReadOffscreenPixels(uint offscreenId, uint width, uint height)
    {
        Calls.Add(new AbiCall("fui_canvas_read_offscreen_pixels", [offscreenId, width, height]));
        return new byte[width * height * 4];
    }

    /// <inheritdoc/>
    public virtual byte[] UiRenderNodeToRgba(ulong handle, uint width, uint height, float scale, float x, float y)
    {
        Calls.Add(new AbiCall("fui_render_node_to_rgba", [handle, width, height, scale, x, y]));
        return [];
    }
}
