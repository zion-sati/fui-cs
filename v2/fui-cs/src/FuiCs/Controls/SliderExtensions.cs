namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Slider"/>.</summary>
public static class SliderExtensions
{
    extension<TSlider>(TSlider slider) where TSlider : Slider
    {
        /// <summary>Sets <see cref="Slider.Minimum"/>.</summary>
        public TSlider Minimum(float minimum)
        {
            slider.Minimum = minimum;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Maximum"/>.</summary>
        public TSlider Maximum(float maximum)
        {
            slider.Maximum = maximum;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Step"/>.</summary>
        public TSlider Step(float step)
        {
            slider.Step = step;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Value"/>.</summary>
        public TSlider Value(float value)
        {
            slider.Value = value;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Length"/>.</summary>
        public TSlider Length(float length)
        {
            slider.Length = length;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Orientation"/>.</summary>
        public TSlider Orientation(Orientation orientation)
        {
            slider.Orientation = orientation;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Colors"/>.</summary>
        public TSlider Colors(SliderColors colors)
        {
            slider.Colors = colors;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Sizing"/>.</summary>
        public TSlider Sizing(SliderSizing sizing)
        {
            slider.Sizing = sizing;
            return slider;
        }

        /// <summary>Sets <see cref="Slider.Template"/>.</summary>
        public TSlider Template(SliderTemplate template)
        {
            slider.Template = template;
            return slider;
        }

        /// <summary>Adds a handler to <see cref="Slider.Changed"/>.</summary>
        public TSlider OnChanged(Action<SliderChangedEventArgs> handler)
        {
            slider.Changed += handler;
            return slider;
        }
    }
}
