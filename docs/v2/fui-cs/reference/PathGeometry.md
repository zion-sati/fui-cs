# PathGeometry

class in `FuiCs`: `IDisposable`

A vector path held by the host and drawn with [`DrawContext.DrawPath`](DrawContext.md). Dispose it to release the host resource.

## Methods

- `AddCircle(float, float, float)`: Adds a circle as a closed contour.
- `AddRect(float, float, float, float)`: Adds a rectangle as a closed contour.
- `Close()`: Closes the current contour.
- `CubicTo(float, float, float, float, float, float)`: Adds a cubic curve to the point.
- `Dispose()`: Releases the host's path.
- `LineTo(float, float)`: Adds a straight line to the point.
- `MoveTo(float, float)`: Starts a new contour at the point.
- `QuadTo(float, float, float, float)`: Adds a quadratic curve to the point.
