using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class TextAreaTests
{
    private static (LayoutAbi Abi, TextArea Area) Mounted(TextArea area)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f });
        Application.Mount(FlexBox.Column(area));
        return (abi, area);
    }

    private static bool IsInside(Node node, Node ancestor)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }
        return false;
    }

    [Test]
    public async Task TheEditorSitsInAScrollBoxAndWrapsByDefault()
    {
        var (abi, area) = Mounted(new TextArea("one\ntwo"));
        var scroll = (ScrollBox)area.Children[0];

        await Assert.That(area.Wrapping).IsTrue();
        await Assert.That(area.Editor.Wrapping).IsTrue();
        await Assert.That(IsInside(area.Editor, scroll.Viewport)).IsTrue();
        await Assert.That(area.Editor.Width).IsEqualTo(Length.Percent(100.0f));
        await Assert.That(area.Editor.Height).IsEqualTo(Length.Auto);
        await Assert.That(area.Editor.VerticalAlign).IsEqualTo(TextVerticalAlign.Top);
        await Assert.That(abi.HasCall("ui_set_text_limits", (object)area.Editor.Handle, int.MaxValue, 0)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)area.Editor.Handle, "Text area")).IsTrue();
        await Assert.That(scroll.IsHorizontalScrollEnabled).IsFalse();
        await Assert.That(scroll.HorizontalScrollBarVisibility).IsEqualTo(ScrollBarVisibility.Hidden);
        // A single-line input does not wrap.
        await Assert.That(new TextInput().Wrapping).IsFalse();
    }

    [Test]
    public async Task TurningWrappingOffScrollsSidewaysWithTheChosenBars()
    {
        var (_, area) = Mounted(new TextArea("long line"));
        var scroll = (ScrollBox)area.Children[0];
        area.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        area.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

        // While it wraps, the horizontal bar stays hidden whatever was chosen.
        await Assert.That(scroll.HorizontalScrollBarVisibility).IsEqualTo(ScrollBarVisibility.Hidden);
        await Assert.That(scroll.VerticalScrollBarVisibility).IsEqualTo(ScrollBarVisibility.Hidden);

        area.Wrapping = false;
        await Assert.That(scroll.IsHorizontalScrollEnabled).IsTrue();
        await Assert.That(scroll.HorizontalScrollBarVisibility).IsEqualTo(ScrollBarVisibility.Visible);
        await Assert.That(area.Editor.Wrapping).IsFalse();
        await Assert.That(area.Editor.Width).IsEqualTo(Length.Auto);
    }

    [Test]
    public async Task APressOnTheViewportFocusesTheEditor()
    {
        var (abi, area) = Mounted(new TextArea("notes"));
        var scroll = (ScrollBox)area.Children[0];

        HostInput.Pointer(PointerEventType.Down, scroll.Viewport.Handle);
        await Assert.That(abi.HasCall("ui_request_focus", (object)area.Editor.Handle)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_text_selection_range", (object)area.Editor.Handle, 5u, 5u)).IsTrue();
    }
}
