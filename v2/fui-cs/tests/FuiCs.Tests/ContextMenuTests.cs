using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ContextMenuTests
{
    private const HostCapability AllCapabilities = HostCapability.BrowserHistory | HostCapability.Reload
        | HostCapability.ClipboardWrite | HostCapability.NewBrowsingContext | HostCapability.OpenExternalUri;

    private static LayoutAbi Start(HostCapability capabilities = AllCapabilities) =>
        TestRuntime.Start(new LayoutAbi { ViewportWidth = 800.0f, ViewportHeight = 600.0f, HostCapabilities = capabilities });

    private static List<ContextMenuEntry> Entries(ContextMenu menu)
    {
        var entries = new List<ContextMenuEntry>();
        Collect(menu, entries);
        return entries;
    }

    private static void Collect(Node node, List<ContextMenuEntry> entries)
    {
        foreach (var child in node.Children)
        {
            if (child is ContextMenuEntry entry)
            {
                entries.Add(entry);
            }
            Collect(child, entries);
        }
    }

    private static string Labels(ContextMenu menu)
    {
        var labels = new List<string>();
        foreach (var item in menu.Items)
        {
            labels.Add(item.IsSeparator ? "-" : item.Label + (item.IsEnabled ? "" : "(off)"));
        }
        return string.Join("|", labels);
    }

    /// <summary>Clicks an entry as the host would: press and release inside it and its panel.</summary>
    private static void Choose(LayoutAbi abi, ContextMenuEntry entry)
    {
        abi.Bounds[entry.Parent!.Handle] = new Rect(96.0f, 96.0f, 220.0f, 120.0f);
        abi.Bounds[entry.Handle] = new Rect(100.0f, 100.0f, 200.0f, 28.0f);
        HostInput.Pointer(PointerEventType.Down, entry.Handle, 150.0f, 110.0f);
        HostInput.Pointer(PointerEventType.Up, entry.Handle, 150.0f, 110.0f);
    }

    [Test]
    public async Task ARequestOverThePageShowsHistoryAndReload()
    {
        var abi = Start();
        abi.CanNavigateBack = true;
        var page = new FlexBox().Width(200.0f).Height(100.0f);
        Application.Mount(FlexBox.Column(page));

        HostEvents.OnContextMenu(page.Handle, 40.0f, 50.0f);
        var menu = Application.ShellContextMenu!;

        await Assert.That(menu.IsOpen).IsTrue();
        await Assert.That(Labels(menu)).IsEqualTo("Back|Reload Page");
        await Assert.That(menu.Items[0].ShortcutLabel).IsEqualTo(KeyboardShortcuts.FormatLabel("ArrowLeft", KeyModifier.Alt, PlatformFamily.Linux));
        await Assert.That(Entries(menu).Count).IsEqualTo(2);
        await Assert.That(abi.HasCall("ui_set_semantic_label", (object)Entries(menu)[0].Handle, "Back")).IsTrue();
    }

    [Test]
    public async Task AnAppMenuShowsTheBuiltInItemsUntilANewRootIsMounted()
    {
        var abi = Start();
        abi.CanNavigateBack = true;
        var page = new FlexBox().Width(200.0f).Height(100.0f);
        var appMenu = new ContextMenu().PanelColor(0x112233FFu);
        Application.Mount(FlexBox.Column(page, appMenu));
        ContextMenuManager.SetMenu(appMenu);

        HostEvents.OnContextMenu(page.Handle, 40.0f, 50.0f);
        var shownInApp = (appMenu.IsOpen, Labels(appMenu), Application.ShellContextMenu!.IsOpen);
        appMenu.Hide();
        var nextPage = new FlexBox().Width(200.0f).Height(100.0f);
        Application.Mount(FlexBox.Column(nextPage));
        HostEvents.OnContextMenu(nextPage.Handle, 40.0f, 50.0f);

        await Assert.That(shownInApp).IsEqualTo((true, "Back|Reload Page", false));
        await Assert.That(Application.ShellContextMenu!.IsOpen).IsTrue();
        await Assert.That(Labels(Application.ShellContextMenu)).IsEqualTo("Back|Reload Page");
    }

    [Test]
    public async Task ChoosingAnItemHidesTheMenuAndRunsIt()
    {
        var abi = Start();
        abi.CanNavigateBack = true;
        var page = new FlexBox().Width(200.0f).Height(100.0f);
        Application.Mount(FlexBox.Column(page));
        HostEvents.OnContextMenu(page.Handle, 40.0f, 50.0f);
        var menu = Application.ShellContextMenu!;
        var visibility = new List<bool>();
        menu.VisibilityChanged += e => visibility.Add(e.Visible);

        Choose(abi, Entries(menu)[0]);

        await Assert.That(menu.IsOpen).IsFalse();
        await Assert.That(abi.CountOf("fui_navigate_back")).IsEqualTo(1);
        await Assert.That(string.Join(",", visibility)).IsEqualTo("False");
        await Assert.That(ContextMenu.Active).IsNull();
    }

    [Test]
    public async Task SelectableTextOffersCopyAndSelectAll()
    {
        var abi = Start();
        var text = new Text("Hello").Selectable(true);
        Application.Mount(FlexBox.Column(text));

        HostEvents.OnContextMenu(text.Handle, 5.0f, 5.0f);
        var menu = Application.ShellContextMenu!;
        var labels = Labels(menu);
        Choose(abi, Entries(menu)[1]);

        await Assert.That(labels).IsEqualTo("Copy(off)|Select All|-|Reload Page");
        await Assert.That(abi.HasCall("ui_select_all_text", (object)text.Handle)).IsTrue();
    }

    [Test]
    public async Task CopyTakesTheTextSelectionAsItIsWhenTheMenuOpens()
    {
        var abi = Start();
        var text = new Text("Café crème").Selectable(true);
        Application.Mount(FlexBox.Column(text));

        // UTF-8 offsets 6..12 select "crème": "é" takes two bytes.
        HostEvents.OnSelectionChanged(text.Handle, 6, 12);
        HostEvents.OnContextMenu(text.Handle, 5.0f, 5.0f);
        var menu = Application.ShellContextMenu!;
        HostEvents.OnSelectionChanged(text.Handle, 0, 0);
        Choose(abi, Entries(menu)[0]);

        await Assert.That(menu.Items[0].Payload).IsEqualTo("crème");
        await Assert.That(abi.HasCall("fui_copy_text", "crème")).IsTrue();
    }

    [Test]
    public async Task MenuHandlersAndDisabledRegionsAreHitTargets()
    {
        var abi = Start();
        var handled = new FlexBox().OnContextMenuOpening(_ => { });
        var disabled = new FlexBox().IsContextMenuEnabled(false);
        var plain = new FlexBox();
        Application.Mount(FlexBox.Column(handled, disabled, plain));

        await Assert.That(abi.HasCall("ui_set_interactive", (object)handled.Handle, true)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_interactive", (object)disabled.Handle, true)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_interactive", (object)plain.Handle, true)).IsFalse();
    }

    [Test]
    public async Task ARequestOverThePageSelectionCopiesIt()
    {
        var abi = Start();
        var box = new FlexBox().Width(200.0f).Height(100.0f);
        Application.Mount(FlexBox.Column(box));

        HostInput.Pointer(PointerEventType.Down, box.Handle, 10.0f, 10.0f);
        HostEvents.OnCrossSelectionChanged(box.Handle, "picked text");
        HostEvents.OnContextMenu(box.Handle, 10.0f, 10.0f);
        var menu = Application.ShellContextMenu!;
        var labels = Labels(menu);
        Choose(abi, Entries(menu)[0]);

        await Assert.That(labels).IsEqualTo("Copy|-|Reload Page");
        await Assert.That(abi.HasCall("fui_copy_text", "picked text")).IsTrue();
    }

    [Test]
    public async Task ImagesOfferToOpenTheirUrlButInlineDataDoesNot()
    {
        var abi = Start(HostCapability.NewBrowsingContext | HostCapability.OpenExternalUri);
        var image = Image.Load("assets/photo.png").Width(40.0f).Height(40.0f);
        var inline = Svg.Load("data:image/svg+xml;utf8,<svg/>").Width(40.0f).Height(40.0f);
        Application.Mount(FlexBox.Column(image, inline));

        HostEvents.OnContextMenu(image.Handle, 5.0f, 5.0f);
        var menu = Application.ShellContextMenu!;
        var labels = Labels(menu);
        Choose(abi, Entries(menu)[0]);
        HostEvents.OnContextMenu(inline.Handle, 5.0f, 45.0f);

        await Assert.That(labels).IsEqualTo("New Tab|Open");
        await Assert.That(abi.HasCall("fui_navigate_to", "assets/photo.png", true)).IsTrue();
        await Assert.That(menu.IsOpen).IsFalse();
    }

    [Test]
    public async Task AHandlerReplacesTheBuiltInMenuAndDisablingHidesIt()
    {
        Start();
        var requests = new List<string>();
        var leaf = new FlexBox().Width(20.0f).Height(20.0f);
        var owner = new FlexBox().Child(leaf).OnContextMenuOpening(e => requests.Add($"{ReferenceEquals(e.Target, leaf)}@{e.X},{e.Y}"));
        var quiet = new FlexBox().Width(20.0f).Height(20.0f);
        var disabled = new FlexBox().IsContextMenuEnabled(false).Child(quiet);
        Application.Mount(FlexBox.Column(owner, disabled));

        HostEvents.OnContextMenu(leaf.Handle, 3.0f, 4.0f);
        var shownAfterHandler = Application.ShellContextMenu!.IsOpen;
        HostEvents.OnContextMenu(quiet.Handle, 3.0f, 30.0f);

        await Assert.That(string.Join(",", requests)).IsEqualTo("True@3,4");
        await Assert.That(shownAfterHandler).IsFalse();
        await Assert.That(Application.ShellContextMenu!.IsOpen).IsFalse();
        await Assert.That(HostEvents.CanShowContextMenu(quiet.Handle)).IsFalse();
        await Assert.That(HostEvents.CanShowContextMenu(leaf.Handle)).IsTrue();
    }

    [Test]
    public async Task AnAppMenuRunsItsItemsAndDropsStraySeparators()
    {
        var abi = Start();
        var log = new List<string>();
        var menu = new ContextMenu().Items(
            MenuItem.Separator,
            new MenuItem("Rename", () => log.Add("rename")),
            MenuItem.Separator,
            MenuItem.Separator,
            new MenuItem("Delete", () => log.Add("delete")) { IsEnabled = false },
            MenuItem.Separator);
        var anchor = new FlexBox().Width(50.0f).Height(20.0f);
        Application.Mount(FlexBox.Column(anchor, menu));
        abi.Bounds[anchor.Handle] = new Rect(30.0f, 40.0f, 50.0f, 20.0f);

        menu.Show(anchor, 5.0f, 5.0f);
        var entries = Entries(menu);
        var panel = entries[0].Parent!;
        var separators = 0;
        foreach (var child in panel.Children)
        {
            separators += child is ContextMenuSeparator ? 1 : 0;
        }
        Choose(abi, entries[1]);
        var openAfterDisabled = menu.IsOpen;
        Choose(abi, entries[0]);

        await Assert.That(separators).IsEqualTo(1);
        await Assert.That(openAfterDisabled).IsTrue();
        await Assert.That(string.Join(",", log)).IsEqualTo("rename");
        await Assert.That(menu.IsOpen).IsFalse();
        await Assert.That(abi.HasCall("ui_set_position", (object)panel.Handle, 35.0f, 45.0f)).IsTrue();
    }

    [Test]
    public async Task EscapeAndAPressOutsideHideTheMenuAndThePressGoesNoFurther()
    {
        var abi = Start();
        var presses = 0;
        var menu = new ContextMenu().Items(new MenuItem("One"));
        var page = new FlexBox().Width(400.0f).Height(300.0f);
        page.PointerDown += _ => presses++;
        Application.Mount(FlexBox.Column(page, menu));
        abi.Bounds[page.Handle] = new Rect(0.0f, 0.0f, 400.0f, 300.0f);

        menu.Show(null, 10.0f, 10.0f);
        HostInput.Key("Escape");
        var openAfterEscape = menu.IsOpen;
        menu.Show(null, 10.0f, 10.0f);
        var panel = Entries(menu)[0].Parent!;
        abi.Bounds[panel.Handle] = new Rect(10.0f, 10.0f, 220.0f, 40.0f);
        var consumedInside = HostInput.Pointer(PointerEventType.Down, page.Handle, 20.0f, 20.0f);
        var openAfterInside = menu.IsOpen;
        var consumedOutside = HostInput.Pointer(PointerEventType.Down, page.Handle, 300.0f, 200.0f);

        await Assert.That(openAfterEscape).IsFalse();
        await Assert.That(openAfterInside).IsTrue();
        await Assert.That(consumedOutside).IsTrue();
        await Assert.That(menu.IsOpen).IsFalse();
        await Assert.That(presses).IsEqualTo(1);
        await Assert.That(consumedInside == consumedOutside).IsFalse();
    }

    [Test]
    public async Task TheMenuStaysInsideTheViewportAndFollowsStyleOverrides()
    {
        var abi = Start();
        var red = new Color(0xFF0000FF);
        var menu = new ContextMenu().Items(new MenuItem("One"), new MenuItem("Two")).PanelColor(red).ItemHeight(30.0f);
        Application.Mount(FlexBox.Column(menu));

        menu.Show(null, 790.0f, 590.0f);
        var panel = (FlexBox)Entries(menu)[0].Parent!;

        await Assert.That(abi.HasCall("ui_set_position", (object)panel.Handle, 572.0f, 524.0f)).IsTrue();
        await Assert.That(panel.Background).IsEqualTo(red);
        await Assert.That(Entries(menu)[0].Height).IsEqualTo((Length)30.0f);
    }
}
