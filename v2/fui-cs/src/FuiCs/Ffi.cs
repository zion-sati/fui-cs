namespace FuiCs;

public enum NodeType : uint
{
    FlexBox = 0,
    Text = 1,
}

public enum Unit : uint
{
    Pixel = 0,
    Auto = 1,
    Star = 2,
}

public enum FlexDirection : uint
{
    Column = 0,
    Row = 1,
}

public interface IFuiBackend
{
    void Reset();
    ulong CreateNode(uint nodeType);
    void DeleteNode(ulong handle);
    void AddChild(ulong parent, ulong child);
    void RemoveChild(ulong parent, ulong child);
    void SetRoot(ulong handle);
    void SetWidth(ulong handle, float value, uint unit);
    void SetHeight(ulong handle, float value, uint unit);
    void SetBgColor(ulong handle, uint color);
    void SetText(ulong handle, string text);
    void SetFont(ulong handle, uint fontId, float size);
    void SetTextColor(ulong handle, uint color);
    void SetPadding(ulong handle, float top, float right, float bottom, float left);
    void SetFlexDirection(ulong handle, uint direction);
    void CommitFrame();
    void ResizeWindow(float width, float height);
    void RequestRender();
    float GetViewportWidth();
    float GetViewportHeight();
}

public static class Runtime
{
    private static IFuiBackend? backend;

    public static IFuiBackend Backend =>
        backend ?? throw new InvalidOperationException("FuiCs.Runtime.Init must be called before mounting an application.");

    public static void Init(IFuiBackend nextBackend)
    {
        backend = nextBackend;
    }
}
