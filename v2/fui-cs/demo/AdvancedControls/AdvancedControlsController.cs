using System;
using System.Collections.Generic;
using System.Globalization;

using FuiCs;
using FuiCs.Demo.AdvancedControls.Fetch;
using FuiCs.Demo.AdvancedControls.Reorder;
using FuiCs.Demo.AdvancedControls.TextAreaDemo;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls;

/// <summary>
/// Drives the route: the transition and smooth-scroll showcase, the prime worker and its progress bar, and the reorder
/// and fetch sections, spliced into the reference's section order. The reference's external file drop joins with the
/// file support of 4.4G.
/// </summary>
internal sealed class AdvancedControlsController : IRoutePageLifecycleOwner
{
    /// <summary>The worker project's published folder, relative to the route's page.</summary>
    private const string WorkerPath = "workers/FuiCs.Demo.AdvancedControlsWorker";
    private const float MiddleOffset = AdvancedControlsView.AnimationScrollRowHeight * 6.0f;
    private const float FinalOffset = AdvancedControlsView.AnimationScrollRowHeight * 12.0f;
    private const float LogicalTailOffset = AdvancedControlsView.AnimationScrollLogicalContentHeight - AdvancedControlsView.AnimationScrollViewportHeight;
    private static readonly AnimationTiming ScrollTiming = new(TimeSpan.FromMilliseconds(260.0), Easings.CubicOut);

    public AdvancedControlsController()
    {
        View.AnimationPreviewCalmButton.Click += _ => SetAnimationPreview(false);
        View.AnimationPreviewEmphasisButton.Click += _ => SetAnimationPreview(true);
        View.AnimationScrollTopButton.Click += _ => AnimateScrollSamples("top", 0.0f);
        View.AnimationScrollMiddleButton.Click += _ => AnimateScrollSamples("midpoint", MiddleOffset);
        View.AnimationScrollBottomButton.Click += _ => AnimateScrollSamples("final", FinalOffset);
        View.AnimationScrollTailButton.Click += _ => AnimateScrollSamples("logical tail", LogicalTailOffset);
        View.WorkerStartButton.Click += _ => StartLargestPrimeCalculator();
        View.WorkerCancelButton.Click += _ => CancelLargestPrimeCalculator();
        SyncAnimationShowcase();
        SyncWorkerUi();
    }

    public AdvancedControlsModel Model { get; } = new();

    public AdvancedControlsView View { get; } = new();

    public TextAreaDemoSection TextAreaDemo { get; } = new();

    public ReorderDemoSection ReorderDemo { get; } = new();

    public FetchDemoSection FetchDemo { get; } = new();

    public IReadOnlyList<RoutePageSection> BuildSections()
    {
        var sections = View.BuildSections();
        return [TextAreaDemo.BuildSection(), sections[0], ReorderDemo.BuildSection(), sections[1], FetchDemo.BuildSection(), sections[2], sections[3], sections[4]];
    }

    public void ApplyTheme(Theme theme)
    {
        View.ApplyTheme(theme);
        View.SetAnimationPreviewState(Model.AnimationPreviewEmphasized, theme);
        SyncAnimationShowcase();
    }

    public void Dispose()
    {
        TextAreaDemo.Dispose();
        ReorderDemo.Dispose();
        FetchDemo.Dispose();
        Model.ActiveWorker?.Dispose();
        Model.ActiveWorker = null;
    }

    /// <summary>The first number in <paramref name="text"/>, clamped to 0..100; 0 when it has none.</summary>
    private static int ParseLeadingPercent(string text)
    {
        var value = 0;
        var sawDigit = false;
        foreach (var character in text)
        {
            if (character is >= '0' and <= '9')
            {
                sawDigit = true;
                value = Math.Min(1_000, (value * 10) + (character - '0'));
                continue;
            }
            if (sawDigit)
            {
                break;
            }
        }
        return sawDigit ? Math.Clamp(value, 0, 100) : 0;
    }

    private void SetAnimationPreview(bool emphasized)
    {
        Model.AnimationPreviewEmphasized = emphasized;
        View.SetAnimationPreviewState(emphasized, Theme.Active.Value);
        SyncAnimationShowcase();
    }

