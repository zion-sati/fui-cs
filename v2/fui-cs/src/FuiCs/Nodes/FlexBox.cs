using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A flexbox container with box styling: size, flex layout, padding and margin, background, border, corners,
/// opacity, blur, shadow and gradient. Properties only reach the native node once they are set.
/// </summary>
public class FlexBox : Node, ISizable, IContentContainer
{
    private const float MainAxisPercentFullThreshold = 99.99f;
    private const float MainAxisPercentOverflowThreshold = 100.01f;

    private readonly NodeSize size;
    private float flexBasis = float.NaN;
    private Color background;
    private bool hasBackground;
    private CornerRadius cornerRadius;
    private Border border;
    private bool hasBoxStyle;
    private FlexDirection flexDirection = FlexDirection.Column;
    private bool hasFlexDirection;
    private FlexWrap flexWrap = FlexWrap.NoWrap;
    private bool hasFlexWrap;
    private JustifyContent justifyContent = JustifyContent.Start;
    private bool hasJustifyContent;
    private AlignItems alignItems = AlignItems.Start;
    private bool hasAlignItems;
    private AlignSelf alignSelf = AlignSelf.Auto;
    private bool hasAlignSelf;
    private Thickness padding;
    private bool hasPadding;
    private Thickness margin;
    private bool hasMargin;
    private PositionType positionType = PositionType.Relative;
    private bool hasPositionType;
    private Point position;
    private bool hasPosition;
    private bool clipToBounds;
    private float opacity = 1.0f;
    private float blur;
    private bool hasLayerEffect;
    private DropShadow dropShadow;
    private bool hasDropShadow;
    private float backgroundBlur;
    private bool hasBackgroundBlur;
    private LinearGradient? linearGradient;
    private NodeTransitions? transitions;
    private Animation? opacityTransition;
    private Animation? backgroundTransition;
    private bool warnedFullMainAxisPercent;
    private bool warnedPercentOverflow;

    /// <summary>A row of <paramref name="children"/>.</summary>
    public static FlexBox Row(params ReadOnlySpan<Node> children) => Create(FlexDirection.Row, children);

    /// <summary>A column of <paramref name="children"/>.</summary>
    public static FlexBox Column(params ReadOnlySpan<Node> children) => Create(FlexDirection.Column, children);

    /// <summary>An empty box laying out its children in a column.</summary>
    public FlexBox() => size = new NodeSize(this);

    /// <inheritdoc/>
    public virtual Length Width
    {
        get => size.Width;
        set => size.Width = value;
    }

    /// <inheritdoc/>
    public virtual Length Height
    {
        get => size.Height;
        set => size.Height = value;
    }

    /// <inheritdoc/>
    public bool FillWidth
    {
        get => size.FillWidth;
        set => size.FillWidth = value;
    }

    /// <inheritdoc/>
    public bool FillHeight
    {
        get => size.FillHeight;
        set => size.FillHeight = value;
    }

    /// <inheritdoc/>
    public float? FillWidthPercent
    {
        get => size.FillWidthPercent;
        set => size.FillWidthPercent = value;
    }

    /// <inheritdoc/>
    public float? FillHeightPercent
    {
        get => size.FillHeightPercent;
        set => size.FillHeightPercent = value;
    }

    /// <inheritdoc/>
    public Length MinWidth
    {
        get => size.MinWidth;
        set => size.MinWidth = value;
    }

    /// <inheritdoc/>
    public Length MaxWidth
    {
        get => size.MaxWidth;
        set => size.MaxWidth = value;
    }

    /// <inheritdoc/>
    public Length MinHeight
    {
        get => size.MinHeight;
        set => size.MinHeight = value;
    }

    /// <inheritdoc/>
    public Length MaxHeight
    {
        get => size.MaxHeight;
        set => size.MaxHeight = value;
    }

