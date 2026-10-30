# HostEvents

class in `FuiCs.Hosting`

Entry points for the host: application lifecycle, frames and input. The host calls them through the exports that the FuiCs build adds to every application; application code does not call them.

## Methods

- `CanShowContextMenu(ulong)`: Whether the app may show a context menu over `handle`.
- `CapturePersistedUiState()`: Host entry point, called by the generated exports; applications do not call it.
- `DisposeApp()`: Host entry point, called by the generated exports; applications do not call it.
- `FlushRenders()`: Host entry point, called by the generated exports; applications do not call it.
- `GetGestureIntent(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `GetLongPressMinimumDurationMs(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `GetLongPressMovementTolerance(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `HideActiveContextMenu()`: Host entry point, called by the generated exports; applications do not call it.
- `LongPressContinuesPointerEvents(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `NeedsAnimationFrame()`: Whether the host should keep sending frames: an animation is running.
- `OnContextMenu(ulong, float, float)`: The user asked for a context menu over `handle` at a page point.
- `OnCrossSelectionChanged(ulong, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnCustomDraw(ulong, uint)`: The render pass reached a custom-drawn node; `canvas` is the host canvas token.
- `OnFetchComplete(uint, bool, int, byte[])`: A fetch finished; `payload` holds the status text and final URL as text parts.
- `OnFetchError(uint, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnFocusChanged(ulong, bool)`: Host entry point, called by the generated exports; applications do not call it.
- `OnFontLoaded(uint)`: Host entry point, called by the generated exports; applications do not call it.
- `OnFrame(double)`: Host entry point, called by the generated exports; applications do not call it.
- `OnGestureEvent(ulong, uint, uint, float, float, float, float, float, int)`: Host entry point, called by the generated exports; applications do not call it.
- `OnKeyEvent(uint, string, uint)`: Offers a key event to the key filters, then to the focused node.
- `OnLongPressEvent(ulong, float, float, int, uint, uint, int)`: Host entry point, called by the generated exports; applications do not call it.
- `OnPointerEvent(uint, ulong, float, float, uint, int, uint, int, uint, float, float, float, int, bool, float, float, float, float)`: Host entry point, called by the generated exports; applications do not call it.
- `OnRouteChanged(string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnScroll(ulong, float, float, float, float, float, float)`: Host entry point, called by the generated exports; applications do not call it.
- `OnSelectionChanged(ulong, uint, uint)`: Host entry point, called by the generated exports; applications do not call it.
- `OnSvgFailed(uint, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnSvgLoaded(uint, float, float)`: Host entry point, called by the generated exports; applications do not call it.
- `OnSystemAccentColorChanged(uint)`: Host entry point, called by the generated exports; applications do not call it.
- `OnSystemDarkModeChanged(bool)`: Host entry point, called by the generated exports; applications do not call it.
- `OnTextChanged(ulong, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnTextReplaced(ulong, uint, uint, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnTextureFailed(uint, string)`: Host entry point, called by the generated exports; applications do not call it.
- `OnTextureLoaded(uint, float, float)`: Host entry point, called by the generated exports; applications do not call it.
- `OnViewportChanged(float, float)`: Host entry point, called by the generated exports; applications do not call it.
- `OnWheelEvent(ulong, float, float, float, float, uint, uint)`: Host entry point, called by the generated exports; applications do not call it.
- `OnWorkerMessage(uint, uint, string)`: A worker's progress (kind 0), result (1) or failure (2).
- `ResolveGestureOwner(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `ResolveLongPressOwner(ulong)`: Host entry point, called by the generated exports; applications do not call it.
- `RestorePersistedUiState()`: Host entry point, called by the generated exports; applications do not call it.
- `RunApp(Func<IApplication>)`: Runs the current application, creating it with `createApplication` the first time.
- `Trace(string)`: Logs host input when the library is built with FUICS_TRACE_EVENTS; otherwise the call and its message are compiled away, which keeps number formatting out of applications that never format numbers.
