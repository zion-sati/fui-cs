# LinearGradient

class in `FuiCs`

A gradient from ([`LinearGradient.StartX`](LinearGradient.md), [`LinearGradient.StartY`](LinearGradient.md)) to ([`LinearGradient.EndX`](LinearGradient.md), [`LinearGradient.EndY`](LinearGradient.md)) through [`LinearGradient.Stops`](LinearGradient.md). The points are relative to the node's top-left corner, as CSS gradients and WPF's default mapping are, and the stops carry their own alpha.

## Properties

- `EndX`: The end point's x, from the node's left edge.
- `EndY`: The end point's y, from the node's top edge.
- `StartX`: The start point's x, from the node's left edge.
- `StartY`: The start point's y, from the node's top edge.
- `Stops`: The gradient's colors in order.
