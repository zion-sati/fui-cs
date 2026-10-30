using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// The editable text control that <see cref="TextInput"/> and <see cref="TextArea"/> build on: a shell styled by its
/// <see cref="TextInputTemplate"/> around an editable text node, with a placeholder while the text is empty.
/// Selection positions count characters (Unicode scalar values); the <c>ByteOffset</c> members give the same
/// positions in UTF-8 bytes, as the runtime counts them. Its text persists across history navigation, unless it is a
/// password.
/// </summary>
public abstract class TextInputCore : FlexBox
{
    private static readonly PersistedValue<TextInputCore, string> PersistedText = new(
        "text-input-value",
        input => input.text,
        (input, value) => input.RestoreText(value),
        PersistedCodecs.Text)
    {
        ShouldCapture = input => !input.isPassword,
    };

    private readonly TextInputProfile profile;
    private readonly TextInputEditor editor;
    private readonly TextCore placeholderText;
    private readonly TextInputPlaceholderHost placeholderHost;
    private readonly ScrollBox? editorScrollBox;
    private TextInputPresenter presenter;
    private TextInputTemplate? template;
    private TextInputColors? colors;
    private string text;
    private string placeholder = string.Empty;
    private int selectionStart;
    private int selectionEnd;
    private uint selectionStartByte;
    private uint selectionEndByte;
    private bool focused;
    private bool isReadOnly;
    private bool isPassword;
    private bool acceptsTab;
    private string? hostAutofill;
    private int? maxChars;
    private bool wrapping;
    private ScrollBarVisibility verticalScrollBarVisibility = ScrollBarVisibility.Auto;
    private ScrollBarVisibility horizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    private FontFamily? fontFamily;
    private float? fontSize;
    private bool placeholderAttached;
    private bool disposed;

    private protected TextInputCore(TextInputProfile profile, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        this.profile = profile;
        this.text = text;
        selectionStart = selectionEnd = TextOffsets.CharacterCount(text);
        selectionStartByte = selectionEndByte = TextOffsets.Utf8Length(text);
        wrapping = profile.WrapsByDefault;
        editor = new TextInputEditor(this, text);
        placeholderText = new TextCore(string.Empty);
        placeholderHost = new TextInputPlaceholderHost(this, placeholderText);
        if (profile.IsMultiline)
        {
            editorScrollBox = new ScrollBox(new ScrollState(), new TextInputEditorViewport(this)).FillSize().Child(editor);
            AddChild(editorScrollBox);
        }
        else
        {
            AddChild(editor);
        }
        presenter = TextInputTemplate.Default.CreatePresenter();
        presenter.Bind(this, EditorHost, placeholderHost);
        // A field clips its text and placeholder, as a browser's input does; children never extend its chrome.
        ClipToBounds = true;
        Track(Theme.Active.Subscribe(SyncThemeState));
        Track(FocusVisibility.KeyboardFocusVisible.Subscribe(_ => SyncFocusChrome(Theme.Active.Value)));
        RequireInteractive();
        SyncSemanticLabel();
        SyncEditorLimits();
        SyncEditorEditability();
        SyncBrowserInputMetadata();
        SyncThemeState(Theme.Active.Value);
        SyncPlaceholderVisibility();
        RegisterPersistedState(PersistedText);
    }

    /// <summary>Raised when the user edits the text, or a restored value differs; not when code sets <see cref="Text"/>.</summary>
    public event Action<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the user moves the caret or selection, with character positions.</summary>
    public event Action<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>A text input contains selection: page selection does not extend into or out of it.</summary>
    public override bool IsSelectionBarrier => true;

    /// <summary>Whether the editor has keyboard focus.</summary>
    public override bool IsFocused => focused;

