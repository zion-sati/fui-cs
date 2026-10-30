namespace FuiCs;

/// <summary>Colors a <see cref="Button"/> uses instead of the theme's; unset colors follow the theme.</summary>
public sealed record ButtonColors
{
    /// <summary>The fill at rest.</summary>
    public Color? Background { get; init; }

    /// <summary>The background while hovered; falls back to <see cref="Background"/>.</summary>
    public Color? BackgroundHover { get; init; }

    /// <summary>The background while pressed; falls back to <see cref="BackgroundHover"/>, then <see cref="Background"/>.</summary>
    public Color? BackgroundPressed { get; init; }

    /// <summary>The label's color.</summary>
    public Color? TextPrimary { get; init; }

    /// <summary>The label color while disabled.</summary>
    public Color? TextMuted { get; init; }

    /// <summary>The border's color.</summary>
    public Color? Border { get; init; }
}

/// <summary>The state a button's presenter styles.</summary>
public readonly record struct ButtonVisualState(bool IsHovered, bool IsPressed, bool IsFocused, bool IsEnabled);

/// <summary>
/// The content a <see cref="ButtonTemplate"/> builds inside a button: a root and the label text. The button styles
/// its own chrome and the label's font and color; <see cref="Apply"/> styles anything else for the current state.
/// </summary>
public class ButtonPresenter(FlexBox contentRoot, TextCore label)
{
    /// <summary>The node holding the label.</summary>
    public FlexBox ContentRoot { get; } = contentRoot ?? throw new ArgumentNullException(nameof(contentRoot));

    /// <summary>The label's text node.</summary>
    public TextCore Label { get; } = label ?? throw new ArgumentNullException(nameof(label));

    /// <summary>
    /// Restyles the button for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public virtual void Apply(Theme theme, ButtonVisualState state, ButtonColors? colors)
    {
    }
}

/// <summary>Creates the content of a <see cref="Button"/>, as WPF's control templates do.</summary>
public sealed class ButtonTemplate(Func<ButtonPresenter> createPresenter)
{
    /// <summary>A centered row holding the label.</summary>
    public static ButtonTemplate Default { get; } = new(() =>
    {
        var label = new TextCore();
        var root = FlexBox.Row(label).AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center);
        return new ButtonPresenter(root, label);
    });

    /// <summary>Builds a presenter.</summary>
    public ButtonPresenter CreatePresenter() => createPresenter();
}
