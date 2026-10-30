namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Checkbox"/>.</summary>
public static class CheckboxExtensions
{
    extension<TCheckbox>(TCheckbox checkbox) where TCheckbox : Checkbox
    {
        /// <summary>Sets <see cref="Checkbox.IsChecked"/>.</summary>
        public TCheckbox IsChecked(bool? isChecked)
        {
            checkbox.IsChecked = isChecked;
            return checkbox;
        }

        /// <summary>Sets <see cref="Checkbox.IsThreeState"/>.</summary>
        public TCheckbox IsThreeState(bool isThreeState = true)
        {
            checkbox.IsThreeState = isThreeState;
            return checkbox;
        }

        /// <summary>Adds a handler to <see cref="Checkbox.Changed"/>.</summary>
        public TCheckbox OnChanged(Action<CheckboxChangedEventArgs> handler)
        {
            checkbox.Changed += handler;
            return checkbox;
        }
    }
}
