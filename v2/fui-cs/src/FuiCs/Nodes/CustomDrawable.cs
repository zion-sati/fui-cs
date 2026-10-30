using FuiCs.Bindings;
using FuiCs.Hosting;

namespace FuiCs;

/// <summary>
/// A box that draws itself with a <see cref="DrawContext"/>, as WPF elements do in <c>OnRender</c>. It keeps FlexBox
/// layout and children, but the host paints the drawing in place of the box style: background, border and shadow
/// are not painted, so draw them in <see cref="OnRender"/>. The host calls <see cref="OnRender"/> when it paints
/// the node: once after it is first shown, then after each <see cref="InvalidateVisual"/>; there is no continuous
/// repaint.
/// </summary>
public abstract class CustomDrawable : FlexBox
{
    static CustomDrawable()
    {
        HostEvents.CustomDraw = Dispatch;
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        BuildStyledNode(NodeType.FlexBox);
        Ui.SetCustomDrawable(Handle, true);
        return Handle;
    }

    /// <summary>Requests a repaint on the next frame. A node that is not visible on screen is not repainted.</summary>
    public void InvalidateVisual()
    {
        if (IsBuilt && (!Ui.TryGetVisibleBounds(Handle, out var visible) || visible.Width <= 0.0f || visible.Height <= 0.0f))
        {
            return;
        }
        FrameScheduler.MarkNeedsCommit();
    }

    /// <summary>Draws the node. Coordinates are relative to its top-left corner, clipped to its rounded bounds.</summary>
    protected abstract void OnRender(DrawContext context);

    /// <summary>Renders the custom node with this handle into the host canvas the render pass is drawing.</summary>
    internal static void Dispatch(ulong handle, uint canvas)
    {
        if (EventRouter.ResolveNode(handle) is not CustomDrawable node)
        {
            return;
        }
        var bounds = node.GetBounds();
        var context = new DrawContext(canvas);
        context.Save();
        var radius = node.CornerRadius;
        if (radius.TopLeft > 0.0f || radius.TopRight > 0.0f || radius.BottomRight > 0.0f || radius.BottomLeft > 0.0f)
        {
            context.ClipRoundRect(0.0f, 0.0f, bounds.Width, bounds.Height, radius);
        }
        else
        {
            context.ClipRect(0.0f, 0.0f, bounds.Width, bounds.Height);
        }
        node.OnRender(context);
        context.Restore();
        context.Flush();
    }
}
