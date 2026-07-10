using FuiCs;

namespace FuiCs.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void ApplicationMountBuildsAndCommitsARedBox()
    {
        var backend = new RecordingBackend();
        Runtime.Init(backend);

        Application.Mount(
            Fui.FlexBox()
                .Width(320.0f)
                .Height(220.0f)
                .BgColor(0xFF0000FFu));

        Assert.Contains(backend.Calls, call => call == "Reset");
        Assert.Contains(backend.Calls, call => call.StartsWith("ResizeWindow(", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.StartsWith("CreateNode(0)", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.Contains("SetBgColor", StringComparison.Ordinal) && call.Contains("4278190335", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.StartsWith("SetRoot(", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call == "CommitFrame");
    }

    [Fact]
    public void SmokeTreeBuildsTextAndChildren()
    {
        var backend = new RecordingBackend();
        Runtime.Init(backend);

        Application.Mount(Smoke.CreateSmokeTree());

        Assert.Contains(backend.Calls, call => call.StartsWith("CreateNode(0)", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.StartsWith("CreateNode(1)", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.Contains("SetText", StringComparison.Ordinal) && call.Contains("left", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.Contains("SetText", StringComparison.Ordinal) && call.Contains("right", StringComparison.Ordinal));
        Assert.Contains(backend.Calls, call => call.StartsWith("AddChild(", StringComparison.Ordinal));
    }

    private sealed class RecordingBackend : IFuiBackend
    {
        private ulong nextHandle = 1;

        public List<string> Calls { get; } = [];

        public void Reset()
        {
            Calls.Add("Reset");
            nextHandle = 1;
        }

        public ulong CreateNode(uint nodeType)
        {
            var handle = nextHandle++;
            Calls.Add($"CreateNode({nodeType})={handle}");
            return handle;
        }

        public void DeleteNode(ulong handle) => Calls.Add($"DeleteNode({handle})");
        public void AddChild(ulong parent, ulong child) => Calls.Add($"AddChild({parent},{child})");
        public void RemoveChild(ulong parent, ulong child) => Calls.Add($"RemoveChild({parent},{child})");
        public void SetRoot(ulong handle) => Calls.Add($"SetRoot({handle})");
        public void SetWidth(ulong handle, float value, uint unit) => Calls.Add($"SetWidth({handle},{value},{unit})");
        public void SetHeight(ulong handle, float value, uint unit) => Calls.Add($"SetHeight({handle},{value},{unit})");
        public void SetBgColor(ulong handle, uint color) => Calls.Add($"SetBgColor({handle},{color})");
        public void SetText(ulong handle, string text) => Calls.Add($"SetText({handle},{text})");
        public void SetFont(ulong handle, uint fontId, float size) => Calls.Add($"SetFont({handle},{fontId},{size})");
        public void SetTextColor(ulong handle, uint color) => Calls.Add($"SetTextColor({handle},{color})");
        public void SetPadding(ulong handle, float top, float right, float bottom, float left) => Calls.Add($"SetPadding({handle},{top},{right},{bottom},{left})");
        public void SetFlexDirection(ulong handle, uint direction) => Calls.Add($"SetFlexDirection({handle},{direction})");
        public void CommitFrame() => Calls.Add("CommitFrame");
        public void ResizeWindow(float width, float height) => Calls.Add($"ResizeWindow({width},{height})");
        public void RequestRender() => Calls.Add("RequestRender");
        public float GetViewportWidth() => 320.0f;
        public float GetViewportHeight() => 220.0f;
    }
}
