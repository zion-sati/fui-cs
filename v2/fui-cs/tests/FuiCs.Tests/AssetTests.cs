using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class AssetTests
{
    private const uint FirstTexture = 0x20000000;
    private const uint FirstSvg = 0x10000000;

    [Test]
    public async Task ATextureAssetSharesItsUrlWithImagesAndIsReleasedByItsLastHolder()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var texture = TextureAsset.Load("img/shared.png");
        var image = Image.Load("img/shared.png");

        image.Dispose();
        var afterImage = abi.CountOf("fui_release_texture");
        texture.Dispose();

        await Assert.That(abi.CountOf("fui_load_texture")).IsEqualTo(1);
        await Assert.That(texture.Url).IsEqualTo("img/shared.png");
        await Assert.That(afterImage).IsEqualTo(0);
        await Assert.That(abi.HasCall("fui_release_texture", FirstTexture)).IsTrue();
        await Assert.That(texture.Id).IsEqualTo(0u);
        await Assert.That(texture.State).IsEqualTo(AssetLoadState.Idle);
    }

    [Test]
    public async Task AnAssetReportsItsStateSizeAndError()
    {
        TestRuntime.Start(new LayoutAbi());
        var texture = TextureAsset.Load("img/a.png");
        var svg = SvgAsset.Load("img/b.svg");
        var states = new List<string>();
        texture.StateChanged += state => states.Add("texture " + state);
        svg.StateChanged += state => states.Add("svg " + state);
        var loading = (texture.State, svg.State);

        HostEvents.OnTextureLoaded(FirstTexture, 64.0f, 32.0f);
        HostEvents.OnSvgFailed(FirstSvg, "404");

        await Assert.That(loading).IsEqualTo((AssetLoadState.Loading, AssetLoadState.Loading));
        await Assert.That(string.Join("|", states)).IsEqualTo("texture Ready|svg Failed");
        await Assert.That((texture.Width, texture.Height)).IsEqualTo((64.0f, 32.0f));
        await Assert.That(svg.Error).IsEqualTo("404");
        await Assert.That((svg.Width, svg.Height)).IsEqualTo((0.0f, 0.0f));
    }

    [Test]
    public async Task ADisposedAssetStopsRaisingStateChanges()
    {
        TestRuntime.Start(new LayoutAbi());
        var held = TextureAsset.Load("img/a.png");
        var dropped = TextureAsset.Load("img/a.png");
        var raised = 0;
        dropped.StateChanged += _ => raised++;

        dropped.Dispose();
        HostEvents.OnTextureLoaded(FirstTexture, 10.0f, 10.0f);

        await Assert.That(raised).IsEqualTo(0);
        await Assert.That(held.State).IsEqualTo(AssetLoadState.Ready);
    }

    [Test]
    public async Task AnEmptyUrlIsRejected()
    {
        TestRuntime.Start(new LayoutAbi());
        var rejected = false;
        try
        {
            TextureAsset.Load("");
        }
        catch (System.ArgumentException)
        {
            rejected = true;
        }

        await Assert.That(rejected).IsTrue();
    }

    [Test]
    public async Task DrawingByAssetUsesItsId()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var texture = TextureAsset.Load("img/a.png");
        var svg = SvgAsset.Load("img/b.svg");
        var drawable = new AssetDrawable(texture, svg);
        drawable.Build();

        HostEvents.OnCustomDraw(drawable.Handle, 1);

        var words = (uint[])abi.Named("fui_canvas_draw_batch")[0].Arguments[1]!;

        await Assert.That(System.Array.IndexOf(words, FirstTexture) >= 0).IsTrue();
        await Assert.That(System.Array.IndexOf(words, FirstSvg) >= 0).IsTrue();
    }

    private sealed class AssetDrawable(TextureAsset texture, SvgAsset svg) : CustomDrawable
    {
        protected override void OnRender(DrawContext context)
        {
            context.DrawImage(texture, 0.0f, 0.0f, 10.0f, 10.0f, ImageSampling.Nearest);
            context.DrawSvg(svg, 10.0f, 0.0f, 10.0f, 10.0f);
        }
    }
}
