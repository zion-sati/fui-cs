namespace FuiCs;

/// <summary>The state a text input's presenter styles: whether it is a multi-line area, enabled, and wrapping.</summary>
public readonly record struct TextInputVisualState(bool IsMultiline, bool IsEnabled, bool IsWrapping);

/// <summary>
/// Styles a text input's shell for each state. The control binds it to the shell, the node that hosts the editor (the
/// editor itself, or a text area's scroll box) and the placeholder's host before the first <see cref="Apply"/>.
/// </summary>
public abstract class TextInputPresenter
{
    private FlexBox? host;
    private Node? editorHost;
    private FlexBox? placeholderHost;

    /// <summary>The control itself, which holds the editor and the placeholder.</summary>
    protected FlexBox Host => host ?? throw new InvalidOperationException("The presenter is not bound to a text input.");

    /// <summary>The node holding the editor: the editor itself, or a text area's scroll box.</summary>
    protected Node EditorHost => editorHost ?? throw new InvalidOperationException("The presenter is not bound to a text input.");

    /// <summary>
    /// The placeholder's host, shown over the editor while the text is empty; <see cref="Apply"/> positions and sizes it
    /// so its content lines up with the editor.
    /// </summary>
    protected FlexBox PlaceholderHost => placeholderHost ?? throw new InvalidOperationException("The presenter is not bound to a text input.");

    /// <summary>
    /// Styles the shell for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public abstract void Apply(Theme theme, TextInputVisualState state, TextInputColors? colors);

    internal void Bind(FlexBox shell, Node editor, FlexBox placeholder)
    {
        host = shell;
        editorHost = editor;
        placeholderHost = placeholder;
        // A new presenter lays out the placeholder from scratch, not on top of the previous presenter's geometry.
        placeholder.Position = new Point(0.0f, 0.0f);
        placeholder.Padding = new Thickness(0.0f);
        placeholder.Width = Length.Auto;
        placeholder.Height = Length.Auto;
    }
}

/// <summary>Creates a text input's presenter, as WPF's control templates do.</summary>
public sealed class TextInputTemplate(Func<TextInputPresenter> createPresenter)
{
    /// <summary>A bordered, rounded field on the theme's surface, padded by the theme's spacing.</summary>
    public static TextInputTemplate Default { get; } = new(static () => new DefaultTextInputPresenter());

    /// <summary>Builds a presenter.</summary>
    public TextInputPresenter CreatePresenter() => createPresenter();

    private sealed class DefaultTextInputPresenter : TextInputPresenter
    {
        public override void Apply(Theme theme, TextInputVisualState state, TextInputColors? colors)
        {
            var horizontalPadding = theme.Spacing.Md;
            var verticalPadding = theme.Spacing.Sm;
            var editableCursor = state.IsEnabled ? CursorStyle.Text : CursorStyle.Default;
            Host.Background = colors?.Background ?? theme.Colors.Surface;
            Host.CornerRadius = new CornerRadius(theme.Spacing.Sm);
            Host.Border = new Border(1.0f, colors?.Border ?? theme.Colors.Border);
            Host.Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
            Host.Cursor = !state.IsMultiline && state.IsEnabled ? CursorStyle.Text : CursorStyle.Default;
            Host.Opacity = state.IsEnabled ? 1.0f : 0.6f;
            EditorHost.Cursor = editableCursor;
            // The placeholder covers the field inside its border, with the field's padding, so its text sits where the
            // editor's does and its box ends at the field's edge.
            PlaceholderHost.Position = new Point(0.0f, 0.0f);
            PlaceholderHost.Width = Length.Percent(100.0f);
            PlaceholderHost.Height = Length.Percent(100.0f);
            PlaceholderHost.Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
            PlaceholderHost.Cursor = editableCursor;
        }
    }
}
