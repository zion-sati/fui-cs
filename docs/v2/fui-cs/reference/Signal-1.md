# Signal<T>

class in `FuiCs`

A value that raises [`Signal.Changed`](Signal-1.md) whenever it is set to a different value.

## Properties

- `HasListeners`: Whether anything subscribes.
- `Value`: The current value; setting a different one notifies the subscribers.

## Methods

- `Reset(T0)`: Restores `value` without raising [`Signal.Changed`](Signal-1.md) and drops every subscriber.
- `Set(T0)`: Sets the value; returns false when it is unchanged.
- `Subscribe(Action<T0>)`: Adds `handler` to [`Signal.Changed`](Signal-1.md); disposing the result removes it.

## Events

- `Changed`: Raised with the new value. Raising it schedules a commit.
