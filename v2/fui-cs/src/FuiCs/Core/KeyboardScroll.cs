namespace FuiCs;

/// <summary>
/// Scrolls with the keyboard when nothing focused handles the key: arrows by a line, Page Up and Page Down by a
/// viewport less some overlap, Home and End to the ends. The focused node's scroll views go first, then the view the
/// last pointer release selected, then the others.
/// </summary>
internal static class KeyboardScroll
{
    private const float LineStep = 40.0f;
    private const float PageOverlap = 40.0f;
    private const float Tolerance = 0.5f;

    public static bool TryHandle(string key, KeyModifier modifiers)
    {
        if (modifiers != KeyModifier.None || !IsScrollKey(key))
        {
            return false;
        }
        if (EventRouter.FocusedNode is { } focused)
        {
            if (focused is TextCore text && SkipsTextOwnedKey(text, key))
            {
                return false;
            }
            for (Node? node = focused; node is not null; node = node.Parent)
            {
                if (node is ScrollView view && TryScroll(view, key))
                {
                    return true;
                }
            }
        }
        if (KeyboardScrollTracker.Selected is { } selected && TryScroll(selected, key))
        {
            return true;
        }
        foreach (var candidate in KeyboardScrollTracker.FallbackCandidates())
        {
            if (TryScroll(candidate, key))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsScrollKey(string key) =>
        key is "ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown" or "PageUp" or "PageDown" or "Home" or "End";

    /// <summary>Text keeps Home and End, and editable text keeps the arrows, for its caret.</summary>
    private static bool SkipsTextOwnedKey(TextCore text, string key) =>
        key is "Home" or "End" || (text.IsEditableText && key is "ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown");

    private static float PageStep(float viewport) =>
        viewport <= 0.0f ? 0.0f : viewport > PageOverlap ? viewport - PageOverlap : viewport * 0.875f;

    private static bool TryScroll(ScrollView view, string key)
    {
        var state = view.ScrollState;
        if (key is "ArrowLeft" or "ArrowRight")
        {
            var maxX = Math.Max(0.0f, state.ContentWidth.Value - state.ViewportWidth.Value);
            if (!view.IsHorizontalScrollEnabled || maxX <= Tolerance)
            {
                return false;
            }
            var currentX = Math.Clamp(state.OffsetX.Value, 0.0f, maxX);
            var nextX = Math.Clamp(currentX + (key == "ArrowLeft" ? -LineStep : LineStep), 0.0f, maxX);
            if (MathF.Abs(nextX - currentX) <= Tolerance)
            {
                return false;
            }
            view.ScrollTo(nextX, state.OffsetY.Value);
            return true;
        }
        var viewportHeight = state.ViewportHeight.Value;
        var maxY = Math.Max(0.0f, state.ContentHeight.Value - viewportHeight);
        if (!view.IsVerticalScrollEnabled || maxY <= Tolerance)
        {
            return false;
        }
        var currentY = Math.Clamp(state.OffsetY.Value, 0.0f, maxY);
        var nextY = Math.Clamp(key switch
        {
            "ArrowUp" => currentY - LineStep,
            "ArrowDown" => currentY + LineStep,
            "PageUp" => currentY - PageStep(viewportHeight),
            "PageDown" => currentY + PageStep(viewportHeight),
            "Home" => 0.0f,
            _ => maxY,
        }, 0.0f, maxY);
        if (MathF.Abs(nextY - currentY) <= Tolerance)
        {
            return false;
        }
        view.ScrollTo(state.OffsetX.Value, nextY);
        return true;
    }
}
