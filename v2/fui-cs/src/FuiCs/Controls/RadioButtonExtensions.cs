namespace FuiCs;

/// <summary>Fluent configuration for <see cref="RadioButton"/>.</summary>
public static class RadioButtonExtensions
{
    extension<TRadio>(TRadio radio) where TRadio : RadioButton
    {
        /// <summary>Sets <see cref="RadioButton.IsChecked"/>.</summary>
        public TRadio IsChecked(bool isChecked)
        {
            radio.IsChecked = isChecked;
            return radio;
        }

        /// <summary>Adds a handler to <see cref="RadioButton.Changed"/>.</summary>
        public TRadio OnChanged(Action<RadioButtonChangedEventArgs> handler)
        {
            radio.Changed += handler;
            return radio;
        }
    }
}
