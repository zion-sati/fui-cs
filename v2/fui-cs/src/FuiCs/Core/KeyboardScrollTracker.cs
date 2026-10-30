using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// Keeps the built scroll views and picks the one keyboard scrolling falls back to: the view that contains the last
/// pointer release (or one inside the released node's branch), then views around it, then every visible view from
/// the top left.
/// </summary>
internal static class KeyboardScrollTracker
{
    private const float BoundsTolerance = 0.5f;

    private static readonly List<ScrollView> views = [];
    private static ScrollView? selected;
    private static Node? selectedBranch;
    private static bool installed;

    /// <summary>Hooks the router on first use, so applications without scroll views do not carry keyboard scrolling.</summary>
    public static void Install()
    {
        if (installed)
        {
            return;
        }
        installed = true;
        EventRouter.PointerReleased += TrackPointerUp;
        EventRouter.UnhandledKeyDown += KeyboardScroll.TryHandle;
    }

    public static ScrollView? Selected => selected;

    public static void Register(ScrollView view)
    {
        if (!views.Contains(view))
        {
            views.Add(view);
        }
    }

    public static void Unregister(ScrollView view)
    {
        views.Remove(view);
        if (ReferenceEquals(selected, view))
        {
            selected = null;
        }
    }

    public static void Reset()
    {
        views.Clear();
        selected = null;
        selectedBranch = null;
    }

    public static void TrackPointerUp(Node? target, float x, float y)
    {
        selectedBranch = target is ScrollView ? target : NearestDescendantScrollBranch(target);
        if (Ancestor(target) is { } ancestor)
        {
            selectedBranch ??= ancestor;
            selected = ancestor;
            return;
        }
        if (selectedBranch is not null)
        {
            var candidates = DefaultCandidatesWithin(selectedBranch);
            selected = candidates.Count > 0 ? candidates[0] : null;
            if (selected is not null)
            {
                return;
            }
        }
        selected = ByPoint(x, y);
        if (selected is null)
        {
            selectedBranch = null;
        }
    }

    /// <summary>The views to try after the selected one: its ancestors, its branch, then every view in reading order.</summary>
    public static List<ScrollView> FallbackCandidates()
    {
        var ordered = new List<ScrollView>();
        for (var node = selected?.Parent; node is not null; node = node.Parent)
        {
            if (node is ScrollView view)
            {
                AppendUnique(ordered, view);
            }
        }
        if (selectedBranch is not null)
        {
            foreach (var candidate in DefaultCandidatesWithin(selectedBranch))
            {
                if (!ReferenceEquals(candidate, selected))
                {
                    AppendUnique(ordered, candidate);
                }
            }
        }
        foreach (var candidate in DefaultCandidatesWithin(null))
        {
            if (!ReferenceEquals(candidate, selected))
            {
                AppendUnique(ordered, candidate);
            }
        }
        return ordered;
    }

    private static ScrollView? Ancestor(Node? node)
    {
        for (; node is not null; node = node.Parent)
        {
            if (node is ScrollView view)
            {
                return view;
            }
        }
        return null;
    }

    /// <summary>The nearest ancestor (not inside a scroll view) whose subtree holds a visible scroll view.</summary>
    private static Node? NearestDescendantScrollBranch(Node? node)
    {
        for (; node is not null; node = node.Parent)
        {
            if (node is ScrollView)
            {
                return null;
            }
            if (DefaultCandidatesWithin(node).Count > 0)
            {
                return node;
            }
        }
        return null;
    }

    private static ScrollView? ByPoint(float x, float y)
    {
        ScrollView? best = null;
        var bestBounds = default(Rect);
        var bestContains = false;
        foreach (var view in views)
        {
            if (!view.IsBuilt)
            {
                continue;
            }
            var bounds = view.GetBounds();
            if (!Usable(bounds))
            {
                continue;
            }
            var contains = Contains(bounds, x, y);
            if (best is null || BetterPointCandidate(bounds, contains, bestBounds, bestContains, x, y))
            {
                (best, bestBounds, bestContains) = (view, bounds, contains);
            }
        }
        return best;
    }

