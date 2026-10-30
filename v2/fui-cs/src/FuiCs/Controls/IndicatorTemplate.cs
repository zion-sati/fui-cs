namespace FuiCs;

/// <summary>
/// The state an indicator shows. <see cref="IsChecked"/> is null for a checkbox in its indeterminate state.
/// </summary>
public readonly record struct IndicatorVisualState(bool? IsChecked, bool IsHovered, bool IsPressed, bool IsFocused, bool IsEnabled);

/// <summary>
/// The indicator a <see cref="IndicatorTemplate"/> builds for a checkbox, radio button or switch: a root node that
/// <see cref="Apply"/> restyles for each state.
/// </summary>
public abstract class IndicatorPresenter(FlexBox root)
{
    /// <summary>The indicator's root.</summary>
    public FlexBox Root { get; } = root ?? throw new ArgumentNullException(nameof(root));

    /// <summary>
    /// Restyles the indicator for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public abstract void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors);
}

/// <summary>Creates the indicator of a checkbox, radio button or switch for its sizing, as WPF's control templates do.</summary>
public sealed class IndicatorTemplate(Func<LabeledControlSizing?, IndicatorPresenter> createPresenter)
{
    /// <summary>Builds an indicator for <paramref name="sizing"/>.</summary>
    public IndicatorPresenter CreatePresenter(LabeledControlSizing? sizing) => createPresenter(sizing);
}