    /// <summary>The text. Setting it moves the caret to the end and does not raise <see cref="TextChanged"/>.</summary>
    public string Text
    {
        get => text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            text = value;
            editor.Text = value;
            SetSelection(TextOffsets.CharacterCount(value), TextOffsets.CharacterCount(value));
            SyncSemanticLabel();
            SyncPlaceholderVisibility();
        }
    }

    /// <summary>The selection's start, in characters.</summary>
    public int SelectionStart => selectionStart;

    /// <summary>The selection's end, in characters; it equals the start for a caret.</summary>
    public int SelectionEnd => selectionEnd;

    /// <summary>The selection's start in UTF-8 bytes, as the runtime counts.</summary>
    public uint SelectionStartByteOffset => selectionStartByte;

    /// <summary>The selection's end in UTF-8 bytes, as the runtime counts.</summary>
    public uint SelectionEndByteOffset => selectionEndByte;

    /// <summary>Shown in the muted color while the text is empty; it is also the default accessible label.</summary>
    public string Placeholder
    {
        get => placeholder;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            placeholder = value;
            placeholderText.Text = value;
            SyncSemanticLabel();
            SyncPlaceholderVisibility();
        }
    }

    /// <summary>The most characters the user can enter; null for no limit.</summary>
    public int? MaxChars
    {
        get => maxChars;
        set
        {
            if (value is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The limit must not be negative, or null for no limit.");
            }
            maxChars = value;
            SyncEditorLimits();
        }
    }

    /// <summary>Lets the user select and copy the text but not change it.</summary>
    public bool IsReadOnly
    {
        get => isReadOnly;
        set
        {
            isReadOnly = value;
            SyncEditorEditability();
        }
    }

    /// <summary>Shows the text as bullets and marks the field as a password for the browser and password managers.</summary>
    public bool IsPassword
    {
        get => isPassword;
        set
        {
            isPassword = value;
            editor.Obscured = value;
            SyncBrowserInputMetadata();
            SyncSemanticLabel();
        }
    }

    /// <summary>Tab inserts a tab character instead of moving focus.</summary>
    public bool AcceptsTab
    {
        get => acceptsTab;
        set
        {
            acceptsTab = value;
            editor.EditorAcceptsTab = value;
        }
    }

    /// <summary>The browser autofill hint (such as <c>username</c> or <c>current-password</c>); null or empty for none.</summary>
    public string? HostAutofill
    {
        get => hostAutofill;
        set
        {
            hostAutofill = string.IsNullOrEmpty(value) ? null : value;
            SyncBrowserInputMetadata();
        }
    }

    /// <summary>The editor's accessible label, which otherwise follows the placeholder, a read-only value or the control's kind.</summary>
    public override string? SemanticLabel
    {
        get => editor.SemanticLabel;
        set => editor.SemanticLabel = value;
    }

    /// <summary>The theme's body family when null.</summary>
    public FontFamily? FontFamily
    {
        get => fontFamily;
        set
        {
            fontFamily = value;
            SyncThemeState(Theme.Active.Value);
        }
    }

    /// <summary>The theme's body size when null.</summary>
    public float? FontSize
    {
        get => fontSize;
        set
        {
            if (value is <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The font size must be positive, or null for the theme's.");
            }
            fontSize = value;
            SyncThemeState(Theme.Active.Value);
        }
    }

    /// <summary>The editor's and placeholder's line height; null follows the font.</summary>
    public float? LineHeight
    {
        get => editor.LineHeight;
        set
        {
            editor.LineHeight = value;
            placeholderText.LineHeight = value;
        }
    }

    /// <summary>Builds the shell's look; null uses <see cref="TextInputTemplate.Default"/>.</summary>
    public TextInputTemplate? Template
    {
        get => template;
        set
        {
            template = value;
            presenter = (value ?? TextInputTemplate.Default).CreatePresenter();
            presenter.Bind(this, EditorHost, placeholderHost);
            SyncThemeState(Theme.Active.Value);
        }
    }

    /// <summary>Colors over the theme's for the shell, text, placeholder and caret.</summary>
    public TextInputColors? Colors
    {
        get => colors;
        set
        {
            colors = value;
            SyncThemeState(Theme.Active.Value);
        }
    }

    /// <summary>The editable text node inside the shell.</summary>
    public TextCore Editor => editor;

    internal override TextCore? InnerEditor => editor;

    /// <summary>
    /// Answers the editor's key presses in place of the input's own handling, for a control built around the input
    /// (a combo box). Keys it returns false for stay unhandled, so Tab still moves focus.
    /// </summary>
    internal Func<string, KeyModifier, bool>? KeyDownOverride { get; set; }

    private protected bool IsMultiline => profile.IsMultiline;

    private protected ScrollBox? EditorScrollBox => editorScrollBox;

    /// <summary>Wraps lines at the editor's width; a text area wraps by default and scrolls sideways when it does not.</summary>
    public bool Wrapping
    {
        get => wrapping;
        set
        {
            if (wrapping == value)
            {
                return;
            }
            wrapping = value;
            SyncEditorWrapping();
            SyncScrollChromeState();
            SyncThemeState(Theme.Active.Value);
        }
    }

    /// <summary>A text area's vertical scroll bar.</summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => verticalScrollBarVisibility;
        set
        {
            verticalScrollBarVisibility = value;
            SyncScrollChromeState();
        }
    }

    /// <summary>A text area's horizontal scroll bar, hidden while it wraps.</summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => horizontalScrollBarVisibility;
        set
        {
            horizontalScrollBarVisibility = value;
            SyncScrollChromeState();
        }
    }

    private Node EditorHost => editorScrollBox is null ? editor : editorScrollBox;

    /// <summary>Selects from <paramref name="start"/> to <paramref name="end"/> characters; positions past the end clamp to it.</summary>
    public void SetSelection(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(end);
        var length = TextOffsets.CharacterCount(text);
        selectionStart = Math.Min(start, length);
        selectionEnd = Math.Min(end, length);
        selectionStartByte = TextOffsets.Utf8OffsetOfCharacter(text, selectionStart);
        selectionEndByte = TextOffsets.Utf8OffsetOfCharacter(text, selectionEnd);
        if (editor.IsBuilt)
        {
            Ui.SetTextSelectionRange(editor.Handle, selectionStartByte, selectionEndByte);
        }
    }

    /// <summary>Puts the caret at a character position, clamped to the end.</summary>
    public void SetCaret(int position) => SetSelection(position, position);

    /// <summary>Puts the caret after the last character.</summary>
    public void CaretToEnd() => SetCaret(TextOffsets.CharacterCount(text));

    /// <summary>Focuses the editor and restores the selection the control holds.</summary>
    public override void Focus()
    {
        FocusEditor();
        SetByteSelection(selectionStartByte, selectionEndByte);
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        var intendedStart = selectionStartByte;
        var intendedEnd = selectionEndByte;
        var handle = base.Build();
        Ui.SetSelectionAreaBarrier(handle, true);
        SyncBrowserInputMetadata();
        SetByteSelection(intendedStart, intendedEnd);
        return handle;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            FocusAdorner.Hide(this);
        }
        if (!placeholderAttached)
        {
            placeholderHost.Dispose();
        }
        base.Dispose();
    }

    /// <inheritdoc/>
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        if (e.EventType == PointerEventType.Down && IsEnabled)
        {
            HandleShellPointerDown();
        }
    }

    /// <inheritdoc/>
    protected override void OnEffectiveEnabledChanged(bool isEnabled)
    {
        base.OnEffectiveEnabledChanged(isEnabled);
        SyncEditorEditability();
        SyncThemeState(Theme.Active.Value);
        SyncPlaceholderVisibility();
    }

    internal void HandleEditorFocusChanged(bool isFocused)
    {
        if (focused == isFocused)
        {
            return;
        }
        focused = isFocused;
        SyncFocusChrome(Theme.Active.Value);
        SyncPlaceholderVisibility();
        RaiseFocusChanged(new FocusChangedEventArgs(isFocused));
    }

    internal void HandleEditorTextChanged(string value)
    {
        if (text == value)
        {
            return;
        }
        text = value;
        ClampSelectionToText();
        SyncSemanticLabel();
        SyncPlaceholderVisibility();
        TextChanged?.Invoke(new TextChangedEventArgs(value));
    }

    internal void HandleEditorSelectionChanged(uint start, uint end)
    {
        selectionStartByte = start;
        selectionEndByte = end;
        selectionStart = TextOffsets.CharacterOfUtf8Offset(text, start);
        selectionEnd = TextOffsets.CharacterOfUtf8Offset(text, end);
        RaiseSelectionChanged();
    }

    /// <summary>
    /// Consumes the editor's navigation keys, so they do not reach the page, and inserts a tab when the input accepts
    /// tabs. Other keys, such as browser shortcuts, pass through.
    /// </summary>
    internal bool HandleEditorKeyDown(string key, KeyModifier modifiers)
    {
        if (!IsEnabled)
        {
            return false;
        }
        if (KeyDownOverride is { } handler)
        {
            return handler(key, modifiers);
        }
        var nonShift = modifiers & (KeyModifier.Ctrl | KeyModifier.Alt | KeyModifier.Meta);
        if (nonShift == KeyModifier.None && key is "ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown" or "Home" or "End" or "PageUp" or "PageDown")
        {
            return true;
        }
        if (acceptsTab && key == "Tab" && modifiers == KeyModifier.None && !isReadOnly)
        {
            ReplaceSelection("\t");
            return true;
        }
        return false;
    }

    internal void HandleShellPointerDown()
    {
        if (!IsEnabled)
        {
            return;
        }
        if (profile.IsMultiline)
        {
            HandleViewportPointerDown();
        }
        else if (focused)
        {
            FocusEditor();
        }
        else
        {
            Focus();
        }
    }

    internal void HandleViewportPointerDown()
    {
        if (!IsEnabled)
        {
            return;
        }
        if (focused)
        {
            FocusEditor();
        }
        else
        {
            Focus();
        }
    }

    private void RestoreText(string value)
    {
        if (text == value)
        {
            return;
        }
        Text = value;
        TextChanged?.Invoke(new TextChangedEventArgs(value));
    }

    private void ReplaceSelection(string inserted)
    {
        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);
        var insertedLength = TextOffsets.CharacterCount(inserted);
        if (maxChars is { } limit && TextOffsets.CharacterCount(text) - (end - start) + insertedLength > limit)
        {
            return;
        }
        var previous = text;
        var value = string.Concat(
            previous.AsSpan(0, TextOffsets.StringOffsetOfCharacter(previous, start)),
            inserted,
            previous.AsSpan(TextOffsets.StringOffsetOfCharacter(previous, end)));
        var caret = start + insertedLength;
        text = value;
        if (editor.IsBuilt)
        {
            var caretByte = TextOffsets.Utf8OffsetOfCharacter(value, caret);
            Ui.ReplaceTextRange(
                editor.Handle,
                TextOffsets.Utf8OffsetOfCharacter(previous, start),
                TextOffsets.Utf8OffsetOfCharacter(previous, end),
                inserted,
                caretByte);
            selectionStart = selectionEnd = caret;
            selectionStartByte = selectionEndByte = caretByte;
        }
        else
        {
            editor.Text = value;
            SetSelection(caret, caret);
        }
        SyncSemanticLabel();
        SyncPlaceholderVisibility();
        TextChanged?.Invoke(new TextChangedEventArgs(value));
        RaiseSelectionChanged();
    }

    private void RaiseSelectionChanged() => SelectionChanged?.Invoke(new SelectionChangedEventArgs((uint)selectionStart, (uint)selectionEnd));

    private void SetByteSelection(uint start, uint end)
    {
        var length = TextOffsets.Utf8Length(text);
        selectionStartByte = Math.Min(start, length);
        selectionEndByte = Math.Min(end, length);
        selectionStart = TextOffsets.CharacterOfUtf8Offset(text, selectionStartByte);
        selectionEnd = TextOffsets.CharacterOfUtf8Offset(text, selectionEndByte);
        if (editor.IsBuilt)
        {
            Ui.SetTextSelectionRange(editor.Handle, selectionStartByte, selectionEndByte);
        }
    }

    private void ClampSelectionToText()
    {
        var length = TextOffsets.CharacterCount(text);
        selectionStart = Math.Min(selectionStart, length);
        selectionEnd = Math.Min(selectionEnd, length);
        selectionStartByte = TextOffsets.Utf8OffsetOfCharacter(text, selectionStart);
        selectionEndByte = TextOffsets.Utf8OffsetOfCharacter(text, selectionEnd);
    }

    private void FocusEditor()
    {
        if (IsEnabled)
        {
            editor.Focus();
        }
    }

    private void SyncThemeState(Theme theme)
    {
        if (disposed)
        {
            return;
        }
        var family = fontFamily ?? theme.Fonts.BodyFamily;
        var size = fontSize ?? theme.Fonts.SizeBody;
        var lineHeight = size + theme.Spacing.Sm;
        var editableCursor = IsEnabled ? CursorStyle.Text : CursorStyle.Default;
        var multiline = profile.IsMultiline;
        presenter.Apply(theme, new TextInputVisualState(multiline, IsEnabled, wrapping), colors);
        var verticalAlign = multiline ? TextVerticalAlign.Top : TextVerticalAlign.Center;
        var rowHeight = multiline ? Length.Auto : Length.Pixels(lineHeight);

        editor.Width = TracksViewportWidth ? Length.Percent(100.0f) : Length.Auto;
        editor.Height = rowHeight;
        editor.FontFamily = family;
        editor.FontSize = size;
        editor.VerticalAlign = verticalAlign;
        editor.TextColor = IsEnabled ? colors?.TextPrimary ?? theme.Colors.TextPrimary : colors?.TextMuted ?? theme.Colors.TextMuted;
        editor.CaretColor = colors?.Caret ?? theme.Colors.Accent;
        SyncEditorWrapping();

        placeholderHost.Cursor = editableCursor;
        placeholderText.Width = Length.Percent(100.0f);
        placeholderText.Height = rowHeight;
        placeholderText.FontFamily = family;
        placeholderText.FontSize = size;
        placeholderText.VerticalAlign = verticalAlign;
        placeholderText.TextColor = colors?.Placeholder ?? theme.Colors.TextMuted;
        if (editorScrollBox is { } scrollBox)
        {
            scrollBox.Cursor = CursorStyle.Default;
            scrollBox.Viewport.Cursor = editableCursor;
            scrollBox.FillSize();
            SyncScrollChromeState();
        }

        // A custom template may leave the shell alone; explicit colors still apply.
        if (colors?.Background is { } background)
        {
            Background = background;
        }
        if (colors?.Border is { } border)
        {
            Border = new Border(1.0f, border);
        }
        SyncFocusChrome(theme);
    }

    private bool TracksViewportWidth => !profile.IsMultiline || wrapping;

    private void SyncEditorLimits() => editor.SetTextLimits(maxChars ?? int.MaxValue, profile.MaxLines);

    private void SyncEditorEditability()
    {
        editor.Selectable = IsEnabled;
        editor.Editable = !isReadOnly && IsEnabled;
    }

    private void SyncBrowserInputMetadata()
    {
        if (editor.IsBuilt)
        {
            Ui.RegisterTextInputMetadata(editor.Handle, isPassword, hostAutofill);
        }
    }

    private void SyncSemanticLabel()
    {
        if (placeholder.Length > 0)
        {
            editor.SetDefaultSemanticLabel(placeholder);
        }
        else if (isReadOnly && text.Length > 0)
        {
            editor.SetDefaultSemanticLabel(text);
        }
        else
        {
            editor.SetDefaultSemanticLabel(isPassword ? "Password input" : profile.DefaultSemanticLabel);
        }
    }

    private void SyncPlaceholderVisibility()
    {
        var show = text.Length == 0 && placeholder.Length > 0;
        placeholderText.Text = show ? placeholder : string.Empty;
        if (show && !placeholderAttached)
        {
            placeholderAttached = true;
            AddChild(placeholderHost);
        }
        else if (!show && placeholderAttached)
        {
            placeholderAttached = false;
            RemoveChild(placeholderHost);
        }
    }

    private void SyncEditorWrapping()
    {
        editor.Wrapping = wrapping;
        placeholderText.Wrapping = wrapping;
        if (profile.IsMultiline)
        {
            editor.Width = TracksViewportWidth ? Length.Percent(100.0f) : Length.Auto;
        }
    }

    private void SyncScrollChromeState()
    {
        if (editorScrollBox is not { } scrollBox)
        {
            return;
        }
        scrollBox.IsHorizontalScrollEnabled = !wrapping;
        scrollBox.IsVerticalScrollEnabled = true;
        scrollBox.VerticalScrollBarVisibility = verticalScrollBarVisibility;
        scrollBox.HorizontalScrollBarVisibility = wrapping ? ScrollBarVisibility.Hidden : horizontalScrollBarVisibility;
    }

    private void SyncFocusChrome(Theme theme)
    {
        if (focused && IsEnabled && FocusVisibility.KeyboardFocusVisible.Value)
        {
            FocusAdorner.Show(this, new CornerRadius(theme.Spacing.Sm));
        }
        else
        {
            FocusAdorner.Hide(this);
        }
    }
}
