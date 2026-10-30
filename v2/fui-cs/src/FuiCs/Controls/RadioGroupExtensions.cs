namespace FuiCs;

/// <summary>Fluent configuration for <see cref="RadioGroup"/>.</summary>
public static class RadioGroupExtensions
{
    extension<TGroup>(TGroup group) where TGroup : RadioGroup
    {
        /// <summary>Adds radios to the group.</summary>
        public TGroup Radio(params ReadOnlySpan<RadioButton> radios)
        {
            foreach (var radio in radios)
            {
                group.AddRadio(radio);
            }
            return group;
        }

        /// <summary>
        /// Adds a radio for <paramref name="value"/>, labelled <paramref name="label"/> or the value.
        /// </summary>
        public TGroup Option(string value, string? label = null)
        {
            group.AddOption(value, label);
            return group;
        }

        /// <summary>Sets <see cref="RadioGroup.SelectedIndex"/>.</summary>
        public TGroup SelectedIndex(int index)
        {
            group.SelectedIndex = index;
            return group;
        }

        /// <summary>Adds a handler to <see cref="RadioGroup.Changed"/>.</summary>
        public TGroup OnChanged(Action<RadioGroupChangedEventArgs> handler)
        {
            group.Changed += handler;
            return group;
        }
    }
}
