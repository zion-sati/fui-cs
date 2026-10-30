namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Dropdown"/>.</summary>
public static class DropdownExtensions
{
    extension<TDropdown>(TDropdown dropdown) where TDropdown : Dropdown
    {
        /// <summary>Sets <see cref="Dropdown.Items"/>.</summary>
        public TDropdown Items(params IReadOnlyList<DropdownItem> items)
        {
            dropdown.Items = items;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.SelectedIndex"/>.</summary>
        public TDropdown SelectedIndex(int index)
        {
            dropdown.SelectedIndex = index;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.MaxVisibleItems"/>.</summary>
        public TDropdown MaxVisibleItems(int? count)
        {
            dropdown.MaxVisibleItems = count;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.PopupWidth"/>.</summary>
        public TDropdown PopupWidth(float? width)
        {
            dropdown.PopupWidth = width;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.PopupPanelColor"/>.</summary>
        public TDropdown PopupPanelColor(Color? color)
        {
            dropdown.PopupPanelColor = color;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.PopupPanelBackgroundBlur"/>.</summary>
        public TDropdown PopupPanelBackgroundBlur(float? blur)
        {
            dropdown.PopupPanelBackgroundBlur = blur;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.Sizing"/>.</summary>
        public TDropdown Sizing(DropdownSizing? sizing)
        {
            dropdown.Sizing = sizing;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.Colors"/>.</summary>
        public TDropdown Colors(DropdownColors? colors)
        {
            dropdown.Colors = colors;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.FieldTemplate"/>.</summary>
        public TDropdown FieldTemplate(DropdownFieldTemplate? template)
        {
            dropdown.FieldTemplate = template;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.ChevronTemplate"/>.</summary>
        public TDropdown ChevronTemplate(DropdownChevronTemplate? template)
        {
            dropdown.ChevronTemplate = template;
            return dropdown;
        }

        /// <summary>Sets <see cref="Dropdown.OptionRowTemplate"/>.</summary>
        public TDropdown OptionRowTemplate(DropdownOptionRowTemplate? template)
        {
            dropdown.OptionRowTemplate = template;
            return dropdown;
        }

        /// <summary>Adds a handler to <see cref="Dropdown.Changed"/>.</summary>
        public TDropdown OnChanged(Action<DropdownChangedEventArgs<DropdownItem>> handler)
        {
            dropdown.Changed += handler;
            return dropdown;
        }
    }
}
