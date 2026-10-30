using System;
using System.Threading;
using System.Threading.Tasks;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>
/// The route's page: eight 300 px drawn cards in a wrapping row on a dark page. Once the page has loaded, a 25 ms tick
/// sweeps a value from 0 to 100 and back and feeds the plots values derived from it, and the paint canvas's hint is
/// rendered from rich text into its bitmap. Disposing the page stops the tick.
/// </summary>
internal sealed class DrawingPage : ManagedApplicationController
{
    private readonly Gauge gauge = new();
    private readonly BarChart chart = new();
    private readonly Waveform wave = new();
    private readonly Sparkline spark = new();
    private readonly PieChart pie = new();
    private readonly ScatterPlot scatter = new();
    private readonly DancingYarn yarn = new();
    private readonly PaintCanvas paintCanvas = new();
    private readonly CancellationTokenSource ticking = new();
    private readonly Node root;
    private float gaugeValue;
    private float gaugeDirection = 1.0f;

    public DrawingPage()
    {
        // The cards wrap to the window's width and the page scrolls when they are taller than it.
        root = new FlexBox()
            .FlexDirection(FlexDirection.Column)
            .FillSize()
            .Background(DrawingColors.Page)
            .Child(new ScrollBox().FillSize().Child(new FlexBox()
                .FlexDirection(FlexDirection.Row)
                .FlexWrap(FlexWrap.Wrap)
                .FillWidth()
                .Padding(24.0f, 24.0f, 24.0f, 24.0f)
                .Child(Card(gauge))
                .Child(Spacer())
                .Child(Card(chart))
                .Child(Spacer())
                .Child(Card(wave))
                .Child(Spacer())
                .Child(Card(spark))
                .Child(Spacer())
                .Child(Card(pie))
                .Child(Spacer())
                .Child(Card(scatter))
                .Child(Spacer())
                .Child(Card(yarn))
                .Child(Spacer())
                .Child(Card(paintCanvas))));
        FrameScheduler.OnLoaded(_ =>
        {
            RunTicks(ticking.Token);
            var hint = new RichText(
                    new Span("Draw ").Color(DrawingColors.Rgba(235, 238, 245, 210)).Italic(),
                    new Span("here").FontSize(30.0f).Color(Color.FromRgb(58, 197, 108)).FontFamily(Theme.Active.Value.Fonts.MonoFamily).Bold().Underline(),
                    new Span(" on this canvas!").Color(DrawingColors.Rgba(235, 238, 245, 210)).Bold())
                .FontFamily(Theme.Active.Value.Fonts.BodyFamily)
                .FontSize(24.0f)
                .Width(268.0f)
                .Height(268.0f);
            hint.Build();
            Bitmap.WhenTextReady(hint, e => paintCanvas.SetHintLabel(e.Node));
        });
    }

    public override Node GetRoot() => root;

    public override void Dispose()
    {
        ticking.Cancel();
        ticking.Dispose();
        base.Dispose();
    }

    private static CustomDrawable Card(CustomDrawable drawable) => drawable.Margin(0.0f, 0.0f, 0.0f, 16.0f);

    private static FlexBox Spacer() => new FlexBox().Width(16.0f).Height(1.0f);

    /// <summary>Ticks every 25 ms, each tick scheduled after the last, until the page is disposed.</summary>
    private async void RunTicks(CancellationToken cancellation)
    {
        try
        {
            while (true)
            {
                await Task.Delay(25, cancellation);
                Tick();
            }
        }
        catch (OperationCanceledException)
        {
            // The page was disposed.
        }
    }

    private void Tick()
    {
        gaugeValue += gaugeDirection * 2.0f;
        if (gaugeValue >= 100.0f)
        {
            gaugeValue = 100.0f;
            gaugeDirection = -1.0f;
        }
        if (gaugeValue <= 0.0f)
        {
            gaugeValue = 0.0f;
            gaugeDirection = 1.0f;
        }
        var value = gaugeValue;
        var sine = (float)Math.Sin(value / 100.0 * Math.PI);
        var cosine = (float)Math.Cos(value / 100.0 * Math.PI * 0.7);
        var distance = Math.Abs(value - 50.0f) * 2.0f;
        gauge.Value.Value = value;
        chart.PushValues(value, distance, (sine * 80.0f) + 20.0f, (cosine * 60.0f) + 40.0f);
        wave.PushValues(value, distance, (sine * 80.0f) + 20.0f, (cosine * 60.0f) + 40.0f);
        spark.Push(value);
        pie.PushValues(value, distance, (sine * 40.0f) + 30.0f, (cosine * 30.0f) + 20.0f);
        scatter.PushValues(
            (float)Math.Sin(value / 100.0 * Math.PI * 2.0),
            (float)Math.Cos(value / 100.0 * Math.PI * 2.0),
            (float)Math.Sin(value / 100.0 * Math.PI * 3.0),
            (float)Math.Sin(value / 100.0 * Math.PI * 1.5));
        yarn.Tick();
    }
}
