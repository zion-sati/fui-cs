namespace FuiCs;

/// <summary>Fluent configuration for <see cref="ComboBox"/>.</summary>
public static class ComboBoxExtensions
{
    extension<TComboBox>(TComboBox comboBox) where TComboBox : ComboBox
    {
        /// <summary>Sets the items from their values.</summary>
        public TComboBox Items(params IReadOnlyList<string> values)
        {
            var items = new ComboBoxItem[values.Count];
            for (var index = 0; index < values.Count; ++index)
            {
                items[index] = new ComboBoxItem(values[index]);
            }
            comboBox.Items = items;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.Text"/>.</summary>
        public TComboBox Text(string text)
        {
            comboBox.Text = text;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.Placeholder"/>.</summary>
        public TComboBox Placeholder(string placeholder)
        {
            comboBox.Placeholder = placeholder;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.SelectedIndex"/>.</summary>
        public TComboBox SelectedIndex(int index)
        {
            comboBox.SelectedIndex = index;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.AllowCustomValue"/>.</summary>
        public TComboBox AllowCustomValue(bool allow = true)
        {
            comboBox.AllowCustomValue = allow;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.AutoComplete"/>.</summary>
        public TComboBox AutoComplete(bool autoComplete = true)
        {
            comboBox.AutoComplete = autoComplete;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.FilterMode"/>.</summary>
        public TComboBox FilterMode(ComboBoxFilterMode mode)
        {
            comboBox.FilterMode = mode;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.CommitMode"/>.</summary>
        public TComboBox CommitMode(ComboBoxCommitMode mode)
        {
            comboBox.CommitMode = mode;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.OpenOnFocus"/>.</summary>
        public TComboBox OpenOnFocus(bool open = true)
        {
            comboBox.OpenOnFocus = open;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.StaysOpenOnEdit"/>.</summary>
        public TComboBox StaysOpenOnEdit(bool staysOpen = true)
        {
            comboBox.StaysOpenOnEdit = staysOpen;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.MaxVisibleItems"/>.</summary>
        public TComboBox MaxVisibleItems(int? count)
        {
            comboBox.MaxVisibleItems = count;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.PopupWidth"/>.</summary>
        public TComboBox PopupWidth(float? width)
        {
            comboBox.PopupWidth = width;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.PopupPanelColor"/>.</summary>
        public TComboBox PopupPanelColor(Color? color)
        {
            comboBox.PopupPanelColor = color;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.PopupPanelBackgroundBlur"/>.</summary>
        public TComboBox PopupPanelBackgroundBlur(float? blur)
        {
            comboBox.PopupPanelBackgroundBlur = blur;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.Sizing"/>.</summary>
        public TComboBox Sizing(DropdownSizing? sizing)
        {
            comboBox.Sizing = sizing;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.Colors"/>.</summary>
        public TComboBox Colors(DropdownColors? colors)
        {
            comboBox.Colors = colors;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.ChevronTemplate"/>.</summary>
        public TComboBox ChevronTemplate(DropdownChevronTemplate? template)
        {
            comboBox.ChevronTemplate = template;
            return comboBox;
        }

        /// <summary>Sets <see cref="ComboBox.OptionRowTemplate"/>.</summary>
        public TComboBox OptionRowTemplate(DropdownOptionRowTemplate? template)
        {
            comboBox.OptionRowTemplate = template;
            return comboBox;
        }

        /// <summary>Adds a handler to <see cref="ComboBox.Changed"/>.</summary>
        public TComboBox OnChanged(Action<ComboBoxChangedEventArgs<ComboBoxItem>> handler)
        {
            comboBox.Changed += handler;
            return comboBox;
        }

        /// <summary>Adds a handler to <see cref="ComboBox.TextChanged"/>.</summary>
        public TComboBox OnTextChanged(Action<TextChangedEventArgs> handler)
        {
            comboBox.TextChanged += handler;
            return comboBox;
        }
    }
}
