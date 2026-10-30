using FuiCs;

namespace FuiCs.Browser;

/// <summary>Text selection, editing, rich-text runs, a loaded font and the system theme, rendered by the host.</summary>
internal static class TextPage
{
    public static readonly Color Marker = 0xFDE047FF;
    public static readonly Color Red = 0xDC2626FF;

    public static Node Build()
    {
        var selectionLog = Status("selection-log", "none");
        var selectable = new Text("Select this sentence")
            .FontSize(24.0f)
            .NodeId("selectable")
            .OnSelectionChanged(e => selectionLog.Text = e.Start.ToString() + "-" + e.End.ToString());

        var editLog = Status("edit-log", "empty");
        var editor = new TextCore()
            .Editable()
            .FontSize(20.0f)
            .Width(300.0f)
            .Height(32.0f)
            .NodeId("editor")
            .OnTextChanged(e => editLog.Text = e.Text.Length == 0 ? "empty" : e.Text);

        var rich = new RichText(
                new Span("plain "),
                new Span("marked").Background(Marker),
                new Span(" red").Color(Red))
            .FontSize(24.0f)
            .NodeId("rich");

        var fontLog = Status("font-log", "pending");
        var mono = FontStack.Load("fonts/NotoSansMono-Regular.ttf");
        FontFace.WhenLoaded(mono.Id, _ => fontLog.Text = "loaded");

        var themeLog = Status("theme-log", Theme.IsDarkMode ? "dark" : "light");
        Theme.Active.Changed += _ => themeLog.Text = Theme.IsDarkMode ? "dark" : "light";

        return FlexBox.Column(
                selectable,
                Gap(),
                new FlexBox().Border(1.0f, LayoutPage.Grey).Padding(4.0f).Child(editor),
                Gap(),
                rich,
                Gap(),
                FlexBox.Row(
                    new Text("iiiiiiii").FontStack(mono, 24.0f).NodeId("mono"),
                    new FlexBox().Width(20.0f),
                    new Text("iiiiiiii").FontSize(24.0f).NodeId("proportional")),
                Gap(),
                selectionLog,
                editLog,
                fontLog,
                themeLog)
            .NodeId("text-page")
            .Width(100.0f, Unit.Percent)
            .Height(100.0f, Unit.Percent)
            .Padding(20.0f)
            .AlignItems(AlignItems.Start);
    }

    private static FlexBox Gap() => new FlexBox().Height(16.0f);

    private static Text Status(string id, string initial) => new Text(initial).FontSize(14.0f).NodeId(id);
}
