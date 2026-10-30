namespace FuiCs;

/// <summary>
/// Fluent configuration for every node. Each member sets the property or subscribes to the event of the same
/// name and returns the node as its own type, so chains keep the concrete type.
/// </summary>
public static class NodeExtensions
{
    extension<TNode>(TNode node) where TNode : Node
    {
        /// <summary>Sets <see cref="Node.NodeId"/>.</summary>
        public TNode NodeId(string id)
        {
            node.NodeId = id;
            return node;
        }

        /// <summary>
        /// Saves and restores the node's state with <paramref name="adapter"/> across history navigation.
        /// </summary>
        public TNode PersistState(PersistedStateAdapter adapter)
        {
            node.RegisterPersistedState(adapter);
            return node;
        }

        /// <summary>Sets <see cref="Node.Cursor"/>.</summary>
        public TNode Cursor(CursorStyle style)
        {
            node.Cursor = style;
            return node;
        }

        /// <summary>Sets <see cref="Node.IsEnabled"/>.</summary>
        public TNode Enabled(bool enabled)
        {
            node.IsEnabled = enabled;
            return node;
        }

        /// <summary>Sets <see cref="Node.Visibility"/>.</summary>
        public TNode Visibility(Visibility visibility)
        {
            node.Visibility = visibility;
            return node;
        }

        /// <summary>
        /// Lets the node take keyboard focus, at <paramref name="tabIndex"/> in the tab order (0 for document order).
        /// </summary>
        public TNode Focusable(bool focusable, int tabIndex = 0)
        {
            node.SetFocusable(focusable, tabIndex);
            return node;
        }

        /// <summary>Sets <see cref="Node.PreserveSelectionOnPointerDown"/>.</summary>
        public TNode PreserveSelectionOnPointerDown(bool preserve = true)
        {
            node.PreserveSelectionOnPointerDown = preserve;
            return node;
        }

        /// <summary>Sets <see cref="Node.SemanticRole"/>.</summary>
        public TNode SemanticRole(SemanticRole role)
        {
            node.SemanticRole = role;
            return node;
        }

        /// <summary>Sets <see cref="Node.SemanticLabel"/>.</summary>
        public TNode SemanticLabel(string label)
        {
            node.SemanticLabel = label;
            return node;
        }

        /// <summary>Sets <see cref="Node.SemanticChecked"/>.</summary>
        public TNode SemanticChecked(SemanticCheckedState state)
        {
            node.SemanticChecked = state;
            return node;
        }

        /// <summary>Sets the accessibility disabled state; null leaves it unset.</summary>
        public TNode SemanticDisabled(bool? disabled)
        {
            node.SemanticDisabled = disabled;
            return node;
        }

        /// <summary>Sets the accessibility selected state; null leaves it unset.</summary>
        public TNode SemanticSelected(bool? selected)
        {
            node.SemanticSelected = selected;
            return node;
        }

        /// <summary>Sets the accessibility expanded state; null leaves it unset.</summary>
        public TNode SemanticExpanded(bool? expanded)
        {
            node.SemanticExpanded = expanded;
            return node;
        }

        /// <summary>
        /// Reports a value of <paramref name="now"/> between <paramref name="min"/> and <paramref name="max"/> to
        /// assistive technology.
        /// </summary>
        public TNode SemanticValueRange(float now, float min, float max)
        {
            node.SemanticValueRange = new SemanticValueRange(now, min, max);
            return node;
        }

        /// <summary>Sets the accessibility value range; null leaves it unset.</summary>
        public TNode SemanticValueRange(SemanticValueRange? range)
        {
            node.SemanticValueRange = range;
            return node;
        }

