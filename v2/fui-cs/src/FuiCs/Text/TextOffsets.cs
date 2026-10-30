using System.Text;

namespace FuiCs;

/// <summary>
/// Conversions between the three ways text positions are counted: characters (Unicode scalar values) in public
/// editing APIs, UTF-16 indices in .NET strings, and UTF-8 byte offsets at the runtime boundary.
/// </summary>
internal static class TextOffsets
{
    public static int CharacterCount(string text)
    {
        var count = 0;
        foreach (var _ in text.EnumerateRunes())
        {
            ++count;
        }
        return count;
    }

    public static uint Utf8Length(string text) => (uint)Encoding.UTF8.GetByteCount(text);

    /// <summary>The UTF-8 offset of a character position, clamped to the text.</summary>
    public static uint Utf8OffsetOfCharacter(string text, int character)
    {
        var bytes = 0u;
        var count = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            if (count == character)
            {
                break;
            }
            bytes += (uint)rune.Utf8SequenceLength;
            ++count;
        }
        return bytes;
    }

    /// <summary>The string index of a character position, clamped to the text.</summary>
    public static int StringOffsetOfCharacter(string text, int character)
    {
        var index = 0;
        var count = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            if (count == character)
            {
                break;
            }
            index += rune.Utf16SequenceLength;
            ++count;
        }
        return index;
    }

    /// <summary>The position of the character that starts at or before a UTF-8 offset, clamped to the text.</summary>
    public static int CharacterOfUtf8Offset(string text, uint utf8Offset)
    {
        var bytes = 0u;
        var count = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            if (bytes + (uint)rune.Utf8SequenceLength > utf8Offset)
            {
                break;
            }
            bytes += (uint)rune.Utf8SequenceLength;
            ++count;
        }
        return count;
    }

    /// <summary>The string index of the code point that starts at <paramref name="utf8Offset"/> bytes into <paramref name="text"/>.</summary>
    public static int StringOffsetOfUtf8Offset(string text, uint utf8Offset)
    {
        var bytes = 0u;
        var index = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            if (bytes + (uint)rune.Utf8SequenceLength > utf8Offset)
            {
                break;
            }
            bytes += (uint)rune.Utf8SequenceLength;
            index += rune.Utf16SequenceLength;
        }
        return index;
    }
}
