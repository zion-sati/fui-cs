namespace FuiCs;

/// <summary>
/// Display text: selectable by default, and announced as static text labelled with its content unless an
/// ancestor already has a semantic role.
/// </summary>
public class Text : TextCore
{
    /// <summary>Selectable text showing <paramref name="text"/>.</summary>
    public Text(string text = "")
        : base(text)
    {
        Selectable = true;
    }

    /// <inheritdoc/>
    public override string? SemanticLabel
    {
        get => base.SemanticLabel;
        set
        {
            if (!HasExplicitSemanticRole)
            {
                DefaultSemanticRole = FuiCs.SemanticRole.StaticText;
            }
            base.SemanticLabel = value;
        }
    }

    /// <summary>Fades text that overflows its box horizontally and/or vertically.</summary>
    public void SetOverflowFade(bool horizontal = true, bool vertical = false) => ApplyOverflowFade(horizontal, vertical);

    /// <inheritdoc/>
    public override ulong Build()
    {
        SyncDefaultSemantics();
        return base.Build();
    }

    /// <inheritdoc/>
    protected override void OnContentChanged() => SyncDefaultSemantics();

    /// <inheritdoc/>
    protected override void OnParentChanged() => SyncDefaultSemantics();

    private void SyncDefaultSemantics()
    {
        if (HasExplicitSemanticRole)
        {
            DefaultSemanticLabel = null;
            DefaultSemanticRole = FuiCs.SemanticRole.None;
            return;
        }
        if (HasExplicitSemanticLabel)
        {
            DefaultSemanticLabel = null;
            DefaultSemanticRole = FuiCs.SemanticRole.StaticText;
            return;
        }
        if (!ShouldApplyDefaultSemantics())
        {
            DefaultSemanticLabel = null;
            DefaultSemanticRole = FuiCs.SemanticRole.None;
            return;
        }
        DefaultSemanticRole = FuiCs.SemanticRole.StaticText;
        DefaultSemanticLabel = Text;
    }

    /// <summary>Non-empty text whose ancestors have no semantic role of their own.</summary>
    private bool ShouldApplyDefaultSemantics()
    {
        if (Text.Length == 0)
        {
            return false;
        }
        for (var ancestor = Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor.SemanticRole != FuiCs.SemanticRole.None)
            {
                return false;
            }
        }
        return true;
    }
}
