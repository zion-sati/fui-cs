using System.Text;

using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A retained text node: content, font, color, alignment, wrapping, limits, selection and editing. Unset color,
/// font family and selection color follow the active theme.
/// </summary>
public class TextCore : Node, ISizable, ITextFormat
{
    private readonly NodeSize size;
    private string content;
    private uint fontId;
    private float fontSize;
    private bool hasFont;
    private bool usesDirectFontId;
    private FontFamily? fontFamily;
    private FontStack? fontStack;
    private FontWeight fontWeight = FontWeight.Regular;
    private FontStyle fontStyle = FontStyle.Normal;
    private float? lineHeight;
    private Color? textColor;
    private TextAlign textAlign = TextAlign.Left;
    private bool hasTextAlign;
    private TextVerticalAlign verticalAlign = TextVerticalAlign.Top;
    private bool hasVerticalAlign;
    private int maxChars = int.MaxValue;
    private int maxLines;
    private bool hasLimits;
    private bool wrapping = true;
    private bool hasWrapping;
    private TextOverflow overflow = TextOverflow.Clip;
    private bool hasOverflow;
    private (bool Horizontal, bool Vertical)? overflowFade;
    private bool? obscured;
    private bool? editable;
    private bool? editorCommandKeys;
    private bool? editorAcceptsTab;
    private Color? caretColor;
    private bool? selectable;
    private Color? selectionColor;
    private uint[]? styleRuns;

    /// <summary>A text node showing <paramref name="text"/>.</summary>
    public TextCore(string text = "")
    {
        size = new NodeSize(this);
        content = text;
        Track(Theme.Active.Subscribe(OnThemeChanged));
    }

