namespace FuiCs;

/// <summary>How a text input edits: its line limit, whether it is a multi-line area, and its default wrapping and label.</summary>
internal sealed record TextInputProfile(int MaxLines, bool IsMultiline, bool WrapsByDefault, string DefaultSemanticLabel)
{
    public static TextInputProfile SingleLine { get; } = new(1, false, false, "Text input");

    public static TextInputProfile MultilineArea { get; } = new(0, true, true, "Text area");
}

/// <summary>The editable text of a text input, which reports its focus, edits, selection and keys to the control.</summary>
internal sealed class TextInputEditor : TextCore
{
    private readonly TextInputCore owner;

    public TextInputEditor(TextInputCore owner, string text)
        : base(text)
    {
        this.owner = owner;
        SemanticRole = FuiCs.SemanticRole.Textbox;
        SetFocusable(true, 0);
        ReflectSemanticDisabledFromEnabled();
        Selectable = true;
        Editable = true;
    }

    /// <summary>The control the editor belongs to, which tracks its selection in characters and UTF-8 offsets.</summary>
    public TextInputCore Owner => owner;

    public void SetDefaultSemanticLabel(string label) => DefaultSemanticLabel = label;

    protected internal override void HandleFocusChanged(bool focused)
    {
        base.HandleFocusChanged(focused);
        owner.HandleEditorFocusChanged(focused);
    }

    protected internal override void HandleTextChanged(string text)
    {
        base.HandleTextChanged(text);
        owner.HandleEditorTextChanged(text);
    }

    protected internal override void HandleTextReplaced(uint utf8Start, uint utf8End, string text)
    {
        base.HandleTextReplaced(utf8Start, utf8End, text);
        owner.HandleEditorTextChanged(Text);
    }

    protected internal override void HandleSelectionChanged(uint utf8Start, uint utf8End)
    {
        base.HandleSelectionChanged(utf8Start, utf8End);
        owner.HandleEditorSelectionChanged(utf8Start, utf8End);
    }

    protected internal override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers) =>
        base.HandleKeyEvent(eventType, key, modifiers) || (eventType == KeyEventType.Down && owner.HandleEditorKeyDown(key, modifiers));
}

/// <summary>Holds the placeholder over the editor; a press on it focuses the editor.</summary>
internal sealed class TextInputPlaceholderHost : FlexBox
{
    private readonly TextInputCore owner;

    public TextInputPlaceholderHost(TextInputCore owner, TextCore placeholder)
    {
        this.owner = owner;
        PositionType = FuiCs.PositionType.Absolute;
        ClipToBounds = false;
        AddChild(placeholder);
        RequireInteractive();
    }

    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        if (e.EventType == PointerEventType.Down)
        {
            owner.HandleShellPointerDown();
        }
    }
}

/// <summary>A text area's scroll view; a press on it focuses the editor.</summary>
internal sealed class TextInputEditorViewport(TextInputCore owner) : ScrollView
{
    protected internal override void HandlePointerEvent(PointerEventArgs e)
    {
        base.HandlePointerEvent(e);
        if (e.EventType == PointerEventType.Down)
        {
            owner.HandleViewportPointerDown();
        }
    }
}
