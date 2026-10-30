using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A node sized by a fixed length, by filling the space its parent offers, and by min/max limits.</summary>
public interface ISizable
{
    /// <summary>A fixed width; <see cref="Length.Auto"/> sizes to content. Setting it replaces any fill.</summary>
    Length Width { get; set; }

    /// <summary>A fixed height; <see cref="Length.Auto"/> sizes to content. Setting it replaces any fill.</summary>
    Length Height { get; set; }

    /// <summary>Takes the remaining space on a row's main axis, or the full width in a column. Replaces a fixed width.</summary>
    bool FillWidth { get; set; }

    /// <summary>Takes the remaining space on a column's main axis, or the full height in a row. Replaces a fixed height.</summary>
    bool FillHeight { get; set; }

    /// <summary>Takes this percentage of the remaining width; null when not set.</summary>
    float? FillWidthPercent { get; set; }

    /// <summary>Takes this percentage of the remaining height; null when not set.</summary>
    float? FillHeightPercent { get; set; }

    /// <summary>The smallest width layout gives the node; <see cref="Length.Auto"/> for no minimum.</summary>
    Length MinWidth { get; set; }

    /// <summary>The largest width layout gives the node; <see cref="Length.Auto"/> for no maximum.</summary>
    Length MaxWidth { get; set; }

    /// <summary>The smallest height layout gives the node; <see cref="Length.Auto"/> for no minimum.</summary>
    Length MinHeight { get; set; }

    /// <summary>The largest height layout gives the node; <see cref="Length.Auto"/> for no maximum.</summary>
    Length MaxHeight { get; set; }
}

/// <summary>The sizing state of one node: stored until build, sent to the native node once built.</summary>
internal sealed class NodeSize(Node owner)
{
    private Length width = Length.Auto;
    private bool hasWidth;
    private bool fillWidth;
    private float? fillWidthPercent;
    private Length height = Length.Auto;
    private bool hasHeight;
    private bool fillHeight;
    private float? fillHeightPercent;
    private Length minWidth = Length.Auto;
    private Length maxWidth = Length.Auto;
    private Length minHeight = Length.Auto;
    private Length maxHeight = Length.Auto;

