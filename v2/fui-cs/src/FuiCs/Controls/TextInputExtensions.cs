namespace FuiCs;

/// <summary>Fluent configuration for <see cref="TextInputCore"/> controls.</summary>
public static class TextInputExtensions
{
    extension<TInput>(TInput input) where TInput : TextInputCore
    {
        /// <summary>Sets <see cref="TextInputCore.Text"/>.</summary>
        public TInput Text(string text)
        {
            input.Text = text;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.Placeholder"/>.</summary>
        public TInput Placeholder(string placeholder)
        {
            input.Placeholder = placeholder;
            return input;
        }

        /// <summary>Selects from <paramref name="start"/> to <paramref name="end"/> characters.</summary>
        public TInput Selection(int start, int end)
        {
            input.SetSelection(start, end);
            return input;
        }

        /// <summary>Puts the caret at a character position.</summary>
        public TInput Caret(int position)
        {
            input.SetCaret(position);
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.MaxChars"/>.</summary>
        public TInput MaxChars(int? limit)
        {
            input.MaxChars = limit;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.IsReadOnly"/>.</summary>
        public TInput ReadOnly(bool readOnly = true)
        {
            input.IsReadOnly = readOnly;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.IsPassword"/>.</summary>
        public TInput Password(bool password = true)
        {
            input.IsPassword = password;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.AcceptsTab"/>.</summary>
        public TInput AcceptsTab(bool acceptsTab = true)
        {
            input.AcceptsTab = acceptsTab;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.HostAutofill"/>.</summary>
        public TInput HostAutofill(string? hint)
        {
            input.HostAutofill = hint;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.FontFamily"/>.</summary>
        public TInput FontFamily(FontFamily? family)
        {
            input.FontFamily = family;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.FontSize"/>.</summary>
        public TInput FontSize(float? size)
        {
            input.FontSize = size;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.LineHeight"/>.</summary>
        public TInput LineHeight(float? lineHeight)
        {
            input.LineHeight = lineHeight;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.Template"/>.</summary>
        public TInput Template(TextInputTemplate? template)
        {
            input.Template = template;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.Colors"/>.</summary>
        public TInput Colors(TextInputColors? colors)
        {
            input.Colors = colors;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.Wrapping"/>.</summary>
        public TInput Wrapping(bool wrapping = true)
        {
            input.Wrapping = wrapping;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.VerticalScrollBarVisibility"/>.</summary>
        public TInput VerticalScrollBarVisibility(ScrollBarVisibility visibility)
        {
            input.VerticalScrollBarVisibility = visibility;
            return input;
        }

        /// <summary>Sets <see cref="TextInputCore.HorizontalScrollBarVisibility"/>.</summary>
        public TInput HorizontalScrollBarVisibility(ScrollBarVisibility visibility)
        {
            input.HorizontalScrollBarVisibility = visibility;
            return input;
        }

        /// <summary>Adds a handler to <see cref="TextInputCore.TextChanged"/>.</summary>
        public TInput OnTextChanged(Action<TextChangedEventArgs> handler)
        {
            input.TextChanged += handler;
            return input;
        }

        /// <summary>Adds a handler to <see cref="TextInputCore.SelectionChanged"/>.</summary>
        public TInput OnSelectionChanged(Action<SelectionChangedEventArgs> handler)
        {
            input.SelectionChanged += handler;
            return input;
        }
    }
}