    /// <summary>The initial main-axis size before flexing; NaN until set.</summary>
    public float FlexBasis
    {
        get => flexBasis;
        set
        {
            flexBasis = value;
            if (IsBuilt)
            {
                Ui.SetFlexBasis(Handle, value);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <summary>The fill color; on a built box with a <see cref="NodeTransitions.Background"/> timing, a change animates and this reads the current color.</summary>
    public virtual Color Background
    {
        get => background;
        set
        {
            backgroundTransition?.Cancel();
            if (transitions?.Background is { } timing && IsBuilt && background != value)
            {
                backgroundTransition = Animation.AnimateColor(background, value, timing, ApplyBackground);
                return;
            }
            ApplyBackground(value);
        }
    }

    /// <summary>Which property changes animate; null changes them at once.</summary>
    public virtual NodeTransitions? Transitions
    {
        get => transitions;
        set => transitions = value;
    }

    /// <summary>The radius of each corner.</summary>
    public virtual CornerRadius CornerRadius
    {
        get => cornerRadius;
        set
        {
            cornerRadius = value;
            hasBoxStyle = true;
            UpdateVisualStyle();
        }
    }

    /// <summary>The outline drawn around the box.</summary>
    public virtual Border Border
    {
        get => border;
        set
        {
            border = value;
            hasBoxStyle = true;
            UpdateVisualStyle();
        }
    }

    /// <summary>Whether children flow in a row or a column, and in which direction.</summary>
    public FlexDirection FlexDirection
    {
        get => flexDirection;
        set
        {
            flexDirection = value;
            hasFlexDirection = true;
            if (IsBuilt)
            {
                Ui.SetFlexDirection(Handle, value);
                NotifyRetainedLayoutMutation();
                OnChildLayoutChanged();
            }
        }
    }

    /// <summary>Whether children wrap onto more lines when they do not fit.</summary>
    public FlexWrap FlexWrap
    {
        get => flexWrap;
        set
        {
            flexWrap = value;
            hasFlexWrap = true;
            if (IsBuilt)
            {
                Ui.SetFlexWrap(Handle, value);
                NotifyRetainedLayoutMutation();
                OnChildLayoutChanged();
            }
        }
    }

    /// <summary>How children are spaced along the main axis.</summary>
    public JustifyContent JustifyContent
    {
        get => justifyContent;
        set
        {
            justifyContent = value;
            hasJustifyContent = true;
            if (IsBuilt)
            {
                Ui.SetJustifyContent(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>How children align across the main axis.</summary>
    public AlignItems AlignItems
    {
        get => alignItems;
        set
        {
            alignItems = value;
            hasAlignItems = true;
            if (IsBuilt)
            {
                Ui.SetAlignItems(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>How this box aligns in its parent, overriding the parent's AlignItems.</summary>
    public AlignSelf AlignSelf
    {
        get => alignSelf;
        set
        {
            alignSelf = value;
            hasAlignSelf = true;
            if (IsBuilt)
            {
                Ui.SetAlignSelf(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The space between the border and the children.</summary>
    public virtual Thickness Padding
    {
        get => padding;
        set
        {
            padding = value;
            hasPadding = true;
            if (IsBuilt)
            {
                Ui.SetPadding(Handle, value.Left, value.Top, value.Right, value.Bottom);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The space around the box.</summary>
    public Thickness Margin
    {
        get => margin;
        set
        {
            margin = value;
            hasMargin = true;
            if (IsBuilt)
            {
                Ui.SetMargin(Handle, value.Left, value.Top, value.Right, value.Bottom);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <summary>Whether the box takes part in its parent's flow or is placed at Position.</summary>
    public PositionType PositionType
    {
        get => positionType;
        set
        {
            positionType = value;
            hasPositionType = true;
            if (IsBuilt)
            {
                Ui.SetPositionType(Handle, value);
                NotifyRetainedLayoutMutation();
            }
        }
    }

    /// <summary>The left and top offsets of an absolutely positioned box.</summary>
    public Point Position
    {
        get => position;
        set
        {
            position = value;
            hasPosition = true;
            if (IsBuilt)
            {
                Ui.SetPosition(Handle, value.X, value.Y, float.NaN, float.NaN);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>Clips children to the box, as CSS's <c>overflow: hidden</c> does; off by default, so content and shadows may overflow.</summary>
    public bool ClipToBounds
    {
        get => clipToBounds;
        set
        {
            clipToBounds = value;
            if (IsBuilt)
            {
                Ui.SetClipToBounds(Handle, value);
                NotifyRetainedMutation();
            }
        }
    }

    /// <summary>The opacity of the box and its content; on a built box with a <see cref="NodeTransitions.Opacity"/> timing, a change animates and this reads the current value.</summary>
    public float Opacity
    {
        get => opacity;
        set
        {
            opacityTransition?.Cancel();
            if (transitions?.Opacity is { } timing && IsBuilt && opacity != value)
            {
                opacityTransition = Animation.AnimateFloat(opacity, value, timing, ApplyOpacity);
                return;
            }
            ApplyOpacity(value);
        }
    }

    /// <summary>The Gaussian blur sigma applied to the box and its content.</summary>
    public float Blur
    {
        get => blur;
        set
        {
            blur = value;
            hasLayerEffect = true;
            UpdateVisualStyle();
        }
    }

    /// <summary>The shadow drawn behind the box.</summary>
    public virtual DropShadow DropShadow
    {
        get => dropShadow;
        set
        {
            dropShadow = value;
            hasDropShadow = true;
            UpdateVisualStyle();
        }
    }

    /// <summary>The Gaussian blur sigma applied to what is behind the box.</summary>
    public virtual float BackgroundBlur
    {
        get => backgroundBlur;
        set
        {
            backgroundBlur = value;
            hasBackgroundBlur = true;
            UpdateVisualStyle();
        }
    }

    /// <summary>A gradient that fills the box in place of its background color; a gradient with no stops is ignored with a warning.</summary>
    public LinearGradient? LinearGradient
    {
        get => linearGradient;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Stops.Count == 0)
            {
                Logger.Warn("Layout", "FlexBox.LinearGradient received an empty stop list.");
                return;
            }
            linearGradient = value;
            UpdateVisualStyle();
        }
    }

    internal override bool DebugIsAbsolutelyPositioned => hasPositionType && positionType == PositionType.Absolute;

    /// <inheritdoc/>
    public virtual void AddContent(Node node) => AddChild(node);

    /// <inheritdoc/>
    public virtual void SetContent(params ReadOnlySpan<Node> nodes) => ReplaceChildren(nodes);

    /// <inheritdoc/>
    public override ulong Build() => BuildStyledNode(NodeType.FlexBox);

    /// <inheritdoc/>
    public override void Dispose()
    {
        opacityTransition?.Cancel();
        backgroundTransition?.Cancel();
        base.Dispose();
    }

    private void ApplyBackground(Color value)
    {
        background = value;
        hasBackground = true;
        hasBoxStyle = true;
        UpdateVisualStyle();
    }

    private void ApplyOpacity(float value)
    {
        opacity = value;
        hasLayerEffect = true;
        UpdateVisualStyle();
    }

    internal override float DebugMainAxisPercentValue(bool isHorizontal) => size.MainAxisPercent(isHorizontal);

    /// <summary>Creates the native node of <paramref name="type"/> with this box's metadata, layout and style.</summary>
    protected ulong BuildStyledNode(NodeType type, bool includeChildren = true)
    {
        if (IsBuilt)
        {
            return Handle;
        }
        Handle = Ui.CreateNode(type);
        ApplyNodeMetadata();
        FinishBuild();
        ApplyLayoutStyle();
        ApplyVisualStyle();
        EmitDeveloperLayoutWarnings();
        if (includeChildren)
        {
            BuildChildren();
        }
        return Handle;
    }

    /// <summary>Sends the box's layout properties to the native node, for a Build override.</summary>
    protected void ApplyLayoutStyle()
    {
        size.Apply(Handle);
        if (hasFlexDirection)
        {
            Ui.SetFlexDirection(Handle, flexDirection);
        }
        if (hasFlexWrap)
        {
            Ui.SetFlexWrap(Handle, flexWrap);
        }
        if (!float.IsNaN(flexBasis))
        {
            Ui.SetFlexBasis(Handle, flexBasis);
        }
        if (hasJustifyContent)
        {
            Ui.SetJustifyContent(Handle, justifyContent);
        }
        if (hasAlignItems)
        {
            Ui.SetAlignItems(Handle, alignItems);
        }
        if (hasAlignSelf)
        {
            Ui.SetAlignSelf(Handle, alignSelf);
        }
        if (hasMargin)
        {
            Ui.SetMargin(Handle, margin.Left, margin.Top, margin.Right, margin.Bottom);
        }
        if (hasPadding)
        {
            Ui.SetPadding(Handle, padding.Left, padding.Top, padding.Right, padding.Bottom);
        }
        if (hasPositionType)
        {
            Ui.SetPositionType(Handle, positionType);
        }
        if (hasPosition)
        {
            Ui.SetPosition(Handle, position.X, position.Y, float.NaN, float.NaN);
        }
        Ui.SetClipToBounds(Handle, clipToBounds);
    }

    /// <summary>Sends the box's visual properties to the native node, for a Build override.</summary>
    protected void ApplyVisualStyle()
    {
        if (hasBoxStyle)
        {
            Ui.SetBoxStyle(
                Handle,
                background.Rgba,
                cornerRadius.TopLeft,
                cornerRadius.TopRight,
                cornerRadius.BottomRight,
                cornerRadius.BottomLeft,
                border.Width,
                border.Color.Rgba,
                border.Style,
                border.DashOn,
                border.DashOff);
        }
        else if (hasBackground)
        {
            Ui.SetBackgroundColor(Handle, background.Rgba);
        }
        if (hasLayerEffect)
        {
            Ui.SetLayerEffect(Handle, opacity, blur, 0);
        }
        if (hasDropShadow)
        {
            Ui.SetDropShadow(Handle, dropShadow.Color.Rgba, dropShadow.OffsetX, dropShadow.OffsetY, dropShadow.BlurSigma, dropShadow.Spread);
        }
        if (hasBackgroundBlur)
        {
            Ui.SetBackgroundBlur(Handle, backgroundBlur);
        }
        if (linearGradient is { } gradient)
        {
            var offsets = new float[gradient.Stops.Count];
            var colors = new uint[gradient.Stops.Count];
            for (var index = 0; index < offsets.Length; ++index)
            {
                offsets[index] = gradient.Stops[index].Offset;
                colors[index] = gradient.Stops[index].Color.Rgba;
            }
            Ui.SetLinearGradient(Handle, gradient.StartX, gradient.StartY, gradient.EndX, gradient.EndY, offsets, colors);
        }
    }

    /// <summary>Whether this container checks its children's percent sizes for likely layout mistakes.</summary>
    protected virtual bool UsesFlexChildLayoutDiagnostics => true;

    /// <inheritdoc/>
    protected override void OnChildLayoutChanged() => EmitDeveloperLayoutWarnings();

    private static FlexBox Create(FlexDirection direction, ReadOnlySpan<Node> children)
    {
        var box = new FlexBox { FlexDirection = direction };
        box.SetContent(children);
        return box;
    }

    private void UpdateVisualStyle()
    {
        if (IsBuilt)
        {
            ApplyVisualStyle();
            NotifyRetainedMutation();
        }
    }

    /// <summary>
    /// Warns once when a child of several uses a 100% main-axis size, or when main-axis percentages exceed 100%:
    /// percent is parent-relative sizing, not flex sharing.
    /// </summary>
    private void EmitDeveloperLayoutWarnings()
    {
        if (!UsesFlexChildLayoutDiagnostics)
        {
            return;
        }
        var isRow = flexDirection == FlexDirection.Row;
        var inFlowChildCount = 0;
        var fullMainAxisPercentChildCount = 0;
        var mainAxisPercentTotal = 0.0f;
        Node? firstFullChild = null;
        var firstFullChildIndex = -1;
        for (var index = 0; index < Children.Count; ++index)
        {
            var child = Children[index];
            if (child.DebugIsAbsolutelyPositioned)
            {
                continue;
            }
            inFlowChildCount += 1;
            var percent = child.DebugMainAxisPercentValue(isRow);
            if (percent < 0.0f)
            {
                continue;
            }
            mainAxisPercentTotal += percent;
            if (percent >= MainAxisPercentFullThreshold)
            {
                fullMainAxisPercentChildCount += 1;
                if (firstFullChild is null)
                {
                    firstFullChild = child;
                    firstFullChildIndex = index;
                }
            }
        }
        if (inFlowChildCount < 2)
        {
            return;
        }
        if (fullMainAxisPercentChildCount > 0)
        {
            if (!warnedFullMainAxisPercent)
            {
                warnedFullMainAxisPercent = true;
                Logger.Warn("Layout", FullMainAxisPercentWarning(isRow) + WarningContext(firstFullChild, firstFullChildIndex));
            }
            return;
        }
        if (mainAxisPercentTotal > MainAxisPercentOverflowThreshold && !warnedPercentOverflow)
        {
            warnedPercentOverflow = true;
            Logger.Warn("Layout", PercentOverflowWarning(isRow) + WarningContext(null, -1));
        }
    }

    private static string FullMainAxisPercentWarning(bool isRow) => isRow
        ? "A row container has an in-flow child using Width = Length.Percent(100) alongside siblings. Percent is literal parent-relative sizing, not flex sharing. Use FillWidth when the child should take remaining row space."
        : "A column container has an in-flow child using Height = Length.Percent(100) alongside siblings. Percent is literal parent-relative sizing, not flex sharing. Use FillHeight when the child should take remaining column space.";

    private static string PercentOverflowWarning(bool isRow) => isRow
        ? "A row container has in-flow children whose explicit width percentages exceed 100% in total. Percent is literal parent-relative sizing, not flex sharing. Use FillWidth for the child that should expand, or reduce the percentages so they fit."
        : "A column container has in-flow children whose explicit height percentages exceed 100% in total. Percent is literal parent-relative sizing, not flex sharing. Use FillHeight for the child that should expand, or reduce the percentages so they fit.";

    private string WarningContext(Node? child, int childIndex)
    {
        var context = " [containerPath=" + DebugTreePath();
        if (child is not null)
        {
            context += ", childPath=" + child.DebugTreePath();
        }
        if (!string.IsNullOrEmpty(NodeId))
        {
            context += ", containerNodeId=" + NodeId;
        }
        if (!string.IsNullOrEmpty(child?.NodeId))
        {
            context += ", childNodeId=" + child.NodeId;
        }
        if (childIndex >= 0)
        {
            context += ", childIndex=" + childIndex;
        }
        return context + "]";
    }
}
