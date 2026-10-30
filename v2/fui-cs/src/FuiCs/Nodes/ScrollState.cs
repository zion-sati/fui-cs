namespace FuiCs;

/// <summary>
/// The scroll position and sizes of a <see cref="ScrollView"/> as signals, shared with the scroll bars and other views
/// that follow it. The view keeps them current; a scroll bar moves the view through its own target binding.
/// </summary>
public sealed class ScrollState
{
    /// <summary>The horizontal offset.</summary>
    public Signal<float> OffsetX { get; } = new(0.0f);

    /// <summary>The vertical offset.</summary>
    public Signal<float> OffsetY { get; } = new(0.0f);

    /// <summary>The content's width.</summary>
    public Signal<float> ContentWidth { get; } = new(0.0f);

    /// <summary>The content's height.</summary>
    public Signal<float> ContentHeight { get; } = new(0.0f);

    /// <summary>The viewport's width.</summary>
    public Signal<float> ViewportWidth { get; } = new(0.0f);

    /// <summary>The viewport's height.</summary>
    public Signal<float> ViewportHeight { get; } = new(0.0f);
}
