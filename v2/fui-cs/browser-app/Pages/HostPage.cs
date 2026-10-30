using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// Generated host bindings: a button calls every probe host service and shows what came back, and the probe's host
/// events show the values the page sends.
/// </summary>
internal static class HostPage
{
    public static Node Build()
    {
        var servicesLog = Status("services-log", "not run");
        var eventsLog = Status("events-log", "no event");
        var pingLog = Status("ping-log", "0 pings");
        var pings = 0;
        Probe.Everything += (text, flag, number, unsigned, wide, unsignedWide, real, bytes, ints, uints, longs, ulongs, reals) =>
            eventsLog.Text = string.Join("|",
                text, Lower(flag), Format(number), Format(unsigned), Format(wide), Format(unsignedWide), Format(real),
                Join(bytes), Join(ints), Join(uints), Join(longs), Join(ulongs), Join(reals));
        Probe.Pinged += () => pingLog.Text = Format(++pings) + (pings == 1 ? " ping" : " pings");

        var run = new Button("Run services").NodeId("run-services").OnClick(_ =>
        {
            Probe.Record("done", [7u, 8u]);
            servicesLog.Text = string.Join("|",
                Probe.Greet("C#"),
                Lower(Probe.Invert(true)),
                Format(Probe.Add(2, 3)),
                Format(Probe.Complement(0u)),
                Format(Probe.NegateLong(9_007_199_254_740_993L)),
                Format(Probe.NextUlong(18_446_744_073_709_551_614UL)),
                Format(Probe.Half(5.0)),
                Join(Probe.ReverseBytes([1, 2, 3])),
                Join(Probe.DoubleInts([-1, 2])),
                Join(Probe.ComplementUints([0u, 1u])),
                Join(Probe.NegateLongs([9_007_199_254_740_993L])),
                Join(Probe.IncrementUlongs([18_446_744_073_709_551_614UL])),
                Join(Probe.HalveDoubles([1.0, 3.0])));
        });

        return FlexBox.Column(run, Gap(), servicesLog, eventsLog, pingLog).Padding(20.0f).AlignItems(AlignItems.Start);
    }

    private static string Lower(bool value) => value ? "true" : "false";

    private static string Format(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Format(uint value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Format(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Format(ulong value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Format(double value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Join(byte[] values) => string.Join(",", values);

    private static string Join(int[] values) => string.Join(",", values);

    private static string Join(uint[] values) => string.Join(",", values);

    private static string Join(long[] values) => string.Join(",", values);

    private static string Join(ulong[] values) => string.Join(",", values);

    private static string Join(double[] values)
    {
        var texts = new string[values.Length];
        for (var index = 0; index < values.Length; ++index)
        {
            texts[index] = Format(values[index]);
        }
        return string.Join(",", texts);
    }

    private static FlexBox Gap() => new FlexBox().Width(12.0f).Height(12.0f);

    private static Text Status(string id, string text) => new Text(text).FontSize(13.0f).NodeId(id).Selectable(false);
}
