namespace FuiCs;

/// <summary>
/// A text hint shown next to a node while the pointer rests on it, or while the node has keyboard focus. The timing
/// follows WPF's ToolTipService: a delay before the first tooltip shows, a shorter one for the next within a moment,
/// and a duration after which it hides. A string converts to a tooltip with the default timing.
/// </summary>
public sealed class ToolTip(string text)
{
    /// <summary>The tooltip's text.</summary>
    public string Text { get; init; } = text;

    /// <summary>How long the pointer rests on the node before the tooltip shows.</summary>
    public TimeSpan InitialShowDelay { get; init; } = TimeSpan.FromMilliseconds(700);

    /// <summary>How soon after one tooltip shows that the next shows at once.</summary>
    public TimeSpan BetweenShowDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>How long the tooltip stays; zero keeps it until the pointer leaves or focus moves.</summary>
    public TimeSpan ShowDuration { get; init; } = TimeSpan.FromMilliseconds(5000);

    /// <summary>Where the tooltip opens relative to the node.</summary>
    public PopupPlacement Placement { get; init; } = PopupPlacement.Top;

    /// <summary>A horizontal shift from the placed position.</summary>
    public float HorizontalOffset { get; init; }

    /// <summary>A vertical shift from the placed position.</summary>
    public float VerticalOffset { get; init; }

    /// <summary>Whether the tooltip also shows while the node has visible keyboard focus.</summary>
    public bool ShowsOnKeyboardFocus { get; init; } = true;

    /// <summary>The panel color; the theme's when null.</summary>
    public Color? Background { get; init; }

    /// <summary>The text color; the theme's when null.</summary>
    public Color? TextColor { get; init; }

    /// <summary>A tooltip of <paramref name="text"/> with the default timing.</summary>
    public static implicit operator ToolTip(string text) => new(text);
}

public abstract partial class Node
{
    /// <summary>The tooltip shown while the pointer rests on this node or it has visible keyboard focus.</summary>
    public ToolTip? ToolTip
    {
        get;
        set
        {
            field = value;
            if (value is not null)
            {
                RequireInteractive();
                ToolTipManager.Install();
            }
            EventRouter.ToolTips?.ToolTipChanged(this, value);
        }
    }
}

/// <summary>Fluent tooltip assignment.</summary>
public static class ToolTipExtensions
{
    extension<TNode>(TNode node) where TNode : Node
    {
        /// <summary>Sets <see cref="Node.ToolTip"/>.</summary>
        public TNode ToolTip(ToolTip? toolTip)
        {
            node.ToolTip = toolTip;
            return node;
        }
    }
}
