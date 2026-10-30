namespace FuiCs;

/// <summary>The field's size and padding, which the dropdown also uses to size its list.</summary>
public readonly record struct DropdownFieldMetrics(
    float Height,
    float FontSize,
    float ChevronBoxSize,
    float PaddingLeft,
    float PaddingTop,
    float PaddingRight,
    float PaddingBottom);

/// <summary>The state a dropdown field shows, with the label of the selected option.</summary>
public readonly record struct DropdownFieldVisualState(bool IsOpen, bool IsFocused, bool IsEnabled, bool IsPressed, string SelectedLabel);

/// <summary>
/// The field a <see cref="DropdownFieldTemplate"/> builds for a dropdown: a root holding the host of the selected
/// value's text and the host the chevron goes in. <see cref="Apply"/> restyles them for each state.
/// </summary>
public abstract class DropdownFieldPresenter(FlexBox root, FlexBox valueHost, Text valueNode, FlexBox chevronHost, DropdownFieldMetrics metrics)
{
    /// <summary>The field's root, which the dropdown holds.</summary>
    public FlexBox Root { get; } = root ?? throw new ArgumentNullException(nameof(root));

    /// <summary>The node holding the selected value's text.</summary>
    public FlexBox ValueHost { get; } = valueHost ?? throw new ArgumentNullException(nameof(valueHost));

    /// <summary>The text the dropdown sets to the selected option's label.</summary>
    public Text ValueNode { get; } = valueNode ?? throw new ArgumentNullException(nameof(valueNode));

    /// <summary>Where the dropdown places its chevron presenter's root.</summary>
    public FlexBox ChevronHost { get; } = chevronHost ?? throw new ArgumentNullException(nameof(chevronHost));

    /// <summary>The field's size and padding.</summary>
    public DropdownFieldMetrics Metrics { get; } = metrics;

    /// <summary>
    /// Restyles the field for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public abstract void Apply(Theme theme, DropdownFieldVisualState state, DropdownColors? colors);
}

/// <summary>Creates a dropdown's field for its sizing, as WPF's control templates do.</summary>
public sealed class DropdownFieldTemplate(Func<DropdownSizing?, DropdownFieldPresenter> createPresenter)
{
    /// <summary>A bordered, rounded field: the value fading at its right edge, then the chevron's box.</summary>
    public static DropdownFieldTemplate Default { get; } = new(DefaultDropdownFieldPresenter.Create);

    /// <summary>Builds a field for <paramref name="sizing"/>.</summary>
    public DropdownFieldPresenter CreatePresenter(DropdownSizing? sizing) => createPresenter(sizing);
}

/// <summary>The state a dropdown's chevron shows.</summary>
public readonly record struct DropdownChevronVisualState(bool IsOpen, bool IsHovered, bool IsEnabled);

/// <summary>The chevron a <see cref="DropdownChevronTemplate"/> builds; <see cref="Apply"/> restyles it for each state.</summary>
public abstract class DropdownChevronPresenter(FlexBox root)
{
    /// <summary>The chevron's root.</summary>
    public FlexBox Root { get; } = root ?? throw new ArgumentNullException(nameof(root));

    /// <summary>Restyles the chevron for <paramref name="state"/>.</summary>
    public abstract void Apply(Theme theme, DropdownChevronVisualState state);
}

/// <summary>Creates a dropdown's chevron for its sizing.</summary>
public sealed class DropdownChevronTemplate(Func<DropdownSizing?, DropdownChevronPresenter> createPresenter)
{
    /// <summary>A down arrow that turns up while the list is open, muted until hovered.</summary>
    public static DropdownChevronTemplate Default { get; } = new(DefaultDropdownChevronPresenter.Create);

    /// <summary>Builds a chevron for <paramref name="sizing"/>.</summary>
    public DropdownChevronPresenter CreatePresenter(DropdownSizing? sizing) => createPresenter(sizing);
}

/// <summary>An option row's height, horizontal padding and font size.</summary>
public readonly record struct DropdownOptionRowMetrics(float Height, float PaddingLeft = 10.0f, float PaddingRight = 10.0f, float FontSize = 16.0f);

/// <summary>The state a dropdown's option row shows.</summary>
public readonly record struct DropdownOptionRowVisualState(bool IsHighlighted, bool IsSelected, bool IsEnabled);

/// <summary>
/// One row of a dropdown's list, which a <see cref="DropdownOptionRowTemplate"/> builds: a root holding the option's
/// label. <see cref="Apply"/> restyles it for each state.
/// </summary>
public abstract class DropdownOptionRowPresenter(FlexBox root, Text label, DropdownOptionRowMetrics metrics)
{
    /// <summary>The row's root.</summary>
    public FlexBox Root { get; } = root ?? throw new ArgumentNullException(nameof(root));

    /// <summary>The text the dropdown sets to the option's label.</summary>
    public Text Label { get; } = label ?? throw new ArgumentNullException(nameof(label));

    /// <summary>The row's size and padding.</summary>
    public DropdownOptionRowMetrics Metrics { get; } = metrics;

    /// <summary>
    /// Restyles the row for <paramref name="state"/>, with <paramref name="colors"/> over the theme's.
    /// </summary>
    public abstract void Apply(Theme theme, DropdownOptionRowVisualState state, DropdownColors? colors);
}

/// <summary>Creates the rows of a dropdown's list for its sizing.</summary>
public sealed class DropdownOptionRowTemplate(Func<DropdownSizing?, DropdownOptionRowPresenter> createPresenter)
{
    /// <summary>A padded row whose highlight is the context menu's hover color and whose selected label is the accent.</summary>
    public static DropdownOptionRowTemplate Default { get; } = new(DefaultDropdownOptionRowPresenter.Create);

    /// <summary>Builds a row for <paramref name="sizing"/>.</summary>
    public DropdownOptionRowPresenter CreatePresenter(DropdownSizing? sizing) => createPresenter(sizing);
}
