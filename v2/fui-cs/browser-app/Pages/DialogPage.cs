using FuiCs;

namespace FuiCs.Browser;

/// <summary>A modal dialog opened from a button; status lines report the result, the opener's focus and page clicks.</summary>
internal static class DialogPage
{
    public static Node Build()
    {
        var result = Status("result", "none");
        var focus = Status("opener-focus", "blurred");
        var pageClicks = Status("page-clicks", "0");
        var dialog = new Dialog("Delete file?", "The file is removed from the project.")
            .OnAccepted(() => result.Text = "accepted")
            .OnCancelled(() => result.Text = "cancelled");
        dialog.TitleText.NodeId("dialog-title");
        dialog.AcceptButton.NodeId("dialog-ok");
        dialog.CancelButton.NodeId("dialog-cancel");

        var opener = new Button("Open dialog").NodeId("open").OnClick(_ => dialog.Show());
        opener.OnFocusChanged(e => focus.Text = e.Focused ? "focused" : "blurred");
        var under = new Button("Under").NodeId("under").OnClick(_ => pageClicks.Text = (int.Parse(pageClicks.Text) + 1).ToString());

        return FlexBox.Column(
                FlexBox.Row(opener, Gap(), under).AlignItems(AlignItems.Center),
                Gap(),
                result,
                focus,
                pageClicks,
                dialog)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Gap() => new FlexBox().Width(16.0f).Height(16.0f);

    private static Text Status(string id, string text) => new Text(text).FontSize(14.0f).NodeId(id);
}
