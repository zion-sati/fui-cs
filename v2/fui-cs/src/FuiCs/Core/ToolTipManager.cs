using FuiCs.Bindings;

namespace FuiCs;

/// <summary>What the tooltip manager hears from nodes. The router holds it once any node has had a tooltip.</summary>
internal interface IToolTipRouting
{
    void ToolTipChanged(Node owner, ToolTip? toolTip);

    void PointerDown(Node owner);

    void PointerMove(Node owner, float x, float y);

    void PointerEnter(Node owner, ToolTip? toolTip, float x, float y);

    void PointerLeave(Node owner);

    void FocusChanged(Node owner, ToolTip? toolTip, bool focused);

    /// <summary>The owner is being disposed; its children, which may hold the tooltip, have not been yet.</summary>
    void OwnerDisposing(Node owner);

    void Clear();
}

/// <summary>
/// Shows one tooltip at a time: the hovered node's at the pointer, otherwise the one of the node with visible keyboard
/// focus next to that node. A tooltip shows after its initial delay, or at once within the between-show delay of the
/// last one, and hides after its duration, on a press on its owner, when the page scrolls under a pointer tooltip, or
/// when its owner goes away. While it shows, its host portal is a child of the owner.
/// </summary>
internal static class ToolTipManager
{
    private const float MinimumSurfaceSize = 1.0f;
    private const float AnchorGap = 8.0f;

    private static Portal? host;
    private static FlexBox? panel;
    private static TextCore? label;
    private static PopupPresenter? presenter;
    private static Node? activeOwner;
    private static ToolTip? activeToolTip;
    private static Node? pendingOwner;
    private static ToolTip? pendingToolTip;
    private static AnchorKind pendingAnchorKind;
    private static Point? pendingPopupPoint;
    private static Node? hoveredOwner;
    private static ToolTip? hoveredToolTip;
    private static Point? hoveredPointer;
    private static Node? suppressedHoverOwner;
    private static Node? focusedOwner;
    private static ToolTip? focusedToolTip;
    private static long quickShowUntil = long.MinValue;
    private static AnchorKind activeAnchorKind;
    private static Rect activeAnchor;
    private static Point? activePopupPoint;
    private static ITimer? showTimer;
    private static ITimer? hideTimer;
    private static IDisposable? focusVisibilitySubscription;

    private enum AnchorKind
    {
        None,
        Owner,
        Pointer,
    }

    /// <summary>The clock behind the show and hide delays; tests replace it.</summary>
    internal static TimeProvider Time { get; set; } = TimeProvider.System;

    internal static Node? ActiveOwner => activeOwner;

    internal static FlexBox? Panel => panel;

    internal static TextCore? Label => label;

    /// <summary>Starts routing node input to the manager; the first tooltip calls it.</summary>
    internal static void Install()
    {
        if (EventRouter.ToolTips is not null)
        {
            return;
        }
        EventRouter.ToolTips = Routing.Instance;
        EventRouter.Scrolled += HandleScroll;
        Application.Unmounting -= ClearShell;
        Application.Unmounting += ClearShell;
        focusVisibilitySubscription = FocusVisibility.KeyboardFocusVisible.Subscribe(static _ => ActivateBestCandidate());
    }

    internal static void Reset()
    {
        ClearShell();
        EventRouter.Scrolled -= HandleScroll;
        focusVisibilitySubscription?.Dispose();
        focusVisibilitySubscription = null;
        EventRouter.ToolTips = null;
        Time = TimeProvider.System;
    }

    /// <summary>Hides the tooltip, cancels a pending one and forgets the panel, which went with the shell that hosted it.</summary>
    internal static void ClearShell()
    {
        Clear();
        (host, panel, label, presenter) = (null, null, null, null);
    }

    private static void Clear()
    {
        CancelShowTimer();
        ClearPending();
        (hoveredOwner, hoveredToolTip, hoveredPointer, suppressedHoverOwner) = (null, null, null, null);
        (focusedOwner, focusedToolTip) = (null, null);
        HideCurrent();
        quickShowUntil = long.MinValue;
    }

