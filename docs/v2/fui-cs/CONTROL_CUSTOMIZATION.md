# FUI-CS Control Customization and Templates

Controls can be restyled at four levels, from the lightest touch to a
completely new look:

1. **Chrome properties** on the control: `Background`, `HoverBackground`,
   `TextColor`, `Padding`, `FontSize`, ...
2. **Colors and sizing records**, shared across controls: `ButtonColors`,
   `LabeledControlColors`, `SliderColors`, `DropdownColors`,
   `TextInputColors`; `LabeledControlSizing`, `SliderSizing`, `DropdownSizing`.
3. **Templates**, as WPF's control templates: a factory that builds the
   control's visual parts, and a presenter that restyles them for each state.
4. **Custom controls**, built from nodes and events.

The [Theming and style matrix](./THEMING_STYLE_MATRIX.md) lists what each
control takes from the theme and in what order the levels apply.

## Colors and sizing records

Each record's members are nullable; an unset member follows the theme or the
default template's size. Records are immutable, so share one across controls
and derive variants with `with`:

<!-- example: Checkbox checkbox, RadioButton radio, Slider slider -->
```csharp
var calm = new LabeledControlColors { Accent = 0x0EA5E9FF, Border = 0x0369A1FF };
checkbox.Colors = calm;
radio.Colors = calm with { Accent = 0x22C55EFF };
slider.Colors = new SliderColors { Track = 0x1E293BFF, Fill = 0x0EA5E9FF };
```

`ButtonColors` falls back from pressed to hovered to rest: an unset
`BackgroundPressed` uses `BackgroundHover`, then `Background`.

## Templates

| Control | Template property | Presenter | Its `Apply` receives |
|---|---|---|---|
| `Button` | `Template` (`ButtonTemplate`) | `ButtonPresenter`: `ContentRoot`, `Label` | `ButtonVisualState`, `ButtonColors` |
| `Checkbox`, `RadioButton`, `Switch` | `Template` (`IndicatorTemplate`) | `IndicatorPresenter`: `Root` | `IndicatorVisualState`, `LabeledControlColors` |
| `Slider` | `Template` (`SliderTemplate`) | `SliderPresenter`: `Root`, `ThumbSize`; `Layout(state, length)` places the parts | `SliderVisualState`, `SliderColors` |
| `TextInput`, `TextArea` | `Template` (`TextInputTemplate`) | `TextInputPresenter`: `Host`, `EditorHost`, `PlaceholderHost` | `TextInputVisualState`, `TextInputColors` |
| `Dropdown` | `FieldTemplate`, `ChevronTemplate`, `OptionRowTemplate` | `DropdownFieldPresenter`, `DropdownChevronPresenter`, `DropdownOptionRowPresenter` | the field, chevron and row states, `DropdownColors` |
| `ComboBox` | `ChevronTemplate`, `OptionRowTemplate` | as the dropdown's | as the dropdown's |

A template wraps a factory. Templates that depend on size receive the
control's sizing record (`LabeledControlSizing`, `SliderSizing`,
`DropdownSizing`); the rest take none. Setting a template replaces the
control's presenter with one from the factory, and the control calls the
presenter's `Apply` with the active theme, the current state and the control's
colors record whenever any of them change. Each control type has a `DefaultTemplate` (or the template type a
`Default`), which a null template returns to.

The control keeps its behavior: input, focus, keyboard handling, semantics
and state all stay with the control. A template decides only what the parts
look like and where they sit.

### Example: a rounded checkbox

```csharp
sealed class RoundCheckPresenter : IndicatorPresenter
{
    private readonly FlexBox dot = new FlexBox().Width(10.0f).Height(10.0f).CornerRadius(5.0f);

    public RoundCheckPresenter()
        : base(new FlexBox().Width(22.0f).Height(22.0f).CornerRadius(11.0f)
            .AlignItems(AlignItems.Center).JustifyContent(JustifyContent.Center))
    {
        Root.Child(dot);
    }

    public override void Apply(Theme theme, IndicatorVisualState state, LabeledControlColors? colors)
    {
        var accent = colors?.Accent ?? (state.IsHovered ? theme.Colors.AccentHovered : theme.Colors.Accent);
        var on = state.IsChecked != false;
        Root.Border(2.0f, on ? accent : colors?.Border ?? theme.Colors.Border)
            .Background(colors?.Background ?? theme.Colors.Surface);
        dot.Background(accent).Opacity(on ? 1.0f : 0.0f);
    }
}

var roundCheck = new IndicatorTemplate(_ => new RoundCheckPresenter());
var checkbox = new Checkbox("Remember me").Template(roundCheck);
```

A presenter should take every color from the colors record first and the
theme second, as above, so the record keeps working with the template.

The demo's templated controls route (`demo/TemplatedControls/`) builds a
house style for buttons, indicators, a switch and a slider this way.

## Custom controls

A control of your own is a node subclass, usually of `FlexBox`, that builds its
parts in its constructor and reacts to input:

- Subscribe to the node's own events (`PointerClick`, `KeyDown`,
  `FocusChanged`), or override the handlers (`protected override bool
  HandleKeyEvent`, `HandleFocusChanged`, `HandlePointerEvent`); the base
  handlers raise the events, so call them first.
- `Focusable(true)` makes it a tab stop; `CapturePointer()` and
  `ReleasePointer()` keep a drag's moves on it.
- Override `OnEffectiveEnabledChanged` and `OnEffectiveVisibilityChanged` to
  follow its ancestors' state.
- `Track(Theme.Active.Subscribe(_ => Refresh()))` restyles it when the theme
  changes, and releases the subscription when it is disposed.
- Set `SemanticRole`, `SemanticLabel` and the other semantics so assistive
  technology can use it (see [Accessibility](./ACCESSIBILITY_AND_SEMANTICS.md)).
- Keep its state across history navigation: give it a `NodeId` and persist a
  value with `PersistState(new PersistedValue<Rating, int>("stars", r => r.Stars, (r, v) => r.Stars = v, PersistedCodecs.Int32))`,
  or override `CapturePersistedState` and `RestorePersistedState`, calling the
  base methods.

For example, a star rating that the arrow keys change:

```csharp
public class Rating : FlexBox
{
    private readonly Text stars = new("");
    private int value;

    public Rating()
    {
        FlexDirection = FuiCs.FlexDirection.Row;
        SemanticRole = FuiCs.SemanticRole.Slider;
        this.Child(stars);
        this.Focusable(true);
        Refresh();
    }

    public event Action<int>? Changed;

    public int Stars
    {
        get => value;
        set
        {
            this.value = Math.Clamp(value, 0, 5);
            Refresh();
        }
    }

    protected override bool HandleKeyEvent(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        var handled = base.HandleKeyEvent(eventType, key, modifiers);
        if (eventType != KeyEventType.Down || key is not ("ArrowLeft" or "ArrowRight"))
        {
            return handled;
        }
        Stars += key == "ArrowRight" ? 1 : -1;
        Changed?.Invoke(Stars);
        return true;
    }

    private void Refresh()
    {
        stars.Text = new string('★', value) + new string('☆', 5 - value);
        SemanticLabel = value + " of 5 stars";
        SemanticValueRange = new SemanticValueRange(value, 0, 5);
    }
}
```

Give it fluent members in an extension block, generic over the control type,
as the SDK does; leave the control unsealed so the generic constraint can
name it:

```csharp
public static class RatingExtensions
{
    extension<TRating>(TRating rating) where TRating : Rating
    {
        public TRating Stars(int stars)
        {
            rating.Stars = stars;
            return rating;
        }
    }
}
```
