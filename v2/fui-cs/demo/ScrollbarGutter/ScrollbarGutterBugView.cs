using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.ScrollbarGutter;

/// <summary>
/// An inner scroll box with an automatic vertical bar inside a column with a minimum size, inside an outer scroll box
/// with automatic bars on both axes: the outer box's bars appear or vanish as the window resizes, and neither box may
/// flip its bars back and forth.
/// </summary>
internal sealed class ScrollbarGutterBugView
{
    public ScrollbarGutterBugView(ScrollbarGutterBugModel model)
    {
        _ = model;
        var cardBackground = DemoColors.CardBackground(Theme.Active.Value);

        // Inner scroll content: overflows vertically.
        var innerContent = new FlexBox().FillWidth();
        for (var i = 0; i < 6; i += 1)
        {
            innerContent.Child(new FlexBox()
                .FillWidth()
                .Height(48.0f)
                .CornerRadius(4.0f)
                .Background(cardBackground)
                .Child(new DemoText("Inner row " + (i + 1).ToString(CultureInfo.InvariantCulture), DemoTextRecipe.Body)));
            if (i < 5)
            {
                innerContent.Child(VerticalSpacer(4.0f));
            }
        }

        var innerScrollBox = new DemoScrollBox()
            .NodeId("innerBugScrollBox")
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .FillWidth()
            .Height(140.0f)
            .Child(innerContent);

        // Content blocks fill the column.
        var innerColumn = FlexBox.Column(
                new DemoText("Scrollbar Gutter Bug Repro", DemoTextRecipe.SectionTitle),
                VerticalSpacer(12.0f),
                innerScrollBox,
                VerticalSpacer(12.0f))
            .FillSize()
            .MinWidth(800.0f)
            .MinHeight(600.0f)
            .Padding(24.0f, 24.0f, 24.0f, 24.0f);

        // Extra blocks force the outer box to overflow vertically in shorter windows.
        for (var i = 0; i < 8; i += 1)
        {
            innerColumn.Child(new FlexBox()
                .FillWidth()
                .Height(56.0f)
                .CornerRadius(4.0f)
                .Background(cardBackground)
                .Child(new DemoText("Block " + (i + 1).ToString(CultureInfo.InvariantCulture), DemoTextRecipe.Body)));
            innerColumn.Child(VerticalSpacer(6.0f));
        }

        // The outer scroll box wraps the column.
        var outerScrollBox = new DemoScrollBox()
            .NodeId("outerBugScrollBox")
            .HorizontalScrollEnabled(true)
            .VerticalScrollEnabled(true)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Child(innerColumn)
            .FillSize();

        Root = new SelectionArea().FillSize().Child(outerScrollBox);
    }

    public SelectionArea Root { get; }

    private static FlexBox VerticalSpacer(float height) => new FlexBox().FillWidth().Height(height);
}