    private static void ToolTipChanged(Node owner, ToolTip? toolTip)
    {
        if (toolTip is null)
        {
            ClearOwner(owner);
            ActivateBestCandidate();
            return;
        }
        if (ReferenceEquals(hoveredOwner, owner))
        {
            hoveredToolTip = toolTip;
        }
        if (ReferenceEquals(focusedOwner, owner))
        {
            focusedToolTip = toolTip;
        }
        if (ReferenceEquals(activeOwner, owner))
        {
            activeToolTip = toolTip;
            ShowNow(owner, toolTip, preserveAnchor: true);
            return;
        }
        if (ReferenceEquals(pendingOwner, owner))
        {
            pendingToolTip = toolTip;
            ActivateBestCandidate();
        }
    }

    private static void PointerEnter(Node owner, ToolTip? toolTip, float x, float y)
    {
        if (toolTip is null || toolTip.Text.Length == 0)
        {
            return;
        }
        if (ReferenceEquals(suppressedHoverOwner, owner))
        {
            suppressedHoverOwner = null;
        }
        (hoveredOwner, hoveredToolTip, hoveredPointer) = (owner, toolTip, new Point(x, y));
        ActivateBestCandidate();
    }

    private static void PointerMove(Node owner, float x, float y)
    {
        if (!ReferenceEquals(hoveredOwner, owner))
        {
            return;
        }
        hoveredPointer = new Point(x, y);
        if (ReferenceEquals(pendingOwner, owner) && pendingAnchorKind == AnchorKind.Pointer)
        {
            pendingPopupPoint = hoveredPointer;
        }
    }

    private static void PointerLeave(Node owner)
    {
        if (ReferenceEquals(hoveredOwner, owner))
        {
            (hoveredOwner, hoveredToolTip, hoveredPointer) = (null, null, null);
        }
        if (ReferenceEquals(suppressedHoverOwner, owner))
        {
            suppressedHoverOwner = null;
        }
        ActivateBestCandidate();
    }

    private static void PointerDown(Node owner)
    {
        if (ReferenceEquals(activeOwner, owner) || ReferenceEquals(pendingOwner, owner))
        {
            CancelShowTimer();
            HideCurrent();
            ClearPending();
        }
    }

    private static void FocusChanged(Node owner, ToolTip? toolTip, bool focused)
    {
        if (focused)
        {
            if (toolTip is { ShowsOnKeyboardFocus: true, Text.Length: > 0 })
            {
                (focusedOwner, focusedToolTip) = (owner, toolTip);
            }
        }
        else if (ReferenceEquals(focusedOwner, owner))
        {
            (focusedOwner, focusedToolTip) = (null, null);
        }
        ActivateBestCandidate();
    }

    /// <summary>Scrolling moves the page under a pointer tooltip, so it hides until the pointer enters its owner again.</summary>
    private static void HandleScroll()
    {
        if (activeAnchorKind != AnchorKind.Pointer || activeOwner is null)
        {
            return;
        }
        suppressedHoverOwner = activeOwner;
        if (ReferenceEquals(hoveredOwner, activeOwner))
        {
            (hoveredOwner, hoveredToolTip, hoveredPointer) = (null, null, null);
        }
        ClearPending();
        CancelShowTimer();
        HideCurrent();
        ActivateBestCandidate();
    }

    private static void ClearOwner(Node owner)
    {
        if (ReferenceEquals(hoveredOwner, owner))
        {
            (hoveredOwner, hoveredToolTip, hoveredPointer) = (null, null, null);
        }
        if (ReferenceEquals(suppressedHoverOwner, owner))
        {
            suppressedHoverOwner = null;
        }
        if (ReferenceEquals(focusedOwner, owner))
        {
            (focusedOwner, focusedToolTip) = (null, null);
        }
        if (ReferenceEquals(pendingOwner, owner))
        {
            (pendingOwner, pendingToolTip) = (null, null);
            CancelShowTimer();
        }
        if (ReferenceEquals(activeOwner, owner))
        {
            HideCurrent();
        }
    }

