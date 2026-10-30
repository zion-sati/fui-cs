namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Switch"/>.</summary>
public static class SwitchExtensions
{
    extension<TSwitch>(TSwitch control) where TSwitch : Switch
    {
        /// <summary>Sets <see cref="Switch.IsChecked"/>.</summary>
        public TSwitch IsChecked(bool isChecked)
        {
            control.IsChecked = isChecked;
            return control;
        }

        /// <summary>Adds a handler to <see cref="Switch.Changed"/>.</summary>
        public TSwitch OnChanged(Action<SwitchChangedEventArgs> handler)
        {
            control.Changed += handler;
            return control;
        }
    }
}
