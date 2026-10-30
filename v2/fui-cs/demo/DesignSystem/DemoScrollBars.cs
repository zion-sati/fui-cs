namespace FuiCs.Demo.DesignSystem;

/// <summary>The demo's scroll bars: a 12 px rounded track with an 8 px rounded thumb in the demo's primary color.</summary>
public static class DemoScrollBars
{
    public static ScrollBar Configure(ScrollBar scrollBar) => scrollBar
        .TrackThickness(DemoTokens.ScrollBarTrackWidth)
        .ThumbThickness(DemoTokens.ScrollBarThumbWidth)
        .MinThumbLength(DemoTokens.ScrollBarThumbMinHeight)
        .TrackCornerRadius(DemoTokens.ScrollBarTrackWidth * 0.5f)
        .ThumbCornerRadius(DemoTokens.ScrollBarThumbWidth * 0.5f);

    public static void Configure(ScrollBox scrollBox)
    {
        ArgumentNullException.ThrowIfNull(scrollBox);
        Configure(scrollBox.VerticalScrollBar);
        Configure(scrollBox.HorizontalScrollBar);
    }

    /// <summary>Colors a bar for the theme; the track defaults to the page color.</summary>
    public static void ApplyTheme(ScrollBar scrollBar, Theme theme, Color? trackColor = null) =>
        scrollBar.TrackColor(trackColor ?? DemoColors.ScrollBarTrackColor(theme)).ThumbColor(DemoColors.ScrollBarThumbColor(theme));

    public static void ApplyTheme(ScrollBox scrollBox, Theme theme, Color? trackColor = null)
    {
        ArgumentNullException.ThrowIfNull(scrollBox);
        ApplyTheme(scrollBox.VerticalScrollBar, theme, trackColor);
        ApplyTheme(scrollBox.HorizontalScrollBar, theme, trackColor);
    }
}

/// <summary>A scroll box with the demo's scroll bars; its owner colors them for the theme with <see cref="DemoScrollBars.ApplyTheme(ScrollBox, Theme, Color?)"/>.</summary>
public class DemoScrollBox : ScrollBox
{
    public DemoScrollBox(ScrollState? scrollState = null)
        : base(scrollState)
    {
        DemoScrollBars.Configure(this);
    }
}
