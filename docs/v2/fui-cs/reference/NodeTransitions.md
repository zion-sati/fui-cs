# NodeTransitions

class in `FuiCs`

Which property changes on a built node animate, and how. A null timing changes that property at once; a change cancels the property's running transition and starts the next one from the current value.

## Properties

- `Background`: The timing of a [`FlexBox.Background`](FlexBox.md) change.
- `Opacity`: The timing of a [`FlexBox.Opacity`](FlexBox.md) change.
- `ScrollOffset`: The timing of a [`ScrollView.SetScrollOffset`](ScrollView.md) change.
