// Compiled into every FuiCs application by FuiCs.Exports.targets, which also generates CreateApplication.
using System.Runtime.InteropServices.JavaScript;

namespace FuiCs.Hosting;

/// <summary>The exports the EffinDOM host calls. Results the host tests with <c>!== 0</c> are returned as int.</summary>
public static partial class HostExports
{
    /// <summary>Starts the application: initializes the runtime with the browser ABI and runs <c>FuiProgram.CreateApplication()</c>.</summary>
    [JSExport("__runApp")]
    public static void RunApp()
    {
        if (!Runtime.IsInitialized)
        {
            Runtime.Init(new JsImportFuiAbi());
        }
        HostEvents.RunApp(CreateApplication);
    }

    /// <summary>The application's <c>FuiProgram.CreateApplication</c>, bound by the generated entry file.</summary>
    private static partial IApplication CreateApplication();

    [JSExport("__disposeApp")]
    public static void DisposeApp() => HostEvents.DisposeApp();

    [JSExport("__flushRenders")]
    public static void FlushRenders() => HostEvents.FlushRenders();

    [JSExport("__fui_on_viewport_changed")]
    public static void OnViewportChanged(float width, float height) => HostEvents.OnViewportChanged(width, height);

    [JSExport("__fui_on_frame")]
    public static void OnFrame(double timestampMs) => HostEvents.OnFrame(timestampMs);

    [JSExport("__fui_needs_animation_frame")]
    public static bool NeedsAnimationFrame() => HostEvents.NeedsAnimationFrame();

    [JSExport("__fui_on_system_dark_mode_changed")]
    public static void OnSystemDarkModeChanged(bool isDark) => HostEvents.OnSystemDarkModeChanged(isDark);

    [JSExport("__fui_on_system_accent_color_changed")]
    public static void OnSystemAccentColorChanged(uint color) => HostEvents.OnSystemAccentColorChanged(color);

    [JSExport("fui_dispatch_custom_draw")]
    public static void DispatchCustomDraw(ulong handle, uint canvas) => HostEvents.OnCustomDraw(handle, canvas);

    [JSExport("__fui_on_fetch_complete")]
    public static void OnFetchComplete(uint requestId, bool ok, int status, byte[] payload) => HostEvents.OnFetchComplete(requestId, ok, status, payload);

    [JSExport("__fui_on_fetch_error")]
    public static void OnFetchError(uint requestId, string message) => HostEvents.OnFetchError(requestId, message);

    [JSExport("__fui_on_worker_message")]
    public static void OnWorkerMessage(uint workerId, uint kind, string text) => HostEvents.OnWorkerMessage(workerId, kind, text);

    [JSExport("__fui_on_svg_loaded")]
    public static void OnSvgLoaded(uint svgId, float width, float height) => HostEvents.OnSvgLoaded(svgId, width, height);

    [JSExport("__fui_on_svg_failed")]
    public static void OnSvgFailed(uint svgId, string error) => HostEvents.OnSvgFailed(svgId, error);

    [JSExport("__fui_on_texture_loaded")]
    public static void OnTextureLoaded(uint textureId, float width, float height) => HostEvents.OnTextureLoaded(textureId, width, height);

    [JSExport("__fui_on_texture_failed")]
    public static void OnTextureFailed(uint textureId, string error) => HostEvents.OnTextureFailed(textureId, error);

    [JSExport("__fui_capture_persisted_ui_state")]
    public static void CapturePersistedUiState() => HostEvents.CapturePersistedUiState();

    [JSExport("__fui_restore_persisted_ui_state")]
    public static void RestorePersistedUiState() => HostEvents.RestorePersistedUiState();

    [JSExport("__fui_on_route_changed")]
    public static void OnRouteChanged(string route) => HostEvents.OnRouteChanged(route);

    [JSExport("__fui_on_font_loaded")]
    public static void OnFontLoaded(uint fontId) => HostEvents.OnFontLoaded(fontId);

