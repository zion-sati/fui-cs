using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ImageTests
{
    private const uint FirstTexture = 0x20000000;
    private const uint FirstSvg = 0x10000000;

    [Test]
    public async Task AnImageLoadsItsUrlAndShowsTheTexture()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var image = Image.Load("img/a.png", ObjectFit.Contain);

        var handle = image.Build();

        await Assert.That(abi.HasCall("fui_load_texture", FirstTexture, "img/a.png")).IsTrue();
        await Assert.That(abi.HasCall("ui_create_node", NodeType.Image)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_image", handle, FirstTexture, ObjectFit.Contain, ImageSamplingKind.Linear, 0u)).IsTrue();
        await Assert.That(image.AssetState).IsEqualTo(AssetLoadState.Loading);
        await Assert.That(image.Source).IsEqualTo("img/a.png");
    }

    [Test]
    public async Task ImagesOfOneUrlShareTheTextureUntilTheLastIsDisposed()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var first = Image.Load("img/shared.png");
        var second = Image.Load("img/shared.png");

        first.Dispose();
        var releasedAfterFirst = abi.CountOf("fui_release_texture");
        second.Dispose();

        await Assert.That(abi.CountOf("fui_load_texture")).IsEqualTo(1);
        await Assert.That(first.TextureId).IsEqualTo(second.TextureId);
        await Assert.That(releasedAfterFirst).IsEqualTo(0);
        await Assert.That(abi.HasCall("fui_release_texture", FirstTexture)).IsTrue();
    }

    [Test]
    public async Task AutoSidesTakeTheIntrinsicSizeOrKeepTheAspectRatio()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var natural = Image.Load("img/wide.png");
        var fixedWidth = Image.Load("img/wide.png").Width(50.0f);
        natural.Build();
        fixedWidth.Build();
        abi.Calls.Clear();

        HostEvents.OnTextureLoaded(FirstTexture, 200.0f, 100.0f);

        await Assert.That(abi.HasCall("ui_set_width", natural.Handle, 200.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_height", natural.Handle, 100.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_height", fixedWidth.Handle, 25.0f, Unit.Pixel)).IsTrue();
        await Assert.That(fixedWidth.Width).IsEqualTo(Length.Pixels(50.0f));
        await Assert.That(natural.AssetState).IsEqualTo(AssetLoadState.Ready);
        await Assert.That(natural.AssetWidth).IsEqualTo(200.0f);
    }

    [Test]
    public async Task FillSizesAreNotReplacedByTheIntrinsicSize()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var image = Image.Load("img/fill.png").FillWidth();
        image.Build();
        abi.Calls.Clear();

        HostEvents.OnTextureLoaded(FirstTexture, 64.0f, 32.0f);

        await Assert.That(abi.HasCall("ui_set_width", image.Handle)).IsFalse();
        await Assert.That(abi.HasCall("ui_set_height", image.Handle, 32.0f, Unit.Pixel)).IsTrue();
    }

    [Test]
    public async Task LoadFailuresAreReportedOnEveryImageOfTheUrl()
    {
        TestRuntime.Start(new LayoutAbi());
        var states = new List<AssetLoadState>();
        var image = Image.Load("img/missing.png").OnAssetStateChanged(states.Add);

        HostEvents.OnTextureFailed(FirstTexture, "404");

        await Assert.That(image.AssetState).IsEqualTo(AssetLoadState.Failed);
        await Assert.That(image.AssetError).IsEqualTo("404");
        await Assert.That(string.Join(",", states)).IsEqualTo("Failed");
    }

    [Test]
    public async Task NinePatchSamplingAndAltTextUpdateTheBuiltImage()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var image = Image.Load("img/frame.png");
        var handle = image.Build();
        abi.Calls.Clear();

        image.Sampling(ImageSampling.Anisotropic(4)).NinePatch(4.0f, 6.0f, 4.0f, 6.0f).AltText("Frame");

        await Assert.That(abi.HasCall("ui_set_image", handle, FirstTexture, ObjectFit.Fill, ImageSamplingKind.Anisotropic, 4u)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_image_nine", handle, FirstTexture, 4.0f, 6.0f, 4.0f, 6.0f, ImageSamplingKind.Anisotropic, 4u)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_role", handle, SemanticRole.Image)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_label", handle, "Frame")).IsTrue();
    }

    [Test]
    public async Task ClearingTheSourceReleasesTheTextureAndShowsNothing()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var image = Image.Load("img/a.png").NinePatch(new Thickness(2.0f));
        var handle = image.Build();
        abi.Calls.Clear();

        image.Source = null;

        await Assert.That(abi.HasCall("fui_release_texture", FirstTexture)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_image", handle, 0u)).IsTrue();
        await Assert.That(image.NinePatch).IsNull();
        await Assert.That(image.AssetState).IsEqualTo(AssetLoadState.Idle);
    }

    [Test]
    public async Task ATextureIdShowsAnAssetLoadedElsewhere()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var image = Image.Load("img/a.png");

        image.TextureId = 7;
        var handle = image.Build();

        await Assert.That(abi.HasCall("fui_release_texture", FirstTexture)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_image", handle, 7u)).IsTrue();
        await Assert.That(image.Source).IsNull();
    }

    [Test]
    public async Task AnSvgLoadsItsUrlWithATintAndSizesFromTheLoadedSvg()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var svg = Svg.Load("icons/star.svg", 0xFF0000FFu).Height(48.0f);
        var handle = svg.Build();

        HostEvents.OnSvgLoaded(FirstSvg, 24.0f, 12.0f);
        svg.Tint = null;

        await Assert.That(abi.HasCall("fui_load_svg", FirstSvg, "icons/star.svg")).IsTrue();
        await Assert.That(abi.HasCall("ui_create_node", NodeType.Svg)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_svg", handle, FirstSvg, 0xFF0000FFu, ImageSamplingKind.Linear, 0u)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_width", handle, 96.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_svg", handle, FirstSvg, 0u)).IsTrue();
    }
}
