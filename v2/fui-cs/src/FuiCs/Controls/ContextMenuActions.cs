using FuiCs.Bindings;

namespace FuiCs;

/// <summary>Runs a menu item's built-in action through the host, if the host supports it.</summary>
internal static class ContextMenuActions
{
    public static void Run(MenuItem item)
    {
        if (!IsSupported(item.Action) || (!item.IsEnabled && !HasLiveSelection(item)))
        {
            return;
        }
        var target = item.TargetHandle;
        switch (item.Action)
        {
            case ContextMenuAction.CopyCurrentSelection when item.Payload is { } text:
                Runtime.Abi.HostCopyText(text);
                break;
            case ContextMenuAction.CopyCurrentSelection when target != 0:
                if (!Runtime.Abi.HostCopyTextSelectionSnapshot(target))
                {
                    Ui.CopyTextSelection(target);
                }
                break;
            case ContextMenuAction.CopyCurrentSelection:
                Ui.CopyCurrentSelection();
                return;
            case ContextMenuAction.SelectAllText when target != 0:
                Ui.SelectAllText(target);
                break;
            case ContextMenuAction.UndoTextEdit when target != 0:
                Ui.UndoTextEdit(target);
                break;
            case ContextMenuAction.RedoTextEdit when target != 0:
                Ui.RedoTextEdit(target);
                break;
            case ContextMenuAction.CutTextSelection when target != 0:
                Cut(item);
                break;
            case ContextMenuAction.PasteText when target != 0:
                Ui.PasteText(target);
                break;
            case ContextMenuAction.ReloadPage:
                Runtime.Abi.HostReloadPage();
                break;
            case ContextMenuAction.NavigateBack:
                Runtime.Abi.HostNavigateBack();
                break;
            case ContextMenuAction.NavigateForward:
                Runtime.Abi.HostNavigateForward();
                break;
            case ContextMenuAction.OpenLink or ContextMenuAction.OpenImage when item.Payload is { } url:
                Navigation.NavigateTo(url);
                break;
            case ContextMenuAction.OpenLinkInNewTab or ContextMenuAction.OpenImageInNewTab when item.Payload is { } url:
                Navigation.NavigateTo(url, openInNewTab: true);
                break;
        }
        if (item.FocusTargetAfterAction && target != 0)
        {
            Runtime.Abi.HostCommitTextActionFocus(target);
        }
    }

    /// <summary>
    /// A disabled Copy or Cut still runs while the target has a selection: the item was built before the selection the
    /// host kept for it reached the app.
    /// </summary>
    private static bool HasLiveSelection(MenuItem item) =>
        item.TargetHandle != 0
        && item.Action is ContextMenuAction.CopyCurrentSelection or ContextMenuAction.CutTextSelection
        && (Runtime.Abi.HostHasTextSelectionSnapshot(item.TargetHandle) || Ui.HasTextSelection(item.TargetHandle));

    /// <summary>
    /// Cuts through the host's selection snapshot, then the focused editor's range or selection, then the runtime's
    /// selection, writing the text the menu captured first.
    /// </summary>
    private static void Cut(MenuItem item)
    {
        var target = item.TargetHandle;
        if (item.Payload is { } text)
        {
            Runtime.Abi.HostCopyText(text);
        }
        if (Runtime.Abi.HostCutTextSelectionSnapshot(target))
        {
            return;
        }
        var (start, end) = item.SelectionRange;
        if (start != end && Runtime.Abi.HostDeleteFocusedTextRange(start, end))
        {
            return;
        }
        if (Runtime.Abi.HostCutFocusedTextSelection())
        {
            return;
        }
        if (item.Payload is null)
        {
            Runtime.Abi.HostCopyTextSelectionSnapshot(target);
        }
        Ui.CutTextSelection(target);
    }

    public static bool IsSupported(ContextMenuAction action) => action switch
    {
        ContextMenuAction.NavigateBack or ContextMenuAction.NavigateForward => Platform.Supports(HostCapability.BrowserHistory),
        ContextMenuAction.ReloadPage => Platform.Supports(HostCapability.Reload),
        ContextMenuAction.OpenLinkInNewTab or ContextMenuAction.OpenImageInNewTab => Platform.Supports(HostCapability.NewBrowsingContext),
        ContextMenuAction.OpenLink or ContextMenuAction.OpenImage => Platform.Supports(HostCapability.OpenExternalUri),
        ContextMenuAction.CopyCurrentSelection or ContextMenuAction.CutTextSelection => Platform.Supports(HostCapability.ClipboardWrite),
        ContextMenuAction.PasteText => Platform.Supports(HostCapability.ClipboardRead),
        _ => true,
    };
}