    /// <summary>Shows the hovered node's tooltip, or else the keyboard-focused node's, or hides the tooltip.</summary>
    private static void ActivateBestCandidate()
    {
        var hovered = ReferenceEquals(hoveredOwner, suppressedHoverOwner) ? null : hoveredOwner;
        var keyboardFocusVisible = FocusVisibility.KeyboardFocusVisible.Value;
        var owner = hovered ?? (keyboardFocusVisible ? focusedOwner : null);
        var toolTip = hovered is not null ? hoveredToolTip : keyboardFocusVisible ? focusedToolTip : null;
        if (owner is null || toolTip is null || toolTip.Text.Length == 0)
        {
            ClearPending();
            CancelShowTimer();
            HideCurrent();
            return;
        }
        if (ReferenceEquals(activeOwner, owner) && ReferenceEquals(activeToolTip, toolTip))
        {
            return;
        }
        RequestShow(owner, toolTip, hovered is not null ? AnchorKind.Pointer : AnchorKind.Owner);
    }

    private static void RequestShow(Node owner, ToolTip toolTip, AnchorKind anchorKind)
    {
        (pendingOwner, pendingToolTip, pendingAnchorKind) = (owner, toolTip, anchorKind);
        pendingPopupPoint = anchorKind == AnchorKind.Pointer ? hoveredPointer : null;
        CancelHideTimer();
        CancelShowTimer();
        var delay = Time.GetTimestamp() <= quickShowUntil ? TimeSpan.Zero : toolTip.InitialShowDelay;
        if (delay <= TimeSpan.Zero)
        {
            CommitPendingShow();
            return;
        }
        showTimer = Time.CreateTimer(static _ => CommitPendingShow(), null, delay, Timeout.InfiniteTimeSpan);
    }

    private static void CommitPendingShow()
    {
        CancelShowTimer();
        if (pendingOwner is not { } owner || pendingToolTip is not { } toolTip)
        {
            return;
        }
        var anchorKind = pendingAnchorKind;
        (pendingOwner, pendingToolTip, pendingAnchorKind) = (null, null, AnchorKind.None);
        ShowNow(owner, toolTip, preserveAnchor: false, anchorKind);
    }

