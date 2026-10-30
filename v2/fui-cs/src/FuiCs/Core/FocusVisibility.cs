namespace FuiCs;

/// <summary>
/// Whether focus indicators show: hidden after pointer presses and Escape, shown again by keyboard
/// navigation. Caret movement and shortcuts leave it unchanged.
/// </summary>
internal static class FocusVisibility
{
    public static Signal<bool> KeyboardFocusVisible { get; } = new(true);

    public static void ShowForPointerEvent(PointerEventType eventType)
    {
        if (eventType == PointerEventType.Down)
        {
            KeyboardFocusVisible.Value = false;
        }
    }

    public static void ShowForKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        if (eventType != KeyEventType.Down || IsModifierKey(key))
        {
            return;
        }
        if ((modifiers & (KeyModifier.Ctrl | KeyModifier.Alt | KeyModifier.Meta)) != 0 || IsCaretNavigationKey(key))
        {
            return;
        }
        KeyboardFocusVisible.Value = key != "Escape";
    }

    public static void Reset() => KeyboardFocusVisible.Reset(true);

    private static bool IsModifierKey(string key) => key is "Shift" or "Control" or "Alt" or "Meta";

    private static bool IsCaretNavigationKey(string key) =>
        key is "ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown" or "Home" or "End" or "PageUp" or "PageDown";
}
