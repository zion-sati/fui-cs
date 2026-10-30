namespace FuiCs;

/// <summary>Fluent configuration for <see cref="Image"/> and <see cref="Svg"/>.</summary>
public static class ImageContentExtensions
{
    extension<TContent>(TContent content) where TContent : Node, IImageContent
    {
        /// <summary>Loads the content from <paramref name="url"/>.</summary>
        public TContent Source(string url)
        {
            content.Source = url;
            return content;
        }

        /// <summary>Sets <see cref="IImageContent.Sampling"/>.</summary>
        public TContent Sampling(ImageSampling sampling)
        {
            content.Sampling = sampling;
            return content;
        }

        /// <summary>Sets <see cref="IImageContent.AltText"/>.</summary>
        public TContent AltText(string text)
        {
            content.AltText = text;
            return content;
        }

        /// <summary>Adds a handler for the asset loading or failing.</summary>
        public TContent OnAssetStateChanged(Action<AssetLoadState> handler)
        {
            content.AssetStateChanged += handler;
            return content;
        }
    }

    extension<TImage>(TImage image) where TImage : Image
    {
        /// <summary>Shows the loaded texture <paramref name="textureId"/>.</summary>
        public TImage TextureId(uint textureId)
        {
            image.TextureId = textureId;
            return image;
        }

        /// <summary>Sets <see cref="Image.ObjectFit"/>.</summary>
        public TImage ObjectFit(ObjectFit objectFit)
        {
            image.ObjectFit = objectFit;
            return image;
        }

        /// <summary>Stretches the image as a nine-patch, keeping <paramref name="insets"/> unscaled.</summary>
        public TImage NinePatch(Thickness insets)
        {
            image.NinePatch = insets;
            return image;
        }

        /// <summary>Stretches the image as a nine-patch, keeping each edge's inset unscaled.</summary>
        public TImage NinePatch(float left, float top, float right, float bottom)
        {
            image.NinePatch = new Thickness(left, top, right, bottom);
            return image;
        }
    }

    extension<TSvg>(TSvg svg) where TSvg : Svg
    {
        /// <summary>Shows the loaded SVG <paramref name="svgId"/>.</summary>
        public TSvg SvgId(uint svgId)
        {
            svg.SvgId = svgId;
            return svg;
        }

        /// <summary>Tints the SVG's drawing.</summary>
        public TSvg Tint(Color color)
        {
            svg.Tint = color;
            return svg;
        }
    }
}
