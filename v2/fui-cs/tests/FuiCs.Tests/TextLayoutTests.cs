using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class TextLayoutTests
{
    private static readonly TextMetrics Measured = new(120.0f, 24.0f, 18.0f, 1u, 118.0f);

    [Test]
    public async Task ALayoutIsPreparedAndMeasuredOnceThePageHasLoaded()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var ready = new List<bool>();
        var layout = new TextLayout("Hello").FontSize(18.0f).OnReady(e => ready.Add(e.Layout.IsReady));
        var beforeLoad = ready.Count;
        abi.TextMetrics[1] = Measured;

        FrameScheduler.FireLoadedCallbacks();

        await Assert.That(beforeLoad).IsEqualTo(0);
        await Assert.That(string.Join(",", ready)).IsEqualTo("True");
        await Assert.That(layout.Measure()).IsEqualTo(Measured);
        await Assert.That(layout.MeasuredWidth).IsEqualTo(120.0f);
        await Assert.That(layout.MeasuredHeight).IsEqualTo(24.0f);
        await Assert.That(abi.HasCall("ui_set_text", 1ul, "Hello")).IsTrue();
        await Assert.That(abi.HasCall("ui_prepare_node", 1ul)).IsTrue();
        await Assert.That(abi.CountOf("ui_set_root")).IsEqualTo(0);
    }

    [Test]
    public async Task ALayoutWaitsForItsFonts()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        FrameScheduler.FireLoadedCallbacks();
        var stack = FontStack.Load("https://fonts.example/display.woff2");
        var ready = 0;

        var layout = new TextLayout("Title").FontStack(stack, 32.0f).OnReady(_ => ready++);
        var beforeFont = ready;
        HostEvents.OnFontLoaded(stack.Id);

        await Assert.That(beforeFont).IsEqualTo(0);
        await Assert.That(ready).IsEqualTo(1);
        await Assert.That(layout.IsReady).IsTrue();
        await Assert.That(abi.HasCall("ui_set_font", layout.DrawNode().Handle, stack.Id, 32.0f)).IsTrue();
    }

    [Test]
    public async Task ChangesMakeTheLayoutUnreadyAndMeasuringItThrows()
    {
        TestRuntime.Start(new LayoutAbi());
        FrameScheduler.FireLoadedCallbacks();
        var layout = new TextLayout("a").OnReady(_ => { });
        var readyBefore = layout.IsReady;

        layout.Text("b").Width(80.0f).MaxLines(2);
        var thrown = false;
        try
        {
            layout.Measure();
        }
        catch (InvalidOperationException)
        {
            thrown = true;
        }

        await Assert.That(readyBefore).IsTrue();
        await Assert.That(layout.IsReady).IsFalse();
        await Assert.That(thrown).IsTrue();
        await Assert.That(layout.Width).IsEqualTo(new Length(80.0f, Unit.Pixel));
        await Assert.That(layout.Text).IsEqualTo("b");
    }

    [Test]
    public async Task AFailedPreparationStillReportsButIsNotReady()
    {
        var abi = TestRuntime.Start(new LayoutAbi { PrepareResult = 0 });
        FrameScheduler.FireLoadedCallbacks();
        var reported = new List<bool>();

        new TextLayout("x").OnReady(e => reported.Add(e.Layout.IsReady));

        await Assert.That(string.Join(",", reported)).IsEqualTo("False");
        await Assert.That(abi.CountOf("ui_get_text_metrics")).IsEqualTo(0);
    }

    [Test]
    public async Task RichLayoutsSendStyleRuns()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        FrameScheduler.FireLoadedCallbacks();

        var layout = TextLayout.Rich(new Span("bold").Bold(), new Span(" plain")).OnReady(_ => { });

        await Assert.That(layout.Text).IsEqualTo("bold plain");
        await Assert.That(abi.HasCall("ui_set_text_style_runs", layout.DrawNode().Handle, 2u)).IsTrue();
    }

    [Test]
    public async Task ADisposedLayoutIsNeverPrepared()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var ready = 0;
        var layout = new TextLayout("gone").OnReady(_ => ready++);

        layout.Dispose();
        FrameScheduler.FireLoadedCallbacks();

        await Assert.That(ready).IsEqualTo(0);
        await Assert.That(abi.CountOf("ui_prepare_node")).IsEqualTo(0);
    }

    [Test]
    public async Task DynamicLayoutsSendTheirCharsetAndPrepareAcceptedChangesImmediately()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        FrameScheduler.FireLoadedCallbacks();
        var layout = DynamicTextLayout.Numeric().OnReady(_ => { });
        var handle = layout.DrawNode().Handle;
        abi.TextMetrics[handle] = Measured;

        var accepted = layout.TrySetText("42");

        await Assert.That(accepted).IsTrue();
        await Assert.That(layout.IsReady).IsTrue();
        await Assert.That(layout.Measure()).IsEqualTo(Measured);
        await Assert.That(abi.CountOf("ui_prepare_node")).IsEqualTo(2);
        await Assert.That(abi.HasCall("ui_set_dynamic_text_charset", handle, "0123456789.-")).IsTrue();
    }

    [Test]
    public async Task RejectingLayoutsKeepTheirTextWhenACharacterIsOutsideTheCharset()
    {
        TestRuntime.Start(new LayoutAbi());
        var strict = new DynamicTextLayout("abc").CharsetOverflow(DynamicTextOverflow.Reject);
        var lenient = new DynamicTextLayout("abc");

        var acceptedInside = strict.TrySetText("cab");
        var acceptedOutside = strict.TrySetText("abd");
        var thrown = false;
        try
        {
            strict.Text = "zzz";
        }
        catch (ArgumentException)
        {
            thrown = true;
        }

        await Assert.That(acceptedInside).IsTrue();
        await Assert.That(acceptedOutside).IsFalse();
        await Assert.That(thrown).IsTrue();
        await Assert.That(strict.Text).IsEqualTo("cab");
        await Assert.That(lenient.TrySetText("xyz")).IsTrue();
    }

    [Test]
    public async Task NumericLayoutsFormatValuesWithPrecisionPrefixAndSuffix()
    {
        TestRuntime.Start(new LayoutAbi());
        var layout = DynamicTextLayout.Numeric().Prefix("$").Precision(2);

        layout.TrySetValue(-3.14159);
        var twoDigits = layout.Text;
        layout.Suffix(" USD").Precision(0);
        var noDigits = layout.Text;
        layout.Precision = null;
        var shortest = layout.Text;
        layout.TrySetValue(double.NaN);
        var notANumber = layout.Text;
        var thrown = false;
        try
        {
            layout.Precision = -1;
        }
        catch (ArgumentOutOfRangeException)
        {
            thrown = true;
        }

        await Assert.That(twoDigits).IsEqualTo("$-3.14");
        await Assert.That(noDigits).IsEqualTo("$-3 USD");
        await Assert.That(shortest).IsEqualTo("$-3.14159 USD");
        await Assert.That(notANumber).IsEqualTo("$NaN USD");
        await Assert.That(layout.Charset).IsEqualTo("0123456789.-$ USD");
        await Assert.That(layout.Value is { } value && double.IsNaN(value)).IsTrue();
        await Assert.That(thrown).IsTrue();
    }
}
