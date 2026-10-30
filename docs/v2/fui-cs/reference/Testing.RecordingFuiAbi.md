# RecordingFuiAbi

class in `FuiCs.Testing`: [`IFuiAbi`](IFuiAbi.md)

Records every ABI call instead of reaching EffinDOM, for tests on desktop .NET and NetWasm. Creation calls return increasing handles; override a member to return other values.

## Methods

- `HostGetOffscreenCanvasToken(uint)`: The canvas token that draws onto an offscreen canvas.
- `HostReadOffscreenPixels(uint, uint, uint)`: An offscreen canvas's RGBA pixels, width × height × 4 bytes.
- `HostTryCopyPersistedState(string, string, out uint, out string)`: Reads the persisted text state for a node and kind; false when the host has none.
- `UiReadCrossSelectionEndpointRects(ulong)`: The start and end endpoint rectangles of a selection area's cross-node selection, eight floats; empty when the area has none.
- `UiReadTextRangeRects(ulong, uint, uint)`: The rectangles a text range covers, four floats (x, y, width, height) per line; empty when none.
- `UiRenderNodeToRgba(ulong, uint, uint, float, float, float)`: A built node rendered into width × height RGBA pixels; empty when the host cannot render it.
