namespace FuiCs;

/// <summary>Fluent configuration for <see cref="ScrollBox"/>.</summary>
public static class ScrollBoxExtensions
{
    extension<TBox>(TBox box) where TBox : ScrollBox
    {
        /// <summary>Sets <see cref="ScrollBox.VerticalScrollBarVisibility"/>.</summary>
        public TBox VerticalScrollBarVisibility(ScrollBarVisibility visibility)
        {
            box.VerticalScrollBarVisibility = visibility;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.HorizontalScrollBarVisibility"/>.</summary>
        public TBox HorizontalScrollBarVisibility(ScrollBarVisibility visibility)
        {
            box.HorizontalScrollBarVisibility = visibility;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.ScrollBarGutter"/>.</summary>
        public TBox ScrollBarGutter(float gutter)
        {
            box.ScrollBarGutter = gutter;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.IsHorizontalScrollEnabled"/>.</summary>
        public TBox HorizontalScrollEnabled(bool enabled = true)
        {
            box.IsHorizontalScrollEnabled = enabled;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.IsVerticalScrollEnabled"/>.</summary>
        public TBox VerticalScrollEnabled(bool enabled = true)
        {
            box.IsVerticalScrollEnabled = enabled;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.SmoothScrolling"/>.</summary>
        public TBox SmoothScrolling(bool smooth = true)
        {
            box.SmoothScrolling = smooth;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.WheelLatching"/>.</summary>
        public TBox WheelLatching(bool latching = true)
        {
            box.WheelLatching = latching;
            return box;
        }

        /// <summary>Sets <see cref="ScrollBox.PersistScroll"/>.</summary>
        public TBox PersistScroll(bool persist = true)
        {
            box.PersistScroll = persist;
            return box;
        }

        /// <summary>Sets the offset, through its transition if it has one; before the box is built, it is the initial offset.</summary>
        public TBox ScrollOffset(float horizontalOffset, float verticalOffset)
        {
            box.SetScrollOffset(horizontalOffset, verticalOffset);
            return box;
        }

        /// <summary>Overrides the content's size for scrolling; null keeps the measured size.</summary>
        public TBox ScrollContentSize(float? width, float? height)
        {
            box.SetScrollContentSize(width, height);
            return box;
        }

        /// <summary>Adds a handler to <see cref="ScrollBox.ScrollChanged"/>.</summary>
        public TBox OnScrollChanged(Action<ScrollChangedEventArgs> handler)
        {
            box.ScrollChanged += handler;
            return box;
        }
    }
}