    private void AnimateScrollSamples(string targetLabel, float offsetY)
    {
        Model.AnimationScrollTargetLabel = targetLabel;
        View.AnimationScrollBox.ScrollToAnimated(0.0f, offsetY, ScrollTiming);
        SyncAnimationShowcase();
    }

    private void StartLargestPrimeCalculator()
    {
        if (Model.ActiveWorker is not null)
        {
            return;
        }
        Model.WorkerProgressPercent = 0;
        Model.WorkerStateLabel = "running";
        View.WorkerProgressBar.Value = 0.0f;
        View.WorkerDetailText.Text = "Largest-prime calculator is running for 5 seconds and yields once per second.";
        var worker = new Worker(WorkerPath, "largestPrimeCalculatorWorker")
            .OnProgress(HandleWorkerProgress)
            .OnCompleted(HandleWorkerComplete)
            .OnError(HandleWorkerError);
        Model.ActiveWorker = worker;
        SyncWorkerUi();
        worker.Start("advanced-controls-demo");
    }

    private void CancelLargestPrimeCalculator()
    {
        if (Model.ActiveWorker is not { } worker)
        {
            return;
        }
        Model.WorkerStateLabel = "cancelling";
        View.WorkerDetailText.Text = "Cancellation requested. Waiting for the worker to yield.";
        SyncWorkerUi();
        worker.Cancel();
    }

    private void HandleWorkerProgress(WorkerProgressEventArgs e)
    {
        Model.WorkerProgressPercent = ParseLeadingPercent(e.Message);
        Model.WorkerStateLabel = "running";
        View.WorkerProgressBar.Value = Model.WorkerProgressPercent;
        View.WorkerDetailText.Text = "Prime search progress: " + Model.WorkerProgressPercent.ToString(CultureInfo.InvariantCulture) + "%.";
        SyncWorkerUi();
    }

    private void HandleWorkerComplete(WorkerCompletedEventArgs e)
    {
        Model.ActiveWorker = null;
        Model.WorkerProgressPercent = 100;
        Model.WorkerStateLabel = "complete";
        View.WorkerProgressBar.Value = 100.0f;
        View.WorkerDetailText.Text = "Largest prime after 5s: " + e.Result;
        SyncWorkerUi();
    }

    private void HandleWorkerError(WorkerErrorEventArgs e)
    {
        Model.ActiveWorker = null;
        if (e.Message.StartsWith("cancelled:", StringComparison.Ordinal))
        {
            Model.WorkerProgressPercent = Math.Max(Model.WorkerProgressPercent, ParseLeadingPercent(e.Message));
            Model.WorkerStateLabel = "cancelled";
            View.WorkerDetailText.Text = "Prime search cancelled after yielding at " + Model.WorkerProgressPercent.ToString(CultureInfo.InvariantCulture) + "%.";
        }
        else
        {
            Model.WorkerStateLabel = "error";
            View.WorkerDetailText.Text = "Worker error: " + e.Message;
        }
        View.WorkerProgressBar.Value = Model.WorkerProgressPercent;
        SyncWorkerUi();
    }

    private void SyncWorkerUi()
    {
        View.WorkerStartButton.IsEnabled = Model.ActiveWorker is null;
        View.WorkerCancelButton.IsEnabled = Model.ActiveWorker is not null && Model.WorkerStateLabel != "cancelling";
        View.WorkerStatusText.Text = "Worker status: " + Model.WorkerStateLabel + " • Progress: " + Model.WorkerProgressPercent.ToString(CultureInfo.InvariantCulture) + "%";
    }

    private void SyncAnimationShowcase()
    {
        View.AnimationPreviewStatusText.Text = "Preview state: " + (Model.AnimationPreviewEmphasized ? "emphasized" : "calm") + " • Transition slots: bgColor + opacity";
        View.AnimationScrollStatusText.Text = "Smooth scroll target: " + Model.AnimationScrollTargetLabel
            + " • API: scrollToAnimated(0, offset, cubicOut)"
            + " • scrollContentSize(-1, " + ((int)AdvancedControlsView.AnimationScrollLogicalContentHeight).ToString(CultureInfo.InvariantCulture) + ")";
    }
}
