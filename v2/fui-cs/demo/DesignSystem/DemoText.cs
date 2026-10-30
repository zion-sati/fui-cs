namespace FuiCs.Demo.DesignSystem;

/// <summary>The demo's text roles, each with its color.</summary>
public enum DemoTextRecipe
{
    Body,
    Supporting,
    Hint,
    InverseLabel,
    PageTitle,
    SectionTitle,
    StatusValue,
    StatusSupporting,
    ListTitle,
    ListMeta,
    Accent,
    Error,
}

/// <summary>Resolves the demo's text recipes against a theme.</summary>
public static class DemoTextRecipes
{
    private static readonly Color InverseLabelColor = Color.FromRgb(0xf8, 0xfb, 0xff);

    public static Color ColorOf(Theme theme, DemoTextRecipe recipe) => recipe switch
    {
        DemoTextRecipe.Accent => theme.Colors.Accent,
        DemoTextRecipe.Error => theme.Colors.AccentPressed,
        DemoTextRecipe.Supporting or DemoTextRecipe.StatusSupporting => DemoColors.MutedText(theme),
        DemoTextRecipe.Hint or DemoTextRecipe.ListMeta => DemoColors.SubtleText(theme),
        DemoTextRecipe.InverseLabel => InverseLabelColor,
        _ => DemoColors.PrimaryText(theme),
    };

    public static void Apply(Text text, Theme theme, DemoTextRecipe recipe) => text.TextColor = ColorOf(theme, recipe);
}

/// <summary>Text in one of the demo's roles, recolored when the theme changes; it fades where it overflows.</summary>
public class DemoText : Text
{
    private DemoTextRecipe recipe;

    public DemoText(string content = "", DemoTextRecipe recipe = DemoTextRecipe.Body, bool selectable = true)
        : base(content)
    {
        this.recipe = recipe;
        Selectable = selectable;
        SetOverflowFade(true, true);
        Track(Theme.Bind(ApplyDemoTheme));
    }

    public DemoTextRecipe Recipe
    {
        get => recipe;
        set
        {
            recipe = value;
            ApplyDemoTheme(Theme.Active.Value);
        }
    }

    private void ApplyDemoTheme(Theme theme) => DemoTextRecipes.Apply(this, theme, recipe);
}

/// <summary>Fluent configuration for <see cref="DemoText"/>.</summary>
public static class DemoTextExtensions
{
    extension<TText>(TText text) where TText : DemoText
    {
        public TText Recipe(DemoTextRecipe recipe)
        {
            text.Recipe = recipe;
            return text;
        }
    }
}
