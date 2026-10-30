namespace FuiCs;

/// <summary>What a drop does with the dragged data, as the source allows and the target accepts.</summary>
public enum DragDropEffects
{
    /// <summary>The drop is refused.</summary>
    None = 0,
    /// <summary>The data is copied to the target.</summary>
    Copy = 1,
    /// <summary>The data moves to the target.</summary>
    Move = 2,
    /// <summary>The target links to the data.</summary>
    Link = 4,
}

/// <summary>The unit of a wheel event's deltas.</summary>
public enum WheelDeltaMode
{
    /// <summary>Deltas are in pixels.</summary>
    Pixel = 0,
    /// <summary>Deltas are in lines.</summary>
    Line = 1,
    /// <summary>Deltas are in pages.</summary>
    Page = 2,
}

/// <summary>The kind of device behind a pointer event.</summary>
public enum PointerType
{
    /// <summary>The host could not tell.</summary>
    Unknown = 0,
    /// <summary>A mouse or trackpad.</summary>
    Mouse = 1,
    /// <summary>A finger on a touch screen.</summary>
    Touch = 2,
    /// <summary>A stylus.</summary>
    Pen = 3,
}

/// <summary>The button that changed in a pointer event, numbered as the browser numbers them.</summary>
public enum PointerButton
{
    /// <summary>No button changed, as in a move.</summary>
    None = -1,
    /// <summary>The main button: the left mouse button, a touch or a pen contact.</summary>
    Primary = 0,
    /// <summary>The middle mouse button or wheel.</summary>
    Auxiliary = 1,
    /// <summary>The right mouse button.</summary>
    Secondary = 2,
    /// <summary>The browser-back button.</summary>
    Back = 3,
    /// <summary>The browser-forward button.</summary>
    Forward = 4,
}

/// <summary>The buttons held during a pointer event, as flags.</summary>
[Flags]
public enum PointerButtons
{
    /// <summary>No button is held.</summary>
    None = 0,
    /// <summary>The main button.</summary>
    Primary = 1 << 0,
    /// <summary>The right mouse button.</summary>
    Secondary = 1 << 1,
    /// <summary>The middle mouse button or wheel.</summary>
    Auxiliary = 1 << 2,
    /// <summary>The browser-back button.</summary>
    Back = 1 << 3,
    /// <summary>The browser-forward button.</summary>
    Forward = 1 << 4,
}

/// <summary>The touch gestures a node handles itself, so the host does not scroll or zoom the page for them.</summary>
[Flags]
public enum GestureIntent
{
    /// <summary>The node handles no gestures.</summary>
    None = 0,
    /// <summary>The node handles one-finger pans.</summary>
    Pan = 1,
    /// <summary>The node handles two-finger pinches.</summary>
    Pinch = 2,
    /// <summary>The node handles pans and pinches.</summary>
    PanAndPinch = 3,
}

/// <summary>Where a gesture event falls in its gesture.</summary>
public enum GestureEventPhase
{
    /// <summary>The gesture started.</summary>
    Begin = 1,
    /// <summary>The gesture moved.</summary>
    Update = 2,
    /// <summary>The gesture ended.</summary>
    End = 3,
    /// <summary>The gesture was cancelled.</summary>
    Cancel = 4,
}

/// <summary>The gesture a gesture event reports.</summary>
public enum GestureEventKind
{
    /// <summary>No gesture.</summary>
    None = 0,
    /// <summary>A pan.</summary>
    Pan = 1,
    /// <summary>A pinch.</summary>
    Pinch = 2,
}

/// <summary>Pan and pinch gesture events.</summary>
public sealed class GestureEventArgs(
    GestureEventPhase phase,
    GestureEventKind kind,
    float sceneX,
    float sceneY,
    float deltaX,
    float deltaY,
    float scale,
    int pointerCount) : EventArgs
{
    /// <summary>Where the event falls in its gesture.</summary>
    public GestureEventPhase Phase { get; } = phase;
    /// <summary>Whether the event reports a pan or a pinch.</summary>
    public GestureEventKind Kind { get; } = kind;
    /// <summary>The gesture's point in page coordinates.</summary>
    public float SceneX { get; } = sceneX;
    /// <summary>The gesture's point in page coordinates.</summary>
    public float SceneY { get; } = sceneY;
    /// <summary>The pan's movement since the last event, in pixels.</summary>
    public float DeltaX { get; } = deltaX;
    /// <summary>The pan's movement since the last event, in pixels.</summary>
    public float DeltaY { get; } = deltaY;
    /// <summary>The pinch's scale since the last event; 1 when it did not change.</summary>
    public float Scale { get; } = scale;
    /// <summary>How many pointers take part in the gesture.</summary>
    public int PointerCount { get; } = pointerCount;
    /// <summary>The gesture's point relative to the node handling the event.</summary>
    public float X { get; set; } = sceneX;
    /// <summary>The gesture's point relative to the node handling the event.</summary>
    public float Y { get; set; } = sceneY;
    /// <summary>Set to stop the event reaching ancestors and the host's default handling.</summary>
    public bool Handled { get; set; }
}

