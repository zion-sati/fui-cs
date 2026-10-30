namespace FuiCs.Demo.DesignSystem;

public enum DemoButtonTone
{
    Primary,
    Secondary,
}

/// <summary>A button in the demo's primary or secondary tone: rounded and roomy, recolored when the theme changes.</summary>
public class DemoButton : Button
{
    private DemoButtonTone tone;

    public DemoButton(string label, DemoButtonTone tone = DemoButtonTone.Secondary)
        : base(label)
    {
        this.tone = tone;
        CornerRadius = new CornerRadius(DemoTokens.ButtonRadius);
        Padding = new Thickness(DemoTokens.ButtonPaddingX, DemoTokens.ButtonPaddingY);
        FontSize = 16.0f;
        Track(Theme.Bind(ApplyDemoTheme));
    }

    public DemoButtonTone Tone
    {
        get => tone;
        set
        {
            tone = value;
            ApplyDemoTheme(Theme.Active.Value);
        }
    }

    private void ApplyDemoTheme(Theme theme) =>
        DemoRecipes.Apply(this, theme, tone == DemoButtonTone.Primary ? DemoButtonRecipe.Primary : DemoButtonRecipe.Secondary);
}

/// <summary>Fluent configuration for <see cref="DemoButton"/>.</summary>
public static class DemoButtonExtensions
{
    extension<TButton>(TButton button) where TButton : DemoButton
    {
        public TButton Tone(DemoButtonTone tone)
        {
            button.Tone = tone;
            return button;
        }
    }
}
