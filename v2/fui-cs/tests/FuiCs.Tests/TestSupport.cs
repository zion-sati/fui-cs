using System;
using System.Collections.Generic;

using FuiCs.Bindings;
using FuiCs.Hosting;
using FuiCs.Testing;

namespace FuiCs.Tests;

internal static class TestRuntime
{
    /// <summary>Points the runtime at a fresh recording ABI and clears framework state left by earlier tests.</summary>
    public static RecordingFuiAbi Start() => Start(new RecordingFuiAbi());

    public static TAbi Start<TAbi>(TAbi abi) where TAbi : RecordingFuiAbi
    {
        Runtime.Init(abi);
        Application.Reset();
        Theme.Reset();
        FontRegistry.Reset();
        AssetRegistry.Reset();
        Platform.Reset();
        Navigation.Reset();
        FetchRegistry.Reset();
        FrameScheduler.Reset();
        EventRouter.Reset();
        HostEvents.Reset();
        abi.Calls.Clear();
        return abi;
    }
}

/// <summary>A recording ABI that reports layout bounds and text metrics per handle and a configurable pointer kind.</summary>
internal sealed class LayoutAbi : RecordingFuiAbi
{
    public Dictionary<ulong, Rect> Bounds { get; } = [];

    public bool CoarsePointer { get; set; }

    /// <summary>Line rectangles (x, y, width, height) the host reports for any text range of a node.</summary>
    public Dictionary<ulong, float[]> TextRangeRects { get; } = [];

    /// <summary>A selection area's start and end endpoint rectangles.</summary>
    public float[] CrossSelectionEndpointRects { get; set; } = [];

    public bool SelectWordResult { get; set; } = true;

    private uint nextOffscreenId;

    public override uint HostCanvasCreateOffscreen(uint width, uint height)
    {
        base.HostCanvasCreateOffscreen(width, height);
        return ++nextOffscreenId;
    }

    /// <summary>What rendering any node to RGBA produces; empty (the host cannot render) when null.</summary>
    public byte[]? RenderedRgba { get; set; }

    public override byte[] UiRenderNodeToRgba(ulong handle, uint width, uint height, float scale, float x, float y)
    {
        base.UiRenderNodeToRgba(handle, width, height, scale, x, y);
        return RenderedRgba ?? [];
    }

    public override float[] UiReadTextRangeRects(ulong handle, uint startOffset, uint endOffset)
    {
        base.UiReadTextRangeRects(handle, startOffset, endOffset);
        return TextRangeRects.TryGetValue(handle, out var rects) ? rects : [];
    }

    public override float[] UiReadCrossSelectionEndpointRects(ulong areaHandle)
    {
        base.UiReadCrossSelectionEndpointRects(areaHandle);
        return CrossSelectionEndpointRects;
    }

    public override bool UiSelectWordAt(ulong handle, float logicalX, float logicalY)
    {
        base.UiSelectWordAt(handle, logicalX, logicalY);
        return SelectWordResult;
    }

    public override bool UiBeginSelectionEndpointDrag(ulong handle, uint endpoint)
    {
        base.UiBeginSelectionEndpointDrag(handle, endpoint);
        return true;
    }

    public override bool UiGetBounds(ulong handle, out float x, out float y, out float width, out float height)
    {
        base.UiGetBounds(handle, out x, out y, out width, out height);
        if (!Bounds.TryGetValue(handle, out var bounds))
        {
            return false;
        }
        (x, y, width, height) = (bounds.X, bounds.Y, bounds.Width, bounds.Height);
        return true;
    }

    /// <summary>Text metrics the host reports per handle once the node is prepared.</summary>
    public Dictionary<ulong, TextMetrics> TextMetrics { get; } = [];

    /// <summary>What <c>ui_prepare_node</c> returns; 0 reports a failed preparation.</summary>
    public uint PrepareResult { get; set; } = 1;

    public override uint UiPrepareNode(ulong handle)
    {
        base.UiPrepareNode(handle);
        return PrepareResult;
    }

