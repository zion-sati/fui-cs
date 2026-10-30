using System.ComponentModel;
using System.Diagnostics;

namespace FuiCs.Hosting;

/// <summary>
/// Entry points for the host: application lifecycle, frames and input. The host calls them through the exports that the FuiCs build adds to every
/// application; application code does not call them.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HostEvents
{
    private static ulong focusedHandle;

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static bool OnPointerEvent(
        uint eventType,
        ulong handle,
        float x,
        float y,
        uint modifiers,
        int pointerId,
        uint pointerType,
        int button,
        uint buttons,
        float pressure,
        float width,
        float height,
        int clickCount,
        bool isPrimary,
        float tangentialPressure,
        float tiltX,
        float tiltY,
        float twist)
    {
        Trace("pointer type=" + eventType + " handle=" + handle + " modifiers=" + modifiers + " pointerId=" + pointerId
            + " pointerType=" + pointerType + " button=" + button + " buttons=" + buttons + " clickCount=" + clickCount);
        var args = new PointerEventArgs(
            (PointerEventType)eventType,
            x,
            y,
            (KeyModifier)modifiers,
            pointerId,
            (PointerType)pointerType,
            (PointerButton)button,
            (PointerButtons)buttons,
            pressure,
            width,
            height,
            clickCount,
            isPrimary,
            tangentialPressure,
            tiltX,
            tiltY,
            twist);
        ContextMenuManager.TrackPointerEvent(args.EventType, handle);
        return EventRouter.DispatchPointerEvent(handle, args);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static bool OnWheelEvent(ulong handle, float x, float y, float deltaX, float deltaY, uint deltaMode, uint modifiers)
    {
        Trace("wheel handle=" + handle + " modifiers=" + modifiers);
        return EventRouter.DispatchWheelEvent(
            handle,
            new WheelEventArgs(x, y, deltaX, deltaY, (WheelDeltaMode)deltaMode, (KeyModifier)modifiers));
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static ulong ResolveGestureOwner(ulong handle) => EventRouter.ResolveGestureOwner(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static uint GetGestureIntent(ulong handle) => (uint)EventRouter.GetGestureIntent(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static ulong ResolveLongPressOwner(ulong handle) => EventRouter.ResolveLongPressOwner(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static int GetLongPressMinimumDurationMs(ulong handle) => EventRouter.GetLongPressMinimumDurationMs(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static float GetLongPressMovementTolerance(ulong handle) => EventRouter.GetLongPressMovementTolerance(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static bool LongPressContinuesPointerEvents(ulong handle) => EventRouter.LongPressContinuesPointerEvents(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static bool OnGestureEvent(
        ulong handle,
        uint phase,
        uint kind,
        float x,
        float y,
        float deltaX,
        float deltaY,
        float scale,
        int pointerCount) =>
        EventRouter.DispatchGestureEvent(
            handle,
            new GestureEventArgs((GestureEventPhase)phase, (GestureEventKind)kind, x, y, deltaX, deltaY, scale, pointerCount));

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static bool OnLongPressEvent(ulong handle, float x, float y, int pointerId, uint pointerType, uint modifiers, int durationMs) =>
        EventRouter.DispatchLongPressEvent(
            handle,
            new LongPressEventArgs(x, y, pointerId, (PointerType)pointerType, (KeyModifier)modifiers, durationMs));

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnFocusChanged(ulong handle, bool focused)
    {
        Trace("focus handle=" + handle + " focused=" + (focused ? "true" : "false"));
        if (focused)
        {
            focusedHandle = handle;
        }
        else if (focusedHandle == handle)
        {
            focusedHandle = 0;
        }
        EventRouter.DispatchFocusChanged(handle, focused);
    }

    /// <summary>Offers a key event to the key filters, then to the focused node.</summary>
    public static bool OnKeyEvent(uint eventType, string key, uint modifiers)
    {
        Trace("key type=" + eventType + " key=" + key + " modifiers=" + modifiers);
        var type = (KeyEventType)eventType;
        var keyModifiers = (KeyModifier)modifiers;
        if (EventRouter.DispatchGlobalKeyEvent(type, key, keyModifiers))
        {
            return true;
        }
        if (focusedHandle != 0 && EventRouter.DispatchKeyEvent(focusedHandle, type, key, keyModifiers))
        {
            return true;
        }
        return type == KeyEventType.Down && EventRouter.DispatchUnhandledKeyDown(key, keyModifiers);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnScroll(
        ulong handle,
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight)
    {
        Trace("scroll handle=" + handle);
        EventRouter.DispatchScroll(handle, offsetX, offsetY, contentWidth, contentHeight, viewportWidth, viewportHeight);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnTextChanged(ulong handle, string text)
    {
        Trace("text-changed handle=" + handle + " text=" + text);
        EventRouter.DispatchTextChanged(handle, text);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnTextReplaced(ulong handle, uint start, uint end, string text)
    {
        Trace("text-replaced handle=" + handle + " start=" + start + " end=" + end + " text=" + text);
        EventRouter.DispatchTextReplaced(handle, start, end, text);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnSelectionChanged(ulong handle, uint start, uint end)
    {
        Trace("selection-changed handle=" + handle + " start=" + start + " end=" + end);
        EventRouter.DispatchSelectionChanged(handle, start, end);
    }

    /// <summary>The user asked for a context menu over <paramref name="handle"/> at a page point.</summary>
    public static void OnContextMenu(ulong handle, float x, float y)
    {
        Trace("context-menu handle=" + handle);
        ContextMenuManager.Show(handle, x, y);
    }

    /// <summary>Whether the app may show a context menu over <paramref name="handle"/>.</summary>
    public static bool CanShowContextMenu(ulong handle) => ContextMenuManager.CanShow(handle);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void HideActiveContextMenu() => ContextMenu.HideActive();

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnCrossSelectionChanged(ulong handle, string text)
    {
        Trace("cross-selection handle=" + handle + " text=" + text);
        ContextMenuManager.HandleSelectionChanged(text);
        EventRouter.DispatchCrossSelectionChanged(handle, text);
    }

    /// <summary>Runs the current application, creating it with <paramref name="createApplication"/> the first time.</summary>
    public static void RunApp(Func<IApplication> createApplication)
    {
        ArgumentNullException.ThrowIfNull(createApplication);
        Application.RunOrCreate(createApplication);
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void DisposeApp() => Application.DisposeCurrent();

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void FlushRenders() => Application.FlushCurrent();

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnViewportChanged(float width, float height)
    {
        Trace("viewport width=" + width + " height=" + height);
        Application.ViewportWidth.Value = width;
        Application.ViewportHeight.Value = height;
    }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnFrame(double timestampMs)
    {
        Application.FrameTime.Value = timestampMs;
        AnimationManager.Shared.Tick(timestampMs);
    }

    /// <summary>Whether the host should keep sending frames: an animation is running.</summary>
    public static bool NeedsAnimationFrame() => AnimationManager.Shared.HasActiveAnimations;

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnSystemDarkModeChanged(bool isDark) => Theme.HandleSystemDarkModeChanged(isDark);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnSystemAccentColorChanged(uint color) => Theme.HandleSystemAccentColorChanged(color);

    /// <summary>The render pass reached a custom-drawn node; <paramref name="canvas"/> is the host canvas token.</summary>
    public static void OnCustomDraw(ulong handle, uint canvas) => CustomDraw?.Invoke(handle, canvas);

    /// <summary>A fetch finished; <paramref name="payload"/> holds the status text and final URL as text parts.</summary>
    public static void OnFetchComplete(uint requestId, bool ok, int status, byte[] payload) => FetchCompleted?.Invoke(requestId, ok, status, payload);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnFetchError(uint requestId, string message) => FetchFailed?.Invoke(requestId, message);

    /// <summary>A worker's progress (kind 0), result (1) or failure (2).</summary>
    public static void OnWorkerMessage(uint workerId, uint kind, string text) => WorkerMessage?.Invoke(workerId, kind, text);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnSvgLoaded(uint svgId, float width, float height) => SvgLoaded?.Invoke(svgId, width, height);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnSvgFailed(uint svgId, string error) => SvgFailed?.Invoke(svgId, error);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnTextureLoaded(uint textureId, float width, float height) => TextureLoaded?.Invoke(textureId, width, height);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnTextureFailed(uint textureId, string error) => TextureFailed?.Invoke(textureId, error);

    // Callbacks of optional features go through hooks the feature installs when it is first used. The exports are
    // compiled into every application, so calling the feature directly would keep it in applications that never use it.
    internal static Action<ulong, uint>? CustomDraw { get; set; }

    internal static Action<uint, bool, int, byte[]>? FetchCompleted { get; set; }

    internal static Action<uint, string>? FetchFailed { get; set; }

    internal static Action<uint, uint, string>? WorkerMessage { get; set; }

    internal static Action<uint, float, float>? SvgLoaded { get; set; }

    internal static Action<uint, string>? SvgFailed { get; set; }

    internal static Action<uint, float, float>? TextureLoaded { get; set; }

    internal static Action<uint, string>? TextureFailed { get; set; }

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnFontLoaded(uint fontId) => FontRegistry.DispatchFontLoaded(fontId);

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void CapturePersistedUiState() => Application.CapturePersistedUiState();

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void RestorePersistedUiState() => Application.RestorePersistedUiState();

    /// <summary>Host entry point, called by the generated exports; applications do not call it.</summary>
    public static void OnRouteChanged(string route)
    {
        ArgumentNullException.ThrowIfNull(route);
        Navigation.CurrentRoute.Value = route;
    }

    internal static void Reset()
    {
        focusedHandle = 0;
        ContextMenuManager.Reset();
    }

    /// <summary>
    /// Logs host input when the library is built with FUICS_TRACE_EVENTS; otherwise the call and its message
    /// are compiled away, which keeps number formatting out of applications that never format numbers.
    /// </summary>
    [Conditional("FUICS_TRACE_EVENTS")]
    private static void Trace(string message) => Logger.Log("Event", message);
}
