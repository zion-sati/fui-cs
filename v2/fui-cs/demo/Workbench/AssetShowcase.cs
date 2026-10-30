using FuiCs;

namespace FuiCs.Demo.Workbench;

/// <summary>
/// Route-relative assets: an image and an SVG from the route's own assets folder, two that do not exist, and a
/// drawing of the image's texture through a shared texture asset. Each reports its load state.
/// </summary>
internal static class AssetShowcase
{
    private const string Halves = "assets/halves.png";
    private const string Block = "assets/block.svg";

    public static Node Build()
    {
        var image = Reported(Image.Load(Halves).NodeId("asset-image").Width(80.0f).AltText("Red and blue halves"), "asset-image-state", "Image");
        var vector = Reported(Svg.Load(Block, Color.FromRgba(0x16, 0xA3, 0x4A, 0xFF)).NodeId("asset-svg").Height(40.0f), "asset-svg-state", "SVG");
        var missingImage = Reported(Image.Load("assets/missing.png").NodeId("asset-missing-image").Width(40.0f).Height(40.0f), "asset-missing-image-state", "Missing image");
        var missingSvg = Reported(Svg.Load("assets/missing.svg").NodeId("asset-missing-svg").Width(40.0f).Height(40.0f), "asset-missing-svg-state", "Missing SVG");
        var drawing = new TextureDrawing(TextureAsset.Load(Halves));
        drawing.NodeId("asset-drawing").Width(80.0f).Height(40.0f);

        return FlexBox.Column(
                new Text("Assets").FontSize(20.0f).FontWeight(FontWeight.Bold),
                new FlexBox().Height(8.0f),
                FlexBox.Row(image.Node, Gap(), vector.Node, Gap(), drawing, Gap(), missingImage.Node, Gap(), missingSvg.Node)
                    .AlignItems(AlignItems.Center),
                image.State,
                vector.State,
                missingImage.State,
                missingSvg.State)
            .NodeId("asset-showcase")
            .AlignItems(AlignItems.Start);
    }

    private static (TContent Node, Text State) Reported<TContent>(TContent node, string stateId, string label)
        where TContent : Node, IImageContent
    {
        var state = new Text(label + " " + Describe(node.AssetState)).FontSize(13.0f).NodeId(stateId).Selectable(false);
        node.AssetStateChanged += value => state.Text = label + " " + Describe(value);
        return (node, state);
    }

    private static string Describe(AssetLoadState state) => state switch
    {
        AssetLoadState.Loading => "loading",
        AssetLoadState.Ready => "ready",
        AssetLoadState.Failed => "failed",
        _ => "idle",
    };

    private static FlexBox Gap() => new FlexBox().Width(12.0f);

    /// <summary>Draws a texture asset scaled into its box once the host has loaded it.</summary>
    private sealed class TextureDrawing : CustomDrawable
    {
        private readonly TextureAsset texture;

        public TextureDrawing(TextureAsset texture)
        {
            this.texture = texture;
            texture.StateChanged += _ => InvalidateVisual();
        }

        public override void Dispose()
        {
            texture.Dispose();
            base.Dispose();
        }

        protected override void OnRender(DrawContext context)
        {
            if (texture.State == AssetLoadState.Ready)
            {
                context.DrawImage(texture, 0.0f, 0.0f, 80.0f, 40.0f, ImageSampling.Nearest);
            }
        }
    }
}
