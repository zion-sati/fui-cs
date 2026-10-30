namespace FuiCs;

/// <summary>A node that takes content. Containers that present content inside an inner node redirect it there.</summary>
public interface IContentContainer
{
    /// <summary>Adds <paramref name="node"/> to the container's content.</summary>
    void AddContent(Node node);

    /// <summary>Replaces the content with <paramref name="nodes"/>.</summary>
    void SetContent(params ReadOnlySpan<Node> nodes);
}

/// <summary>Fluent content for every <see cref="IContentContainer"/>.</summary>
public static class ContentContainerExtensions
{
    extension<TContainer>(TContainer container) where TContainer : Node, IContentContainer
    {
        /// <summary>Adds <paramref name="child"/> to the content.</summary>
        public TContainer Child(Node child)
        {
            container.AddContent(child);
            return container;
        }

        /// <summary>Adds <paramref name="children"/> to the content.</summary>
        public TContainer Children(params ReadOnlySpan<Node> children)
        {
            container.SetContent(children);
            return container;
        }
    }
}
