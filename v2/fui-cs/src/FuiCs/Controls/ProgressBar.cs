using System.Globalization;

namespace FuiCs;

/// <summary>
/// A bar that fills from its start in proportion to <see cref="Value"/> between <see cref="Minimum"/> and
/// <see cref="Maximum"/>; a vertical bar fills from the bottom.
/// </summary>
public class ProgressBar : FlexBox
{
    private const float BorderWidth = 1.0f;

    private readonly FlexBox fill;
    private float minimum;
    private float maximum = 100.0f;
    private float value;
    private float length = 220.0f;
    private bool fillsLength;
    private float thickness = 14.0f;
    private Orientation orientation = FuiCs.Orientation.Horizontal;
    private Color? trackColor;
    private Color? fillColor;
    private CornerRadius? cornerRadius;

    /// <summary>A horizontal bar at <paramref name="value"/>, from 0 to 100.</summary>
    public ProgressBar(float value = 0.0f)
    {
        fill = new FlexBox();
        AddChild(fill);
        SemanticOrientation = FuiCs.Orientation.Horizontal;
        Track(Theme.Active.Subscribe(_ => Refresh()));
        this.value = Math.Clamp(value, minimum, maximum);
        Refresh();
    }

    /// <summary>The lowest value; raising it above <see cref="Maximum"/> raises the maximum too.</summary>
    public float Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            maximum = Math.Max(maximum, value);
            this.value = Math.Clamp(this.value, minimum, maximum);
            Refresh();
        }
    }

    /// <summary>The highest value; lowering it below <see cref="Minimum"/> lowers the minimum too.</summary>
    public float Maximum
    {
        get => maximum;
        set
        {
            maximum = value;
            minimum = Math.Min(minimum, value);
            this.value = Math.Clamp(this.value, minimum, maximum);
            Refresh();
        }
    }

    /// <summary>The progress, clamped to the range.</summary>
    public float Value
    {
        get => value;
        set
        {
            this.value = Math.Clamp(value, minimum, maximum);
            Refresh();
        }
    }

    /// <summary>The bar's extent along its orientation, in pixels.</summary>
    public float Length
    {
        get => length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            length = value;
            fillsLength = false;
            Refresh();
        }
    }

    /// <summary>
    /// Whether the bar fills its container along its orientation, as <c>FillWidth</c> or <c>FillHeight</c> does,
    /// instead of a fixed <see cref="Length"/>. Setting <see cref="Length"/> returns to a fixed length.
    /// </summary>
    public bool FillsLength
    {
        get => fillsLength;
        set
        {
            fillsLength = value;
            Refresh();
        }
    }

    /// <summary>The bar's extent across its orientation, in pixels.</summary>
    public float Thickness
    {
        get => thickness;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            thickness = value;
            Refresh();
        }
    }

    /// <summary>Whether the bar fills across or upwards.</summary>
    public Orientation Orientation
    {
        get => orientation;
        set
        {
            orientation = value == FuiCs.Orientation.Vertical ? FuiCs.Orientation.Vertical : FuiCs.Orientation.Horizontal;
            SemanticOrientation = orientation;
            Refresh();
        }
    }

    /// <summary>The unfilled track; null uses the theme's scrollbar track color.</summary>
    public Color? TrackColor
    {
        get => trackColor;
        set
        {
            trackColor = value;
            Refresh();
        }
    }

    /// <summary>The filled part; null uses the theme's accent.</summary>
    public Color? FillColor
    {
        get => fillColor;
        set
        {
            fillColor = value;
            Refresh();
        }
    }

    /// <summary>The bar's corners; unset, the ends are fully rounded.</summary>
    public override CornerRadius CornerRadius
    {
        get => base.CornerRadius;
        set
        {
            cornerRadius = value;
            Refresh();
        }
    }

    private void Refresh()
    {
        var theme = Theme.Active.Value;
        var range = maximum - minimum;
        var percent = range > 0.0f ? Math.Clamp((value - minimum) / range, 0.0f, 1.0f) * 100.0f : 0.0f;
        // The fill is the value's share of the bar inside its border, whatever length the bar has.
        if (orientation == FuiCs.Orientation.Vertical)
        {
            FlexDirection = FuiCs.FlexDirection.Column;
            JustifyContent = FuiCs.JustifyContent.End;
            Width = thickness;
            if (fillsLength)
            {
                FillHeight = true;
            }
            else
            {
                Height = length;
            }
            fill.Width = FuiCs.Length.Percent(100.0f);
            fill.Height = FuiCs.Length.Percent(percent);
        }
        else
        {
            FlexDirection = FuiCs.FlexDirection.Row;
            JustifyContent = FuiCs.JustifyContent.Start;
            Height = thickness;
            if (fillsLength)
            {
                FillWidth = true;
            }
            else
            {
                Width = length;
            }
            fill.Width = FuiCs.Length.Percent(percent);
            fill.Height = FuiCs.Length.Percent(100.0f);
        }
        var corners = cornerRadius ?? new CornerRadius(thickness * 0.5f);
        base.CornerRadius = corners;
        fill.CornerRadius = new CornerRadius(
            Inset(corners.TopLeft), Inset(corners.TopRight), Inset(corners.BottomRight), Inset(corners.BottomLeft));
        base.Background = trackColor ?? theme.Colors.ScrollbarTrack;
        base.Border = new Border(BorderWidth, theme.Colors.Border);
        fill.Background = fillColor ?? theme.Colors.Accent;
        SemanticValueRange = new SemanticValueRange(value, minimum, maximum);
        DefaultSemanticLabel = string.Create(CultureInfo.InvariantCulture, $"Progress bar, value {value}, range {minimum} to {maximum}");
    }

    private static float Inset(float radius) => Math.Max(0.0f, radius - BorderWidth);
}
