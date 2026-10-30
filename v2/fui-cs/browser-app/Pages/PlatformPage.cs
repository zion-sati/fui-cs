using System;
using System.Threading;
using System.Threading.Tasks;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>The host context the browser reports, navigation round-tripping its route, and a target for the debug input API.</summary>
internal static class PlatformPage
{
    public static Node Build()
    {
        var context = Platform.Current;
        var route = new Text(Navigation.CurrentRoute.Value).FontSize(14.0f).NodeId("route-log");
        Navigation.CurrentRoute.Changed += value => route.Text = value;
        var navigate = new FlexBox().NodeId("navigate").Width(80.0f).Height(30.0f).Background(LayoutPage.Blue).Cursor(CursorStyle.Pointer);
        navigate.OnPointerClick(_ => Navigation.NavigateTo("?page=platform#second"));

        var debugLog = new Text("none").FontSize(14.0f).NodeId("debug-log");
        var debugTarget = new FlexBox().NodeId("debug-target").Width(80.0f).Height(30.0f).Background(LayoutPage.Green);
        debugTarget.OnPointerClick(e => debugLog.Text = "clicked");

        // Page zoom: a target that counts its clicks, so hit testing can be checked while zoomed, and a toggle that
        // turns the framework's page zoom off through the application API.
        var zoomClicks = 0;
        var zoomLog = new Text("zoom target 0").FontSize(14.0f).NodeId("zoom-log");
        var zoomTarget = new FlexBox().NodeId("zoom-target").Width(60.0f).Height(40.0f).Background(LayoutPage.Blue).Cursor(CursorStyle.Pointer);
        zoomTarget.OnPointerClick(_ => zoomLog.Text = "zoom target " + (++zoomClicks).ToString());
        var zoomOff = new FlexBox().NodeId("zoom-off").Width(60.0f).Height(30.0f).Background(LayoutPage.Green).Cursor(CursorStyle.Pointer);
        zoomOff.OnPointerClick(_ => Application.SetPageZoomEnabled(false));

        var timers = new Text("waiting").FontSize(14.0f).NodeId("timer-log");
        _ = RunTimers(timers);

        return FlexBox.Column(
                new Text(Describe(context.Family)).FontSize(14.0f).NodeId("family-log"),
                new Text(Describe(context.Environment)).FontSize(14.0f).NodeId("environment-log"),
                new Text(context.Supports(HostCapability.BrowserHistory) ? "history" : "no history").FontSize(14.0f).NodeId("history-log"),
                new Text(KeyboardShortcuts.UndoLabel()).FontSize(14.0f).NodeId("undo-log"),
                navigate,
                route,
                debugTarget,
                debugLog,
                timers,
                zoomTarget,
                zoomLog,
                zoomOff)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    /// <summary>BCL timers drive UI updates: a delay, then three ticks of a periodic timer.</summary>
    private static async Task RunTimers(Text status)
    {
        await Task.Delay(100);
        status.Text = "delayed";
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        var ticks = 0;
        while (ticks < 3 && await timer.WaitForNextTickAsync())
        {
            ++ticks;
        }
        status.Text = "ticked " + ticks.ToString();
    }

    private static string Describe(PlatformFamily family) => family switch
    {
        PlatformFamily.Apple => "apple",
        PlatformFamily.Windows => "windows",
        PlatformFamily.Linux => "linux",
        _ => "unknown",
    };

    private static string Describe(HostEnvironment environment) => environment switch
    {
        HostEnvironment.Browser => "browser",
        HostEnvironment.Desktop => "desktop",
        HostEnvironment.Headless => "headless",
        _ => "unknown",
    };
}
