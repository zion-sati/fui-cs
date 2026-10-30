using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class BitmapTests
{
    private const uint FirstTexture = 0x20000000;

    [Test]
    public async Task ABitmapCommitsItsPixelsAndItsTextureBecomesReady()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var bitmap = new Bitmap(2, 1);
        bitmap.Pixels[0] = 0xFF;
        bitmap.Pixels[7] = 0x80;
        var image = new Image(bitmap.TextureId);
        var states = new List<AssetLoadState>();
        image.AssetStateChanged += states.Add;

        var committed = bitmap.Commit();
        var call = abi.Named("fui_bitmap_commit")[0].Arguments;

        await Assert.That(committed).IsEqualTo(FirstTexture);
        await Assert.That(call[0]).IsEqualTo((object)FirstTexture);
        await Assert.That(Convert.ToHexString((byte[])call[1]!)).IsEqualTo("FF00000000000080");
        await Assert.That((call[2], call[3])).IsEqualTo(((object)2u, (object)1u));
        await Assert.That(string.Join(",", states)).IsEqualTo("Ready");
        await Assert.That((image.AssetWidth, image.AssetHeight)).IsEqualTo((2.0f, 1.0f));
    }

    [Test]
    public async Task DirtyRectsUploadOnlyTheirClampedRegions()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var bitmap = new Bitmap(3, 3);
        for (var index = 0; index < bitmap.Pixels.Length; ++index)
        {
            bitmap.Pixels[index] = (byte)(index / 4);
        }

        bitmap.AddDirtyRect(1, 1, 5, 5);
        bitmap.AddDirtyRect(3, 0, 1, 1);
        var pending = bitmap.HasDirtyRects;
        bitmap.Commit();
        var call = abi.Named("fui_bitmap_commit_dirty")[0].Arguments;

        // Pixels 4, 5, 7 and 8 (the bottom-right 2 × 2), one byte value per pixel; the out-of-range rect is ignored.
        await Assert.That(pending).IsTrue();
        await Assert.That(abi.CountOf("fui_bitmap_commit_dirty")).IsEqualTo(1);
        await Assert.That(abi.CountOf("fui_bitmap_commit")).IsEqualTo(0);
        await Assert.That(Convert.ToHexString((byte[])call[1]!)).IsEqualTo("04040404050505050707070708080808");
        await Assert.That(string.Join(",", call[2], call[3], call[4], call[5], call[6], call[7])).IsEqualTo("3,3,1,1,2,2");
        await Assert.That(bitmap.HasDirtyRects).IsFalse();
    }

    [Test]
    public async Task DrawingOnTheCanvasReplacesThePixelsAtCommit()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var bitmap = new Bitmap(2, 2);
        bitmap.Pixels[0] = 0x7F;

        bitmap.Canvas.DrawRect(0.0f, 0.0f, 2.0f, 2.0f, Paint.Fill(0xFF0000FF));
        bitmap.Commit();
        var offscreen = abi.Named("fui_canvas_create_offscreen")[0].Arguments;

        await Assert.That((offscreen[0], offscreen[1])).IsEqualTo(((object)2u, (object)2u));
        await Assert.That(abi.CountOf("fui_canvas_draw_batch")).IsEqualTo(1);
        await Assert.That(abi.CountOf("fui_canvas_read_offscreen_pixels")).IsEqualTo(1);
        // The recording host's offscreen canvas reads back as transparent.
        await Assert.That(bitmap.Pixels[0]).IsEqualTo((byte)0);
    }

    [Test]
    public async Task TextRendersIntoThePixelsOnceItIsReady()
    {
        var white = new byte[4 * 2 * 4];
        Array.Fill(white, (byte)0xFF);
        var abi = TestRuntime.Start(new LayoutAbi { RenderedRgba = white });
        var bitmap = new Bitmap(4, 2);
        var label = new Text("Hi").FontSize(12.0f);
        label.Build();
        var ready = new List<Node>();

        Bitmap.WhenTextReady(label, e => ready.Add(e.Node));
        var beforeLoad = ready.Count;
        FrameScheduler.FireLoadedCallbacks();
        // The shaping PrepareText queued runs at the next commit; rendering before it would draw unshaped text.
        var beforeCommit = bitmap.Render(label);
        FrameScheduler.FlushCommit();
        var rendered = bitmap.Render(label);

        await Assert.That(beforeLoad).IsEqualTo(0);
        await Assert.That(ready.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(ready[0], label)).IsTrue();
        await Assert.That(abi.HasCall("ui_prepare_node", label.Handle)).IsTrue();
        await Assert.That(beforeCommit).IsFalse();
        await Assert.That(rendered).IsTrue();
        await Assert.That(bitmap.Pixels[31]).IsEqualTo((byte)0xFF);
    }

    [Test]
    public async Task TextChangedAfterItWasPreparedIsPreparedAgainBeforeItRenders()
    {
        // A change after preparing discards a detached node's shaping in the host, so rendering then would draw it stale.
        var white = new byte[4 * 2 * 4];
        Array.Fill(white, (byte)0xFF);
        var abi = TestRuntime.Start(new LayoutAbi { RenderedRgba = white });
        var bitmap = new Bitmap(4, 2);
        var label = new Text("Hi").FontSize(12.0f);
        label.Build();
        Bitmap.PrepareText(label);
        FrameScheduler.FlushCommit();
        label.TextColor = 0x112233FFu;
        abi.Calls.Clear();

        var afterChange = bitmap.Render(label);
        var preparedAgain = abi.HasCall("ui_prepare_node", label.Handle);
        var rendersCalled = abi.CountOf("fui_render_node_to_rgba");
        FrameScheduler.FlushCommit();
        var next = bitmap.Render(label);

        await Assert.That(afterChange).IsFalse();
        await Assert.That(preparedAgain).IsTrue();
        await Assert.That(rendersCalled).IsEqualTo(0);
        await Assert.That(next).IsTrue();
    }

    [Test]
    public async Task ADisposedBitmapReleasesItsTextureAndCanvas()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var bitmap = new Bitmap(1, 1);
        _ = bitmap.Canvas;

        bitmap.Dispose();
        bitmap.Dispose();
        var threw = false;
        try
        {
            _ = bitmap.Pixels;
        }
        catch (ObjectDisposedException)
        {
            threw = true;
        }

        await Assert.That(abi.CountOf("fui_canvas_destroy_offscreen")).IsEqualTo(1);
        await Assert.That(abi.HasCall("fui_bitmap_release", FirstTexture)).IsTrue();
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task SidesMustBePositive()
    {
        TestRuntime.Start(new LayoutAbi());
        var rejected = 0;
        foreach (var (width, height) in new[] { (0, 1), (1, -1) })
        {
            try
            {
                _ = new Bitmap(width, height);
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected++;
            }
        }

        await Assert.That(rejected).IsEqualTo(2);
    }
}
