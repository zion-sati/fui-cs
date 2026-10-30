namespace FuiCs;

/// <summary>Fluent styling for <see cref="ScrollBar"/>, as a <see cref="ScrollBox"/>'s bars are restyled.</summary>
public static class ScrollBarExtensions
{
    extension(ScrollBar scrollBar)
    {
        /// <summary>Sets <see cref="ScrollBar.TrackThickness"/>.</summary>
        public ScrollBar TrackThickness(float thickness)
        {
            scrollBar.TrackThickness = thickness;
            return scrollBar;
        }

        /// <summary>Sets <see cref="ScrollBar.ThumbThickness"/>.</summary>
        public ScrollBar ThumbThickness(float thickness)
        {
            scrollBar.ThumbThickness = thickness;
            return scrollBar;
        }

        /// <summary>Sets the shortest the thumb gets on long content.</summary>
        public ScrollBar MinThumbLength(float length)
        {
            scrollBar.MinThumbLength = length;
            return scrollBar;
        }

        /// <summary>Sets <see cref="ScrollBar.TrackCornerRadius"/>.</summary>
        public ScrollBar TrackCornerRadius(float radius)
        {
            scrollBar.TrackCornerRadius = radius;
            return scrollBar;
        }

        /// <summary>Sets <see cref="ScrollBar.ThumbCornerRadius"/>.</summary>
        public ScrollBar ThumbCornerRadius(float radius)
        {
            scrollBar.ThumbCornerRadius = radius;
            return scrollBar;
        }

        /// <summary>Sets the track's color.</summary>
        public ScrollBar TrackColor(Color color)
        {
            scrollBar.TrackColor = color;
            return scrollBar;
        }

        /// <summary>Sets the thumb's color.</summary>
        public ScrollBar ThumbColor(Color color)
        {
            scrollBar.ThumbColor = color;
            return scrollBar;
        }
    }
}
