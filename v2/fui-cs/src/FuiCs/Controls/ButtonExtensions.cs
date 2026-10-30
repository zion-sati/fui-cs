namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Button"/>.</summary>
public static class ButtonExtensions
{
    extension<TButton>(TButton button) where TButton : Button
    {
        /// <summary>Sets <see cref="Button.Label"/>.</summary>
        public TButton Label(string label)
        {
            button.Label = label;
            return button;
        }

        /// <summary>Adds a handler to <see cref="Button.Click"/>.</summary>
        public TButton OnClick(Action<ClickEventArgs> handler)
        {
            button.Click += handler;
            return button;
        }

        /// <summary>Adds a handler to <see cref="Button.HoverChanged"/>.</summary>
        public TButton OnHoverChanged(Action<HoverChangedEventArgs> handler)
        {
            button.HoverChanged += handler;
            return button;
        }

        /// <summary>Sets <see cref="Button.Colors"/>.</summary>
        public TButton Colors(ButtonColors colors)
        {
            button.Colors = colors;
            return button;
        }

        /// <summary>Sets <see cref="Button.Template"/>.</summary>
        public TButton Template(ButtonTemplate template)
        {
            button.Template = template;
            return button;
        }

        /// <summary>Sets <see cref="Button.HoverBackground"/>.</summary>
        public TButton HoverBackground(Color color)
        {
            button.HoverBackground = color;
            return button;
        }

        /// <summary>Sets <see cref="Button.PressedBackground"/>.</summary>
        public TButton PressedBackground(Color color)
        {
            button.PressedBackground = color;
            return button;
        }

        /// <summary>Sets the label's font family.</summary>
        public TButton FontFamily(FontFamily family)
        {
            button.FontFamily = family;
            return button;
        }

        /// <summary>Sets <see cref="Button.FontWeight"/>.</summary>
        public TButton FontWeight(FontWeight weight)
        {
            button.FontWeight = weight;
            return button;
        }

        /// <summary>Sets <see cref="Button.FontStyle"/>.</summary>
        public TButton FontStyle(FontStyle style)
        {
            button.FontStyle = style;
            return button;
        }

        /// <summary>Sets <see cref="Button.FontSize"/>.</summary>
        public TButton FontSize(float size)
        {
            button.FontSize = size;
            return button;
        }

        /// <summary>Sets the label's color.</summary>
        public TButton TextColor(Color color)
        {
            button.TextColor = color;
            return button;
        }
    }
}
