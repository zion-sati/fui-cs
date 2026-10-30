namespace FuiCs.Bindings;

/// <summary>Wrappers over the host's immediate-mode canvas and path ABI. Canvases are opaque host tokens.</summary>
internal static class Canvas
{
    public static void DrawBatch(uint canvas, uint[] words) => Runtime.Abi.HostCanvasDrawBatch(canvas, words);
    public static uint PathCreate() => Runtime.Abi.HostPathCreate();
    public static void PathDestroy(uint pathId) => Runtime.Abi.HostPathDestroy(pathId);
    public static void PathMoveTo(uint pathId, float x, float y) => Runtime.Abi.HostPathMoveTo(pathId, x, y);
    public static void PathLineTo(uint pathId, float x, float y) => Runtime.Abi.HostPathLineTo(pathId, x, y);
    public static void PathQuadTo(uint pathId, float cx, float cy, float x, float y) => Runtime.Abi.HostPathQuadTo(pathId, cx, cy, x, y);
    public static void PathCubicTo(uint pathId, float cx1, float cy1, float cx2, float cy2, float x, float y) =>
        Runtime.Abi.HostPathCubicTo(pathId, cx1, cy1, cx2, cy2, x, y);
    public static void PathClose(uint pathId) => Runtime.Abi.HostPathClose(pathId);
    public static void PathAddRect(uint pathId, float x, float y, float width, float height) => Runtime.Abi.HostPathAddRect(pathId, x, y, width, height);
    public static void PathAddCircle(uint pathId, float cx, float cy, float radius) => Runtime.Abi.HostPathAddCircle(pathId, cx, cy, radius);
}
