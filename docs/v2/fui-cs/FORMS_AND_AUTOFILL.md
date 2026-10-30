# FUI-CS Forms and Autofill

## Default and cancel buttons

A `Form` gives a page the Enter and Escape keys of a dialog. Set
`DefaultButton` or `CancelButton` (or both), then `Activate()` the form; a
form with neither does not activate. While it is active, Enter presses the default button and Escape the cancel button, as WPF's
`IsDefault` and `IsCancel` do. A focused button takes Enter itself. The most
recently activated form wins; `Deactivate()` stops it, and disposing it does
too.

<!-- example: Action Save, Action Close, TextInput nameField -->
```csharp
var save = new Button("Save").OnClick(_ => Save());
var cancel = new Button("Cancel").OnClick(_ => Close());
var form = new Form().DefaultButton(save).CancelButton(cancel)
    .Child(nameField)
    .Child(FlexBox.Row(save, cancel));
form.Activate();
```

`Dialog` uses a form of its own for OK and Cancel.

## Autofill and password managers

Browsers and password managers fill real form fields; an application's fields
are drawn on a canvas. The host bridges the two: it projects each text input
that has a `HostAutofill` hint, inside a `Form`, into a field of a real HTML
form while the input is on screen. The projected field takes:

- its `autocomplete` from the hint (`username`, `current-password`,
  `new-password`, `email`, ...);
- its `name` and `id` from the input's `NodeId` (the nearest one at or above the
  editor);
- its type from `IsPassword`;
- its accessible label from the input's label.

Values the browser or a password manager fills flow back into the inputs, and
the projected fields stand in for the inputs in the accessibility tree, so
assistive technology sees each field once.

<!-- example: Dialog dialog -->
```csharp
var username = new TextInput().Placeholder("Username or email")
    .HostAutofill("username").NodeId("username");
var password = new TextInput().Placeholder("Password").Password()
    .HostAutofill("current-password").NodeId("current-password");
dialog.BodyContent = new Form().Child(FlexBox.Column(username, password));
dialog.Shown += () => username.Focus();
```

Give each field a stable `NodeId`: password managers key what they save by the
field names. Clear the fields when the form opens if they should not keep
earlier values.

When an input with a hint is focused, the browser's hidden editor that takes
the keystrokes carries the same type, hint and name.

A password field's text is never persisted across history navigation.

## Manual checks

Browser extensions cannot be automated. Check a sign-in form by hand with the
password managers you support: that they offer to fill both fields, fill them,
and offer to save what was submitted.
