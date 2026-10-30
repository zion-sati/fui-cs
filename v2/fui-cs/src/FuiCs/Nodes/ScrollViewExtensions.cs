namespace FuiCs;

/// <summary>Fluent configuration for <see cref="ScrollView"/>.</summary>
public static class ScrollViewExtensions
{
    extension<TScroll>(TScroll scroll) where TScroll : ScrollView
    {
        /// <summary>Sets <see cref="ScrollView.IsHorizontalScrollEnabled"/>.</summary>
        public TScroll HorizontalScrollEnabled(bool enabled = true)
        {
            scroll.IsHorizontalScrollEnabled = enabled;
            return scroll;
        }

        /// <summary>Sets <see cref="ScrollView.IsVerticalScrollEnabled"/>.</summary>
        public TScroll VerticalScrollEnabled(bool enabled = true)
        {
            scroll.IsVerticalScrollEnabled = enabled;
            return scroll;
        }

        /// <summary>Sets how quickly momentum scrolling slows.</summary>
        public TScroll Friction(float friction)
        {
            scroll.Friction = friction;
            return scroll;
        }

        /// <summary>Sets <see cref="ScrollView.SmoothScrolling"/>.</summary>
        public TScroll SmoothScrolling(bool smooth = true)
        {
            scroll.SmoothScrolling = smooth;
            return scroll;
        }

        /// <summary>Sets the share of the main axis the view starts from before growing.</summary>
        public TScroll FlexBasis(float basis)
        {
            scroll.FlexBasis = basis;
            return scroll;
        }

        /// <summary>Overrides the content's size for scrolling; null keeps the measured size.</summary>
        public TScroll ScrollContentSize(float? width, float? height)
        {
            scroll.SetScrollContentSize(width, height);
            return scroll;
        }

        /// <summary>Sets the offset, through its transition if it has one; before the view is built, it is the initial offset.</summary>
        public TScroll ScrollOffset(float horizontalOffset, float verticalOffset)
        {
            scroll.SetScrollOffset(horizontalOffset, verticalOffset);
            return scroll;
        }

        /// <summary>Sets which property changes animate; only the scroll offset applies.</summary>
        public TScroll Transitions(NodeTransitions? transitions)
        {
            scroll.Transitions = transitions;
            return scroll;
        }

        /// <summary>Sets <see cref="ScrollView.PersistScroll"/>.</summary>
        public TScroll PersistScroll(bool persist = true)
        {
            scroll.PersistScroll = persist;
            return scroll;
        }

        /// <summary>Adds a handler to <see cref="ScrollView.ScrollChanged"/>.</summary>
        public TScroll OnScrollChanged(Action<ScrollChangedEventArgs> handler)
        {
            scroll.ScrollChanged += handler;
            return scroll;
        }
    }
}
