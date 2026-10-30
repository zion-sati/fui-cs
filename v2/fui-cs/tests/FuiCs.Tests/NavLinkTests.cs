using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class NavLinkTests
{
    private const HostCapability Browser = HostCapability.BrowserHistory | HostCapability.Reload
        | HostCapability.ClipboardWrite | HostCapability.NewBrowsingContext | HostCapability.OpenExternalUri;

    private static (LayoutAbi Abi, NavLink Link) Mount(string href = "/docs/")
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f, HostCapabilities = Browser });
        var link = new NavLink(href).Child(new Text("Docs"));
        Application.Mount(FlexBox.Column(link));
        abi.Bounds[link.Handle] = new Rect(10.0f, 10.0f, 80.0f, 20.0f);
        return (abi, link);
    }

    /// <summary>A press and release at a point, with a button and modifiers, as the host sends them.</summary>
    private static void Click(ulong handle, float x, float y, PointerButton button = PointerButton.Primary, KeyModifier modifiers = KeyModifier.None)
    {
        foreach (var type in new[] { PointerEventType.Down, PointerEventType.Up })
        {
            HostEvents.OnPointerEvent((uint)type, handle, x, y, (uint)modifiers, 1, (uint)PointerType.Mouse, (int)button,
                button == PointerButton.Primary ? 1u : 4u, 0.5f, 1.0f, 1.0f, 1, true, 0.0f, 0.0f, 0.0f, 0.0f);
        }
    }

    [Test]
    public async Task ALinkIsAFocusableLinkWithAPointerCursor()
    {
        var (abi, link) = Mount();

        await Assert.That(abi.HasCall("ui_set_semantic_role", link.Handle, SemanticRole.Link)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_focusable", link.Handle, true)).IsTrue();
        await Assert.That(link.Cursor).IsEqualTo(CursorStyle.Pointer);
    }

    [Test]
    public async Task ClicksNavigateAndMiddleClicksOrTheShortcutModifierOpenANewTab()
    {
        var (abi, link) = Mount();
        var paths = new List<string>();
        link.Navigate += e => paths.Add(e.Path);

        Click(link.Handle, 20.0f, 15.0f);
        Click(link.Handle, 20.0f, 15.0f, PointerButton.Auxiliary);
        Click(link.Handle, 20.0f, 15.0f, modifiers: KeyModifier.Ctrl);
        Click(link.Handle, 20.0f, 15.0f, PointerButton.Secondary);

        await Assert.That(string.Join(",", paths)).IsEqualTo("/docs/,/docs/,/docs/");
        await Assert.That(abi.HasCall("fui_navigate_to", "/docs/", false)).IsTrue();
        await Assert.That(abi.Named("fui_navigate_to").Count).IsEqualTo(3);
        await Assert.That((bool)abi.Named("fui_navigate_to")[1].Arguments[1]!).IsTrue();
        await Assert.That((bool)abi.Named("fui_navigate_to")[2].Arguments[1]!).IsTrue();
    }

    [Test]
    public async Task HoveringAndClickingTheLinksContentCountAsTheLinks()
    {
        var (abi, link) = Mount();
        var text = (Text)link.Children[0];
        abi.Bounds[text.Handle] = new Rect(12.0f, 12.0f, 40.0f, 16.0f);

        HostInput.Pointer(PointerEventType.Enter, text.Handle, 20.0f, 15.0f);
        var previewed = abi.HasCall("fui_show_url_preview", "/docs/");
        HostInput.Pointer(PointerEventType.Leave, text.Handle, 70.0f, 15.0f);
        var stillHovered = link.InteractionState.IsHovered;
        Click(text.Handle, 20.0f, 15.0f);

        await Assert.That(previewed).IsTrue();
        await Assert.That(stillHovered).IsTrue();
        await Assert.That(abi.Named("fui_navigate_to").Count).IsEqualTo(1);
    }

    [Test]
    public async Task AReleaseOutsideTheLinkDoesNotNavigate()
    {
        var (abi, link) = Mount();

        HostInput.Pointer(PointerEventType.Down, link.Handle, 20.0f, 15.0f);
        HostInput.Pointer(PointerEventType.Up, link.Handle, 300.0f, 15.0f);

        await Assert.That(abi.Named("fui_navigate_to").Count).IsEqualTo(0);
    }

    [Test]
    public async Task EnterNavigatesAndTheShortcutModifierOpensANewTab()
    {
        var (abi, link) = Mount();
        link.OpenInNewTab = false;
        HostEvents.OnFocusChanged(link.Handle, true);

        HostInput.Key("Enter");
        HostInput.Key("Enter", KeyEventType.Up);
        HostInput.Key("Enter", modifiers: KeyModifier.Ctrl);
        HostInput.Key("Enter", KeyEventType.Up, KeyModifier.Ctrl);
        HostInput.Key("Enter", modifiers: KeyModifier.Shift);
        HostInput.Key("Enter", KeyEventType.Up, KeyModifier.Shift);

        await Assert.That(abi.Named("fui_navigate_to").Count).IsEqualTo(2);
        await Assert.That((bool)abi.Named("fui_navigate_to")[0].Arguments[1]!).IsFalse();
        await Assert.That((bool)abi.Named("fui_navigate_to")[1].Arguments[1]!).IsTrue();
    }

    [Test]
    public async Task HoverAndFocusPreviewTheUrl()
    {
        var (abi, link) = Mount();
        var states = new List<string>();
        link.OnInteractionStateChanged(state => states.Add(state.IsHovered ? "hovered" : "idle"));

        HostInput.Pointer(PointerEventType.Enter, link.Handle, 20.0f, 15.0f);
        var shown = abi.HasCall("fui_show_url_preview", "/docs/");
        HostInput.Pointer(PointerEventType.Leave, link.Handle, 200.0f, 15.0f);
        var hiddenAfterLeave = abi.CountOf("fui_hide_url_preview");
        HostEvents.OnFocusChanged(link.Handle, true);
        HostEvents.OnFocusChanged(link.Handle, false);

        await Assert.That(shown).IsTrue();
        await Assert.That(hiddenAfterLeave).IsEqualTo(1);
        await Assert.That(abi.CountOf("fui_show_url_preview")).IsEqualTo(2);
        await Assert.That(abi.CountOf("fui_hide_url_preview")).IsEqualTo(2);
        await Assert.That(string.Join(",", states)).IsEqualTo("idle,hovered,idle,idle,idle");
    }

    [Test]
    public async Task AContextMenuOverALinkOffersItAndKeepsItsPreviewWhileOpen()
    {
        var (abi, link) = Mount();
        var text = (Text)link.Children[0];
        HostInput.Pointer(PointerEventType.Enter, link.Handle, 20.0f, 15.0f);

        HostEvents.OnContextMenu(text.Handle, 20.0f, 15.0f);
        var menu = Application.ShellContextMenu!;
        var labels = new List<string>();
        foreach (var item in menu.Items)
        {
            labels.Add(item.IsSeparator ? "-" : item.Label);
        }
        HostInput.Pointer(PointerEventType.Leave, link.Handle, 200.0f, 15.0f);
        var hiddenWhileOpen = abi.CountOf("fui_hide_url_preview");
        menu.Hide();

        await Assert.That(string.Join("|", labels)).IsEqualTo("New Tab|Open|-|Copy|Select All|-|Reload Page");
        await Assert.That(menu.Items[0].Payload).IsEqualTo("/docs/");
        await Assert.That(hiddenWhileOpen).IsEqualTo(0);
        await Assert.That(abi.CountOf("fui_hide_url_preview")).IsEqualTo(1);
    }

    [Test]
    public async Task LinksImagesAndTheirContainersOwnALongPress()
    {
        var (_, link) = Mount();
        var image = Image.Load("assets/photo.png").Width(40.0f).Height(40.0f);
        var card = new FlexBox().Child(new FlexBox().Child(image));
        var blank = new FlexBox().Width(40.0f).Height(40.0f);
        ((FlexBox)link.Parent!).Child(card).Child(blank);
        var inner = new FlexBox().Width(10.0f).Height(10.0f);
        link.Child(inner);

        await Assert.That(HostEvents.ResolveLongPressOwner(inner.Handle)).IsEqualTo(link.Handle);
        await Assert.That(HostEvents.ResolveLongPressOwner(card.Handle)).IsEqualTo(card.Handle);
        // The blank box's container holds an image, so it owns the press; the items then depend on the point.
        await Assert.That(HostEvents.ResolveLongPressOwner(blank.Handle)).IsEqualTo(link.Parent!.Handle);
    }

    [Test]
    public async Task ALongPressWithNoItemsReleasesTheLinkPreview()
    {
        var abi = TestRuntime.Start(new LayoutAbi { HostCapabilities = HostCapability.None });
        var link = new NavLink("/docs/").Child(new FlexBox().Width(10.0f).Height(10.0f));
        Application.Mount(FlexBox.Column(link));

        var shown = ContextMenuManager.ShowForLongPress(link.Handle, 5.0f, 5.0f);

        await Assert.That(shown).IsFalse();
        await Assert.That(abi.CountOf("fui_show_url_preview")).IsEqualTo(1);
        await Assert.That(abi.CountOf("fui_hide_url_preview")).IsEqualTo(1);
    }

    [Test]
    public async Task LongPressItemsCoverLinksImagesAndTextButNotTheBlankPage()
    {
        var (abi, link) = Mount();
        var image = Image.Load("assets/photo.png").Width(40.0f).Height(40.0f);
        var blank = new FlexBox().Width(40.0f).Height(40.0f);
        ((FlexBox)link.Parent!).Child(image).Child(blank);

        string Labels(Node node)
        {
            var labels = new List<string>();
            foreach (var item in ContextMenuManager.BuildLongPressItems(node.Handle, 5.0f, 5.0f))
            {
                labels.Add(item.IsSeparator ? "-" : item.Label);
            }
            return string.Join("|", labels);
        }

        await Assert.That(Labels(link)).IsEqualTo("New Tab|Open");
        await Assert.That(Labels(image)).IsEqualTo("New Tab|Open");
        await Assert.That(Labels(link.Children[0])).IsEqualTo("New Tab|Open|-|Copy|Select All");
        await Assert.That(Labels(blank)).IsEqualTo(string.Empty);
        await Assert.That(abi.CountOf("ui_clear_current_selection")).IsEqualTo(0);
    }
}