    [JSExport("__fui_on_pointer_event_with_metadata")]
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
        float twist) =>
        HostEvents.OnPointerEvent(
            eventType, handle, x, y, modifiers, pointerId, pointerType, button, buttons, pressure, width, height,
            clickCount, isPrimary, tangentialPressure, tiltX, tiltY, twist);

    [JSExport("__fui_on_wheel_event")]
    public static int OnWheelEvent(ulong handle, float x, float y, float deltaX, float deltaY, uint deltaMode, uint modifiers) =>
        HostEvents.OnWheelEvent(handle, x, y, deltaX, deltaY, deltaMode, modifiers) ? 1 : 0;

    [JSExport("__fui_resolve_gesture_owner")]
    public static ulong ResolveGestureOwner(ulong handle) => HostEvents.ResolveGestureOwner(handle);

    [JSExport("__fui_get_gesture_intent")]
    public static uint GetGestureIntent(ulong handle) => HostEvents.GetGestureIntent(handle);

    [JSExport("__fui_resolve_long_press_owner")]
    public static ulong ResolveLongPressOwner(ulong handle) => HostEvents.ResolveLongPressOwner(handle);

    [JSExport("__fui_get_long_press_minimum_duration_ms")]
    public static int GetLongPressMinimumDurationMs(ulong handle) => HostEvents.GetLongPressMinimumDurationMs(handle);

    [JSExport("__fui_get_long_press_movement_tolerance")]
    public static float GetLongPressMovementTolerance(ulong handle) => HostEvents.GetLongPressMovementTolerance(handle);

    [JSExport("__fui_long_press_continues_pointer_events")]
    public static bool LongPressContinuesPointerEvents(ulong handle) => HostEvents.LongPressContinuesPointerEvents(handle);

    [JSExport("__fui_on_gesture_event")]
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
        HostEvents.OnGestureEvent(handle, phase, kind, x, y, deltaX, deltaY, scale, pointerCount);

    [JSExport("__fui_on_long_press_event")]
    public static bool OnLongPressEvent(ulong handle, float x, float y, int pointerId, uint pointerType, uint modifiers, int durationMs) =>
        HostEvents.OnLongPressEvent(handle, x, y, pointerId, pointerType, modifiers, durationMs);

    [JSExport("__fui_on_focus_changed")]
    public static void OnFocusChanged(ulong handle, bool focused) => HostEvents.OnFocusChanged(handle, focused);

    [JSExport("__fui_on_key_event")]
    public static int OnKeyEvent(uint eventType, string key, uint modifiers) => HostEvents.OnKeyEvent(eventType, key, modifiers) ? 1 : 0;

    [JSExport("__fui_on_scroll")]
    public static void OnScroll(
        ulong handle,
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight) =>
        HostEvents.OnScroll(handle, offsetX, offsetY, contentWidth, contentHeight, viewportWidth, viewportHeight);

    [JSExport("__fui_on_text_changed")]
    public static void OnTextChanged(ulong handle, string text) => HostEvents.OnTextChanged(handle, text);

    [JSExport("__fui_on_text_replaced")]
    public static void OnTextReplaced(ulong handle, uint start, uint end, string text) => HostEvents.OnTextReplaced(handle, start, end, text);

    [JSExport("__fui_on_selection_changed")]
    public static void OnSelectionChanged(ulong handle, uint start, uint end) => HostEvents.OnSelectionChanged(handle, start, end);

    [JSExport("__fui_on_context_menu")]
    public static void OnContextMenu(ulong handle, float x, float y) => HostEvents.OnContextMenu(handle, x, y);

    [JSExport("__fui_can_show_context_menu")]
    public static bool CanShowContextMenu(ulong handle) => HostEvents.CanShowContextMenu(handle);

    [JSExport("__fui_hide_active_context_menu")]
    public static void HideActiveContextMenu() => HostEvents.HideActiveContextMenu();

    [JSExport("__fui_on_cross_selection_changed")]
    public static void OnCrossSelectionChanged(ulong handle, string text) => HostEvents.OnCrossSelectionChanged(handle, text);

#if FUICS_DEBUG_EXPORTS
    // Input injection for the harness debug API (window.__fui_debug), compiled in only when FuiCsDebugExports is on.
    [JSExport("__fui_debug_pointer_event")]
    public static void DebugPointerEvent(uint eventType, ulong handle, float x, float y, uint modifiers) =>
        HostEvents.OnPointerEvent(eventType, handle, x, y, modifiers, -1, (uint)PointerType.Mouse, 0, 0, 0.0f, 0.0f, 0.0f, 0, true, 0.0f, 0.0f, 0.0f, 0.0f);

    [JSExport("__fui_debug_focus_changed")]
    public static void DebugFocusChanged(ulong handle, bool focused) => HostEvents.OnFocusChanged(handle, focused);

    [JSExport("__fui_debug_key_event")]
    public static void DebugKeyEvent(uint eventType, string key, uint modifiers) => HostEvents.OnKeyEvent(eventType, key, modifiers);

    [JSExport("__fui_debug_scroll")]
    public static void DebugScroll(
        ulong handle,
        float offsetX,
        float offsetY,
        float contentWidth,
        float contentHeight,
        float viewportWidth,
        float viewportHeight) =>
        HostEvents.OnScroll(handle, offsetX, offsetY, contentWidth, contentHeight, viewportWidth, viewportHeight);
#endif
}