/// <summary>The defaults a long press is recognized with.</summary>
public static class LongPressDefaults
{
    /// <summary>How long a pointer must stay down, in milliseconds.</summary>
    public const int MinimumDurationMs = 500;
    /// <summary>How far a pointer may move, in pixels, before it no longer counts as a long press.</summary>
    public const float MovementTolerance = 10.0f;
}

/// <summary>A long press: a pointer held still past the minimum duration.</summary>
public sealed class LongPressEventArgs(
    float sceneX,
    float sceneY,
    int pointerId,
    PointerType pointerType,
    KeyModifier modifiers,
    int durationMs) : EventArgs
{
    /// <summary>The press point in page coordinates.</summary>
    public float SceneX { get; } = sceneX;
    /// <summary>The press point in page coordinates.</summary>
    public float SceneY { get; } = sceneY;
    /// <summary>The pointer's identifier.</summary>
    public int PointerId { get; } = pointerId;
    /// <summary>The device that pressed.</summary>
    public PointerType PointerType { get; } = pointerType;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>How long the pointer was held, in milliseconds.</summary>
    public int DurationMs { get; } = durationMs;
    /// <summary>The press point relative to the node handling the event.</summary>
    public float X { get; set; } = sceneX;
    /// <summary>The press point relative to the node handling the event.</summary>
    public float Y { get; set; } = sceneY;
    /// <summary>Set to stop the event reaching ancestors and the built-in long-press handling.</summary>
    public bool Handled { get; set; }
}

/// <summary>A pointer moving, pressing, releasing, entering, leaving or being cancelled over a node.</summary>
public sealed class PointerEventArgs(
    PointerEventType eventType,
    float sceneX,
    float sceneY,
    KeyModifier modifiers = 0,
    int pointerId = -1,
    PointerType pointerType = PointerType.Unknown,
    PointerButton button = PointerButton.None,
    PointerButtons buttons = PointerButtons.None,
    float pressure = 0.0f,
    float width = 0.0f,
    float height = 0.0f,
    int clickCount = 0,
    bool isPrimary = true,
    float tangentialPressure = 0.0f,
    float tiltX = 0.0f,
    float tiltY = 0.0f,
    float twist = 0.0f) : EventArgs
{
    /// <summary>What the pointer did.</summary>
    public PointerEventType EventType { get; } = eventType;
    /// <summary>The pointer in page coordinates.</summary>
    public float SceneX { get; } = sceneX;
    /// <summary>The pointer in page coordinates.</summary>
    public float SceneY { get; } = sceneY;
    /// <summary>The pointer's identifier, stable for the life of a contact.</summary>
    public int PointerId { get; } = pointerId;
    /// <summary>The device behind the pointer.</summary>
    public PointerType PointerType { get; } = pointerType;
    /// <summary>The button that changed.</summary>
    public PointerButton Button { get; } = button;
    /// <summary>The buttons held.</summary>
    public PointerButtons Buttons { get; } = buttons;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>The contact pressure, from 0 to 1.</summary>
    public float Pressure { get; } = pressure;
    /// <summary>The contact's width in pixels.</summary>
    public float Width { get; } = width;
    /// <summary>The contact's height in pixels.</summary>
    public float Height { get; } = height;
    /// <summary>The click count of the press: 2 for a double click.</summary>
    public int ClickCount { get; } = clickCount;
    /// <summary>Whether this is the primary pointer of its type.</summary>
    public bool IsPrimary { get; } = isPrimary;
    /// <summary>A pen's barrel pressure, from -1 to 1.</summary>
    public float TangentialPressure { get; } = tangentialPressure;
    /// <summary>A pen's tilt across, in degrees.</summary>
    public float TiltX { get; } = tiltX;
    /// <summary>A pen's tilt along, in degrees.</summary>
    public float TiltY { get; } = tiltY;
    /// <summary>A pen's rotation, in degrees.</summary>
    public float Twist { get; } = twist;
    /// <summary>The pointer relative to the node handling the event.</summary>
    public float X { get; set; } = sceneX;
    /// <summary>The pointer relative to the node handling the event.</summary>
    public float Y { get; set; } = sceneY;
    /// <summary>Set to stop the event reaching ancestors.</summary>
    public bool Handled { get; set; }

    /// <summary>A primary-button press, or any touch or pen contact.</summary>
    internal bool IsPrimaryActivation =>
        Button == PointerButton.Primary || PointerType == PointerType.Touch || PointerType == PointerType.Pen;
}

