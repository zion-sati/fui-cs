using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A text layout becoming ready to measure.</summary>
public sealed class TextLayoutReadyEventArgs(TextLayout layout) : EventArgs
{
    /// <summary>The layout.</summary>
    public TextLayout Layout { get; } = layout;
}

/// <summary>
/// Formatted text for drawing and measuring outside the page tree. The host shapes it like a <see cref="FuiCs.Text"/>
/// node, with the same fonts, fallbacks and rich-text runs. It can be measured once <see cref="IsReady"/>, which
/// the fluent <c>OnReady</c> callback reports; any change makes it unready until it is prepared again.
/// </summary>
public class TextLayout : ITextFormat, ISizable, IDisposable
{
    private readonly Text node;
    private TextMetrics metrics;

    /// <summary>A layout of <paramref name="text"/>, measured once its fonts have loaded.</summary>
    public TextLayout(string text = "")
        : this(new Text(text))
    {
    }

    private protected TextLayout(Text node)
    {
        this.node = node;
    }

    /// <summary>A layout of styled spans.</summary>
    public static TextLayout Rich(params ReadOnlySpan<Span> spans) => new(new RichText(spans));

    /// <summary>Whether the layout's fonts have loaded, so it can be measured.</summary>
    public bool IsReady { get; private set; }

    /// <summary>The width the text needs; throws until <see cref="IsReady"/>.</summary>
    public float MeasuredWidth => Measure().Width;

    /// <summary>The height the text needs; throws until <see cref="IsReady"/>.</summary>
    public float MeasuredHeight => Measure().Height;

    /// <inheritdoc/>
    public virtual string Text
    {
        get => node.Text;
        set
        {
            node.Text = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public FontFamily? FontFamily
    {
        get => node.FontFamily;
        set
        {
            node.FontFamily = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public FontWeight FontWeight
    {
        get => node.FontWeight;
        set
        {
            node.FontWeight = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public FontStyle FontStyle
    {
        get => node.FontStyle;
        set
        {
            node.FontStyle = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public float FontSize
    {
        get => node.FontSize;
        set
        {
            node.FontSize = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public float? LineHeight
    {
        get => node.LineHeight;
        set
        {
            node.LineHeight = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Color? TextColor
    {
        get => node.TextColor;
        set
        {
            node.TextColor = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public TextAlign TextAlign
    {
        get => node.TextAlign;
        set
        {
            node.TextAlign = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public TextVerticalAlign VerticalAlign
    {
        get => node.VerticalAlign;
        set
        {
            node.VerticalAlign = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public int MaxLines
    {
        get => node.MaxLines;
        set
        {
            node.MaxLines = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public bool Wrapping
    {
        get => node.Wrapping;
        set
        {
            node.Wrapping = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public TextOverflow Overflow
    {
        get => node.Overflow;
        set
        {
            node.Overflow = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length Width
    {
        get => node.Width;
        set
        {
            node.Width = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length Height
    {
        get => node.Height;
        set
        {
            node.Height = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public bool FillWidth
    {
        get => node.FillWidth;
        set
        {
            node.FillWidth = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public bool FillHeight
    {
        get => node.FillHeight;
        set
        {
            node.FillHeight = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public float? FillWidthPercent
    {
        get => node.FillWidthPercent;
        set
        {
            node.FillWidthPercent = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public float? FillHeightPercent
    {
        get => node.FillHeightPercent;
        set
        {
            node.FillHeightPercent = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length MinWidth
    {
        get => node.MinWidth;
        set
        {
            node.MinWidth = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length MaxWidth
    {
        get => node.MaxWidth;
        set
        {
            node.MaxWidth = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length MinHeight
    {
        get => node.MinHeight;
        set
        {
            node.MinHeight = value;
            MarkDirty();
        }
    }

    /// <inheritdoc/>
    public Length MaxHeight
    {
        get => node.MaxHeight;
        set
        {
            node.MaxHeight = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Measures with <paramref name="stack"/> at <paramref name="fontSize"/> pixels instead of a family.
    /// </summary>
    public void SetFontStack(FontStack stack, float fontSize)
    {
        node.SetFontStack(stack, fontSize);
        MarkDirty();
    }

    /// <inheritdoc/>
    public void SetTextLimits(int maxChars, int maxLines)
    {
        node.SetTextLimits(maxChars, maxLines);
        MarkDirty();
    }

    /// <summary>The shaped size, baseline and line count.</summary>
    /// <exception cref="InvalidOperationException">The layout is not ready; measure from an <c>OnReady</c> callback.</exception>
    public TextMetrics Measure()
    {
        if (!IsReady)
        {
            throw new InvalidOperationException("The TextLayout is not ready; measure it from an OnReady callback.");
        }
        return metrics;
    }

    /// <summary>Releases the native layout.</summary>
    public void Dispose()
    {
        IsReady = false;
        metrics = default;
        node.Dispose();
    }

    /// <summary>
    /// Runs <paramref name="callback"/> once the page has loaded and the layout's fonts are available, after
    /// preparing it. The fluent <c>OnReady</c> calls this.
    /// </summary>
    internal void WhenReady(Action<TextLayoutReadyEventArgs> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        FrameScheduler.OnLoaded(_ =>
        {
            if (node.IsDisposed)
            {
                return;
            }
            EnsureBuilt();
            FontFace.WhenFontsLoaded(node.RequiredFontIds(), _ =>
            {
                if (node.IsDisposed)
                {
                    return;
                }
                Prepare();
                callback(new TextLayoutReadyEventArgs(this));
            });
        });
    }

    /// <summary>The detached node the host shapes and draws.</summary>
    internal Node DrawNode()
    {
        EnsureBuilt();
        return node;
    }

    private protected ulong EnsureBuilt() => node.Build();

    /// <summary>Shapes the text now and caches its metrics.</summary>
    private protected virtual void Prepare()
    {
        var handle = EnsureBuilt();
        if (Ui.PrepareNode(handle) == 0)
        {
            IsReady = false;
            metrics = default;
            return;
        }
        metrics = Ui.TryGetTextMetrics(handle, out var measured) ? measured : default;
        IsReady = true;
    }

    private void MarkDirty()
    {
        IsReady = false;
        metrics = default;
    }
}
