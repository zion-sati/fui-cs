using System.Globalization;

namespace FuiCs;

/// <summary>What a <see cref="DynamicTextLayout"/> does with text that has characters outside its charset.</summary>
public enum DynamicTextOverflow
{
    /// <summary><see cref="DynamicTextLayout.TrySetText"/> returns false and the text is unchanged.</summary>
    Reject = 0,

    /// <summary>The text is accepted and shaped like any other text.</summary>
    FallbackShape = 1,
}

/// <summary>
/// Short, single-style text that changes often, such as counters and clocks. The host prepares the glyphs of a
/// fixed charset once, so new text reshapes without a font round trip; once ready, every accepted change is
/// prepared immediately and stays measurable.
/// </summary>
public class DynamicTextLayout : TextLayout
{
    private string charset;
    private int? precision;
    private string prefix = "";
    private string suffix = "";

    /// <summary>A layout for text drawn from <paramref name="charset"/>; an empty charset accepts any text.</summary>
    public DynamicTextLayout(string charset = "")
        : base(new Text())
    {
        ArgumentNullException.ThrowIfNull(charset);
        this.charset = charset;
    }

    /// <summary>A layout for numbers set with <see cref="TrySetValue"/>, formatted with <see cref="Precision"/>, <see cref="Prefix"/> and <see cref="Suffix"/>.</summary>
    public static DynamicTextLayout Numeric() => new("0123456789.-");

    /// <summary>The prepared characters, including those of <see cref="Prefix"/> and <see cref="Suffix"/>.</summary>
    public string Charset => charset;

    /// <summary>What the layout does with characters its prepared charset lacks.</summary>
    public DynamicTextOverflow CharsetOverflow { get; set; } = DynamicTextOverflow.FallbackShape;

    /// <summary>The last number set with <see cref="TrySetValue"/>; null until one is set.</summary>
    public double? Value { get; private set; }

    /// <summary>Digits after the decimal point; null formats the shortest text that round-trips.</summary>
    public int? Precision
    {
        get => precision;
        set
        {
            if (value is { } digits)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(digits);
            }
            precision = value;
            RefreshValue();
        }
    }

    /// <summary>Text drawn before the value.</summary>
    public string Prefix
    {
        get => prefix;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            prefix = value;
            IncludeInCharset(value);
            RefreshValue();
        }
    }

    /// <summary>Text drawn after the value.</summary>
    public string Suffix
    {
        get => suffix;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            suffix = value;
            IncludeInCharset(value);
            RefreshValue();
        }
    }

    /// <exception cref="ArgumentException">The text has characters outside the charset and <see cref="CharsetOverflow"/> is <see cref="DynamicTextOverflow.Reject"/>.</exception>
    public override string Text
    {
        get => base.Text;
        set
        {
            if (!TrySetText(value))
            {
                throw new ArgumentException("The text has characters outside the layout's charset.", nameof(value));
            }
        }
    }

    /// <summary>Sets the text; returns false when it is rejected under <see cref="DynamicTextOverflow.Reject"/>.</summary>
    public bool TrySetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (CharsetOverflow == DynamicTextOverflow.Reject && !IsInCharset(text))
        {
            return false;
        }
        var wasReady = IsReady;
        base.Text = text;
        if (wasReady)
        {
            Prepare();
        }
        return true;
    }

    /// <summary>Shows <paramref name="value"/> between <see cref="Prefix"/> and <see cref="Suffix"/>; returns false when the text is rejected.</summary>
    public bool TrySetValue(double value)
    {
        Value = value;
        return TrySetText(Compose(value));
    }

    private protected override void Prepare()
    {
        Bindings.Ui.SetDynamicTextCharset(EnsureBuilt(), charset);
        base.Prepare();
    }

    private bool IsInCharset(string text)
    {
        if (charset.Length == 0)
        {
            return true;
        }
        foreach (var character in text)
        {
            if (!charset.Contains(character))
            {
                return false;
            }
        }
        return true;
    }

    private void IncludeInCharset(string text)
    {
        foreach (var character in text)
        {
            if (!charset.Contains(character))
            {
                charset += character;
            }
        }
    }

    private void RefreshValue()
    {
        if (Value is { } value)
        {
            TrySetText(Compose(value));
        }
    }

    private string Compose(double value) => string.Concat(prefix, Format(value), suffix);

    private string Format(double value) => precision is { } digits && double.IsFinite(value)
        ? value.ToString("F" + digits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
        : value.ToString(CultureInfo.InvariantCulture);
}