        /// <summary>Sets <see cref="Node.SemanticOrientation"/>.</summary>
        public TNode SemanticOrientation(Orientation orientation)
        {
            node.SemanticOrientation = orientation;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerClick"/>.</summary>
        public TNode OnPointerClick(Action<PointerClickEventArgs> handler)
        {
            node.PointerClick += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerDoubleClick"/>.</summary>
        public TNode OnPointerDoubleClick(Action<PointerClickEventArgs> handler)
        {
            node.PointerDoubleClick += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerTripleClick"/>.</summary>
        public TNode OnPointerTripleClick(Action<PointerClickEventArgs> handler)
        {
            node.PointerTripleClick += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerDown"/>.</summary>
        public TNode OnPointerDown(Action<PointerEventArgs> handler)
        {
            node.PointerDown += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerMove"/>.</summary>
        public TNode OnPointerMove(Action<PointerEventArgs> handler)
        {
            node.PointerMove += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerUp"/>.</summary>
        public TNode OnPointerUp(Action<PointerEventArgs> handler)
        {
            node.PointerUp += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerEnter"/>.</summary>
        public TNode OnPointerEnter(Action<PointerEventArgs> handler)
        {
            node.PointerEnter += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerLeave"/>.</summary>
        public TNode OnPointerLeave(Action<PointerEventArgs> handler)
        {
            node.PointerLeave += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PointerCancel"/>.</summary>
        public TNode OnPointerCancel(Action<PointerEventArgs> handler)
        {
            node.PointerCancel += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.Wheel"/>.</summary>
        public TNode OnWheel(Action<WheelEventArgs> handler)
        {
            node.Wheel += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PanGesture"/>.</summary>
        public TNode OnPanGesture(Action<GestureEventArgs> handler)
        {
            node.PanGesture += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.PinchGesture"/>.</summary>
        public TNode OnPinchGesture(Action<GestureEventArgs> handler)
        {
            node.PinchGesture += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.LongPress"/>.</summary>
        public TNode OnLongPress(Action<LongPressEventArgs> handler)
        {
            node.LongPress += handler;
            return node;
        }

        /// <summary>Sets the recognizer for long presses on the node; null uses the defaults.</summary>
        public TNode LongPressRecognizer(LongPressGesture? gesture)
        {
            node.LongPressRecognizer = gesture;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.FocusChanged"/>.</summary>
        public TNode OnFocusChanged(Action<FocusChangedEventArgs> handler)
        {
            node.FocusChanged += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.KeyDown"/>.</summary>
        public TNode OnKeyDown(Action<KeyEventArgs> handler)
        {
            node.KeyDown += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.KeyUp"/>.</summary>
        public TNode OnKeyUp(Action<KeyEventArgs> handler)
        {
            node.KeyUp += handler;
            return node;
        }

        /// <summary>Makes the node a drag source allowing <paramref name="effects"/>.</summary>
        public TNode DragAllowedEffects(DragDropEffects effects)
        {
            node.DragAllowedEffects = effects;
            return node;
        }

        /// <summary>Makes the node a drag source whose data <paramref name="provider"/> creates when a drag starts.</summary>
        public TNode DragData(Func<DragDataObject?> provider)
        {
            node.DragDataProvider = provider;
            return node;
        }

        /// <summary>Sets <see cref="Node.AllowDrop"/>.</summary>
        public TNode AllowDrop(bool allow = true)
        {
            node.AllowDrop = allow;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.DragCompleted"/>.</summary>
        public TNode OnDragCompleted(Action<DragCompletedEventArgs> handler)
        {
            node.DragCompleted += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.DragEnter"/>.</summary>
        public TNode OnDragEnter(Action<DragEventArgs> handler)
        {
            node.DragEnter += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.DragOver"/>.</summary>
        public TNode OnDragOver(Action<DragEventArgs> handler)
        {
            node.DragOver += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.DragLeave"/>.</summary>
        public TNode OnDragLeave(Action<DragEventArgs> handler)
        {
            node.DragLeave += handler;
            return node;
        }

        /// <summary>Adds a handler to <see cref="Node.Drop"/>.</summary>
        public TNode OnDrop(Action<DragEventArgs> handler)
        {
            node.Drop += handler;
            return node;
        }
    }
}
