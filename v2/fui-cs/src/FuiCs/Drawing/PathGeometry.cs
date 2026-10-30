using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A vector path held by the host and drawn with <see cref="DrawContext.DrawPath"/>. Dispose it to release the host
/// resource.
/// </summary>
public sealed class PathGeometry : IDisposable
{
    private bool disposed;

    /// <summary>An empty path.</summary>
    public PathGeometry()
    {
        Id = Canvas.PathCreate();
    }

    internal uint Id { get; }

    /// <summary>Starts a new contour at the point.</summary>
    public PathGeometry MoveTo(float x, float y)
    {
        Canvas.PathMoveTo(LiveId(), x, y);
        return this;
    }

    /// <summary>Adds a straight line to the point.</summary>
    public PathGeometry LineTo(float x, float y)
    {
        Canvas.PathLineTo(LiveId(), x, y);
        return this;
    }

    /// <summary>Adds a quadratic curve to the point.</summary>
    public PathGeometry QuadTo(float controlX, float controlY, float x, float y)
    {
        Canvas.PathQuadTo(LiveId(), controlX, controlY, x, y);
        return this;
    }

    /// <summary>Adds a cubic curve to the point.</summary>
    public PathGeometry CubicTo(float control1X, float control1Y, float control2X, float control2Y, float x, float y)
    {
        Canvas.PathCubicTo(LiveId(), control1X, control1Y, control2X, control2Y, x, y);
        return this;
    }

    /// <summary>Closes the current contour.</summary>
    public PathGeometry Close()
    {
        Canvas.PathClose(LiveId());
        return this;
    }

    /// <summary>Adds a rectangle as a closed contour.</summary>
    public PathGeometry AddRect(float x, float y, float width, float height)
    {
        Canvas.PathAddRect(LiveId(), x, y, width, height);
        return this;
    }

    /// <summary>Adds a circle as a closed contour.</summary>
    public PathGeometry AddCircle(float centerX, float centerY, float radius)
    {
        Canvas.PathAddCircle(LiveId(), centerX, centerY, radius);
        return this;
    }

    /// <summary>Releases the host's path.</summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
        Canvas.PathDestroy(Id);
    }

    internal uint LiveId()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return Id;
    }
}
