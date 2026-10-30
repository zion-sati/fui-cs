using FuiCs.Bindings;

namespace FuiCs;

/// <summary>An accessibility value range: the current value and its bounds.</summary>
public readonly record struct SemanticValueRange(float Now, float Min, float Max);

public abstract partial class Node
{
    private SemanticRole? semanticRole;
    private SemanticRole defaultSemanticRole = FuiCs.SemanticRole.None;
    private string? semanticLabel;
    private string? defaultSemanticLabel;
    private SemanticCheckedState semanticChecked = SemanticCheckedState.None;
    private bool? semanticSelected;
    private bool? semanticExpanded;
    private bool? semanticDisabled;
    private bool trackSemanticDisabledFromEnabled;
    private SemanticValueRange? semanticValueRange;
    private Orientation semanticOrientation = Orientation.None;

    /// <summary>The accessibility role: the role set here, otherwise the node's default role.</summary>
    public SemanticRole SemanticRole
    {
        get => semanticRole ?? defaultSemanticRole;
        set
        {
            semanticRole = value;
            if (IsBuilt)
            {
                ApplySemanticRole();
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The accessibility label: the label set here, otherwise the node's default label.</summary>
    public virtual string? SemanticLabel
    {
        get => semanticLabel ?? defaultSemanticLabel;
        set
        {
            semanticLabel = value;
            if (IsBuilt)
            {
                ApplySemanticLabel();
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The checked state assistive technology reports.</summary>
    public SemanticCheckedState SemanticChecked
    {
        get => semanticChecked;
        set
        {
            semanticChecked = value;
            if (IsBuilt)
            {
                Ui.SetSemanticChecked(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The accessibility disabled state; null leaves it unset. Setting it stops tracking <see cref="IsEnabled"/>.</summary>
    public bool? SemanticDisabled
    {
        get => semanticDisabled;
        set
        {
            trackSemanticDisabledFromEnabled = false;
            semanticDisabled = value;
            if (IsBuilt)
            {
                Ui.SetSemanticDisabled(Handle, value.HasValue, value ?? false);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The accessibility selected state; null leaves it unset.</summary>
    public bool? SemanticSelected
    {
        get => semanticSelected;
        set
        {
            semanticSelected = value;
            if (IsBuilt)
            {
                Ui.SetSemanticSelected(Handle, value.HasValue, value ?? false);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The accessibility expanded state; null leaves it unset.</summary>
    public bool? SemanticExpanded
    {
        get => semanticExpanded;
        set
        {
            semanticExpanded = value;
            if (IsBuilt)
            {
                Ui.SetSemanticExpanded(Handle, value.HasValue, value ?? false);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The accessibility value range; null leaves it unset.</summary>
    public SemanticValueRange? SemanticValueRange
    {
        get => semanticValueRange;
        set
        {
            semanticValueRange = value;
            if (IsBuilt)
            {
                ApplySemanticValueRange();
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The orientation assistive technology reports, as for a slider or scroll bar.</summary>
    public Orientation SemanticOrientation
    {
        get => semanticOrientation;
        set
        {
            semanticOrientation = value;
            if (IsBuilt)
            {
                Ui.SetSemanticOrientation(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Whether a role was set explicitly rather than by the node's type.</summary>
    protected bool HasExplicitSemanticRole => semanticRole is not null;

    /// <summary>Whether a label was set explicitly rather than by the node's type.</summary>
    protected bool HasExplicitSemanticLabel => semanticLabel is not null;

    /// <summary>The role used when none is set explicitly; <c>SemanticRole.None</c> means no default.</summary>
    protected SemanticRole DefaultSemanticRole
    {
        get => defaultSemanticRole;
        set
        {
            if (defaultSemanticRole == value)
            {
                return;
            }
            defaultSemanticRole = value;
            if (IsBuilt && semanticRole is null)
            {
                ApplySemanticRole();
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The label used when none is set explicitly; null means no default.</summary>
    protected string? DefaultSemanticLabel
    {
        get => defaultSemanticLabel;
        set
        {
            if (defaultSemanticLabel == value)
            {
                return;
            }
            defaultSemanticLabel = value;
            if (IsBuilt && semanticLabel is null)
            {
                ApplySemanticLabel();
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Asks assistive technology to announce this node.</summary>
    public void RequestSemanticAnnouncement()
    {
        if (IsBuilt)
        {
            Ui.RequestSemanticAnnouncement(Handle);
        }
    }

    /// <summary>Keeps <see cref="SemanticDisabled"/> equal to the inverse of <see cref="IsEnabled"/>.</summary>
    protected void ReflectSemanticDisabledFromEnabled()
    {
        trackSemanticDisabledFromEnabled = true;
        semanticDisabled = !IsEnabled;
        if (IsBuilt)
        {
            Ui.SetSemanticDisabled(Handle, true, !IsEnabled);
            NotifyRetainedMutation();
        }
    }

    private void ApplySemanticMetadata()
    {
        if (semanticRole is not null || defaultSemanticRole != FuiCs.SemanticRole.None)
        {
            ApplySemanticRole();
        }
        if (semanticLabel is not null || defaultSemanticLabel is not null)
        {
            ApplySemanticLabel();
        }
        Ui.SetSemanticChecked(Handle, semanticChecked);
        Ui.SetSemanticSelected(Handle, semanticSelected.HasValue, semanticSelected ?? false);
        Ui.SetSemanticExpanded(Handle, semanticExpanded.HasValue, semanticExpanded ?? false);
        Ui.SetSemanticDisabled(Handle, semanticDisabled.HasValue, semanticDisabled ?? false);
        ApplySemanticValueRange();
        Ui.SetSemanticOrientation(Handle, semanticOrientation);
    }

    private void ApplySemanticRole() => Ui.SetSemanticRole(Handle, SemanticRole);

    private void ApplySemanticLabel() => Ui.SetSemanticLabel(Handle, semanticLabel ?? defaultSemanticLabel ?? "");

    private void ApplySemanticValueRange()
    {
        var range = semanticValueRange.GetValueOrDefault();
        Ui.SetSemanticValueRange(Handle, semanticValueRange.HasValue, range.Now, range.Min, range.Max);
    }
}
