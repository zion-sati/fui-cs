namespace FuiCs;

// ABI members whose host functions fill caller-sized buffers, which the generator cannot project. Their browser
// implementations live in the hand-written fui_host_buffers JSImport module.
public partial interface IFuiAbi
{
    /// <summary>Reads the persisted text state for a node and kind; false when the host has none.</summary>
    bool HostTryCopyPersistedState(string nodeId, string kind, out uint version, out string payload);

    /// <summary>The rectangles a text range covers, four floats (x, y, width, height) per line; empty when none.</summary>
    float[] UiReadTextRangeRects(ulong handle, uint startOffset, uint endOffset);

    /// <summary>
    /// The start and end endpoint rectangles of a selection area's cross-node selection, eight floats; empty when the
    /// area has none.
    /// </summary>
    float[] UiReadCrossSelectionEndpointRects(ulong areaHandle);

    /// <summary>The canvas token that draws onto an offscreen canvas.</summary>
    uint HostGetOffscreenCanvasToken(uint offscreenId);

    /// <summary>An offscreen canvas's RGBA pixels, width × height × 4 bytes.</summary>
    byte[] HostReadOffscreenPixels(uint offscreenId, uint width, uint height);

    /// <summary>A built node rendered into width × height RGBA pixels; empty when the host cannot render it.</summary>
    byte[] UiRenderNodeToRgba(ulong handle, uint width, uint height, float scale, float x, float y);
}