    public override bool UiGetTextMetrics(ulong handle, out float width, out float height, out float baseline, out uint lineCount, out float maxLineWidth)
    {
        base.UiGetTextMetrics(handle, out width, out height, out baseline, out lineCount, out maxLineWidth);
        if (!TextMetrics.TryGetValue(handle, out var metrics))
        {
            return false;
        }
        (width, height, baseline, lineCount, maxLineWidth) = (metrics.Width, metrics.Height, metrics.Baseline, metrics.LineCount, metrics.MaxLineWidth);
        return true;
    }

    /// <summary>Visible bounds are the layout bounds: nothing in these tests is clipped.</summary>
    public override bool UiGetVisibleBounds(ulong handle, out float x, out float y, out float width, out float height)
    {
        base.UiGetVisibleBounds(handle, out x, out y, out width, out height);
        if (!Bounds.TryGetValue(handle, out var bounds))
        {
            return false;
        }
        (x, y, width, height) = (bounds.X, bounds.Y, bounds.Width, bounds.Height);
        return true;
    }

    /// <summary>The host's persisted text state, keyed by node id and kind.</summary>
    public Dictionary<(string NodeId, string Kind), (uint Version, string Payload)> PersistedText { get; } = [];

    public Dictionary<string, (float X, float Y)> PersistedScroll { get; } = [];

    public override void HostSetPersistedState(string nodeId, string kind, uint version, string payload)
    {
        base.HostSetPersistedState(nodeId, kind, version, payload);
        PersistedText[(nodeId, kind)] = (version, payload);
    }

    public override bool HostTryCopyPersistedState(string nodeId, string kind, out uint version, out string payload)
    {
        base.HostTryCopyPersistedState(nodeId, kind, out version, out payload);
        if (!PersistedText.TryGetValue((nodeId, kind), out var entry))
        {
            return false;
        }
        (version, payload) = entry;
        return true;
    }

    public override void HostSetPersistedScrollOffset(string nodeId, float x, float y)
    {
        base.HostSetPersistedScrollOffset(nodeId, x, y);
        PersistedScroll[nodeId] = (x, y);
    }

    public override bool HostTryGetPersistedScrollOffset(string nodeId, out float x, out float y)
    {
        base.HostTryGetPersistedScrollOffset(nodeId, out x, out y);
        if (!PersistedScroll.TryGetValue(nodeId, out var offset))
        {
            return false;
        }
        (x, y) = offset;
        return true;
    }

    public PlatformFamily PlatformFamily { get; set; } = PlatformFamily.Linux;

    public HostEnvironment HostEnvironment { get; set; } = HostEnvironment.Browser;

    public HostCapability HostCapabilities { get; set; } = HostCapability.BrowserHistory | HostCapability.ClipboardWrite;

    public override PlatformFamily HostGetPlatformFamily()
    {
        base.HostGetPlatformFamily();
        return PlatformFamily;
    }

    public override HostEnvironment HostGetHostEnvironment()
    {
        base.HostGetHostEnvironment();
        return HostEnvironment;
    }

    public bool CanNavigateBack { get; set; }

    public bool CanNavigateForward { get; set; }

    public override bool HostCanNavigateBack()
    {
        base.HostCanNavigateBack();
        return CanNavigateBack;
    }

    public override bool HostCanNavigateForward()
    {
        base.HostCanNavigateForward();
        return CanNavigateForward;
    }

    public override HostCapability HostGetHostCapabilities()
    {
        base.HostGetHostCapabilities();
        return HostCapabilities;
    }

    public override bool HostIsCoarsePointer()
    {
        base.HostIsCoarsePointer();
        return CoarsePointer;
    }

    public float ViewportWidth { get; set; } = 1280.0f;

    public float ViewportHeight { get; set; } = 720.0f;

    /// <summary>Every node created, in order, with its handle.</summary>
    public List<(NodeType Type, ulong Handle)> Created { get; } = [];

    private uint nextScopeToken = 1;

    public override float HostGetViewportWidth()
    {
        base.HostGetViewportWidth();
        return ViewportWidth;
    }

    public override float HostGetViewportHeight()
    {
        base.HostGetViewportHeight();
        return ViewportHeight;
    }

    public override ulong UiCreateNode(NodeType type)
    {
        var handle = base.UiCreateNode(type);
        Created.Add((type, handle));
        return handle;
    }

    public override uint UiPushSemanticScope(ulong handle)
    {
        base.UiPushSemanticScope(handle);
        return nextScopeToken++;
    }

