namespace FuiCs;

/// <summary>Fluent configuration for <see cref="ContextMenu"/> and nodes' context menus.</summary>
public static class ContextMenuExtensions
{
    extension<TMenu>(TMenu menu) where TMenu : ContextMenu
    {
        /// <summary>Replaces the menu's items.</summary>
        public TMenu Items(params ReadOnlySpan<MenuItem> items)
        {
            menu.Items.Clear();
            foreach (var item in items)
            {
                menu.Items.Add(item);
            }
            return menu;
        }

        /// <summary>Sets <see cref="ContextMenu.MenuWidth"/>.</summary>
        public TMenu MenuWidth(float width)
        {
            menu.MenuWidth = width;
            return menu;
        }

        /// <summary>Sets <see cref="ContextMenu.ItemHeight"/>.</summary>
        public TMenu ItemHeight(float height)
        {
            menu.ItemHeight = height;
            return menu;
        }

        /// <summary>Sets <see cref="ContextMenu.PanelColor"/>.</summary>
        public TMenu PanelColor(Color color)
        {
            menu.PanelColor = color;
            return menu;
        }

        /// <summary>Sets <see cref="ContextMenu.ItemHoverColor"/>.</summary>
        public TMenu ItemHoverColor(Color color)
        {
            menu.ItemHoverColor = color;
            return menu;
        }

        /// <summary>Adds a handler to <see cref="ContextMenu.VisibilityChanged"/>.</summary>
        public TMenu OnVisibilityChanged(Action<VisibilityChangedEventArgs> handler)
        {
            menu.VisibilityChanged += handler;
            return menu;
        }
    }

    extension<TNode>(TNode node) where TNode : Node
    {
        /// <summary>Adds a handler to <see cref="Node.ContextMenuOpening"/>.</summary>
        public TNode OnContextMenuOpening(Action<ContextMenuEventArgs> handler)
        {
            node.ContextMenuOpening += handler;
            return node;
        }

        /// <summary>Sets <see cref="Node.IsContextMenuEnabled"/>.</summary>
        public TNode IsContextMenuEnabled(bool enabled)
        {
            node.IsContextMenuEnabled = enabled;
            return node;
        }
    }
}
