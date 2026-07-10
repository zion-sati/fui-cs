namespace FuiCs;

public interface INode
{
    BuiltNode Build();
}

public sealed class BuiltNode
{
    private readonly List<BuiltNode> children = [];
    private bool destroyed;

    public BuiltNode(ulong handle)
    {
        Handle = handle;
    }

    public ulong Handle { get; }

    public IReadOnlyList<BuiltNode> Children => children;

    public void AddChild(BuiltNode child)
    {
        Runtime.Backend.AddChild(Handle, child.Handle);
        children.Add(child);
    }

    public void Destroy()
    {
        if (destroyed)
        {
            return;
        }

        for (var index = children.Count - 1; index >= 0; index -= 1)
        {
            children[index].Destroy();
        }
        children.Clear();

        if (Handle != 0)
        {
            Runtime.Backend.DeleteNode(Handle);
        }
        destroyed = true;
    }
}

public abstract class Node<TSelf> : INode where TSelf : Node<TSelf>
{
    private (float value, Unit unit)? width;
    private (float value, Unit unit)? height;

    public TSelf Width(float value, Unit unit = Unit.Pixel)
    {
        width = (value, unit);
        return (TSelf)this;
    }

    public TSelf Height(float value, Unit unit = Unit.Pixel)
    {
        height = (value, unit);
        return (TSelf)this;
    }

    public BuiltNode Build()
    {
        var node = BuildCore();
        ApplySize(node.Handle);
        ApplyCore(node);
        return node;
    }

    protected abstract BuiltNode BuildCore();

    protected virtual void ApplyCore(BuiltNode node)
    {
    }

    private void ApplySize(ulong handle)
    {
        if (width is { } widthValue)
        {
            Runtime.Backend.SetWidth(handle, widthValue.value, (uint)widthValue.unit);
        }
        if (height is { } heightValue)
        {
            Runtime.Backend.SetHeight(handle, heightValue.value, (uint)heightValue.unit);
        }
    }
}

public sealed class FlexBox : Node<FlexBox>
{
    private readonly List<INode> children = [];
    private uint? bgColor;
    private (float top, float right, float bottom, float left)? padding;
    private FlexDirection? flexDirection;

    public FlexBox BgColor(uint color)
    {
        bgColor = color;
        return this;
    }

    public FlexBox Padding(float all)
    {
        return Padding(all, all, all, all);
    }

    public FlexBox Padding(float top, float right, float bottom, float left)
    {
        padding = (top, right, bottom, left);
        return this;
    }

    public FlexBox FlexDirection(FlexDirection direction)
    {
        flexDirection = direction;
        return this;
    }

    public FlexBox Child(INode child)
    {
        children.Add(child);
        return this;
    }

    protected override BuiltNode BuildCore()
    {
        return new BuiltNode(Runtime.Backend.CreateNode((uint)NodeType.FlexBox));
    }

    protected override void ApplyCore(BuiltNode node)
    {
        if (bgColor is { } color)
        {
            Runtime.Backend.SetBgColor(node.Handle, color);
        }
        if (padding is { } paddingValue)
        {
            Runtime.Backend.SetPadding(
                node.Handle,
                paddingValue.top,
                paddingValue.right,
                paddingValue.bottom,
                paddingValue.left);
        }
        if (flexDirection is { } direction)
        {
            Runtime.Backend.SetFlexDirection(node.Handle, (uint)direction);
        }

        foreach (var child in children)
        {
            node.AddChild(child.Build());
        }
    }
}

public sealed class TextNode : Node<TextNode>
{
    private readonly string content;
    private (uint fontId, float size)? font;
    private uint? textColor;

    public TextNode(string content)
    {
        this.content = content;
    }

    public TextNode Font(uint fontId, float size)
    {
        font = (fontId, size);
        return this;
    }

    public TextNode TextColor(uint color)
    {
        textColor = color;
        return this;
    }

    protected override BuiltNode BuildCore()
    {
        return new BuiltNode(Runtime.Backend.CreateNode((uint)NodeType.Text));
    }

    protected override void ApplyCore(BuiltNode node)
    {
        Runtime.Backend.SetText(node.Handle, content);
        if (font is { } fontValue)
        {
            Runtime.Backend.SetFont(node.Handle, fontValue.fontId, fontValue.size);
        }
        if (textColor is { } color)
        {
            Runtime.Backend.SetTextColor(node.Handle, color);
        }
    }
}

public static class Fui
{
    public static FlexBox FlexBox()
    {
        return new FlexBox();
    }

    public static TextNode Text(string content)
    {
        return new TextNode(content);
    }

    public static FlexBox Row()
    {
        return new FlexBox().FlexDirection(FlexDirection.Row);
    }

    public static FlexBox Column()
    {
        return new FlexBox().FlexDirection(FlexDirection.Column);
    }
}
