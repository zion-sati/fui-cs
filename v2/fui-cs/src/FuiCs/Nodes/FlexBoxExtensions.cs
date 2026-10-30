namespace FuiCs;

/// <summary>Fluent configuration for <see cref="FlexBox"/> and its subclasses.</summary>
public static class FlexBoxExtensions
{
    extension<TBox>(TBox box) where TBox : FlexBox
    {
        /// <summary>Sets the share of the main axis the box starts from before growing.</summary>
        public TBox FlexBasis(float basis)
        {
            box.FlexBasis = basis;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.Background"/>.</summary>
        public TBox Background(Color color)
        {
            box.Background = color;
            return box;
        }

        /// <summary>Rounds every corner by <paramref name="radius"/>.</summary>
        public TBox CornerRadius(float radius)
        {
            box.CornerRadius = new CornerRadius(radius);
            return box;
        }

        /// <summary>Rounds each corner by its own radius.</summary>
        public TBox CornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            box.CornerRadius = new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            return box;
        }

        /// <summary>Draws a solid border.</summary>
        public TBox Border(float width, Color color)
        {
            box.Border = FuiCs.Border.Solid(width, color);
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.Border"/>.</summary>
        public TBox Border(Border border)
        {
            box.Border = border;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.FlexDirection"/>.</summary>
        public TBox FlexDirection(FlexDirection direction)
        {
            box.FlexDirection = direction;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.FlexWrap"/>.</summary>
        public TBox FlexWrap(FlexWrap wrap)
        {
            box.FlexWrap = wrap;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.JustifyContent"/>.</summary>
        public TBox JustifyContent(JustifyContent justify)
        {
            box.JustifyContent = justify;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.AlignItems"/>.</summary>
        public TBox AlignItems(AlignItems align)
        {
            box.AlignItems = align;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.AlignSelf"/>.</summary>
        public TBox AlignSelf(AlignSelf align)
        {
            box.AlignSelf = align;
            return box;
        }

        /// <summary>Pads every side equally.</summary>
        public TBox Padding(float uniform)
        {
            box.Padding = new Thickness(uniform);
            return box;
        }

        /// <summary>Pads the left and right, and the top and bottom.</summary>
        public TBox Padding(float horizontal, float vertical)
        {
            box.Padding = new Thickness(horizontal, vertical);
            return box;
        }

        /// <summary>Pads each side.</summary>
        public TBox Padding(float left, float top, float right, float bottom)
        {
            box.Padding = new Thickness(left, top, right, bottom);
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.Padding"/>.</summary>
        public TBox Padding(Thickness padding)
        {
            box.Padding = padding;
            return box;
        }

        /// <summary>Spaces every side equally.</summary>
        public TBox Margin(float uniform)
        {
            box.Margin = new Thickness(uniform);
            return box;
        }

        /// <summary>Spaces the left and right, and the top and bottom.</summary>
        public TBox Margin(float horizontal, float vertical)
        {
            box.Margin = new Thickness(horizontal, vertical);
            return box;
        }

        /// <summary>Spaces each side.</summary>
        public TBox Margin(float left, float top, float right, float bottom)
        {
            box.Margin = new Thickness(left, top, right, bottom);
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.Margin"/>.</summary>
        public TBox Margin(Thickness margin)
        {
            box.Margin = margin;
            return box;
        }

        /// <summary>Sets <see cref="FlexBox.PositionType"/>.</summary>
        public TBox PositionType(PositionType positionType)
        {
            box.PositionType = positionType;
            return box;
        }

        /// <summary>Takes the box out of the flow, placed at Position.</summary>
        public TBox PositionAbsolute()
        {
            box.PositionType = FuiCs.PositionType.Absolute;
            return box;
        }

        /// <summary>Places an absolutely positioned box relative to its parent.</summary>
        public TBox Position(float left, float top)
        {
            box.Position = new Point(left, top);
            return box;
        }

        /// <summary>Clips the children to the box's bounds.</summary>
        public TBox ClipToBounds(bool clip)
        {
            box.ClipToBounds = clip;
            return box;
        }

        /// <summary>Sets the box's opacity, from 0 to 1.</summary>
        public TBox Opacity(float opacity)
        {
            box.Opacity = opacity;
            return box;
        }

        /// <summary>Sets which property changes animate.</summary>
        public TBox Transitions(NodeTransitions? transitions)
        {
            box.Transitions = transitions;
            return box;
        }

        /// <summary>Blurs the box's own drawing.</summary>
        public TBox Blur(float sigma)
        {
            box.Blur = sigma;
            return box;
        }

        /// <summary>Draws a shadow behind the box.</summary>
        public TBox DropShadow(Color color, float offsetX, float offsetY, float blurSigma, float spread = 0.0f)
        {
            box.DropShadow = new DropShadow(color, offsetX, offsetY, blurSigma, spread);
            return box;
        }

        /// <summary>Blurs the page behind the box.</summary>
        public TBox BackgroundBlur(float sigma)
        {
            box.BackgroundBlur = sigma;
            return box;
        }

        /// <summary>
        /// Fills the box with a linear gradient from the start point to the end point through <paramref name="stops"/>.
        /// </summary>
        public TBox LinearGradient(float startX, float startY, float endX, float endY, params GradientStop[] stops)
        {
            box.LinearGradient = new LinearGradient(startX, startY, endX, endY, stops);
            return box;
        }
    }
}
