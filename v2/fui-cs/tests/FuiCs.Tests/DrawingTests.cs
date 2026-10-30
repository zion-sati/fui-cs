using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;
using FuiCs.Testing;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DrawingTests
{
    private const uint Canvas = 0x30000000;

    private sealed class Sketch(Action<DrawContext> render) : CustomDrawable
    {
        protected override void OnRender(DrawContext context) => render(context);
    }

    private static uint F(float value) => BitConverter.SingleToUInt32Bits(value);

    private static uint[] DrawnWords(RecordingFuiAbi abi) => (uint[])abi.Named("fui_canvas_draw_batch")[0].Arguments[1]!;

    private static (LayoutAbi Abi, Sketch Node) Mount(Action<DrawContext> render, CornerRadius radius = default)
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var node = new Sketch(render);
        node.CornerRadius = radius;
        node.Build();
        abi.Bounds[node.Handle] = new Rect(10.0f, 20.0f, 120.0f, 80.0f);
        abi.Calls.Clear();
        return (abi, node);
    }

    [Test]
    public async Task ACustomDrawableIsBuiltAsACustomDrawnBox()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var node = new Sketch(_ => { });

        var handle = node.Build();

        await Assert.That(abi.HasCall("ui_create_node", NodeType.FlexBox)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_custom_drawable", handle, true)).IsTrue();
    }

    [Test]
    public async Task ARenderPassSendsOneBatchClippedToTheNode()
    {
        var (abi, node) = Mount(context => context.DrawRect(1.0f, 2.0f, 3.0f, 4.0f, Paint.FilledStroke(0xFF0000FFu, 0x0000FFFFu, 2.0f)));

        HostEvents.OnCustomDraw(node.Handle, Canvas);

        await Assert.That(abi.CountOf("fui_canvas_draw_batch")).IsEqualTo(1);
        await Assert.That(abi.Named("fui_canvas_draw_batch")[0].Arguments[0]).IsEqualTo((object)Canvas);
        await Assert.That(string.Join(",", DrawnWords(abi))).IsEqualTo(string.Join(",",
            1u,
            6u, F(0.0f), F(0.0f), F(120.0f), F(80.0f),
            10u, F(1.0f), F(2.0f), F(3.0f), F(4.0f), 0xFF0000FFu, 0x0000FFFFu, F(2.0f),
            2u));
    }

    [Test]
    public async Task RoundedNodesClipToTheirCornerRadii()
    {
        var (abi, node) = Mount(_ => { }, new CornerRadius(4.0f, 5.0f, 6.0f, 7.0f));

        HostEvents.OnCustomDraw(node.Handle, Canvas);

        await Assert.That(string.Join(",", DrawnWords(abi))).IsEqualTo(string.Join(",",
            1u,
            7u, F(0.0f), F(0.0f), F(120.0f), F(80.0f), F(4.0f), F(5.0f), F(6.0f), F(7.0f),
            2u));
    }

    [Test]
    public async Task EveryCommandUsesTheHostWordLayout()
    {
        PathGeometry? path = null;
        var (abi, node) = Mount(context =>
        {
            context.Translate(1.0f, 2.0f);
            context.Scale(2.0f, 3.0f);
            context.Rotate(45.0f);
            context.DrawCircle(5.0f, 6.0f, 7.0f, Paint.Fill(0x00FF00FFu));
            context.DrawLine(0.0f, 1.0f, 2.0f, 3.0f, 0x112233FFu, 1.5f);
            context.DrawRoundRect(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, Paint.Stroke(0x445566FFu, 2.0f));
            context.DrawPath(path!, Paint.Fill(0x778899FFu));
            context.DrawImage(9, 1.0f, 2.0f, 3.0f, 4.0f, ImageSampling.Anisotropic(4));
            context.DrawSvg(8, 1.0f, 2.0f, 3.0f, 4.0f);
        });
        path = new PathGeometry().MoveTo(0.0f, 0.0f).LineTo(10.0f, 0.0f).Close();
        abi.Calls.Clear();

        HostEvents.OnCustomDraw(node.Handle, Canvas);

        var words = DrawnWords(abi);
        var body = words.AsSpan(6, words.Length - 7).ToArray();
        await Assert.That(string.Join(",", body)).IsEqualTo(string.Join(",",
            3u, F(1.0f), F(2.0f),
            4u, F(2.0f), F(3.0f),
            5u, F(45.0f),
            11u, F(5.0f), F(6.0f), F(7.0f), 0x00FF00FFu, 0u, F(0.0f),
            12u, F(0.0f), F(1.0f), F(2.0f), F(3.0f), 0x112233FFu, F(1.5f),
            13u, F(1.0f), F(2.0f), F(3.0f), F(4.0f), F(5.0f), F(6.0f), 0u, 0x445566FFu, F(2.0f),
            20u, 0u, 0x778899FFu, 0u, F(0.0f),
            31u, 9u, F(1.0f), F(2.0f), F(3.0f), F(4.0f), (uint)ImageSamplingKind.Anisotropic, 4u,
            32u, 8u, F(1.0f), F(2.0f), F(3.0f), F(4.0f)));
    }

    [Test]
    public async Task PathsBuildOnTheHostAndAreReleasedOnce()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var path = new PathGeometry().MoveTo(1.0f, 2.0f).QuadTo(3.0f, 4.0f, 5.0f, 6.0f).CubicTo(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f)
            .AddRect(0.0f, 0.0f, 4.0f, 4.0f).AddCircle(2.0f, 2.0f, 1.0f).Close();

        path.Dispose();
        path.Dispose();
        var thrown = false;
        try
        {
            path.LineTo(1.0f, 1.0f);
        }
        catch (ObjectDisposedException)
        {
            thrown = true;
        }

        await Assert.That(abi.Sequence("fui_path_create", "fui_path_move_to", "fui_path_quad_to", "fui_path_cubic_to", "fui_path_add_rect",
            "fui_path_add_circle", "fui_path_close", "fui_path_destroy")).IsEqualTo(
            "fui_path_create,fui_path_move_to,fui_path_quad_to,fui_path_cubic_to,fui_path_add_rect,fui_path_add_circle,fui_path_close,fui_path_destroy");
        await Assert.That(thrown).IsTrue();
    }

    [Test]
    public async Task TextLayoutsDrawOnceReadyAndRefuseBefore()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        FrameScheduler.FireLoadedCallbacks();
        var unready = new TextLayout("late");
        var ready = new TextLayout("ready").OnReady(_ => { });
        var thrown = false;
        var node = new Sketch(context =>
        {
            context.DrawTextLayout(ready, 3.0f, 4.0f);
            try
            {
                context.DrawTextLayout(unready, 0.0f, 0.0f);
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
        });
        node.Build();
        abi.Calls.Clear();

        HostEvents.OnCustomDraw(node.Handle, Canvas);

        var handle = ready.DrawNode().Handle;
        var words = DrawnWords(abi);
        await Assert.That(string.Join(",", words.AsSpan(6, 5).ToArray())).IsEqualTo(string.Join(",",
            30u, (uint)handle, (uint)(handle >> 32), F(3.0f), F(4.0f)));
        await Assert.That(thrown).IsTrue();
    }

    [Test]
    public async Task OnlyCustomDrawnNodesAreRendered()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var plain = new FlexBox();
        plain.Build();

        new Sketch(_ => { }).Build();
        HostEvents.OnCustomDraw(plain.Handle, Canvas);

        await Assert.That(abi.CountOf("fui_canvas_draw_batch")).IsEqualTo(0);
    }

    [Test]
    public async Task InvalidateVisualRepaintsOnlyAVisibleNode()
    {
        var (abi, node) = Mount(_ => { });

        node.InvalidateVisual();
        var whenVisible = abi.CountOf("request_render");
        abi.Bounds.Remove(node.Handle);
        abi.Calls.Clear();
        FrameScheduler.FlushCommit();
        abi.Calls.Clear();
        node.InvalidateVisual();

        await Assert.That(whenVisible).IsEqualTo(1);
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(0);
    }
}
