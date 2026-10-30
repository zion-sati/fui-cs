namespace FuiCs;

/// <summary>Fluent configuration for checkboxes, radio buttons and switches.</summary>
public static class PressableLabeledControlExtensions
{
    extension<TControl>(TControl control) where TControl : PressableLabeledControl
    {
        /// <summary>Sets <see cref="PressableLabeledControl.Label"/>.</summary>
        public TControl Label(string label)
        {
            control.Label = label;
            return control;
        }

        /// <summary>Adds a handler to <see cref="PressableLabeledControl.Click"/>.</summary>
        public TControl OnClick(Action<ClickEventArgs> handler)
        {
            control.Click += handler;
            return control;
        }

        /// <summary>Sets <see cref="PressableLabeledControl.Colors"/>.</summary>
        public TControl Colors(LabeledControlColors colors)
        {
            control.Colors = colors;
            return control;
        }

        /// <summary>Sets <see cref="PressableLabeledControl.Sizing"/>.</summary>
        public TControl Sizing(LabeledControlSizing sizing)
        {
            control.Sizing = sizing;
            return control;
        }

        /// <summary>Sets <see cref="PressableLabeledControl.Template"/>.</summary>
        public TControl Template(IndicatorTemplate template)
        {
            control.Template = template;
            return control;
        }
    }
}
