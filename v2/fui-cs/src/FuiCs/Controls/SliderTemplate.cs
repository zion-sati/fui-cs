namespace FuiCs;

/// <summary>Colors a <see cref="Slider"/> uses instead of the theme's; unset colors follow the theme.</summary>
public sealed record SliderColors
{
    /// <summary>The track's color.</summary>
    public Color? Track { get; init; }

    /// <summary>The fill's color.</summary>
    public Color? Fill { get; init; }

    /// <summary>The thumb's color.</summary>
    public Color? Thumb { get; init; }
}

/// <summary>The state a slider's presenter shows. <see cref="NormalizedValue"/> is the value's position from 0 to 1.</summary>
public readonly record struct SliderVisualState(
    float Value,
    float Minimum,
    float Maximum,
    float NormalizedValue,
    Orientation Orientation,
    bool IsHovered,
    bool IsDragging,
    bool IsFocused,
    bool IsEnabled);

/// <summary>
/// The track, fill and thumb a <see cref="SliderTemplate"/> builds inside a slider. The slider sizes <see cref="Root"/>
/// to its track area, a fixed length or the length it fills, and maps pointer positions with <see cref="ThumbSize"/>;
/// <see cref="Layout"/> places the parts relative to the root, as percentages along the track rather than pixels, and
/// <see cref="Apply"/> styles them.
/// </summary>
public abstract class SliderPresenter(FlexBox root, float thumbSize, float crossAxisExtra = 2.0f)
{
    /// <summary>The parts' root, which the slider sizes to its track area.</summary>
    public FlexBox Root { get; } = root ?? throw new ArgumentNullException(nameof(root));

    /// <summary>The thumb's extent along the slider, which the value range spans less of.</summary>
    public float ThumbSize { get; } = Math.Max(1.0f, thumbSize);

    /// <summary>Extra room across the slider beyond the thumb.</summary>
    public float CrossAxisExtra { get; } = Math.Max(0.0f, crossAxisExtra);

    /// <summary>Places the parts for <paramref name="state"/> relative to <see cref="Root"/>.</summary>
    public abstract void Layout(SliderVisualState state);

    /// <summary>
    /// Styles the parts for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public abstract void Apply(Theme theme, SliderVisualState state, SliderColors? colors);
}

/// <summary>Creates the parts of a <see cref="Slider"/> for its sizing, as WPF's control templates do.</summary>
public sealed class SliderTemplate(Func<SliderSizing?, SliderPresenter> createPresenter)
{
    /// <summary>Builds a presenter for <paramref name="sizing"/>.</summary>
    public SliderPresenter CreatePresenter(SliderSizing? sizing) => createPresenter(sizing);
}
