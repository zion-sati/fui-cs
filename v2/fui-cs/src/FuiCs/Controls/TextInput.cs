namespace FuiCs;

/// <summary>
/// A single-line text field. Typing, pasting and the browser's editing keys edit <see cref="TextInputCore.Text"/>,
/// Enter inserts nothing, and the arrows, Home and End move the caret without scrolling the page.
/// </summary>
public class TextInput(string text = "") : TextInputCore(TextInputProfile.SingleLine, text);