    /// <summary>Raised when the user edits the text.</summary>
    public event Action<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the selection changes, with UTF-8 offsets.</summary>
    public event Action<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>The text. The host updates it as the user edits editable text.</summary>
    public virtual string Text
    {
        get => content;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            content = value;
            if (IsBuilt)
            {
                Ui.SetText(Handle, value);
                NotifyRetainedMutation();
            }
            OnContentChanged();
        }
    }

    /// <summary>The theme's body family when null; setting it replaces a <see cref="FontStack"/>.</summary>
    public virtual FontFamily? FontFamily
    {
        get => fontFamily;
        set
        {
            fontFamily = value;
            fontStack = null;
            usesDirectFontId = false;
            if (IsBuilt && hasFont)
            {
                ApplyResolvedFont();
            }
        }
    }

    /// <summary>A single stack to render with instead of a family; set it with its size through <see cref="SetFontStack"/>.</summary>
    public FontStack? FontStack => fontStack;

    /// <inheritdoc/>
    public virtual FontWeight FontWeight
    {
        get => fontWeight;
        set
        {
            fontWeight = value;
            if (IsBuilt && hasFont)
            {
                ApplyResolvedFont();
            }
        }
    }

    /// <inheritdoc/>
    public virtual FontStyle FontStyle
    {
        get => fontStyle;
        set
        {
            fontStyle = value;
            if (IsBuilt && hasFont)
            {
                ApplyResolvedFont();
            }
        }
    }

    /// <summary>The font size in pixels; 0 leaves the host default until set.</summary>
    public virtual float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            hasFont = true;
            if (IsBuilt)
            {
                ApplyResolvedFont();
            }
        }
    }

    /// <summary>The line height in pixels; null uses the font's own.</summary>
    public float? LineHeight
    {
        get => lineHeight;
        set
        {
            lineHeight = value;
            if (IsBuilt && value is { } pixels)
            {
                Ui.SetLineHeight(Handle, pixels);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The text color; null follows the theme's primary text color.</summary>
    public virtual Color? TextColor
    {
        get => textColor;
        set
        {
            textColor = value;
            if (IsBuilt)
            {
                Ui.SetTextColor(Handle, (value ?? Theme.Active.Value.Colors.TextPrimary).Rgba);
                NotifyRetainedMutation();
            }
        }
    }

    /// <inheritdoc/>
    public TextAlign TextAlign
    {
        get => textAlign;
        set
        {
            textAlign = value;
            hasTextAlign = true;
            if (IsBuilt)
            {
                Ui.SetTextAlign(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <inheritdoc/>
    public TextVerticalAlign VerticalAlign
    {
        get => verticalAlign;
        set
        {
            verticalAlign = value;
            hasVerticalAlign = true;
            if (IsBuilt)
            {
                Ui.SetTextVerticalAlign(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The maximum number of lines; 0 means no limit.</summary>
    public int MaxLines
    {
        get => maxLines;
        set => SetTextLimits(int.MaxValue, value);
    }

    /// <summary>The most characters the text may hold.</summary>
    public int MaxChars => maxChars;

    /// <summary>Wraps lines at the available width. On by default.</summary>
    public bool Wrapping
    {
        get => wrapping;
        set
        {
            wrapping = value;
            hasWrapping = true;
            if (IsBuilt)
            {
                Ui.SetTextWrapping(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <inheritdoc/>
    public TextOverflow Overflow
    {
        get => overflow;
        set
        {
            overflow = value;
            hasOverflow = true;
            if (IsBuilt)
            {
                Ui.SetTextOverflow(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Renders the text as bullets, for passwords.</summary>
    public bool Obscured
    {
        get => obscured ?? false;
        set
        {
            obscured = value;
            if (IsBuilt)
            {
                Ui.SetTextObscured(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Lets the user edit the text; editable text is selectable unless selection was set explicitly.</summary>
    public bool Editable
    {
        get => editable ?? false;
        set
        {
            editable = value;
            if (value && selectable is null)
            {
                SetSelectable(true);
            }
            if (IsBuilt)
            {
                Ui.SetEditable(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The caret's color while editing; null follows the theme's accent.</summary>
    public Color? CaretColor
    {
        get => caretColor;
        set
        {
            caretColor = value;
            if (IsBuilt && value is { } color)
            {
                Ui.SetCaretColor(Handle, color.Rgba);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Lets the user select the text, with the I-beam cursor.</summary>
    public bool Selectable
    {
        get => selectable ?? false;
        set => SetSelectable(value);
    }

    /// <summary>The selection highlight; null follows the theme's selection color.</summary>
    public Color? SelectionColor
    {
        get => selectionColor;
        set
        {
            selectionColor = value;
            if (IsBuilt && selectable is { } isSelectable)
            {
                Ui.SetSelectable(Handle, isSelectable, ResolvedSelectionColor.Rgba);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Where the selection starts, in UTF-8 offsets.</summary>
    public uint SelectionStart { get; private set; }

    /// <summary>Where the selection ends, in UTF-8 offsets.</summary>
    public uint SelectionEnd { get; private set; }

    /// <summary>The selected text; the selection's UTF-8 offsets are converted to the string's.</summary>
    internal string SelectedText => TextBetween(SelectionStart, SelectionEnd);

    /// <summary>The text between two UTF-8 offsets, in either order.</summary>
    internal string TextBetween(uint utf8Start, uint utf8End)
    {
        var start = TextOffsets.StringOffsetOfUtf8Offset(content, Math.Min(utf8Start, utf8End));
        var end = TextOffsets.StringOffsetOfUtf8Offset(content, Math.Max(utf8Start, utf8End));
        return content[start..end];
    }

    /// <inheritdoc/>
    public override bool IsEditableText => editable ?? false;

    /// <inheritdoc/>
    public override bool IsSelectableText => selectable ?? false;

    /// <summary>Whether selection was never set explicitly, so the node keeps its type's default.</summary>
    internal bool UsesDefaultSelectionBehavior => selectable is null;

    internal bool EditorCommandKeys
    {
        get => editorCommandKeys ?? false;
        set
        {
            editorCommandKeys = value;
            if (IsBuilt)
            {
                Ui.SetEditorCommandKeys(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    internal bool EditorAcceptsTab
    {
        get => editorAcceptsTab ?? false;
        set
        {
            editorAcceptsTab = value;
            if (IsBuilt)
            {
                Ui.SetEditorAcceptsTab(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <inheritdoc/>
    public Length Width
    {
        get => size.Width;
        set => size.Width = value;
    }

    /// <inheritdoc/>
    public Length Height
    {
        get => size.Height;
        set => size.Height = value;
    }

    /// <inheritdoc/>
    public bool FillWidth
    {
        get => size.FillWidth;
        set => size.FillWidth = value;
    }

    /// <inheritdoc/>
    public bool FillHeight
    {
        get => size.FillHeight;
        set => size.FillHeight = value;
    }

    /// <inheritdoc/>
    public float? FillWidthPercent
    {
        get => size.FillWidthPercent;
        set => size.FillWidthPercent = value;
    }

    /// <inheritdoc/>
    public float? FillHeightPercent
    {
        get => size.FillHeightPercent;
        set => size.FillHeightPercent = value;
    }

    /// <inheritdoc/>
    public Length MinWidth
    {
        get => size.MinWidth;
        set => size.MinWidth = value;
    }

    /// <inheritdoc/>
    public Length MaxWidth
    {
        get => size.MaxWidth;
        set => size.MaxWidth = value;
    }

    /// <inheritdoc/>
    public Length MinHeight
    {
        get => size.MinHeight;
        set => size.MinHeight = value;
    }

    /// <inheritdoc/>
    public Length MaxHeight
    {
        get => size.MaxHeight;
        set => size.MaxHeight = value;
    }

    /// <summary>Renders with <paramref name="stack"/> at <paramref name="fontSize"/> instead of a family.</summary>
    public virtual void SetFontStack(FontStack stack, float fontSize)
    {
        ArgumentNullException.ThrowIfNull(stack);
        SetFontId(stack.Id, fontSize);
        fontStack = stack;
    }

    /// <summary>Limits the text to <paramref name="maxChars"/> characters and <paramref name="maxLines"/> lines (0 for no line limit).</summary>
    public void SetTextLimits(int maxChars, int maxLines)
    {
        this.maxChars = maxChars;
        this.maxLines = maxLines;
        hasLimits = true;
        if (IsBuilt)
        {
            Ui.SetTextLimits(Handle, maxChars, maxLines);
            NotifyRetainedMutation();
        }
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        Handle = Ui.CreateNode(NodeType.Text);
        ApplyNodeMetadata();
        FinishBuild();
        size.Apply(Handle);
        Ui.SetText(Handle, content);
        if (styleRuns is not null)
        {
            Ui.SetTextStyleRuns(Handle, styleRuns);
        }
        if (hasFont)
        {
            Ui.SetFont(Handle, ResolveFontId(), fontSize);
        }
        if (lineHeight is { } pixels)
        {
            Ui.SetLineHeight(Handle, pixels);
        }
        Ui.SetTextColor(Handle, (textColor ?? Theme.Active.Value.Colors.TextPrimary).Rgba);
        if (hasTextAlign)
        {
            Ui.SetTextAlign(Handle, textAlign);
        }
        if (hasVerticalAlign)
        {
            Ui.SetTextVerticalAlign(Handle, verticalAlign);
        }
        if (hasLimits)
        {
            Ui.SetTextLimits(Handle, maxChars, maxLines);
        }
        if (hasWrapping)
        {
            Ui.SetTextWrapping(Handle, wrapping);
        }
        if (hasOverflow)
        {
            Ui.SetTextOverflow(Handle, overflow);
        }
        if (overflowFade is { } fade)
        {
            Ui.SetTextOverflowFade(Handle, fade.Horizontal, fade.Vertical);
        }
        if (obscured is { } isObscured)
        {
            Ui.SetTextObscured(Handle, isObscured);
        }
        if (editable is { } isEditable)
        {
            Ui.SetEditable(Handle, isEditable);
        }
        if (editorCommandKeys is { } commandKeys)
        {
            Ui.SetEditorCommandKeys(Handle, commandKeys);
        }
        if (editorAcceptsTab is { } acceptsTab)
        {
            Ui.SetEditorAcceptsTab(Handle, acceptsTab);
        }
        if (selectable is { } isSelectable)
        {
            Ui.SetSelectable(Handle, isSelectable, ResolvedSelectionColor.Rgba);
        }
        if (caretColor is { } caret)
        {
            Ui.SetCaretColor(Handle, caret.Rgba);
        }
        return Handle;
    }

    internal override float DebugMainAxisPercentValue(bool isHorizontal) => size.MainAxisPercent(isHorizontal);

    internal override IReadOnlyList<uint> RequiredFontIds()
    {
        var resolved = ResolveFontId();
        return resolved != 0 ? [resolved] : [];
    }

    /// <summary>Renders with the font <paramref name="id"/> at <paramref name="fontSize"/>, bypassing family resolution.</summary>
    internal void SetFontId(uint id, float fontSize)
    {
        fontFamily = null;
        fontStack = null;
        fontId = id;
        this.fontSize = fontSize;
        hasFont = true;
        usesDirectFontId = true;
        if (id == 0)
        {
            Logger.Warn("Typography", "Text received font id 0; the text will render with the default font fallback.");
        }
        if (IsBuilt)
        {
            ApplyResolvedFont();
        }
    }

    /// <summary>Called after the text changes, from code or by the user.</summary>
    protected virtual void OnContentChanged()
    {
    }

    /// <summary>Fades text that overflows the box horizontally and/or vertically.</summary>
    protected void ApplyOverflowFade(bool horizontal, bool vertical)
    {
        overflowFade = (horizontal, vertical);
        if (IsBuilt)
        {
            Ui.SetTextOverflowFade(Handle, horizontal, vertical);
            NotifyRetainedMutation();
        }
    }

    /// <summary>Sets the styled runs, seven words each: start and end UTF-8 offsets, font id, size bits, color, background, decorations.</summary>
    protected void SetTextStyleRuns(uint[]? runs)
    {
        styleRuns = runs;
        if (IsBuilt)
        {
            Ui.SetTextStyleRuns(Handle, runs ?? []);
            NotifyRetainedMutation();
        }
    }

    /// <inheritdoc/>
    protected internal override void HandleTextChanged(string text)
    {
        content = text;
        TextChanged?.Invoke(new TextChangedEventArgs(text));
        OnContentChanged();
    }

    /// <inheritdoc/>
    protected internal override void HandleTextReplaced(uint utf8Start, uint utf8End, string text)
    {
        var startOffset = TextOffsets.StringOffsetOfUtf8Offset(content, utf8Start);
        var endOffset = TextOffsets.StringOffsetOfUtf8Offset(content, Math.Max(utf8Start, utf8End));
        content = string.Concat(content.AsSpan(0, startOffset), text, content.AsSpan(endOffset));
        TextChanged?.Invoke(new TextChangedEventArgs(content));
        OnContentChanged();
    }

    /// <inheritdoc/>
    protected internal override void HandleSelectionChanged(uint utf8Start, uint utf8End)
    {
        SelectionStart = utf8Start;
        SelectionEnd = utf8End;
        SelectionChanged?.Invoke(new SelectionChangedEventArgs(utf8Start, utf8End));
    }

    private Color ResolvedSelectionColor => selectionColor ?? Theme.Active.Value.Colors.Selection;

    /// <summary>Whether the cursor is the I-beam that being selectable gave it, which an ancestor's own cursor overrides.</summary>
    internal bool HasAutomaticCursor => IsSelectableText && Cursor == CursorStyle.Text;

    private void SetSelectable(bool flag)
    {
        selectable = flag;
        if (flag)
        {
            Cursor = CursorStyle.Text;
        }
        else if (Cursor == CursorStyle.Text)
        {
            Cursor = CursorStyle.Default;
        }
        if (IsBuilt)
        {
            Ui.SetSelectable(Handle, flag, ResolvedSelectionColor.Rgba);
            NotifyRetainedMutation();
        }
    }

    private uint ResolveFontId()
    {
        if (usesDirectFontId)
        {
            return fontId;
        }
        var family = fontFamily ?? Theme.Active.Value.Fonts.BodyFamily;
        fontId = family.Resolve(fontWeight, fontStyle);
        return fontId;
    }

    private void ApplyResolvedFont()
    {
        var resolved = ResolveFontId();
        if (resolved == 0)
        {
            Logger.Warn("Typography", "Text resolved font id 0 while applying font settings; check the configured FontFamily or font id.");
        }
        Ui.SetFont(Handle, resolved, fontSize);
        NotifyRetainedMutation();
    }

    /// <summary>Applies <paramref name="theme"/> to the theme-derived color, font and selection color. Overrides should call the base method.</summary>
    protected virtual void OnThemeChanged(Theme theme)
    {
        if (IsBuilt && textColor is null)
        {
            Ui.SetTextColor(Handle, theme.Colors.TextPrimary.Rgba);
            NotifyRetainedMutation();
        }
        if (IsBuilt && hasFont && !usesDirectFontId && fontFamily is null)
        {
            ApplyResolvedFont();
        }
        if (IsBuilt && selectable is { } isSelectable && selectionColor is null)
        {
            Ui.SetSelectable(Handle, isSelectable, theme.Colors.Selection.Rgba);
            NotifyRetainedMutation();
        }
    }
}