    private static void ShowNow(Node owner, ToolTip toolTip, bool preserveAnchor, AnchorKind anchorKind = AnchorKind.Owner)
    {
        if (!owner.IsBuilt)
        {
            return;
        }
        EnsureHost();
        if (!ReferenceEquals(host!.Parent, owner))
        {
            owner.AddChild(host);
        }
        if (!host.IsBuilt)
        {
            return;
        }
        var theme = Theme.Active.Value.ToolTip;
        label!.Text = toolTip.Text;
        label.FontFamily = theme.FontFamily;
        label.FontSize = theme.FontSize;
        label.TextColor = toolTip.TextColor ?? theme.TextColor;
        panel!.Padding = theme.Padding;
        panel.CornerRadius = new CornerRadius(theme.PanelCornerRadius);
        panel.Border = new Border(1.0f, theme.PanelBorderColor);
        panel.DropShadow = new DropShadow(theme.PanelShadowColor, 0.0f, theme.ShadowOffsetY, theme.ShadowBlur, theme.ShadowSpread);
        panel.Background = toolTip.Background ?? theme.PanelBackground;
        presenter!.Placement = toolTip.Placement;
        presenter.AnchorGap = AnchorGap;
        if (!preserveAnchor || !ReferenceEquals(activeOwner, owner))
        {
            if (anchorKind == AnchorKind.Pointer && pendingPopupPoint is { } point)
            {
                activeAnchorKind = AnchorKind.Pointer;
                activePopupPoint = new Point(point.X + toolTip.HorizontalOffset, point.Y + toolTip.VerticalOffset);
            }
            else
            {
                var bounds = Ui.TryGetBounds(owner.Handle, out var ownerBounds) ? ownerBounds : new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                activeAnchorKind = AnchorKind.Owner;
                activeAnchor = new Rect(bounds.X + toolTip.HorizontalOffset, bounds.Y + toolTip.VerticalOffset, bounds.Width, bounds.Height);
                activePopupPoint = null;
            }
        }
        // Show at the smallest size to lay the panel out, then again at its measured size.
        ShowAtResolvedAnchor(toolTip, MinimumSurfaceSize, MinimumSurfaceSize);
        FrameScheduler.FlushCommit();
        var measured = panel.IsBuilt && Ui.TryGetBounds(panel.Handle, out var panelBounds) ? panelBounds : default;
        ShowAtResolvedAnchor(toolTip, Math.Max(MinimumSurfaceSize, measured.Width), Math.Max(MinimumSurfaceSize, measured.Height));
        FrameScheduler.FlushCommit();
        (activeOwner, activeToolTip) = (owner, toolTip);
        quickShowUntil = Time.GetTimestamp() + (long)(toolTip.BetweenShowDelay.TotalSeconds * Time.TimestampFrequency);
        CancelHideTimer();
        if (toolTip.ShowDuration > TimeSpan.Zero)
        {
            hideTimer = Time.CreateTimer(static _ => HideAfterDuration(), null, toolTip.ShowDuration, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Hides a tooltip whose duration ran out. As in WPF it stays hidden until the pointer leaves its owner or focus
    /// moves, rather than showing again while the pointer rests.
    /// </summary>
    private static void HideAfterDuration()
    {
        if (activeOwner is { } owner)
        {
            if (ReferenceEquals(hoveredOwner, owner))
            {
                suppressedHoverOwner = owner;
            }
            else if (ReferenceEquals(focusedOwner, owner))
            {
                (focusedOwner, focusedToolTip) = (null, null);
            }
        }
        HideCurrent();
        ActivateBestCandidate();
    }

    private static void HideCurrent()
    {
        CancelHideTimer();
        presenter?.Hide();
        host?.Parent?.RemoveChild(host);
        (activeOwner, activeToolTip, activeAnchorKind, activePopupPoint) = (null, null, AnchorKind.None, null);
        activeAnchor = default;
    }

    private static void ShowAtResolvedAnchor(ToolTip toolTip, float width, float height)
    {
        if (activeAnchorKind == AnchorKind.Pointer && activePopupPoint is { } point)
        {
            presenter!.ShowAtPoint(point.X, point.Y, width, height);
            return;
        }
        presenter!.ShowAnchored(activeAnchor.X, activeAnchor.Y, activeAnchor.Width, activeAnchor.Height, width, height, toolTip.Placement);
    }

    private static void EnsureHost()
    {
        if (host is not null)
        {
            return;
        }
        label = new TextCore(string.Empty) { NodeId = "fui-tooltip-text" };
        panel = new FlexBox().PositionAbsolute().FlexDirection(FlexDirection.Column).Child(label);
        panel.NodeId = "fui-tooltip";
        host = new Portal();
        host.PositionAbsolute().Position(0.0f, 0.0f).Width(Length.Percent(100.0f)).Height(Length.Percent(100.0f));
        // No semantic scope: a tooltip must not confine keyboard focus or the accessibility tree to itself.
        presenter = new PopupPresenter(host, panel, semanticScopeNode: null);
    }

    private static void ClearPending()
    {
        (pendingOwner, pendingToolTip, pendingAnchorKind, pendingPopupPoint) = (null, null, AnchorKind.None, null);
    }

    private static void CancelShowTimer()
    {
        showTimer?.Dispose();
        showTimer = null;
    }

    private static void CancelHideTimer()
    {
        hideTimer?.Dispose();
        hideTimer = null;
    }

    private sealed class Routing : IToolTipRouting
    {
        public static readonly Routing Instance = new();

        public void ToolTipChanged(Node owner, ToolTip? toolTip) => ToolTipManager.ToolTipChanged(owner, toolTip);

        public void PointerDown(Node owner) => ToolTipManager.PointerDown(owner);

        public void PointerMove(Node owner, float x, float y) => ToolTipManager.PointerMove(owner, x, y);

        public void PointerEnter(Node owner, ToolTip? toolTip, float x, float y) => ToolTipManager.PointerEnter(owner, toolTip, x, y);

        public void PointerLeave(Node owner) => ToolTipManager.PointerLeave(owner);

        public void FocusChanged(Node owner, ToolTip? toolTip, bool focused) => ToolTipManager.FocusChanged(owner, toolTip, focused);

        public void OwnerDisposing(Node owner)
        {
            ClearOwner(owner);
            ActivateBestCandidate();
        }

        public void Clear() => ToolTipManager.Clear();
    }
}
