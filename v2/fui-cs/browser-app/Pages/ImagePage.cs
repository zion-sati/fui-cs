using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// Images and SVGs loaded by the host: intrinsic and aspect-ratio sizing, object fit, tint, alt text, a failed load
/// and clearing a source. The PNG is 40 x 20, red on the left half and blue on the right; the SVG is a 30 x 15 black block.
/// </summary>
internal static class ImagePage
{
    public const string Halves = "assets/halves.png";
    public const string Block = "assets/block.svg";

    public static Node Build()
    {
        var naturalState = Status("natural-state");
        var natural = Image.Load(Halves).NodeId("natural").OnAssetStateChanged(state => naturalState.Text = Describe(state));
        var missingState = Status("missing-state");
        var missing = Image.Load("assets/missing.png")
            .NodeId("missing")
            .Width(40.0f)
            .Height(20.0f);
        missing.OnAssetStateChanged(state => missingState.Text = Describe(state) + (missing.AssetError.Length > 0 ? ":error" : ""));

        var scaled = Image.Load(Halves).NodeId("scaled").Width(200.0f).AltText("Red and blue halves");
        var clear = new FlexBox().NodeId("clear").Width(60.0f).Height(30.0f).Background(LayoutPage.Grey).Cursor(CursorStyle.Pointer);
        clear.OnPointerClick(_ => scaled.Source = null);

        return FlexBox.Column(
                FlexBox.Row(natural, Spacer(), missing),
                Spacer(),
                scaled,
                Spacer(),
                FlexBox.Row(
                    Image.Load(Halves).NodeId("contain").Width(100.0f).Height(100.0f).ObjectFit(ObjectFit.Contain).Background(LayoutPage.Grey),
                    Spacer(),
                    Image.Load(Halves).NodeId("nearest").Width(120.0f).Height(60.0f).Sampling(ImageSampling.Nearest),
                    Spacer(),
                    Svg.Load(Block).NodeId("svg-natural"),
                    Spacer(),
                    Svg.Load(Block, LayoutPage.Green).NodeId("svg-tinted").Height(30.0f)),
                Spacer(),
                clear,
                naturalState,
                missingState)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static string Describe(AssetLoadState state) => state switch
    {
        AssetLoadState.Loading => "loading",
        AssetLoadState.Ready => "ready",
        AssetLoadState.Failed => "failed",
        _ => "idle",
    };

    private static FlexBox Spacer() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id) => new Text("pending").FontSize(14.0f).NodeId(id);
}