    /// <summary>The visible views under <paramref name="root"/> (or all of them), ordered top to bottom, then left to right.</summary>
    private static List<ScrollView> DefaultCandidatesWithin(Node? root)
    {
        var ordered = new List<ScrollView>();
        foreach (var view in views)
        {
            if (root is null || IsDescendantOf(view, root))
            {
                InsertInReadingOrder(ordered, view);
            }
        }
        return ordered;
    }

    private static void InsertInReadingOrder(List<ScrollView> ordered, ScrollView candidate)
    {
        if (VisibleBounds(candidate) is not { } candidateBounds || VisibleBounds(OrderingAnchor(candidate)) is not { } anchorBounds)
        {
            return;
        }
        var index = ordered.Count;
        while (index > 0)
        {
            var current = ordered[index - 1];
            if (VisibleBounds(OrderingAnchor(current)) is not { } currentAnchor || VisibleBounds(current) is not { } currentBounds)
            {
                --index;
                continue;
            }
            if (ReadsBefore(anchorBounds, currentAnchor) || (anchorBounds == currentAnchor && ReadsBefore(candidateBounds, currentBounds)))
            {
                --index;
                continue;
            }
            break;
        }
        ordered.Insert(index, candidate);
    }

    /// <summary>The outermost ancestor below the next enclosing scroll view: where the view sits in its page.</summary>
    private static Node OrderingAnchor(ScrollView view)
    {
        Node anchor = view;
        for (var node = view.Parent; node is not null and not ScrollView; node = node.Parent)
        {
            anchor = node;
        }
        return anchor;
    }

    private static void AppendUnique(List<ScrollView> list, ScrollView view)
    {
        if (views.Contains(view) && VisibleBounds(view) is not null && !list.Contains(view))
        {
            list.Add(view);
        }
    }

    private static bool IsDescendantOf(Node node, Node ancestor)
    {
        for (Node? current = node; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }
        return false;
    }

    private static Rect? VisibleBounds(Node node)
    {
        if (!node.IsBuilt)
        {
            return null;
        }
        var bounds = node.GetBounds();
        return Usable(bounds)
            && bounds.X + bounds.Width > 0.0f
            && bounds.Y + bounds.Height > 0.0f
            && bounds.X < Ui.GetViewportWidth()
            && bounds.Y < Ui.GetViewportHeight()
                ? bounds
                : null;
    }

    private static bool Usable(Rect bounds) => bounds.Width > BoundsTolerance && bounds.Height > BoundsTolerance;

    private static bool Contains(Rect bounds, float x, float y) =>
        x >= bounds.X && x <= bounds.X + bounds.Width && y >= bounds.Y && y <= bounds.Y + bounds.Height;

    private static bool ReadsBefore(Rect candidate, Rect current)
    {
        if (candidate.Y + BoundsTolerance < current.Y)
        {
            return true;
        }
        return current.Y + BoundsTolerance >= candidate.Y && candidate.X < current.X;
    }

    /// <summary>Prefers a view containing the point, then the smaller of two that do, or the nearer of two that do not.</summary>
    private static bool BetterPointCandidate(Rect candidate, bool candidateContains, Rect best, bool bestContains, float x, float y)
    {
        if (candidateContains != bestContains)
        {
            return candidateContains;
        }
        var (candidateScore, bestScore) = candidateContains
            ? (candidate.Width * candidate.Height, best.Width * best.Height)
            : (DistanceSquared(candidate, x, y), DistanceSquared(best, x, y));
        if (candidateScore + BoundsTolerance < bestScore)
        {
            return true;
        }
        if (bestScore + BoundsTolerance < candidateScore)
        {
            return false;
        }
        return ReadsBefore(candidate, best);
    }

    private static float DistanceSquared(Rect bounds, float x, float y)
    {
        var dx = x < bounds.X ? bounds.X - x : Math.Max(0.0f, x - (bounds.X + bounds.Width));
        var dy = y < bounds.Y ? bounds.Y - y : Math.Max(0.0f, y - (bounds.Y + bounds.Height));
        return (dx * dx) + (dy * dy);
    }
}
