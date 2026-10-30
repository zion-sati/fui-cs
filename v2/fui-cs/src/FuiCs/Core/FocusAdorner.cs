using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// The keyboard focus ring: a 2 px outline in the theme's focus color, 2 px outside its owner's bounds, drawn in a
/// portal over the page and clipped to the viewport. Controls show it while they have focus and focus is visible
/// (<see cref="FocusVisibility"/>). It follows its owner after each commit and hides when the owner goes away.
/// </summary>
internal static class FocusAdorner
{
    private const float RingWidth = 2.0f;
    private const float Outset = 2.0f;

    private static FlexBox? ring;
    private static Node? owner;
    private static CornerRadius corners;
    private static bool attached;
    private static bool hooked;
    private static (Rect Host, Rect Ring, Color Color, CornerRadius Corners)? applied;

    /// <summary>Shows the ring around <paramref name="node"/>, its corners <paramref name="radius"/> plus the outset.</summary>
    public static void Show(Node node, CornerRadius radius)
    {
        Install();
        owner = node;
        corners = new CornerRadius(radius.TopLeft + Outset, radius.TopRight + Outset, radius.BottomRight + Outset, radius.BottomLeft + Outset);
        Sync();
    }

    /// <summary>Hides the ring if <paramref name="node"/> owns it.</summary>
    public static void Hide(Node node)
    {
        if (ReferenceEquals(owner, node))
        {
            owner = null;
            Detach();
        }
    }

    public static void Reset()
    {
        owner = null;
        ring = null;
        attached = false;
        applied = null;
    }

    /// <summary>Hooks the flush and node lifetime once, so applications that never show a ring do not carry it.</summary>
    private static void Install()
    {
        if (ring is not null)
        {
            return;
        }
        ring = new FlexBox().PositionAbsolute().Background(Color.Transparent);
        Application.AfterCommit ??= Sync;
        Application.Unmounting -= Reset;
        Application.Unmounting += Reset;
        if (!hooked)
        {
            hooked = true;
            EventRouter.NodeDestroyed += Hide;
        }
    }

    /// <summary>Moves the ring to its owner's current bounds; returns whether anything changed.</summary>
    private static bool Sync()
    {
        if (owner is not { IsBuilt: true } node || ring is null || Application.ShellFocusHost is not { IsBuilt: true } host)
        {
            return Detach();
        }
        var bounds = node.GetBounds();
        var ringRect = new Rect(bounds.X - Outset, bounds.Y - Outset, bounds.Width + (Outset * 2.0f), bounds.Height + (Outset * 2.0f));
        var left = Math.Max(ringRect.X, 0.0f);
        var top = Math.Max(ringRect.Y, 0.0f);
        var right = Math.Min(ringRect.X + ringRect.Width, Ui.GetViewportWidth());
        var bottom = Math.Min(ringRect.Y + ringRect.Height, Ui.GetViewportHeight());
        if (right <= left || bottom <= top)
        {
            return Detach();
        }
        var hostRect = new Rect(left, top, right - left, bottom - top);
        var color = Theme.Active.Value.Colors.FocusRing;
        var changed = false;
        if (!attached)
        {
            host.AddChild(ring);
            attached = true;
            changed = true;
        }
        if (applied is not { } last || last.Host != hostRect || last.Ring != ringRect)
        {
            host.Position = new Point(hostRect.X, hostRect.Y);
            host.Width = hostRect.Width;
            host.Height = hostRect.Height;
            ring.Position = new Point(ringRect.X - hostRect.X, ringRect.Y - hostRect.Y);
            ring.Width = ringRect.Width;
            ring.Height = ringRect.Height;
            changed = true;
        }
        if (applied is not { } previous || previous.Color != color || previous.Corners != corners)
        {
            ring.CornerRadius = corners;
            ring.Border = new Border(RingWidth, color);
            changed = true;
        }
        applied = (hostRect, ringRect, color, corners);
        return changed;
    }

    private static bool Detach()
    {
        applied = null;
        if (!attached || ring is null || Application.ShellFocusHost is not { } host)
        {
            attached = false;
            return false;
        }
        host.RemoveChild(ring);
        host.Position = new Point(0.0f, 0.0f);
        host.Width = 0.0f;
        host.Height = 0.0f;
        attached = false;
        return true;
    }
}
