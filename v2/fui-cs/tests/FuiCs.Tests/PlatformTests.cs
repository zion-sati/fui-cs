using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class PlatformTests
{
    [Test]
    public async Task TheHostIsQueriedOnceAndUnknownValuesAreNormalized()
    {
        var abi = TestRuntime.Start(new LayoutAbi
        {
            PlatformFamily = (PlatformFamily)9,
            HostEnvironment = HostEnvironment.Desktop,
            HostCapabilities = HostCapability.Reload | HostCapability.FileDialogs | (HostCapability)0x400,
        });

        var first = Platform.Current;
        var second = Platform.Current;

        await Assert.That(ReferenceEquals(first, second)).IsTrue();
        await Assert.That(abi.CountOf("fui_get_platform_family")).IsEqualTo(1);
        await Assert.That(Platform.Family).IsEqualTo(PlatformFamily.Unknown);
        await Assert.That(Platform.Environment).IsEqualTo(HostEnvironment.Desktop);
        await Assert.That(Platform.Current.Capabilities).IsEqualTo(HostCapability.Reload | HostCapability.FileDialogs);
        await Assert.That(Platform.Supports(HostCapability.FileDialogs)).IsTrue();
        await Assert.That(Platform.Supports(HostCapability.ClipboardRead)).IsFalse();
        await Assert.That(Platform.Supports(HostCapability.None)).IsFalse();
    }

    [Test]
    public async Task ShortcutModifiersFollowThePlatform()
    {
        TestRuntime.Start(new LayoutAbi { PlatformFamily = PlatformFamily.Apple });

        await Assert.That(KeyboardShortcuts.PrimaryModifier()).IsEqualTo(KeyModifier.Meta);
        await Assert.That(KeyboardShortcuts.PrimaryModifier(PlatformFamily.Windows)).IsEqualTo(KeyModifier.Ctrl);
        await Assert.That(KeyboardShortcuts.WordNavigationModifier()).IsEqualTo(KeyModifier.Alt);
        await Assert.That(KeyboardShortcuts.HasLineBoundaryModifier(KeyModifier.Meta)).IsTrue();
        await Assert.That(KeyboardShortcuts.HasLineBoundaryModifier(KeyModifier.Ctrl, PlatformFamily.Linux)).IsFalse();
        await Assert.That(KeyboardShortcuts.HasDocumentBoundaryModifier(KeyModifier.Ctrl, PlatformFamily.Windows)).IsTrue();
    }

    [Test]
    public async Task UndoAndRedoShortcutsMatchEachPlatform()
    {
        TestRuntime.Start(new LayoutAbi());

        await Assert.That(KeyboardShortcuts.IsUndo("z", KeyModifier.Meta, PlatformFamily.Apple)).IsTrue();
        await Assert.That(KeyboardShortcuts.IsUndo("Z", KeyModifier.Ctrl)).IsTrue();
        await Assert.That(KeyboardShortcuts.IsUndo("z", KeyModifier.Ctrl | KeyModifier.Shift)).IsFalse();
        await Assert.That(KeyboardShortcuts.IsRedo("y", KeyModifier.Ctrl)).IsTrue();
        await Assert.That(KeyboardShortcuts.IsRedo("z", KeyModifier.Ctrl | KeyModifier.Shift)).IsTrue();
        await Assert.That(KeyboardShortcuts.IsRedo("y", KeyModifier.Meta, PlatformFamily.Apple)).IsFalse();
        await Assert.That(KeyboardShortcuts.IsRedo("z", KeyModifier.Meta | KeyModifier.Shift, PlatformFamily.Apple)).IsTrue();
    }

    [Test]
    public async Task ShortcutLabelsUseThePlatformsConventions()
    {
        TestRuntime.Start(new LayoutAbi());

        await Assert.That(KeyboardShortcuts.FormatLabel("z", KeyModifier.Meta | KeyModifier.Shift, PlatformFamily.Apple)).IsEqualTo("⇧⌘Z");
        await Assert.That(KeyboardShortcuts.FormatLabel("ArrowLeft", KeyModifier.Ctrl | KeyModifier.Alt, PlatformFamily.Windows)).IsEqualTo("Ctrl+Alt+Left");
        await Assert.That(KeyboardShortcuts.FormatLabel("PageDown", KeyModifier.None, PlatformFamily.Apple)).IsEqualTo("PgDn");
        await Assert.That(KeyboardShortcuts.UndoLabel()).IsEqualTo("Ctrl+Z");
        await Assert.That(KeyboardShortcuts.RedoLabel()).IsEqualTo("Ctrl+Y");
        await Assert.That(KeyboardShortcuts.RedoLabel(PlatformFamily.Apple)).IsEqualTo("⇧⌘Z");
    }

    [Test]
    public async Task NavigationAsksTheHostAndFollowsItsRoute()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var routes = new List<string>();
        Navigation.CurrentRoute.Changed += routes.Add;

        Navigation.NavigateTo("#/settings");
        Navigation.NavigateTo("https://example.com", openInNewTab: true);
        HostEvents.OnRouteChanged("#/settings");

        await Assert.That(abi.HasCall("fui_navigate_to", "#/settings", false)).IsTrue();
        await Assert.That(abi.HasCall("fui_navigate_to", "https://example.com", true)).IsTrue();
        await Assert.That(Navigation.CurrentRoute.Value).IsEqualTo("#/settings");
        await Assert.That(string.Join(",", routes)).IsEqualTo("#/settings");
    }
}