/// <summary>A click: a primary press and release on the same node.</summary>
public sealed class PointerClickEventArgs(
    float sceneX,
    float sceneY,
    PointerType pointerType = PointerType.Unknown,
    PointerButton button = PointerButton.None,
    PointerButtons buttons = PointerButtons.None,
    KeyModifier modifiers = 0,
    int clickCount = 0) : EventArgs
{
    /// <summary>The release point in page coordinates.</summary>
    public float SceneX { get; } = sceneX;
    /// <summary>The release point in page coordinates.</summary>
    public float SceneY { get; } = sceneY;
    /// <summary>The device that clicked.</summary>
    public PointerType PointerType { get; } = pointerType;
    /// <summary>The button that clicked.</summary>
    public PointerButton Button { get; } = button;
    /// <summary>The buttons held.</summary>
    public PointerButtons Buttons { get; } = buttons;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>1 for a click, 2 for a double click, 3 for a triple click.</summary>
    public int ClickCount { get; } = clickCount;
    /// <summary>The release point relative to the node handling the event.</summary>
    public float X { get; set; } = sceneX;
    /// <summary>The release point relative to the node handling the event.</summary>
    public float Y { get; set; } = sceneY;
    /// <summary>Set to stop the click reaching ancestors.</summary>
    public bool Handled { get; set; }
}

/// <summary>A button's click, which carries no data.</summary>
public sealed class ClickEventArgs : EventArgs
{
    /// <summary>The shared instance.</summary>
    public static new readonly ClickEventArgs Empty = new();
}

/// <summary>A key pressed or released while a node has focus.</summary>
public sealed class KeyEventArgs(KeyEventType eventType, string key, KeyModifier modifiers = 0) : EventArgs
{
    /// <summary>Whether the key went down or up.</summary>
    public KeyEventType EventType { get; } = eventType;
    /// <summary>The key's value, as the browser's KeyboardEvent.key gives it ("a", "Enter", "ArrowLeft").</summary>
    public string Key { get; } = key;
    /// <summary>The modifier keys held.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>Set to keep the key from its default handling.</summary>
    public bool Handled { get; set; }
}

/// <summary>A node gaining or losing keyboard focus.</summary>
public sealed class FocusChangedEventArgs(bool focused) : EventArgs
{
    /// <summary>Whether the node now has focus.</summary>
    public bool Focused { get; } = focused;
}

/// <summary>The pointer entering or leaving a node.</summary>
public sealed class HoverChangedEventArgs(bool hovered) : EventArgs
{
    /// <summary>Whether the pointer is now over the node.</summary>
    public bool Hovered { get; } = hovered;
}

/// <summary>A text value changing.</summary>
public sealed class TextChangedEventArgs(string text) : EventArgs
{
    /// <summary>The new text.</summary>
    public string Text { get; } = text;
}

/// <summary>A selection changing.</summary>
public sealed class SelectionChangedEventArgs(uint start, uint end) : EventArgs
{
    /// <summary>Where the selection starts.</summary>
    public uint Start { get; } = start;
    /// <summary>Where the selection ends; equal to Start for a caret.</summary>
    public uint End { get; } = end;
}

/// <summary>A checkbox changing state.</summary>
public sealed class CheckboxChangedEventArgs(bool? isChecked) : EventArgs
{
    /// <summary>The new value; null is indeterminate.</summary>
    public bool? IsChecked { get; } = isChecked;
}

/// <summary>A switch turning on or off.</summary>
public sealed class SwitchChangedEventArgs(bool isChecked) : EventArgs
{
    /// <summary>Whether the switch is now on.</summary>
    public bool IsChecked { get; } = isChecked;
}

/// <summary>A radio button being selected or deselected.</summary>
public sealed class RadioButtonChangedEventArgs(bool isChecked) : EventArgs
{
    /// <summary>Whether the radio button is now selected.</summary>
    public bool IsChecked { get; } = isChecked;
}

