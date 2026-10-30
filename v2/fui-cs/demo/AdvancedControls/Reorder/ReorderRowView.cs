using System.Globalization;

using FuiCs;
using FuiCs.Demo.DesignSystem;

namespace FuiCs.Demo.AdvancedControls.Reorder;

/// <summary>
/// One slot of the reorder list: an insertion marker above a card with a drag grip. The slot is the drop target for
/// its index; the grip, and the label on it that receives the pointer, start a drag of the item the slot shows.
/// </summary>
internal sealed class ReorderRowView
{
    private readonly ReorderDemoSection section;
    private ReorderDemoItem? currentItem;
    private string? pendingDragItemId;

    public ReorderRowView(ReorderDemoSection section, int rawIndex)
    {
        this.section = section;
        RawIndex = rawIndex;
        Marker = new FlexBox()
            .FillWidth()
            .Height(ReorderDemoLogic.MarkerHeight)
            .CornerRadius(ReorderDemoLogic.MarkerHeight * 0.5f)
            .Opacity(0.0f);
        GripLabel = new DemoText("Drag", DemoTextRecipe.StatusValue)
            .FontSize(14.0f)
            .Selectable(false)
            .Cursor(CursorStyle.Grab)
            .DragData(ProvideDragData)
            .DragAllowedEffects(DragDropEffects.Move)
            .OnDragCompleted(e => HandleDragCompleted(e.Effect));
        Grip = new FlexBox()
            .Width(76.0f)
            .Height(40.0f)
            .CornerRadius(12.0f)
            .JustifyContent(JustifyContent.Center)
            .AlignItems(AlignItems.Center)
            .Child(GripLabel)
            .Cursor(CursorStyle.Grab)
            .SemanticRole(SemanticRole.Button)
            .SemanticLabel("Drag grip")
            .DragData(ProvideDragData)
            .DragAllowedEffects(DragDropEffects.Move)
            .OnDragCompleted(e => HandleDragCompleted(e.Effect));
        TitleText = new DemoText("", DemoTextRecipe.Body).FontSize(16.0f);
        DetailText = new DemoText("", DemoTextRecipe.Hint).FontSize(14.0f).MaxLines(2);
        Card = new FlexBox()
            .FillWidth()
            .Height(ReorderDemoLogic.RowBodyHeight)
            .Padding(16.0f, 14.0f, 16.0f, 14.0f)
            .CornerRadius(18.0f)
            .Child(FlexBox.Row(
                    Grip,
                    new FlexBox().Width(14.0f).Height(1.0f),
                    FlexBox.Column(TitleText, new FlexBox().FillWidth().Height(4.0f), DetailText).FillWidth())
                .FillWidth());
        Slot = FlexBox.Column(Marker, Card)
            .FillWidth()
            .AllowDrop()
            .OnDragEnter(e => e.Proposal = section.PreviewInsertion(e, RawIndex))
            .OnDragOver(e => e.Proposal = section.PreviewInsertion(e, RawIndex))
            .OnDragLeave(section.HandleTargetLeave)
            .OnDrop(section.DropAtPreview);
    }

    public int RawIndex { get; }

    public FlexBox Marker { get; }

    public Text GripLabel { get; }

    public FlexBox Grip { get; }

    public Text TitleText { get; }

    public Text DetailText { get; }

    public FlexBox Card { get; }

    public FlexBox Slot { get; }

    public void BindItem(ReorderDemoItem item)
    {
        currentItem = item;
        TitleText.Text = item.Label;
        DetailText.Text = item.Detail;
        Grip.SemanticLabel = "Drag grip for " + item.Label;
        Card.SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder item " + (RawIndex + 1).ToString(CultureInfo.InvariantCulture) + ": " + item.Label);
    }

    public void ApplyTheme(Theme theme, string? activeSourceId, int rawInsertionIndex)
    {
        var colors = theme.Colors;
        var isSource = currentItem is not null && currentItem.Id == activeSourceId;
        Marker.Background(colors.Accent).Opacity(rawInsertionIndex == RawIndex ? 1.0f : 0.0f);
        var resting = (RawIndex & 1) == 0 ? DemoColors.CardBackground(theme) : DemoColors.CardBackgroundAlt(theme);
        Card.Background(isSource ? colors.AccentHovered : resting).Border(1.0f, isSource ? colors.Accent : DemoColors.DividerColor(theme));
        TitleText.TextColor(isSource ? colors.Surface : DemoColors.PrimaryText(theme));
        DetailText.TextColor(isSource ? colors.Surface : DemoColors.MutedText(theme));
        var cursor = isSource ? CursorStyle.Grabbing : CursorStyle.Grab;
        Grip.Background(isSource ? colors.Accent : DemoColors.CardBackgroundAlt(theme))
            .Border(1.0f, isSource ? colors.AccentPressed : DemoColors.DividerColor(theme))
            .Cursor(cursor);
        GripLabel.TextColor(isSource ? colors.Surface : DemoColors.PrimaryText(theme)).Cursor(cursor);
    }

    private DragDataObject? ProvideDragData()
    {
        if (currentItem is null)
        {
            return null;
        }
        pendingDragItemId = currentItem.Id;
        return section.BeginDrag(currentItem);
    }

    private void HandleDragCompleted(DragDropEffects effect)
    {
        var draggedItemId = pendingDragItemId;
        pendingDragItemId = null;
        if (draggedItemId is not null)
        {
            section.CompleteDrag(draggedItemId, effect);
        }
    }
}
