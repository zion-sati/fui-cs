namespace FuiCs;

/// <summary>
/// Which property changes on a built node animate, and how. A null timing changes that property at once; a change
/// cancels the property's running transition and starts the next one from the current value.
/// </summary>
public sealed class NodeTransitions
{
    /// <summary>The timing of a <see cref="FlexBox.Opacity"/> change.</summary>
    public AnimationTiming? Opacity { get; set; }

    /// <summary>The timing of a <see cref="FlexBox.Background"/> change.</summary>
    public AnimationTiming? Background { get; set; }

    /// <summary>The timing of a <see cref="ScrollView.SetScrollOffset"/> change.</summary>
    public AnimationTiming? ScrollOffset { get; set; }
}
