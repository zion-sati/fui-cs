using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Testing;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class FlexBoxTests
{
    private static readonly string[] StyleCalls =
    [
        "ui_set_width", "ui_set_height", "ui_set_fill_width", "ui_set_fill_height", "ui_set_fill_width_percent",
        "ui_set_fill_height_percent", "ui_set_min_width", "ui_set_max_width", "ui_set_min_height", "ui_set_max_height",
        "ui_set_flex_direction", "ui_set_flex_wrap", "ui_set_flex_basis", "ui_set_justify_content", "ui_set_align_items",
        "ui_set_align_self", "ui_set_margin", "ui_set_padding", "ui_set_position_type", "ui_set_position",
        "ui_set_clip_to_bounds", "ui_set_box_style", "ui_set_bg_color", "ui_set_layer_effect", "ui_set_drop_shadow",
        "ui_set_background_blur", "ui_set_linear_gradient",
    ];

    [Test]
    public async Task BuildSendsLayoutThenVisualStyleForSetPropertiesOnly()
    {
        var abi = TestRuntime.Start();
        var box = new FlexBox()
            .Width(100.0f)
            .Height(50.0f, Unit.Percent)
            .MinWidth(10.0f)
            .FlexDirection(FlexDirection.Row)
            .FlexWrap(FlexWrap.Wrap)
            .FlexBasis(5.0f)
            .JustifyContent(JustifyContent.Center)
            .AlignItems(AlignItems.Center)
            .AlignSelf(AlignSelf.Stretch)
            .Margin(1.0f)
            .Padding(2.0f)
            .PositionAbsolute()
            .Position(3.0f, 4.0f)
            .Background(0x112233FFu)
            .Opacity(0.5f)
            .DropShadow(0x000000FFu, 1.0f, 2.0f, 3.0f)
            .BackgroundBlur(6.0f)
            .LinearGradient(0.0f, 0.0f, 1.0f, 1.0f, new GradientStop(0.0f, 0xFF0000FFu), new GradientStop(1.0f, 0x0000FFFFu));

        box.Build();

        await Assert.That(abi.Sequence(StyleCalls)).IsEqualTo(string.Join(",",
            "ui_set_width", "ui_set_height", "ui_set_min_width", "ui_set_flex_direction", "ui_set_flex_wrap", "ui_set_flex_basis",
            "ui_set_justify_content", "ui_set_align_items", "ui_set_align_self", "ui_set_margin", "ui_set_padding",
            "ui_set_position_type", "ui_set_position", "ui_set_clip_to_bounds", "ui_set_box_style", "ui_set_layer_effect",
            "ui_set_drop_shadow", "ui_set_background_blur", "ui_set_linear_gradient"));
        await Assert.That(abi.Arguments("ui_set_box_style", 1)).IsEqualTo("287454207");
        var gradient = abi.Named("ui_set_linear_gradient")[0].Arguments;
        await Assert.That(gradient[5]).IsEqualTo((object)2u);
        await Assert.That(string.Join(",", (uint[])gradient[7]!)).IsEqualTo("4278190335,65535");
    }

    [Test]
    public async Task AnUnstyledBoxSendsOnlyItsClipping()
    {
        var abi = TestRuntime.Start();

        new FlexBox().Build();

        await Assert.That(abi.Sequence(StyleCalls)).IsEqualTo("ui_set_clip_to_bounds");
        await Assert.That(abi.Arguments("ui_set_clip_to_bounds", 1)).IsEqualTo("False");
    }

    [Test]
    public async Task EverySetterUpdatesTheBuiltNodeWithoutRecreatingIt()
    {
        var cases = new List<(string Call, Action<FlexBox> Set)>
        {
            ("ui_set_width", box => box.Width(10.0f)),
            ("ui_set_height", box => box.Height(10.0f)),
            ("ui_set_fill_width", box => box.FillWidth()),
            ("ui_set_fill_height", box => box.FillHeight()),
            ("ui_set_fill_width_percent", box => box.FillWidthPercent(50.0f)),
            ("ui_set_fill_height_percent", box => box.FillHeightPercent(50.0f)),
            ("ui_set_min_width", box => box.MinWidth(1.0f)),
            ("ui_set_max_width", box => box.MaxWidth(1.0f)),
            ("ui_set_min_height", box => box.MinHeight(1.0f)),
            ("ui_set_max_height", box => box.MaxHeight(1.0f)),
            ("ui_set_flex_basis", box => box.FlexBasis(1.0f)),
            ("ui_set_box_style", box => box.Background(0xFFFFFFFFu)),
            ("ui_set_box_style", box => box.CornerRadius(4.0f)),
            ("ui_set_box_style", box => box.Border(1.0f, 0x000000FFu)),
            ("ui_set_flex_direction", box => box.FlexDirection(FlexDirection.Row)),
            ("ui_set_flex_wrap", box => box.FlexWrap(FlexWrap.Wrap)),
            ("ui_set_justify_content", box => box.JustifyContent(JustifyContent.End)),
            ("ui_set_align_items", box => box.AlignItems(AlignItems.End)),
            ("ui_set_align_self", box => box.AlignSelf(AlignSelf.End)),
            ("ui_set_padding", box => box.Padding(1.0f)),
            ("ui_set_margin", box => box.Margin(1.0f)),
            ("ui_set_position_type", box => box.PositionAbsolute()),
            ("ui_set_position", box => box.Position(1.0f, 2.0f)),
            ("ui_set_clip_to_bounds", box => box.ClipToBounds(false)),
            ("ui_set_layer_effect", box => box.Opacity(0.5f)),
            ("ui_set_layer_effect", box => box.Blur(2.0f)),
            ("ui_set_drop_shadow", box => box.DropShadow(0x000000FFu, 0.0f, 1.0f, 2.0f)),
            ("ui_set_background_blur", box => box.BackgroundBlur(4.0f)),
            ("ui_set_linear_gradient", box => box.LinearGradient(0.0f, 0.0f, 1.0f, 0.0f, new GradientStop(0.0f, 0xFFFFFFFFu))),
        };
        var failures = new List<string>();
        foreach (var (call, set) in cases)
        {
            var abi = TestRuntime.Start();
            var box = new FlexBox();
            box.Build();
            abi.Calls.Clear();
            set(box);
            if (abi.CountOf(call) != 1 || abi.CountOf("ui_create_node") != 0 || abi.CountOf("request_render") != 1)
            {
                failures.Add(call + ": " + string.Join(",", abi.Calls.ConvertAll(c => c.Name)));
            }
        }

        await Assert.That(string.Join(" | ", failures)).IsEqualTo("");
    }

    [Test]
    public async Task AFixedSizeAndAFillReplaceEachOther()
    {
        var abi = TestRuntime.Start();
        var box = new FlexBox().Width(40.0f).FillWidth().FillHeightPercent(30.0f).Height(20.0f);

        box.Build();

        await Assert.That(box.FillWidth).IsTrue();
        await Assert.That(box.FillHeightPercent).IsNull();
        await Assert.That(abi.Sequence(StyleCalls)).IsEqualTo("ui_set_fill_width,ui_set_height,ui_set_clip_to_bounds");
    }

    [Test]
    public async Task PaddingAndMarginAreSentLeftTopRightBottom()
    {
        var abi = TestRuntime.Start();
        var box = new FlexBox().Padding(1.0f, 2.0f, 3.0f, 4.0f).Margin(5.0f, 6.0f);

        box.Build();

        var padding = abi.Named("ui_set_padding")[0].Arguments;
        var margin = abi.Named("ui_set_margin")[0].Arguments;
        await Assert.That(string.Join(",", padding[1], padding[2], padding[3], padding[4])).IsEqualTo("1,2,3,4");
        await Assert.That(string.Join(",", margin[1], margin[2], margin[3], margin[4])).IsEqualTo("5,6,5,6");
    }

    [Test]
    public async Task LayoutChangesScheduleACommitAndReachTheParent()
    {
        var abi = TestRuntime.Start();
        var row = FlexBox.Row(new FlexBox(), new FlexBox());
        row.Build();
        abi.Calls.Clear();

        ((FlexBox)row.Children[0]).Width(100.0f, Unit.Percent);
        ((FlexBox)row.Children[1]).Width(50.0f, Unit.Percent);

        var warnings = abi.Named("fui_log").FindAll(call => (string)call.Arguments[0]! == "Warning/Layout");
        await Assert.That(warnings.Count).IsEqualTo(1);
        await Assert.That(((string)warnings[0].Arguments[1]!).Contains("FillWidth", StringComparison.Ordinal)).IsTrue();
        await Assert.That(((string)warnings[0].Arguments[1]!).EndsWith("[containerPath=root, childPath=root/0, childIndex=0]", StringComparison.Ordinal)).IsTrue();
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(1);
    }

    [Test]
    public async Task AGradientWithoutStopsIsIgnoredWithAWarning()
    {
        var abi = TestRuntime.Start();
        var box = new FlexBox();
        box.Build();
        abi.Calls.Clear();

        box.LinearGradient(0.0f, 0.0f, 1.0f, 1.0f);

        await Assert.That(box.LinearGradient).IsNull();
        await Assert.That(abi.CountOf("ui_set_linear_gradient")).IsEqualTo(0);
        await Assert.That(abi.CountOf("fui_log")).IsEqualTo(1);
    }

    [Test]
    public async Task ColorsPackChannelsAsRgba()
    {
        await Assert.That(Color.FromRgba(0x12, 0x34, 0x56, 0x78).Rgba).IsEqualTo(0x12345678u);
        await Assert.That(Color.FromHsl(0.0f, 1.0f, 0.5f).Rgba).IsEqualTo(0xFF0000FFu);
        await Assert.That(Color.FromHsl(-120.0f, 1.0f, 0.5f).Rgba).IsEqualTo(0x0000FFFFu);
        await Assert.That(((Color)0x000000FFu).Mix(0xFFFFFFFFu, 0.5f).Rgba).IsEqualTo(0x808080FFu);
        await Assert.That(((Color)0x11223344u).B).IsEqualTo((byte)0x33);
    }
}
