namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Dialog"/> and <see cref="Form"/>.</summary>
public static class DialogExtensions
{
    extension<TDialog>(TDialog dialog) where TDialog : Dialog
    {
        /// <summary>Sets <see cref="Dialog.Title"/>.</summary>
        public TDialog Title(string title)
        {
            dialog.Title = title;
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.Body"/>.</summary>
        public TDialog Body(string body)
        {
            dialog.Body = body;
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.BodyContent"/>.</summary>
        public TDialog BodyContent(Node? content)
        {
            dialog.BodyContent = content;
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.BackdropColor"/>.</summary>
        public TDialog BackdropColor(Color color)
        {
            dialog.BackdropColor = color;
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.CardColor"/>.</summary>
        public TDialog CardColor(Color color)
        {
            dialog.CardColor = color;
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.CardBorder"/>.</summary>
        public TDialog CardBorder(Border border)
        {
            dialog.CardBorder = border;
            return dialog;
        }

        /// <summary>Rounds the card's corners.</summary>
        public TDialog CardCornerRadius(float radius)
        {
            dialog.CardCornerRadius = new CornerRadius(radius);
            return dialog;
        }

        /// <summary>Sets <see cref="Dialog.CardShadow"/>.</summary>
        public TDialog CardShadow(DropShadow shadow)
        {
            dialog.CardShadow = shadow;
            return dialog;
        }

        /// <summary>Adds a handler to <see cref="Dialog.Accepted"/>.</summary>
        public TDialog OnAccepted(Action handler)
        {
            dialog.Accepted += handler;
            return dialog;
        }

        /// <summary>Adds a handler to <see cref="Dialog.Cancelled"/>.</summary>
        public TDialog OnCancelled(Action handler)
        {
            dialog.Cancelled += handler;
            return dialog;
        }

        /// <summary>Adds a handler to <see cref="Dialog.Shown"/>.</summary>
        public TDialog OnShown(Action handler)
        {
            dialog.Shown += handler;
            return dialog;
        }
    }

    extension<TForm>(TForm form) where TForm : Form
    {
        /// <summary>Sets <see cref="Form.DefaultButton"/>.</summary>
        public TForm DefaultButton(Button? button)
        {
            form.DefaultButton = button;
            return form;
        }

        /// <summary>Sets <see cref="Form.CancelButton"/>.</summary>
        public TForm CancelButton(Button? button)
        {
            form.CancelButton = button;
            return form;
        }
    }
}