/// <summary>A radio group's selection changing.</summary>
public sealed class RadioGroupChangedEventArgs(int selectedIndex, string? selectedValue) : EventArgs
{
    /// <summary>The selected radio's index, or -1 when none is selected.</summary>
    public int SelectedIndex { get; } = selectedIndex;

    /// <summary>The selected radio's value, or null when none is selected.</summary>
    public string? SelectedValue { get; } = selectedValue;
}

/// <summary>A slider's value changing.</summary>
public sealed class SliderChangedEventArgs(float value) : EventArgs
{
    /// <summary>The new value.</summary>
    public float Value { get; } = value;
}

/// <summary>A dropdown's selection changing.</summary>
public sealed class DropdownChangedEventArgs<TItem>(TItem item, int index) : EventArgs
{
    /// <summary>The selected item.</summary>
    public TItem Item { get; } = item;
    /// <summary>The selected item's index.</summary>
    public int Index { get; } = index;
}

/// <summary>A combo box's selection changing.</summary>
public sealed class ComboBoxChangedEventArgs<TItem>(TItem item, int index) : EventArgs
{
    /// <summary>The selected item.</summary>
    public TItem Item { get; } = item;
    /// <summary>The selected item's index.</summary>
    public int Index { get; } = index;
}

/// <summary>A drag finishing.</summary>
public sealed class DragCompletedEventArgs(DragDropEffects effect) : EventArgs
{
    /// <summary>What the drop did, or None when it was refused or cancelled.</summary>
    public DragDropEffects Effect { get; } = effect;
}

/// <summary>A popup or menu showing or hiding.</summary>
public sealed class VisibilityChangedEventArgs(bool visible) : EventArgs
{
    /// <summary>Whether it is now shown.</summary>
    public bool Visible { get; } = visible;
}

/// <summary>A mouse wheel or trackpad scroll over a node.</summary>
public sealed class WheelEventArgs(
    float sceneX,
    float sceneY,
    float deltaX,
    float deltaY,
    WheelDeltaMode deltaMode,
    KeyModifier modifiers) : EventArgs
{
    /// <summary>The pointer in page coordinates.</summary>
    public float SceneX { get; } = sceneX;
    /// <summary>The pointer in page coordinates.</summary>
    public float SceneY { get; } = sceneY;
    /// <summary>The horizontal scroll amount, in DeltaMode units.</summary>
    public float DeltaX { get; } = deltaX;
    /// <summary>The vertical scroll amount, in DeltaMode units.</summary>
    public float DeltaY { get; } = deltaY;
    /// <summary>The unit of the deltas.</summary>
    public WheelDeltaMode DeltaMode { get; } = deltaMode;
    /// <summary>The modifier keys held; Ctrl with a wheel is a trackpad pinch.</summary>
    public KeyModifier Modifiers { get; } = modifiers;
    /// <summary>The pointer relative to the node handling the event.</summary>
    public float X { get; set; } = sceneX;
    /// <summary>The pointer relative to the node handling the event.</summary>
    public float Y { get; set; } = sceneY;
    /// <summary>Set to keep the wheel from scrolling ancestors.</summary>
    public bool Handled { get; set; }
}

/// <summary>A configurable long-press recognizer for <see cref="Node"/>.</summary>
public sealed class LongPressGesture
{
    /// <summary>A long-press recognizer with the default duration and tolerance.</summary>
    public static LongPressGesture Create() => new();

    /// <summary>How long a pointer must stay down, in milliseconds.</summary>
    public int MinimumDurationMs { get; private set; } = LongPressDefaults.MinimumDurationMs;

    /// <summary>How far a pointer may move, in pixels, and still count.</summary>
    public float MovementTolerancePx { get; private set; } = LongPressDefaults.MovementTolerance;

    /// <summary>Raised when a long press is recognized.</summary>
    public event Action<LongPressEventArgs>? Recognized;

    internal bool HasHandler => Recognized is not null;

    /// <summary>Sets how long a pointer must stay down, in milliseconds.</summary>
    public LongPressGesture MinimumDuration(int ms)
    {
        MinimumDurationMs = ms < 0 ? 0 : ms;
        return this;
    }

    /// <summary>Sets how far a pointer may move, in pixels, and still count.</summary>
    public LongPressGesture MovementTolerance(float px)
    {
        MovementTolerancePx = px < 0.0f ? 0.0f : px;
        return this;
    }

    /// <summary>Adds a handler for recognized long presses.</summary>
    public LongPressGesture OnRecognized(Action<LongPressEventArgs> handler)
    {
        Recognized += handler;
        return this;
    }

    internal void Raise(LongPressEventArgs args) => Recognized?.Invoke(args);
}
