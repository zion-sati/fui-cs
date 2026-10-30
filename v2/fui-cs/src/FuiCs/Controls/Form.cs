namespace FuiCs;

/// <summary>
/// A column grouping a form's fields. While active, Enter presses <see cref="DefaultButton"/> and Escape presses
/// <see cref="CancelButton"/>, as WPF's IsDefault and IsCancel buttons do; Enter on a focused button presses that
/// button instead.
/// </summary>
public class Form : FlexBox
{
    private IDisposable? keyFilter;
    private string? armedKey;
    private Button? armedButton;

    /// <summary>An empty form laying out its fields in a column.</summary>
    public Form()
    {
        FlexDirection = FuiCs.FlexDirection.Column;
        SemanticRole = FuiCs.SemanticRole.Form;
    }

    /// <summary>The button Enter presses while the form is active.</summary>
    public Button? DefaultButton { get; set; }

    /// <summary>The button Escape presses while the form is active.</summary>
    public Button? CancelButton { get; set; }

    /// <summary>Whether the form is handling Enter and Escape.</summary>
    public bool IsActive => keyFilter is not null;

    /// <summary>Starts handling Enter and Escape for the whole page; the most recently activated form wins.</summary>
    public void Activate()
    {
        if (keyFilter is null && (DefaultButton is not null || CancelButton is not null))
        {
            keyFilter = EventRouter.PushKeyFilter(HandleKey);
        }
    }

    /// <summary>Stops handling Enter and Escape, releasing a key press in progress.</summary>
    public void Deactivate()
    {
        var button = armedButton;
        armedKey = null;
        armedButton = null;
        button?.EndPress(fire: false);
        keyFilter?.Dispose();
        keyFilter = null;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Deactivate();
        base.Dispose();
    }

    private bool HandleKey(KeyEventType eventType, string key, KeyModifier modifiers)
    {
        if (key == "Enter" && modifiers == KeyModifier.None && EventRouter.FocusedNode is Button { IsEnabled: true })
        {
            return false;
        }
        if (eventType == KeyEventType.Down)
        {
            var button = key switch
            {
                "Enter" => DefaultButton,
                "Escape" => CancelButton,
                _ => null,
            };
            if (button is null)
            {
                return false;
            }
            if (armedKey is not null)
            {
                return armedKey == key;
            }
            armedKey = key;
            armedButton = button;
            button.BeginPress();
            return true;
        }
        if (eventType == KeyEventType.Up && armedKey == key)
        {
            var button = armedButton;
            armedKey = null;
            armedButton = null;
            button?.EndPress(fire: true);
            return true;
        }
        return false;
    }
}