    /// <summary>Handles created after <paramref name="mark"/> entries of <see cref="Created"/>.</summary>
    public List<ulong> CreatedSince(int mark)
    {
        var handles = new List<ulong>();
        for (var index = mark; index < Created.Count; ++index)
        {
            handles.Add(Created[index].Handle);
        }
        return handles;
    }

    public bool HasCall(string name, params object[] arguments) =>
        Calls.Exists(call => call.Name == name && Matches(call.Arguments, arguments));

    private static bool Matches(object?[] actual, object[] expected)
    {
        for (var index = 0; index < expected.Length; ++index)
        {
            if (index >= actual.Length || !Equals(actual[index], expected[index]))
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>Host input, sent the way the host sends it.</summary>
internal static class HostInput
{
    public static bool Pointer(
        PointerEventType type,
        ulong handle,
        float x = 0.0f,
        float y = 0.0f,
        int clickCount = 1,
        PointerType pointerType = PointerType.Mouse,
        int button = 0) =>
        HostEvents.OnPointerEvent(
            (uint)type, handle, x, y, 0, 1, (uint)pointerType, button, button == 0 ? 1u : 0u, 0.5f, 1.0f, 1.0f,
            clickCount, true, 0.0f, 0.0f, 0.0f, 0.0f);

    public static bool Click(ulong handle, float x = 0.0f, float y = 0.0f, int clickCount = 1)
    {
        Pointer(PointerEventType.Down, handle, x, y, clickCount);
        return Pointer(PointerEventType.Up, handle, x, y, clickCount);
    }

    public static bool Key(string key, KeyEventType type = KeyEventType.Down, KeyModifier modifiers = KeyModifier.None) =>
        HostEvents.OnKeyEvent((uint)type, key, (uint)modifiers);
}

internal static class RecordedCalls
{
    public static List<AbiCall> Named(this RecordingFuiAbi abi, string name) =>
        abi.Calls.FindAll(call => call.Name == name);

    public static int CountOf(this RecordingFuiAbi abi, string name) => abi.Named(name).Count;

    /// <summary>The names of the recorded calls, in order, keeping only <paramref name="names"/>.</summary>
    public static string Sequence(this RecordingFuiAbi abi, params string[] names)
    {
        var kept = new List<string>();
        foreach (var call in abi.Calls)
        {
            if (Array.IndexOf(names, call.Name) >= 0)
            {
                kept.Add(call.Name);
            }
        }
        return string.Join(",", kept);
    }

    /// <summary>Argument <paramref name="index"/> of every call named <paramref name="name"/>, joined.</summary>
    public static string Arguments(this RecordingFuiAbi abi, string name, int index)
    {
        var values = new List<string>();
        foreach (var call in abi.Named(name))
        {
            values.Add(Convert.ToString(call.Arguments[index], System.Globalization.CultureInfo.InvariantCulture) ?? "null");
        }
        return string.Join(",", values);
    }
}

/// <summary>A plain retained node, built the way framework nodes build themselves.</summary>
internal sealed class TestNode : Node
{
    public List<bool> EnabledChanges { get; } = [];

    public List<Visibility> VisibilityChanges { get; } = [];

    public int ParentChanges { get; private set; }

    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        Handle = Ui.CreateNode(NodeType.FlexBox);
        ApplyNodeMetadata();
        FinishBuild();
        BuildChildren();
        return Handle;
    }

    public void MakeInteractive() => RequireInteractive();

    public void TrackResource(IDisposable resource) => Track(resource);

    public void SetDefaultRole(SemanticRole role) => DefaultSemanticRole = role;

    public void SetDefaultLabel(string? label) => DefaultSemanticLabel = label;

    public void TrackSemanticDisabled() => ReflectSemanticDisabledFromEnabled();

    protected override void OnEffectiveEnabledChanged(bool isEnabled) => EnabledChanges.Add(isEnabled);

    protected override void OnEffectiveVisibilityChanged(Visibility visibility) => VisibilityChanges.Add(visibility);

    protected override void OnParentChanged() => ParentChanges++;
}

internal sealed class DisposeProbe(List<string> log, string name) : IDisposable
{
    public void Dispose() => log.Add(name);
}
