namespace FuiCs;

/// <summary>
/// A multi-line text field in a scroll box. It wraps by default, scrolls vertically, and scrolls sideways when
/// <see cref="TextInputCore.Wrapping"/> is off; Enter inserts a line break, and a press anywhere in it focuses the editor.
/// </summary>
public class TextArea(string text = "") : TextInputCore(TextInputProfile.MultilineArea, text);