    public Length Width
    {
        get => width;
        set
        {
            width = value;
            hasWidth = true;
            fillWidth = false;
            fillWidthPercent = null;
            if (owner.IsBuilt)
            {
                Ui.SetWidth(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public Length Height
    {
        get => height;
        set
        {
            height = value;
            hasHeight = true;
            fillHeight = false;
            fillHeightPercent = null;
            if (owner.IsBuilt)
            {
                Ui.SetHeight(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public bool FillWidth
    {
        get => fillWidth;
        set
        {
            if (value)
            {
                hasWidth = false;
                fillWidthPercent = null;
            }
            fillWidth = value;
            if (owner.IsBuilt)
            {
                Ui.SetFillWidth(owner.Handle, value);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public bool FillHeight
    {
        get => fillHeight;
        set
        {
            if (value)
            {
                hasHeight = false;
                fillHeightPercent = null;
            }
            fillHeight = value;
            if (owner.IsBuilt)
            {
                Ui.SetFillHeight(owner.Handle, value);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public float? FillWidthPercent
    {
        get => fillWidthPercent;
        set
        {
            fillWidthPercent = value;
            if (value is not { } percent)
            {
                return;
            }
            hasWidth = false;
            fillWidth = false;
            if (owner.IsBuilt)
            {
                Ui.SetFillWidthPercent(owner.Handle, percent);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public float? FillHeightPercent
    {
        get => fillHeightPercent;
        set
        {
            fillHeightPercent = value;
            if (value is not { } percent)
            {
                return;
            }
            hasHeight = false;
            fillHeight = false;
            if (owner.IsBuilt)
            {
                Ui.SetFillHeightPercent(owner.Handle, percent);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public Length MinWidth
    {
        get => minWidth;
        set
        {
            minWidth = value;
            if (owner.IsBuilt)
            {
                Ui.SetMinWidth(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public Length MaxWidth
    {
        get => maxWidth;
        set
        {
            maxWidth = value;
            if (owner.IsBuilt)
            {
                Ui.SetMaxWidth(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public Length MinHeight
    {
        get => minHeight;
        set
        {
            minHeight = value;
            if (owner.IsBuilt)
            {
                Ui.SetMinHeight(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    public Length MaxHeight
    {
        get => maxHeight;
        set
        {
            maxHeight = value;
            if (owner.IsBuilt)
            {
                Ui.SetMaxHeight(owner.Handle, value.Value, value.Unit);
                owner.NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <summary>Sends the sizes that were set to a newly created native node.</summary>
    public void Apply(ulong handle)
    {
        if (hasWidth)
        {
            Ui.SetWidth(handle, width.Value, width.Unit);
        }
        if (fillWidth)
        {
            Ui.SetFillWidth(handle, true);
        }
        if (fillWidthPercent is { } widthPercent)
        {
            Ui.SetFillWidthPercent(handle, widthPercent);
        }
        if (hasHeight)
        {
            Ui.SetHeight(handle, height.Value, height.Unit);
        }
        if (fillHeight)
        {
            Ui.SetFillHeight(handle, true);
        }
        if (fillHeightPercent is { } heightPercent)
        {
            Ui.SetFillHeightPercent(handle, heightPercent);
        }
        if (minWidth.Unit != Unit.Auto)
        {
            Ui.SetMinWidth(handle, minWidth.Value, minWidth.Unit);
        }
        if (maxWidth.Unit != Unit.Auto)
        {
            Ui.SetMaxWidth(handle, maxWidth.Value, maxWidth.Unit);
        }
        if (minHeight.Unit != Unit.Auto)
        {
            Ui.SetMinHeight(handle, minHeight.Value, minHeight.Unit);
        }
        if (maxHeight.Unit != Unit.Auto)
        {
            Ui.SetMaxHeight(handle, maxHeight.Value, maxHeight.Unit);
        }
    }

    /// <summary>For layout diagnostics: the percent size on a parent's main axis, or -1.</summary>
    public float MainAxisPercent(bool isHorizontal)
    {
        if (isHorizontal)
        {
            return hasWidth && width.Unit == Unit.Percent ? width.Value : -1.0f;
        }
        return hasHeight && height.Unit == Unit.Percent ? height.Value : -1.0f;
    }
}

/// <summary>Fluent sizing for every <see cref="ISizable"/> node or layout.</summary>
public static class SizableExtensions
{
    extension<TNode>(TNode node) where TNode : class, ISizable
    {
        /// <summary>Sets a fixed width in <paramref name="unit"/>.</summary>
        public TNode Width(float value, Unit unit = Unit.Pixel)
        {
            node.Width = new Length(value, unit);
            return node;
        }

        /// <summary>Sets <see cref="ISizable.Width"/>.</summary>
        public TNode Width(Length width)
        {
            node.Width = width;
            return node;
        }

        /// <summary>Sets a fixed height in <paramref name="unit"/>.</summary>
        public TNode Height(float value, Unit unit = Unit.Pixel)
        {
            node.Height = new Length(value, unit);
            return node;
        }

        /// <summary>Sets <see cref="ISizable.Height"/>.</summary>
        public TNode Height(Length height)
        {
            node.Height = height;
            return node;
        }

        /// <summary>Makes the node fill the width its parent offers.</summary>
        public TNode FillWidth()
        {
            node.FillWidth = true;
            return node;
        }

        /// <summary>Makes the node fill the height its parent offers.</summary>
        public TNode FillHeight()
        {
            node.FillHeight = true;
            return node;
        }

        /// <summary>Makes the node fill both directions.</summary>
        public TNode FillSize()
        {
            node.FillWidth = true;
            node.FillHeight = true;
            return node;
        }

        /// <summary>Makes the node take <paramref name="percent"/> of the remaining width.</summary>
        public TNode FillWidthPercent(float percent)
        {
            node.FillWidthPercent = percent;
            return node;
        }

        /// <summary>Makes the node take <paramref name="percent"/> of the remaining height.</summary>
        public TNode FillHeightPercent(float percent)
        {
            node.FillHeightPercent = percent;
            return node;
        }

        /// <summary>Sets the minimum width in <paramref name="unit"/>.</summary>
        public TNode MinWidth(float value, Unit unit = Unit.Pixel)
        {
            node.MinWidth = new Length(value, unit);
            return node;
        }

        /// <summary>Sets the maximum width in <paramref name="unit"/>.</summary>
        public TNode MaxWidth(float value, Unit unit = Unit.Pixel)
        {
            node.MaxWidth = new Length(value, unit);
            return node;
        }

        /// <summary>Sets the minimum height in <paramref name="unit"/>.</summary>
        public TNode MinHeight(float value, Unit unit = Unit.Pixel)
        {
            node.MinHeight = new Length(value, unit);
            return node;
        }

        /// <summary>Sets the maximum height in <paramref name="unit"/>.</summary>
        public TNode MaxHeight(float value, Unit unit = Unit.Pixel)
        {
            node.MaxHeight = new Length(value, unit);
            return node;
        }
    }
}
