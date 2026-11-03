namespace FuiCs;

/// <summary>Fluent configuration for <see cref="ProgressBar"/>.</summary>
public static class ProgressBarExtensions
{
    extension<TBar>(TBar bar) where TBar : ProgressBar
    {
        /// <summary>Sets <see cref="ProgressBar.Minimum"/>.</summary>
        public TBar Minimum(float minimum)
        {
            bar.Minimum = minimum;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.Maximum"/>.</summary>
        public TBar Maximum(float maximum)
        {
            bar.Maximum = maximum;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.Value"/>.</summary>
        public TBar Value(float value)
        {
            bar.Value = value;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.Length"/>.</summary>
        public TBar Length(float length)
        {
            bar.Length = length;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.FillsLength"/>: the bar fills its container along its orientation.</summary>
        public TBar FillLength()
        {
            bar.FillsLength = true;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.Thickness"/>.</summary>
        public TBar Thickness(float thickness)
        {
            bar.Thickness = thickness;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.Orientation"/>.</summary>
        public TBar Orientation(Orientation orientation)
        {
            bar.Orientation = orientation;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.TrackColor"/>.</summary>
        public TBar TrackColor(Color color)
        {
            bar.TrackColor = color;
            return bar;
        }

        /// <summary>Sets <see cref="ProgressBar.FillColor"/>.</summary>
        public TBar FillColor(Color color)
        {
            bar.FillColor = color;
            return bar;
        }
    }
}
